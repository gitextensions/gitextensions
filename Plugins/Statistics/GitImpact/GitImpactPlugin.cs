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

            using var form = new FormImpact(args.GitModule);
            form.ShowDialog(args.OwnerForm);

            return false;
        }

        #endregion
    }
}
