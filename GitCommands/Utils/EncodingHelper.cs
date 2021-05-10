using System;
using System.Diagnostics.Contracts;
using System.IO;
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

            if (output is not null && output.Length > 0)
            {
                sb.Append(encoding.GetString(output));
            }

            if (error is not null && error.Length > 0 && output is not null && output.Length > 0)
            {
                sb.AppendLine();
            }

            if (error is not null && error.Length > 0)
            {
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
            if (output is not null && output.Length > 0)
            {
                Stream? ms = null;
                try
                {
                    ms = new MemoryStream(output);
                    using StreamReader reader = new(ms, encoding);
                    ms = null;
                    reader.Peek();
                    encoding = reader.CurrentEncoding;
                    outputString = reader.ReadToEnd();
                    if (error is null || error.Length == 0)
                    {
                        return outputString;
                    }
                }
                finally
                {
                    ms?.Dispose();
                }

                outputString = outputString + Environment.NewLine;
            }

            string? errorString = null;
            if (error is not null && error.Length > 0)
            {
                Stream? ms = null;
                try
                {
                    ms = new MemoryStream(error);
                    using StreamReader reader = new(ms, encoding);
                    ms = null;
                    reader.Peek();

                    // .Net automatically detect Unicode encoding in StreamReader
                    encoding = reader.CurrentEncoding;
                    errorString = reader.ReadToEnd();
                    if (output is null || output.Length == 0)
                    {
                        return errorString;
                    }
                }
                finally
                {
                    ms?.Dispose();
                }
            }

            return outputString + errorString;
        }
    }
}
