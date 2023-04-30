using System.ComponentModel.Composition;
using GitCommands;
using GitUI;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft;
using ResourceManager;

namespace AzureDevOpsIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata(AzureDevOpsAdapter.PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private readonly TranslationString _failToExtractDataFromClipboardMessage = new("The clipboard doesn't contain a valid build url." + Environment.NewLine + Environment.NewLine +
                "Please copy the url of the build into the clipboard before retrying." + Environment.NewLine +
                "(Should contain at least the \"buildId\" parameter)");
        private readonly TranslationString _failToLoadBuildDefinitionInfoMessage = new("Error while trying to retrieve build definition information from url." + Environment.NewLine + Environment.NewLine +
                "Please ensure that the url is valid and that the API token has access to build and project information.");
        private readonly TranslationString _infoNoApiTokenMessage = new("Unable to retrieve build definition information without API token. Field will be left blank.");
        private readonly TranslationString _failToExtractDataFromClipboardCaption = new("Could not extract data");

        private string? _defaultProjectName;
        private IEnumerable<string?>? _remotes;

        private bool _isUpdating;
        private IntegrationSettings _currentSettings = new();

        public SettingsUserControl()
        {
            InitializeComponent();
            InitializeComplete();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            UpdateView();
        }

        private string? TokenManagementUrl => ProjectUrlHelper.TryGetTokenManagementUrlFromProject(_currentSettings.ProjectUrl).tokenManagementUrl;

        public void Initialize(string defaultProjectName, IEnumerable<string?> remotes)
        {
            _defaultProjectName = defaultProjectName;
            _remotes = remotes;
        }

        private void UpdateView()
        {
            if (_isUpdating)
            {
                return;
            }

            _isUpdating = true;
            try
            {
                TfsServer.Text = _currentSettings.ProjectUrl;

                TfsBuildDefinitionNameFilter.Text = _currentSettings.BuildDefinitionFilter;
                labelRegexError.Visible = !BuildServerSettingsHelper.IsRegexValid(TfsBuildDefinitionNameFilter.Text);

                RestApiToken.Text = _currentSettings.ApiToken;
                TokenManagementLink.Enabled = BuildServerSettingsHelper.IsUrlValid(TokenManagementUrl);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void UpdateModel()
        {
            if (_isUpdating)
            {
                return;
            }

            _isUpdating = true;
            try
            {
                _currentSettings.ProjectUrl = TfsServer.Text;
                _currentSettings.BuildDefinitionFilter = TfsBuildDefinitionNameFilter.Text;
                _currentSettings.ApiToken = RestApiToken.Text;
            }
            finally
            {
                _isUpdating = false;
            }
        }

        public void LoadSettings(ISettingsSource buildServerConfig)
        {
            IntegrationSettings settings = IntegrationSettings.ReadFrom(buildServerConfig);

            if (string.IsNullOrWhiteSpace(settings.ProjectUrl))
            {
                Validates.NotNull(_remotes);

                var (vstsOrTfsProjectFound, autoDetectedProjectUrl) = ProjectUrlHelper.TryDetectProjectFromRemoteUrls(_remotes);
                if (vstsOrTfsProjectFound)
                {
                    settings.ProjectUrl = autoDetectedProjectUrl!;
                }
            }

            _currentSettings = settings;
            UpdateView();
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            if (_currentSettings.IsValid())
            {
                _currentSettings.WriteTo(buildServerConfig);
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateModel();
            UpdateView();
        }

        private void RestApiTokenLink_Click(object sender, EventArgs e)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(TokenManagementUrl);
        }

        private void ExtractLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                var buildUrl = Clipboard.ContainsText() ? Clipboard.GetText() : "";
                var (success, projectUrl, buildId) = ProjectUrlHelper.TryParseBuildUrl(buildUrl);
                if (success)
                {
                    Validates.NotNull(projectUrl);

                    string buildDefinitionName;
                    if (!string.IsNullOrWhiteSpace(_currentSettings.ApiToken))
                    {
                        try
                        {
                            using ApiClient apiClient = new(projectUrl, _currentSettings.ApiToken);
                            buildDefinitionName = await apiClient.GetBuildDefinitionNameFromIdAsync(buildId) ?? "";
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(_failToLoadBuildDefinitionInfoMessage.Text, _failToExtractDataFromClipboardCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        buildDefinitionName = "";
                        MessageBox.Show(_infoNoApiTokenMessage.Text, _failToExtractDataFromClipboardCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    _currentSettings.ProjectUrl = projectUrl;
                    _currentSettings.BuildDefinitionFilter = buildDefinitionName;
                    UpdateView();
                }
                else
                {
                    MessageBox.Show(_failToExtractDataFromClipboardMessage.Text, _failToExtractDataFromClipboardCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }).FileAndForget();
        }
    }
}
