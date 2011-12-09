namespace GitCommands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Encoding Helper
    /// </summary>
    public class EncodingHelper
    {
        public static string GetString(byte[] output, byte[] error, Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            StringBuilder sb = new StringBuilder();

            if (output != null && output.Length > 0)
            {
                sb.Append(encoding.GetString(output));
            }
            if (error != null && error.Length > 0 && output != null && output.Length > 0)
            {
                sb.AppendLine();
            }
            if (error != null && error.Length > 0)
            {
                sb.Append(encoding.GetString(error));
            }
            return sb.ToString();
        }

        public static byte[] ConvertTo(Encoding encoding, string filename)
        {
            byte[] bytesunicode = Encoding.Unicode.GetBytes(filename);
            return Encoding.Convert(Encoding.Unicode, encoding, bytesunicode);
        }
    }
}
