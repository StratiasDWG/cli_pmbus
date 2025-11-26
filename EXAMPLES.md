# Usage Examples - PMBus Master CLI

This document provides practical, real-world examples of using the PMBus Master CLI.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Basic Operations](#basic-operations)
3. [Advanced Features](#advanced-features)
4. [Safety Features](#safety-features)
5. [Output Formats](#output-formats)
6. [Multi-Rail Power Supplies](#multi-rail-power-supplies)
7. [Automation & Scripting](#automation--scripting)
8. [Troubleshooting](#troubleshooting)
9. [Real-World Scenarios](#real-world-scenarios)

---

## Getting Started

### First Time Setup

```bash
# 1. List connected NI-8451 devices
$ pmbus-cli list

Found 1 device(s):
  [0] NI-845x-1A2B3C4D

# 2. Scan I2C bus for PMBus devices
$ pmbus-cli scan

Connecting to NI-8451...
Connected: NI-845x-1A2B3C4D
Voltage: 3.3V, Clock: 100kHz

Scanning I2C bus for PMBus devices (0x10-0x7F)...

Found 2 device(s):

  0x58 (88)
  0x5A (90)

Use 'pmbus-cli info -a 0x58' to read device information

# 3. Get device information
$ pmbus-cli info -a 0x58

═══════════════════════════════════════════════════
  PMBus Device Information - Address 0x58
═══════════════════════════════════════════════════

  Manufacturer ID:    Texas Instruments
  Model:              TPS546D24
  Revision:           A01
  Serial Number:      12345678
  PMBus Revision:     1.3
  Capability:         0xB0
  VOUT_MODE:          Linear (exponent: -9)

═══════════════════════════════════════════════════
```

---

## Basic Operations

### Reading Device Status

```bash
# Read status word
$ pmbus-cli status -a 0x58

═════════════════════════════════════════════════════
  PMBus Device Status - Address 0x58
═════════════════════════════════════════════════════

  STATUS_WORD:        0x0000
  Status:             OK

  STATUS_VOUT:        0x00
  STATUS_IOUT:        0x00
  STATUS_INPUT:       0x00
  STATUS_TEMP:        0x00
  STATUS_CML:         0x00

═════════════════════════════════════════════════════
```

### Reading Telemetry

```bash
# Read specific registers
$ pmbus-cli read --command 0x8B --format linear16 -a 0x58

Reading command 0x8B (READ_VOUT)...

  Raw value:      0x0D48
  Decoded:        3.300000 V

$ pmbus-cli read --command 0x8C --format linear11 -a 0x58

Reading command 0x8C (READ_IOUT)...

  Raw value:      0x1428
  Decoded:        5.000000
```

### Continuous Monitoring

```bash
# Monitor all telemetry at 1-second intervals
$ pmbus-cli monitor -a 0x58

Monitoring PMBus device at 0x58 (Press Ctrl+C to stop)

Time         VIN (V)    VOUT (V)   IIN (A)    IOUT (A)   TEMP (°C)    PIN (W)    POUT (W)
────────────────────────────────────────────────────────────────────────────────────────
10:30:00.123 12.000     3.300      0.500      5.000      45.0         6.000      16.500
10:30:01.124 12.001     3.301      0.501      5.001      45.1         6.005      16.508
10:30:02.125 12.000     3.300      0.500      5.000      45.0         6.000      16.500
^C
Monitoring stopped.

# Monitor at faster rate (every 100ms)
$ pmbus-cli monitor -a 0x58 --interval 100
```

---

## Advanced Features

### Writing Configuration

```bash
# Set output voltage (with safety confirmation)
$ pmbus-cli set-voltage --value 3.3 -a 0x58

Setting output voltage to 3.300V...
  Command sent:   3.300V
  Readback:       3.300V
  Success!

# Write to any register (requires confirmation)
$ pmbus-cli write --command 0x21 --data "48 0D" -a 0x58

═════════════════════════════════════════════════════
  Write Operation: VOUT_COMMAND (0x21)
═════════════════════════════════════════════════════
  New Value:       48 0D
═════════════════════════════════════════════════════

⚠️  WARNING: This will change the output voltage setpoint.
             Ensure connected devices can handle the new voltage.

Type 'yes' to continue or anything else to cancel: yes
  Written (word): 0x0D48
  Success!
```

### Controlling Output

```bash
# Turn output on
$ pmbus-cli operation --operation on -a 0x58

Setting operation to: ON...
  Success!

# Turn output off
$ pmbus-cli operation --operation off -a 0x58

Setting operation to: OFF...
  Success!

# Soft shutdown
$ pmbus-cli operation --operation soft-off -a 0x58

# Margining
$ pmbus-cli operation --operation margin-high -a 0x58
$ pmbus-cli operation --operation margin-low -a 0x58
```

### Clearing Faults

```bash
# Clear all faults
$ pmbus-cli clear-faults -a 0x58

Clearing all faults...
  Success!
  New status:     OK
```

---

## Safety Features

### Write Confirmation

```bash
# Dangerous writes require confirmation
$ pmbus-cli write --command 0x01 --data 80 -a 0x58

⚠️  WARNING: This will change the output state (ON/OFF).
             The power supply may turn on or off.

Type 'yes' to continue or anything else to cancel: no
Operation cancelled.

# Use --force to bypass (for scripts)
$ pmbus-cli write --command 0x01 --data 80 -a 0x58 --force
  Written (byte): 0x50
  Success!
```

### Voltage Range Validation

```bash
# Attempt to set invalid voltage
$ pmbus-cli set-voltage --value 999.9 -a 0x58

Error: Voltage 999.900V is out of valid range (0.0-100.0V)
```

### Write Protection Detection

```bash
# Attempt write on protected device
$ pmbus-cli write --command 0x21 --data "00 10" -a 0x58

Warning: Device has write protection enabled (0x80)
Write operation may fail.

Type 'yes' to attempt anyway or anything else to cancel: no
Operation cancelled.
```

---

## Output Formats

### Table Format (Default)

```bash
$ pmbus-cli info -a 0x58

  Manufacturer ID:    Texas Instruments
  Model:              TPS546D24
  Revision:           A01
```

### JSON Format

```bash
$ pmbus-cli info -a 0x58 --output json

{
  "Manufacturer": "Texas Instruments",
  "Model": "TPS546D24",
  "Revision": "A01",
  "Serial": "12345678",
  "PMBusRevision": "1.3"
}

# Use with jq for parsing
$ pmbus-cli info -a 0x58 --output json | jq '.Model'
"TPS546D24"

# Pretty print
$ pmbus-cli info -a 0x58 --output json | jq .
```

### CSV Format

```bash
$ pmbus-cli info -a 0x58 --output csv

Field,Value
"Manufacturer","Texas Instruments"
"Model","TPS546D24"
"Revision","A01"
"Serial","12345678"
"PMBusRevision","1.3"

# Monitor with CSV output for logging
$ pmbus-cli monitor -a 0x58 --output csv > power_log.csv

# View in real-time
$ tail -f power_log.csv
```

---

## Multi-Rail Power Supplies

### Reading Multiple Rails

```bash
# Read voltage from rail 0
$ pmbus-cli read --command 0x8B --page 0 --format linear16 -a 0x58
  Decoded:        3.300000 V

# Read voltage from rail 1
$ pmbus-cli read --command 0x8B --page 1 --format linear16 -a 0x58
  Decoded:        5.000000 V

# Read voltage from rail 2
$ pmbus-cli read --command 0x8B --page 2 --format linear16 -a 0x58
  Decoded:        12.000000 V
```

### Configuring Multiple Rails

```bash
# Set voltage on rail 0
$ pmbus-cli set-voltage --value 3.3 --page 0 -a 0x58

# Set voltage on rail 1
$ pmbus-cli set-voltage --value 5.0 --page 1 -a 0x58

# Control output per rail
$ pmbus-cli operation --operation on --page 0 -a 0x58
$ pmbus-cli operation --operation on --page 1 -a 0x58
```

---

## Automation & Scripting

### Bash Script Example

```bash
#!/bin/bash
# power_supply_setup.sh - Configure power supply

ADDRESS="0x58"

echo "Configuring power supply at $ADDRESS..."

# Clear any existing faults
pmbus-cli clear-faults -a $ADDRESS --force

# Set output voltage
pmbus-cli set-voltage --value 3.3 -a $ADDRESS --force

# Turn on output
pmbus-cli operation --operation on -a $ADDRESS --force

# Verify output
VOUT=$(pmbus-cli read --command 0x8B --format linear16 -a $ADDRESS --output json | jq -r '.value')

if (( $(echo "$VOUT > 3.2 && $VOUT < 3.4" | bc -l) )); then
    echo "✓ Output voltage OK: ${VOUT}V"
else
    echo "✗ Output voltage ERROR: ${VOUT}V (expected ~3.3V)"
    exit 1
fi

echo "✓ Power supply configured successfully"
```

### Python Script Example

```python
#!/usr/bin/env python3
# monitor_power.py - Monitor power supply and log to database

import subprocess
import json
import time
from datetime import datetime

ADDRESS = "0x58"
INTERVAL = 1  # seconds

def read_telemetry():
    """Read telemetry from power supply"""
    cmd = [
        "pmbus-cli", "read",
        "--command", "0x8B",  # READ_VOUT
        "--format", "linear16",
        "-a", ADDRESS,
        "--output", "json"
    ]

    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode == 0:
        data = json.loads(result.stdout)
        return data
    else:
        return None

def main():
    print(f"Monitoring power supply at {ADDRESS}")
    print(f"Press Ctrl+C to stop\n")

    try:
        while True:
            data = read_telemetry()
            if data:
                timestamp = datetime.now().isoformat()
                print(f"{timestamp} - VOUT: {data['value']}V")
                # Here you could save to database, send to monitoring, etc.

            time.sleep(INTERVAL)

    except KeyboardInterrupt:
        print("\nMonitoring stopped")

if __name__ == "__main__":
    main()
```

### PowerShell Script Example

```powershell
# Setup-PowerSupply.ps1
$Address = "0x58"

Write-Host "Configuring power supply at $Address..."

# Clear faults
& pmbus-cli clear-faults -a $Address --force

# Set voltage
& pmbus-cli set-voltage --value 3.3 -a $Address --force

# Turn on
& pmbus-cli operation --operation on -a $Address --force

# Read status
$status = & pmbus-cli status -a $Address --output json | ConvertFrom-Json

if ($status.StatusWord -eq 0) {
    Write-Host "✓ Power supply OK" -ForegroundColor Green
} else {
    Write-Host "✗ Power supply has faults: $($status.Status)" -ForegroundColor Red
    exit 1
}
```

---

## Troubleshooting

### Device Not Found

```bash
$ pmbus-cli scan

Scanning I2C bus for PMBus devices (0x10-0x7F)...

No PMBus devices found on the bus.

Troubleshooting:
  1. Check I2C connections (SDA, SCL, GND)
  2. Verify power supply is powered
  3. Check pull-up resistors on I2C lines
  4. Try different voltage levels if needed

# Try different voltage level
$ pmbus-cli scan --voltage 18  # Try 1.8V instead of 3.3V
```

### Communication Errors

```bash
# Reduce clock speed for long cables
$ pmbus-cli info -a 0x58 --clock 50

# Enable verbose mode
$ pmbus-cli info -a 0x58 --verbose

# Check specific address
$ pmbus-cli read --command 0x98 --format byte -a 0x58  # PMBUS_REVISION
```

### Fault Status

```bash
# Read detailed status
$ pmbus-cli status -a 0x58

  STATUS_WORD:        0x4410
  Status:             IOUT_FAULT, VOUT_OV_FAULT

  STATUS_VOUT:        0x80  # Overvoltage fault
  STATUS_IOUT:        0x80  # Overcurrent fault

# Clear and retry
$ pmbus-cli clear-faults -a 0x58
$ pmbus-cli operation --operation on -a 0x58
```

---

## Real-World Scenarios

### Scenario 1: Production Test

```bash
#!/bin/bash
# production_test.sh - Test power supply in production

ADDRESS="0x58"
TEST_VOLTAGES="3.3 5.0 12.0"

echo "=== Production Test Started ==="

# 1. Device identification
MODEL=$(pmbus-cli info -a $ADDRESS --output json | jq -r '.Model')
echo "Testing: $MODEL"

# 2. Clear faults
pmbus-cli clear-faults -a $ADDRESS --force

# 3. Test each voltage
for VOLTAGE in $TEST_VOLTAGES; do
    echo "Testing ${VOLTAGE}V output..."

    # Set voltage
    pmbus-cli set-voltage --value $VOLTAGE -a $ADDRESS --force
    sleep 0.5

    # Read back
    VOUT=$(pmbus-cli read --command 0x8B --format linear16 -a $ADDRESS --output json | jq -r '.value')

    # Check tolerance (±1%)
    TOLERANCE=$(echo "$VOLTAGE * 0.01" | bc -l)
    LOW=$(echo "$VOLTAGE - $TOLERANCE" | bc -l)
    HIGH=$(echo "$VOLTAGE + $TOLERANCE" | bc -l)

    if (( $(echo "$VOUT >= $LOW && $VOUT <= $HIGH" | bc -l) )); then
        echo "  ✓ PASS: ${VOUT}V (${VOLTAGE}V ±1%)"
    else
        echo "  ✗ FAIL: ${VOUT}V (expected ${VOLTAGE}V ±1%)"
        exit 1
    fi
done

echo "=== Production Test PASSED ==="
```

### Scenario 2: Continuous Data Logging

```bash
# Start continuous logging to CSV
$ pmbus-cli monitor -a 0x58 --output csv --interval 1000 > power_log_$(date +%Y%m%d_%H%M%S).csv &

# Let it run for 1 hour
sleep 3600

# Stop monitoring
pkill -f "pmbus-cli monitor"

# Analyze data
$ python3 analyze_power_log.py power_log_*.csv
```

### Scenario 3: Remote Monitoring

```bash
#!/bin/bash
# remote_monitor.sh - Send telemetry to remote server

while true; do
    # Read telemetry
    DATA=$(pmbus-cli monitor -a 0x58 --output json --interval 1000 --samples 1)

    # Send to remote server
    curl -X POST https://monitoring.example.com/api/telemetry \
         -H "Content-Type: application/json" \
         -d "$DATA"

    sleep 60  # Every minute
done
```

### Scenario 4: Fault Detection and Alert

```bash
#!/bin/bash
# fault_monitor.sh - Monitor for faults and send alerts

ADDRESS="0x58"

while true; do
    STATUS=$(pmbus-cli status -a $ADDRESS --output json | jq -r '.StatusWord')

    if [ "$STATUS" != "0" ]; then
        # Fault detected
        DETAILS=$(pmbus-cli status -a $ADDRESS --output json)

        # Send alert email
        echo "$DETAILS" | mail -s "Power Supply Fault Detected" admin@example.com

        # Log to syslog
        logger -t pmbus-monitor "Power supply fault: $DETAILS"

        # Clear faults
        pmbus-cli clear-faults -a $ADDRESS --force
    fi

    sleep 5
done
```

### Scenario 5: Automated Voltage Sweep

```bash
#!/bin/bash
# voltage_sweep.sh - Test device under various voltages

ADDRESS="0x58"
START_V=3.0
END_V=3.6
STEP=0.1

echo "Voltage,Current,Power,Efficiency"

VOLTAGE=$START_V
while (( $(echo "$VOLTAGE <= $END_V" | bc -l) )); do
    # Set voltage
    pmbus-cli set-voltage --value $VOLTAGE -a $ADDRESS --force
    sleep 0.5

    # Read telemetry
    VIN=$(pmbus-cli read --command 0x88 --format linear11 -a $ADDRESS --output json | jq -r '.value')
    VOUT=$(pmbus-cli read --command 0x8B --format linear16 -a $ADDRESS --output json | jq -r '.value')
    IOUT=$(pmbus-cli read --command 0x8C --format linear11 -a $ADDRESS --output json | jq -r '.value')
    PIN=$(pmbus-cli read --command 0x97 --format linear11 -a $ADDRESS --output json | jq -r '.value')
    POUT=$(pmbus-cli read --command 0x96 --format linear11 -a $ADDRESS --output json | jq -r '.value')

    EFF=$(echo "scale=2; $POUT / $PIN * 100" | bc -l)

    echo "$VOUT,$IOUT,$POUT,$EFF"

    VOLTAGE=$(echo "$VOLTAGE + $STEP" | bc -l)
done
```

---

## Tips and Best Practices

### 1. Use Configuration File

```bash
# Create config once
cat > ~/.pmbus-cli/config.json <<EOF
{
  "DefaultAddress": 88,
  "DefaultClockRate": 100,
  "RequireWriteConfirmation": true,
  "DefaultOutputFormat": "Table"
}
EOF

# Now commands are simpler
pmbus-cli info  # Uses DefaultAddress
```

### 2. Alias for Common Operations

```bash
# Add to ~/.bashrc
alias ps-info='pmbus-cli info -a 0x58'
alias ps-status='pmbus-cli status -a 0x58'
alias ps-monitor='pmbus-cli monitor -a 0x58 --interval 1000'

# Usage
$ ps-info
$ ps-status
$ ps-monitor
```

### 3. Safety First

```bash
# Always check status before writing
pmbus-cli status -a 0x58

# Clear faults before operations
pmbus-cli clear-faults -a 0x58

# Verify writes
pmbus-cli set-voltage --value 3.3 -a 0x58
pmbus-cli read --command 0x8B --format linear16 -a 0x58
```

### 4. Logging and Documentation

```bash
# Log all operations
pmbus-cli info -a 0x58 | tee -a operations.log

# Document configuration changes
echo "$(date): Set VOUT to 3.3V" >> changes.log
pmbus-cli set-voltage --value 3.3 -a 0x58 --force
```

---

## See Also

- [README.md](README.md) - Full documentation
- [CONFIGURATION.md](CONFIGURATION.md) - Configuration guide
- [TESTING.md](TESTING.md) - Testing guide
- [IMPROVEMENTS.md](IMPROVEMENTS.md) - Feature documentation

---

**Last Updated**: 2025-11-26
