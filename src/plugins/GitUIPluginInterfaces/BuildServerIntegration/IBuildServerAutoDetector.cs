using GitExtensions.Extensibility.Settings;

namespace GitUIPluginInterfaces.BuildServerIntegration;

/// <summary>
///  Allows a build server adapter to participate in automatic detection
///  based on the repository's remote URLs. Implement this interface and
///  export it via MEF alongside <see cref="IBuildServerAdapter"/>.
/// </summary>
public interface IBuildServerAutoDetector
{
    /// <summary>
    ///  The build server type name, matching <see cref="IBuildServerTypeMetadata.BuildServerType"/>.
    /// </summary>
    string BuildServerType { get; }

    /// <summary>
    ///  Attempts to detect this build server from the given remote URLs.
    ///  When detected and <paramref name="settingsSource"/> is not <see langword="null"/>,
    ///  populates adapter-specific settings (e.g. project URL, owner, repository).
    /// </summary>
    /// <param name="remoteUrls">Remote URLs ordered by priority.</param>
    /// <param name="settingsSource">Optional settings to populate when detected.</param>
    /// <returns><see langword="true"/> if this build server was detected.</returns>
    bool TryDetect(IReadOnlyList<string> remoteUrls, SettingsSource? settingsSource);
}
