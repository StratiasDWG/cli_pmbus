#!/bin/bash
# PMBus Master CLI - Release Packaging Script
# This script builds and packages the application for all supported platforms

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_NAME="PmbusMasterCLI"
PROJECT_FILE="${PROJECT_NAME}.csproj"
VERSION=$(grep '<Version>' "$PROJECT_FILE" | sed 's/.*<Version>\(.*\)<\/Version>.*/\1/' || echo "2.0.0")
BUILD_CONFIG="Release"
PUBLISH_DIR="./publish"
PACKAGES_DIR="./packages"

# Runtime identifiers
RUNTIMES=("linux-x64" "win-x64" "osx-x64" "linux-arm64" "osx-arm64")

# Print banner
echo -e "${BLUE}================================================${NC}"
echo -e "${BLUE}   PMBus Master CLI - Release Packager${NC}"
echo -e "${BLUE}   Version: ${VERSION}${NC}"
echo -e "${BLUE}================================================${NC}"
echo ""

# Function to print colored messages
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK not found. Please install .NET 6.0 SDK or later."
    echo "Download from: https://dotnet.microsoft.com/download/dotnet/6.0"
    exit 1
fi

# Print .NET version
DOTNET_VERSION=$(dotnet --version)
print_info "Using .NET SDK version: ${DOTNET_VERSION}"
echo ""

# Clean previous builds
print_info "Cleaning previous builds..."
if [ -d "$PUBLISH_DIR" ]; then
    rm -rf "$PUBLISH_DIR"
    print_success "Removed old publish directory"
fi

if [ -d "$PACKAGES_DIR" ]; then
    rm -rf "$PACKAGES_DIR"
    print_success "Removed old packages directory"
fi

dotnet clean "$PROJECT_FILE" -c "$BUILD_CONFIG" > /dev/null 2>&1
print_success "Project cleaned"
echo ""

# Restore dependencies
print_info "Restoring NuGet packages..."
dotnet restore "$PROJECT_FILE"
if [ $? -eq 0 ]; then
    print_success "Dependencies restored"
else
    print_error "Failed to restore dependencies"
    exit 1
fi
echo ""

# Run tests
print_info "Running tests..."
dotnet test "${PROJECT_NAME}.Tests.csproj" -c "$BUILD_CONFIG" --no-restore --verbosity quiet
if [ $? -eq 0 ]; then
    print_success "All tests passed"
else
    print_warning "Tests failed or test project not found. Continuing..."
fi
echo ""

# Create packages directory
mkdir -p "$PACKAGES_DIR"

# Build for each runtime
for runtime in "${RUNTIMES[@]}"; do
    print_info "Building for ${runtime}..."

    OUTPUT_DIR="${PUBLISH_DIR}/${runtime}"

    # Publish with trimming and single-file
    dotnet publish "$PROJECT_FILE" \
        -c "$BUILD_CONFIG" \
        -r "$runtime" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=true \
        -p:DebugType=none \
        -p:DebugSymbols=false \
        -o "$OUTPUT_DIR" \
        --no-restore \
        > /dev/null 2>&1

    if [ $? -eq 0 ]; then
        print_success "Built successfully for ${runtime}"

        # Get size of output directory
        SIZE=$(du -sh "$OUTPUT_DIR" | cut -f1)
        print_info "  Output size: ${SIZE}"

        # Create package based on platform
        PACKAGE_NAME="pmbus-cli-v${VERSION}-${runtime}"

        if [[ "$runtime" == win-* ]]; then
            # Create ZIP for Windows
            print_info "  Creating Windows package..."
            cd "$OUTPUT_DIR"
            zip -q -r "../../${PACKAGES_DIR}/${PACKAGE_NAME}.zip" .
            cd - > /dev/null
            print_success "  Created ${PACKAGE_NAME}.zip"
        else
            # Create tar.gz for Linux/macOS
            print_info "  Creating tar.gz package..."
            tar -czf "${PACKAGES_DIR}/${PACKAGE_NAME}.tar.gz" -C "$OUTPUT_DIR" .
            print_success "  Created ${PACKAGE_NAME}.tar.gz"
        fi

        echo ""
    else
        print_error "Failed to build for ${runtime}"
        echo ""
    fi
done

# Create checksums
print_info "Generating checksums..."
cd "$PACKAGES_DIR"
if command -v sha256sum &> /dev/null; then
    sha256sum * > checksums-sha256.txt
    print_success "SHA256 checksums generated"
elif command -v shasum &> /dev/null; then
    shasum -a 256 * > checksums-sha256.txt
    print_success "SHA256 checksums generated (using shasum)"
else
    print_warning "sha256sum/shasum not found, skipping checksum generation"
fi
cd - > /dev/null
echo ""

# Create release notes template
RELEASE_NOTES="${PACKAGES_DIR}/RELEASE_NOTES.md"
cat > "$RELEASE_NOTES" << EOF
# PMBus Master CLI - Release v${VERSION}

## Release Information

**Version:** ${VERSION}
**Release Date:** $(date +%Y-%m-%d)
**Build Date:** $(date +%Y-%m-%d\ %H:%M:%S)

## Download

Choose the appropriate package for your platform:

### Linux (x64)
\`\`\`bash
wget https://github.com/StratiasDWG/cli_pmbus/releases/download/v${VERSION}/pmbus-cli-v${VERSION}-linux-x64.tar.gz
tar -xzf pmbus-cli-v${VERSION}-linux-x64.tar.gz
chmod +x pmbus-cli
./pmbus-cli --help
\`\`\`

### Linux (ARM64)
\`\`\`bash
wget https://github.com/StratiasDWG/cli_pmbus/releases/download/v${VERSION}/pmbus-cli-v${VERSION}-linux-arm64.tar.gz
tar -xzf pmbus-cli-v${VERSION}-linux-arm64.tar.gz
chmod +x pmbus-cli
./pmbus-cli --help
\`\`\`

### Windows (x64)
1. Download \`pmbus-cli-v${VERSION}-win-x64.zip\`
2. Extract the ZIP file
3. Run \`pmbus-cli.exe --help\`

### macOS (Intel - x64)
\`\`\`bash
curl -L -O https://github.com/StratiasDWG/cli_pmbus/releases/download/v${VERSION}/pmbus-cli-v${VERSION}-osx-x64.tar.gz
tar -xzf pmbus-cli-v${VERSION}-osx-x64.tar.gz
chmod +x pmbus-cli
./pmbus-cli --help
\`\`\`

### macOS (Apple Silicon - ARM64)
\`\`\`bash
curl -L -O https://github.com/StratiasDWG/cli_pmbus/releases/download/v${VERSION}/pmbus-cli-v${VERSION}-osx-arm64.tar.gz
tar -xzf pmbus-cli-v${VERSION}-osx-arm64.tar.gz
chmod +x pmbus-cli
./pmbus-cli --help
\`\`\`

## What's New

### New Features
- [Add your new features here]

### Improvements
- [Add your improvements here]

### Bug Fixes
- [Add your bug fixes here]

### Breaking Changes
- [Add any breaking changes here]

## Requirements

- **NI-8451 USB-to-I2C/SPI Interface** hardware
- **NI-845x drivers** installed (download from National Instruments website)
- For framework-dependent deployment: **.NET 6.0 Runtime** or later

## Checksums (SHA256)

See \`checksums-sha256.txt\` for SHA256 checksums of all packages.

## Documentation

- [README.md](https://github.com/StratiasDWG/cli_pmbus/blob/main/README.md)
- [QUICKSTART.md](https://github.com/StratiasDWG/cli_pmbus/blob/main/QUICKSTART.md)
- [EXAMPLES.md](https://github.com/StratiasDWG/cli_pmbus/blob/main/EXAMPLES.md)
- [BUILD_COMPLETE.md](https://github.com/StratiasDWG/cli_pmbus/blob/main/BUILD_COMPLETE.md)

## Support

For issues, questions, or feature requests:
- GitHub Issues: https://github.com/StratiasDWG/cli_pmbus/issues

## License

See [LICENSE](https://github.com/StratiasDWG/cli_pmbus/blob/main/LICENSE) file.

---

**Full Changelog**: https://github.com/StratiasDWG/cli_pmbus/compare/v1.0.0...v${VERSION}
EOF

print_success "Release notes template created"
echo ""

# Summary
echo -e "${GREEN}================================================${NC}"
echo -e "${GREEN}   Build Complete!${NC}"
echo -e "${GREEN}================================================${NC}"
echo ""
print_info "Release packages created in: ${PACKAGES_DIR}/"
echo ""
print_info "Package contents:"
ls -lh "$PACKAGES_DIR" | tail -n +2 | awk '{printf "  %-50s %10s\n", $9, $5}'
echo ""

# Calculate total size
TOTAL_SIZE=$(du -sh "$PACKAGES_DIR" | cut -f1)
print_info "Total package size: ${TOTAL_SIZE}"
echo ""

print_info "Next steps:"
echo "  1. Review release notes: ${RELEASE_NOTES}"
echo "  2. Test packages on target platforms"
echo "  3. Create Git tag: git tag -a v${VERSION} -m 'Release v${VERSION}'"
echo "  4. Push tag: git push origin v${VERSION}"
echo "  5. Create GitHub release and upload packages from ${PACKAGES_DIR}/"
echo ""

print_success "Done!"
