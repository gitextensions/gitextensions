using System.Diagnostics;

namespace GitUI.Avatars;

/// <summary>
/// A helper provider that wraps another avatar provider.
/// </summary>
public sealed class HotSwapAvatarProvider : IAvatarProvider
{
    /// <summary>
    /// Gets or sets the currently active provider.
    /// </summary>
    public IAvatarProvider? Provider { get; set; }

    public bool PerformsIo => Provider?.PerformsIo ?? false;

    public Task<byte[]?> GetAvatarAsync(string email, string? name, int imageSize)
    {
        try
        {
            return Provider?.GetAvatarAsync(email, name, imageSize) ?? Task.FromResult<byte[]?>(null);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
            return Task.FromResult<byte[]?>(null);
        }
    }
}
