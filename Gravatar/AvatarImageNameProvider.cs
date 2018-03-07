namespace Gravatar
{
    public interface IImageNameProvider
    {
        /// <summary>
        /// Returns the image filename for the given email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>The image filename.</returns>
        string Get(string email);
    }

    /// <summary>
    /// Translate emails into corresponding avatar image filenames.
    /// </summary>
    public sealed class AvatarImageNameProvider : IImageNameProvider
    {
        /// <summary>
        /// Returns the avatar image filename for the given email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>The avatar image filename, if email is supplied; otherwise <see langword="null"/>.</returns>
        public string Get(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            return $"{email}.png";
        }
    }
}