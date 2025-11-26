using System.Text.Json;

namespace PmbusMasterCLI
{
    /// <summary>
    /// Application configuration settings
    /// </summary>
    public class AppConfiguration
    {
        /// <summary>
        /// Default PMBus device address
        /// </summary>
        public byte DefaultAddress { get; set; } = 0x58;

        /// <summary>
        /// Default I2C bus voltage level (1.2V-3.3V)
        /// </summary>
        public byte DefaultVoltageLevel { get; set; } = NI8451Interface.kNi845x33Volts;

        /// <summary>
        /// Default I2C clock rate in kHz
        /// </summary>
        public ushort DefaultClockRate { get; set; } = 100;

        /// <summary>
        /// Default monitoring interval in milliseconds
        /// </summary>
        public int DefaultMonitorInterval { get; set; } = 1000;

        /// <summary>
        /// Maximum retry attempts for I2C operations
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Initial retry delay in milliseconds (exponential backoff)
        /// </summary>
        public int RetryDelayMs { get; set; } = 10;

        /// <summary>
        /// Operation timeout in milliseconds
        /// </summary>
        public int OperationTimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Enable color output in console
        /// </summary>
        public bool EnableColorOutput { get; set; } = true;

        /// <summary>
        /// Require confirmation for dangerous write operations
        /// </summary>
        public bool RequireWriteConfirmation { get; set; } = true;

        /// <summary>
        /// Enable write operation verification (readback)
        /// </summary>
        public bool VerifyWrites { get; set; } = true;

        /// <summary>
        /// Check write protection before write operations
        /// </summary>
        public bool CheckWriteProtection { get; set; } = true;

        /// <summary>
        /// Default output format (Table, Json, Csv)
        /// </summary>
        public string DefaultOutputFormat { get; set; } = "Table";

        /// <summary>
        /// Enable verbose logging
        /// </summary>
        public bool VerboseMode { get; set; } = false;

        /// <summary>
        /// Cache device metadata
        /// </summary>
        public bool EnableDeviceCache { get; set; } = true;

        /// <summary>
        /// Device cache expiration in hours
        /// </summary>
        public int CacheExpirationHours { get; set; } = 24;

        private static string ConfigFilePath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        ".pmbus-cli", "config.json");

        /// <summary>
        /// Load configuration from file
        /// </summary>
        public static AppConfiguration Load()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    var config = JsonSerializer.Deserialize<AppConfiguration>(json);
                    return config ?? new AppConfiguration();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load configuration: {ex.Message}");
                Console.WriteLine("Using default configuration.");
            }

            return new AppConfiguration();
        }

        /// <summary>
        /// Save configuration to file
        /// </summary>
        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(ConfigFilePath)!;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to save configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Get configuration file path
        /// </summary>
        public static string GetConfigPath() => ConfigFilePath;

        /// <summary>
        /// Reset to default configuration
        /// </summary>
        public static void ResetToDefaults()
        {
            var config = new AppConfiguration();
            config.Save();
        }

        /// <summary>
        /// Validate configuration values
        /// </summary>
        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            if (DefaultAddress > 0x7F)
                errors.Add("DefaultAddress must be 7-bit (0x00-0x7F)");

            if (DefaultClockRate < 10 || DefaultClockRate > 400)
                errors.Add("DefaultClockRate must be between 10 and 400 kHz");

            if (DefaultMonitorInterval < 100)
                errors.Add("DefaultMonitorInterval must be at least 100 ms");

            if (MaxRetries < 0 || MaxRetries > 10)
                errors.Add("MaxRetries must be between 0 and 10");

            if (RetryDelayMs < 0 || RetryDelayMs > 1000)
                errors.Add("RetryDelayMs must be between 0 and 1000 ms");

            if (OperationTimeoutMs < 100 || OperationTimeoutMs > 30000)
                errors.Add("OperationTimeoutMs must be between 100 and 30000 ms");

            if (!new[] { "Table", "Json", "Csv" }.Contains(DefaultOutputFormat, StringComparer.OrdinalIgnoreCase))
                errors.Add("DefaultOutputFormat must be Table, Json, or Csv");

            return errors.Count == 0;
        }

        /// <summary>
        /// Print current configuration
        /// </summary>
        public void Print()
        {
            Console.WriteLine("\nCurrent Configuration:");
            Console.WriteLine(new string('─', 60));
            Console.WriteLine($"  Config File:              {GetConfigPath()}");
            Console.WriteLine($"  Default Address:          0x{DefaultAddress:X2}");
            Console.WriteLine($"  Default Voltage Level:    {DefaultVoltageLevel / 10.0:F1}V");
            Console.WriteLine($"  Default Clock Rate:       {DefaultClockRate} kHz");
            Console.WriteLine($"  Monitor Interval:         {DefaultMonitorInterval} ms");
            Console.WriteLine($"  Max Retries:              {MaxRetries}");
            Console.WriteLine($"  Retry Delay:              {RetryDelayMs} ms");
            Console.WriteLine($"  Operation Timeout:        {OperationTimeoutMs} ms");
            Console.WriteLine($"  Color Output:             {EnableColorOutput}");
            Console.WriteLine($"  Require Write Confirm:    {RequireWriteConfirmation}");
            Console.WriteLine($"  Verify Writes:            {VerifyWrites}");
            Console.WriteLine($"  Check Write Protection:   {CheckWriteProtection}");
            Console.WriteLine($"  Default Output Format:    {DefaultOutputFormat}");
            Console.WriteLine($"  Verbose Mode:             {VerboseMode}");
            Console.WriteLine($"  Device Cache:             {EnableDeviceCache}");
            Console.WriteLine($"  Cache Expiration:         {CacheExpirationHours} hours");
            Console.WriteLine(new string('─', 60));
        }
    }
}
