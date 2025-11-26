namespace PmbusMasterCLI
{
    /// <summary>
    /// PMBus Protocol Implementation
    /// Provides high-level PMBus communication functions
    /// </summary>
    public class PMBusProtocol : IDisposable
    {
        private readonly II2CInterface _interface;
        private byte _currentAddress = 0;
        private byte _currentPage = 0;
        private int? _voutModeExponent = null;
        private int _maxRetries = 3;
        private int _retryDelayMs = 10;

        public byte CurrentAddress => _currentAddress;
        public byte CurrentPage => _currentPage;
        public int MaxRetries { get => _maxRetries; set => _maxRetries = Math.Max(0, value); }
        public int RetryDelayMs { get => _retryDelayMs; set => _retryDelayMs = Math.Max(0, value); }

        public PMBusProtocol(II2CInterface i2cInterface)
        {
            _interface = i2cInterface ?? throw new ArgumentNullException(nameof(i2cInterface));
        }

        /// <summary>
        /// Set the PMBus device address for subsequent operations
        /// </summary>
        public void SetDeviceAddress(byte address)
        {
            if (address > 0x7F)
                throw new ArgumentException("Address must be 7-bit (0x00-0x7F)");

            _currentAddress = address;
            _interface.SetSlaveAddress(address);
            _voutModeExponent = null; // Reset cached exponent
            _currentPage = 0; // Reset page
        }

        /// <summary>
        /// Set the current PAGE for multi-rail power supplies
        /// </summary>
        public void SetPage(byte page)
        {
            WriteByte(PMBusCommands.PAGE, page);
            _currentPage = page;
            _voutModeExponent = null; // Reset cached exponent as it may differ per page
        }

        /// <summary>
        /// Check if writes are protected
        /// </summary>
        /// <returns>Write protection status byte</returns>
        public byte CheckWriteProtection()
        {
            try
            {
                return ReadByte(PMBusCommands.WRITE_PROTECT);
            }
            catch
            {
                // Device may not support WRITE_PROTECT command
                return 0x00; // Assume not protected
            }
        }

        /// <summary>
        /// Execute an operation with retry logic
        /// </summary>
        private T ExecuteWithRetry<T>(Func<T> operation, string operationName)
        {
            int attempts = 0;
            Exception? lastException = null;

            while (attempts <= _maxRetries)
            {
                try
                {
                    return operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempts++;

                    if (attempts <= _maxRetries)
                    {
                        Thread.Sleep(_retryDelayMs * attempts); // Exponential backoff
                    }
                }
            }

            throw new Exception($"{operationName} failed after {_maxRetries + 1} attempts", lastException);
        }

        /// <summary>
        /// Send a command byte (for commands without data)
        /// </summary>
        public void SendCommand(byte commandCode)
        {
            ExecuteWithRetry(() =>
            {
                _interface.Write(new[] { commandCode });
                return true;
            }, $"SendCommand(0x{commandCode:X2})");
        }

        /// <summary>
        /// Write a byte to a PMBus register
        /// </summary>
        public void WriteByte(byte commandCode, byte value)
        {
            ExecuteWithRetry(() =>
            {
                _interface.Write(new[] { commandCode, value });
                return true;
            }, $"WriteByte(0x{commandCode:X2}, 0x{value:X2})");
        }

        /// <summary>
        /// Write a word (2 bytes) to a PMBus register
        /// </summary>
        public void WriteWord(byte commandCode, ushort value)
        {
            ExecuteWithRetry(() =>
            {
                byte lowByte = (byte)(value & 0xFF);
                byte highByte = (byte)((value >> 8) & 0xFF);
                _interface.Write(new[] { commandCode, lowByte, highByte });
                return true;
            }, $"WriteWord(0x{commandCode:X2}, 0x{value:X4})");
        }

        /// <summary>
        /// Write multiple bytes to a PMBus register (block write)
        /// </summary>
        public void WriteBlock(byte commandCode, byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data cannot be null or empty");

            if (data.Length > 255)
                throw new ArgumentException("Block write data cannot exceed 255 bytes");

            ExecuteWithRetry(() =>
            {
                // PMBus block format: Command + ByteCount + Data
                byte[] writeData = new byte[data.Length + 2];
                writeData[0] = commandCode;
                writeData[1] = (byte)data.Length;
                Array.Copy(data, 0, writeData, 2, data.Length);

                _interface.Write(writeData);
                return true;
            }, $"WriteBlock(0x{commandCode:X2}, {data.Length} bytes)");
        }

        /// <summary>
        /// Read a single byte from a PMBus register
        /// </summary>
        public byte ReadByte(byte commandCode)
        {
            byte[] data = _interface.WriteRead(new[] { commandCode }, 1);
            return data[0];
        }

        /// <summary>
        /// Read a word (2 bytes) from a PMBus register
        /// </summary>
        public ushort ReadWord(byte commandCode)
        {
            byte[] data = _interface.WriteRead(new[] { commandCode }, 2);
            return (ushort)(data[0] | (data[1] << 8));
        }

        /// <summary>
        /// Read a block of data from a PMBus register
        /// PMBus block format: first byte is count, followed by data
        /// </summary>
        public byte[] ReadBlock(byte commandCode, int maxBytes = 255)
        {
            if (maxBytes < 1 || maxBytes > 255)
                throw new ArgumentException("Max bytes must be between 1 and 255");

            // Read byte count first, then read that many bytes
            // We read maxBytes + 1 (1 for count byte)
            byte[] rawData = _interface.WriteRead(new[] { commandCode }, maxBytes + 1);

            if (rawData.Length == 0)
                return Array.Empty<byte>();

            byte count = rawData[0];

            // Return only the actual data (excluding count byte)
            byte[] data = new byte[Math.Min(count, rawData.Length - 1)];
            Array.Copy(rawData, 1, data, 0, data.Length);

            return data;
        }

        /// <summary>
        /// Read voltage in LINEAR16 format
        /// Requires VOUT_MODE to be read first to get exponent
        /// </summary>
        public double ReadVoltage(byte commandCode)
        {
            // Cache VOUT_MODE exponent if not already cached
            if (!_voutModeExponent.HasValue)
            {
                byte voutMode = ReadByte(PMBusCommands.VOUT_MODE);
                _voutModeExponent = LinearDataFormat.GetVoutModeExponent(voutMode);
            }

            byte[] data = _interface.WriteRead(new[] { commandCode }, 2);
            return LinearDataFormat.Linear16ToReal(data, _voutModeExponent.Value);
        }

        /// <summary>
        /// Read value in LINEAR11 format (most PMBus telemetry uses this)
        /// </summary>
        public double ReadLinear11(byte commandCode)
        {
            ushort rawValue = ReadWord(commandCode);
            return LinearDataFormat.Linear11ToReal(rawValue);
        }

        /// <summary>
        /// Write voltage in LINEAR16 format
        /// </summary>
        public void WriteVoltage(byte commandCode, double voltage)
        {
            // Cache VOUT_MODE exponent if not already cached
            if (!_voutModeExponent.HasValue)
            {
                byte voutMode = ReadByte(PMBusCommands.VOUT_MODE);
                _voutModeExponent = LinearDataFormat.GetVoutModeExponent(voutMode);
            }

            ushort linearValue = LinearDataFormat.RealToLinear16(voltage, _voutModeExponent.Value);
            WriteWord(commandCode, linearValue);
        }

        /// <summary>
        /// Write value in LINEAR11 format
        /// </summary>
        public void WriteLinear11(byte commandCode, double value)
        {
            ushort linearValue = LinearDataFormat.RealToLinear11(value);
            WriteWord(commandCode, linearValue);
        }

        /// <summary>
        /// Clear all faults
        /// </summary>
        public void ClearFaults()
        {
            SendCommand(PMBusCommands.CLEAR_FAULTS);
            Thread.Sleep(50); // Give device time to clear faults
        }

        /// <summary>
        /// Read device status byte
        /// </summary>
        public byte ReadStatusByte()
        {
            return ReadByte(PMBusCommands.STATUS_BYTE);
        }

        /// <summary>
        /// Read device status word
        /// </summary>
        public ushort ReadStatusWord()
        {
            return ReadWord(PMBusCommands.STATUS_WORD);
        }

        /// <summary>
        /// Read and decode STATUS_WORD
        /// </summary>
        public StatusWordFlags ReadStatusWordDecoded()
        {
            ushort statusWord = ReadStatusWord();
            return new StatusWordFlags(statusWord);
        }

        /// <summary>
        /// Read manufacturer ID
        /// </summary>
        public string ReadManufacturerId()
        {
            byte[] data = ReadBlock(PMBusCommands.MFR_ID);
            return System.Text.Encoding.ASCII.GetString(data).TrimEnd('\0', ' ');
        }

        /// <summary>
        /// Read model name
        /// </summary>
        public string ReadModel()
        {
            byte[] data = ReadBlock(PMBusCommands.MFR_MODEL);
            return System.Text.Encoding.ASCII.GetString(data).TrimEnd('\0', ' ');
        }

        /// <summary>
        /// Read firmware/hardware revision
        /// </summary>
        public string ReadRevision()
        {
            byte[] data = ReadBlock(PMBusCommands.MFR_REVISION);
            return System.Text.Encoding.ASCII.GetString(data).TrimEnd('\0', ' ');
        }

        /// <summary>
        /// Read serial number
        /// </summary>
        public string ReadSerialNumber()
        {
            byte[] data = ReadBlock(PMBusCommands.MFR_SERIAL);
            return System.Text.Encoding.ASCII.GetString(data).TrimEnd('\0', ' ');
        }

        /// <summary>
        /// Read PMBus revision
        /// </summary>
        public string ReadPMBusRevision()
        {
            byte revision = ReadByte(PMBusCommands.PMBUS_REVISION);
            int major = (revision >> 4) & 0x0F;
            int minor = revision & 0x0F;
            return $"{major}.{minor}";
        }

        /// <summary>
        /// Read device capability
        /// </summary>
        public byte ReadCapability()
        {
            return ReadByte(PMBusCommands.CAPABILITY);
        }

        /// <summary>
        /// Set output voltage
        /// </summary>
        public void SetOutputVoltage(double voltage)
        {
            WriteVoltage(PMBusCommands.VOUT_COMMAND, voltage);
        }

        /// <summary>
        /// Turn output on/off
        /// </summary>
        public void SetOperation(byte operation)
        {
            WriteByte(PMBusCommands.OPERATION, operation);
        }

        /// <summary>
        /// Scan I2C bus for PMBus devices
        /// </summary>
        public static List<byte> ScanBus(NI8451Interface interface8451)
        {
            var foundDevices = new List<byte>();

            // Scan common PMBus addresses (0x10-0x7F, skipping reserved addresses)
            for (byte addr = 0x10; addr <= 0x7F; addr++)
            {
                try
                {
                    interface8451.SetSlaveAddress(addr);

                    // Try to read PMBus revision (most devices support this)
                    byte[] data = interface8451.WriteRead(new[] { PMBusCommands.PMBUS_REVISION }, 1);

                    if (data.Length > 0)
                    {
                        foundDevices.Add(addr);
                    }
                }
                catch
                {
                    // Device not present at this address
                }
            }

            return foundDevices;
        }

        public void Dispose()
        {
            // Nothing to dispose in this class
            // NI8451Interface is owned by caller
        }
    }

    /// <summary>
    /// STATUS_WORD bit flags
    /// </summary>
    public class StatusWordFlags
    {
        public ushort RawValue { get; }

        public bool VoutFault => (RawValue & 0x8000) != 0;
        public bool IoutFault => (RawValue & 0x4000) != 0;
        public bool InputFault => (RawValue & 0x2000) != 0;
        public bool MfrSpecific => (RawValue & 0x1000) != 0;
        public bool PowerGoodNegated => (RawValue & 0x0800) != 0;
        public bool FanFault => (RawValue & 0x0400) != 0;
        public bool OtherFault => (RawValue & 0x0200) != 0;
        public bool Unknown => (RawValue & 0x0100) != 0;
        public bool Busy => (RawValue & 0x0080) != 0;
        public bool Off => (RawValue & 0x0040) != 0;
        public bool VoutOvFault => (RawValue & 0x0020) != 0;
        public bool IoutOcFault => (RawValue & 0x0010) != 0;
        public bool VinUvFault => (RawValue & 0x0008) != 0;
        public bool TemperatureFault => (RawValue & 0x0004) != 0;
        public bool CmlFault => (RawValue & 0x0002) != 0;
        public bool None => (RawValue & 0x0001) != 0;

        public StatusWordFlags(ushort statusWord)
        {
            RawValue = statusWord;
        }

        public override string ToString()
        {
            var flags = new List<string>();

            if (VoutFault) flags.Add("VOUT_FAULT");
            if (IoutFault) flags.Add("IOUT_FAULT");
            if (InputFault) flags.Add("INPUT_FAULT");
            if (MfrSpecific) flags.Add("MFR_SPECIFIC");
            if (PowerGoodNegated) flags.Add("POWER_GOOD_NEGATED");
            if (FanFault) flags.Add("FAN_FAULT");
            if (OtherFault) flags.Add("OTHER_FAULT");
            if (Unknown) flags.Add("UNKNOWN");
            if (Busy) flags.Add("BUSY");
            if (Off) flags.Add("OFF");
            if (VoutOvFault) flags.Add("VOUT_OV_FAULT");
            if (IoutOcFault) flags.Add("IOUT_OC_FAULT");
            if (VinUvFault) flags.Add("VIN_UV_FAULT");
            if (TemperatureFault) flags.Add("TEMP_FAULT");
            if (CmlFault) flags.Add("CML_FAULT");

            return flags.Count > 0 ? string.Join(", ", flags) : "OK";
        }
    }

    /// <summary>
    /// OPERATION command values
    /// </summary>
    public static class OperationCommands
    {
        public const byte Immediate_Off = 0x00;
        public const byte Soft_Off = 0x40;
        public const byte On = 0x80;
        public const byte Margin_Low = 0x98;
        public const byte Margin_High = 0xA8;
    }
}
