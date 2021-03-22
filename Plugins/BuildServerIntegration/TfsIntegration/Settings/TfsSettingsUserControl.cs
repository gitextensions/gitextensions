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
        private string? _defaultProjectName;

        public TfsSettingsUserControl()
        {
            InitializeComponent();
            InitializeComplete();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        }

        public void Initialize(string defaultProjectName, IEnumerable<string?> remotes)
        {
            _defaultProjectName = defaultProjectName;
        }

        public void LoadSettings(ISettingsSource buildServerConfig)
        {
            TfsServer.Text = buildServerConfig.GetValue("TfsServer", string.Empty);
            TfsTeamCollectionName.Text = buildServerConfig.GetValue("TfsTeamCollectionName", "DefaultCollection");
            TfsProjectName.Text = buildServerConfig.GetValue("ProjectName", _defaultProjectName ?? string.Empty);
            TfsBuildDefinitionNameFilter.Text = buildServerConfig.GetValue("TfsBuildDefinitionName", string.Empty);
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            if (BuildServerSettingsHelper.IsRegexValid(TfsBuildDefinitionNameFilter.Text))
            {
                buildServerConfig.SetValue("TfsServer", TfsServer.Text);
                buildServerConfig.SetValue("TfsTeamCollectionName", TfsTeamCollectionName.Text);
                buildServerConfig.SetValue("ProjectName", TfsProjectName.Text);
                buildServerConfig.SetValue("TfsBuildDefinitionName", TfsBuildDefinitionNameFilter.Text);
            }
        }

        private void TfsBuildDefinitionNameFilter_TextChanged(object sender, EventArgs e)
        {
            labelRegexError.Visible = !BuildServerSettingsHelper.IsRegexValid(TfsBuildDefinitionNameFilter.Text);
        }
    }
}
