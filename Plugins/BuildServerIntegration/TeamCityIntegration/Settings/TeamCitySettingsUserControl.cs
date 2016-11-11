using System;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace TeamCityIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata("TeamCity")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TeamCitySettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private string _defaultProjectName;
        private TeamCityAdapter _teamCityAdapter = new TeamCityAdapter();

        public TeamCitySettingsUserControl()
        {
            InitializeComponent();
            Translate();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        }

        public void Initialize(string defaultProjectName)
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
            catch (Exception)
            {
                MessageBox.Show(this,
                    "Fail to load the projects and build list." + Environment.NewLine + "Please verify the server url.",
                    "Error when loading the projects and build list", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        Regex teamcityBuildUrlParameters = new Regex(@"(\?|\&)([^=]+)\=([^&]+)");
        private void lnkExtractDataFromBuildUrlCopiedInTheClipboard_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Clipboard.ContainsText() && Clipboard.GetText().Contains("buildTypeId="))
            {
                var buildUri = new Uri(Clipboard.GetText());
                var teamCityServerUrl = buildUri.Scheme + "://" + buildUri.Authority;
                TeamCityServerUrl.Text = teamCityServerUrl;
                _teamCityAdapter.InitializeHttpClient(teamCityServerUrl);

                var paramResults = teamcityBuildUrlParameters.Matches(buildUri.Query);
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

            MessageBox.Show(this,
                "The clipboard doesn't contain a valid build url." + Environment.NewLine + Environment.NewLine +
                "Please copy in the clipboard the url of the build before retrying." + Environment.NewLine +
                "(Should contains at least the parameter\"buildTypeId\")", "Build url not valid",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }
    }
}
