# Quick Start Guide - PMBus Master CLI

Get up and running with PMBus Master CLI in 5 minutes!

## Prerequisites

- ✅ NI-8451 USB device connected
- ✅ NI-845x driver installed ([download here](https://www.ni.com/en-us/support/downloads/drivers/download.ni-845x.html))
- ✅ .NET 6.0 SDK or later ([download here](https://dotnet.microsoft.com/download))
- ✅ PMBus power supply connected via I2C

## Installation

### Option 1: Build from Source

```bash
# Clone repository
git clone <repository-url>
cd cli_pmbus

# Build
./build.sh          # Linux/macOS
# or
build.bat           # Windows

# Run
dotnet run -- list
```

### Option 2: Use Pre-built Binary

```bash
# Publish standalone executable
./publish.sh        # Linux/macOS
# or
publish.bat         # Windows

# Run
./publish/pmbus-cli list
```

## 30-Second Test

```bash
# 1. Find NI-8451 device
pmbus-cli list

# 2. Scan for PMBus devices
pmbus-cli scan

# 3. Read device info (use address from scan)
pmbus-cli info -a 0x58

# Success! You're ready to use the CLI.
```

## Common Tasks

### Read Device Information

```bash
pmbus-cli info -a 0x58
```

**Output:**
```
═══════════════════════════════════════════════
  PMBus Device Information - Address 0x58
═══════════════════════════════════════════════

  Manufacturer ID:    Texas Instruments
  Model:              TPS546D24
  Revision:           A01
  PMBus Revision:     1.3
```

### Check Status

```bash
pmbus-cli status -a 0x58
```

**Output:**
```
  STATUS_WORD:        0x0000
  Status:             ✓ OK
```

### Monitor Telemetry

```bash
pmbus-cli monitor -a 0x58
```

**Output:**
```
Time         VIN (V)    VOUT (V)   IIN (A)    IOUT (A)   TEMP (°C)    PIN (W)    POUT (W)
────────────────────────────────────────────────────────────────────────────────────────
10:30:00     12.000     3.300      0.500      5.000      45.0         6.000      16.500
10:30:01     12.001     3.301      0.501      5.001      45.1         6.005      16.508
```

Press Ctrl+C to stop.

### Set Output Voltage

```bash
pmbus-cli set-voltage --value 3.3 -a 0x58
```

**Output:**
```
Setting output voltage to 3.300V...
  Command sent:   3.300V
  Readback:       3.300V
  ✓ Success!
```

### Turn Output On/Off

```bash
# Turn on
pmbus-cli operation --operation on -a 0x58

# Turn off
pmbus-cli operation --operation off -a 0x58
```

### Clear Faults

```bash
pmbus-cli clear-faults -a 0x58
```

## Configuration (Optional)

Create configuration file to set defaults:

```bash
# Create config directory
mkdir -p ~/.pmbus-cli

# Copy sample configuration
cp config.sample.json ~/.pmbus-cli/config.json

# Edit with your preferences
nano ~/.pmbus-cli/config.json
```

**Basic config.json:**
```json
{
  "DefaultAddress": 88,
  "DefaultClockRate": 100,
  "EnableColorOutput": true,
  "RequireWriteConfirmation": true
}
```

Now you can use shorter commands:
```bash
# Instead of: pmbus-cli info -a 0x58
pmbus-cli info
```

## Output Formats

### JSON (for scripts)

```bash
pmbus-cli info -a 0x58 --output json
```

```json
{
  "Manufacturer": "Texas Instruments",
  "Model": "TPS546D24",
  "Revision": "A01"
}
```

### CSV (for logging)

```bash
pmbus-cli monitor -a 0x58 --output csv > log.csv
```

## Safety Features

### Write Confirmation

Dangerous operations require confirmation:

```bash
$ pmbus-cli write --command 0x01 --data 80 -a 0x58

⚠️  WARNING: This will change the output state (ON/OFF).
             The power supply may turn on or off.

Type 'yes' to continue or anything else to cancel: yes
```

**Bypass for automation:**
```bash
pmbus-cli write --command 0x01 --data 80 -a 0x58 --force
```

## Troubleshooting

### No NI-8451 devices found

1. Check USB connection
2. Install NI-845x driver
3. Restart computer
4. Check Device Manager (Windows) or `lsusb` (Linux)

### No PMBus devices found

1. Verify I2C connections (SDA, SCL, GND)
2. Check power supply is powered on
3. Try different voltage level:
   ```bash
   pmbus-cli scan --voltage 18  # Try 1.8V
   pmbus-cli scan --voltage 25  # Try 2.5V
   ```
4. Reduce clock speed:
   ```bash
   pmbus-cli scan --clock 50  # 50 kHz instead of 100 kHz
   ```

### Communication errors

```bash
# Slower clock for long cables
pmbus-cli info -a 0x58 --clock 50

# Different voltage level
pmbus-cli info -a 0x58 --voltage 18
```

## Command Reference

| Command | Description | Example |
|---------|-------------|---------|
| `list` | List NI-8451 devices | `pmbus-cli list` |
| `scan` | Scan I2C bus | `pmbus-cli scan` |
| `info` | Device information | `pmbus-cli info -a 0x58` |
| `status` | Device status | `pmbus-cli status -a 0x58` |
| `monitor` | Monitor telemetry | `pmbus-cli monitor -a 0x58` |
| `read` | Read register | `pmbus-cli read -cmd 0x8B -f linear16 -a 0x58` |
| `write` | Write register | `pmbus-cli write -cmd 0x21 --data "48 0D" -a 0x58` |
| `set-voltage` | Set output voltage | `pmbus-cli set-voltage --value 3.3 -a 0x58` |
| `operation` | Control output | `pmbus-cli operation --operation on -a 0x58` |
| `clear-faults` | Clear faults | `pmbus-cli clear-faults -a 0x58` |

## Global Options

| Option | Description | Example |
|--------|-------------|---------|
| `-a, --address` | Device address (hex) | `-a 0x58` |
| `-d, --device` | NI-8451 device name | `-d NI-845x-12345` |
| `-v, --voltage` | Bus voltage (12/15/18/25/33) | `-v 33` (3.3V) |
| `-c, --clock` | Clock rate in kHz | `-c 100` |
| `--output` | Output format (table/json/csv) | `--output json` |
| `--force` | Skip confirmations | `--force` |

## Examples

### Example 1: Quick Health Check

```bash
# Check device is responding
pmbus-cli info -a 0x58

# Check for faults
pmbus-cli status -a 0x58

# If faults exist, clear them
pmbus-cli clear-faults -a 0x58
```

### Example 2: Set Voltage and Turn On

```bash
# Set voltage
pmbus-cli set-voltage --value 3.3 -a 0x58

# Turn on
pmbus-cli operation --operation on -a 0x58

# Verify
pmbus-cli read --command 0x8B --format linear16 -a 0x58
```

### Example 3: Data Logging

```bash
# Log to CSV file
pmbus-cli monitor -a 0x58 --output csv > power_$(date +%Y%m%d).csv

# Run for 1 hour then stop
sleep 3600 && pkill pmbus-cli
```

### Example 4: Automation Script

```bash
#!/bin/bash
# Simple automation script

ADDRESS="0x58"

# Clear faults
pmbus-cli clear-faults -a $ADDRESS --force

# Set voltage
pmbus-cli set-voltage --value 3.3 -a $ADDRESS --force

# Turn on
pmbus-cli operation --operation on -a $ADDRESS --force

# Verify status
STATUS=$(pmbus-cli status -a $ADDRESS --output json | jq -r '.StatusWord')

if [ "$STATUS" = "0" ]; then
    echo "✓ Power supply OK"
else
    echo "✗ Power supply has faults"
    exit 1
fi
```

## Next Steps

1. **Read Full Documentation**: Check [README.md](README.md) for complete documentation
2. **Explore Examples**: See [EXAMPLES.md](EXAMPLES.md) for real-world scenarios
3. **Configure**: Read [CONFIGURATION.md](CONFIGURATION.md) for configuration options
4. **Test**: See [TESTING.md](TESTING.md) for testing guide
5. **Learn About Improvements**: Check [IMPROVEMENTS.md](IMPROVEMENTS.md) for new features

## Getting Help

```bash
# General help
pmbus-cli --help

# Command-specific help
pmbus-cli info --help
pmbus-cli monitor --help
pmbus-cli write --help
```

## Safety Reminder

⚠️ **Important Safety Notes:**

1. Always verify voltage settings before turning on output
2. Ensure connected devices can handle the configured voltage
3. Check status for faults before operations
4. Use `--force` flag carefully in automation
5. Keep backups of working configurations
6. Test in safe environment before production use

## Quick Reference Card

**Most Common Operations:**
```bash
# Device discovery
pmbus-cli list && pmbus-cli scan

# Device info
pmbus-cli info -a 0x58

# Monitor (live)
pmbus-cli monitor -a 0x58

# Set voltage
pmbus-cli set-voltage --value 3.3 -a 0x58

# Turn on/off
pmbus-cli operation --operation on -a 0x58
pmbus-cli operation --operation off -a 0x58

# Clear faults
pmbus-cli clear-faults -a 0x58

# Status check
pmbus-cli status -a 0x58
```

Save this for quick reference!

---

**You're ready to use PMBus Master CLI!** 🎉

For detailed information, see the full [README.md](README.md).

---

**Last Updated**: 2025-11-26
