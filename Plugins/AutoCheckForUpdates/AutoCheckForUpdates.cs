using System;
using System.Globalization;
using GitUIPluginInterfaces;

namespace AutoCheckForUpdates
{
    public class AutoCheckForUpdates : IGitPlugin
    {
        //Description of the plugin

        #region IGitPlugin Members

        public string Description
        {
            get { return "Check for updates"; }
        }

        //Store settings to use later
        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
            //Register settings
            Settings.AddSetting("Enabled (true / false)", "true");
            Settings.AddSetting("Check every # days", "7");
            Settings.AddSetting("Last check (yyyy/M/dd)",
                                new DateTime(2000, 1, 1).ToString("yyyy/M/dd", CultureInfo.InvariantCulture));

            //Connect to events -> connect to all main events because in theory the prebrowse
            //event can be missed since plugins are loaded asynchronous
            gitUiCommands.PreBrowse += GitUiCommandsPreBrowse;
            gitUiCommands.PreCommit += GitUiCommandsPreBrowse;
        }

        public void Execute(GitUIBaseEventArgs e)
        {
            var updateForm = new Updates(e.GitVersion) {AutoClose = false};
            updateForm.ShowDialog();
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

            var updateForm = new Updates(e.GitVersion) {AutoClose = true};
            updateForm.ShowDialog();
        }
    }
}