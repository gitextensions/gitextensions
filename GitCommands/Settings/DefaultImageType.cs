namespace GitCommands
{
    /// <summary>
    /// Services that gravatar provides in order to provide avatars in
    /// the absence of a user-uploaded image.
    /// </summary>
    public enum DefaultImageType
    {
        /// <summary>
        /// Return an HTTP 404 response.
        /// </summary>
        None = 0,

#if false
        /// <summary>
        /// Return a simple, cartoon-style silhouetted outline of a person
        /// (does not vary by email hash)
        /// </summary>
        /// Don't use it, use Resources.User image instead
        MysteryMan,
#endif

        /// <summary>
        /// Return a generated monster based on the email hash.
        /// </summary>
        MonsterId,

        /// <summary>
        /// Return a generated face based on the email hash.
        /// </summary>
        Wavatar,

        /// <summary>
        /// Return a geometric pattern based on the email hash.
        /// </summary>
        Identicon,

        /// <summary>
        /// Return an 8-bit-style face based on the email hash.
        /// </summary>
        Retro
    }
}
