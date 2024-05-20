namespace GitUI.Avatars
{
    /// <summary>
    /// A source and cache agnostic provider for avatar images.
    /// </summary>
    public interface IAvatarProvider
    {
        /// <summary>
        /// Provides the avatar image for the associated email at the requested size.
        /// </summary>
        Task<Image?> GetAvatarAsync(string email, string? name, int imageSize);

        /// <summary>
        /// Provider doesn't perform any I/O and can generate an avatar quicker then getting it from a filesystem cache
        /// and so the filesystem cache can be bypassed.
        /// </summary>
        bool PerformsIo { get; }
    }
}
