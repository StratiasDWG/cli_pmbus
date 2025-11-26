using System.Text;
using System.Text.Json;

namespace PmbusMasterCLI
{
    /// <summary>
    /// Output format options
    /// </summary>
    public enum OutputFormat
    {
        Table,
        Json,
        Csv
    }

    /// <summary>
    /// Handles formatting output in different formats
    /// </summary>
    public static class OutputFormatter
    {
        /// <summary>
        /// Format device information
        /// </summary>
        public static string FormatDeviceInfo(Dictionary<string, string> info, OutputFormat format)
        {
            return format switch
            {
                OutputFormat.Json => JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true }),
                OutputFormat.Csv => FormatCsv(info),
                _ => FormatTable(info)
            };
        }

        /// <summary>
        /// Format telemetry data
        /// </summary>
        public static string FormatTelemetry(Dictionary<string, object> data, OutputFormat format, bool includeHeader = false)
        {
            return format switch
            {
                OutputFormat.Json => JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }),
                OutputFormat.Csv => FormatTelemetryCsv(data, includeHeader),
                _ => FormatTelemetryTable(data)
            };
        }

        /// <summary>
        /// Format scan results
        /// </summary>
        public static string FormatScanResults(List<DeviceInfo> devices, OutputFormat format)
        {
            return format switch
            {
                OutputFormat.Json => JsonSerializer.Serialize(new { devices, count = devices.Count },
                    new JsonSerializerOptions { WriteIndented = true }),
                OutputFormat.Csv => FormatDevicesCsv(devices),
                _ => FormatDevicesTable(devices)
            };
        }

        private static string FormatTable(Dictionary<string, string> info)
        {
            var sb = new StringBuilder();
            int maxKeyLength = info.Keys.Max(k => k.Length);

            foreach (var kvp in info)
            {
                sb.AppendLine($"  {kvp.Key.PadRight(maxKeyLength + 2)}: {kvp.Value}");
            }

            return sb.ToString();
        }

        private static string FormatCsv(Dictionary<string, string> info)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Field,Value");

            foreach (var kvp in info)
            {
                sb.AppendLine($"\"{EscapeCsv(kvp.Key)}\",\"{EscapeCsv(kvp.Value)}\"");
            }

            return sb.ToString();
        }

        private static string FormatTelemetryTable(Dictionary<string, object> data)
        {
            var sb = new StringBuilder();

            foreach (var kvp in data)
            {
                string value = FormatValue(kvp.Value);
                sb.AppendLine($"  {kvp.Key,-20}: {value}");
            }

            return sb.ToString();
        }

        private static string FormatTelemetryCsv(Dictionary<string, object> data, bool includeHeader)
        {
            var sb = new StringBuilder();

            if (includeHeader)
            {
                sb.AppendLine(string.Join(",", data.Keys.Select(k => $"\"{EscapeCsv(k)}\"")));
            }

            sb.AppendLine(string.Join(",", data.Values.Select(v => FormatCsvValue(v))));

            return sb.ToString();
        }

        private static string FormatDevicesTable(List<DeviceInfo> devices)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"{"Address",-10} {"Model",-30} {"Manufacturer",-20}");
            sb.AppendLine(new string('─', 62));

            foreach (var dev in devices)
            {
                sb.AppendLine($"0x{dev.Address:X2}      {dev.Model,-30} {dev.Manufacturer,-20}");
            }

            return sb.ToString();
        }

        private static string FormatDevicesCsv(List<DeviceInfo> devices)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Address,Model,Manufacturer,Revision,Serial");

            foreach (var dev in devices)
            {
                sb.AppendLine($"0x{dev.Address:X2},\"{EscapeCsv(dev.Model)}\",\"{EscapeCsv(dev.Manufacturer)}\",\"{EscapeCsv(dev.Revision)}\",\"{EscapeCsv(dev.Serial)}\"");
            }

            return sb.ToString();
        }

        private static string FormatValue(object value)
        {
            return value switch
            {
                double d => $"{d:F3}",
                float f => $"{f:F3}",
                int i => i.ToString(),
                long l => l.ToString(),
                byte b => $"0x{b:X2}",
                _ => value?.ToString() ?? "N/A"
            };
        }

        private static string FormatCsvValue(object value)
        {
            return value switch
            {
                double d => $"{d:F6}",
                float f => $"{f:F6}",
                _ => $"\"{EscapeCsv(value?.ToString() ?? "")}\""
            };
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return value.Replace("\"", "\"\"");
        }

        /// <summary>
        /// Format status word with color coding
        /// </summary>
        public static void PrintStatus(StatusWordFlags status, bool useColor = true)
        {
            Console.WriteLine($"  STATUS_WORD: 0x{status.RawValue:X4}");

            if (useColor)
            {
                if (status.RawValue == 0 || status.ToString() == "OK")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  Status: {status}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  Status: {status}");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine($"  Status: {status}");
            }
        }

        /// <summary>
        /// Print a header/separator line
        /// </summary>
        public static void PrintHeader(string title, int width = 60)
        {
            Console.WriteLine();
            Console.WriteLine(new string('═', width));
            Console.WriteLine($"  {title}");
            Console.WriteLine(new string('═', width));
            Console.WriteLine();
        }

        /// <summary>
        /// Print an error message
        /// </summary>
        public static void PrintError(string message, bool useColor = true)
        {
            if (useColor)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error: {message}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"Error: {message}");
            }
        }

        /// <summary>
        /// Print a success message
        /// </summary>
        public static void PrintSuccess(string message, bool useColor = true)
        {
            if (useColor)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ {message}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Print a warning message
        /// </summary>
        public static void PrintWarning(string message, bool useColor = true)
        {
            if (useColor)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠ Warning: {message}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"Warning: {message}");
            }
        }
    }

    /// <summary>
    /// Device information for scan results
    /// </summary>
    public class DeviceInfo
    {
        public byte Address { get; set; }
        public string Model { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string Serial { get; set; } = string.Empty;
    }
}
