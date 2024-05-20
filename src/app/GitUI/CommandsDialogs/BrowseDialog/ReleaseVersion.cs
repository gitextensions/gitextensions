using System.Diagnostics;
using GitCommands.Config;
using GitExtensions.Extensibility.Configurations;

namespace GitUI.CommandsDialogs.BrowseDialog;

internal class ReleaseVersion
{
    public Version Version { get; }
    public ReleaseType ReleaseType { get; }
    public string DownloadPage { get; }

    public ReleaseVersion(Version version, ReleaseType releaseType, string downloadPage)
    {
        Version = version;
        ReleaseType = releaseType;
        DownloadPage = downloadPage;
    }

    public static ReleaseVersion? FromSection(IConfigSection section)
    {
        Version ver;
        try
        {
            ver = new Version(section.SubSection);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return null;
        }

        Enum.TryParse(section.GetValue("ReleaseType"), true, out ReleaseType releaseType);

        return new ReleaseVersion(ver, releaseType, section.GetValue("DownloadPage"));
    }

    public static IEnumerable<ReleaseVersion> Parse(string versionsStr)
    {
        ConfigFile cfg = new(fileName: "");
        cfg.LoadFromString(versionsStr);
        IEnumerable<IConfigSection> sections = cfg.GetConfigSections("Version");
        sections = sections.Concat(cfg.GetConfigSections("RCVersion"));

        return sections.Select(FromSection).WhereNotNull();
    }

    public static IEnumerable<ReleaseVersion> GetNewerVersions(
        Version currentVersion,
        bool checkForReleaseCandidates,
        IEnumerable<ReleaseVersion> availableVersions)
    {
        IEnumerable<ReleaseVersion> versions = availableVersions.Where(version =>
                version.ReleaseType == ReleaseType.Major ||
                version.ReleaseType == ReleaseType.HotFix ||
                (checkForReleaseCandidates && version.ReleaseType == ReleaseType.ReleaseCandidate));

        return versions.Where(version => version.Version.CompareTo(currentVersion) > 0);
    }
}
