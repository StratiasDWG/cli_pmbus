# PMBus Master CLI - Complete Build Guide

This guide provides comprehensive instructions for building the PMBus Master CLI application using various methods.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Build Methods](#build-methods)
   - [Method 1: Local .NET Build](#method-1-local-net-build)
   - [Method 2: Docker Build](#method-2-docker-build)
   - [Method 3: GitHub Actions CI/CD](#method-3-github-actions-cicd)
4. [Build Outputs](#build-outputs)
5. [Running Tests](#running-tests)
6. [Creating Release Packages](#creating-release-packages)
7. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### For Local .NET Build
- **.NET 6.0 SDK** or later
  - Download: https://dotnet.microsoft.com/download/dotnet/6.0
  - Verify installation: `dotnet --version`

### For Docker Build
- **Docker** and **Docker Compose**
  - Download: https://docs.docker.com/get-docker/
  - Verify installation: `docker --version` and `docker-compose --version`

### For All Builds
- **Git** (for cloning the repository)
- **NI-8451 drivers** (for runtime, not required for building)

---

## Quick Start

### Fastest Method (Docker - No .NET Installation Required)

```bash
# Clone the repository
git clone https://github.com/StratiasDWG/cli_pmbus.git
cd cli_pmbus

# Build using Docker (extracts binaries to ./bin directory)
docker build --target export -o bin .

# The compiled binary is now in ./bin/pmbus-cli.dll
# Run it with: dotnet ./bin/pmbus-cli.dll --help
```

### Standard Method (Local .NET)

```bash
# Clone the repository
git clone https://github.com/StratiasDWG/cli_pmbus.git
cd cli_pmbus

# Build the application
dotnet build -c Release

# Run tests
dotnet test -c Release

# Publish the application
dotnet publish -c Release -o ./publish
```

---

## Build Methods

## Method 1: Local .NET Build

This method requires .NET 6.0 SDK installed on your system.

### 1.1 Basic Build

```bash
# Restore NuGet packages
dotnet restore PmbusMasterCLI.csproj

# Build in Debug mode
dotnet build PmbusMasterCLI.csproj -c Debug

# Build in Release mode (optimized)
dotnet build PmbusMasterCLI.csproj -c Release
```

**Output location:** `./bin/Release/net6.0/pmbus-cli.dll`

### 1.2 Build with Tests

```bash
# Build and run tests
dotnet build PmbusMasterCLI.csproj -c Release
dotnet test PmbusMasterCLI.Tests.csproj -c Release --verbosity normal

# Build with code coverage
dotnet test PmbusMasterCLI.Tests.csproj \
  -c Release \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=./coverage/
```

### 1.3 Publish (Deployment Build)

```bash
# Publish framework-dependent deployment (requires .NET runtime)
dotnet publish PmbusMasterCLI.csproj -c Release -o ./publish

# Publish self-contained deployment for Linux x64
dotnet publish PmbusMasterCLI.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -o ./publish/linux-x64

# Publish self-contained deployment for Windows x64
dotnet publish PmbusMasterCLI.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -o ./publish/win-x64

# Publish self-contained deployment for macOS x64
dotnet publish PmbusMasterCLI.csproj \
  -c Release \
  -r osx-x64 \
  --self-contained true \
  -o ./publish/osx-x64
```

### 1.4 Publish Single-File Executable

```bash
# Linux x64 - Single file, trimmed, ready-to-run
dotnet publish PmbusMasterCLI.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -p:PublishReadyToRun=true \
  -o ./publish/linux-x64-single

# Windows x64 - Single file executable
dotnet publish PmbusMasterCLI.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -o ./publish/win-x64-single

# macOS x64 - Single file executable
dotnet publish PmbusMasterCLI.csproj \
  -c Release \
  -r osx-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -o ./publish/osx-x64-single
```

### 1.5 Using Build Scripts

Convenient build scripts are provided:

**Linux/macOS:**
```bash
# Basic build
./build.sh

# Publish for deployment
./publish.sh
```

**Windows:**
```cmd
REM Basic build
build.bat

REM Publish for deployment
publish.bat
```

---

## Method 2: Docker Build

This method requires Docker installed but **does not require .NET SDK** on your system.

### 2.1 Docker Build - Extract Binaries

This is the recommended method for users without .NET SDK:

```bash
# Build and extract compiled binaries to ./bin directory
docker build --target export -o bin .

# The output will be in ./bin/
# Run with: dotnet ./bin/pmbus-cli.dll --help
```

### 2.2 Docker Build - Full Build and Test

```bash
# Build the Docker image (includes build and test)
docker build -t pmbus-cli:latest .

# Run tests inside container
docker run --rm pmbus-cli:latest dotnet test
```

### 2.3 Docker Compose Build

```bash
# Build using docker-compose
docker-compose run --rm build

# Extract build artifacts
docker build --target export -o bin .
```

### 2.4 Docker Build - Runtime Container

**Note:** Running the CLI in a Docker container requires special USB device access configuration.

```bash
# Build runtime container
docker build --target runtime -t pmbus-cli:latest .

# Run help command
docker run --rm pmbus-cli:latest --help

# Run with USB device access (Linux only, requires privileged mode)
docker run --rm --privileged -v /dev/bus/usb:/dev/bus/usb pmbus-cli:latest list
```

---

## Method 3: GitHub Actions CI/CD

This project includes a comprehensive GitHub Actions workflow that automatically builds, tests, and packages the application.

### 3.1 Automatic Builds

The CI/CD pipeline runs automatically on:
- **Push** to `main`, `master`, `develop`, or `claude/**` branches
- **Pull requests** to `main`, `master`, or `develop`
- **Release** creation
- **Manual trigger** via GitHub Actions UI

### 3.2 Build Matrix

The workflow builds and tests on:
- Ubuntu (Linux)
- Windows
- macOS

All in **Release** configuration.

### 3.3 Artifacts Generated

For each build, the following artifacts are created:

1. **Build Artifacts** (per OS):
   - `pmbus-cli-ubuntu-latest-Release`
   - `pmbus-cli-windows-latest-Release`
   - `pmbus-cli-macos-latest-Release`

2. **Test Results** (per OS):
   - `test-results-ubuntu-latest`
   - `test-results-windows-latest`
   - `test-results-macos-latest`

3. **Code Coverage**:
   - `coverage-report`

4. **Release Packages** (on release or main/master):
   - `pmbus-cli-v2.0.0-linux-x64.tar.gz`
   - `pmbus-cli-v2.0.0-windows-x64.zip`
   - `pmbus-cli-v2.0.0-macos-x64.tar.gz`

5. **Docker Build Artifacts**:
   - `docker-build-artifacts`

### 3.4 Download Artifacts

```bash
# Using GitHub CLI (gh)
gh run list
gh run download <run-id>

# Or download from GitHub Actions UI:
# 1. Go to Actions tab
# 2. Click on a workflow run
# 3. Scroll to "Artifacts" section
# 4. Download desired artifacts
```

### 3.5 Manual Workflow Trigger

```bash
# Using GitHub CLI
gh workflow run build.yml

# Or use GitHub Actions UI:
# 1. Go to Actions tab
# 2. Select "Build and Test" workflow
# 3. Click "Run workflow" button
```

---

## Build Outputs

### Framework-Dependent Deployment (FDD)

**Location:** `./bin/Release/net6.0/` or `./publish/`

**Files:**
- `pmbus-cli.dll` - Main assembly
- `pmbus-cli.deps.json` - Dependency manifest
- `pmbus-cli.runtimeconfig.json` - Runtime configuration
- `Ni845x.dll` - NI-8451 driver library
- Other dependencies

**Run command:**
```bash
dotnet pmbus-cli.dll --help
```

**Requirements:**
- .NET 6.0 Runtime (smaller download: ~30 MB)
- Works on any platform with .NET runtime

**Size:** ~500 KB - 2 MB (excluding .NET runtime)

### Self-Contained Deployment (SCD)

**Location:** `./publish/linux-x64/`, `./publish/win-x64/`, `./publish/osx-x64/`

**Files:**
- All .NET runtime files
- `pmbus-cli` or `pmbus-cli.exe` - Executable
- `pmbus-cli.dll` - Main assembly
- `Ni845x.dll` - NI-8451 driver library
- 100+ runtime libraries

**Run command:**
```bash
# Linux/macOS
./pmbus-cli --help

# Windows
pmbus-cli.exe --help
```

**Requirements:**
- No .NET installation required
- Platform-specific (Linux, Windows, or macOS)

**Size:** ~60-80 MB (includes .NET runtime)

### Single-File Deployment

**Location:** `./publish/linux-x64-single/`, `./publish/win-x64-single/`, `./publish/osx-x64-single/`

**Files:**
- Single executable file: `pmbus-cli` or `pmbus-cli.exe`
- `Ni845x.dll` (must be in same directory or system path)

**Run command:**
```bash
# Linux/macOS
./pmbus-cli --help

# Windows
pmbus-cli.exe --help
```

**Requirements:**
- No .NET installation required
- Platform-specific
- NI-8451 drivers must be installed

**Size:** ~50-70 MB (single file, trimmed)

---

## Running Tests

### Unit Tests

```bash
# Run all tests
dotnet test PmbusMasterCLI.Tests.csproj -c Release --verbosity normal

# Run tests with detailed output
dotnet test PmbusMasterCLI.Tests.csproj -c Release --verbosity detailed

# Run specific test class
dotnet test PmbusMasterCLI.Tests.csproj --filter "FullyQualifiedName~InputValidatorTests"

# Run tests matching pattern
dotnet test PmbusMasterCLI.Tests.csproj --filter "Name~Validate"
```

### Code Coverage

```bash
# Run tests with code coverage
dotnet test PmbusMasterCLI.Tests.csproj \
  -c Release \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=./coverage/

# Generate coverage report (requires reportgenerator tool)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:./coverage/coverage.opencover.xml -targetdir:./coverage/report -reporttypes:Html

# Open coverage report
xdg-open ./coverage/report/index.html  # Linux
open ./coverage/report/index.html      # macOS
start ./coverage/report/index.html     # Windows
```

### Test Results

Test results are saved in TRX format:
- **Location:** `./TestResults/`
- **Format:** `.trx` (Visual Studio Test Results)

---

## Creating Release Packages

### Using the Package Script

A release packaging script is provided:

```bash
# Linux/macOS
./package-release.sh

# This will create:
# - pmbus-cli-v2.0.0-linux-x64.tar.gz
# - pmbus-cli-v2.0.0-windows-x64.zip
# - pmbus-cli-v2.0.0-macos-x64.tar.gz
```

### Manual Packaging

```bash
# Build all platforms
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/linux-x64
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-x64

# Create archives
cd publish
tar -czf pmbus-cli-v2.0.0-linux-x64.tar.gz linux-x64/
zip -r pmbus-cli-v2.0.0-windows-x64.zip win-x64/
tar -czf pmbus-cli-v2.0.0-macos-x64.tar.gz osx-x64/
```

### Release Checklist

Before creating a release:

1. ✅ Update version in `PmbusMasterCLI.csproj`
2. ✅ Update `CHANGELOG.md` (if exists)
3. ✅ Run all tests: `dotnet test -c Release`
4. ✅ Build all platforms
5. ✅ Test each platform build
6. ✅ Create release packages
7. ✅ Test release packages on target platforms
8. ✅ Create Git tag: `git tag -a v2.0.0 -m "Release v2.0.0"`
9. ✅ Push tag: `git push origin v2.0.0`
10. ✅ Create GitHub release with artifacts

---

## Troubleshooting

### Common Issues

#### 1. `.NET SDK not found`

**Error:**
```
dotnet: command not found
```

**Solution:**
- Install .NET 6.0 SDK from https://dotnet.microsoft.com/download/dotnet/6.0
- Verify: `dotnet --version`
- Alternative: Use Docker build method (no .NET required)

#### 2. `NuGet package restore failed`

**Error:**
```
error NU1301: Unable to load the service index for source
```

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with verbose logging
dotnet restore --verbosity detailed

# Use alternative NuGet source
dotnet restore --source https://api.nuget.org/v3/index.json
```

#### 3. `System.CommandLine package not found`

**Error:**
```
error NU1102: Unable to find package 'System.CommandLine'
```

**Solution:**
```bash
# Add the package explicitly
dotnet add package System.CommandLine --version 2.0.0-beta4.22272.1

# Or restore with pre-release packages
dotnet restore --include-prerelease
```

#### 4. `Build succeeded but tests failed`

**Common causes:**
- Missing NI-8451 drivers (mock tests should still pass)
- Incorrect test configuration

**Solution:**
```bash
# Run tests with detailed output
dotnet test --verbosity detailed

# Run tests in isolation
dotnet test --no-build --verbosity normal
```

#### 5. `Docker build fails - cannot copy files`

**Error:**
```
ERROR [build 4/8] COPY *.csproj ./
```

**Solution:**
- Ensure you're in the project root directory
- Check `.dockerignore` doesn't exclude `.csproj` files
- Run: `docker build --no-cache -t pmbus-cli:latest .`

#### 6. `Publish fails - runtime not found`

**Error:**
```
error NU1202: Package is not compatible with net6.0 (.NETCoreApp,Version=v6.0)
```

**Solution:**
```bash
# List available runtimes
dotnet --list-runtimes
dotnet --list-sdks

# Install required runtime
# Download from: https://dotnet.microsoft.com/download/dotnet/6.0
```

#### 7. `Permission denied when running binary`

**Linux/macOS:**
```bash
chmod +x pmbus-cli
./pmbus-cli --help
```

#### 8. `Ni845x.dll not found at runtime`

**Solution:**
- Ensure NI-8451 drivers are installed
- Windows: Add NI-845x directory to PATH
- Linux: Copy `Ni845x.dll` to application directory or `/usr/lib`
- macOS: Copy to application directory

### Performance Issues

#### Slow Build Times

```bash
# Use parallel builds
dotnet build -c Release -m

# Disable incremental builds
dotnet build --no-incremental

# Clean before build
dotnet clean && dotnet build -c Release
```

#### Large Binary Size

```bash
# Enable trimming
dotnet publish -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishTrimmed=true \
  -p:TrimMode=link

# Enable ready-to-run compilation
dotnet publish -c Release \
  -r linux-x64 \
  -p:PublishReadyToRun=true
```

### Getting Help

If you encounter issues not covered here:

1. Check existing issues: https://github.com/StratiasDWG/cli_pmbus/issues
2. Review documentation: `README.md`, `QUICKSTART.md`, `EXAMPLES.md`
3. Run with verbose logging: `dotnet build --verbosity detailed`
4. Create new issue with:
   - Build command used
   - Full error output
   - Platform and .NET version
   - Docker version (if using Docker)

---

## Additional Resources

- **Main Documentation:** `README.md`
- **Quick Start Guide:** `QUICKSTART.md`
- **Usage Examples:** `EXAMPLES.md`
- **Configuration Guide:** `CONFIGURATION.md`
- **Testing Guide:** `TESTING.md`
- **Improvements Log:** `IMPROVEMENTS.md`
- **Analysis Report:** `ANALYSIS_AND_FIXES.md`

---

**Version:** 2.0.0
**Last Updated:** November 2024
**Project:** PMBus Master CLI
**License:** See LICENSE file
