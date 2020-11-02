using System.ComponentModel.Composition;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using GitImpact.Properties;
using ResourceManager;

namespace GitImpact
{
    [Export(typeof(IGitPlugin))]
    public class GitImpactPlugin : GitPluginBase,
        IGitPluginForRepository,
        IGitPluginExecutable
    {
        public GitImpactPlugin()
        {
            SetNameAndDescription("Impact Graph");
            Translate();
            Icon = Resources.IconGitImpact;
        }

        #region IGitPlugin Members

        public bool Execute(GitUIEventArgs args)
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
