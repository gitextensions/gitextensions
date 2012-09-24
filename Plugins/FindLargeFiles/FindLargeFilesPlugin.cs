using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace FindLargeFiles
{
    public class FindLargeFilesPlugin : GitPluginBase, IGitPluginForRepository
    {
        public override string Description
        {
            get { return "Find large files"; }
        }

        protected override void RegisterSettings()
        {
            base.RegisterSettings();
            Settings.AddSetting("Find large files bigger than (Mb)", "1");
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            float threshold;
            if (!float.TryParse(Settings.GetSetting("Find large files bigger than (Mb)"), out threshold))
                threshold = 1;

            using (var frm = new FindLargeFilesForm(threshold, gitUiCommands)) frm.ShowDialog(gitUiCommands.OwnerForm as IWin32Window);
            return true;
        }
    }
}
