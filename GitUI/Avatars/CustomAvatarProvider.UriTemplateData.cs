using System;
using System.Security.Cryptography;
using System.Text;

namespace GitUI.Avatars
{
    public sealed partial class CustomAvatarProvider
    {
        /// <summary>
        /// A type that holds and prepares for variables in custom avatar templates.
        /// It's processed by an <see cref="UriTemplateResolver"/>.
        /// </summary>
        private class UriTemplateData
        {
            public UriTemplateData(string email, string? name, int imageSize)
            {
                Email = email ?? throw new ArgumentNullException(nameof(email));
                Name = name;
                ImageSize = imageSize;

                NormalizedEmail = Email.Trim().ToLowerInvariant();

                EmailMd5 = new Lazy<string>(() => ComputeHash(new MD5CryptoServiceProvider(), NormalizedEmail));
                EmailSha1 = new Lazy<string>(() => ComputeHash(new SHA1CryptoServiceProvider(), NormalizedEmail));
                EmailSha256 = new Lazy<string>(() => ComputeHash(new SHA256CryptoServiceProvider(), NormalizedEmail));
            }

            public string Email { get; }
            public string? Name { get; }
            public int ImageSize { get; }
            public string NormalizedEmail { get; }

            public Lazy<string> EmailMd5 { get; }
            public Lazy<string> EmailSha1 { get; }
            public Lazy<string> EmailSha256 { get; }

            private static string ComputeHash(HashAlgorithm hashAlgorithm, string input)
            {
                using (hashAlgorithm)
                {
                    var inputBytes = Encoding.UTF8.GetBytes(input);
                    var hashBytes = hashAlgorithm.ComputeHash(inputBytes);
                    return HexString.FromByteArray(hashBytes);
                }
            }
        }
    }
}
