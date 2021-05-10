using System.Text;

namespace GitUI.Avatars
{
    internal static class HexString
    {
        /// <summary>
        /// Converts a byte array into its lowercase hex representation.
        /// </summary>
        public static string FromByteArray(byte[] data)
        {
            if (data is null || data.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new(capacity: data.Length * 2);

            foreach (var b in data)
            {
                builder.AppendFormat("{0:x2}", b);
            }

            return builder.ToString();
        }
    }
}
