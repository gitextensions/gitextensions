using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace TeamCityIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata(TeamCityAdapter.PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TeamCitySettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private string _defaultProjectName;
        private readonly TeamCityAdapter _teamCityAdapter = new TeamCityAdapter();
        private readonly TranslationString _failToLoadProjectMessage = new TranslationString("Failed to load the projects and build list." + Environment.NewLine + "Please verify the server url.");
        private readonly TranslationString _failToLoadProjectCaption = new TranslationString("Error when loading the projects and build list");
        private readonly TranslationString _failToExtractDataFromClipboardMessage = new TranslationString("The clipboard doesn't contain a valid build url." + Environment.NewLine + Environment.NewLine +
                "Please copy in the clipboard the url of the build before retrying." + Environment.NewLine +
                "(Should contain at least the \"buildTypeId\" parameter)");
        private readonly TranslationString _failToExtractDataFromClipboardCaption = new TranslationString("Build url not valid");

        public TeamCitySettingsUserControl()
        {
            InitializeComponent();
            InitializeComplete();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        }

        public void Initialize(string defaultProjectName, IEnumerable<string> remotes)
        {
            _defaultProjectName = defaultProjectName;
            SetChooseBuildButtonState();
        }

        public void LoadSettings(ISettingsSource buildServerConfig)
        {
            if (buildServerConfig != null)
            {
                TeamCityServerUrl.Text = buildServerConfig.GetString("BuildServerUrl", string.Empty);
                TeamCityProjectName.Text = buildServerConfig.GetString("ProjectName", _defaultProjectName);
                TeamCityBuildIdFilter.Text = buildServerConfig.GetString("BuildIdFilter", string.Empty);
                CheckBoxLogAsGuest.Checked = buildServerConfig.GetBool("LogAsGuest", false);
            }
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            if (BuildServerSettingsHelper.IsRegexValid(TeamCityBuildIdFilter.Text))
            {
                buildServerConfig.SetString("BuildServerUrl", TeamCityServerUrl.Text);
                buildServerConfig.SetString("ProjectName", TeamCityProjectName.Text);
                buildServerConfig.SetString("BuildIdFilter", TeamCityBuildIdFilter.Text);
                buildServerConfig.SetBool("LogAsGuest", CheckBoxLogAsGuest.Checked);
            }
        }

        private void TeamCityBuildIdFilter_TextChanged(object sender, EventArgs e)
        {
            labelRegexError.Visible = !BuildServerSettingsHelper.IsRegexValid(TeamCityBuildIdFilter.Text);
        }

        private void buttonProjectChooser_Click(object sender, EventArgs e)
        {
            try
            {
                var teamCityBuildChooser = new TeamCityBuildChooser(TeamCityServerUrl.Text, TeamCityProjectName.Text, TeamCityBuildIdFilter.Text);
                var result = teamCityBuildChooser.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    TeamCityProjectName.Text = teamCityBuildChooser.TeamCityProjectName;
                    TeamCityBuildIdFilter.Text = teamCityBuildChooser.TeamCityBuildIdFilter;
                }
            }
            catch
            {
                MessageBox.Show(this, _failToLoadProjectMessage.Text, _failToLoadProjectCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TeamCityServerUrl_TextChanged(object sender, EventArgs e)
        {
            SetChooseBuildButtonState();
        }

        private void SetChooseBuildButtonState()
        {
            buttonProjectChooser.Enabled = !string.IsNullOrWhiteSpace(TeamCityServerUrl.Text);
        }

        private readonly Regex _teamcityBuildUrlParameters = new Regex(@"(\?|\&)([^=]+)\=([^&]+)");
        private void lnkExtractDataFromBuildUrlCopiedInTheClipboard_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Clipboard.ContainsText() && Clipboard.GetText().Contains("buildTypeId="))
            {
                var buildUri = new Uri(Clipboard.GetText());
                var teamCityServerUrl = buildUri.Scheme + "://" + buildUri.Authority;
                TeamCityServerUrl.Text = teamCityServerUrl;
                _teamCityAdapter.InitializeHttpClient(teamCityServerUrl);

                var paramResults = _teamcityBuildUrlParameters.Matches(buildUri.Query);
                foreach (Match paramResult in paramResults)
                {
                    if (paramResult.Success)
                    {
                        if (paramResult.Groups[2].Value == "buildTypeId")
                        {
                            var buildType = _teamCityAdapter.GetBuildType(paramResult.Groups[3].Value);
                            TeamCityProjectName.Text = buildType.ParentProject;
                            TeamCityBuildIdFilter.Text = buildType.Id;
                            return;
                        }
                    }
                }
            }

            MessageBox.Show(this, _failToExtractDataFromClipboardMessage.Text, _failToExtractDataFromClipboardCaption.Text,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
