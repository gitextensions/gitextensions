#nullable enable

using System.Diagnostics;
using GitCommands.Config;
using GitExtensions.Extensibility.Configurations;

namespace GitUI.CommandsDialogs.BrowseDialog;

/// <summary>
///  Represents a release version with its details.
/// </summary>
internal class ReleaseVersion
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="ReleaseVersion"/> class.
    /// </summary>
    /// <param name="version">The version of the release.</param>
    /// <param name="releaseType">The type of the release.</param>
    /// <param name="downloadPage">The download page URL for the release.</param>
    /// <param name="requiredNetRuntimeVersion">The required .NET runtime version for the release.</param>
    private ReleaseVersion(Version version, ReleaseType releaseType, string downloadPage, Version? requiredNetRuntimeVersion)
    {
        ApplicationVersion = version;
        ReleaseType = releaseType;
        DownloadPage = downloadPage;
        RequiredNetRuntimeVersion = requiredNetRuntimeVersion;
    }

    /// <summary>
    ///  Gets the version of the release.
    /// </summary>
    public Version ApplicationVersion { get; }

    /// <summary>
    ///  Gets the type of the release.
    /// </summary>
    public ReleaseType ReleaseType { get; }

    /// <summary>
    ///  Gets the download page URL for the release.
    /// </summary>
    public string DownloadPage { get; }

    /// <summary>
    ///  Gets the required .NET runtime version for the release.
    /// </summary>
    public Version? RequiredNetRuntimeVersion { get; }

    /// <summary>
    ///  Creates a <see cref="ReleaseVersion"/> instance from a configuration section.
    /// </summary>
    /// <param name="section">The configuration section.</param>
    /// <returns>A <see cref="ReleaseVersion"/> instance or null if parsing fails.</returns>
    private static ReleaseVersion? FromSection(IConfigSection section)
    {
        Version appVersion;
        try
        {
            appVersion = new Version(section.SubSection!);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return null;
        }

        Enum.TryParse(section.GetValue(nameof(ReleaseType)), true, out ReleaseType releaseType);

        _ = Version.TryParse(section.GetValue("NetRuntimeVersion"), out Version? requiredNetRuntimeVersion);

        return new ReleaseVersion(appVersion, releaseType, section.GetValue(nameof(DownloadPage)), requiredNetRuntimeVersion);
    }

    /// <summary>
    ///  Parses a string containing version information into a collection of <see cref="ReleaseVersion"/> instances.
    /// </summary>
    /// <param name="versionsStr">The string containing version information.</param>
    /// <returns>A collection of <see cref="ReleaseVersion"/> instances.</returns>
    public static IEnumerable<ReleaseVersion> Parse(string versionsStr)
    {
        ConfigFile cfg = new(fileName: "");
        cfg.LoadFromString(versionsStr);
        IEnumerable<IConfigSection> sections = cfg.GetConfigSections("Version");
        sections = sections.Concat(cfg.GetConfigSections("RCVersion"));

        return sections.Select(FromSection).WhereNotNull();
    }

    /// <summary>
    ///  Gets the newer versions from the available versions compared to the current version.
    /// </summary>
    /// <param name="currentVersion">The current version.</param>
    /// <param name="checkForReleaseCandidates">Whether to check for release candidates.</param>
    /// <param name="availableVersions">The available versions.</param>
    /// <returns>A collection of newer <see cref="ReleaseVersion"/> instances.</returns>
    public static IEnumerable<ReleaseVersion> GetNewerVersions(
        Version currentVersion,
        bool checkForReleaseCandidates,
        IEnumerable<ReleaseVersion> availableVersions)
    {
        IEnumerable<ReleaseVersion> versions = availableVersions.Where(version =>
                version.ReleaseType == ReleaseType.Major ||
                version.ReleaseType == ReleaseType.HotFix ||
                (checkForReleaseCandidates && version.ReleaseType == ReleaseType.ReleaseCandidate));

        return versions.Where(version => version.ApplicationVersion.CompareTo(currentVersion) > 0);
    }
}
