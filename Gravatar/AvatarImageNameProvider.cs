using System;
using JetBrains.Annotations;

namespace Gravatar
{
    public interface IImageNameProvider
    {
        /// <summary>
        /// Returns the image filename for the given email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>The image filename.</returns>
        /// <exception cref="ArgumentException"><paramref name="email"/> is <c>null</c> or white-space.</exception>
        [NotNull]
        string Get([NotNull] string email);
    }

    /// <summary>
    /// Translate emails into corresponding avatar image filenames.
    /// </summary>
    public sealed class AvatarImageNameProvider : IImageNameProvider
    {
        string IImageNameProvider.Get(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Must be a non-blank value.", nameof(email));
            }

            return $"{email}.png";
        }
    }
}