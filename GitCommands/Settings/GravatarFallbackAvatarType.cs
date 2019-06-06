using System.ComponentModel;

namespace GitCommands
{
    /// <summary>
    /// Types of generated Avatars images that are used as fallback of the Gravatar service in
    /// the absence of a user-uploaded image.
    /// See http://en.gravatar.com/site/implement/images#default-image for the ones provided by the Gravatar service
    /// </summary>
    public enum GravatarFallbackAvatarType
    {
        /// <summary>
        /// Return an HTTP 404 response.
        /// </summary>
        None = 0,

        /// <summary>
        /// Git Extensions will generate an avatar with the author initials avatar (color based on the email hash).
        /// </summary>
        [Description("Author initials")]
        AuthorInitials,

#if false
        /// <summary>
        /// Return a simple, cartoon-style silhouetted outline of a person (provided by Gravatar service)
        /// (does not vary by email hash)
        /// </summary>
        /// Don't use it, use Resources.User image instead
        MysteryMan,
#endif

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
