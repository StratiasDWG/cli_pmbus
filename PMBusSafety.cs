namespace PmbusMasterCLI
{
    /// <summary>
    /// Safety utilities for PMBus operations
    /// </summary>
    public static class PMBusSafety
    {
        /// <summary>
        /// Commands that are potentially dangerous and require confirmation
        /// </summary>
        public static readonly byte[] DangerousCommands = new[]
        {
            PMBusCommands.OPERATION,
            PMBusCommands.VOUT_COMMAND,
            PMBusCommands.CLEAR_FAULTS,
            PMBusCommands.STORE_DEFAULT_ALL,
            PMBusCommands.RESTORE_DEFAULT_ALL,
            PMBusCommands.STORE_DEFAULT_CODE,
            PMBusCommands.RESTORE_DEFAULT_CODE,
            PMBusCommands.STORE_USER_ALL,
            PMBusCommands.RESTORE_USER_ALL,
            PMBusCommands.STORE_USER_CODE,
            PMBusCommands.RESTORE_USER_CODE,
            PMBusCommands.ON_OFF_CONFIG,
            PMBusCommands.WRITE_PROTECT
        };

        /// <summary>
        /// Check if a command is considered dangerous
        /// </summary>
        public static bool IsDangerousCommand(byte commandCode)
        {
            return Array.Exists(DangerousCommands, cmd => cmd == commandCode);
        }

        /// <summary>
        /// Get safety warning message for a command
        /// </summary>
        public static string GetSafetyWarning(byte commandCode)
        {
            return commandCode switch
            {
                PMBusCommands.OPERATION => "This will change the output state (ON/OFF). The power supply may turn on or off.",
                PMBusCommands.VOUT_COMMAND => "This will change the output voltage setpoint. Ensure connected devices can handle the new voltage.",
                PMBusCommands.CLEAR_FAULTS => "This will clear all fault conditions. Ensure the fault cause has been addressed.",
                PMBusCommands.STORE_DEFAULT_ALL => "This will permanently store current settings to non-volatile memory. This cannot be easily undone.",
                PMBusCommands.RESTORE_DEFAULT_ALL => "This will restore factory defaults. All custom settings will be lost.",
                PMBusCommands.STORE_USER_ALL => "This will save current configuration to user memory.",
                PMBusCommands.RESTORE_USER_ALL => "This will restore previously saved user configuration.",
                PMBusCommands.ON_OFF_CONFIG => "This will change the power-on behavior and output control settings.",
                PMBusCommands.WRITE_PROTECT => "This will change write protection settings. This may prevent further configuration changes.",
                _ => "This command may alter device configuration or operation."
            };
        }

        /// <summary>
        /// Validate voltage value is within safe ranges
        /// </summary>
        public static bool IsVoltageInRange(double voltage, double min = 0.0, double max = 100.0)
        {
            return voltage >= min && voltage <= max && !double.IsNaN(voltage) && !double.IsInfinity(voltage);
        }

        /// <summary>
        /// Validate PMBus command code is valid
        /// </summary>
        public static bool IsValidCommandCode(byte commandCode)
        {
            // PMBus commands are typically in ranges:
            // 0x00-0x0F: Control commands
            // 0x10-0x1F: Configuration commands
            // 0x20-0x3F: Output/input configuration
            // 0x40-0x7F: Limits and settings
            // 0x78-0x7F: Status commands
            // 0x80-0x97: Read commands
            // 0x98-0x9E: Identification commands
            // 0xA0-0xFF: Manufacturer specific or reserved
            return true; // All byte values are potentially valid
        }

        /// <summary>
        /// Request user confirmation for dangerous operation
        /// </summary>
        public static bool RequestConfirmation(string message, bool forceMode = false)
        {
            if (forceMode)
                return true;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️  WARNING: " + message);
            Console.ResetColor();
            Console.Write("\nType 'yes' to continue or anything else to cancel: ");

            string? response = Console.ReadLine();
            return response?.Trim().ToLowerInvariant() == "yes";
        }

        /// <summary>
        /// Display value being written before confirmation
        /// </summary>
        public static void DisplayWriteOperation(byte commandCode, string value, byte? currentValue = null)
        {
            Console.WriteLine("\n" + new string('═', 60));
            Console.WriteLine($"  Write Operation: {PMBusCommands.GetCommandName(commandCode)} (0x{commandCode:X2})");
            Console.WriteLine(new string('═', 60));
            Console.WriteLine($"  New Value:       {value}");

            if (currentValue.HasValue)
            {
                Console.WriteLine($"  Current Value:   0x{currentValue.Value:X2}");
            }

            Console.WriteLine(new string('═', 60));
        }
    }

    /// <summary>
    /// Write protection status
    /// </summary>
    public enum WriteProtectLevel
    {
        Disabled = 0x00,            // All writes enabled
        ProtectVoltageCurrent = 0x40, // Protect voltage/current commands
        ProtectAll = 0x80           // Protect all write commands
    }

    /// <summary>
    /// Write operation result
    /// </summary>
    public class WriteResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public byte[]? WrittenValue { get; set; }
        public byte[]? ReadbackValue { get; set; }
        public bool VerificationPassed { get; set; }

        public override string ToString()
        {
            if (Success)
            {
                var msg = "Write successful";
                if (VerificationPassed)
                    msg += " (verified)";
                return msg;
            }
            else
            {
                return $"Write failed: {Message}";
            }
        }
    }
}
