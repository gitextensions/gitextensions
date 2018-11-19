using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace AzureDevOpsIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata(AzureDevOpsAdapter.PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private readonly TranslationString _failToExtractDataFromClipboardMessage = new TranslationString("The clipboard doesn't contain a valid build url." + Environment.NewLine + Environment.NewLine +
                "Please copy the url of the build into the clipboard before retrying." + Environment.NewLine +
                "(Should contain at least the \"buildId\" parameter)");
        private readonly TranslationString _failToLoadBuildDefinitionInfoMessage = new TranslationString("Error while trying to retrieve build definition information from url." + Environment.NewLine + Environment.NewLine +
                "Please ensure that the url is valid and that the API token has access to build and project information.");
        private readonly TranslationString _infoNoApiTokenMessage = new TranslationString("Unable to retrieve build definition information without API token. Field will be left blank.");
        private readonly TranslationString _failToExtractDataFromClipboardCaption = new TranslationString("Could not extract data");

        private string _defaultProjectName;
        private IEnumerable<string> _remotes;

        private bool _isUpdating;
        private IntegrationSettings _currentSettings = new IntegrationSettings();

        public SettingsUserControl()
        {
            InitializeComponent();
            InitializeComplete();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            UpdateView();
        }

        private string TokenManagementUrl => ProjectUrlHelper.TryGetTokenManagementUrlFromProject(_currentSettings.ProjectUrl).tokenManagementUrl;

        public void Initialize(string defaultProjectName, IEnumerable<string> remotes)
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
            var settings = IntegrationSettings.ReadFrom(buildServerConfig);

            if (string.IsNullOrWhiteSpace(settings.ProjectUrl))
            {
                var (vstsOrTfsProjectFound, autoDetectedProjectUrl) = ProjectUrlHelper.TryDetectProjectFromRemoteUrls(_remotes);
                if (vstsOrTfsProjectFound)
                {
                    settings.ProjectUrl = autoDetectedProjectUrl;
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
            Process.Start(TokenManagementUrl);
        }

        private async void ExtractLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var buildUrl = Clipboard.ContainsText() ? Clipboard.GetText() : "";
            var (success, projectUrl, buildId) = ProjectUrlHelper.TryParseBuildUrl(buildUrl);
            if (success)
            {
                string buildDefinitionName;
                if (!string.IsNullOrWhiteSpace(_currentSettings.ApiToken))
                {
                    try
                    {
                        using (var apiClient = new ApiClient(projectUrl, _currentSettings.ApiToken))
                        {
                            buildDefinitionName = await apiClient.GetBuildDefinitionNameFromIdAsync(buildId) ?? "";
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(_failToLoadBuildDefinitionInfoMessage.Text, _failToExtractDataFromClipboardCaption.Text);
                        return;
                    }
                }
                else
                {
                    buildDefinitionName = "";
                    MessageBox.Show(_infoNoApiTokenMessage.Text, _failToExtractDataFromClipboardCaption.Text);
                }

                _currentSettings.ProjectUrl = projectUrl;
                _currentSettings.BuildDefinitionFilter = buildDefinitionName;
                UpdateView();
            }
            else
            {
                MessageBox.Show(_failToExtractDataFromClipboardMessage.Text, _failToExtractDataFromClipboardCaption.Text);
            }
        }
    }
}
