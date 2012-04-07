using System;
using System.Collections.Generic;
using System.Text;
using GitUIPluginInterfaces;

namespace FindLargeFiles
{
    public class FindLargeFilesPlugin : IGitPlugin
    {
        public string Description
        {
            get { return "Find large files"; }
        }

        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
            Settings.AddSetting("Find large files bigger than (Mb)", "1");
        }

        public void Execute(GitUIBaseEventArgs gitUiCommands)
        {
            float threshold;
            if (!float.TryParse(Settings.GetSetting("Find large files bigger than (Mb)"), out threshold))
                threshold = 1;

            new FindLargeFilesForm(threshold, gitUiCommands).ShowDialog();
        }
    }
}
