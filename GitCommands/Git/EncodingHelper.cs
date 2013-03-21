using System;
using System.IO;
using System.Text;

namespace GitCommands
{
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

        public static string DecodeString(byte[] output, byte[] error, ref Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            
            string outputString = "";
            if (output != null && output.Length > 0)
            {
                Stream ms = null;
                try
                {
                    ms = new MemoryStream(output);
                    using (StreamReader reader = new StreamReader(ms, encoding))
                    {
                        ms = null;
                        reader.Peek();
                        encoding = reader.CurrentEncoding;
                        outputString = reader.ReadToEnd();
                        if (error == null || error.Length == 0)
                            return outputString;
                    }
                }
                finally
                {
                    if (ms != null)
                        ms.Dispose();
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
                    using (StreamReader reader = new StreamReader(ms, encoding))
                    {
                        ms = null;
                        reader.Peek();
                        // .Net automatically detect Unicode encoding in StreamReader
                        encoding = reader.CurrentEncoding;
                        errorString = reader.ReadToEnd();
                        if (output == null || output.Length == 0)
                            return errorString;
                    }
                }
                finally
                {
                    if (ms != null)
                        ms.Dispose();
                }
            }

            return outputString + errorString;
        }
    }
}
