using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands.Remotes;
using GitCommands.Settings;
using GitExtensions.Plugins.BuildServer.Core;
using GitUI;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitExtensions.Plugins.BuildServer
{
    internal partial class SettingsPage : GitExtensionsControl
    {
        private readonly TranslationString _noneItem = new("None");
        private IConfigFileRemoteSettingsManager? _remotesManager;
        private JoinableTask<object> _populateBuildServerTypeTask;
        private ISettingsSource? _currentSettings;
        private IGitModule _module;

        public SettingsPage(IGitModule module)
        {
            InitializeComponent();
            Text = "Build server integration";

            _module = module;
            _remotesManager = new ConfigFileRemoteSettingsManager(() => module);
            _populateBuildServerTypeTask = ThreadHelper.JoinableTaskFactory
                .RunAsync(async () =>
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

        public void SettingsToPage(ISettingsSource settings)
        {
            ThreadHelper.JoinableTaskFactory
                .RunAsync(async () =>
                {
                    _currentSettings = settings;

                    await _populateBuildServerTypeTask.JoinAsync();

                    await this.SwitchToMainThreadAsync();

                    var settingsByPath = new SettingsPath(settings, string.Empty);

                    checkBoxEnableBuildServerIntegration.SetNullableChecked(settingsByPath.GetBool("EnableIntegration", false));
                    checkBoxShowBuildResultPage.SetNullableChecked(settingsByPath.GetBool("ShowBuildResultPage", true));

                    BuildServerType.SelectedItem = settingsByPath.GetString("Type", null) ?? _noneItem.Text;
                });
        }

        public void PageToSettings(ISettingsSource settings)
        {
            var settingsByPath = new SettingsPath(settings, string.Empty);

            settingsByPath.SetBool("EnableIntegration", checkBoxEnableBuildServerIntegration.Checked);
            settingsByPath.SetBool("ShowBuildResultPage", checkBoxShowBuildResultPage.Checked);

            var selectedBuildServerType = GetSelectedBuildServerType();

            if (selectedBuildServerType is null)
            {
                return;
            }

            settingsByPath.SetString("Type", selectedBuildServerType);

            var control = buildServerSettingsPanel.Controls
                .OfType<IBuildServerSettingsUserControl>()
                .SingleOrDefault();

            control?.SaveSettings(new SettingsPath(settingsByPath, selectedBuildServerType));
        }

        private void ActivateBuildServerSettingsControl()
        {
            var controls = buildServerSettingsPanel.Controls
                .OfType<IBuildServerSettingsUserControl>()
                .Cast<Control>();

            var previousControl = controls.SingleOrDefault();
            previousControl?.Dispose();

            var control = CreateBuildServerSettingsUserControl();

            buildServerSettingsPanel.Controls.Clear();

            if (control is not null)
            {
                var settingsByPath = new SettingsPath(new SettingsPath(_currentSettings, string.Empty), BuildServerType.SelectedItem.ToString());

                control.LoadSettings(settingsByPath);

                buildServerSettingsPanel.Controls.Add((Control)control);

                ((Control)control).Dock = DockStyle.Fill;
            }
        }

        private IBuildServerSettingsUserControl? CreateBuildServerSettingsUserControl()
        {
            _remotesManager = new ConfigFileRemoteSettingsManager(() => _module);

            if (BuildServerType.SelectedIndex == 0 || string.IsNullOrEmpty(_module.WorkingDir))
            {
                return null;
            }

            var defaultProjectName = _module.WorkingDir
                .Split(new[]
                {
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar
                }, StringSplitOptions.RemoveEmptyEntries)
                .Last();

            var exports = ManagedExtensibility.GetExports<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>();
            var selectedExport = exports.SingleOrDefault(export => export.Metadata.BuildServerType == GetSelectedBuildServerType());

            if (selectedExport is not null)
            {
                var buildServerSettingsUserControl = selectedExport.Value;
                var remoteUrls = _remotesManager.LoadRemotes(false)
                    .Select(r => string.IsNullOrEmpty(r.PushUrl) ? r.Url : r.PushUrl);

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
