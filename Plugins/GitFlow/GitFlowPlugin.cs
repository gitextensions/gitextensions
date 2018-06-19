using System.ComponentModel.Composition;
using GitFlow.Properties;
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
            Icon = Resource.IconGitFlow;
        }

        public override bool Execute(GitUIEventArgs args)
        {
            using (var frm = new GitFlowForm(args))
            {
                frm.ShowDialog(args.OwnerForm);
                return frm.IsRefreshNeeded;
            }
        }
    }
}
