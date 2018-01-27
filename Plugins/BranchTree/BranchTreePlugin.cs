using GitUIPluginInterfaces;
using ResourceManager;
using System;

namespace BranchTree
{
    public class BranchTreePlugin : GitPluginBase, IGitPluginForRepository
    {

        public BranchTreePlugin()
        {            
            SetNameAndDescription ("BranchTree");
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
