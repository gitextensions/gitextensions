using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands.Remotes;
using GitCommands.Settings;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using GitUIPluginInterfaces.Settings;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class BuildServerIntegrationSettingsPage : RepoDistSettingsPage
    {
        private readonly TranslationString _noneItem =
            new("None");
        private IConfigFileRemoteSettingsManager? _remotesManager;
        private JoinableTask<object>? _populateBuildServerTypeTask;

        public BuildServerIntegrationSettingsPage()
        {
            InitializeComponent();
            Text = "Build server integration";
            InitializeComplete();
        }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);

            _remotesManager = new ConfigFileRemoteSettingsManager(() => Module);
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
        }

        public override bool IsInstantSavePage => false;

        protected override void SettingsToPage()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    Validates.NotNull(_populateBuildServerTypeTask);

                    await _populateBuildServerTypeTask.JoinAsync();

                    await this.SwitchToMainThreadAsync();

                    IBuildServerSettings buildServerSettings = GetCurrentSettings()
                        .BuildServer();

                    checkBoxEnableBuildServerIntegration.SetNullableChecked(buildServerSettings.EnableIntegration);
                    checkBoxShowBuildResultPage.SetNullableChecked(buildServerSettings.ShowBuildResultPage);

                    BuildServerType.SelectedItem = buildServerSettings.Type ?? _noneItem.Text;
                });
        }

        protected override void PageToSettings()
        {
            IBuildServerSettings buildServerSettings = GetCurrentSettings()
                .BuildServer();

            buildServerSettings.EnableIntegration = checkBoxEnableBuildServerIntegration.Checked;
            buildServerSettings.ShowBuildResultPage = checkBoxShowBuildResultPage.Checked;

            var selectedBuildServerType = GetSelectedBuildServerType();

            buildServerSettings.Type = selectedBuildServerType;

            var control =
                buildServerSettingsPanel.Controls.OfType<IBuildServerSettingsUserControl>()
                                        .SingleOrDefault();
            control?.SaveSettings(GetCurrentSettings().ByPath(buildServerSettings.Type!));
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
                IBuildServerSettings buildServerSettings = GetCurrentSettings()
                    .BuildServer();

                control.LoadSettings(GetCurrentSettings().ByPath(buildServerSettings.Type!));

                buildServerSettingsPanel.Controls.Add((Control)control);
                ((Control)control).Dock = DockStyle.Fill;
            }
        }

        private IBuildServerSettingsUserControl? CreateBuildServerSettingsUserControl()
        {
            Validates.NotNull(Module);

            if (BuildServerType.SelectedIndex == 0 || string.IsNullOrEmpty(Module.WorkingDir))
            {
                return null;
            }

            var defaultProjectName = Module.WorkingDir.Split(Delimiters.PathSeparators, StringSplitOptions.RemoveEmptyEntries).Last();

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
