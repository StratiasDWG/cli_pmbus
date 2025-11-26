namespace PmbusMasterCLI
{
    /// <summary>
    /// Centralized input validation for PMBus operations
    /// </summary>
    public static class InputValidator
    {
        /// <summary>
        /// Validate I2C device address
        /// </summary>
        /// <exception cref="PMBusInvalidAddressException">Thrown when address is invalid</exception>
        public static void ValidateAddress(int address)
        {
            if (!PMBusConstants.IsValidI2CAddress(address))
            {
                throw new PMBusInvalidAddressException(address);
            }
        }

        /// <summary>
        /// Validate I2C device address (byte overload)
        /// </summary>
        public static void ValidateAddress(byte address)
        {
            ValidateAddress((int)address);
        }

        /// <summary>
        /// Validate voltage value for hardware safety
        /// </summary>
        /// <exception cref="PMBusVoltageOutOfRangeException">Thrown when voltage is out of range</exception>
        public static void ValidateVoltage(double voltage, double min = PMBusConstants.MIN_VOLTAGE, double max = PMBusConstants.MAX_VOLTAGE)
        {
            if (double.IsNaN(voltage))
            {
                throw new PMBusVoltageOutOfRangeException(voltage, min, max);
            }

            if (double.IsInfinity(voltage))
            {
                throw new PMBusVoltageOutOfRangeException(voltage, min, max);
            }

            if (voltage < min || voltage > max)
            {
                throw new PMBusVoltageOutOfRangeException(voltage, min, max);
            }
        }

        /// <summary>
        /// Validate I2C clock rate
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when clock rate is invalid</exception>
        public static void ValidateClockRate(int clockRate)
        {
            if (!PMBusConstants.IsValidClockRate(clockRate))
            {
                throw new ArgumentOutOfRangeException(nameof(clockRate),
                    clockRate,
                    $"Clock rate must be between {PMBusConstants.MIN_CLOCK_RATE_KHZ} and {PMBusConstants.MAX_CLOCK_RATE_KHZ} kHz");
            }
        }

        /// <summary>
        /// Validate I2C clock rate (ushort overload)
        /// </summary>
        public static void ValidateClockRate(ushort clockRate)
        {
            ValidateClockRate((int)clockRate);
        }

        /// <summary>
        /// Validate voltage level for NI-8451
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when voltage level is invalid</exception>
        public static void ValidateVoltageLevel(byte voltageLevel)
        {
            if (!PMBusConstants.IsValidVoltageLevel(voltageLevel))
            {
                string validLevels = string.Join(", ", PMBusConstants.VALID_VOLTAGE_LEVELS.Select(v => $"{v / PMBusConstants.VOLTAGE_DIVIDER:F1}V"));
                throw new ArgumentException(
                    $"Voltage level {voltageLevel} is invalid. Valid levels are: {validLevels}",
                    nameof(voltageLevel));
            }
        }

        /// <summary>
        /// Validate monitoring interval
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when interval is invalid</exception>
        public static void ValidateMonitorInterval(int intervalMs)
        {
            if (intervalMs < PMBusConstants.MIN_MONITOR_INTERVAL_MS)
            {
                throw new ArgumentOutOfRangeException(nameof(intervalMs),
                    intervalMs,
                    $"Monitoring interval must be at least {PMBusConstants.MIN_MONITOR_INTERVAL_MS}ms");
            }

            if (intervalMs > PMBusConstants.MAX_MONITOR_INTERVAL_MS)
            {
                throw new ArgumentOutOfRangeException(nameof(intervalMs),
                    intervalMs,
                    $"Monitoring interval cannot exceed {PMBusConstants.MAX_MONITOR_INTERVAL_MS}ms");
            }
        }

        /// <summary>
        /// Validate timeout value
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when timeout is invalid</exception>
        public static void ValidateTimeout(int timeoutMs)
        {
            if (timeoutMs < PMBusConstants.MIN_OPERATION_TIMEOUT_MS)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutMs),
                    timeoutMs,
                    $"Timeout must be at least {PMBusConstants.MIN_OPERATION_TIMEOUT_MS}ms");
            }

            if (timeoutMs > PMBusConstants.MAX_OPERATION_TIMEOUT_MS)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutMs),
                    timeoutMs,
                    $"Timeout cannot exceed {PMBusConstants.MAX_OPERATION_TIMEOUT_MS}ms");
            }
        }

        /// <summary>
        /// Validate retry count
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when retry count is invalid</exception>
        public static void ValidateRetryCount(int retries)
        {
            if (retries < PMBusConstants.MIN_RETRIES || retries > PMBusConstants.MAX_RETRIES)
            {
                throw new ArgumentOutOfRangeException(nameof(retries),
                    retries,
                    $"Retry count must be between {PMBusConstants.MIN_RETRIES} and {PMBusConstants.MAX_RETRIES}");
            }
        }

        /// <summary>
        /// Validate retry delay
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when delay is invalid</exception>
        public static void ValidateRetryDelay(int delayMs)
        {
            if (delayMs < 0 || delayMs > PMBusConstants.MAX_RETRY_DELAY_MS)
            {
                throw new ArgumentOutOfRangeException(nameof(delayMs),
                    delayMs,
                    $"Retry delay must be between 0 and {PMBusConstants.MAX_RETRY_DELAY_MS}ms");
            }
        }

        /// <summary>
        /// Validate block data size
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when data is invalid</exception>
        public static void ValidateBlockData(byte[]? data)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentException("Block data cannot be null or empty", nameof(data));
            }

            if (data.Length > PMBusConstants.MAX_BLOCK_SIZE)
            {
                throw new ArgumentException(
                    $"Block data cannot exceed {PMBusConstants.MAX_BLOCK_SIZE} bytes",
                    nameof(data));
            }
        }

        /// <summary>
        /// Validate read length
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when length is invalid</exception>
        public static void ValidateReadLength(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length),
                    length,
                    "Read length must be greater than zero");
            }

            if (length > PMBusConstants.MAX_BLOCK_READ_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(length),
                    length,
                    $"Read length cannot exceed {PMBusConstants.MAX_BLOCK_READ_SIZE} bytes");
            }
        }

        /// <summary>
        /// Validate PMBus page number
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when page is invalid</exception>
        public static void ValidatePage(byte page, byte maxPages = 255)
        {
            if (page > maxPages)
            {
                throw new ArgumentException(
                    $"Page {page} is invalid. Maximum page is {maxPages}",
                    nameof(page));
            }
        }

        /// <summary>
        /// Validate output format string
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when format is invalid</exception>
        public static void ValidateOutputFormat(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                throw new ArgumentException("Output format cannot be null or empty", nameof(format));
            }

            if (!PMBusConstants.IsValidOutputFormat(format))
            {
                string validFormats = string.Join(", ", PMBusConstants.VALID_OUTPUT_FORMATS);
                throw new ArgumentException(
                    $"Output format '{format}' is invalid. Valid formats are: {validFormats}",
                    nameof(format));
            }
        }

        /// <summary>
        /// Validate device name
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when device name is invalid</exception>
        public static void ValidateDeviceName(string? deviceName)
        {
            if (!string.IsNullOrEmpty(deviceName))
            {
                if (deviceName.Length > PMBusConstants.MAX_DEVICE_NAME_LENGTH)
                {
                    throw new ArgumentException(
                        $"Device name cannot exceed {PMBusConstants.MAX_DEVICE_NAME_LENGTH} characters",
                        nameof(deviceName));
                }

                // Check for potentially dangerous characters
                if (deviceName.Contains('\0'))
                {
                    throw new ArgumentException(
                        "Device name cannot contain null characters",
                        nameof(deviceName));
                }
            }
        }

        /// <summary>
        /// Validate LINEAR11 exponent range
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when exponent is invalid</exception>
        public static void ValidateExponent(int exponent)
        {
            if (exponent < PMBusConstants.MIN_EXPONENT || exponent > PMBusConstants.MAX_EXPONENT)
            {
                throw new ArgumentOutOfRangeException(nameof(exponent),
                    exponent,
                    $"Exponent must be between {PMBusConstants.MIN_EXPONENT} and {PMBusConstants.MAX_EXPONENT}");
            }
        }

        /// <summary>
        /// Validate that object is not null
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when object is null</exception>
        public static void ValidateNotNull<T>(T? obj, string paramName) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Validate hex string format
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when hex string is invalid</exception>
        public static void ValidateHexString(string hexString)
        {
            if (string.IsNullOrWhiteSpace(hexString))
            {
                throw new ArgumentException("Hex string cannot be null or empty", nameof(hexString));
            }

            string cleaned = hexString.Replace(" ", "").Replace("0x", "").Replace("0X", "");

            if (cleaned.Length % 2 != 0)
            {
                throw new ArgumentException(
                    "Hex string must have even number of characters",
                    nameof(hexString));
            }

            if (cleaned.Length > PMBusConstants.MAX_BLOCK_SIZE * 2)
            {
                throw new ArgumentException(
                    $"Hex string too long (max {PMBusConstants.MAX_BLOCK_SIZE} bytes)",
                    nameof(hexString));
            }

            // Validate all characters are hex digits
            if (!cleaned.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
            {
                throw new ArgumentException(
                    "Hex string contains invalid characters",
                    nameof(hexString));
            }
        }
    }
}
