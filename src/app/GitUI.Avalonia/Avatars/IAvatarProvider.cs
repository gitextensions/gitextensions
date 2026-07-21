namespace GitUI.Avatars;

/// <summary>
/// A source and cache agnostic provider for encoded avatar images.
/// </summary>
public interface IAvatarProvider
{
    /// <summary>
    /// Provides encoded avatar image bytes for the associated email at the requested size.
    /// </summary>
    Task<byte[]?> GetAvatarAsync(string email, string? name, int imageSize);

    /// <summary>
    /// Provider doesn't perform any I/O and can generate an avatar quicker then getting it from a filesystem cache
    /// and so the filesystem cache can be bypassed.
    /// </summary>
    bool PerformsIo { get; }
}
