using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace FindLargeFiles
{
    public class FindLargeFilesPlugin : IGitPluginForRepository
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

        public bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            float threshold;
            if (!float.TryParse(Settings.GetSetting("Find large files bigger than (Mb)"), out threshold))
                threshold = 1;

            new FindLargeFilesForm(threshold, gitUiCommands).ShowDialog(gitUiCommands.OwnerForm as IWin32Window);
            return true;
        }
    }
}
