using Xunit;
using PmbusMasterCLI;

namespace PmbusMasterCLI.Tests
{
    public class PMBusConstantsTests
    {
        [Theory]
        [InlineData(12, true)]
        [InlineData(15, true)]
        [InlineData(18, true)]
        [InlineData(25, true)]
        [InlineData(33, true)]
        [InlineData(0, false)]
        [InlineData(10, false)]
        [InlineData(99, false)]
        public void IsValidVoltageLevel_VariousLevels_ReturnsExpected(byte level, bool expected)
        {
            // Act
            var result = PMBusConstants.IsValidVoltageLevel(level);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(0x00, true)]
        [InlineData(0x10, true)]
        [InlineData(0x58, true)]
        [InlineData(0x7F, true)]
        [InlineData(0x80, false)]
        [InlineData(0xFF, false)]
        [InlineData(-1, false)]
        [InlineData(128, false)]
        public void IsValidI2CAddress_VariousAddresses_ReturnsExpected(int address, bool expected)
        {
            // Act
            var result = PMBusConstants.IsValidI2CAddress(address);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(10, true)]
        [InlineData(100, true)]
        [InlineData(400, true)]
        [InlineData(0, false)]
        [InlineData(9, false)]
        [InlineData(401, false)]
        [InlineData(1000, false)]
        public void IsValidClockRate_VariousRates_ReturnsExpected(int clockRate, bool expected)
        {
            // Act
            var result = PMBusConstants.IsValidClockRate(clockRate);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(0.0, true)]
        [InlineData(3.3, true)]
        [InlineData(100.0, true)]
        [InlineData(-1.0, false)]
        [InlineData(100.001, false)]
        public void IsValidVoltage_DefaultRange_ReturnsExpected(double voltage, bool expected)
        {
            // Act
            var result = PMBusConstants.IsValidVoltage(voltage);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsValidVoltage_NaN_ReturnsFalse()
        {
            // Act
            var result = PMBusConstants.IsValidVoltage(double.NaN);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void IsValidVoltage_Infinity_ReturnsFalse(double voltage)
        {
            // Act
            var result = PMBusConstants.IsValidVoltage(voltage);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("Table", true)]
        [InlineData("Json", true)]
        [InlineData("Csv", true)]
        [InlineData("table", true)]  // Case insensitive
        [InlineData("JSON", true)]
        [InlineData("XML", false)]
        [InlineData("Invalid", false)]
        public void IsValidOutputFormat_VariousFormats_ReturnsExpected(string format, bool expected)
        {
            // Act
            var result = PMBusConstants.IsValidOutputFormat(format);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void VALID_VOLTAGE_LEVELS_ContainsExpectedValues()
        {
            // Assert
            Assert.Contains((byte)12, PMBusConstants.VALID_VOLTAGE_LEVELS);
            Assert.Contains((byte)15, PMBusConstants.VALID_VOLTAGE_LEVELS);
            Assert.Contains((byte)18, PMBusConstants.VALID_VOLTAGE_LEVELS);
            Assert.Contains((byte)25, PMBusConstants.VALID_VOLTAGE_LEVELS);
            Assert.Contains((byte)33, PMBusConstants.VALID_VOLTAGE_LEVELS);
            Assert.Equal(5, PMBusConstants.VALID_VOLTAGE_LEVELS.Length);
        }

        [Fact]
        public void VALID_OUTPUT_FORMATS_ContainsExpectedValues()
        {
            // Assert
            Assert.Contains("Table", PMBusConstants.VALID_OUTPUT_FORMATS);
            Assert.Contains("Json", PMBusConstants.VALID_OUTPUT_FORMATS);
            Assert.Contains("Csv", PMBusConstants.VALID_OUTPUT_FORMATS);
            Assert.Equal(3, PMBusConstants.VALID_OUTPUT_FORMATS.Length);
        }

        [Fact]
        public void VOUT_MODE_NAMES_ContainsExpectedValues()
        {
            // Assert
            Assert.Equal("Linear", PMBusConstants.VOUT_MODE_NAMES[0]);
            Assert.Equal("VID", PMBusConstants.VOUT_MODE_NAMES[1]);
            Assert.Equal("Direct", PMBusConstants.VOUT_MODE_NAMES[2]);
            Assert.Equal("IEEE Half-Precision", PMBusConstants.VOUT_MODE_NAMES[3]);
        }

        [Fact]
        public void AddressConstants_HaveCorrectValues()
        {
            // Assert
            Assert.Equal(0x00, PMBusConstants.MIN_I2C_ADDRESS);
            Assert.Equal(0x7F, PMBusConstants.MAX_I2C_ADDRESS);
            Assert.Equal(0x58, PMBusConstants.DEFAULT_PMBUS_ADDRESS);
        }

        [Fact]
        public void ClockRateConstants_HaveCorrectValues()
        {
            // Assert
            Assert.Equal(10, PMBusConstants.MIN_CLOCK_RATE_KHZ);
            Assert.Equal(400, PMBusConstants.MAX_CLOCK_RATE_KHZ);
            Assert.Equal(100, PMBusConstants.DEFAULT_CLOCK_RATE_KHZ);
            Assert.Equal(100, PMBusConstants.STANDARD_MODE_CLOCK_KHZ);
            Assert.Equal(400, PMBusConstants.FAST_MODE_CLOCK_KHZ);
        }

        [Fact]
        public void TimeoutConstants_HaveCorrectValues()
        {
            // Assert
            Assert.Equal(5000, PMBusConstants.DEFAULT_OPERATION_TIMEOUT_MS);
            Assert.Equal(100, PMBusConstants.MIN_OPERATION_TIMEOUT_MS);
            Assert.Equal(30000, PMBusConstants.MAX_OPERATION_TIMEOUT_MS);
        }

        [Fact]
        public void VoltageConstants_HaveCorrectValues()
        {
            // Assert
            Assert.Equal(0.0, PMBusConstants.MIN_VOLTAGE);
            Assert.Equal(100.0, PMBusConstants.MAX_VOLTAGE);
        }

        [Fact]
        public void LINEAR11_Constants_HaveCorrectValues()
        {
            // Assert
            Assert.Equal(11, PMBusConstants.LINEAR11_MANTISSA_BITS);
            Assert.Equal(5, PMBusConstants.LINEAR11_EXPONENT_BITS);
            Assert.Equal(0x7FF, PMBusConstants.LINEAR11_MANTISSA_MASK);
            Assert.Equal(0x400, PMBusConstants.LINEAR11_MANTISSA_SIGN_BIT);
            Assert.Equal(11, PMBusConstants.LINEAR11_EXPONENT_SHIFT);
        }

        [Fact]
        public void BlockConstants_HaveCorrectValues()
        {
            // Assert
            Assert.Equal(255, PMBusConstants.MAX_BLOCK_SIZE);
            Assert.Equal(255, PMBusConstants.MAX_BLOCK_READ_SIZE);
        }
    }
}
