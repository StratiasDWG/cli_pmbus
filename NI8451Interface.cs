using System.Runtime.InteropServices;
using System.Text;

namespace PmbusMasterCLI
{
    /// <summary>
    /// NI-8451 USB-to-I2C/SPI Interface Hardware Wrapper
    /// </summary>
    public class NI8451Interface : II2CInterface
    {
        private IntPtr _deviceHandle = IntPtr.Zero;
        private IntPtr _i2cConfigHandle = IntPtr.Zero;
        private bool _isConnected = false;
        private string _deviceName = string.Empty;

        // Voltage Levels
        public const byte kNi845x33Volts = 33; // 3.3V
        public const byte kNi845x25Volts = 25; // 2.5V
        public const byte kNi845x18Volts = 18; // 1.8V
        public const byte kNi845x15Volts = 15; // 1.5V
        public const byte kNi845x12Volts = 12; // 1.2V

        // Address Sizes
        private const int kNi845xI2cAddress7Bit = 7;
        private const int kNi845xI2cAddress10Bit = 10;

        public bool IsConnected => _isConnected;
        public string DeviceName => _deviceName;

        #region DLL Imports

        [DllImport("Ni845x.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern Int32 ni845xFindDevice(StringBuilder FirstDevice, ref IntPtr FindDeviceHandle, ref uint NumberFound);

        [DllImport("Ni845x.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern Int32 ni845xOpen(string ResourceName, ref IntPtr DeviceHandle);

        [DllImport("Ni845x.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern Int32 ni845xClose(IntPtr DeviceHandle);

        [DllImport("Ni845x.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern Int32 ni845xSetIoVoltageLevel(IntPtr DeviceHandle, byte VoltageLevel);

        [DllImport("Ni845x.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern Int32 ni845xI2cConfigurationOpen(ref IntPtr I2CHandle);

        [DllImport("Ni845x.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern Int32 ni845xI2cConfigurationClose(IntPtr I2CHandle);

        [DllImport("Ni845x.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern Int32 ni845xI2cConfigurationSetAddressSize(IntPtr I2CHandle, Int32 Size);

        [DllImport("Ni845x.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern Int32 ni845xI2cConfigurationSetAddress(IntPtr I2CHandle, ushort Address);

        [DllImport("Ni845x.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern Int32 ni845xI2cConfigurationSetClockRate(IntPtr I2CHandle, ushort ClockRate);

        [DllImport("Ni845x.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 ni845xI2cWriteRead(
            IntPtr DeviceHandle,
            IntPtr ConfigurationHandle,
            uint WriteSize,
            [MarshalAs(UnmanagedType.LPArray)][In] byte[] pWriteData,
            uint NumBytesToRead,
            ref uint ReadSize,
            [MarshalAs(UnmanagedType.LPArray)][Out] byte[] pReadData);

        [DllImport("Ni845x.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern Int32 ni845xI2cWrite(
            IntPtr DeviceHandle,
            IntPtr I2CHandle,
            uint WriteSize,
            [MarshalAs(UnmanagedType.LPArray)][In] byte[] pWriteData);

        [DllImport("Ni845x.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern Int32 ni845xI2cSetPullupEnable(IntPtr DeviceHandle, byte Enable);

        [DllImport("Ni845x.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 ni845xStatusToString(Int32 StatusCode, uint MaxSize, StringBuilder StatusString);

        #endregion

        /// <summary>
        /// Find and list all NI-8451 devices
        /// </summary>
        public static List<string> FindDevices()
        {
            var devices = new List<string>();
            StringBuilder deviceName = new StringBuilder(256);
            IntPtr findHandle = IntPtr.Zero;
            uint numFound = 0;

            Int32 status = ni845xFindDevice(deviceName, ref findHandle, ref numFound);

            if (status == 0 && numFound > 0)
            {
                devices.Add(deviceName.ToString());
            }

            return devices;
        }

        /// <summary>
        /// Connect to NI-8451 device
        /// </summary>
        public void Connect(string? deviceName = null, byte voltageLevel = kNi845x33Volts, ushort clockRate = 100)
        {
            if (_isConnected)
                throw new InvalidOperationException("Already connected to a device");

            // Find device if name not specified
            if (string.IsNullOrEmpty(deviceName))
            {
                var devices = FindDevices();
                if (devices.Count == 0)
                    throw new Exception("No NI-8451 devices found");

                deviceName = devices[0];
            }

            // Open device
            Int32 status = ni845xOpen(deviceName, ref _deviceHandle);
            CheckStatus(status, "Failed to open device");

            _deviceName = deviceName;

            try
            {
                // Set IO voltage level
                status = ni845xSetIoVoltageLevel(_deviceHandle, voltageLevel);
                CheckStatus(status, "Failed to set voltage level");

                // Create I2C configuration
                status = ni845xI2cConfigurationOpen(ref _i2cConfigHandle);
                CheckStatus(status, "Failed to open I2C configuration");

                // Set address size to 7-bit
                status = ni845xI2cConfigurationSetAddressSize(_i2cConfigHandle, kNi845xI2cAddress7Bit);
                CheckStatus(status, "Failed to set address size");

                // Set clock rate (in kHz)
                status = ni845xI2cConfigurationSetClockRate(_i2cConfigHandle, clockRate);
                CheckStatus(status, "Failed to set clock rate");

                // Enable internal pull-ups
                status = ni845xI2cSetPullupEnable(_deviceHandle, 1);
                CheckStatus(status, "Failed to enable pull-ups");

                _isConnected = true;
            }
            catch
            {
                // Cleanup on error
                if (_i2cConfigHandle != IntPtr.Zero)
                {
                    ni845xI2cConfigurationClose(_i2cConfigHandle);
                    _i2cConfigHandle = IntPtr.Zero;
                }

                if (_deviceHandle != IntPtr.Zero)
                {
                    ni845xClose(_deviceHandle);
                    _deviceHandle = IntPtr.Zero;
                }

                throw;
            }
        }

        /// <summary>
        /// Disconnect from device
        /// </summary>
        public void Disconnect()
        {
            if (_i2cConfigHandle != IntPtr.Zero)
            {
                ni845xI2cConfigurationClose(_i2cConfigHandle);
                _i2cConfigHandle = IntPtr.Zero;
            }

            if (_deviceHandle != IntPtr.Zero)
            {
                ni845xClose(_deviceHandle);
                _deviceHandle = IntPtr.Zero;
            }

            _isConnected = false;
            _deviceName = string.Empty;
        }

        /// <summary>
        /// Set I2C slave address for subsequent operations
        /// </summary>
        public void SetSlaveAddress(byte address)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Not connected to device");

            Int32 status = ni845xI2cConfigurationSetAddress(_i2cConfigHandle, address);
            CheckStatus(status, "Failed to set slave address");
        }

        /// <summary>
        /// Write data to I2C bus
        /// </summary>
        public void Write(byte[] data)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Not connected to device");

            if (data == null || data.Length == 0)
                throw new ArgumentException("Write data cannot be null or empty");

            Int32 status = ni845xI2cWrite(_deviceHandle, _i2cConfigHandle, (uint)data.Length, data);
            CheckStatus(status, "I2C write failed");
        }

        /// <summary>
        /// Write data and read response from I2C bus
        /// </summary>
        public byte[] WriteRead(byte[] writeData, int readLength)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Not connected to device");

            if (writeData == null || writeData.Length == 0)
                throw new ArgumentException("Write data cannot be null or empty");

            if (readLength <= 0)
                throw new ArgumentException("Read length must be greater than zero");

            byte[] readData = new byte[readLength];
            uint actualReadSize = 0;

            Int32 status = ni845xI2cWriteRead(
                _deviceHandle,
                _i2cConfigHandle,
                (uint)writeData.Length,
                writeData,
                (uint)readLength,
                ref actualReadSize,
                readData);

            CheckStatus(status, "I2C write-read failed");

            // Trim to actual read size if different
            if (actualReadSize < readLength)
            {
                Array.Resize(ref readData, (int)actualReadSize);
            }

            return readData;
        }

        /// <summary>
        /// Check NI-845x status code and throw exception on error
        /// </summary>
        private void CheckStatus(Int32 statusCode, string message)
        {
            if (statusCode != 0)
            {
                StringBuilder errorString = new StringBuilder(256);
                ni845xStatusToString(statusCode, 256, errorString);
                throw new Exception($"{message}: {errorString} (Code: 0x{statusCode:X8})");
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
