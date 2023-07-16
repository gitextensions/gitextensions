using System.ComponentModel.Composition;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace GitExtensions.Plugins.GitlabIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata(GitlabAdapter.PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class GitlabSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        public GitlabSettingsUserControl()
        {
            InitializeComponent();
            InitializeComplete();
        }

        public void Initialize(string defaultProjectName, IEnumerable<string?> remotes)
        {
        }

        public void LoadSettings(ISettingsSource buildServerConfig)
        {
            InstanceUrlTextBox.Text = buildServerConfig.GetString("InstanceUrl", null);
            ProjectIdTextBox.Text = buildServerConfig.GetInt("ProjectId", 0).ToString();
            ApiTokenTextBox.Text = buildServerConfig.GetString("ApiToken", null);
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            buildServerConfig.SetString("InstanceUrl", InstanceUrlTextBox.Text.NullIfEmpty());
            if (int.TryParse(ProjectIdTextBox.Text, out int projectId))
            {
                buildServerConfig.SetInt("ProjectId", projectId);
            }

            buildServerConfig.SetString("ApiToken", ApiTokenTextBox.Text.NullIfEmpty());
        }
    }
}
