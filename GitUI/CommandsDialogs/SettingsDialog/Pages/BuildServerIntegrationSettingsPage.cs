using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Nini.Config;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class BuildServerIntegrationSettingsPage : RepoDistSettingsPage
    {
        private const string NoneItem = "<None>";
        private Task<object> _populateBuildServerTypeTask;
        private IniConfigSource _buildServerConfigSource;
        private IConfig _buildServerConfig;

        public BuildServerIntegrationSettingsPage()
        {
            InitializeComponent();
            Text = "Build server integration";
            Translate();
        }

        protected override void Init(ISettingsPageHost aPageHost)
        {
            base.Init(aPageHost);

            _populateBuildServerTypeTask =
                Task.Factory.StartNew(() =>
                        {
                            var exports = ManagedExtensibility.CompositionContainer.GetExports<IBuildServerAdapter, IBuildServerTypeMetadata>();
                            var buildServerTypes = exports.Select(export => export.Metadata.BuildServerType).ToArray();

                            return buildServerTypes;
                        })
                    .ContinueWith(
                        task =>
                            {
                                checkBoxEnableBuildServerIntegration.Enabled = true;
                                BuildServerType.Enabled = true;

                                BuildServerType.DataSource = new[] { NoneItem }.Concat(task.Result).ToArray();
                                return BuildServerType.DataSource;
                            },
                        TaskScheduler.FromCurrentSynchronizationContext());
        }

        public override bool IsInstantSavePage
        {
            get { return true; }
        }

        protected override void SettingsToPage()
        {
            bool isRepositoryValid = IsRepositoryValid;

            BuildServerType.Enabled = isRepositoryValid;
            if (isRepositoryValid)
            {
                _populateBuildServerTypeTask.ContinueWith(
                    task =>
                        {
                            checkBoxEnableBuildServerIntegration.Checked = CurrentSettings.EnableBuildServerIntegration;

                            var fileName = GetBuildServerFileName();
                            if (File.Exists(fileName))
                            {
                                _buildServerConfigSource = new IniConfigSource(fileName);
                                _buildServerConfig = GetBuildServerConfig("General");
                            }

                            BuildServerType.SelectedItem = _buildServerConfig != null
                                                                ? _buildServerConfig.GetString("ActiveBuildServerType", NoneItem)
                                                                : NoneItem;
                        },
                    TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private bool IsRepositoryValid
        {
            get { return !string.IsNullOrEmpty(Module.GitWorkingDir); }
        }

        protected override void PageToSettings()
        {
            CurrentSettings.EnableBuildServerIntegration = checkBoxEnableBuildServerIntegration.Checked;

            if (IsRepositoryValid)
            {
                try
                {
                    var fileName = GetBuildServerFileName();
                    var selectedBuildServerType = GetSelectedBuildServerType();

                    if (File.Exists(fileName) || selectedBuildServerType != NoneItem)
                    {
                        FileInfoExtensions.MakeFileTemporaryWritable(
                            fileName,
                            x =>
                                {
                                    if (_buildServerConfig == null)
                                    {
                                        _buildServerConfigSource = new IniConfigSource();
                                    }

                                    _buildServerConfig = GetBuildServerConfig("General");
                                    _buildServerConfig.Set("ActiveBuildServerType", selectedBuildServerType ?? NoneItem);

                                    var control =
                                        buildServerSettingsPanel.Controls.OfType<IBuildServerSettingsUserControl>()
                                                                .SingleOrDefault();
                                    if (control != null)
                                        control.SaveSettings(GetBuildServerConfig(selectedBuildServerType));

                                    _buildServerConfigSource.Save(fileName);
                                });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.ToString());
                }
            }
        }

        private string GetBuildServerFileName()
        {
            return Path.Combine(Module.WorkingDir, ".buildserver");
        }

        private void ActivateBuildServerSettingsControl()
        {
            var controls = buildServerSettingsPanel.Controls.OfType<IBuildServerSettingsUserControl>().Cast<Control>();
            var previousControl = controls.SingleOrDefault();
            if (previousControl != null) previousControl.Dispose();

            var control = CreateBuildServerSettingsUserControl();

            buildServerSettingsPanel.Controls.Clear();

            if (control != null)
            {
                if (_buildServerConfigSource != null && _buildServerConfigSource.Configs != null)
                    control.LoadSettings(_buildServerConfigSource.Configs[GetSelectedBuildServerType()]);

                buildServerSettingsPanel.Controls.Add((Control)control);
            }
        }

        private IBuildServerSettingsUserControl CreateBuildServerSettingsUserControl()
        {
            if (!Equals(BuildServerType.SelectedItem, NoneItem) && !string.IsNullOrEmpty(Module.GitWorkingDir))
            {
                var defaultProjectName = Module.GitWorkingDir.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Last();

                var exports = ManagedExtensibility.CompositionContainer.GetExports<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>();
                var selectedExport = exports.SingleOrDefault(export => export.Metadata.BuildServerType == GetSelectedBuildServerType());
                if (selectedExport != null)
                {
                    var buildServerSettingsUserControl = selectedExport.Value;
                    buildServerSettingsUserControl.Initialize(defaultProjectName);
                    return buildServerSettingsUserControl;
                }
            }

            return null;
        }

        private string GetSelectedBuildServerType()
        {
            return (string)BuildServerType.SelectedItem;
        }

        private void BuildServerType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            ActivateBuildServerSettingsControl();
        }

        private IConfig GetBuildServerConfig(string configName)
        {
            return _buildServerConfigSource.Configs[configName] ?? _buildServerConfigSource.AddConfig(configName);
        }
    }
}
