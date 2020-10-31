using System.ComponentModel.Composition;
using CreateLocalBranches.Properties;
using GitExtensions.Extensibility;
using GitUIPluginInterfaces;
using ResourceManager;

namespace CreateLocalBranches
{
    [Export(typeof(IGitPlugin))]
    public class CreateLocalBranchesPlugin : GitPluginBase,
        IGitPluginForRepository,
        IGitPluginExecutable
    {
        public CreateLocalBranchesPlugin()
        {
            SetNameAndDescription("Create local tracking branches");
            Translate();
            Icon = Resources.IconCreateLocalBranches;
        }

        public bool Execute(GitUIEventArgs args)
        {
            using var frm = new CreateLocalBranchesForm(args);

            frm.ShowDialog(args.OwnerForm);

            return true;
        }
    }
}
