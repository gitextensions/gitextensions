using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace VstsAndTfsIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata(VstsAndTfsAdapter.PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class VstsAndTfsSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private string _defaultProjectName;
        private IEnumerable<string> _remotes;

        private bool _isUpdating;
        private VstsIntegrationSettings _currentSettings = new VstsIntegrationSettings();

        public VstsAndTfsSettingsUserControl()
        {
            InitializeComponent();
            InitializeComplete();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            UpdateView();
        }

        private string TokenManagementUrl => VstsProjectUrlHelper.TryConvertProjectToTokenManagementUrl(_currentSettings.ProjectUrl).tokenManagementUrl;

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
                RestApiTokenLink.Enabled = BuildServerSettingsHelper.IsUrlValid(TokenManagementUrl);
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
            var settings = VstsIntegrationSettings.ReadFrom(buildServerConfig);

            if (string.IsNullOrWhiteSpace(settings.ProjectUrl) && VstsProjectUrlHelper.TryDetectProjectUrlFromRemotesList(_remotes, out var autoDetectedProjectUrl))
            {
                settings.ProjectUrl = autoDetectedProjectUrl;
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
    }
}
