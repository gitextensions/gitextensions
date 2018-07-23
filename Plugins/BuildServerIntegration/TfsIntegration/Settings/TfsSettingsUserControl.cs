using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace TfsIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata(TfsAdapter.PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TfsSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private string _defaultProjectName;

        public TfsSettingsUserControl()
        {
            InitializeComponent();
            InitializeComplete();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        }

        public void Initialize(string defaultProjectName, IEnumerable<string> remotes)
        {
            _defaultProjectName = defaultProjectName;
        }

        public void LoadSettings(ISettingsSource buildServerConfig)
        {
            if (buildServerConfig != null)
            {
                TfsServer.Text = buildServerConfig.GetString("TfsServer", string.Empty);
                TfsTeamCollectionName.Text = buildServerConfig.GetString("TfsTeamCollectionName", "DefaultCollection");
                TfsProjectName.Text = buildServerConfig.GetString("ProjectName", _defaultProjectName);
                TfsBuildDefinitionNameFilter.Text = buildServerConfig.GetString("TfsBuildDefinitionName", string.Empty);
            }
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            if (BuildServerSettingsHelper.IsRegexValid(TfsBuildDefinitionNameFilter.Text))
            {
                buildServerConfig.SetString("TfsServer", TfsServer.Text);
                buildServerConfig.SetString("TfsTeamCollectionName", TfsTeamCollectionName.Text);
                buildServerConfig.SetString("ProjectName", TfsProjectName.Text);
                buildServerConfig.SetString("TfsBuildDefinitionName", TfsBuildDefinitionNameFilter.Text);
            }
        }

        private void TfsBuildDefinitionNameFilter_TextChanged(object sender, EventArgs e)
        {
            labelRegexError.Visible = !BuildServerSettingsHelper.IsRegexValid(TfsBuildDefinitionNameFilter.Text);
        }
    }
}
