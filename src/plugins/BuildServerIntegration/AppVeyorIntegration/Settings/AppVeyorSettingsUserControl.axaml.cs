using System.ComponentModel.Composition;
using GitExtensions.Extensibility.Settings;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace AppVeyorIntegration.Settings;

[Export(typeof(IBuildServerSettingsUserControl))]
[BuildServerSettingsUserControlMetadata(AppVeyorAdapter.PluginName)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class AppVeyorSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
{
    private string? _defaultProjectName;

    public AppVeyorSettingsUserControl()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public void Initialize(string defaultProjectName, IEnumerable<string?> remotes)
    {
        _defaultProjectName = defaultProjectName;
    }

    public void LoadSettings(SettingsSource buildServerConfig)
    {
        AppVeyorProjectName.Text = buildServerConfig.GetString("AppVeyorProjectName", _defaultProjectName);
        AppVeyorAccountName.Text = buildServerConfig.GetString("AppVeyorAccountName", null);
        AppVeyorAccountToken.Text = buildServerConfig.GetString("AppVeyorAccountToken", null);
        cbLoadTestResults.IsChecked = buildServerConfig.GetBool("AppVeyorLoadTestsResults");
    }

    public void SaveSettings(SettingsSource buildServerConfig)
    {
        buildServerConfig.SetString("AppVeyorProjectName", AppVeyorProjectName.Text.NullIfEmpty());
        buildServerConfig.SetString("AppVeyorAccountName", AppVeyorAccountName.Text.NullIfEmpty());
        buildServerConfig.SetString("AppVeyorAccountToken", AppVeyorAccountToken.Text.NullIfEmpty());

        // if the setting is empty, do not set any value (as this could override lower priority levels)
        buildServerConfig.SetBool("AppVeyorLoadTestsResults", cbLoadTestResults.IsChecked);
    }
}
