using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace CreateLocalBranches
{
    public class CreateLocalBranchesPlugin : GitPluginBase, IGitPluginForRepository
    {
        public override string Description
        {
            get { return "Create local tracking branches"; }
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var frm = new CreateLocalBranchesForm(gitUiCommands)) frm.ShowDialog(gitUiCommands.OwnerForm as IWin32Window);
            return true;
        }
    }
}
