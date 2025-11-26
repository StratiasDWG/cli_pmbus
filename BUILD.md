# Building PMBus Master CLI

## Prerequisites

You need the .NET 6.0 SDK or later installed to build this application.

### Install .NET SDK

**Windows:**
```powershell
# Download and run installer from:
https://dotnet.microsoft.com/download
```

**Ubuntu/Debian:**
```bash
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-6.0
```

**Fedora/RHEL/CentOS:**
```bash
sudo dnf install dotnet-sdk-6.0
```

**macOS (Homebrew):**
```bash
brew install --cask dotnet-sdk
```

## Build Instructions

### Option 1: Using Build Scripts

**Linux/macOS:**
```bash
# Make scripts executable
chmod +x build.sh publish.sh

# Build the project
./build.sh

# Create standalone executable (optional)
./publish.sh
```

**Windows:**
```cmd
REM Build the project
build.bat

REM Create standalone executable (optional)
publish.bat
```

### Option 2: Manual Build

**Development Build:**
```bash
# Restore NuGet packages
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build -c Release
```

**Run without installing:**
```bash
# Run directly with dotnet
dotnet run -- list
dotnet run -- scan
dotnet run -- info -a 0x58
```

**Create standalone executable:**
```bash
# Linux x64
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish

# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish

# macOS Intel
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ./publish

# macOS Apple Silicon
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

## Build Output

After building, you'll find:

**Development build:**
- Binary: `./bin/Debug/net6.0/pmbus-cli.dll` or `./bin/Release/net6.0/pmbus-cli.dll`
- Run with: `dotnet ./bin/Release/net6.0/pmbus-cli.dll <command>`

**Published standalone:**
- Executable: `./publish/pmbus-cli` (Linux/macOS) or `./publish/pmbus-cli.exe` (Windows)
- Run directly: `./publish/pmbus-cli <command>`

## Installation

**Linux/macOS:**
```bash
# Build standalone executable
./publish.sh

# Install system-wide
sudo cp ./publish/pmbus-cli /usr/local/bin/
sudo chmod +x /usr/local/bin/pmbus-cli

# Now run from anywhere
pmbus-cli list
```

**Windows:**
```cmd
REM Build standalone executable
publish.bat

REM Add to PATH or copy to a location in PATH
copy .\publish\pmbus-cli.exe C:\Windows\System32\

REM Now run from anywhere
pmbus-cli list
```

## Verify Installation

```bash
# Check .NET version
dotnet --version

# Should be 6.0.0 or later
```

## Troubleshooting

### "dotnet: command not found"
- Install .NET SDK (see Prerequisites above)
- Restart your terminal/command prompt
- Verify installation: `dotnet --version`

### Build Errors
- Ensure you're in the project directory
- Run `dotnet restore` first
- Check you have .NET 6.0 or later: `dotnet --version`
- Delete `bin/` and `obj/` folders and rebuild

### NuGet Package Restore Fails
- Check internet connection
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Retry: `dotnet restore`

### Runtime Errors
- Ensure NI-845x driver is installed
- On Linux, you may need to run with `sudo` for USB access
- Check USB permissions: `sudo usermod -a -G dialout $USER` (logout/login required)

## Development

For active development:

```bash
# Watch mode (auto-rebuild on changes)
dotnet watch run -- list

# Run tests (if added)
dotnet test

# Clean build artifacts
dotnet clean
```

## Platform-Specific Notes

### Windows
- Requires NI-845x driver from National Instruments
- The `Ni845x.dll` must be in system PATH or same directory as executable
- Usually installed to: `C:\Program Files (x86)\National Instruments\NI-845x\MS .NET\`

### Linux
- Requires NI-845x Linux driver
- May need USB permissions: `sudo usermod -a -G dialout $USER`
- Some distros require `libusb-1.0-0` package

### macOS
- Requires NI-845x macOS driver
- May need to approve USB device access in System Preferences
- Code signing may be required for distribution

## CI/CD Build

For automated builds:

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build -c Release

# Run tests
dotnet test -c Release

# Publish for multiple platforms
dotnet publish -c Release -r linux-x64 -o ./dist/linux-x64
dotnet publish -c Release -r win-x64 -o ./dist/win-x64
dotnet publish -c Release -r osx-x64 -o ./dist/osx-x64
```
