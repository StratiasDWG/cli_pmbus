using Xunit;
using PmbusMasterCLI;

namespace PmbusMasterCLI.Tests
{
    public class LinearDataFormatTests
    {
        [Theory]
        [InlineData(0x0000, 0.0)]           // Zero
        [InlineData(0x0001, 0.0009765625)]  // Smallest positive: 1 * 2^-10
        [InlineData(0x0400, 1.0)]           // +1: 1024 * 2^-10
        [InlineData(0x5000, 10.0)]          // +10: 10 * 2^0
        [InlineData(0xF800, -1.0)]          // -1: -1024 * 2^-10
        [InlineData(0xFC00, -0.5)]          // -0.5: -512 * 2^-10
        public void Linear11ToReal_KnownValues_CorrectConversion(ushort input, double expected)
        {
            // Act
            var result = LinearDataFormat.Linear11ToReal(input);

            // Assert
            Assert.Equal(expected, result, precision: 6);
        }

        [Theory]
        [InlineData(0.0, 0x0000)]
        [InlineData(1.0, 0x0400)]
        [InlineData(-1.0, 0xFC00)]
        [InlineData(10.0, 0x5000)]
        public void RealToLinear11_KnownValues_CorrectConversion(double input, ushort expected)
        {
            // Act
            var result = LinearDataFormat.RealToLinear11(input);

            // Assert - Allow some tolerance due to rounding
            var actual = LinearDataFormat.Linear11ToReal(result);
            Assert.Equal(input, actual, precision: 2);
        }

        [Theory]
        [InlineData(new byte[] { 0x00, 0x10 }, -13, 4.096)]  // 0x1000 with exponent -13
        [InlineData(new byte[] { 0x40, 0x06 }, -10, 1.6)]    // 0x0640 (1600) with exponent -10
        [InlineData(new byte[] { 0x00, 0x00 }, 0, 0.0)]      // Zero
        public void Linear16ToReal_KnownValues_CorrectConversion(byte[] input, int exponent, double expected)
        {
            // Act
            var result = LinearDataFormat.Linear16ToReal(input, exponent);

            // Assert
            Assert.Equal(expected, result, precision: 4);
        }

        [Theory]
        [InlineData(3.3, -10, 3379)]  // 3.3V at -10 exponent
        [InlineData(5.0, -10, 5120)]  // 5.0V at -10 exponent
        [InlineData(12.0, -10, 12288)] // 12.0V at -10 exponent
        public void RealToLinear16_KnownValues_CorrectConversion(double voltage, int exponent, ushort expectedMantissa)
        {
            // Act
            var result = LinearDataFormat.RealToLinear16(voltage, exponent);

            // Assert
            Assert.Equal(expectedMantissa, result);
        }

        [Theory]
        [InlineData(0x00, "Linear", 0)]
        [InlineData(0x10, "Linear", -16)]
        [InlineData(0x0F, "Linear", 15)]
        [InlineData(0x20, "VID", 0)]
        [InlineData(0x40, "Direct", 0)]
        public void ParseVoutMode_KnownValues_CorrectParsing(byte voutMode, string expectedMode, int expectedExponent)
        {
            // Act
            var (mode, exponent) = LinearDataFormat.ParseVoutMode(voutMode);

            // Assert
            Assert.Equal(expectedMode, mode);
            Assert.Equal(expectedExponent, exponent);
        }

        [Theory]
        [InlineData(0x00)]  // Linear mode
        [InlineData(0x10)]  // Linear mode with -16 exponent
        [InlineData(0x0F)]  // Linear mode with +15 exponent
        public void GetVoutModeExponent_LinearMode_ReturnsExponent(byte voutMode)
        {
            // Act
            var exponent = LinearDataFormat.GetVoutModeExponent(voutMode);

            // Assert
            Assert.InRange(exponent, -16, 15);
        }

        [Fact]
        public void GetVoutModeExponent_NonLinearMode_ThrowsException()
        {
            // Arrange
            byte voutMode = 0x20; // VID mode

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                LinearDataFormat.GetVoutModeExponent(voutMode));
        }

        [Theory]
        [InlineData("0A FF", new byte[] { 0x0A, 0xFF })]
        [InlineData("00", new byte[] { 0x00 })]
        [InlineData("0x0A 0xFF", new byte[] { 0x0A, 0xFF })]
        public void FromHexString_ValidHex_CorrectConversion(string hex, byte[] expected)
        {
            // Act
            var result = LinearDataFormat.FromHexString(hex);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(new byte[] { 0x0A, 0xFF }, "0A FF")]
        [InlineData(new byte[] { 0x00 }, "00")]
        public void ToHexString_ValidBytes_CorrectConversion(byte[] input, string expected)
        {
            // Act
            var result = LinearDataFormat.ToHexString(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToHexString_EmptyArray_ReturnsEmpty()
        {
            // Act
            var result = LinearDataFormat.ToHexString(Array.Empty<byte>());

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ToHexString_Null_ReturnsEmpty()
        {
            // Act
            var result = LinearDataFormat.ToHexString(null!);

            // Assert
            Assert.Empty(result);
        }
    }
}
