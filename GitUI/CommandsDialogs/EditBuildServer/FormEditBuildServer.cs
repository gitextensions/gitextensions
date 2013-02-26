using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.BuildServerIntegration;
using Nini.Config;

namespace GitUI.CommandsDialogs.EditBuildServer
{
    public partial class FormEditBuildServer : GitModuleForm
    {
        private IniConfigSource _buildServerConfigSource;
        private IConfig _buildServerConfig;

        public FormEditBuildServer(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

            BuildServerType.SelectedIndex = 0;

            var fileName = Path.Combine(Module.WorkingDir, ".buildserver");
            if (File.Exists(fileName))
            {
                _buildServerConfigSource = new IniConfigSource(fileName);
                _buildServerConfig = GetBuildServerConfig("General");
            }

            if (_buildServerConfig != null)
            {
                BuildServerType.SelectedIndex = (int)Enum.Parse(typeof(BuildServerType), _buildServerConfig.GetString("ActiveBuildServerType", GitCommands.BuildServerIntegration.BuildServerType.None.ToString()));
            }

            ActivateBuildServerSettingsControl();
        }

        private void ActivateBuildServerSettingsControl()
        {
            var previousControl = buildServerSettingsPanel.Controls.Count > 0 ? buildServerSettingsPanel.Controls[0] : null;
            if (previousControl != null) previousControl.Dispose();

            var control = CreateBuildServerSettingsUserControl();

            buildServerSettingsPanel.Controls.Clear();

            if (control != null)
            {
                if (_buildServerConfigSource != null && _buildServerConfigSource.Configs != null)
                    control.LoadSettings(_buildServerConfigSource.Configs[GetSelectedBuildServerType().ToString()]);

                buildServerSettingsPanel.Controls.Add(control);
            }
        }

        private TeamCitySettingsUserControl CreateBuildServerSettingsUserControl()
        {
            var defaultProjectName = Module.GitWorkingDir.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Last();

            switch (GetSelectedBuildServerType())
            {
                case GitCommands.BuildServerIntegration.BuildServerType.TeamCity:
                    return new TeamCitySettingsUserControl(defaultProjectName);
                default:
                    return null;
            }
        }

        private BuildServerType GetSelectedBuildServerType()
        {
            return (BuildServerType)BuildServerType.SelectedIndex;
        }

        private void BuildServerType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            ActivateBuildServerSettingsControl();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            try
            {
                var fileName = Path.Combine(Module.WorkingDir, ".buildserver");
                FileInfoExtensions.MakeFileTemporaryWritable(
                    fileName,
                    x =>
                        {
                            if (_buildServerConfig == null)
                            {
                                _buildServerConfigSource = new IniConfigSource();
                            }

                            _buildServerConfig = GetBuildServerConfig("General");
                            _buildServerConfig.Set("ActiveBuildServerType", GetSelectedBuildServerType().ToString());

                            var control = buildServerSettingsPanel.Controls.Cast<TeamCitySettingsUserControl>().SingleOrDefault();
                            if (control != null) control.SaveSettings(GetBuildServerConfig(GetSelectedBuildServerType().ToString()));

                            _buildServerConfigSource.Save(fileName);
                        });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString());
            }

            Close();
        }

        private IConfig GetBuildServerConfig(string configName)
        {
            return _buildServerConfigSource.Configs[configName] ?? _buildServerConfigSource.AddConfig(configName);
        }
    }
}
