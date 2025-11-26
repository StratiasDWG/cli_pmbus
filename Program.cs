using System.CommandLine;
using PmbusMasterCLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("PMBus Master CLI - Professional tool for communicating with PMBus power supplies via NI-8451");

        // Global options
        var deviceOption = new Option<string?>(
            aliases: new[] { "--device", "-d" },
            description: "NI-8451 device name (auto-detect if not specified)");

        var addressOption = new Option<byte>(
            aliases: new[] { "--address", "-a" },
            getDefaultValue: () => 0x58,
            description: "PMBus device I2C address (7-bit, default: 0x58)");

        var voltageOption = new Option<byte>(
            aliases: new[] { "--voltage", "-v" },
            getDefaultValue: () => NI8451Interface.kNi845x33Volts,
            description: "I2C bus voltage level (12, 15, 18, 25, 33 for 1.2V-3.3V, default: 33)");

        var clockOption = new Option<ushort>(
            aliases: new[] { "--clock", "-c" },
            getDefaultValue: () => 100,
            description: "I2C clock rate in kHz (default: 100)");

        // List devices command
        var listCmd = new Command("list", "List all connected NI-8451 devices")
        {
            Handler = CommandHandler.Create(ListDevices)
        };

        // Scan command
        var scanCmd = new Command("scan", "Scan I2C bus for PMBus devices")
        {
            deviceOption,
            voltageOption,
            clockOption
        };
        scanCmd.SetHandler(ScanBus, deviceOption, voltageOption, clockOption);

        // Info command
        var infoCmd = new Command("info", "Read device information")
        {
            deviceOption,
            addressOption,
            voltageOption,
            clockOption
        };
        infoCmd.SetHandler(ReadDeviceInfo, deviceOption, addressOption, voltageOption, clockOption);

        // Status command
        var statusCmd = new Command("status", "Read device status")
        {
            deviceOption,
            addressOption,
            voltageOption,
            clockOption
        };
        statusCmd.SetHandler(ReadStatus, deviceOption, addressOption, voltageOption, clockOption);

        // Monitor command
        var intervalOption = new Option<int>(
            aliases: new[] { "--interval", "-i" },
            getDefaultValue: () => 1000,
            description: "Monitoring interval in milliseconds (default: 1000)");

        var monitorCmd = new Command("monitor", "Continuously monitor device telemetry")
        {
            deviceOption,
            addressOption,
            voltageOption,
            clockOption,
            intervalOption
        };
        monitorCmd.SetHandler(MonitorDevice, deviceOption, addressOption, voltageOption, clockOption, intervalOption);

        // Read command
        var commandCodeOption = new Option<string>(
            aliases: new[] { "--command", "-cmd" },
            description: "PMBus command code (hex, e.g., 0x8B or decimal)")
        { IsRequired = true };

        var bytesOption = new Option<int>(
            aliases: new[] { "--bytes", "-b" },
            getDefaultValue: () => 2,
            description: "Number of bytes to read (default: 2)");

        var formatOption = new Option<string>(
            aliases: new[] { "--format", "-f" },
            getDefaultValue: () => "hex",
            description: "Output format: hex, linear11, linear16, byte, word, block");

        var readCmd = new Command("read", "Read from a PMBus register")
        {
            deviceOption,
            addressOption,
            voltageOption,
            clockOption,
            commandCodeOption,
            bytesOption,
            formatOption
        };
        readCmd.SetHandler(ReadRegister, deviceOption, addressOption, voltageOption, clockOption,
                          commandCodeOption, bytesOption, formatOption);

        // Write command
        var dataOption = new Option<string>(
            aliases: new[] { "--data" },
            description: "Data to write (hex format, e.g., '0A FF' or decimal with --decimal)")
        { IsRequired = true };

        var decimalOption = new Option<bool>(
            aliases: new[] { "--decimal" },
            getDefaultValue: () => false,
            description: "Interpret data as decimal value");

        var writeCmd = new Command("write", "Write to a PMBus register")
        {
            deviceOption,
            addressOption,
            voltageOption,
            clockOption,
            commandCodeOption,
            dataOption,
            decimalOption
        };
        writeCmd.SetHandler(WriteRegister, deviceOption, addressOption, voltageOption, clockOption,
                           commandCodeOption, dataOption, decimalOption);

        // Set voltage command
        var voltageValueOption = new Option<double>(
            aliases: new[] { "--value" },
            description: "Voltage value in volts")
        { IsRequired = true };

        var setVoltageCmd = new Command("set-voltage", "Set output voltage (VOUT_COMMAND)")
        {
            deviceOption,
            addressOption,
            voltageOption,
            clockOption,
            voltageValueOption
        };
        setVoltageCmd.SetHandler(SetVoltage, deviceOption, addressOption, voltageOption, clockOption, voltageValueOption);

        // Operation command
        var operationOption = new Option<string>(
            aliases: new[] { "--operation", "-op" },
            description: "Operation: on, off, soft-off, margin-high, margin-low")
        { IsRequired = true };

        var operationCmd = new Command("operation", "Control output operation")
        {
            deviceOption,
            addressOption,
            voltageOption,
            clockOption,
            operationOption
        };
        operationCmd.SetHandler(SetOperation, deviceOption, addressOption, voltageOption, clockOption, operationOption);

        // Clear faults command
        var clearFaultsCmd = new Command("clear-faults", "Clear all fault conditions")
        {
            deviceOption,
            addressOption,
            voltageOption,
            clockOption
        };
        clearFaultsCmd.SetHandler(ClearFaults, deviceOption, addressOption, voltageOption, clockOption);

        // Add all commands to root
        rootCommand.AddCommand(listCmd);
        rootCommand.AddCommand(scanCmd);
        rootCommand.AddCommand(infoCmd);
        rootCommand.AddCommand(statusCmd);
        rootCommand.AddCommand(monitorCmd);
        rootCommand.AddCommand(readCmd);
        rootCommand.AddCommand(writeCmd);
        rootCommand.AddCommand(setVoltageCmd);
        rootCommand.AddCommand(operationCmd);
        rootCommand.AddCommand(clearFaultsCmd);

        return await rootCommand.InvokeAsync(args);
    }

    static void ListDevices()
    {
        Console.WriteLine("Scanning for NI-8451 devices...\n");

        try
        {
            var devices = NI8451Interface.FindDevices();

            if (devices.Count == 0)
            {
                Console.WriteLine("No NI-8451 devices found.");
                Console.WriteLine("\nTroubleshooting:");
                Console.WriteLine("  1. Ensure NI-8451 is connected via USB");
                Console.WriteLine("  2. Install NI-845x driver from ni.com");
                Console.WriteLine("  3. Check Device Manager (Windows) for NI USB-845x device");
                return;
            }

            Console.WriteLine($"Found {devices.Count} device(s):");
            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine($"  [{i}] {devices[i]}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("\nEnsure NI-845x driver is installed.");
        }
    }

    static void ScanBus(string? device, byte voltage, ushort clock)
    {
        using var ni8451 = new NI8451Interface();

        try
        {
            Console.WriteLine("Connecting to NI-8451...");
            ni8451.Connect(device, voltage, clock);
            Console.WriteLine($"Connected: {ni8451.DeviceName}");
            Console.WriteLine($"Voltage: {voltage / 10.0:F1}V, Clock: {clock}kHz\n");

            Console.WriteLine("Scanning I2C bus for PMBus devices (0x10-0x7F)...\n");

            var foundDevices = PMBusProtocol.ScanBus(ni8451);

            if (foundDevices.Count == 0)
            {
                Console.WriteLine("No PMBus devices found on the bus.");
                Console.WriteLine("\nTroubleshooting:");
                Console.WriteLine("  1. Check I2C connections (SDA, SCL, GND)");
                Console.WriteLine("  2. Verify power supply is powered");
                Console.WriteLine("  3. Check pull-up resistors on I2C lines");
                Console.WriteLine("  4. Try different voltage levels if needed");
                return;
            }

            Console.WriteLine($"Found {foundDevices.Count} device(s):\n");
            foreach (var addr in foundDevices)
            {
                Console.WriteLine($"  0x{addr:X2} ({addr})");
            }

            Console.WriteLine($"\nUse 'pmbus-cli info -a 0x{foundDevices[0]:X2}' to read device information");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void ReadDeviceInfo(string? device, byte address, byte voltage, ushort clock)
    {
        using var ni8451 = new NI8451Interface();
        using var pmbus = ConnectToPMBus(ni8451, device, address, voltage, clock);

        if (pmbus == null) return;

        try
        {
            Console.WriteLine($"\n{'═'.ToString().PadRight(50, '═')}");
            Console.WriteLine($"  PMBus Device Information - Address 0x{address:X2}");
            Console.WriteLine($"{'═'.ToString().PadRight(50, '═')}\n");

            try
            {
                string mfrId = pmbus.ReadManufacturerId();
                Console.WriteLine($"  Manufacturer ID:    {mfrId}");
            }
            catch { Console.WriteLine("  Manufacturer ID:    (not available)"); }

            try
            {
                string model = pmbus.ReadModel();
                Console.WriteLine($"  Model:              {model}");
            }
            catch { Console.WriteLine("  Model:              (not available)"); }

            try
            {
                string revision = pmbus.ReadRevision();
                Console.WriteLine($"  Revision:           {revision}");
            }
            catch { Console.WriteLine("  Revision:           (not available)"); }

            try
            {
                string serial = pmbus.ReadSerialNumber();
                Console.WriteLine($"  Serial Number:      {serial}");
            }
            catch { Console.WriteLine("  Serial Number:      (not available)"); }

            try
            {
                string pmbusRev = pmbus.ReadPMBusRevision();
                Console.WriteLine($"  PMBus Revision:     {pmbusRev}");
            }
            catch { Console.WriteLine("  PMBus Revision:     (not available)"); }

            try
            {
                byte capability = pmbus.ReadCapability();
                Console.WriteLine($"  Capability:         0x{capability:X2}");
            }
            catch { Console.WriteLine("  Capability:         (not available)"); }

            try
            {
                byte voutMode = pmbus.ReadByte(PMBusCommands.VOUT_MODE);
                var (mode, exp) = LinearDataFormat.ParseVoutMode(voutMode);
                Console.WriteLine($"  VOUT_MODE:          {mode} (exponent: {exp})");
            }
            catch { Console.WriteLine("  VOUT_MODE:          (not available)"); }

            Console.WriteLine($"\n{'═'.ToString().PadRight(50, '═')}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading device info: {ex.Message}");
        }
    }

    static void ReadStatus(string? device, byte address, byte voltage, ushort clock)
    {
        using var ni8451 = new NI8451Interface();
        using var pmbus = ConnectToPMBus(ni8451, device, address, voltage, clock);

        if (pmbus == null) return;

        try
        {
            Console.WriteLine($"\n{'═'.ToString().PadRight(60, '═')}");
            Console.WriteLine($"  PMBus Device Status - Address 0x{address:X2}");
            Console.WriteLine($"{'═'.ToString().PadRight(60, '═')}\n");

            var statusWord = pmbus.ReadStatusWordDecoded();
            Console.WriteLine($"  STATUS_WORD:        0x{statusWord.RawValue:X4}");
            Console.WriteLine($"  Status:             {statusWord}\n");

            try
            {
                byte statusVout = pmbus.ReadByte(PMBusCommands.STATUS_VOUT);
                Console.WriteLine($"  STATUS_VOUT:        0x{statusVout:X2}");
            }
            catch { }

            try
            {
                byte statusIout = pmbus.ReadByte(PMBusCommands.STATUS_IOUT);
                Console.WriteLine($"  STATUS_IOUT:        0x{statusIout:X2}");
            }
            catch { }

            try
            {
                byte statusInput = pmbus.ReadByte(PMBusCommands.STATUS_INPUT);
                Console.WriteLine($"  STATUS_INPUT:       0x{statusInput:X2}");
            }
            catch { }

            try
            {
                byte statusTemp = pmbus.ReadByte(PMBusCommands.STATUS_TEMPERATURE);
                Console.WriteLine($"  STATUS_TEMP:        0x{statusTemp:X2}");
            }
            catch { }

            try
            {
                byte statusCml = pmbus.ReadByte(PMBusCommands.STATUS_CML);
                Console.WriteLine($"  STATUS_CML:         0x{statusCml:X2}");
            }
            catch { }

            Console.WriteLine($"\n{'═'.ToString().PadRight(60, '═')}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading status: {ex.Message}");
        }
    }

    static void MonitorDevice(string? device, byte address, byte voltage, ushort clock, int interval)
    {
        using var ni8451 = new NI8451Interface();
        using var pmbus = ConnectToPMBus(ni8451, device, address, voltage, clock);

        if (pmbus == null) return;

        Console.WriteLine($"\nMonitoring PMBus device at 0x{address:X2} (Press Ctrl+C to stop)\n");
        Console.WriteLine($"{"Time",-12} {"VIN (V)",-10} {"VOUT (V)",-10} {"IIN (A)",-10} {"IOUT (A)",-10} {"TEMP (°C)",-12} {"PIN (W)",-10} {"POUT (W)",-10}");
        Console.WriteLine(new string('─', 100));

        try
        {
            while (true)
            {
                try
                {
                    string time = DateTime.Now.ToString("HH:mm:ss.fff");

                    double vin = pmbus.ReadLinear11(PMBusCommands.READ_VIN);
                    double vout = pmbus.ReadVoltage(PMBusCommands.READ_VOUT);
                    double iin = pmbus.ReadLinear11(PMBusCommands.READ_IIN);
                    double iout = pmbus.ReadLinear11(PMBusCommands.READ_IOUT);
                    double temp = pmbus.ReadLinear11(PMBusCommands.READ_TEMPERATURE_1);
                    double pin = pmbus.ReadLinear11(PMBusCommands.READ_PIN);
                    double pout = pmbus.ReadLinear11(PMBusCommands.READ_POUT);

                    Console.WriteLine($"{time,-12} {vin,-10:F3} {vout,-10:F3} {iin,-10:F3} {iout,-10:F3} {temp,-12:F1} {pin,-10:F2} {pout,-10:F2}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Error: {ex.Message}");
                }

                Thread.Sleep(interval);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nMonitoring stopped.");
        }
    }

    static void ReadRegister(string? device, byte address, byte voltage, ushort clock,
                            string commandCode, int bytes, string format)
    {
        using var ni8451 = new NI8451Interface();
        using var pmbus = ConnectToPMBus(ni8451, device, address, voltage, clock);

        if (pmbus == null) return;

        try
        {
            byte cmd = ParseCommandCode(commandCode);
            Console.WriteLine($"\nReading command 0x{cmd:X2} ({PMBusCommands.GetCommandName(cmd)})...\n");

            switch (format.ToLower())
            {
                case "byte":
                    byte byteVal = pmbus.ReadByte(cmd);
                    Console.WriteLine($"  Value (byte):   0x{byteVal:X2} ({byteVal})");
                    break;

                case "word":
                    ushort wordVal = pmbus.ReadWord(cmd);
                    Console.WriteLine($"  Value (word):   0x{wordVal:X4} ({wordVal})");
                    break;

                case "linear11":
                    double linear11Val = pmbus.ReadLinear11(cmd);
                    ushort raw11 = pmbus.ReadWord(cmd);
                    Console.WriteLine($"  Raw value:      0x{raw11:X4}");
                    Console.WriteLine($"  Decoded:        {linear11Val:F6}");
                    break;

                case "linear16":
                    double linear16Val = pmbus.ReadVoltage(cmd);
                    ushort raw16 = pmbus.ReadWord(cmd);
                    Console.WriteLine($"  Raw value:      0x{raw16:X4}");
                    Console.WriteLine($"  Decoded:        {linear16Val:F6} V");
                    break;

                case "block":
                    byte[] blockData = pmbus.ReadBlock(cmd);
                    Console.WriteLine($"  Length:         {blockData.Length} bytes");
                    Console.WriteLine($"  Data (hex):     {LinearDataFormat.ToHexString(blockData)}");
                    Console.WriteLine($"  Data (ASCII):   {System.Text.Encoding.ASCII.GetString(blockData)}");
                    break;

                default: // hex
                    byte[] data = ni8451.WriteRead(new[] { cmd }, bytes);
                    Console.WriteLine($"  Data (hex):     {LinearDataFormat.ToHexString(data)}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void WriteRegister(string? device, byte address, byte voltage, ushort clock,
                             string commandCode, string data, bool isDecimal)
    {
        using var ni8451 = new NI8451Interface();
        using var pmbus = ConnectToPMBus(ni8451, device, address, voltage, clock);

        if (pmbus == null) return;

        try
        {
            byte cmd = ParseCommandCode(commandCode);
            Console.WriteLine($"\nWriting to command 0x{cmd:X2} ({PMBusCommands.GetCommandName(cmd)})...");

            byte[] writeData;

            if (isDecimal)
            {
                // Parse as decimal value (byte or word)
                if (ushort.TryParse(data, out ushort value))
                {
                    if (value <= 255)
                    {
                        pmbus.WriteByte(cmd, (byte)value);
                        Console.WriteLine($"  Written (byte): {value} (0x{value:X2})");
                    }
                    else
                    {
                        pmbus.WriteWord(cmd, value);
                        Console.WriteLine($"  Written (word): {value} (0x{value:X4})");
                    }
                    return;
                }
                else
                {
                    throw new ArgumentException("Invalid decimal value");
                }
            }
            else
            {
                // Parse as hex
                writeData = LinearDataFormat.FromHexString(data);
            }

            if (writeData.Length == 1)
            {
                pmbus.WriteByte(cmd, writeData[0]);
                Console.WriteLine($"  Written (byte): 0x{writeData[0]:X2}");
            }
            else if (writeData.Length == 2)
            {
                ushort value = (ushort)(writeData[0] | (writeData[1] << 8));
                pmbus.WriteWord(cmd, value);
                Console.WriteLine($"  Written (word): 0x{value:X4}");
            }
            else
            {
                pmbus.WriteBlock(cmd, writeData);
                Console.WriteLine($"  Written (block): {LinearDataFormat.ToHexString(writeData)}");
            }

            Console.WriteLine("  Success!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void SetVoltage(string? device, byte address, byte voltage, ushort clock, double value)
    {
        using var ni8451 = new NI8451Interface();
        using var pmbus = ConnectToPMBus(ni8451, device, address, voltage, clock);

        if (pmbus == null) return;

        try
        {
            Console.WriteLine($"\nSetting output voltage to {value:F3}V...");

            pmbus.SetOutputVoltage(value);

            // Read back to verify
            Thread.Sleep(100);
            double readback = pmbus.ReadVoltage(PMBusCommands.VOUT_COMMAND);

            Console.WriteLine($"  Command sent:   {value:F3}V");
            Console.WriteLine($"  Readback:       {readback:F3}V");
            Console.WriteLine("  Success!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void SetOperation(string? device, byte address, byte voltage, ushort clock, string operation)
    {
        using var ni8451 = new NI8451Interface();
        using var pmbus = ConnectToPMBus(ni8451, device, address, voltage, clock);

        if (pmbus == null) return;

        try
        {
            byte opCode = operation.ToLower() switch
            {
                "on" => OperationCommands.On,
                "off" => OperationCommands.Immediate_Off,
                "soft-off" => OperationCommands.Soft_Off,
                "margin-high" => OperationCommands.Margin_High,
                "margin-low" => OperationCommands.Margin_Low,
                _ => throw new ArgumentException($"Unknown operation: {operation}")
            };

            Console.WriteLine($"\nSetting operation to: {operation.ToUpper()}...");

            pmbus.SetOperation(opCode);

            Console.WriteLine("  Success!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void ClearFaults(string? device, byte address, byte voltage, ushort clock)
    {
        using var ni8451 = new NI8451Interface();
        using var pmbus = ConnectToPMBus(ni8451, device, address, voltage, clock);

        if (pmbus == null) return;

        try
        {
            Console.WriteLine("\nClearing all faults...");

            pmbus.ClearFaults();

            Console.WriteLine("  Success!");

            // Read status to verify
            Thread.Sleep(100);
            var status = pmbus.ReadStatusWordDecoded();
            Console.WriteLine($"  New status:     {status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static PMBusProtocol? ConnectToPMBus(NI8451Interface ni8451, string? device, byte address,
                                        byte voltage, ushort clock)
    {
        try
        {
            ni8451.Connect(device, voltage, clock);

            var pmbus = new PMBusProtocol(ni8451);
            pmbus.SetDeviceAddress(address);

            return pmbus;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
            return null;
        }
    }

    static byte ParseCommandCode(string commandCode)
    {
        commandCode = commandCode.Trim().Replace("0x", "").Replace("0X", "");

        if (byte.TryParse(commandCode, System.Globalization.NumberStyles.HexNumber, null, out byte result))
        {
            return result;
        }

        if (byte.TryParse(commandCode, out result))
        {
            return result;
        }

        throw new ArgumentException($"Invalid command code: {commandCode}");
    }
}
