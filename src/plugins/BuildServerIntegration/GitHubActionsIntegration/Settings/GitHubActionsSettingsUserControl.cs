using System.ComponentModel.Composition;
using GitCommands;
using GitCommands.Remotes;
using GitExtensions.Extensibility.Settings;
using GitUI;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace GitExtensions.Plugins.GitHubActionsIntegration.Settings;

[Export(typeof(IBuildServerSettingsUserControl))]
[BuildServerSettingsUserControlMetadata(GitHubActionsAdapter.PluginName)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class GitHubActionsSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
{
    private const string DefaultApiUrl = "https://api.github.com";

    private readonly GitHubRemoteParser _remoteParser = new();

    public GitHubActionsSettingsUserControl()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public void Initialize(string defaultProjectName, IEnumerable<string?> remotes)
    {
        foreach (string? remote in remotes)
        {
            if (remote is not null
                && _remoteParser.TryExtractGitHubDataFromRemoteUrl(remote, out string? owner, out string? repository))
            {
                txtOwner.Text = owner;
                txtRepository.Text = repository;
                break;
            }
        }
    }

    public void LoadSettings(SettingsSource buildServerConfig)
    {
        string? apiUrl = buildServerConfig.GetString("GitHubActionsApiUrl", null);
        string? owner = buildServerConfig.GetString("GitHubActionsOwner", null);
        string? repository = buildServerConfig.GetString("GitHubActionsRepository", null);
        string? apiToken = buildServerConfig.GetString("GitHubActionsApiToken", null);

        txtApiUrl.Text = apiUrl ?? DefaultApiUrl;

        if (!string.IsNullOrWhiteSpace(owner))
        {
            txtOwner.Text = owner;
        }

        if (!string.IsNullOrWhiteSpace(repository))
        {
            txtRepository.Text = repository;
        }

        txtApiToken.Text = apiToken;
    }

    public void SaveSettings(SettingsSource buildServerConfig)
    {
        string? apiUrl = txtApiUrl.Text.NullIfEmpty();
        if (apiUrl == DefaultApiUrl)
        {
            apiUrl = null;
        }

        buildServerConfig.SetString("GitHubActionsApiUrl", apiUrl);
        buildServerConfig.SetString("GitHubActionsOwner", txtOwner.Text.NullIfEmpty());
        buildServerConfig.SetString("GitHubActionsRepository", txtRepository.Text.NullIfEmpty());
        buildServerConfig.SetString("GitHubActionsApiToken", txtApiToken.Text.NullIfEmpty());
    }

    private void lnkTokenManagement_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser("https://github.com/settings/tokens/new?scopes=repo&description=GitExtensions");
    }
}
