# Testing Guide - PMBus Master CLI

This guide explains how to test the PMBus Master CLI application after applying improvements.

## Prerequisites

- .NET 6.0 SDK or later installed
- Clone the repository
- NI-8451 device (for hardware tests)

## Quick Start

```bash
# Navigate to project directory
cd /path/to/cli_pmbus

# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Test Structure

```
Tests/
├── LinearDataFormatTests.cs      # 30+ tests for LINEAR11/LINEAR16 conversions
├── PMBusCommandsTests.cs         # Command validation tests
└── PMBusSafetyTests.cs          # Safety feature tests
```

## Running Specific Test Classes

```bash
# Run only LinearDataFormat tests
dotnet test --filter "FullyQualifiedName~LinearDataFormatTests"

# Run only PMBusCommands tests
dotnet test --filter "FullyQualifiedName~PMBusCommandsTests"

# Run only PMBusSafety tests
dotnet test --filter "FullyQualifiedName~PMBusSafetyTests"
```

## Running Specific Tests

```bash
# Run a specific test method
dotnet test --filter "FullyQualifiedName~Linear11ToReal_KnownValues_CorrectConversion"

# Run all tests with "Linear11" in the name
dotnet test --filter "DisplayName~Linear11"
```

## Code Coverage

### Generate Coverage Report

```bash
# Install coverage report generator (one-time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# Open report (Linux/macOS)
open coveragereport/index.html

# Open report (Windows)
start coveragereport/index.html
```

### Coverage Thresholds

Current coverage targets:
- **LinearDataFormat.cs**: 90%+ (critical for accuracy)
- **PMBusCommands.cs**: 80%+ (lookup and validation)
- **PMBusSafety.cs**: 85%+ (safety is critical)
- **Overall**: 60%+ (good baseline)

## Test Categories

### 1. LinearDataFormat Tests (30+ tests)

**LINEAR11 Format:**
```bash
# Test LINEAR11 to real conversion
dotnet test --filter "FullyQualifiedName~Linear11ToReal"

# Test real to LINEAR11 conversion
dotnet test --filter "FullyQualifiedName~RealToLinear11"
```

**Test Cases:**
- Zero value
- Positive values (1.0, 10.0)
- Negative values (-1.0, -0.5)
- Smallest representable value
- Edge cases

**LINEAR16 Format:**
```bash
# Test LINEAR16 conversions
dotnet test --filter "FullyQualifiedName~Linear16"
```

**Test Cases:**
- Various voltages (3.3V, 5.0V, 12.0V)
- Different exponents (-13, -10, 0)
- Zero value

**VOUT_MODE Parsing:**
```bash
# Test VOUT_MODE parsing
dotnet test --filter "FullyQualifiedName~ParseVoutMode"
```

**Test Cases:**
- Linear mode (mode 0)
- VID mode (mode 1)
- Direct mode (mode 2)
- Various exponents (-16 to +15)

### 2. PMBusCommands Tests

```bash
# Run all command tests
dotnet test --filter "FullyQualifiedName~PMBusCommandsTests"
```

**Test Coverage:**
- Command name lookup
- Command code validation
- Range verification
- Unknown command handling

### 3. PMBusSafety Tests

```bash
# Run all safety tests
dotnet test --filter "FullyQualifiedName~PMBusSafetyTests"
```

**Test Coverage:**
- Dangerous command detection
- Voltage range validation
- Safety warning generation
- WriteResult formatting
- Invalid input handling

## Expected Output

### Successful Test Run

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    42, Skipped:     0, Total:    42, Duration: 156 ms
```

### Test Failure Example

```
Failed LinearDataFormatTests.Linear11ToReal_KnownValues_CorrectConversion [13 ms]
  Error Message:
   Assert.Equal() Failure
   Expected: 1.0
   Actual:   0.9999
```

## Continuous Testing

### Watch Mode (Auto-rerun on changes)

```bash
# Run tests automatically when files change
dotnet watch test
```

This is useful during development to get immediate feedback.

### CI/CD Integration

#### GitHub Actions Example

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 6.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"

    - name: Upload coverage
      uses: codecov/codecov-action@v3
```

## Adding New Tests

### Test Template

```csharp
using Xunit;
using PmbusMasterCLI;

namespace PmbusMasterCLI.Tests
{
    public class MyNewTests
    {
        [Fact]
        public void MyTest_Scenario_ExpectedBehavior()
        {
            // Arrange
            var input = "test";

            // Act
            var result = SomeMethod(input);

            // Assert
            Assert.Equal("expected", result);
        }

        [Theory]
        [InlineData(1, "one")]
        [InlineData(2, "two")]
        [InlineData(3, "three")]
        public void MyParameterizedTest(int input, string expected)
        {
            // Arrange & Act
            var result = NumberToString(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
```

### Best Practices

1. **Naming Convention**: `MethodName_Scenario_ExpectedBehavior`
2. **Arrange-Act-Assert**: Structure tests clearly
3. **One Assertion**: Generally one logical assertion per test
4. **Parameterize**: Use `[Theory]` for multiple similar cases
5. **Isolate**: Tests should be independent
6. **Fast**: Keep tests fast (< 100ms each)

## Troubleshooting

### "dotnet: command not found"

```bash
# Install .NET SDK
# Ubuntu/Debian:
sudo apt-get install -y dotnet-sdk-6.0

# macOS:
brew install --cask dotnet-sdk

# Windows:
# Download from https://dotnet.microsoft.com/download
```

### "No test is available"

```bash
# Ensure test project is built
dotnet build PmbusMasterCLI.Tests.csproj

# Verify test discovery
dotnet test --list-tests
```

### "Package restore failed"

```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Try again
dotnet test
```

### Coverage report not generating

```bash
# Ensure coverlet is installed
dotnet add package coverlet.collector

# Run with explicit coverage collection
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Performance Testing

While not included in the current test suite, you can add performance tests:

```bash
# Install BenchmarkDotNet (optional)
dotnet add package BenchmarkDotNet

# Run benchmarks
dotnet run -c Release --project Benchmarks
```

## Integration Testing

For integration tests with real hardware:

```bash
# Set environment variable for hardware address
export PMBUS_TEST_ADDRESS=0x58

# Run integration tests
dotnet test --filter "Category=Integration"
```

**Note**: Integration tests require NI-8451 hardware connected.

## Test Coverage Goals

| Component | Current | Target |
|-----------|---------|--------|
| LinearDataFormat | ~90% | 95% |
| PMBusCommands | ~80% | 85% |
| PMBusSafety | ~85% | 90% |
| PMBusProtocol | ~40% | 70% |
| Overall | ~60% | 75% |

## Next Steps

1. Run initial test suite: `dotnet test`
2. Review coverage: Generate and review HTML report
3. Add tests for edge cases you discover
4. Integrate into CI/CD pipeline
5. Monitor coverage trends

## Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

**Last Updated**: 2025-11-26
