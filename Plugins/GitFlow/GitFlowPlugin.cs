using System.ComponentModel.Composition;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using GitFlow.Properties;
using ResourceManager;

namespace GitFlow
{
    [Export(typeof(IGitPlugin))]
    public class GitFlowPlugin : GitPluginBase,
        IGitPluginForRepository,
        IGitPluginExecutable
    {
        public GitFlowPlugin()
        {
            SetNameAndDescription("GitFlow");
            Translate();
            Icon = Resource.IconGitFlow;
        }

        public bool Execute(GitUIEventArgs args)
        {
            using var frm = new GitFlowForm(args);

            frm.ShowDialog(args.OwnerForm);

            return frm.IsRefreshNeeded;
        }
    }
}
