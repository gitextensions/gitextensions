using System.ComponentModel.Composition;
using GitCommands.Remotes;
using GitExtensions.Extensibility.Settings;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace AzureDevOpsIntegration;

[Export(typeof(IBuildServerAutoDetector))]
internal sealed class AzureDevOpsAutoDetector : IBuildServerAutoDetector
{
    public string BuildServerType => AzureDevOpsAdapter.PluginName;

    public bool TryDetect(IReadOnlyList<string> remoteUrls, SettingsSource? settingsSource)
    {
        AzureDevOpsRemoteParser parser = new();

        foreach (string remoteUrl in remoteUrls)
        {
            if (parser.TryExtractAzureDevopsDataFromRemoteUrl(remoteUrl, out string? owner, out string? project, out string? repository))
            {
                if (settingsSource is not null)
                {
                    if (string.IsNullOrWhiteSpace(settingsSource.GetString("ProjectUrl", null)))
                    {
                        string? projectUrl = AzureDevOpsRemoteParser.BuildProjectUrl(remoteUrl, owner, project);
                        if (projectUrl is not null)
                        {
                            settingsSource.SetString("ProjectUrl", projectUrl);
                        }
                    }

                    if (string.IsNullOrWhiteSpace(settingsSource.GetString("RepositoryName", null)))
                    {
                        settingsSource.SetString("RepositoryName", repository);
                    }
                }

                return true;
            }
        }

        return false;
    }
}
