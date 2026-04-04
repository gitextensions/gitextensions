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
                OwnerTextBox.Text = owner;
                RepositoryTextBox.Text = repository;
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

        ApiUrlTextBox.Text = apiUrl ?? DefaultApiUrl;

        if (!string.IsNullOrWhiteSpace(owner))
        {
            OwnerTextBox.Text = owner;
        }

        if (!string.IsNullOrWhiteSpace(repository))
        {
            RepositoryTextBox.Text = repository;
        }

        ApiTokenTextBox.Text = apiToken;
    }

    public void SaveSettings(SettingsSource buildServerConfig)
    {
        string? apiUrl = ApiUrlTextBox.Text.NullIfEmpty();
        if (apiUrl == DefaultApiUrl)
        {
            apiUrl = null;
        }

        buildServerConfig.SetString("GitHubActionsApiUrl", apiUrl);
        buildServerConfig.SetString("GitHubActionsOwner", OwnerTextBox.Text.NullIfEmpty());
        buildServerConfig.SetString("GitHubActionsRepository", RepositoryTextBox.Text.NullIfEmpty());
        buildServerConfig.SetString("GitHubActionsApiToken", ApiTokenTextBox.Text.NullIfEmpty());
    }

    private void TokenManagementLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser("https://github.com/settings/tokens/new?scopes=repo&description=GitExtensions");
    }
}
