using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
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
        private string? _defaultProjectName;
        private readonly TeamCityAdapter _teamCityAdapter = new();
        private readonly TranslationString _failToLoadProjectMessage = new("Failed to load the projects and build list." + Environment.NewLine + "Please verify the server url.");
        private readonly TranslationString _failToLoadProjectCaption = new("Error when loading the projects and build list");
        private readonly TranslationString _failToExtractDataFromClipboardMessage = new("The clipboard doesn't contain a valid build url." + Environment.NewLine + Environment.NewLine +
                "Please copy in the clipboard the url of the build before retrying." + Environment.NewLine +
                "(Should contain at least the \"buildTypeId\" parameter)");
        private readonly TranslationString _failToExtractDataFromClipboardCaption = new("Build url not valid");

        public TeamCitySettingsUserControl()
        {
            InitializeComponent();
            InitializeComplete();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        }

        public void Initialize(string defaultProjectName, IEnumerable<string?> remotes)
        {
            _defaultProjectName = defaultProjectName;
            SetChooseBuildButtonState();
        }

        public void LoadSettings(ISettingsSource buildServerConfig)
        {
            TeamCityServerUrl.Text = buildServerConfig.GetString("BuildServerUrl", null);
            TeamCityProjectName.Text = buildServerConfig.GetString("ProjectName", _defaultProjectName);
            TeamCityBuildIdFilter.Text = buildServerConfig.GetString("BuildIdFilter", null);
            CheckBoxLogAsGuest.CheckState = SetNullableChecked(buildServerConfig.GetBool("LogAsGuest", false));
            return;

            static CheckState SetNullableChecked(bool? value)
            {
                return value.HasValue
                    ? value.Value ? CheckState.Checked : CheckState.Unchecked
                    : CheckState.Indeterminate;
            }
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            if (!BuildServerSettingsHelper.IsRegexValid(TeamCityBuildIdFilter.Text))
            {
                return;
            }

            // Empty string is handled as unset, not overriding lower priority levels
            buildServerConfig.SetString("BuildServerUrl", TeamCityServerUrl.Text.NullIfEmpty());
            buildServerConfig.SetString("ProjectName", TeamCityProjectName.Text.NullIfEmpty());
            buildServerConfig.SetString("BuildIdFilter", TeamCityBuildIdFilter.Text.NullIfEmpty());
            buildServerConfig.SetBool("LogAsGuest", NullIfIndeterminate(CheckBoxLogAsGuest));
            return;

            // if the setting is empty, do not set any value (as this could override lower priority levels)
            static bool? NullIfIndeterminate(CheckBox s)
            {
                return s.CheckState == CheckState.Indeterminate ? null : s.Checked;
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
                TeamCityBuildChooser teamCityBuildChooser = new(TeamCityServerUrl.Text, TeamCityProjectName.Text, TeamCityBuildIdFilter.Text);
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

        private readonly Regex _teamcityBuildUrlParameters = new(@"(\?|\&)([^=]+)\=([^&]+)");
        private void lnkExtractDataFromBuildUrlCopiedInTheClipboard_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Clipboard.ContainsText() && Clipboard.GetText().Contains("buildTypeId="))
            {
                Uri buildUri = new(Clipboard.GetText());
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
