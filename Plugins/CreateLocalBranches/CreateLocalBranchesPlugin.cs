using System.ComponentModel.Composition;
using GitCommands;
using GitExtensions.Plugins.CreateLocalBranches.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensions.Plugins.CreateLocalBranches
{
    [Export(typeof(IGitPlugin))]
    public class CreateLocalBranchesPlugin : GitPluginBase, IGitPluginForRepository
    {
        public CreateLocalBranchesPlugin() : base(false)
        {
            Id = new Guid("BE7BEE10-21B5-489F-9664-957945C203DC");
            Name = "Create local tracking branches";
            Translate(AppSettings.CurrentTranslation);
            Icon = Resources.IconCreateLocalBranches;
        }

        public override bool Execute(GitUIEventArgs args)
        {
            using CreateLocalBranchesForm frm = new(args);
            frm.ShowDialog(args.OwnerForm);

            return true;
        }
    }
}
