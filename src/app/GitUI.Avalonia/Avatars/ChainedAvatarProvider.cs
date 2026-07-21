namespace GitUI.Avatars;

/// <summary>
/// An avatar provider that combines multiple avatar providers.
/// </summary>
public sealed class ChainedAvatarProvider : IAvatarProvider
{
    private readonly IAvatarProvider[] _avatarProviders;

    public ChainedAvatarProvider(params IAvatarProvider[] avatarProviders)
    {
        _avatarProviders = avatarProviders ?? throw new ArgumentNullException(nameof(avatarProviders));

        if (_avatarProviders.Any(p => p is null))
        {
            throw new ArgumentNullException(nameof(avatarProviders));
        }
    }

    public ChainedAvatarProvider(IEnumerable<IAvatarProvider> avatarProviders)
    {
        ArgumentNullException.ThrowIfNull(avatarProviders);
        _avatarProviders = [.. avatarProviders];
    }

    public bool PerformsIo => _avatarProviders.Length != 0 && _avatarProviders.Any(p => p.PerformsIo);

    /// <summary>
    /// Gets an avatar image from multiple avatar providers and returns the first hit.
    /// </summary>
    public async Task<byte[]?> GetAvatarAsync(string email, string? name, int imageSize)
    {
        foreach (IAvatarProvider provider in _avatarProviders)
        {
            byte[]? avatar = null;

            try
            {
                avatar = await provider.GetAvatarAsync(email, name, imageSize);
            }
            catch
            {
            }

            if (avatar is not null)
            {
                return avatar;
            }
        }

        return null;
    }
}
