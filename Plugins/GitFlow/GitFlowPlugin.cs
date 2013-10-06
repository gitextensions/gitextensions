using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitFlow
{
    public class GitFlowPlugin : GitPluginBase, IGitPluginForRepository
    {
        public override string Description
        {
            get { return "GitFlow"; }
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var frm = new GitFlowForm(gitUiCommands))
            {
                frm.ShowDialog(gitUiCommands.OwnerForm);
                return frm.IsRefreshNeeded;
            }
        }
    }
}
