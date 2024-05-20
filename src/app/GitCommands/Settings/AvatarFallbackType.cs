using System.ComponentModel;

namespace GitCommands
{
    /// <summary>
    /// Types of generated Avatars images that are used as fallback in the absence of a user-uploaded image.
    /// <see cref="AuthorInitials"/> is provided locally, the rest is served by Gravatar.
    /// See http://en.gravatar.com/site/implement/images#default-image for the ones provided by the Gravatar service
    /// </summary>
    public enum AvatarFallbackType
    {
        /// <summary>
        /// Git Extensions will generate an avatar with the author initials avatar (color based on the email hash).
        /// </summary>
        [Description("Author initials")]
        AuthorInitials,

        /// <summary>
        /// Return a generated monster based on the email hash (provided by Gravatar service).
        /// </summary>
        MonsterId,

        /// <summary>
        /// Return a generated face based on the email hash (provided by Gravatar service).
        /// </summary>
        Wavatar,

        /// <summary>
        /// Return a geometric pattern based on the email hash (provided by Gravatar service).
        /// </summary>
        Identicon,

        /// <summary>
        /// Return an 8-bit-style face based on the email hash (provided by Gravatar service).
        /// </summary>
        Retro,

        /// <summary>
        /// Return a generated robot based on the email hash (provided by Gravatar service).
        /// </summary>
        Robohash,
    }
}
