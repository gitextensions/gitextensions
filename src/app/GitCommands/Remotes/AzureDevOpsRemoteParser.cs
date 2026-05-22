using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GitCommands.Remotes;

public sealed partial class AzureDevOpsRemoteParser : RemoteParser
{
    [GeneratedRegex(@"^https:\/\/(?<owner>[^.]*)\.visualstudio\.com(?:\/DefaultCollection)?\/(?<project>[^\/]*)\/_git\/(?<repo>.*)$", RegexOptions.ExplicitCapture)]
    private static partial Regex VstsHttpsRemoteRegex { get; }

    [GeneratedRegex(@"^[^@]*@vs-ssh\.visualstudio\.com:v\d\/(?<owner>[^\/]*)\/(?<project>[^\/]*)\/(?<repo>.*)$", RegexOptions.ExplicitCapture)]
    private static partial Regex VstsSshRemoteRegex { get; }

    [GeneratedRegex(@"^https:\/\/[^@]*@dev\.azure\.com\/(?<owner>[^\/]*)\/(?<project>[^\/]*)\/_git\/(?<repo>.*)$", RegexOptions.ExplicitCapture)]
    private static partial Regex AzureDevopsHttpsRemoteRegex { get; }

    [GeneratedRegex(@"^git@ssh\.dev\.azure\.com:v\d\/(?<owner>[^\/]*)\/(?<project>[^\/]*)\/(?<repo>.*)$", RegexOptions.ExplicitCapture)]
    private static partial Regex AzureDevopsSshRemoteRegex { get; }

    private static readonly Regex[] _azureDevopsRegexes = [AzureDevopsHttpsRemoteRegex, AzureDevopsSshRemoteRegex, VstsHttpsRemoteRegex, VstsSshRemoteRegex];

    public bool IsValidRemoteUrl(string remoteUrl)
        => TryExtractAzureDevopsDataFromRemoteUrl(remoteUrl, out _, out _, out _);

    public bool TryExtractAzureDevopsDataFromRemoteUrl(string remoteUrl, [NotNullWhen(returnValue: true)] out string? owner, [NotNullWhen(returnValue: true)] out string? project, [NotNullWhen(returnValue: true)] out string? repository)
    {
        owner = null;
        project = null;
        repository = null;

        Match? m = MatchRegExes(remoteUrl, _azureDevopsRegexes);

        if (m is null || !m.Success)
        {
            return false;
        }

        owner = m.Groups["owner"].Value;
        project = m.Groups["project"].Value;
        repository = m.Groups["repo"].Value;
        return true;
    }

    /// <summary>
    ///  Constructs the Azure DevOps project web URL from a remote URL and extracted owner/project names.
    /// </summary>
    /// <returns>The project URL, or <see langword="null"/> if the remote host is not recognized.</returns>
    public static string? BuildProjectUrl(string remoteUrl, string owner, string project)
    {
        if (IsDevAzureCom(remoteUrl))
        {
            return $"https://dev.azure.com/{owner}/{project}";
        }

        if (IsVisualStudioCom(remoteUrl))
        {
            return $"https://{owner}.visualstudio.com/{project}";
        }

        return null;
    }

    /// <summary>
    ///  Constructs the Azure DevOps repository web URL from a remote URL and extracted components.
    /// </summary>
    /// <returns>The repository URL, or <see langword="null"/> if the remote host is not recognized.</returns>
    public static string? BuildRepositoryUrl(string remoteUrl, string owner, string project, string repository)
    {
        if (IsDevAzureCom(remoteUrl))
        {
            return $"https://dev.azure.com/{owner}/{project}/_git/{repository}";
        }

        if (IsVisualStudioCom(remoteUrl))
        {
            return $"https://{owner}.visualstudio.com/{project}/_git/{repository}";
        }

        return null;
    }

    private static bool IsDevAzureCom(string remoteUrl)
        => remoteUrl.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase);

    private static bool IsVisualStudioCom(string remoteUrl)
        => remoteUrl.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase);
}
