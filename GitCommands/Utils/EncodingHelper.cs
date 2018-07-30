using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace GitCommands
{
    /// <summary>
    /// Encoding Helper
    /// </summary>
    public static class EncodingHelper
    {
        [NotNull]
        [Pure]
        public static string GetString([CanBeNull] byte[] output, [CanBeNull] byte[] error, [NotNull] Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            var sb = new StringBuilder();

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

        [NotNull]
        [Pure]
        public static byte[] ConvertTo([NotNull] Encoding encoding, [NotNull] string s)
        {
            byte[] unicodeBytes = Encoding.Unicode.GetBytes(s);

            return Encoding.Convert(Encoding.Unicode, encoding, unicodeBytes);
        }

        [NotNull]
        [Pure]
        public static string DecodeString([CanBeNull] byte[] output, [CanBeNull] byte[] error, [NotNull] ref Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            string outputString = "";
            if (output != null && output.Length > 0)
            {
                Stream ms = null;
                try
                {
                    ms = new MemoryStream(output);
                    using (var reader = new StreamReader(ms, encoding))
                    {
                        ms = null;
                        reader.Peek();
                        encoding = reader.CurrentEncoding;
                        outputString = reader.ReadToEnd();
                        if (error == null || error.Length == 0)
                        {
                            return outputString;
                        }
                    }
                }
                finally
                {
                    ms?.Dispose();
                }

                outputString = outputString + Environment.NewLine;
            }

            string errorString = null;
            if (error != null && error.Length > 0)
            {
                Stream ms = null;
                try
                {
                    ms = new MemoryStream(error);
                    using (var reader = new StreamReader(ms, encoding))
                    {
                        ms = null;
                        reader.Peek();

                        // .Net automatically detect Unicode encoding in StreamReader
                        encoding = reader.CurrentEncoding;
                        errorString = reader.ReadToEnd();
                        if (output == null || output.Length == 0)
                        {
                            return errorString;
                        }
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
