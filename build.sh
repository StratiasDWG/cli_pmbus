#!/bin/bash

# PMBus Master CLI Build Script
# Requires .NET 6.0 SDK or later

set -e

echo "================================================"
echo "  PMBus Master CLI - Build Script"
echo "================================================"
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null
then
    echo "ERROR: .NET SDK not found!"
    echo ""
    echo "Please install .NET 6.0 SDK or later:"
    echo "  - Windows/macOS/Linux: https://dotnet.microsoft.com/download"
    echo "  - Ubuntu/Debian: sudo apt-get install -y dotnet-sdk-6.0"
    echo "  - Fedora/CentOS: sudo dnf install dotnet-sdk-6.0"
    echo "  - macOS (Homebrew): brew install --cask dotnet-sdk"
    echo ""
    exit 1
fi

echo "✓ .NET SDK found: $(dotnet --version)"
echo ""

# Restore dependencies
echo "Restoring NuGet packages..."
dotnet restore
echo ""

# Build in Release mode
echo "Building in Release mode..."
dotnet build -c Release
echo ""

# Show build output location
echo "================================================"
echo "  Build Complete!"
echo "================================================"
echo ""
echo "Binary location:"
echo "  ./bin/Release/net6.0/pmbus-cli.dll"
echo ""
echo "Run with:"
echo "  dotnet run -- <command>"
echo "  or"
echo "  dotnet ./bin/Release/net6.0/pmbus-cli.dll <command>"
echo ""
echo "For standalone executable, run:"
echo "  ./publish.sh"
echo ""
