using GitUIPluginInterfaces;
using ResourceManager;

namespace BranchTree
{
    public class BranchTreePlugin : GitPluginBase, IGitPluginForRepository
    {

        public BranchTreePlugin()
        {
            SetNameAndDescription("Branch tree");
            Translate();
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var frm = new BranchTreeForm(gitUiCommands))
                frm.ShowDialog(gitUiCommands.OwnerForm);
            return true;
        }
    }
}
