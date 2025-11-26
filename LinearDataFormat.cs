namespace PmbusMasterCLI
{
    /// <summary>
    /// Handles PMBus Linear data format conversions (LINEAR11 and LINEAR16)
    /// </summary>
    public static class LinearDataFormat
    {
        /// <summary>
        /// Convert LINEAR11 format (2 bytes) to real value
        /// Format: 5-bit two's complement exponent, 11-bit two's complement mantissa
        /// </summary>
        public static double Linear11ToReal(byte[] data)
        {
            if (data.Length < 2)
                throw new ArgumentException("LINEAR11 requires 2 bytes");

            ushort rawValue = (ushort)(data[0] | (data[1] << 8));
            return Linear11ToReal(rawValue);
        }

        /// <summary>
        /// Convert LINEAR11 format (16-bit value) to real value
        /// </summary>
        public static double Linear11ToReal(ushort rawValue)
        {
            // Extract exponent (upper 5 bits)
            int exponent = (short)(rawValue >> 11);
            // Sign extend the 5-bit exponent
            if ((exponent & 0x10) != 0)
                exponent |= unchecked((int)0xFFFFFFE0);

            // Extract mantissa (lower 11 bits)
            int mantissa = (short)(rawValue & 0x7FF);
            // Sign extend the 11-bit mantissa
            if ((mantissa & 0x400) != 0)
                mantissa |= unchecked((int)0xFFFFF800);

            // Calculate real value: mantissa * 2^exponent
            return mantissa * Math.Pow(2, exponent);
        }

        /// <summary>
        /// Convert real value to LINEAR11 format
        /// </summary>
        public static ushort RealToLinear11(double value)
        {
            // Handle zero case
            if (Math.Abs(value) < 1e-10)
                return 0;

            // Find optimal exponent
            int exponent = 0;
            double absValue = Math.Abs(value);

            // Scale to fit in 11-bit mantissa range (-1024 to 1023)
            while (absValue < 512 && exponent > -16)
            {
                absValue *= 2;
                exponent--;
            }

            while (absValue >= 1024 && exponent < 15)
            {
                absValue /= 2;
                exponent++;
            }

            // Round mantissa
            int mantissa = (int)Math.Round(value / Math.Pow(2, exponent));

            // Clamp mantissa to 11-bit range
            if (mantissa > 1023) mantissa = 1023;
            if (mantissa < -1024) mantissa = -1024;

            // Pack into 16-bit value
            ushort result = (ushort)(((exponent & 0x1F) << 11) | (mantissa & 0x7FF));
            return result;
        }

        /// <summary>
        /// Convert LINEAR16 format to real value using separate exponent
        /// Format: 16-bit two's complement mantissa with separate exponent (from VOUT_MODE)
        /// </summary>
        public static double Linear16ToReal(byte[] data, int exponent)
        {
            if (data.Length < 2)
                throw new ArgumentException("LINEAR16 requires 2 bytes");

            // Extract 16-bit signed mantissa
            short mantissa = (short)(data[0] | (data[1] << 8));

            // Calculate real value: mantissa * 2^exponent
            return mantissa * Math.Pow(2, exponent);
        }

        /// <summary>
        /// Convert real value to LINEAR16 format using specified exponent
        /// </summary>
        public static ushort RealToLinear16(double value, int exponent)
        {
            // Calculate mantissa: value / 2^exponent
            int mantissa = (int)Math.Round(value / Math.Pow(2, exponent));

            // Clamp to 16-bit signed range
            if (mantissa > 32767) mantissa = 32767;
            if (mantissa < -32768) mantissa = -32768;

            return (ushort)mantissa;
        }

        /// <summary>
        /// Extract VOUT_MODE exponent (lower 5 bits)
        /// </summary>
        public static int GetVoutModeExponent(byte voutMode)
        {
            // Mode is in upper 3 bits, exponent in lower 5 bits
            int mode = (voutMode >> 5) & 0x07;

            if (mode != 0) // Only Linear mode (0) uses exponent
                throw new InvalidOperationException($"VOUT_MODE mode {mode} is not Linear format");

            // Extract and sign-extend 5-bit exponent
            int exponent = voutMode & 0x1F;
            if ((exponent & 0x10) != 0)
                exponent |= unchecked((int)0xFFFFFFE0);

            return exponent;
        }

        /// <summary>
        /// Parse VOUT_MODE to determine format and exponent
        /// </summary>
        public static (string Mode, int Exponent) ParseVoutMode(byte voutMode)
        {
            int mode = (voutMode >> 5) & 0x07;
            int exponent = voutMode & 0x1F;

            // Sign extend exponent
            if ((exponent & 0x10) != 0)
                exponent |= unchecked((int)0xFFFFFFE0);

            string modeName = mode switch
            {
                0 => "Linear",
                1 => "VID",
                2 => "Direct",
                3 => "IEEE Half-Precision",
                _ => $"Reserved ({mode})"
            };

            return (modeName, exponent);
        }

        /// <summary>
        /// Convert byte array to hex string for display
        /// </summary>
        public static string ToHexString(byte[] data)
        {
            if (data == null || data.Length == 0)
                return "";

            return string.Join(" ", data.Select(b => $"{b:X2}"));
        }

        /// <summary>
        /// Parse hex string to byte array
        /// </summary>
        public static byte[] FromHexString(string hex)
        {
            hex = hex.Replace(" ", "").Replace("0x", "");

            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must have even number of characters");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }
    }
}
