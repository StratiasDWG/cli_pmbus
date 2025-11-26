#!/bin/bash

# PMBus Master CLI Publish Script
# Creates standalone executable for distribution

set -e

echo "================================================"
echo "  PMBus Master CLI - Publish Script"
echo "================================================"
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null
then
    echo "ERROR: .NET SDK not found!"
    echo "Please install .NET 6.0 SDK or later"
    exit 1
fi

# Detect OS
OS=$(uname -s)
ARCH=$(uname -m)

case "$OS" in
    Linux*)
        if [ "$ARCH" = "x86_64" ]; then
            RID="linux-x64"
        elif [ "$ARCH" = "aarch64" ]; then
            RID="linux-arm64"
        else
            RID="linux-x64"
        fi
        ;;
    Darwin*)
        if [ "$ARCH" = "arm64" ]; then
            RID="osx-arm64"
        else
            RID="osx-x64"
        fi
        ;;
    MINGW*|MSYS*|CYGWIN*)
        RID="win-x64"
        ;;
    *)
        echo "Unknown OS: $OS"
        echo "Please specify runtime identifier manually"
        exit 1
        ;;
esac

echo "Detected platform: $RID"
echo ""

# Publish self-contained executable
echo "Publishing self-contained executable..."
dotnet publish -c Release -r "$RID" --self-contained true -p:PublishSingleFile=true -o ./publish

echo ""
echo "================================================"
echo "  Publish Complete!"
echo "================================================"
echo ""
echo "Standalone executable location:"
echo "  ./publish/pmbus-cli"
echo ""
echo "File size: $(du -h ./publish/pmbus-cli 2>/dev/null | cut -f1 || echo 'N/A')"
echo ""
echo "Run with:"
echo "  ./publish/pmbus-cli <command>"
echo ""
echo "To install system-wide (Linux/macOS):"
echo "  sudo cp ./publish/pmbus-cli /usr/local/bin/"
echo ""
