using System.Diagnostics.Contracts;
using System.Text;

namespace GitCommands
{
    /// <summary>
    /// Encoding Helper
    /// </summary>
    public static class EncodingHelper
    {
        [Pure]
        public static string GetString(byte[]? output, byte[]? error, Encoding encoding)
        {
            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            StringBuilder sb = new();

            if (output?.Length is > 0)
            {
                sb.Append(encoding.GetString(output));
            }

            if (error?.Length is > 0)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.Append(encoding.GetString(error));
            }

            return sb.ToString();
        }

        [Pure]
        public static byte[] ConvertTo(Encoding encoding, string s)
        {
            byte[] unicodeBytes = Encoding.Unicode.GetBytes(s);

            return Encoding.Convert(Encoding.Unicode, encoding, unicodeBytes);
        }

        [Pure]
        public static string DecodeString(byte[]? output, byte[]? error, ref Encoding encoding)
        {
            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            string outputString = "";
            if (output?.Length is > 0)
            {
                using Stream ms = new MemoryStream(output);
                using StreamReader reader = new(ms, encoding);
                reader.Peek();
                encoding = reader.CurrentEncoding;
                outputString = reader.ReadToEnd();
            }

            if (error?.Length is > 0)
            {
                using Stream ms = new MemoryStream(error);
                using StreamReader reader = new(ms, encoding);
                reader.Peek();

                if (outputString.Length > 0)
                {
                    outputString += Environment.NewLine;
                }
                else
                {
                    // .Net automatically detect Unicode encoding in StreamReader
                    encoding = reader.CurrentEncoding;
                }

                outputString += reader.ReadToEnd();
            }

            return outputString;
        }
    }
}
