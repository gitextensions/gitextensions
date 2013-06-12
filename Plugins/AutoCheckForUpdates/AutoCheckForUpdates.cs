using System;
using System.Globalization;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace AutoCheckForUpdates
{
    public class AutoCheckForUpdates : GitPluginBase
    {
        //Description of the plugin

        #region IGitPlugin Members

        public override string Description
        {
            get { return "Check for updates"; }
        }

        protected override void RegisterSettings()
        {
            base.RegisterSettings();
            //Register settings
            Settings.AddSetting("Enabled (true / false)", "true");
            Settings.AddSetting("Check every # days", "7");
            Settings.AddSetting("Last check (yyyy/M/dd)",
                                new DateTime(2000, 1, 1).ToString("yyyy/M/dd", CultureInfo.InvariantCulture));
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            //Connect to events -> connect to all main events because in theory the prebrowse
            //event can be missed since plugins are loaded asynchronous
            gitUiCommands.PreBrowse += GitUiCommandsPreBrowse;
            gitUiCommands.PreCommit += GitUiCommandsPreBrowse;
        }

        public override void Unregister(IGitUICommands gitUiCommands)
        {
            gitUiCommands.PreBrowse -= GitUiCommandsPreBrowse;
            gitUiCommands.PreCommit -= GitUiCommandsPreBrowse;
        }

        public override bool Execute(GitUIBaseEventArgs e)
        {
            using (var updateForm = new Updates(e.GitModule.AppVersion) { AutoClose = false })
                updateForm.ShowDialog(e.OwnerForm);
            return false;
        }

        #endregion

        private void GitUiCommandsPreBrowse(object sender, GitUIBaseEventArgs e)
        {
            //Only check at startup when plugin is enabled
            if (!Settings.GetSetting("Enabled (true / false)").Equals("true", StringComparison.InvariantCultureIgnoreCase))
                return;

            int days;
            if (!int.TryParse(Settings.GetSetting("Check every # days"), out days))
                days = 7;

            try
            {
                if (DateTime.ParseExact(
                    Settings.GetSetting("Last check (yyyy/M/dd)"),
                    "yyyy/M/dd",
                    CultureInfo.InvariantCulture).AddDays(days) >= DateTime.Now)
                    return;
            }
            catch (FormatException)
            {
            }
            finally
            {
                Settings.SetSetting("Last check (yyyy/M/dd)",
                                    DateTime.Now.ToString("yyyy/M/dd", CultureInfo.InvariantCulture));
            }

            using (var updateForm = new Updates(e.GitModule.AppVersion) { AutoClose = true })
                updateForm.ShowDialog();
        }
    }
}