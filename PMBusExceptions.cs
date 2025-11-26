namespace PmbusMasterCLI
{
    /// <summary>
    /// Base exception for all PMBus-related errors
    /// </summary>
    public class PMBusException : Exception
    {
        public PMBusException() { }
        public PMBusException(string message) : base(message) { }
        public PMBusException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when I2C communication fails
    /// </summary>
    public class PMBusCommunicationException : PMBusException
    {
        public byte? DeviceAddress { get; }
        public byte? CommandCode { get; }
        public int AttemptNumber { get; }

        public PMBusCommunicationException(string message, byte? deviceAddress = null, byte? commandCode = null, int attemptNumber = 0)
            : base(message)
        {
            DeviceAddress = deviceAddress;
            CommandCode = commandCode;
            AttemptNumber = attemptNumber;
        }

        public PMBusCommunicationException(string message, Exception innerException, byte? deviceAddress = null, byte? commandCode = null)
            : base(message, innerException)
        {
            DeviceAddress = deviceAddress;
            CommandCode = commandCode;
        }
    }

    /// <summary>
    /// Exception thrown when a timeout occurs
    /// </summary>
    public class PMBusTimeoutException : PMBusException
    {
        public int TimeoutMs { get; }

        public PMBusTimeoutException(string message, int timeoutMs) : base(message)
        {
            TimeoutMs = timeoutMs;
        }
    }

    /// <summary>
    /// Exception thrown when device is write-protected
    /// </summary>
    public class PMBusWriteProtectedException : PMBusException
    {
        public byte WriteProtectLevel { get; }

        public PMBusWriteProtectedException(string message, byte writeProtectLevel) : base(message)
        {
            WriteProtectLevel = writeProtectLevel;
        }
    }

    /// <summary>
    /// Exception thrown when voltage value is out of safe range
    /// </summary>
    public class PMBusVoltageOutOfRangeException : PMBusException
    {
        public double RequestedVoltage { get; }
        public double MinVoltage { get; }
        public double MaxVoltage { get; }

        public PMBusVoltageOutOfRangeException(double requestedVoltage, double minVoltage, double maxVoltage)
            : base($"Voltage {requestedVoltage}V is out of safe range ({minVoltage}V - {maxVoltage}V)")
        {
            RequestedVoltage = requestedVoltage;
            MinVoltage = minVoltage;
            MaxVoltage = maxVoltage;
        }
    }

    /// <summary>
    /// Exception thrown when device address is invalid
    /// </summary>
    public class PMBusInvalidAddressException : PMBusException
    {
        public int InvalidAddress { get; }

        public PMBusInvalidAddressException(int address)
            : base($"I2C address {address} (0x{address:X2}) is invalid. Must be 7-bit (0x00-0x7F)")
        {
            InvalidAddress = address;
        }
    }

    /// <summary>
    /// Exception thrown when hardware is not connected
    /// </summary>
    public class PMBusNotConnectedException : PMBusException
    {
        public PMBusNotConnectedException()
            : base("Not connected to PMBus hardware. Call Connect() first.")
        {
        }
    }

    /// <summary>
    /// Exception thrown when device is not found on bus
    /// </summary>
    public class PMBusDeviceNotFoundException : PMBusException
    {
        public byte DeviceAddress { get; }

        public PMBusDeviceNotFoundException(byte address)
            : base($"No PMBus device found at address 0x{address:X2}")
        {
            DeviceAddress = address;
        }
    }

    /// <summary>
    /// Exception thrown for invalid configuration
    /// </summary>
    public class PMBusConfigurationException : PMBusException
    {
        public List<string> ValidationErrors { get; }

        public PMBusConfigurationException(string message, List<string> validationErrors)
            : base(message)
        {
            ValidationErrors = validationErrors;
        }

        public PMBusConfigurationException(List<string> validationErrors)
            : base($"Configuration validation failed: {string.Join(", ", validationErrors)}")
        {
            ValidationErrors = validationErrors;
        }
    }

    /// <summary>
    /// Exception thrown when data format conversion fails
    /// </summary>
    public class PMBusDataFormatException : PMBusException
    {
        public PMBusDataFormatException(string message) : base(message) { }
        public PMBusDataFormatException(string message, Exception innerException) : base(message, innerException) { }
    }
}
