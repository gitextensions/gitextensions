using System.ComponentModel.Composition;
using GitCommands.Remotes;
using GitExtensions.Extensibility.Settings;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitExtensions.Plugins.GitHubActionsIntegration;

[Export(typeof(IBuildServerAutoDetector))]
internal sealed class GitHubActionsAutoDetector : IBuildServerAutoDetector
{
    public string BuildServerType => GitHubActionsAdapter.PluginName;

    public bool TryDetect(IReadOnlyList<string> remoteUrls, SettingsSource? settingsSource)
    {
        GitHubRemoteParser parser = new();

        foreach (string remoteUrl in remoteUrls)
        {
            if (parser.TryExtractGitHubDataFromRemoteUrl(remoteUrl, out string? owner, out string? repository))
            {
                if (settingsSource is not null)
                {
                    if (string.IsNullOrWhiteSpace(settingsSource.GetString(GitHubActionsAdapter.SettingOwner, null)))
                    {
                        settingsSource.SetString(GitHubActionsAdapter.SettingOwner, owner);
                    }

                    if (string.IsNullOrWhiteSpace(settingsSource.GetString(GitHubActionsAdapter.SettingRepository, null)))
                    {
                        settingsSource.SetString(GitHubActionsAdapter.SettingRepository, repository);
                    }
                }

                return true;
            }
        }

        return false;
    }
}
