using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands.Remotes;
using GitCommands.Settings;
using GitUI;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using GitUIPluginInterfaces.Settings;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitExtensions.Plugins.BuildServerIntegration
{
    public partial class BuildServerIntegrationSettingsControl : GitExtensionsControl
    {
        private static readonly char[] PathSeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        private readonly TranslationString _noneItem = new("None");
        private IConfigFileRemoteSettingsManager? _remotesManager;
        private JoinableTask<object>? _populateBuildServerTypeTask;

        private IGitModule _gitModule;
        private ISettingsSource? _currentSettings;

        public BuildServerIntegrationSettingsControl(IGitModule gitModule)
        {
            InitializeComponent();
            Text = "Build server integration";

            _gitModule = gitModule;

            _remotesManager = new ConfigFileRemoteSettingsManager(() => _gitModule);
            _populateBuildServerTypeTask = ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                    var exports = ManagedExtensibility.GetExports<IBuildServerAdapter, IBuildServerTypeMetadata>();
                    var buildServerTypes = exports.Select(export =>
                        {
                            var canBeLoaded = export.Metadata.CanBeLoaded;
                            return export.Metadata.BuildServerType.Combine(" - ", canBeLoaded);
                        }).ToArray();

                    await this.SwitchToMainThreadAsync();

                    checkBoxEnableBuildServerIntegration.Enabled = true;
                    checkBoxShowBuildResultPage.Enabled = true;
                    BuildServerType.Enabled = true;

                    BuildServerType.DataSource = new[] { _noneItem.Text }.Concat(buildServerTypes).ToArray();
                    return BuildServerType.DataSource;
                });

            InitializeComplete();
        }

        public void SettingsToPage(ISettingsSource settingsSource)
        {
            _currentSettings = settingsSource;

            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    Validates.NotNull(_populateBuildServerTypeTask);

                    await _populateBuildServerTypeTask.JoinAsync();

                    await this.SwitchToMainThreadAsync();

                    IBuildServerSettings buildServerSettings = settingsSource
                        .BuildServer();

                    checkBoxEnableBuildServerIntegration.SetNullableChecked(buildServerSettings.EnableIntegration);
                    checkBoxShowBuildResultPage.SetNullableChecked(buildServerSettings.ShowBuildResultPage);

                    BuildServerType.SelectedItem = buildServerSettings.Type ?? _noneItem.Text;
                });
        }

        public void PageToSettings(ISettingsSource settingsSource)
        {
            _currentSettings = settingsSource;

            IBuildServerSettings buildServerSettings = settingsSource
                .BuildServer();

            buildServerSettings.EnableIntegration = checkBoxEnableBuildServerIntegration.Checked;
            buildServerSettings.ShowBuildResultPage = checkBoxShowBuildResultPage.Checked;

            var selectedBuildServerType = GetSelectedBuildServerType();

            buildServerSettings.Type = selectedBuildServerType;

            var control =
                buildServerSettingsPanel.Controls.OfType<IBuildServerSettingsUserControl>()
                                        .SingleOrDefault();
            control?.SaveSettings(settingsSource.ByPath(buildServerSettings.Type!));
        }

        private void ActivateBuildServerSettingsControl()
        {
            var controls = buildServerSettingsPanel.Controls.OfType<IBuildServerSettingsUserControl>().Cast<Control>();
            var previousControl = controls.SingleOrDefault();
            previousControl?.Dispose();

            var control = CreateBuildServerSettingsUserControl();

            buildServerSettingsPanel.Controls.Clear();

            if (control is not null)
            {
                IBuildServerSettings buildServerSettings = _currentSettings
                    .BuildServer();

                control.LoadSettings(_currentSettings.ByPath(buildServerSettings.Type!));

                buildServerSettingsPanel.Controls.Add((Control)control);
                ((Control)control).Dock = DockStyle.Fill;
            }
        }

        private IBuildServerSettingsUserControl? CreateBuildServerSettingsUserControl()
        {
            if (BuildServerType.SelectedIndex == 0 || string.IsNullOrEmpty(_gitModule.WorkingDir))
            {
                return null;
            }

            var defaultProjectName = _gitModule.WorkingDir.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries).Last();

            var exports = ManagedExtensibility.GetExports<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>();
            var selectedExport = exports.SingleOrDefault(export => export.Metadata.BuildServerType == GetSelectedBuildServerType());
            if (selectedExport is not null)
            {
                var buildServerSettingsUserControl = selectedExport.Value;
                Validates.NotNull(_remotesManager);
                var remoteUrls = _remotesManager.LoadRemotes(false).Select(r => string.IsNullOrEmpty(r.PushUrl) ? r.Url : r.PushUrl);

                buildServerSettingsUserControl.Initialize(defaultProjectName, remoteUrls);
                return buildServerSettingsUserControl;
            }

            return null;
        }

        private string? GetSelectedBuildServerType()
        {
            if (BuildServerType.SelectedIndex == 0)
            {
                return null;
            }

            return (string)BuildServerType.SelectedItem;
        }

        private void BuildServerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActivateBuildServerSettingsControl();
        }
    }
}
