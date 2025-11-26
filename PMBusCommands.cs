namespace PmbusMasterCLI
{
    /// <summary>
    /// PMBus standard command codes as per PMBus Specification Part II
    /// </summary>
    public static class PMBusCommands
    {
        // Standard PMBus Commands
        public const byte PAGE = 0x00;
        public const byte OPERATION = 0x01;
        public const byte ON_OFF_CONFIG = 0x02;
        public const byte CLEAR_FAULTS = 0x03;
        public const byte PHASE = 0x04;
        public const byte PAGE_PLUS_WRITE = 0x05;
        public const byte PAGE_PLUS_READ = 0x06;
        public const byte ZONE_CONFIG = 0x07;
        public const byte ZONE_ACTIVE = 0x08;

        public const byte WRITE_PROTECT = 0x10;
        public const byte STORE_DEFAULT_ALL = 0x11;
        public const byte RESTORE_DEFAULT_ALL = 0x12;
        public const byte STORE_DEFAULT_CODE = 0x13;
        public const byte RESTORE_DEFAULT_CODE = 0x14;
        public const byte STORE_USER_ALL = 0x15;
        public const byte RESTORE_USER_ALL = 0x16;
        public const byte STORE_USER_CODE = 0x17;
        public const byte RESTORE_USER_CODE = 0x18;
        public const byte CAPABILITY = 0x19;
        public const byte QUERY = 0x1A;
        public const byte SMBALERT_MASK = 0x1B;

        // Output Voltage Commands
        public const byte VOUT_MODE = 0x20;
        public const byte VOUT_COMMAND = 0x21;
        public const byte VOUT_TRIM = 0x22;
        public const byte VOUT_CAL_OFFSET = 0x23;
        public const byte VOUT_MAX = 0x24;
        public const byte VOUT_MARGIN_HIGH = 0x25;
        public const byte VOUT_MARGIN_LOW = 0x26;
        public const byte VOUT_TRANSITION_RATE = 0x27;
        public const byte VOUT_DROOP = 0x28;
        public const byte VOUT_SCALE_LOOP = 0x29;
        public const byte VOUT_SCALE_MONITOR = 0x2A;

        // Output Current Commands
        public const byte IOUT_CAL_GAIN = 0x38;
        public const byte IOUT_CAL_OFFSET = 0x39;
        public const byte IOUT_OC_FAULT_LIMIT = 0x46;
        public const byte IOUT_OC_FAULT_RESPONSE = 0x47;
        public const byte IOUT_OC_LV_FAULT_LIMIT = 0x48;
        public const byte IOUT_OC_LV_FAULT_RESPONSE = 0x49;
        public const byte IOUT_OC_WARN_LIMIT = 0x4A;
        public const byte IOUT_UC_FAULT_LIMIT = 0x4B;
        public const byte IOUT_UC_FAULT_RESPONSE = 0x4C;

        // Input Voltage Commands
        public const byte VIN_ON = 0x35;
        public const byte VIN_OFF = 0x36;
        public const byte VIN_OV_FAULT_LIMIT = 0x55;
        public const byte VIN_OV_FAULT_RESPONSE = 0x56;
        public const byte VIN_OV_WARN_LIMIT = 0x57;
        public const byte VIN_UV_WARN_LIMIT = 0x58;
        public const byte VIN_UV_FAULT_LIMIT = 0x59;
        public const byte VIN_UV_FAULT_RESPONSE = 0x5A;

        // Power Commands
        public const byte POUT_OP_FAULT_LIMIT = 0x68;
        public const byte POUT_OP_WARN_LIMIT = 0x6A;
        public const byte PIN_OP_WARN_LIMIT = 0x6B;

        // Temperature Commands
        public const byte OT_FAULT_LIMIT = 0x4F;
        public const byte OT_FAULT_RESPONSE = 0x50;
        public const byte OT_WARN_LIMIT = 0x51;
        public const byte UT_WARN_LIMIT = 0x52;
        public const byte UT_FAULT_LIMIT = 0x53;
        public const byte UT_FAULT_RESPONSE = 0x54;

        // Status Commands
        public const byte STATUS_BYTE = 0x78;
        public const byte STATUS_WORD = 0x79;
        public const byte STATUS_VOUT = 0x7A;
        public const byte STATUS_IOUT = 0x7B;
        public const byte STATUS_INPUT = 0x7C;
        public const byte STATUS_TEMPERATURE = 0x7D;
        public const byte STATUS_CML = 0x7E;
        public const byte STATUS_OTHER = 0x7F;
        public const byte STATUS_MFR_SPECIFIC = 0x80;
        public const byte STATUS_FANS_1_2 = 0x81;
        public const byte STATUS_FANS_3_4 = 0x82;

        // Read Commands
        public const byte READ_EIN = 0x86;
        public const byte READ_EOUT = 0x87;
        public const byte READ_VIN = 0x88;
        public const byte READ_IIN = 0x89;
        public const byte READ_VCAP = 0x8A;
        public const byte READ_VOUT = 0x8B;
        public const byte READ_IOUT = 0x8C;
        public const byte READ_TEMPERATURE_1 = 0x8D;
        public const byte READ_TEMPERATURE_2 = 0x8E;
        public const byte READ_TEMPERATURE_3 = 0x8F;
        public const byte READ_FAN_SPEED_1 = 0x90;
        public const byte READ_FAN_SPEED_2 = 0x91;
        public const byte READ_FAN_SPEED_3 = 0x92;
        public const byte READ_FAN_SPEED_4 = 0x93;
        public const byte READ_DUTY_CYCLE = 0x94;
        public const byte READ_FREQUENCY = 0x95;
        public const byte READ_POUT = 0x96;
        public const byte READ_PIN = 0x97;

        // Manufacturer Commands
        public const byte PMBUS_REVISION = 0x98;
        public const byte MFR_ID = 0x99;
        public const byte MFR_MODEL = 0x9A;
        public const byte MFR_REVISION = 0x9B;
        public const byte MFR_LOCATION = 0x9C;
        public const byte MFR_DATE = 0x9D;
        public const byte MFR_SERIAL = 0x9E;

        // Coefficient Commands
        public const byte COEFFICIENTS = 0x30;
        public const byte POUT_MAX = 0x31;

        // Fan Commands
        public const byte FAN_CONFIG_1_2 = 0x3A;
        public const byte FAN_COMMAND_1 = 0x3B;
        public const byte FAN_COMMAND_2 = 0x3C;
        public const byte FAN_CONFIG_3_4 = 0x3D;
        public const byte FAN_COMMAND_3 = 0x3E;
        public const byte FAN_COMMAND_4 = 0x3F;

        /// <summary>
        /// Get human-readable name for command code
        /// </summary>
        public static string GetCommandName(byte commandCode)
        {
            return commandCode switch
            {
                PAGE => "PAGE",
                OPERATION => "OPERATION",
                ON_OFF_CONFIG => "ON_OFF_CONFIG",
                CLEAR_FAULTS => "CLEAR_FAULTS",
                WRITE_PROTECT => "WRITE_PROTECT",
                VOUT_MODE => "VOUT_MODE",
                VOUT_COMMAND => "VOUT_COMMAND",
                VOUT_MAX => "VOUT_MAX",
                STATUS_BYTE => "STATUS_BYTE",
                STATUS_WORD => "STATUS_WORD",
                STATUS_VOUT => "STATUS_VOUT",
                STATUS_IOUT => "STATUS_IOUT",
                STATUS_INPUT => "STATUS_INPUT",
                STATUS_TEMPERATURE => "STATUS_TEMPERATURE",
                STATUS_CML => "STATUS_CML",
                READ_VIN => "READ_VIN",
                READ_IIN => "READ_IIN",
                READ_VOUT => "READ_VOUT",
                READ_IOUT => "READ_IOUT",
                READ_TEMPERATURE_1 => "READ_TEMPERATURE_1",
                READ_TEMPERATURE_2 => "READ_TEMPERATURE_2",
                READ_POUT => "READ_POUT",
                READ_PIN => "READ_PIN",
                PMBUS_REVISION => "PMBUS_REVISION",
                MFR_ID => "MFR_ID",
                MFR_MODEL => "MFR_MODEL",
                MFR_REVISION => "MFR_REVISION",
                MFR_SERIAL => "MFR_SERIAL",
                CAPABILITY => "CAPABILITY",
                _ => $"0x{commandCode:X2}"
            };
        }
    }
}
