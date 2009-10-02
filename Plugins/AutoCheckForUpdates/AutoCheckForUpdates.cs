using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GitUIPluginInterfaces;
using System.Net;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AutoCheckForUpdates
{
    public class AutoCheckForUpdates : IGitPlugin
    {
        //Description of the plugin
        public string Description 
        {
            get
            {
                return "Check for updates";
            }
        }

        //Store settings to use later
        private IGitPluginSettingsContainer settings;
        public IGitPluginSettingsContainer Settings 
        {
            get
            {
                return settings;
            }
            set
            {
                settings = value;
            }
        }

        public void Register(IGitUICommands gitUICommands)
        {
            //Register settings
            Settings.AddSetting("Enabled (true / false)", "true");
            Settings.AddSetting("Check every # days", "7");
            Settings.AddSetting("Last check (yyyy/M/dd)", new DateTime(2000, 1, 1).ToString("yyyy/M/dd", CultureInfo.InvariantCulture));

            //Connect to events
            gitUICommands.PreBrowse += new GitUIEventHandler(gitUICommands_PreBrowse);
        }

        void gitUICommands_PreBrowse(IGitUIEventArgs e)
        {
            //Only check at startup when plugin is enabled
            if (Settings.GetSetting("Enabled (true / false)").Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                int days = 0;
                if (!int.TryParse(Settings.GetSetting("Check every # days"), out days))
                    days = 0;

                if (DateTime.ParseExact(Settings.GetSetting("Last check (yyyy/M/dd)"), "yyyy/M/dd", CultureInfo.InvariantCulture).AddDays(7) < DateTime.Now)
                {
                    Settings.SetSetting("Last check (yyyy/M/dd)", DateTime.Now.ToString("yyyy/M/dd", CultureInfo.InvariantCulture));
                    CheckForUpdate(e);
                }
            }
        }

        public void Execute(IGitUIEventArgs e)
        {
            if (!CheckForUpdate(e))
                MessageBox.Show("No updates found.");
        }

        private static bool CheckForUpdate(IGitUIEventArgs e)
        {
            WebRequest myWebRequest = WebRequest.Create(@"http://code.google.com/p/gitextensions/");
            WebResponse myWebResponse = myWebRequest.GetResponse();
            Stream ReceiveStream = myWebResponse.GetResponseStream();
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

            StreamReader readStream = new StreamReader(ReceiveStream, encode);
            string response = readStream.ReadToEnd();
            readStream.Close();
            myWebResponse.Close();

            //find for string like "http://gitextensions.googlecode.com/files/GitExtensions170SetupComplete.msi"
            Regex regEx = new Regex(@"http://gitextensions.googlecode.com/files/GitExtensions[0-9][0-9][0-9]SetupComplete.msi");

            MatchCollection matches = regEx.Matches(response);

            foreach (Match match in matches)
            {
                if (!match.Value.Equals("http://gitextensions.googlecode.com/files/GitExtensions" + e.GitVersion + "SetupComplete.msi"))
                {
                    new Updates(match.Value).ShowDialog();
                    return true;
                }
            }

            return false;
        }
    }
}
