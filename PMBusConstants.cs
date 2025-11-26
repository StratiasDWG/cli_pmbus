namespace PmbusMasterCLI
{
    /// <summary>
    /// Constants used throughout the PMBus Master CLI application
    /// </summary>
    public static class PMBusConstants
    {
        // I2C Address Constants
        public const byte MIN_I2C_ADDRESS = 0x00;
        public const byte MAX_I2C_ADDRESS = 0x7F;
        public const byte RESERVED_ADDRESS_START = 0x00;
        public const byte RESERVED_ADDRESS_END = 0x07;
        public const byte COMMON_ADDRESS_START = 0x10;
        public const byte DEFAULT_PMBUS_ADDRESS = 0x58;

        // Voltage Level Constants (for NI-8451)
        public const byte VOLTAGE_1_2V = 12;
        public const byte VOLTAGE_1_5V = 15;
        public const byte VOLTAGE_1_8V = 18;
        public const byte VOLTAGE_2_5V = 25;
        public const byte VOLTAGE_3_3V = 33;
        public const double VOLTAGE_DIVIDER = 10.0;

        // Clock Rate Constants (kHz)
        public const ushort MIN_CLOCK_RATE_KHZ = 10;
        public const ushort MAX_CLOCK_RATE_KHZ = 400;
        public const ushort DEFAULT_CLOCK_RATE_KHZ = 100;
        public const ushort STANDARD_MODE_CLOCK_KHZ = 100;
        public const ushort FAST_MODE_CLOCK_KHZ = 400;

        // Timeout Constants (milliseconds)
        public const int DEFAULT_OPERATION_TIMEOUT_MS = 5000;
        public const int MIN_OPERATION_TIMEOUT_MS = 100;
        public const int MAX_OPERATION_TIMEOUT_MS = 30000;
        public const int DEFAULT_RETRY_DELAY_MS = 10;
        public const int MAX_RETRY_DELAY_MS = 1000;
        public const int FAULT_CLEAR_DELAY_MS = 50;
        public const int WRITE_VERIFICATION_DELAY_MS = 100;

        // Retry Constants
        public const int DEFAULT_MAX_RETRIES = 3;
        public const int MIN_RETRIES = 0;
        public const int MAX_RETRIES = 10;

        // PMBus Data Format Constants
        public const int LINEAR11_MANTISSA_BITS = 11;
        public const int LINEAR11_EXPONENT_BITS = 5;
        public const int LINEAR11_MANTISSA_MASK = 0x7FF;
        public const int LINEAR11_EXPONENT_MASK = 0x1F;
        public const int LINEAR11_MANTISSA_SIGN_BIT = 0x400;
        public const int LINEAR11_EXPONENT_SIGN_BIT = 0x10;
        public const int LINEAR11_EXPONENT_SIGN_EXTEND = unchecked((int)0xFFFFFFE0);
        public const int LINEAR11_MANTISSA_SIGN_EXTEND = unchecked((int)0xFFFFF800);
        public const int LINEAR11_EXPONENT_SHIFT = 11;

        public const int LINEAR16_MANTISSA_MAX = 32767;
        public const int LINEAR16_MANTISSA_MIN = -32768;

        public const int VOUT_MODE_EXPONENT_MASK = 0x1F;
        public const int VOUT_MODE_MODE_SHIFT = 5;
        public const int VOUT_MODE_MODE_MASK = 0x07;

        public const int MIN_EXPONENT = -16;
        public const int MAX_EXPONENT = 15;
        public const int MANTISSA_SCALE_THRESHOLD_LOW = 512;
        public const int MANTISSA_SCALE_THRESHOLD_HIGH = 1024;
        public const int MANTISSA_MAX_11BIT = 1023;
        public const int MANTISSA_MIN_11BIT = -1024;

        public const double ZERO_THRESHOLD = 1e-10;

        // PMBus Block Transfer Constants
        public const int MAX_BLOCK_SIZE = 255;
        public const int MAX_BLOCK_READ_SIZE = 255;
        public const byte BLOCK_COUNT_OFFSET = 0;
        public const int BLOCK_DATA_OFFSET = 1;

        // Voltage Range Constants (Volts)
        public const double MIN_VOLTAGE = 0.0;
        public const double MAX_VOLTAGE = 100.0;
        public const double VOLTAGE_TOLERANCE_PERCENT = 0.01; // 1%

        // Monitoring Constants
        public const int MIN_MONITOR_INTERVAL_MS = 100;
        public const int MAX_MONITOR_INTERVAL_MS = 60000;
        public const int DEFAULT_MONITOR_INTERVAL_MS = 1000;

        // Buffer Size Constants
        public const int STRING_BUILDER_CAPACITY = 256;
        public const int MAX_DEVICE_NAME_LENGTH = 256;
        public const int MAX_ERROR_MESSAGE_LENGTH = 256;

        // UI Constants
        public const int DEFAULT_TABLE_WIDTH = 60;
        public const int INFO_TABLE_WIDTH = 50;
        public const int STATUS_TABLE_WIDTH = 60;
        public const int SEPARATOR_WIDTH = 62;

        // Configuration Constants
        public const int MIN_CACHE_EXPIRATION_HOURS = 1;
        public const int MAX_CACHE_EXPIRATION_HOURS = 168; // 1 week
        public const int DEFAULT_CACHE_EXPIRATION_HOURS = 24;

        // Valid Voltage Levels Array
        public static readonly byte[] VALID_VOLTAGE_LEVELS = new[]
        {
            VOLTAGE_1_2V,
            VOLTAGE_1_5V,
            VOLTAGE_1_8V,
            VOLTAGE_2_5V,
            VOLTAGE_3_3V
        };

        // Safe Confirmation String
        public const string CONFIRMATION_STRING = "yes";

        // Valid Output Formats
        public static readonly string[] VALID_OUTPUT_FORMATS = new[]
        {
            "Table",
            "Json",
            "Csv"
        };

        // VOUT_MODE Format Names
        public static readonly string[] VOUT_MODE_NAMES = new[]
        {
            "Linear",      // 0
            "VID",         // 1
            "Direct",      // 2
            "IEEE Half-Precision" // 3
        };

        /// <summary>
        /// Check if voltage level is valid
        /// </summary>
        public static bool IsValidVoltageLevel(byte level)
        {
            return Array.Exists(VALID_VOLTAGE_LEVELS, v => v == level);
        }

        /// <summary>
        /// Check if I2C address is valid
        /// </summary>
        public static bool IsValidI2CAddress(int address)
        {
            return address >= MIN_I2C_ADDRESS && address <= MAX_I2C_ADDRESS;
        }

        /// <summary>
        /// Check if clock rate is valid
        /// </summary>
        public static bool IsValidClockRate(int clockRate)
        {
            return clockRate >= MIN_CLOCK_RATE_KHZ && clockRate <= MAX_CLOCK_RATE_KHZ;
        }

        /// <summary>
        /// Check if voltage is in safe range
        /// </summary>
        public static bool IsValidVoltage(double voltage, double min = MIN_VOLTAGE, double max = MAX_VOLTAGE)
        {
            return voltage >= min && voltage <= max &&
                   !double.IsNaN(voltage) &&
                   !double.IsInfinity(voltage);
        }

        /// <summary>
        /// Check if output format is valid
        /// </summary>
        public static bool IsValidOutputFormat(string format)
        {
            return Array.Exists(VALID_OUTPUT_FORMATS, f =>
                f.Equals(format, StringComparison.OrdinalIgnoreCase));
        }
    }
}
