using Xunit;
using PmbusMasterCLI;

namespace PmbusMasterCLI.Tests
{
    public class PMBusSafetyTests
    {
        [Theory]
        [InlineData(PMBusCommands.OPERATION)]
        [InlineData(PMBusCommands.VOUT_COMMAND)]
        [InlineData(PMBusCommands.CLEAR_FAULTS)]
        [InlineData(PMBusCommands.STORE_DEFAULT_ALL)]
        [InlineData(PMBusCommands.RESTORE_DEFAULT_ALL)]
        [InlineData(PMBusCommands.WRITE_PROTECT)]
        public void IsDangerousCommand_DangerousCommands_ReturnsTrue(byte commandCode)
        {
            // Act
            var result = PMBusSafety.IsDangerousCommand(commandCode);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(PMBusCommands.READ_VOUT)]
        [InlineData(PMBusCommands.READ_VIN)]
        [InlineData(PMBusCommands.STATUS_WORD)]
        [InlineData(PMBusCommands.MFR_ID)]
        [InlineData(PMBusCommands.PMBUS_REVISION)]
        public void IsDangerousCommand_SafeReadCommands_ReturnsFalse(byte commandCode)
        {
            // Act
            var result = PMBusSafety.IsDangerousCommand(commandCode);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(PMBusCommands.OPERATION)]
        [InlineData(PMBusCommands.VOUT_COMMAND)]
        [InlineData(PMBusCommands.CLEAR_FAULTS)]
        [InlineData(PMBusCommands.WRITE_PROTECT)]
        public void GetSafetyWarning_DangerousCommands_ReturnsWarningMessage(byte commandCode)
        {
            // Act
            var result = PMBusSafety.GetSafetyWarning(commandCode);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(result.Length > 20); // Should be a meaningful message
        }

        [Theory]
        [InlineData(3.3, 0.0, 100.0, true)]
        [InlineData(5.0, 0.0, 100.0, true)]
        [InlineData(12.0, 0.0, 100.0, true)]
        [InlineData(0.0, 0.0, 100.0, true)]
        [InlineData(-1.0, 0.0, 100.0, false)]
        [InlineData(150.0, 0.0, 100.0, false)]
        [InlineData(double.NaN, 0.0, 100.0, false)]
        [InlineData(double.PositiveInfinity, 0.0, 100.0, false)]
        [InlineData(double.NegativeInfinity, 0.0, 100.0, false)]
        public void IsVoltageInRange_VariousValues_ReturnsExpectedResult(
            double voltage, double min, double max, bool expected)
        {
            // Act
            var result = PMBusSafety.IsVoltageInRange(voltage, min, max);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(0x00)]
        [InlineData(0x01)]
        [InlineData(0x7F)]
        [InlineData(0x8B)]
        [InlineData(0xFF)]
        public void IsValidCommandCode_AllByteValues_ReturnsTrue(byte commandCode)
        {
            // Act
            var result = PMBusSafety.IsValidCommandCode(commandCode);

            // Assert
            Assert.True(result); // All bytes are potentially valid PMBus commands
        }

        [Fact]
        public void DangerousCommands_ContainsExpectedCommands()
        {
            // Assert
            Assert.Contains(PMBusCommands.OPERATION, PMBusSafety.DangerousCommands);
            Assert.Contains(PMBusCommands.VOUT_COMMAND, PMBusSafety.DangerousCommands);
            Assert.Contains(PMBusCommands.CLEAR_FAULTS, PMBusSafety.DangerousCommands);
            Assert.Contains(PMBusCommands.STORE_DEFAULT_ALL, PMBusSafety.DangerousCommands);
            Assert.Contains(PMBusCommands.RESTORE_DEFAULT_ALL, PMBusSafety.DangerousCommands);
        }

        [Fact]
        public void WriteResult_Success_ToStringReturnsSuccessMessage()
        {
            // Arrange
            var result = new WriteResult
            {
                Success = true,
                VerificationPassed = true
            };

            // Act
            var message = result.ToString();

            // Assert
            Assert.Contains("successful", message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("verified", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void WriteResult_Failure_ToStringReturnsFailureMessage()
        {
            // Arrange
            var result = new WriteResult
            {
                Success = false,
                Message = "Device not responding"
            };

            // Act
            var message = result.ToString();

            // Assert
            Assert.Contains("failed", message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Device not responding", message);
        }
    }
}
