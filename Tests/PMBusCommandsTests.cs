using Xunit;
using PmbusMasterCLI;

namespace PmbusMasterCLI.Tests
{
    public class PMBusCommandsTests
    {
        [Theory]
        [InlineData(PMBusCommands.READ_VOUT, "READ_VOUT")]
        [InlineData(PMBusCommands.READ_VIN, "READ_VIN")]
        [InlineData(PMBusCommands.READ_IOUT, "READ_IOUT")]
        [InlineData(PMBusCommands.READ_TEMPERATURE_1, "READ_TEMPERATURE_1")]
        [InlineData(PMBusCommands.STATUS_WORD, "STATUS_WORD")]
        [InlineData(PMBusCommands.OPERATION, "OPERATION")]
        [InlineData(PMBusCommands.VOUT_COMMAND, "VOUT_COMMAND")]
        public void GetCommandName_KnownCommands_ReturnsCorrectName(byte commandCode, string expectedName)
        {
            // Act
            var result = PMBusCommands.GetCommandName(commandCode);

            // Assert
            Assert.Equal(expectedName, result);
        }

        [Fact]
        public void GetCommandName_UnknownCommand_ReturnsHexCode()
        {
            // Arrange
            byte unknownCommand = 0xAB;

            // Act
            var result = PMBusCommands.GetCommandName(unknownCommand);

            // Assert
            Assert.Equal("0xAB", result);
        }

        [Theory]
        [InlineData(PMBusCommands.PAGE, 0x00)]
        [InlineData(PMBusCommands.OPERATION, 0x01)]
        [InlineData(PMBusCommands.CLEAR_FAULTS, 0x03)]
        [InlineData(PMBusCommands.VOUT_MODE, 0x20)]
        [InlineData(PMBusCommands.VOUT_COMMAND, 0x21)]
        [InlineData(PMBusCommands.STATUS_WORD, 0x79)]
        [InlineData(PMBusCommands.READ_VOUT, 0x8B)]
        public void CommandCodes_HaveCorrectValues(byte actualValue, byte expectedValue)
        {
            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void StatusCommands_InCorrectRange()
        {
            // Assert - Status commands are 0x78-0x82
            Assert.InRange(PMBusCommands.STATUS_BYTE, (byte)0x78, (byte)0x82);
            Assert.InRange(PMBusCommands.STATUS_WORD, (byte)0x78, (byte)0x82);
            Assert.InRange(PMBusCommands.STATUS_VOUT, (byte)0x78, (byte)0x82);
            Assert.InRange(PMBusCommands.STATUS_IOUT, (byte)0x78, (byte)0x82);
        }

        [Fact]
        public void ReadCommands_InCorrectRange()
        {
            // Assert - Read commands are typically 0x86-0x97
            Assert.InRange(PMBusCommands.READ_VIN, (byte)0x86, (byte)0x97);
            Assert.InRange(PMBusCommands.READ_VOUT, (byte)0x86, (byte)0x97);
            Assert.InRange(PMBusCommands.READ_IOUT, (byte)0x86, (byte)0x97);
            Assert.InRange(PMBusCommands.READ_TEMPERATURE_1, (byte)0x86, (byte)0x97);
            Assert.InRange(PMBusCommands.READ_POUT, (byte)0x86, (byte)0x97);
        }

        [Fact]
        public void ManufacturerCommands_InCorrectRange()
        {
            // Assert - Manufacturer commands are 0x98-0x9E
            Assert.InRange(PMBusCommands.PMBUS_REVISION, (byte)0x98, (byte)0x9E);
            Assert.InRange(PMBusCommands.MFR_ID, (byte)0x98, (byte)0x9E);
            Assert.InRange(PMBusCommands.MFR_MODEL, (byte)0x98, (byte)0x9E);
            Assert.InRange(PMBusCommands.MFR_SERIAL, (byte)0x98, (byte)0x9E);
        }
    }
}
