using System.Security.Cryptography;
using System.Text;

namespace Gravatar
{
    public static class MD5
    {
        /// <summary>
        ///   Small MD5 Function
        /// </summary>
        /// <returns>Hash of the email address passed.</returns>
        public static string CalcMD5(string email)
        {
            var md5CryptoServiceProvider = new MD5CryptoServiceProvider();

            var bytesToHash = Encoding.ASCII.GetBytes(email);

            bytesToHash = md5CryptoServiceProvider.ComputeHash(bytesToHash);

            var builder = new StringBuilder();

            foreach (var b in bytesToHash)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}