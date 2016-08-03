using System.Windows.Forms;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitImpact
{
    public class GitImpactPlugin : GitPluginBase, IGitPluginForRepository
    {
        public GitImpactPlugin()
        {
            SetNameAndDescription("Impact Graph");
            Translate();
        }

        #region IGitPlugin Members

        public override bool Execute(GitUIBaseEventArgs gitUIEventArgs)
        {
            if (string.IsNullOrEmpty(gitUIEventArgs.GitModule.WorkingDir))
                return false;

            using (var form = new FormImpact(gitUIEventArgs.GitModule))
                form.ShowDialog(gitUIEventArgs.OwnerForm);
            return false;
        }

        #endregion
    }
}
