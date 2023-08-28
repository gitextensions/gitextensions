using System.ComponentModel.Composition;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace UITests.CommandsDialogs.SettingsDialog.Pages
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata("GenericBuildServerMock")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MockGenericBuildServerSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private string? _defaultProjectName;

        public MockGenericBuildServerSettingsUserControl()
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
            txtProjectName.Text = buildServerConfig.GetString("ProjectName", _defaultProjectName);
            txtAccountName.Text = buildServerConfig.GetString("AccountName", null);
            cbLoadTestResults.CheckState = SetNullableChecked(buildServerConfig.GetBool("LoadTestsResults"));
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
            buildServerConfig.SetString("ProjectName", txtProjectName.Text.NullIfEmpty());
            buildServerConfig.SetString("AccountName", txtAccountName.Text.NullIfEmpty());
            buildServerConfig.SetBool("LoadTestsResults", NullIfIndeterminate(cbLoadTestResults));
            return;

            // if the setting is empty, do not set any value (as this could override lower priority levels)
            static bool? NullIfIndeterminate(CheckBox s)
            {
                return s.CheckState == CheckState.Indeterminate ? null : s.Checked;
            }
        }
    }
}
