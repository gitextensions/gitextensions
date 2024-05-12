using GitCommands.Remotes;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Extensions;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class BuildServerIntegrationSettingsPage : DistributedSettingsPage
    {
        private readonly TranslationString _noneItem =
            new("None");
        private IConfigFileRemoteSettingsManager? _remotesManager;
        private JoinableTask<object>? _populateBuildServerTypeTask;

        public BuildServerIntegrationSettingsPage(IServiceProvider serviceProvider)
           : base(serviceProvider)
        {
            InitializeComponent();
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

                    IEnumerable<Lazy<IBuildServerAdapter, IBuildServerTypeMetadata>> exports = ManagedExtensibility.GetExports<IBuildServerAdapter, IBuildServerTypeMetadata>();
                    string[] buildServerTypes = exports.Select(export =>
                        {
                            string canBeLoaded = export.Metadata.CanBeLoaded;
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
            ThreadHelper.FileAndForget(async () =>
                {
                    Validates.NotNull(_populateBuildServerTypeTask);

                    await _populateBuildServerTypeTask.JoinAsync();

                    await this.SwitchToMainThreadAsync();

                    IBuildServerSettings buildServerSettings = GetCurrentSettings().GetBuildServerSettings();

                    checkBoxEnableBuildServerIntegration.SetNullableChecked(buildServerSettings.IntegrationEnabled);
                    checkBoxShowBuildResultPage.SetNullableChecked(buildServerSettings.ShowBuildResultPage);

                    BuildServerType.SelectedItem = buildServerSettings.ServerName ?? _noneItem.Text;
                    ActivateBuildServerSettingsControl();

                    base.SettingsToPage();
                });
        }

        protected override void PageToSettings()
        {
            IBuildServerSettings buildServerSettings = GetCurrentSettings().GetBuildServerSettings();

            buildServerSettings.ServerName = GetSelectedBuildServerType();
            buildServerSettings.IntegrationEnabled = checkBoxEnableBuildServerIntegration.CheckState == CheckState.Indeterminate
                ? null
                : checkBoxEnableBuildServerIntegration.Checked;
            buildServerSettings.ShowBuildResultPage = checkBoxShowBuildResultPage.CheckState == CheckState.Indeterminate
                ? null
                : checkBoxShowBuildResultPage.Checked;

            IBuildServerSettingsUserControl control = buildServerSettingsPanel.Controls.OfType<IBuildServerSettingsUserControl>().SingleOrDefault();
            control?.SaveSettings(buildServerSettings.SettingsSource);

            base.PageToSettings();
        }

        private void ActivateBuildServerSettingsControl()
        {
            IEnumerable<Control> controls = buildServerSettingsPanel.Controls.OfType<IBuildServerSettingsUserControl>().Cast<Control>();
            Control previousControl = controls.SingleOrDefault();
            previousControl?.Dispose();

            IBuildServerSettingsUserControl control = CreateBuildServerSettingsUserControl();

            buildServerSettingsPanel.Controls.Clear();

            if (control is not null)
            {
                IBuildServerSettings buildServerSettings = GetCurrentSettings().GetBuildServerSettings();

                control.LoadSettings(buildServerSettings.SettingsSource);

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

            string defaultProjectName = Module.WorkingDir.Split(Delimiters.PathSeparators, StringSplitOptions.RemoveEmptyEntries)[^1];

            IEnumerable<Lazy<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>> exports = ManagedExtensibility.GetExports<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>();
            Lazy<IBuildServerSettingsUserControl, IBuildServerTypeMetadata> selectedExport = exports.SingleOrDefault(export => export.Metadata.BuildServerType == GetSelectedBuildServerType());
            if (selectedExport is not null)
            {
                IBuildServerSettingsUserControl buildServerSettingsUserControl = selectedExport.Value;
                Validates.NotNull(_remotesManager);
                IEnumerable<string> remoteUrls = _remotesManager.LoadRemotes(false).Select(r => string.IsNullOrEmpty(r.PushUrl) ? r.Url : r.PushUrl);

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

        internal TestAccessor GetTestAccessor() => new(this);

        internal readonly struct TestAccessor
        {
            private readonly BuildServerIntegrationSettingsPage _form;

            public TestAccessor(BuildServerIntegrationSettingsPage form)
            {
                _form = form;
            }

            public Panel buildServerSettingsPanel => _form.buildServerSettingsPanel;
            public ComboBox BuildServerType => _form.BuildServerType;
            public CheckBox checkBoxEnableBuildServerIntegration => _form.checkBoxEnableBuildServerIntegration;
            public CheckBox checkBoxShowBuildResultPage => _form.checkBoxShowBuildResultPage;
        }
    }
}
