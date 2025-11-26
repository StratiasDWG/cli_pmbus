# Configuration Guide - PMBus Master CLI

The PMBus Master CLI supports persistent configuration through a JSON configuration file. This allows you to set defaults and preferences that apply to all commands.

## Configuration File Location

**Default Location:**
```
~/.pmbus-cli/config.json
```

**Platform-Specific Paths:**
- **Linux/macOS**: `/home/username/.pmbus-cli/config.json`
- **Windows**: `C:\Users\username\.pmbus-cli\config.json`

## Quick Start

### View Current Configuration

```bash
pmbus-cli config --show
```

### Create Configuration from Template

```bash
# Copy sample configuration
cp config.sample.json ~/.pmbus-cli/config.json

# Edit with your preferred editor
nano ~/.pmbus-cli/config.json
```

### Reset to Defaults

```bash
pmbus-cli config --reset
```

## Configuration Options

### Hardware Defaults

#### DefaultAddress
```json
"DefaultAddress": 88
```
- **Type**: Integer (0-127) or Hex (0x00-0x7F)
- **Default**: 88 (0x58)
- **Description**: Default PMBus device I2C address
- **Example**: Common PMBus addresses: 0x10-0x7F

```bash
# Usage without config
pmbus-cli info --address 0x58

# Usage with DefaultAddress=0x58
pmbus-cli info  # Uses 0x58 automatically
```

#### DefaultVoltageLevel
```json
"DefaultVoltageLevel": 33
```
- **Type**: Integer (12, 15, 18, 25, or 33)
- **Default**: 33 (3.3V)
- **Description**: I2C bus voltage level
- **Options**:
  - `12` = 1.2V
  - `15` = 1.5V
  - `18` = 1.8V
  - `25` = 2.5V
  - `33` = 3.3V

**Important**: Must match your hardware's logic level!

#### DefaultClockRate
```json
"DefaultClockRate": 100
```
- **Type**: Integer (10-400)
- **Default**: 100 kHz
- **Description**: I2C clock rate in kHz
- **Typical Values**:
  - `100` = Standard mode
  - `400` = Fast mode
  - `50` = Slow mode (for long cables or debugging)

### Robustness Settings

#### MaxRetries
```json
"MaxRetries": 3
```
- **Type**: Integer (0-10)
- **Default**: 3
- **Description**: Maximum retry attempts for failed I2C operations
- **Recommended**: 3-5 for production, 0 for debugging

#### RetryDelayMs
```json
"RetryDelayMs": 10
```
- **Type**: Integer (0-1000)
- **Default**: 10 milliseconds
- **Description**: Base delay for exponential backoff
- **Calculation**: Delay = RetryDelayMs × attempt_number
  - Attempt 1: 10ms
  - Attempt 2: 20ms
  - Attempt 3: 30ms

#### OperationTimeoutMs
```json
"OperationTimeoutMs": 5000
```
- **Type**: Integer (100-30000)
- **Default**: 5000 milliseconds (5 seconds)
- **Description**: Maximum time to wait for any single operation
- **Recommended**: 5000-10000 for normal use

### Safety Settings

#### RequireWriteConfirmation
```json
"RequireWriteConfirmation": true
```
- **Type**: Boolean
- **Default**: `true`
- **Description**: Require user to type "yes" for dangerous writes
- **Recommended**: `true` for interactive use, `false` for automation

**Example with confirmation enabled:**
```bash
$ pmbus-cli write --command 0x01 --data 80 -a 0x58

⚠️  WARNING: This will change the output state (ON/OFF).
             The power supply may turn on or off.

Type 'yes' to continue or anything else to cancel: yes
Write successful!
```

**Bypass with --force flag:**
```bash
pmbus-cli write --command 0x01 --data 80 -a 0x58 --force
```

#### VerifyWrites
```json
"VerifyWrites": true
```
- **Type**: Boolean
- **Default**: `true`
- **Description**: Read back value after write to verify
- **Recommended**: `true` for critical operations

#### CheckWriteProtection
```json
"CheckWriteProtection": true
```
- **Type**: Boolean
- **Default**: `true`
- **Description**: Check WRITE_PROTECT register before writing
- **Recommended**: `true` to prevent write failures

### User Interface Settings

#### EnableColorOutput
```json
"EnableColorOutput": true
```
- **Type**: Boolean
- **Default**: `true`
- **Description**: Use color-coded output in terminal
- **Colors**:
  - 🟢 Green: Success, OK status
  - 🔴 Red: Errors, faults
  - 🟡 Yellow: Warnings

**Disable if:**
- Piping output to files
- Using terminals without color support
- Generating logs

#### DefaultOutputFormat
```json
"DefaultOutputFormat": "Table"
```
- **Type**: String
- **Default**: "Table"
- **Options**:
  - `"Table"` = Human-readable formatted tables
  - `"Json"` = JSON format for automation
  - `"Csv"` = CSV format for spreadsheets

**Examples:**

**Table (default):**
```bash
$ pmbus-cli info -a 0x58

  Manufacturer ID:    Texas Instruments
  Model:              TPS546D24
  Revision:           1.0
```

**JSON:**
```bash
$ pmbus-cli info -a 0x58 --output json
{
  "Manufacturer": "Texas Instruments",
  "Model": "TPS546D24",
  "Revision": "1.0"
}
```

**CSV:**
```bash
$ pmbus-cli info -a 0x58 --output csv
Field,Value
"Manufacturer","Texas Instruments"
"Model","TPS546D24"
```

#### VerboseMode
```json
"VerboseMode": false
```
- **Type**: Boolean
- **Default**: `false`
- **Description**: Show detailed debug information
- **Use**: Enable for troubleshooting

### Performance Settings

#### DefaultMonitorInterval
```json
"DefaultMonitorInterval": 1000
```
- **Type**: Integer (100-60000)
- **Default**: 1000 milliseconds (1 second)
- **Description**: Default interval for monitor command
- **Typical Values**:
  - `100` = 10 samples/second (fast)
  - `1000` = 1 sample/second (normal)
  - `5000` = 1 sample/5 seconds (slow)

#### EnableDeviceCache
```json
"EnableDeviceCache": true
```
- **Type**: Boolean
- **Default**: `true`
- **Description**: Cache device metadata (VOUT_MODE, etc.)
- **Benefit**: Reduces I2C reads, faster operations

#### CacheExpirationHours
```json
"CacheExpirationHours": 24
```
- **Type**: Integer (1-168)
- **Default**: 24 hours
- **Description**: How long to keep cached device data
- **Recommended**: 24-72 hours

## Configuration Scenarios

### Scenario 1: Production Environment

**Requirements:**
- Maximum safety
- Verification enabled
- Standard speed

```json
{
  "RequireWriteConfirmation": true,
  "VerifyWrites": true,
  "CheckWriteProtection": true,
  "MaxRetries": 5,
  "DefaultClockRate": 100,
  "VerboseMode": false,
  "EnableColorOutput": true
}
```

### Scenario 2: Automated Testing

**Requirements:**
- No confirmations
- Fast operation
- JSON output

```json
{
  "RequireWriteConfirmation": false,
  "VerifyWrites": true,
  "CheckWriteProtection": false,
  "MaxRetries": 3,
  "DefaultClockRate": 400,
  "DefaultOutputFormat": "Json",
  "EnableColorOutput": false,
  "VerboseMode": false
}
```

**Note**: Use `--force` flag for writes in scripts.

### Scenario 3: Debugging

**Requirements:**
- Maximum visibility
- Slower operation
- Verbose output

```json
{
  "RequireWriteConfirmation": true,
  "VerifyWrites": true,
  "MaxRetries": 0,
  "DefaultClockRate": 50,
  "VerboseMode": true,
  "EnableColorOutput": true,
  "OperationTimeoutMs": 10000
}
```

### Scenario 4: Data Logging

**Requirements:**
- CSV output
- Fast sampling
- No colors (for clean logs)

```json
{
  "DefaultMonitorInterval": 100,
  "DefaultOutputFormat": "Csv",
  "EnableColorOutput": false,
  "VerboseMode": false
}
```

**Usage:**
```bash
pmbus-cli monitor -a 0x58 > power_log.csv
```

## Configuration Validation

The application validates configuration on startup:

```bash
$ pmbus-cli info -a 0x58

Warning: Invalid configuration detected:
  - DefaultClockRate must be between 10 and 400 kHz
  - MaxRetries must be between 0 and 10

Using default values for invalid settings.
```

### Common Validation Errors

| Error | Fix |
|-------|-----|
| `DefaultAddress must be 7-bit` | Use 0x00-0x7F (0-127) |
| `DefaultClockRate out of range` | Use 10-400 kHz |
| `OperationTimeoutMs too small` | Use at least 100ms |
| `Invalid DefaultOutputFormat` | Use "Table", "Json", or "Csv" |

## Environment-Specific Configuration

### Multiple Projects

Keep separate configs for different projects:

```bash
# Project A
cp ~/.pmbus-cli/config.json ~/project_a/pmbus-config.json

# Project B
cp ~/.pmbus-cli/config.json ~/project_b/pmbus-config.json

# Use with --config flag
pmbus-cli --config ~/project_a/pmbus-config.json info -a 0x58
```

### Team Shared Configuration

Share configuration in version control:

```bash
# In your project repository
my-project/
├── .pmbus-cli-config.json  # Committed to git
└── ...

# Team members use
pmbus-cli --config .pmbus-cli-config.json info -a 0x58
```

**Example shared config:**
```json
{
  "DefaultAddress": 88,
  "DefaultClockRate": 100,
  "RequireWriteConfirmation": true,
  "DefaultOutputFormat": "Table",
  "MaxRetries": 3
}
```

## Command-Line Override

Configuration can be overridden by command-line arguments:

```bash
# Config has DefaultAddress=0x58
# Override with -a flag
pmbus-cli info -a 0x5A  # Uses 0x5A, not 0x58

# Config has DefaultOutputFormat="Table"
# Override with --output flag
pmbus-cli info -a 0x58 --output json  # Uses JSON
```

**Priority (highest to lowest):**
1. Command-line arguments
2. Configuration file
3. Built-in defaults

## Troubleshooting

### Configuration not loading

```bash
# Check if file exists
ls -la ~/.pmbus-cli/config.json

# Check file permissions
chmod 644 ~/.pmbus-cli/config.json

# Validate JSON syntax
cat ~/.pmbus-cli/config.json | json_pp
```

### Invalid JSON

```bash
# Use online validator
cat ~/.pmbus-cli/config.json | curl -X POST -d @- https://jsonlint.com/

# Or use jq
cat ~/.pmbus-cli/config.json | jq .
```

### Reset corrupted configuration

```bash
# Remove current config
rm ~/.pmbus-cli/config.json

# Let application create default
pmbus-cli config --reset

# Or copy sample
cp config.sample.json ~/.pmbus-cli/config.json
```

## Best Practices

1. **Start with defaults**: Use sample config as template
2. **Version control**: Keep project-specific configs in git
3. **Document changes**: Comment why you changed defaults
4. **Test changes**: Verify config with `--show` before using
5. **Backup**: Keep backup of working configuration
6. **Validate**: Check for errors after editing
7. **Security**: Don't commit sensitive data to git

## Examples

### Example 1: Multi-Device Setup

```json
{
  "Comment": "Lab setup with 3 power supplies",
  "DefaultAddress": 88,
  "DefaultClockRate": 100,
  "EnableDeviceCache": true,
  "RequireWriteConfirmation": true
}
```

```bash
# Device 1 (uses default 0x58)
pmbus-cli info

# Device 2
pmbus-cli info -a 0x5A

# Device 3
pmbus-cli info -a 0x5C
```

### Example 2: Continuous Monitoring

```json
{
  "DefaultMonitorInterval": 500,
  "DefaultOutputFormat": "Csv",
  "EnableColorOutput": false,
  "MaxRetries": 5
}
```

```bash
# Monitor with configured defaults
pmbus-cli monitor -a 0x58 >> power_data.csv
```

### Example 3: Safe Production Use

```json
{
  "RequireWriteConfirmation": true,
  "VerifyWrites": true,
  "CheckWriteProtection": true,
  "MaxRetries": 5,
  "VerboseMode": false,
  "EnableColorOutput": true
}
```

## See Also

- [TESTING.md](TESTING.md) - Testing guide
- [README.md](README.md) - User guide
- [IMPROVEMENTS.md](IMPROVEMENTS.md) - Feature documentation

---

**Last Updated**: 2025-11-26
