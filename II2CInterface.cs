namespace PmbusMasterCLI
{
    /// <summary>
    /// Interface for I2C hardware communication
    /// Enables mocking for testing and support for different hardware adapters
    /// </summary>
    public interface II2CInterface : IDisposable
    {
        /// <summary>
        /// Indicates if the interface is connected to hardware
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Name of the connected device
        /// </summary>
        string DeviceName { get; }

        /// <summary>
        /// Connect to I2C interface hardware
        /// </summary>
        /// <param name="deviceName">Device name (null for auto-detect)</param>
        /// <param name="voltageLevel">I2C bus voltage level</param>
        /// <param name="clockRate">I2C clock rate in kHz</param>
        void Connect(string? deviceName = null, byte voltageLevel = 33, ushort clockRate = 100);

        /// <summary>
        /// Disconnect from hardware
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Set I2C slave address for subsequent operations
        /// </summary>
        /// <param name="address">7-bit I2C address</param>
        void SetSlaveAddress(byte address);

        /// <summary>
        /// Write data to I2C bus
        /// </summary>
        /// <param name="data">Data bytes to write</param>
        void Write(byte[] data);

        /// <summary>
        /// Write data and read response from I2C bus
        /// </summary>
        /// <param name="writeData">Data to write</param>
        /// <param name="readLength">Number of bytes to read</param>
        /// <returns>Bytes read from device</returns>
        byte[] WriteRead(byte[] writeData, int readLength);
    }
}
