using System.ComponentModel.Composition;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Plugins.GitFlow.Properties;

namespace GitExtensions.Plugins.GitFlow
{
    [Export(typeof(IGitPlugin))]
    public class GitFlowPlugin : GitPluginBase, IGitPluginForRepository
    {
        public GitFlowPlugin() : base(false)
        {
            Id = new Guid("83E1F3F1-B502-4BFB-97D9-7EF108252401");
            Name = "GitFlow";
            Translate(AppSettings.CurrentTranslation);
            Icon = Resource.IconGitFlow;
        }

        public override bool Execute(GitUIEventArgs args)
        {
            using GitFlowForm frm = new(args);
            frm.ShowDialog(args.OwnerForm);
            return frm.IsRefreshNeeded;
        }
    }
}
