# PMBus Master CLI

Professional command-line tool for communicating with PMBus-compliant power supplies using the NI-8451 USB-to-I2C/SPI Interface.

## Features

- **Full PMBus Protocol Support**: Implements complete PMBus specification with all standard commands
- **Linear Data Format**: Automatic conversion between LINEAR11 and LINEAR16 formats
- **Device Discovery**: Automatic scanning for NI-8451 devices and PMBus devices on I2C bus
- **Real-time Monitoring**: Continuous telemetry monitoring with customizable intervals
- **Professional CLI**: Clean command-line interface with comprehensive help system
- **Comprehensive Read/Write**: Support for byte, word, and block read/write operations
- **Status Decoding**: Human-readable status word decoding
- **Device Information**: Read manufacturer ID, model, revision, serial number, and capabilities

## Hardware Requirements

- **NI-8451**: USB-to-I2C/SPI Interface Device
- **NI-845x Driver**: Required driver software from National Instruments
- **PMBus Device**: Any PMBus-compliant power supply

### NI-8451 Connections

```
NI-8451 Pin    │ PMBus Device
───────────────┼─────────────────
SCL            │ SCL (I2C Clock)
SDA            │ SDA (I2C Data)
GND            │ GND (Ground)
VCC (optional) │ Pull-up voltage
```

## Installation

### Prerequisites

1. **Install .NET 6.0 SDK or later**
   ```bash
   # Download from: https://dotnet.microsoft.com/download
   ```

2. **Install NI-845x Driver**
   - Download from: https://www.ni.com/en-us/support/downloads/drivers/download.ni-845x.html
   - Install the driver and restart your computer

### Build from Source

```bash
# Clone the repository
git clone <repository-url>
cd cli_pmbus

# Restore dependencies
dotnet restore

# Build the project
dotnet build -c Release

# Run the CLI
dotnet run -- <command>

# Or build and publish as standalone executable
dotnet publish -c Release -o ./publish
./publish/pmbus-cli <command>
```

## Quick Start

### 1. List Connected NI-8451 Devices

```bash
pmbus-cli list
```

### 2. Scan I2C Bus for PMBus Devices

```bash
pmbus-cli scan
```

### 3. Read Device Information

```bash
pmbus-cli info --address 0x58
```

### 4. Monitor Device Telemetry

```bash
pmbus-cli monitor --address 0x58 --interval 1000
```

## Command Reference

### Global Options

```
-d, --device <name>      NI-8451 device name (auto-detect if not specified)
-a, --address <addr>     PMBus I2C address in hex (default: 0x58)
-v, --voltage <level>    I2C bus voltage: 12, 15, 18, 25, 33 (default: 33 = 3.3V)
-c, --clock <rate>       I2C clock rate in kHz (default: 100)
```

### Commands

#### `list`
List all connected NI-8451 devices.

```bash
pmbus-cli list
```

#### `scan`
Scan I2C bus for PMBus devices.

```bash
pmbus-cli scan [options]

Options:
  -d, --device <name>    Device name
  -v, --voltage <level>  Bus voltage level
  -c, --clock <rate>     Clock rate in kHz
```

#### `info`
Read device identification and capability information.

```bash
pmbus-cli info [options]

Options:
  -a, --address <addr>   Device address (required)
  -d, --device <name>    NI-8451 device
  -v, --voltage <level>  Bus voltage level
  -c, --clock <rate>     Clock rate

Example:
  pmbus-cli info -a 0x58
```

#### `status`
Read and decode device status registers.

```bash
pmbus-cli status [options]

Options:
  -a, --address <addr>   Device address (required)

Example:
  pmbus-cli status -a 0x58
```

#### `monitor`
Continuously monitor device telemetry (VIN, VOUT, IIN, IOUT, Temperature, Power).

```bash
pmbus-cli monitor [options]

Options:
  -a, --address <addr>      Device address (required)
  -i, --interval <ms>       Update interval in milliseconds (default: 1000)

Example:
  pmbus-cli monitor -a 0x58 -i 500

Press Ctrl+C to stop monitoring.
```

#### `read`
Read data from a PMBus register.

```bash
pmbus-cli read --command <code> [options]

Options:
  -cmd, --command <code>    Command code in hex (e.g., 0x8B) or decimal
  -b, --bytes <count>       Number of bytes to read (default: 2)
  -f, --format <format>     Output format: hex, linear11, linear16, byte, word, block
  -a, --address <addr>      Device address

Examples:
  # Read output voltage (LINEAR16 format)
  pmbus-cli read -cmd 0x8B -f linear16 -a 0x58

  # Read input voltage (LINEAR11 format)
  pmbus-cli read -cmd 0x88 -f linear11 -a 0x58

  # Read status word
  pmbus-cli read -cmd 0x79 -f word -a 0x58

  # Read manufacturer ID (block read)
  pmbus-cli read -cmd 0x99 -f block -a 0x58
```

#### `write`
Write data to a PMBus register.

```bash
pmbus-cli write --command <code> --data <value> [options]

Options:
  -cmd, --command <code>    Command code in hex or decimal
  --data <value>            Data to write (hex: "0A FF" or decimal with --decimal)
  --decimal                 Interpret data as decimal value
  -a, --address <addr>      Device address

Examples:
  # Write byte value
  pmbus-cli write -cmd 0x01 --data 80 --decimal -a 0x58

  # Write word value in hex
  pmbus-cli write -cmd 0x21 --data "00 10" -a 0x58

  # Write block data
  pmbus-cli write -cmd 0x10 --data "40 55" -a 0x58
```

#### `set-voltage`
Set output voltage (VOUT_COMMAND).

```bash
pmbus-cli set-voltage --value <volts> [options]

Options:
  --value <volts>       Voltage in volts (e.g., 3.3, 5.0, 12.0)
  -a, --address <addr>  Device address

Example:
  pmbus-cli set-voltage --value 3.3 -a 0x58
```

#### `operation`
Control output operation state.

```bash
pmbus-cli operation --operation <op> [options]

Operations:
  on              Turn output on (0x80)
  off             Immediate shutdown (0x00)
  soft-off        Soft shutdown (0x40)
  margin-high     Enable high margining (0xA8)
  margin-low      Enable low margining (0x98)

Example:
  pmbus-cli operation --operation on -a 0x58
  pmbus-cli operation --operation off -a 0x58
```

#### `clear-faults`
Clear all fault conditions.

```bash
pmbus-cli clear-faults [options]

Options:
  -a, --address <addr>  Device address

Example:
  pmbus-cli clear-faults -a 0x58
```

## PMBus Command Codes

### Common Read Commands

| Command | Code | Format | Description |
|---------|------|--------|-------------|
| READ_VIN | 0x88 | LINEAR11 | Input voltage |
| READ_IIN | 0x89 | LINEAR11 | Input current |
| READ_VOUT | 0x8B | LINEAR16 | Output voltage |
| READ_IOUT | 0x8C | LINEAR11 | Output current |
| READ_TEMPERATURE_1 | 0x8D | LINEAR11 | Temperature sensor 1 |
| READ_POUT | 0x96 | LINEAR11 | Output power |
| READ_PIN | 0x97 | LINEAR11 | Input power |
| STATUS_WORD | 0x79 | Word | Status word |
| STATUS_BYTE | 0x78 | Byte | Status byte |
| MFR_ID | 0x99 | Block | Manufacturer ID |
| MFR_MODEL | 0x9A | Block | Model number |
| MFR_SERIAL | 0x9E | Block | Serial number |

### Common Write Commands

| Command | Code | Format | Description |
|---------|------|--------|-------------|
| OPERATION | 0x01 | Byte | Output on/off control |
| VOUT_COMMAND | 0x21 | LINEAR16 | Output voltage setpoint |
| CLEAR_FAULTS | 0x03 | Send Byte | Clear all faults |
| PAGE | 0x00 | Byte | Select page/channel |

## Data Formats

### LINEAR11 Format
- 5-bit two's complement exponent
- 11-bit two's complement mantissa
- Formula: `Value = Mantissa × 2^Exponent`
- Used for: Current, power, temperature readings

### LINEAR16 Format
- 16-bit two's complement mantissa
- Exponent from VOUT_MODE register (lower 5 bits)
- Formula: `Value = Mantissa × 2^Exponent`
- Used for: Output voltage readings and commands

## Examples

### Example 1: Complete Device Interrogation

```bash
# List NI-8451 devices
pmbus-cli list

# Scan for PMBus devices
pmbus-cli scan

# Read device info
pmbus-cli info -a 0x58

# Read status
pmbus-cli status -a 0x58
```

### Example 2: Monitor Power Supply

```bash
# Monitor all telemetry at 500ms intervals
pmbus-cli monitor -a 0x58 -i 500
```

### Example 3: Set Output Voltage

```bash
# Read current voltage
pmbus-cli read -cmd 0x8B -f linear16 -a 0x58

# Set new voltage to 3.3V
pmbus-cli set-voltage --value 3.3 -a 0x58

# Verify
pmbus-cli read -cmd 0x8B -f linear16 -a 0x58
```

### Example 4: Control Output Operation

```bash
# Turn on output
pmbus-cli operation --operation on -a 0x58

# Check status
pmbus-cli status -a 0x58

# Turn off output
pmbus-cli operation --operation off -a 0x58
```

### Example 5: Fault Recovery

```bash
# Read status
pmbus-cli status -a 0x58

# Clear faults
pmbus-cli clear-faults -a 0x58

# Verify faults cleared
pmbus-cli status -a 0x58
```

## Troubleshooting

### No NI-8451 Devices Found

1. Check USB connection
2. Verify NI-845x driver is installed
3. Check Windows Device Manager for "NI USB-845x" device
4. Try different USB port
5. Restart computer after driver installation

### No PMBus Devices Found on Scan

1. Verify I2C connections (SDA, SCL, GND)
2. Check power supply is powered on
3. Verify pull-up resistors on I2C lines (typically 2.2kΩ to 10kΩ)
4. Try different voltage levels: `--voltage 18` or `--voltage 25`
5. Check PMBus device address in datasheet
6. Use oscilloscope to verify I2C signals

### Communication Errors

1. Reduce clock speed: `--clock 50`
2. Check for address conflicts on I2C bus
3. Verify ground connection between NI-8451 and device
4. Check cable length (keep I2C cables short, < 30cm recommended)
5. Add external pull-up resistors if needed

### Read/Write Failures

1. Verify command code is supported by device (check datasheet)
2. Some commands require specific pages to be selected first
3. Check write protection status (WRITE_PROTECT command)
4. Verify device is not in fault state
5. Clear faults before attempting operations

## Project Structure

```
cli_pmbus/
├── PmbusMasterCLI.csproj    # Project configuration
├── Program.cs               # CLI entry point and command handlers
├── PMBusProtocol.cs         # PMBus protocol implementation
├── PMBusCommands.cs         # PMBus command definitions
├── NI8451Interface.cs       # NI-8451 hardware interface wrapper
├── LinearDataFormat.cs      # LINEAR11/LINEAR16 format converters
└── README.md               # This file
```

## Development

### Adding New Commands

1. Add command code to `PMBusCommands.cs`
2. Implement method in `PMBusProtocol.cs`
3. Add CLI command handler in `Program.cs`

### Testing

```bash
# Build in debug mode
dotnet build

# Run with verbose output
dotnet run -- info -a 0x58
```

## References

- [PMBus Specification Part I - General Requirements](https://pmbus.org/Assets/PDFS/Public/PMBus_Specification_Part_I_Rev_1-3-1_20150206.pdf)
- [PMBus Specification Part II - Command Language](https://pmbus.org/Assets/PDFS/Public/PMBus_Specification_Part_II_Rev_1-3-1_20150206.pdf)
- [NI-845x Hardware Manual](https://www.ni.com/pdf/manuals/372751c.pdf)
- [NI-845x Software Reference Manual](https://www.ni.com/pdf/manuals/372753c.pdf)

## License

This project is provided as-is for educational and professional use.

## Support

For issues, questions, or contributions, please refer to the project repository.

---

**Note**: Always refer to your power supply's datasheet for device-specific commands, addresses, and operational limits. Incorrect commands can damage equipment or cause safety hazards.
