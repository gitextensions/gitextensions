using System;
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
            Id = new Guid("83E1F3F1-B502-4BFB-97D9-7EF108252401");
            Name = "GitFlow";
            Translate();
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
