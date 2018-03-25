using System.ComponentModel.Composition;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitFlow
{
    [Export(typeof(IGitPlugin))]
    public class GitFlowPlugin : GitPluginBase, IGitPluginForRepository
    {
        public GitFlowPlugin()
        {
            SetNameAndDescription("GitFlow");
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
