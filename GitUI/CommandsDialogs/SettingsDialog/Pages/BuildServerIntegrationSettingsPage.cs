using System;
using System.IO;
using System.Linq;
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
        private IniConfigSource _buildServerConfigSource;
        private IConfig _buildServerConfig;

        public BuildServerIntegrationSettingsPage(GitModule gitModule)
        {
            InitializeComponent();
            Text = "Build server integration";
            Translate();

            _gitModule = gitModule;
        }

        public override bool IsInstantSavePage
        {
            get { return true; }
        }

        public override void OnPageShown()
        {
            var exports = ManagedExtensibility.CompositionContainer.GetExports<IBuildServerAdapter, IBuildServerTypeMetadata>();

            BuildServerType.DataSource = new[] { NoneItem }.Concat(exports.Select(export => export.Metadata.BuildServerType)).ToArray();

            var fileName = Path.Combine(_gitModule.WorkingDir, ".buildserver");
            if (File.Exists(fileName))
            {
                _buildServerConfigSource = new IniConfigSource(fileName);
                _buildServerConfig = GetBuildServerConfig("General");
            }

            BuildServerType.SelectedItem = _buildServerConfig != null
                                               ? _buildServerConfig.GetString("ActiveBuildServerType", NoneItem)
                                               : NoneItem;
        }

        public override void SaveSettings()
        {
            base.SaveSettings();

            try
            {
                var fileName = Path.Combine(_gitModule.WorkingDir, ".buildserver");
                FileInfoExtensions.MakeFileTemporaryWritable(
                    fileName,
                    x =>
                    {
                        if (_buildServerConfig == null)
                        {
                            _buildServerConfigSource = new IniConfigSource();
                        }

                        var selectedBuildServerType = GetSelectedBuildServerType();

                        _buildServerConfig = GetBuildServerConfig("General");
                        _buildServerConfig.Set("ActiveBuildServerType", selectedBuildServerType);

                        var control = buildServerSettingsPanel.Controls.OfType<IBuildServerSettingsUserControl>().SingleOrDefault();
                        if (control != null) control.SaveSettings(GetBuildServerConfig(selectedBuildServerType));

                        _buildServerConfigSource.Save(fileName);
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString());
            }
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
            if (!Equals(BuildServerType.SelectedItem, NoneItem))
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
