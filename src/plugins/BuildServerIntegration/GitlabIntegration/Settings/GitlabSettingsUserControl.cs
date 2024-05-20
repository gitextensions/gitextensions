using System.ComponentModel.Composition;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Plugins.GitlabIntegration.ApiClient;
using GitExtensions.Plugins.GitlabIntegration.ApiClient.Models;
using GitUI;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace GitExtensions.Plugins.GitlabIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata(GitlabAdapter.PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class GitlabSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private readonly GitlabRemoteParser _remoteParser = new();
        private IEnumerable<string?> _remotes;
        private string? _repositoryNamespace;
        private string? _repositoryName;

        public GitlabSettingsUserControl()
        {
            InitializeComponent();
            InitializeComplete();
        }

        public void Initialize(string defaultProjectName, IEnumerable<string?> remotes)
        {
            _remotes = remotes;
        }

        public void LoadSettings(SettingsSource buildServerConfig)
        {
            string? host = buildServerConfig.GetValue("InstanceUrl");
            int? projectId = buildServerConfig.GetInt("ProjectId");
            string? apiToken = buildServerConfig.GetValue("ApiToken");

            foreach (string? remote in _remotes)
            {
                if (remote is not null
                    && _remoteParser.TryExtractGitlabDataFromRemoteUrl(remote, out string? remoteHost, out string? owner, out string? repository))
                {
                    remoteHost = $"https://{remoteHost}";
                    if (string.IsNullOrWhiteSpace(host))
                    {
                        host = remoteHost;
                    }

                    _repositoryNamespace = owner;
                    _repositoryName = repository;
                    break;
                }
            }

            InstanceUrlTextBox.Text = host;
            ApiTokenTextBox.Text = apiToken;

            if (projectId is not null)
            {
                ProjectIdTextBox.Text = projectId.ToString();
            }
            else
            {
                if (host is not null)
                {
                    ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                    {
                        projectId = await UpdateProjectIdAsync(host, apiToken);
                        if (projectId is > 0)
                        {
                            ProjectIdTextBox.Text = projectId.ToString();
                        }
                    }).FileAndForget();
                }
            }
        }

        private async Task<int?> UpdateProjectIdAsync(string? host, string? apiToken)
        {
            if (string.IsNullOrWhiteSpace(host)
                || string.IsNullOrWhiteSpace(_repositoryNamespace)
                || string.IsNullOrWhiteSpace(_repositoryName))
            {
                return 0;
            }

            try
            {
                GitlabApiClient apiClient = new(host, apiToken ?? string.Empty);
                GitlabProject? project = await apiClient.GetProjectAsync(_repositoryNamespace, _repositoryName);

                if (project is not null)
                {
                    return project.Id;
                }
            }
            catch (Exception)
            {
                return 0;
            }

            return 0;
        }

        public void SaveSettings(SettingsSource buildServerConfig)
        {
            buildServerConfig.SetString("InstanceUrl", InstanceUrlTextBox.Text.NullIfEmpty());
            if (int.TryParse(ProjectIdTextBox.Text, out int projectId))
            {
                buildServerConfig.SetInt("ProjectId", projectId);
            }

            buildServerConfig.SetString("ApiToken", ApiTokenTextBox.Text.NullIfEmpty());
            buildServerConfig.SetInt("PagesLimit", 0);
        }

        private void GetProjectIdLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!Uri.IsWellFormedUriString(InstanceUrlTextBox.Text, UriKind.Absolute))
            {
                return;
            }

            GetProjectIdStatusText.Visible = false;

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                int? projectId = await UpdateProjectIdAsync(InstanceUrlTextBox.Text, ApiTokenTextBox.Text);
                if (projectId is > 0)
                {
                    ProjectIdTextBox.Text = projectId.ToString();
                }
                else
                {
                    GetProjectIdStatusText.Visible = true;
                }
            }).FileAndForget();
        }

        private void TokenManagementLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            const string urlTail = "/-/profile/personal_access_tokens?name=GitExtensionsIntegration&scopes=api";

            if (Uri.IsWellFormedUriString(InstanceUrlTextBox.Text, UriKind.Absolute))
            {
                OsShellUtil.OpenUrlInDefaultBrowser($"{InstanceUrlTextBox.Text}{urlTail}");
            }
        }

        private void InstanceUrlTextBox_TextChanged(object sender, EventArgs e)
        {
            if (Uri.IsWellFormedUriString(InstanceUrlTextBox.Text, UriKind.Absolute))
            {
                TokenManagementLink.Enabled = true;
                GetProjectIdLink.Enabled = true;
            }
            else
            {
                TokenManagementLink.Enabled = false;
                GetProjectIdLink.Enabled = false;
            }
        }
    }
}
