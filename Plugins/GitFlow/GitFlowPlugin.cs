using GitUIPluginInterfaces;
using ResourceManager;

namespace GitFlow
{
    public class GitFlowPlugin : GitPluginBase, IGitPluginForRepository
    {
        public GitFlowPlugin()
        {
            Description = "GitFlow";
            Translate();
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
