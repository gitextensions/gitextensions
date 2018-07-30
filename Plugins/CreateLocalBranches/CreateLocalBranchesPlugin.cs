using System.ComponentModel.Composition;
using CreateLocalBranches.Properties;
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
            Icon = Resources.IconCreateLocalBranches;
        }

        public override bool Execute(GitUIEventArgs args)
        {
            using (var frm = new CreateLocalBranchesForm(args))
            {
                frm.ShowDialog(args.OwnerForm);
            }

            return true;
        }
    }
}
