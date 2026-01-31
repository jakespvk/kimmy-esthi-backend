#!/usr/bin/env python3
"""
Consent Form Stress Test
Tests write performance and robustness of the consent form submission endpoint.
Designed for SQLite with EF Core - includes retry logic for database locking.
"""

import asyncio
import json
import time
import argparse
import signal
import sys
import uuid
from datetime import datetime
from typing import Dict, List, Any, Optional, Deque
from collections import defaultdict, deque
from dataclasses import dataclass, field
import random

try:
    import aiohttp
    import numpy as np
except ImportError:
    print("Required packages not found. Please run: pip install -r requirements.txt")
    sys.exit(1)

from consent_test_data import ConsentFormTestDataGenerator


@dataclass
class RequestMetrics:
    """Metrics for a single request"""
    endpoint: str
    method: str
    status_code: int
    response_time: float
    error_type: str = ""
    error_message: str = ""
    user_id: int = 0
    timestamp: float = field(default_factory=time.time)


@dataclass
class UserLevelMetrics:
    """Metrics for a specific user count level"""
    user_count: int
    total_requests: int = 0
    successful_requests: int = 0
    failed_requests: int = 0
    sqlite_busy_errors: int = 0
    response_times: List[float] = field(default_factory=list)
    start_time: float = 0
    end_time: float = 0


class SQLiteRetryHandler:
    """Handles SQLite locking with exponential backoff retry logic"""
    
    def __init__(self, config: Dict[str, Any]):
        self.config = config.get('sqlite', {}).get('retry', {})
        self.enabled = self.config.get('enabled', True)
        self.max_retries = self.config.get('max_retries', 5)
        self.initial_delay_ms = self.config.get('initial_delay_ms', 100)
        self.max_delay_ms = self.config.get('max_delay_ms', 3200)
        self.stats = {
            'total_retries': 0,
            'successful_retries': 0,
            'failed_after_retries': 0
        }
    
    async def execute_with_retry(self, coro):
        """Execute a coroutine with retry logic for SQLiteBusy errors"""
        if not self.enabled:
            return await coro
        
        last_error = None
        for attempt in range(self.max_retries + 1):
            try:
                return await coro
            except Exception as e:
                error_str = str(e).lower()
                
                if 'locked' in error_str or 'busy' in error_str:
                    last_error = e
                    self.stats['total_retries'] += 1
                    
                    if attempt < self.max_retries:
                        delay_ms = min(
                            self.initial_delay_ms * (2 ** attempt),
                            self.max_delay_ms
                        )
                        await asyncio.sleep(delay_ms / 1000)
                        continue
                    else:
                        self.stats['failed_after_retries'] += 1
                        raise
                else:
                    raise
            
        self.stats['successful_retries'] += 1
        return None


class ConsentFormMetrics:
    """Collects and analyzes test metrics"""
    
    def __init__(self):
        self.reset()
    
    def reset(self):
        self.total_requests = 0
        self.successful_requests = 0
        self.failed_requests = 0
        self.sqlite_busy_count = 0
        self.response_times: List[float] = []
        self.status_codes: Dict[int, int] = defaultdict(int)
        self.errors: Dict[str, int] = defaultdict(int)
        self.start_time: Optional[float] = None
        self.end_time: Optional[float] = None
        self.user_level_metrics: Dict[int, UserLevelMetrics] = {}
        self.current_user_level: int = 0
        self.requests: List[RequestMetrics] = []
        self.user_submission_counts: Dict[int, int] = defaultdict(int)
    
    def start_test(self):
        self.start_time = time.time()
    
    def end_test(self):
        self.end_time = time.time()
    
    def set_user_level(self, user_count: int):
        if user_count not in self.user_level_metrics:
            self.user_level_metrics[user_count] = UserLevelMetrics(user_count=user_count)
        self.current_user_level = user_count
        self.user_level_metrics[user_count].start_time = time.time()
    
    def record_request(self, metrics: RequestMetrics):
        self.total_requests += 1
        self.response_times.append(metrics.response_time)
        self.status_codes[metrics.status_code] += 1
        self.requests.append(metrics)
        
        level = self.user_level_metrics.get(self.current_user_level)
        if level:
            level.total_requests += 1
            level.response_times.append(metrics.response_time)
            level.end_time = time.time()
        
        if 200 <= metrics.status_code < 300:
            self.successful_requests += 1
            self.user_submission_counts[metrics.user_id] += 1
        else:
            self.failed_requests += 1
            if 'locked' in metrics.error_type.lower() or 'busy' in metrics.error_type.lower():
                self.sqlite_busy_count += 1
                if level:
                    level.sqlite_busy_errors += 1
            if metrics.error_type:
                self.errors[metrics.error_type] += 1
            if metrics.error_message:
                error_key = f"{metrics.error_type}: {metrics.error_message[:50]}"
                self.errors[error_key] += 1
    
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
        
        return {
            'duration_seconds': duration,
            'total_requests': self.total_requests,
            'successful_requests': self.successful_requests,
            'failed_requests': self.failed_requests,
            'sqlite_busy_errors': self.sqlite_busy_count,
            'requests_per_second': self.total_requests / duration if duration > 0 else 0,
            'success_rate': (self.successful_requests / self.total_requests * 100) if self.total_requests > 0 else 0,
            'avg_response_time': np.mean(self.response_times),
            'p95_response_time': np.percentile(self.response_times, 95),
            'p99_response_time': np.percentile(self.response_times, 99),
            'status_codes': dict(self.status_codes),
            'errors': dict(self.errors),
            'user_level_breakdown': {
                level.user_count: {
                    'duration_seconds': level.end_time - level.start_time if level.end_time > 0 else 0,
                    'total_requests': level.total_requests,
                    'successful_requests': level.successful_requests,
                    'failed_requests': level.failed_requests,
                    'sqlite_busy_errors': level.sqlite_busy_errors,
                    'requests_per_second': level.total_requests / max(1, level.end_time - level.start_time),
                    'avg_response_time': np.mean(level.response_times) if level.response_times else 0,
                    'p95_response_time': np.percentile(level.response_times, 95) if level.response_times else 0
                }
                for level in self.user_level_metrics.values()
            }
        }


class ConsentFormStressTestRunner:
    """Main stress test execution engine for consent form submissions"""
    
    def __init__(self, config: Dict[str, Any], base_url: str):
        self.config = config
        self.base_url = base_url.rstrip('/')
        self.data_generator = ConsentFormTestDataGenerator(config)
        self.metrics = ConsentFormMetrics()
        self.retry_handler = SQLiteRetryHandler(config)
        self.session: Optional[aiohttp.ClientSession] = None
        self.running = True
        self.user_count = 0
        self.active_users: List[int] = []
        
        signal.signal(signal.SIGINT, self._signal_handler)
        signal.signal(signal.SIGTERM, self._signal_handler)
    
    def _signal_handler(self, signum, frame):
        print(f"\nReceived signal {signum}. Shutting down gracefully...")
        self.running = False
    
    async def setup(self):
        timeout = aiohttp.ClientTimeout(total=self.config['api'].get('timeout', 60))
        self.session = aiohttp.ClientSession(timeout=timeout)
    
    async def cleanup(self):
        if self.session:
            await self.session.close()
    
    async def run_capacity_ramp_test(self):
        """Run ramp-up test from 1 to configured max users"""
        scenario = self.config['test_scenarios']['consent_capacity_ramp']
        
        start_users = scenario.get('start_users', 1)
        end_users = scenario.get('end_users', 20)
        ramp_interval = scenario.get('ramp_interval_seconds', 5)
        duration = scenario.get('duration_seconds', 300)
        
        print(f"\n{'='*60}")
        print(f"CONSENT FORM CAPACITY RAMP TEST")
        print(f"{'='*60}")
        print(f"Start Users: {start_users}")
        print(f"End Users: {end_users}")
        print(f"Ramp Interval: {ramp_interval}s")
        print(f"Max Duration: {duration}s")
        print(f"Client Flow: NEW CLIENTS (100%)")
        print(f"{'='*60}\n")
        
        self.metrics.reset()
        self.metrics.start_test()
        
        start_time = time.time()
        next_ramp_time = start_time + ramp_interval
        target_users = start_users
        self.user_count = start_users
        
        self.active_users = list(range(1, start_users + 1))
        self.metrics.set_user_level(start_users)
        
        print(f"Starting with {start_users} user(s)")
        
        user_tasks = []
        for user_id in self.active_users:
            task = asyncio.create_task(self._run_user_session(
                user_id=user_id,
                duration=duration,
                endpoint=scenario.get('endpoint', '/consentForm'),
                method=scenario.get('method', 'POST')
            ))
            user_tasks.append(task)
        
        last_progress_time = start_time
        
        while self.running:
            current_time = time.time()
            elapsed = current_time - start_time
            
            if elapsed > duration:
                print(f"\nTest duration reached")
                break
            
            if current_time >= next_ramp_time and target_users < end_users:
                target_users += 1
                self.user_count = target_users
                self.active_users.append(target_users)
                self.metrics.set_user_level(target_users)
                
                print(f"\n[{datetime.now().strftime('%H:%M:%S')}] Ramping up to {target_users} user(s)")
                
                new_task = asyncio.create_task(self._run_user_session(
                    user_id=target_users,
                    duration=duration - (current_time - start_time),
                    endpoint=scenario.get('endpoint', '/consentForm'),
                    method=scenario.get('method', 'POST')
                ))
                user_tasks.append(new_task)
                user_tasks.append(new_task)
                
                next_ramp_time = current_time + ramp_interval
            
            if current_time - last_progress_time >= 30:
                self._print_progress(elapsed)
                last_progress_time = current_time
            
            await asyncio.sleep(0.5)
        
        for task in user_tasks:
            task.cancel()
        
        await asyncio.gather(*user_tasks, return_exceptions=True)
        
        self.metrics.end_test()
        await self._print_capacity_results()
    
    async def run_write_submission_test(self):
        """Run fixed concurrent user write test"""
        scenario = self.config['test_scenarios']['consent_write_submission']
        
        print(f"\n{'='*60}")
        print(f"CONSENT FORM WRITE SUBMISSION TEST")
        print(f"{'='*60}")
        print(f"Concurrent Users: {scenario['concurrent_users']}")
        print(f"Duration: {scenario['duration_seconds']}s")
        print(f"Ramp-up: {scenario['ramp_up_seconds']}s")
        print(f"Client Flow: {scenario.get('client_flow', 'new_clients')}")
        print(f"{'='*60}\n")
        
        self.metrics.reset()
        self.metrics.start_test()
        self.metrics.set_user_level(scenario['concurrent_users'])
        
        ramp_up_delay = scenario['ramp_up_seconds'] / scenario['concurrent_users']
        
        user_tasks = []
        for i in range(scenario['concurrent_users']):
            task = asyncio.create_task(
                self._run_user_session(
                    user_id=i + 1,
                    duration=scenario['duration_seconds'],
                    delay=i * ramp_up_delay,
                    endpoint=scenario.get('endpoint', '/consentForm'),
                    method=scenario.get('method', 'POST')
                )
            )
            user_tasks.append(task)
        
        try:
            await asyncio.wait_for(
                asyncio.gather(*user_tasks, return_exceptions=True),
                timeout=scenario['duration_seconds'] + 60
            )
        except asyncio.TimeoutError:
            print("Test duration reached, stopping...")
        
        self.metrics.end_test()
        await self._print_scenario_results("Consent Write Submission")
    
    async def run_robustness_test(self):
        """Run robustness/edge case tests"""
        scenario = self.config['test_scenarios']['consent_robustness']
        edge_cases = scenario.get('edge_cases', [])
        
        print(f"\n{'='*60}")
        print(f"CONSENT FORM ROBUSTNESS TEST")
        print(f"{'='*60}")
        print(f"Concurrent Users: {scenario['concurrent_users']}")
        print(f"Duration: {scenario['duration_seconds']}s")
        print(f"Edge Cases: {', '.join(edge_cases)}")
        print(f"{'='*60}\n")
        
        self.metrics.reset()
        self.metrics.start_test()
        
        ramp_up_delay = scenario['ramp_up_seconds'] / scenario['concurrent_users']
        
        async def robustness_user_session(user_id: int):
            await asyncio.sleep(ramp_up_delay * (user_id - 1))
            
            end_time = time.time() + scenario['duration_seconds']
            request_num = 0
            
            while self.running and time.time() < end_time:
                edge_case = random.choice(edge_cases)
                payload = self.data_generator.generate_robustness_payload(edge_case)
                
                request_num += 1
                await self._make_consent_request(
                    endpoint=scenario.get('endpoint', '/consentForm'),
                    method=scenario.get('method', 'POST'),
                    payload=payload,
                    user_id=user_id,
                    edge_case=edge_case
                )
                
                await asyncio.sleep(random.uniform(1, 3))
        
        user_tasks = []
        for i in range(scenario['concurrent_users']):
            task = asyncio.create_task(robustness_user_session(i + 1))
            user_tasks.append(task)
        
        try:
            await asyncio.wait_for(
                asyncio.gather(*user_tasks, return_exceptions=True),
                timeout=scenario['duration_seconds'] + 60
            )
        except asyncio.TimeoutError:
            print("Test duration reached, stopping...")
        
        self.metrics.end_test()
        await self._print_robustness_results()
    
    async def _run_user_session(self, user_id: int, duration: float, delay: float = 0,
                                 endpoint: str = '/consentForm', method: str = 'POST'):
        """Simulate a single user session submitting consent forms"""
        await asyncio.sleep(delay)
        
        end_time = time.time() + duration
        request_num = 0
        
        while self.running and time.time() < end_time:
            request_num += 1
            
            payload = self.data_generator.generate_new_client_consent_form(
                user_num=user_id * 1000 + request_num
            )
            
            await self._make_consent_request(
                endpoint=endpoint,
                method=method,
                payload=payload,
                user_id=user_id
            )
            
            await asyncio.sleep(random.uniform(0.5, 1.5))
    
    async def _make_consent_request(self, endpoint: str, method: str, payload: Dict[str, Any],
                                     user_id: int, edge_case: str = ""):
        """Make a single consent form submission request with retry logic"""
        start_time = time.time()
        error_type = ""
        error_message = ""
        status_code = 0
        
        async def make_request():
            nonlocal status_code
            async with self.session.request(
                method,
                f"{self.base_url}{endpoint}",
                json=payload,
                headers={'Content-Type': 'application/json'}
            ) as response:
                status_code = response.status
                await response.text()
                return response
        
        try:
            await self.retry_handler.execute_with_retry(make_request())
        except asyncio.TimeoutError:
            error_type = "Timeout"
            error_message = "Request timed out"
            status_code = 0
        except Exception as e:
            error_type = type(e).__name__
            error_message = str(e)[:100]
            status_code = 0
        
        response_time = time.time() - start_time
        
        self.metrics.record_request(RequestMetrics(
            endpoint=endpoint,
            method=method,
            status_code=status_code,
            response_time=response_time,
            error_type=error_type,
            error_message=error_message,
            user_id=user_id
        ))
    
    def _print_progress(self, elapsed: float):
        """Print real-time progress"""
        summary = self.metrics.get_summary()
        current_level = self.metrics.current_user_level
        
        print(f"[{datetime.now().strftime('%H:%M:%S')}] "
              f"Users: {current_level} | "
              f"Requests: {summary['total_requests']} | "
              f"Success: {summary['success_rate']:.1f}% | "
              f"P95: {summary['p95_response_time']:.3f}s | "
              f"RPS: {summary['requests_per_second']:.2f}")
    
    async def _print_capacity_results(self):
        """Print detailed capacity ramp test results"""
        summary = self.metrics.get_summary()
        
        print(f"\n{'='*60}")
        print(f"CAPACITY RAMP TEST RESULTS")
        print(f"{'='*60}")
        print(f"Total Duration: {summary['duration_seconds']:.2f}s")
        print(f"Total Requests: {summary['total_requests']}")
        print(f"Successful: {summary['successful_requests']}")
        print(f"Failed: {summary['failed_requests']}")
        print(f"SQLite Busy Errors: {summary['sqlite_busy_errors']}")
        print(f"Overall Success Rate: {summary['success_rate']:.2f}%")
        print(f"\nRetry Statistics:")
        print(f"  Total Retries: {self.retry_handler.stats['total_retries']}")
        print(f"  Successful Retries: {self.retry_handler.stats['successful_retries']}")
        print(f"  Failed After Retries: {self.retry_handler.stats['failed_after_retries']}")
        
        print(f"\n{'='*60}")
        print(f"PER-USER-LEVEL BREAKDOWN")
        print(f"{'='*60}")
        print(f"{'Users':<8} {'Requests':<12} {'Success':<10} {'Failed':<10} {'RPS':<10} {'P95':<10}")
        print(f"{'-'*60}")
        
        for level in sorted(self.user_level_metrics.keys()):
            metrics = summary['user_level_breakdown'].get(level, {})
            print(f"{level:<8} {metrics.get('total_requests', 0):<12} "
                  f"{metrics.get('successful_requests', 0):<10} "
                  f"{metrics.get('failed_requests', 0):<10} "
                  f"{metrics.get('requests_per_second', 0):<10.2f} "
                  f"{metrics.get('p95_response_time', 0):<10.3f}")
        
        max_users_reached = max(self.user_level_metrics.keys()) if self.user_level_metrics else 0
        print(f"\n{'='*60}")
        print(f"MAX CONCURRENT USERS REACHED: {max_users_reached}")
        print(f"{'='*60}")
        
        if summary['errors']:
            print(f"\nTop Errors:")
            for error, count in sorted(summary['errors'].items(), key=lambda x: x[1], reverse=True)[:5]:
                print(f"  {count}x: {error}")
        
        if summary['status_codes']:
            print(f"\nStatus Codes:")
            for code, count in sorted(summary['status_codes'].items()):
                print(f"  {code}: {count}")
    
    async def _print_scenario_results(self, scenario_name: str):
        """Print standard scenario results"""
        summary = self.metrics.get_summary()
        
        print(f"\n{scenario_name.upper()} TEST RESULTS")
        print(f"{'='*50}")
        print(f"Duration: {summary['duration_seconds']:.2f}s")
        print(f"Total Requests: {summary['total_requests']}")
        print(f"Successful: {summary['successful_requests']}")
        print(f"Failed: {summary['failed_requests']}")
        print(f"SQLite Busy Errors: {summary['sqlite_busy_errors']}")
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
    
    async def _print_robustness_results(self):
        """Print robustness test results"""
        summary = self.metrics.get_summary()
        
        print(f"\nROBUSTNESS TEST RESULTS")
        print(f"{'='*50}")
        print(f"Duration: {summary['duration_seconds']:.2f}s")
        print(f"Total Requests: {summary['total_requests']}")
        print(f"Successful: {summary['successful_requests']}")
        print(f"Failed: {summary['failed_requests']}")
        print(f"Success Rate: {summary['success_rate']:.2f}%")
        
        if summary['errors']:
            print(f"\nError Distribution:")
            for error, count in sorted(summary['errors'].items(), key=lambda x: x[1], reverse=True):
                print(f"  {count}x: {error}")
        
        print(f"\nNote: Robustness tests EXPECT failures for invalid payloads.")
        print(f"Expected behavior: 400/404 errors for malformed requests.")
    
    async def run_all_tests(self):
        """Run all enabled test scenarios"""
        await self.setup()
        
        try:
            for scenario_name, scenario_config in self.config['test_scenarios'].items():
                if scenario_config.get('enabled', False):
                    if scenario_name == 'consent_capacity_ramp':
                        await self.run_capacity_ramp_test()
                    elif scenario_name == 'consent_write_submission':
                        await self.run_write_submission_test()
                    elif scenario_name == 'consent_robustness':
                        await self.run_robustness_test()
                else:
                    print(f"\nSkipping {scenario_name} (disabled)")
        
        finally:
            await self.cleanup()
    
    def save_results(self, output_file: str = None):
        """Save test results to JSON file"""
        if output_file is None:
            output_file = self.config.get('reporting', {}).get('output_file', 'consent_stress_test_results.json')
        
        summary = self.metrics.get_summary()
        results = {
            'test_run_timestamp': datetime.now().isoformat(),
            'config': self.config,
            'summary': summary,
            'retry_stats': self.retry_handler.stats
        }
        
        with open(output_file, 'w') as f:
            json.dump(results, f, indent=2)
        
        print(f"\nResults saved to: {output_file}")


async def main():
    """Main entry point"""
    parser = argparse.ArgumentParser(description='Consent Form Stress Test')
    parser.add_argument('--config', default='consent_test_config.json', help='Configuration file path')
    parser.add_argument('--url', help='API base URL (overrides config)')
    parser.add_argument('--scenario', choices=['capacity_ramp', 'write_submission', 'robustness', 'all'],
                        default='all', help='Specific scenario to run')
    parser.add_argument('--output', help='Results output file')
    
    args = parser.parse_args()
    
    try:
        with open(args.config, 'r') as f:
            config = json.load(f)
    except FileNotFoundError:
        print(f"Configuration file {args.config} not found!")
        sys.exit(1)
    except json.JSONDecodeError as e:
        print(f"Invalid JSON in configuration file: {e}")
        sys.exit(1)
    
    base_url = args.url or config['api']['base_url']
    
    print("Consent Form Stress Test")
    print(f"Target URL: {base_url}")
    print(f"Configuration: {args.config}")
    
    runner = ConsentFormStressTestRunner(config, base_url)
    
    if args.scenario == 'all':
        await runner.run_all_tests()
    elif args.scenario == 'capacity_ramp':
        await runner.run_capacity_ramp_test()
    elif args.scenario == 'write_submission':
        await runner.run_write_submission_test()
    elif args.scenario == 'robustness':
        await runner.run_robustness_test()
    
    runner.save_results(args.output)
    print("\nStress testing completed!")


if __name__ == '__main__':
    asyncio.run(main())
