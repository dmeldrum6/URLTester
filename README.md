# Website Load Tester

A simple yet powerful command-line load testing tool built in C# for testing website performance under concurrent requests across multiple iterations.

## Features

- **Concurrent Testing**: Simulate multiple simultaneous connections to your target URL
- **Iterative Testing**: Run multiple test iterations to identify performance consistency
- **Comprehensive Reporting**: Detailed statistics including success rates, response times, and performance metrics
- **Smart Throttling**: Built-in connection limiting to prevent overwhelming target servers
- **Robust Error Handling**: Categorized error reporting for timeouts, network issues, and HTTP errors
- **User-Friendly Output**: Clean, formatted output with visual indicators and progress tracking

## Installation

### Prerequisites
- .NET 6.0 or later

### Build from Source
```bash
git clone https://github.com/yourusername/website-load-tester.git
cd website-load-tester
dotnet build -c Release
```

### Run without Building
```bash
dotnet run -- <URL> <NumberOfConnections> <NumberOfIterations>
```

### Create Executable
```bash
dotnet publish -c Release -o ./publish
```

## Usage

### Basic Syntax
```bash
URLTester <URL> <NumberOfConnections> <NumberOfIterations>
```

### Parameters
- **URL**: Target website URL (must include http:// or https://)
- **NumberOfConnections**: Number of simultaneous connections per iteration
- **NumberOfIterations**: Number of test iterations to run

### Examples

#### Basic Load Test
```bash
URLTester https://example.com 10 3
```
Runs 3 iterations with 10 concurrent connections each (30 total requests).

#### High-Concurrency Test
```bash
URLTester https://api.myservice.com/health 50 5
```
Tests API endpoint with 50 concurrent connections across 5 iterations.

#### Single Iteration Stress Test
```bash
URLTester https://mywebsite.com 100 1
```
Single iteration with 100 concurrent connections.

## Sample Output

```
Load Test Configuration:
Target URL: https://example.com
Connections per iteration: 10
Number of iterations: 3
Total requests: 30
============================================================

--- Iteration 1/3 ---
  ✓ Connection 1: OK (OK)
  ✓ Connection 2: OK (OK)
  ✗ Connection 3: Timeout
  ✓ Connection 4: OK (OK)
  ...
Iteration 1: 8/10 successful (80.0%) in 1247ms
Waiting 2 seconds before next iteration...

--- Iteration 2/3 ---
...

============================================================
LOAD TEST SUMMARY REPORT
============================================================
Total Iterations: 3
Total Requests: 30
Total Successes: 28
Total Failures: 2
Overall Success Rate: 93.33%
Total Execution Time: 12.45 seconds
Average Requests/Second: 2.41

Iteration Performance:
Average Time: 1156ms
Fastest Time: 987ms
Slowest Time: 1402ms
============================================================
```

## Configuration

### Built-in Safety Features
- **Connection Limiting**: Automatically limits concurrent connections to prevent resource exhaustion
- **Timeout Protection**: 30-second timeout per request to prevent hanging
- **User Confirmation**: Prompts for confirmation when using high connection counts (>1000)
- **Iteration Delays**: 2-second pause between iterations to avoid overwhelming servers

### HTTP Client Settings
- **User-Agent**: Identifies requests as "LoadTester/1.0"
- **Timeout**: 30 seconds per request
- **Connection Reuse**: Single HttpClient instance for efficiency

## Error Handling

The tool categorizes and reports different types of errors:

- **✓ Success**: HTTP 2xx status codes
- **✗ HTTP Error**: HTTP error status codes (4xx, 5xx)
- **✗ Timeout**: Request exceeded 30-second timeout
- **✗ Network Error**: DNS resolution, connection failures
- **✗ Exception**: Unexpected errors

## Performance Considerations

### Target Server Impact
- Uses connection throttling to prevent overwhelming target servers
- Includes delays between iterations
- Warns users about high connection counts
- Respects HTTP best practices

### Resource Usage
- Efficient HttpClient reuse
- Proper resource disposal
- Memory-conscious design for large test runs

## Best Practices

### Responsible Testing
- **Test your own services**: Only test websites you own or have explicit permission to test
- **Start small**: Begin with low connection counts and increase gradually
- **Monitor target**: Watch server resources during testing
- **Respect limits**: Don't exceed server capacity or terms of service

### Effective Load Testing
- **Baseline first**: Test with single connections to establish baseline performance
- **Gradual increase**: Incrementally increase load to find breaking points
- **Multiple iterations**: Run several iterations to identify consistency issues
- **Document results**: Save outputs for performance trend analysis

## Common Use Cases

- **API Performance Testing**: Test REST API endpoints under load
- **Website Stress Testing**: Verify website performance under traffic spikes
- **CI/CD Integration**: Automated performance testing in deployment pipelines
- **Capacity Planning**: Determine maximum concurrent user capacity
- **Performance Monitoring**: Regular health checks for production services

## Limitations

- **HTTP/HTTPS Only**: Supports web protocols only
- **GET Requests**: Currently limited to GET requests
- **No Authentication**: Does not support authenticated requests
- **Basic Metrics**: Simple success/failure tracking (no response time percentiles)

## Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

### Development Setup
```bash
git clone https://github.com/yourusername/website-load-tester.git
cd website-load-tester
dotnet restore
dotnet build
```

### Running Tests
```bash
dotnet test
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Disclaimer

This tool is intended for testing your own websites and services. Users are responsible for ensuring they have permission to test target URLs and for using the tool responsibly. The authors are not responsible for any misuse or damage caused by this software.

## Changelog

### Version 1.0.0
- Initial release with basic load testing functionality
- Concurrent connection support
- Iterative testing capability
- Comprehensive reporting
- Built-in safety features
