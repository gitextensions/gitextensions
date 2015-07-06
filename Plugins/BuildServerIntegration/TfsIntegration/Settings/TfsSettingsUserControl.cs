using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitCommands.Settings;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace TfsIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata("Team Foundation Server")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TfsSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private string _defaultProjectName;

        public TfsSettingsUserControl()
        {
            InitializeComponent();
            Translate();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        }

        public void Initialize(string defaultProjectName)
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
                TfsBuildDefinitionName.Text = buildServerConfig.GetString("TfsBuildDefinitionName", string.Empty);
            }
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            buildServerConfig.SetString("TfsServer", TfsServer.Text);
            buildServerConfig.SetString("TfsTeamCollectionName", TfsTeamCollectionName.Text);
            buildServerConfig.SetString("ProjectName", TfsProjectName.Text);
            buildServerConfig.SetString("TfsBuildDefinitionName", TfsBuildDefinitionName.Text);
        }
    }
}
