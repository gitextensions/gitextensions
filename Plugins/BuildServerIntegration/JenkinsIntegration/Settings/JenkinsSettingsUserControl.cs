using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace JenkinsIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata("Jenkins")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class JenkinsSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private string _defaultProjectName;

        public JenkinsSettingsUserControl()
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
                JenkinsServerUrl.Text = buildServerConfig.GetString("BuildServerUrl", string.Empty);
                JenkinsProjectName.Text = buildServerConfig.GetString("ProjectName", _defaultProjectName);
            }
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            buildServerConfig.SetString("BuildServerUrl", JenkinsServerUrl.Text);
            buildServerConfig.SetString("ProjectName", JenkinsProjectName.Text);
        }
    }
}
