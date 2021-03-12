using System.ComponentModel.Composition;
using GitExtensions.Plugins.GitFlow.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensions.Plugins.GitFlow
{
    [Export(typeof(IGitPlugin))]
    public class GitFlowPlugin : GitPluginBase, IGitPluginForRepository
    {
        public GitFlowPlugin() : base(false)
        {
            SetNameAndDescription("GitFlow");
            Translate();
            Icon = Resource.IconGitFlow;
        }

        public override bool Execute(GitUIEventArgs args)
        {
            using var frm = new GitFlowForm(args);
            frm.ShowDialog(args.OwnerForm);
            return frm.IsRefreshNeeded;
        }
    }
}
