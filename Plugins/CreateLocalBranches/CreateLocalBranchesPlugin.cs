using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace CreateLocalBranches
{
    public class CreateLocalBranchesPlugin : IGitPluginForRepository
    {
        public string Description
        {
            get { return "Create local tracking branches"; }
        }

        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
        }

        public bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var frm = new CreateLocalBranchesForm(gitUiCommands)) frm.ShowDialog(gitUiCommands.OwnerForm as IWin32Window);
            return true;
        }
    }
}
