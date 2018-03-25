using System.ComponentModel.Composition;
using GitUIPluginInterfaces;
using ResourceManager;

namespace CreateLocalBranches
{
    [Export(typeof(IGitPlugin))]
    public class CreateLocalBranchesPlugin : GitPluginBase, IGitPluginForRepository
    {
        public CreateLocalBranchesPlugin()
        {
            SetNameAndDescription("Create local tracking branches");
            Translate();
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var frm = new CreateLocalBranchesForm(gitUiCommands))
            {
                frm.ShowDialog(gitUiCommands.OwnerForm);
            }

            return true;
        }
    }
}
