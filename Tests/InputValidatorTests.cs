using Xunit;
using PmbusMasterCLI;
using System;

namespace PmbusMasterCLI.Tests
{
    public class InputValidatorTests
    {
        #region Voltage Validation Tests

        [Theory]
        [InlineData(3.3)]
        [InlineData(5.0)]
        [InlineData(12.0)]
        [InlineData(0.0)]
        [InlineData(100.0)]
        public void ValidateVoltage_ValidValues_DoesNotThrow(double voltage)
        {
            // Act & Assert - should not throw
            var exception = Record.Exception(() => InputValidator.ValidateVoltage(voltage));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(-0.001)]
        [InlineData(100.001)]
        [InlineData(999.9)]
        public void ValidateVoltage_OutOfRange_ThrowsPMBusVoltageOutOfRangeException(double voltage)
        {
            // Act & Assert
            var exception = Assert.Throws<PMBusVoltageOutOfRangeException>(() =>
                InputValidator.ValidateVoltage(voltage));

            Assert.Equal(voltage, exception.RequestedVoltage);
            Assert.Equal(PMBusConstants.MIN_VOLTAGE, exception.MinVoltage);
            Assert.Equal(PMBusConstants.MAX_VOLTAGE, exception.MaxVoltage);
        }

        [Fact]
        public void ValidateVoltage_NaN_ThrowsPMBusVoltageOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<PMBusVoltageOutOfRangeException>(() =>
                InputValidator.ValidateVoltage(double.NaN));
        }

        [Theory]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ValidateVoltage_Infinity_ThrowsPMBusVoltageOutOfRangeException(double voltage)
        {
            // Act & Assert
            Assert.Throws<PMBusVoltageOutOfRangeException>(() =>
                InputValidator.ValidateVoltage(voltage));
        }

        [Fact]
        public void ValidateVoltage_CustomRange_ValidatesCorrectly()
        {
            // Arrange
            double voltage = 10.0;

            // Act & Assert - should not throw within custom range
            var exception1 = Record.Exception(() =>
                InputValidator.ValidateVoltage(voltage, 0.0, 15.0));
            Assert.Null(exception1);

            // Should throw outside custom range
            Assert.Throws<PMBusVoltageOutOfRangeException>(() =>
                InputValidator.ValidateVoltage(voltage, 0.0, 5.0));
        }

        #endregion

        #region Address Validation Tests

        [Theory]
        [InlineData(0x00)]
        [InlineData(0x10)]
        [InlineData(0x58)]
        [InlineData(0x7F)]
        public void ValidateAddress_ValidAddresses_DoesNotThrow(byte address)
        {
            // Act & Assert
            var exception = Record.Exception(() => InputValidator.ValidateAddress(address));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(0x80)]
        [InlineData(0xFF)]
        [InlineData(128)]
        [InlineData(255)]
        public void ValidateAddress_InvalidAddresses_ThrowsPMBusInvalidAddressException(int address)
        {
            // Act & Assert
            var exception = Assert.Throws<PMBusInvalidAddressException>(() =>
                InputValidator.ValidateAddress(address));

            Assert.Equal(address, exception.InvalidAddress);
        }

        #endregion

        #region Clock Rate Validation Tests

        [Theory]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(400)]
        public void ValidateClockRate_ValidRates_DoesNotThrow(ushort clockRate)
        {
            // Act & Assert
            var exception = Record.Exception(() => InputValidator.ValidateClockRate(clockRate));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(9)]
        [InlineData(401)]
        [InlineData(1000)]
        public void ValidateClockRate_InvalidRates_ThrowsArgumentOutOfRangeException(int clockRate)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                InputValidator.ValidateClockRate(clockRate));
        }

        #endregion

        #region Voltage Level Validation Tests

        [Theory]
        [InlineData(12)]
        [InlineData(15)]
        [InlineData(18)]
        [InlineData(25)]
        [InlineData(33)]
        public void ValidateVoltageLevel_ValidLevels_DoesNotThrow(byte level)
        {
            // Act & Assert
            var exception = Record.Exception(() => InputValidator.ValidateVoltageLevel(level));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(99)]
        public void ValidateVoltageLevel_InvalidLevels_ThrowsArgumentException(byte level)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                InputValidator.ValidateVoltageLevel(level));

            Assert.Contains("Valid levels", exception.Message);
        }

        #endregion

        #region Monitor Interval Validation Tests

        [Theory]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(60000)]
        public void ValidateMonitorInterval_ValidIntervals_DoesNotThrow(int interval)
        {
            // Act & Assert
            var exception = Record.Exception(() => InputValidator.ValidateMonitorInterval(interval));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(99)]
        public void ValidateMonitorInterval_TooSmall_ThrowsArgumentOutOfRangeException(int interval)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                InputValidator.ValidateMonitorInterval(interval));

            Assert.Contains("at least", exception.Message);
        }

        [Theory]
        [InlineData(60001)]
        [InlineData(100000)]
        public void ValidateMonitorInterval_TooLarge_ThrowsArgumentOutOfRangeException(int interval)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                InputValidator.ValidateMonitorInterval(interval));

            Assert.Contains("cannot exceed", exception.Message);
        }

        #endregion

        #region Timeout Validation Tests

        [Theory]
        [InlineData(100)]
        [InlineData(5000)]
        [InlineData(30000)]
        public void ValidateTimeout_ValidTimeouts_DoesNotThrow(int timeout)
        {
            // Act & Assert
            var exception = Record.Exception(() => InputValidator.ValidateTimeout(timeout));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(99)]
        [InlineData(30001)]
        public void ValidateTimeout_InvalidTimeouts_ThrowsArgumentOutOfRangeException(int timeout)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                InputValidator.ValidateTimeout(timeout));
        }

        #endregion

        #region Retry Validation Tests

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(10)]
        public void ValidateRetryCount_ValidCounts_DoesNotThrow(int retries)
        {
            // Act & Assert
            var exception = Record.Exception(() => InputValidator.ValidateRetryCount(retries));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(11)]
        public void ValidateRetryCount_InvalidCounts_ThrowsArgumentOutOfRangeException(int retries)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                InputValidator.ValidateRetryCount(retries));
        }

        #endregion

        #region Block Data Validation Tests

        [Fact]
        public void ValidateBlockData_ValidData_DoesNotThrow()
        {
            // Arrange
            byte[] data = new byte[] { 0x01, 0x02, 0x03 };

            // Act & Assert
            var exception = Record.Exception(() => InputValidator.ValidateBlockData(data));
            Assert.Null(exception);
        }

        [Fact]
        public void ValidateBlockData_NullData_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                InputValidator.ValidateBlockData(null));
        }

        [Fact]
        public void ValidateBlockData_EmptyData_ThrowsArgumentException()
        {
            // Arrange
            byte[] data = Array.Empty<byte>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                InputValidator.ValidateBlockData(data));
        }

        [Fact]
        public void ValidateBlockData_TooLarge_ThrowsArgumentException()
        {
            // Arrange
            byte[] data = new byte[256];

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                InputValidator.ValidateBlockData(data));

            Assert.Contains("cannot exceed 255", exception.Message);
        }

        #endregion

        #region Hex String Validation Tests

        [Theory]
        [InlineData("0A")]
        [InlineData("0A FF")]
        [InlineData("0x0A0xFF")]
        [InlineData("DEADBEEF")]
        public void ValidateHexString_ValidStrings_DoesNotThrow(string hexString)
        {
            // Act & Assert
            var exception = Record.Exception(() => InputValidator.ValidateHexString(hexString));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void ValidateHexString_NullOrEmpty_ThrowsArgumentException(string hexString)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                InputValidator.ValidateHexString(hexString));
        }

        [Theory]
        [InlineData("0")]  // Odd length
        [InlineData("ABC")]  // Odd length
        public void ValidateHexString_OddLength_ThrowsArgumentException(string hexString)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                InputValidator.ValidateHexString(hexString));

            Assert.Contains("even number", exception.Message);
        }

        [Theory]
        [InlineData("0G")]  // Invalid character
        [InlineData("XY")]  // Invalid characters
        [InlineData("0A ZZ")]  // Invalid characters
        public void ValidateHexString_InvalidCharacters_ThrowsArgumentException(string hexString)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                InputValidator.ValidateHexString(hexString));

            Assert.Contains("invalid characters", exception.Message);
        }

        #endregion

        #region Output Format Validation Tests

        [Theory]
        [InlineData("Table")]
        [InlineData("Json")]
        [InlineData("Csv")]
        [InlineData("table")]  // Case insensitive
        [InlineData("JSON")]
        public void ValidateOutputFormat_ValidFormats_DoesNotThrow(string format)
        {
            // Act & Assert
            var exception = Record.Exception(() => InputValidator.ValidateOutputFormat(format));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("XML")]
        [InlineData("Invalid")]
        public void ValidateOutputFormat_InvalidFormats_ThrowsArgumentException(string format)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                InputValidator.ValidateOutputFormat(format));

            Assert.Contains("Valid formats", exception.Message);
        }

        #endregion

        #region Read Length Validation Tests

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(255)]
        public void ValidateReadLength_ValidLengths_DoesNotThrow(int length)
        {
            // Act & Assert
            var exception = Record.Exception(() => InputValidator.ValidateReadLength(length));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ValidateReadLength_ZeroOrNegative_ThrowsArgumentOutOfRangeException(int length)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                InputValidator.ValidateReadLength(length));

            Assert.Contains("greater than zero", exception.Message);
        }

        [Fact]
        public void ValidateReadLength_TooLarge_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                InputValidator.ValidateReadLength(256));

            Assert.Contains("cannot exceed", exception.Message);
        }

        #endregion
    }
}
