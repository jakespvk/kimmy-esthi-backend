# Kimmy Esthi API Stress Test

A comprehensive stress testing tool for the Kimmy Esthi appointment booking system.

## Features

- **Configurable URL** - Easily test localhost, staging, or production environments
- **Multiple Test Scenarios**:
  - Read Tests (GET endpoints)
  - Write Tests (POST endpoints) 
  - Mixed Tests (realistic user behavior)
  - Admin Tests (authentication required)
- **Real-time Metrics**:
  - Response times (average, 95th, 99th percentile)
  - Success/error rates
  - Requests per second
  - Endpoint-specific breakdowns
- **Concurrent Load Testing** - Simulate multiple simultaneous users
- **Ramp-up Periods** - Gradually increase load
- **Graceful Shutdown** - Clean handling of interruptions

## Quick Start

1. **Install Dependencies**:
```bash
cd stress-test
pip install -r requirements.txt
```

2. **Configure Tests**:
Edit `config.json` to adjust:
- API URL
- Test scenarios
- Concurrent users
- Test duration
- Endpoint weights

3. **Run Tests**:

```bash
# Run all tests against localhost
python stress_test.py

# Test your VPS
python stress_test.py --url https://your-vps-domain.com

# Run specific scenarios
python stress_test.py --scenarios read_test write_test

# Custom configuration file
python stress_test.py --config production-config.json
```

## Configuration

### Basic Setup (`config.json`)

```json
{
  "api": {
    "base_url": "http://localhost:5000",
    "timeout": 30
  },
  "test_scenarios": {
    "read_test": {
      "enabled": true,
      "concurrent_users": 50,
      "duration_seconds": 60,
      "ramp_up_seconds": 10,
      "endpoints": [...]
    }
  }
}
```

### Test Scenarios

1. **Read Test**: Tests GET endpoints for appointment data
2. **Write Test**: Tests appointment booking and consent forms
3. **Mixed Test**: Realistic combination of read/write operations
4. **Admin Test**: Tests admin endpoints with authentication

### Key Parameters

- `concurrent_users`: Number of simultaneous users
- `duration_seconds`: How long the test runs
- `ramp_up_seconds`: Time to gradually increase load
- `weight`: Relative frequency of each endpoint

## Understanding Results

### Sample Output

```
READ TEST RESULTS
==================================================
Duration: 60.12s
Total Requests: 1,247
Successful: 1,198
Failed: 49
Requests/sec: 20.74
Success Rate: 96.07%
Avg Response Time: 0.245s
95th Percentile: 0.892s
99th Percentile: 1.234s

Status Codes:
  200: 1198
  404: 49

Endpoint Breakdown:
  GET /appointments/status:
    Requests: 498
    Success Rate: 98.8%
    Avg Response Time: 0.156s
```

### Key Metrics

- **Requests/sec**: Throughput measurement
- **Success Rate**: Percentage of successful requests (2xx status codes)
- **Response Time Percentiles**: 
  - Average: Overall performance
  - 95th: 95% of requests are faster than this
  - 99th: 99% of requests are faster than this

## Testing Your VPS

1. **Update Configuration**:
```json
{
  "api": {
    "base_url": "https://your-vps-domain.com"
  }
}
```

2. **Run the Test**:
```bash
python stress_test.py --url https://your-vps-domain.com
```

3. **Monitor Results**:
- Look for high success rates (>95%)
- Check response times are reasonable
- Watch for specific error patterns

## Troubleshooting

### Common Issues

1. **"Connection refused"**:
   - Check API is running
   - Verify URL and port
   - Check firewall settings

2. **High failure rates**:
   - API may be overwhelmed
   - Database connection issues
   - Invalid test data

3. **Timeout errors**:
   - Increase timeout in config
   - API performance issues
   - Network latency

### Tips

- Start with small numbers of users and gradually increase
- Monitor your API server resources during tests
- Use realistic data volumes
- Test different times of day for production systems

## Advanced Usage

### Custom Test Scenarios

Add new scenarios in `config.json`:

```json
{
  "test_scenarios": {
    "custom_test": {
      "enabled": true,
      "concurrent_users": 25,
      "duration_seconds": 120,
      "ramp_up_seconds": 15,
      "endpoints": [
        {
          "path": "/appointments/2024-01-15",
          "method": "GET",
          "weight": 100
        }
      ]
    }
  }
}
```

### Running Specific Scenarios

```bash
python stress_test.py --scenarios read_test admin_test
```

### Saving Results

```bash
python stress_test.py --output results.json
```

## File Structure

```
stress-test/
├── stress_test.py          # Main stress testing script
├── test_data.py           # Test data generator
├── config.json            # Configuration file
├── requirements.txt       # Python dependencies
└── README.md             # This file
```

## Security Considerations

- The stress test includes admin credentials in the config
- Never commit real credentials to version control
- Use separate test environments when possible
- Monitor for unintended side effects on production data

## Contributing

To modify or extend the stress test:

1. **Add new endpoints**: Update `config.json`
2. **Custom test data**: Modify `test_data.py`
3. **New metrics**: Enhance `MetricsCollector` class
4. **Additional scenarios**: Create new scenario configurations

## License

This stress testing tool is provided for testing and development purposes. Use responsibly and only on systems you have permission to test.