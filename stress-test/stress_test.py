#!/usr/bin/env python3
"""
Kimmy Esthi API Stress Test
A comprehensive stress testing tool for the appointment booking system
"""

import asyncio
import json
import time
import argparse
import signal
import sys
import uuid
from datetime import datetime, timedelta
from typing import Dict, List, Any, Optional
import random
from collections import defaultdict, deque

try:
    import aiohttp
    import numpy as np
except ImportError:
    print("Required packages not found. Please run: pip install -r requirements.txt")
    sys.exit(1)

from test_data import TestDataGenerator


class MetricsCollector:
    """Collects and analyzes test metrics"""
    
    def __init__(self):
        self.reset()
    
    def reset(self):
        self.total_requests = 0
        self.successful_requests = 0
        self.failed_requests = 0
        self.response_times: deque[float] = deque(maxlen=10000)
        self.status_codes: Dict[int, int] = defaultdict(int)
        self.errors: Dict[str, int] = defaultdict(int)
        self.start_time: Optional[float] = None
        self.end_time: Optional[float] = None
        self.endpoint_stats: Dict[str, Dict[str, Any]] = defaultdict(lambda: {
            'requests': 0, 
            'successes': 0, 
            'response_times': deque(maxlen=1000)
        })
    
    def start_test(self):
        self.start_time = time.time()
    
    def end_test(self):
        self.end_time = time.time()
    
    def record_request(self, endpoint: str, status_code: int, response_time: float, error: str = ""):
        self.total_requests += 1
        self.response_times.append(response_time)
        self.status_codes[status_code] += 1
        
        # Track endpoint-specific stats
        stats = self.endpoint_stats[endpoint]
        stats['requests'] += 1
        stats['response_times'].append(response_time)
        
        if 200 <= status_code < 300:
            self.successful_requests += 1
            stats['successes'] += 1
        else:
            self.failed_requests += 1
            if error:
                self.errors[error] += 1
    
    def get_summary(self) -> Dict[str, Any]:
        current_time = self.end_time if self.end_time is not None else time.time()
        start_time = self.start_time if self.start_time is not None else 0
        duration = current_time - start_time
        
        if not self.response_times:
            return {
                'duration_seconds': duration,
                'total_requests': 0,
                'successful_requests': 0,
                'failed_requests': 0,
                'requests_per_second': 0,
                'success_rate': 0,
                'avg_response_time': 0,
                'p95_response_time': 0,
                'p99_response_time': 0
            }
        
        response_times_list = list(self.response_times)
        
        return {
            'duration_seconds': duration,
            'total_requests': self.total_requests,
            'successful_requests': self.successful_requests,
            'failed_requests': self.failed_requests,
            'requests_per_second': self.total_requests / duration if duration > 0 else 0,
            'success_rate': (self.successful_requests / self.total_requests * 100) if self.total_requests > 0 else 0,
            'avg_response_time': np.mean(response_times_list),
            'p95_response_time': np.percentile(response_times_list, 95),
            'p99_response_time': np.percentile(response_times_list, 99),
            'status_codes': dict(self.status_codes),
            'errors': dict(self.errors),
            'endpoint_breakdown': {
                endpoint: {
                    'requests': stats['requests'],
                    'successes': stats['successes'],
                    'success_rate': (stats['successes'] / stats['requests'] * 100) if stats['requests'] > 0 else 0,
                    'avg_response_time': np.mean(list(stats['response_times'])) if stats['response_times'] else 0
                }
                for endpoint, stats in self.endpoint_stats.items() if isinstance(stats, dict)
            }
        }


class StressTestRunner:
    """Main stress test execution engine"""
    
    def __init__(self, config: Dict[str, Any], base_url: str):
        self.config = config
        self.base_url = base_url.rstrip('/')
        self.data_generator = TestDataGenerator(config)
        self.metrics = MetricsCollector()
        self.session: Optional[aiohttp.ClientSession] = None
        self.admin_token: Optional[str] = None
        self.available_appointment_ids: List[str] = []
        self.created_client_ids: List[str] = []
        self.running = True
        
        # Setup signal handlers for graceful shutdown
        signal.signal(signal.SIGINT, self._signal_handler)
        signal.signal(signal.SIGTERM, self._signal_handler)
    
    def _signal_handler(self, signum, frame):
        """Handle shutdown signals"""
        print(f"\nReceived signal {signum}. Shutting down gracefully...")
        self.running = False
    
    async def setup(self):
        """Initialize the test session and authenticate if needed"""
        timeout = aiohttp.ClientTimeout(total=self.config['api']['timeout'])
        self.session = aiohttp.ClientSession(timeout=timeout)
        
        # Setup test data for write tests
        write_test_enabled = self.config.get('test_scenarios', {}).get('write_test', {}).get('enabled', False)
        mixed_test_enabled = self.config.get('test_scenarios', {}).get('mixed_test', {}).get('enabled', False)
        
        if write_test_enabled or mixed_test_enabled:
            await self.setup_test_data()
    
    async def cleanup(self):
        """Clean up resources"""
        if self.session:
            await self.session.close()
    
    async def setup_test_data(self):
        """Setup test data: fetch existing appointments for testing"""
        print("Checking for existing test data...")
        
        # Skip admin setup for now and just fetch existing appointments
        await self._fetch_available_appointments()
        
        if len(self.available_appointment_ids) > 0:
            print(f"✓ Found {len(self.available_appointment_ids)} existing appointments for testing")
            return True
        else:
            print("⚠ No existing appointments found. Skipping write stress test.")
            return False
    
    async def _check_existing_appointments(self):
        """Check what appointments already exist"""
        try:
            if self.session:
                async with self.session.get(f"{self.base_url}/appointments/status") as response:
                    if response.status == 200:
                        status_data = await response.json()
                        print(f"✓ Found {len(status_data)} appointment dates with availability data")
                    else:
                        print(f"✗ Failed to check appointment status: {response.status}")
        except Exception as e:
            print(f"✗ Error checking existing appointments: {e}")
    
    async def _create_appointment_slots(self):
        """Create appointment slots for testing"""
        if not self.admin_token:
            print("✗ Cannot create appointments without admin token")
            return
        
        # Generate 50 appointment slots with unique timestamps
        appointments_to_create = []
        base_date = datetime.now()
        appointment_counter = 0
        
        for day_offset in range(7):  # Create appointments for next 7 days
            target_date = base_date + timedelta(days=day_offset)
            
            # Create appointments at different times
            for hour in range(9, 17):  # 9 AM to 5 PM
                for minute in [0, 30]:  # Every 30 minutes
                    appointment_time = target_date.replace(hour=hour, minute=minute, second=0, microsecond=0)
                    
                    # Add unique seconds to avoid exact collision
                    unique_time = appointment_time + timedelta(seconds=appointment_counter % 60)
                    
                    appointments_to_create.append({
                        "id": str(uuid.uuid4()),
                        "dateTime": unique_time.isoformat(),
                        "status": 0  # 0 = Available, 1 = Booked
                    })
                    
                    appointment_counter += 1
                    
                    # Limit to 50 appointments to avoid overwhelming
                    if len(appointments_to_create) >= 50:
                        break
            if len(appointments_to_create) >= 50:
                break
        
        print(f"Creating {len(appointments_to_create)} appointment slots...")
        
        try:
            if self.session:
                async with self.session.post(
                    f"{self.base_url}/admin/{self.admin_token}/appointments",
                    json=appointments_to_create
                ) as response:
                    if response.status == 200:
                        print(f"✓ Successfully created appointment slots")
                    else:
                        error_text = await response.text()
                        print(f"✗ Failed to create appointments: {response.status}")
                        print(f"Response: {error_text[:200]}")
        except Exception as e:
            print(f"✗ Error creating appointment slots: {e}")
    
    async def _fetch_available_appointments(self):
        """Fetch available appointment IDs for stress testing"""
        try:
            if self.session:
                # Try a few different dates to get appointment IDs
                base_date = datetime.now()
                
                for day_offset in range(7):  # Check next 7 days
                    target_date = base_date + timedelta(days=day_offset)
                    date_str = target_date.strftime('%m-%d-%Y')
                    
                    async with self.session.get(f"{self.base_url}/appointments/{date_str}") as response:
                        if response.status == 200:
                            appointments = await response.json()
                            for apt in appointments:
                                if apt.get('status', False):  # Only available appointments
                                    self.available_appointment_ids.append(apt['id'])
                        
                        # Stop if we have enough appointments
                        if len(self.available_appointment_ids) >= 100:
                            break
                
                print(f"✓ Fetched {len(self.available_appointment_ids)} available appointment IDs")
                
                # If we still don't have enough, create more
                if len(self.available_appointment_ids) < 20:
                    print("⚠ Not enough available appointments, creating more...")
                    await self._create_appointment_slots()
                    await self._fetch_available_appointments()
                    
        except Exception as e:
            print(f"✗ Error fetching available appointments: {e}")
    
    async def _get_admin_token(self):
        """Authenticate as admin for admin endpoint testing"""
        admin_config = self.config.get('test_scenarios', {}).get('admin_test', {})
        credentials = admin_config.get('credentials', {})
        username = credentials.get('username', 'kimmxthy')
        password = credentials.get('password', 'KakeKakeKake4eva')
        
        login_data = self.data_generator.generate_login_request(username, password)
        
        try:
            if self.session:
                async with self.session.post(
                    f"{self.base_url}/admin/",
                    json=login_data
                ) as response:
                    if response.status == 200:
                        result = await response.json()
                        self.admin_token = result
                        print("✓ Admin authentication successful")
                        return True
                    else:
                        print(f"✗ Admin authentication failed: {response.status}")
                        return False
        except Exception as e:
            print(f"✗ Admin authentication error: {e}")
            return False
        return False
    
    async def run_scenario(self, scenario_name: str, scenario_config: Dict[str, Any]):
        """Run a specific test scenario"""
        print(f"\n{'='*50}")
        print(f"Running {scenario_name.upper()} TEST")
        print(f"{'='*50}")
        print(f"Concurrent Users: {scenario_config['concurrent_users']}")
        print(f"Duration: {scenario_config['duration_seconds']}s")
        print(f"Ramp-up: {scenario_config['ramp_up_seconds']}s")
        
        self.metrics.reset()
        self.metrics.start_test()
        
        # Create tasks for concurrent users
        tasks = []
        ramp_up_delay = scenario_config['ramp_up_seconds'] / scenario_config['concurrent_users']
        
        for i in range(scenario_config['concurrent_users']):
            task = asyncio.create_task(
                self._run_user_session(scenario_config, delay=i * ramp_up_delay)
            )
            tasks.append(task)
        
        # Wait for all tasks to complete or timeout
        try:
            await asyncio.wait_for(
                asyncio.gather(*tasks, return_exceptions=True),
                timeout=scenario_config['duration_seconds'] + 60
            )
        except asyncio.TimeoutError:
            print("Test duration reached, stopping...")
            for task in tasks:
                task.cancel()
        
        self.metrics.end_test()
        
        # Print results
        await self._print_scenario_results(scenario_name)
    
    async def _run_user_session(self, scenario_config: Dict[str, Any], delay: float = 0):
        """Simulate a single user session"""
        await asyncio.sleep(delay)  # Ramp-up delay
        
        end_time = time.time() + scenario_config['duration_seconds']
        
        while self.running and time.time() < end_time:
            # Select endpoint based on weights
            endpoint = self._select_weighted_endpoint(scenario_config['endpoints'])
            
            if endpoint:
                await self._make_request(endpoint)
            
            # Random think time between requests (1-3 seconds)
            await asyncio.sleep(random.uniform(1, 3))
    
    def _select_weighted_endpoint(self, endpoints: List[Dict[str, Any]]) -> Optional[Dict[str, Any]]:
        """Select an endpoint based on weight distribution"""
        total_weight = sum(ep['weight'] for ep in endpoints)
        if total_weight == 0:
            return None
        
        rand = random.uniform(0, total_weight)
        current_weight = 0
        
        for endpoint in endpoints:
            current_weight += endpoint['weight']
            if rand <= current_weight:
                return endpoint
        
        return endpoints[0]
    
    async def _make_request(self, endpoint: Dict[str, Any]):
        """Make a single API request"""
        method = endpoint['method'].upper()
        path = endpoint['path']
        
        # Replace placeholders in path
        if '{token}' in path and self.admin_token:
            path = path.replace('{token}', self.admin_token)
        elif '{token}' in path and not self.admin_token:
            return  # Skip admin endpoints if no token
        
        # Generate request body for POST requests
        json_data = await self._generate_request_body(path)
        
        # Debug output - print the actual JSON being sent
        # Debug output disabled - remove when not needed
        
        start_time = time.time()
        error_message = ""
        
        try:
            if self.session:
                async with self.session.request(
                    method,
                    f"{self.base_url}{path}",
                    json=json_data,
                    headers={'Content-Type': 'application/json'}
                ) as response:
                    response_time = time.time() - start_time
                    
                    # Read response to capture client IDs and ensure request completes
                    response_text = ""
                    try:
                        response_text = await response.text()
                        
                        # Capture client ID from successful appointment bookings
                        if (response.status == 200 and path == '/appointment' and response_text):
                            try:
                                response_data = json.loads(response_text)
                                if 'scheduledAppointment' in response_data and 'clientId' in response_data['scheduledAppointment']:
                                    client_id = response_data['scheduledAppointment']['clientId']
                                    self.created_client_ids.append(client_id)
                                    # Keep only recent client IDs to avoid memory issues
                                    if len(self.created_client_ids) > 1000:
                                        self.created_client_ids = self.created_client_ids[-500:]
                            except json.JSONDecodeError:
                                pass  # Ignore JSON parsing errors
                                
                    except Exception as e:
                        pass  # Ignore response read errors
                    
                    self.metrics.record_request(
                        f"{method} {path}",
                        response.status,
                        response_time,
                        error_message
                    )
                    
        except asyncio.TimeoutError:
            error_message = "timeout"
            response_time = time.time() - start_time
            self.metrics.record_request(f"{method} {path}", 0, response_time, error_message)
        except Exception as e:
            error_message = str(e)[:100]  # Truncate long errors
            response_time = time.time() - start_time
            self.metrics.record_request(f"{method} {path}", 0, response_time, error_message)
    
    async def _generate_request_body(self, path: str) -> Optional[Dict[str, Any]]:
        """Generate appropriate request body for the given endpoint"""
        if path == '/appointment':
            # Use a real appointment ID from our pool
            appointment_id = None
            if self.available_appointment_ids:
                appointment_id = random.choice(self.available_appointment_ids)
            return self.data_generator.generate_appointment_request(appointment_id)
        elif path == '/consentForm':
            # Use a real client ID from previous successful bookings
            client_id = ""
            if self.created_client_ids:
                client_id = random.choice(self.created_client_ids)
            return self.data_generator.generate_consent_form(client_id)
        elif path.startswith('/admin/'):
            # Admin endpoints typically don't need request body for GET requests
            return None
        return None
    
    async def _print_scenario_results(self, scenario_name: str):
        """Print formatted results for a completed scenario"""
        summary = self.metrics.get_summary()
        
        print(f"\n{scenario_name.upper()} TEST RESULTS")
        print(f"{'='*50}")
        print(f"Duration: {summary['duration_seconds']:.2f}s")
        print(f"Total Requests: {summary['total_requests']}")
        print(f"Successful: {summary['successful_requests']}")
        print(f"Failed: {summary['failed_requests']}")
        print(f"Requests/sec: {summary['requests_per_second']:.2f}")
        print(f"Success Rate: {summary['success_rate']:.2f}%")
        print(f"Avg Response Time: {summary['avg_response_time']:.3f}s")
        print(f"95th Percentile: {summary['p95_response_time']:.3f}s")
        print(f"99th Percentile: {summary['p99_response_time']:.3f}s")
        
        if summary['status_codes']:
            print(f"\nStatus Codes:")
            for code, count in sorted(summary['status_codes'].items()):
                print(f"  {code}: {count}")
        
        if summary['errors']:
            print(f"\nTop Errors:")
            for error, count in sorted(summary['errors'].items(), key=lambda x: x[1], reverse=True)[:5]:
                print(f"  {count}x: {error}")
        
        if summary['endpoint_breakdown']:
            print(f"\nEndpoint Breakdown:")
            for endpoint, stats in summary['endpoint_breakdown'].items():
                print(f"  {endpoint}:")
                print(f"    Requests: {stats['requests']}")
                print(f"    Success Rate: {stats['success_rate']:.1f}%")
                print(f"    Avg Response Time: {stats['avg_response_time']:.3f}s")
    
    async def run_all_tests(self):
        """Run all enabled test scenarios"""
        await self.setup()
        
        try:
            for scenario_name, scenario_config in self.config['test_scenarios'].items():
                if scenario_config.get('enabled', False):
                    await self.run_scenario(scenario_name, scenario_config)
                else:
                    print(f"\nSkipping {scenario_name} (disabled)")
        
        finally:
            await self.cleanup()


async def main():
    """Main entry point"""
    parser = argparse.ArgumentParser(description='Kimmy Esthi API Stress Test')
    parser.add_argument('--config', default='config.json', help='Configuration file path')
    parser.add_argument('--url', help='API base URL (overrides config)')
    parser.add_argument('--scenarios', nargs='+', help='Specific scenarios to run')
    parser.add_argument('--output', help='Results output file')
    
    args = parser.parse_args()
    
    # Load configuration
    try:
        with open(args.config, 'r') as f:
            config = json.load(f)
    except FileNotFoundError:
        print(f"Configuration file {args.config} not found!")
        sys.exit(1)
    except json.JSONDecodeError as e:
        print(f"Invalid JSON in configuration file: {e}")
        sys.exit(1)
    
    # Override URL if provided
    base_url = args.url or config['api']['base_url']
    
    print("Kimmy Esthi API Stress Test")
    print(f"Target URL: {base_url}")
    print(f"Configuration: {args.config}")
    
    # Filter scenarios if specified
    if args.scenarios:
        filtered_scenarios = {}
        for scenario in args.scenarios:
            if scenario in config['test_scenarios']:
                filtered_scenarios[scenario] = config['test_scenarios'][scenario]
            else:
                print(f"Warning: Scenario '{scenario}' not found in configuration")
        config['test_scenarios'] = filtered_scenarios
    
    # Run the stress tests
    runner = StressTestRunner(config, base_url)
    await runner.run_all_tests()
    
    print("\nStress testing completed!")


if __name__ == '__main__':
    asyncio.run(main())