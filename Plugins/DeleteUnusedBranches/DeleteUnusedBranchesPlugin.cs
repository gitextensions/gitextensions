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
			Settings.AddSetting("Branch where all branches should be merged in", "HEAD");
        }

        public bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            int days;
            if (!int.TryParse(Settings.GetSetting("Delete obsolete branches older than (days)"), out days))
                days = 30;

	    string referenceBranch = Settings.GetSetting("Branch where all branches should be merged in");
            using (var frm = new DeleteUnusedBranchesForm(days, referenceBranch, gitUiCommands.GitCommands)) frm.ShowDialog(gitUiCommands.OwnerForm as IWin32Window);

            return true;
        }
    }
}
