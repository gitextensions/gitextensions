using System;
using System.ComponentModel.Composition;
using GitExtensions.Plugins.GitImpact.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensions.Plugins.GitImpact
{
    [Export(typeof(IGitPlugin))]
    public class GitImpactPlugin : GitPluginBase, IGitPluginForRepository
    {
        public GitImpactPlugin() : base(false)
        {
            Id = new Guid("F1ACFE42-6A5E-4C30-AC10-9A7C4BB8B480");
            Name = "Impact Graph";
            Translate();
            Icon = Resources.IconGitImpact;
        }

        #region IGitPlugin Members

        public override bool Execute(GitUIEventArgs args)
        {
            if (string.IsNullOrEmpty(args.GitModule.WorkingDir))
            {
                return false;
            }

            using FormImpact form = new(args.GitModule);
            form.ShowDialog(args.OwnerForm);

            return false;
        }

        #endregion
    }
}
