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
    public partial class BuildServerIntegrationSettingsPage : SettingsPageBase
    {
        private const string NoneItem = "<None>";
        private readonly GitModule _gitModule;
        private readonly Task<object> _populateBuildServerTypeTask;
        private IniConfigSource _buildServerConfigSource;
        private IConfig _buildServerConfig;

        public BuildServerIntegrationSettingsPage(GitModule gitModule)
        {
            InitializeComponent();
            Text = "Build server integration";
            Translate();

            _gitModule = gitModule;


            _populateBuildServerTypeTask =
                Task.Factory.StartNew(() =>
                        {
                            var exports = ManagedExtensibility.CompositionContainer.GetExports<IBuildServerAdapter, IBuildServerTypeMetadata>();

                            return exports.Select(export => export.Metadata.BuildServerType).ToArray();
                        })
                    .ContinueWith(
                        task => BuildServerType.DataSource = new[] { NoneItem }.Concat(task.Result).ToArray(),
                        TaskScheduler.FromCurrentSynchronizationContext());
        }

        public override bool IsInstantSavePage
        {
            get { return true; }
        }

        protected override void OnLoadSettings()
        {
            base.OnLoadSettings();

            bool isRepositoryValid = IsRepositoryValid;

            BuildServerType.Enabled = isRepositoryValid;
            if (isRepositoryValid)
            {
                _populateBuildServerTypeTask.ContinueWith(
                    task =>
                        {
                            checkBoxEnableBuildServerIntegration.Checked = Settings.EnableBuildServerIntegration;

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
            get { return !string.IsNullOrEmpty(_gitModule.GitWorkingDir); }
        }

        public override void SaveSettings()
        {
            base.SaveSettings();

            Settings.EnableBuildServerIntegration = checkBoxEnableBuildServerIntegration.Checked;

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
            return Path.Combine(_gitModule.WorkingDir, ".buildserver");
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
            if (!Equals(BuildServerType.SelectedItem, NoneItem) && !string.IsNullOrEmpty(_gitModule.GitWorkingDir))
            {
                var defaultProjectName = _gitModule.GitWorkingDir.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Last();

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
