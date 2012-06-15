using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace DeleteUnusedBranches
{
    public class DeleteUnusedBranchesPlugin : IGitPluginForRepository
    {
        public string Description
        {
            get { return "Delete obsolete branches"; }
        }

        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
            Settings.AddSetting("Delete obsolete branches older than (days)", "30");
        }

        public bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            int days;
            if (!int.TryParse(Settings.GetSetting("Delete obsolete branches older than (days)"), out days))
                days = 30;

            using (var frm = new DeleteUnusedBranchesForm(days, gitUiCommands.GitCommands)) frm.ShowDialog(gitUiCommands.OwnerForm as IWin32Window);
            return true;
        }
    }
}
