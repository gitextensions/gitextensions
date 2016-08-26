using System.Windows.Forms;
using GitUIPluginInterfaces;
using ResourceManager;

namespace CreateLocalBranches
{
    public class CreateLocalBranchesPlugin : GitPluginBase, IGitPluginForRepository
    {
        public CreateLocalBranchesPlugin()
        {
            Description = "Create local tracking branches";
            Translate();
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var frm = new CreateLocalBranchesForm(gitUiCommands)) frm.ShowDialog(gitUiCommands.OwnerForm as IWin32Window);
            return true;
        }
    }
}
