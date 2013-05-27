using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitImpact
{
    public class GitImpactPlugin : GitPluginBase, IGitPluginForRepository
    {
        #region IGitPlugin Members

        public override string Description
        {
            get { return "Impact Graph"; }
        }

        public override bool Execute(GitUIBaseEventArgs gitUIEventArgs)
        {
            if (string.IsNullOrEmpty(gitUIEventArgs.GitModule.GitWorkingDir))
                return false;

            using (FormImpact form = new FormImpact(gitUIEventArgs.GitModule))
                form.ShowDialog(gitUIEventArgs.OwnerForm as IWin32Window);
            return false;
        }

        #endregion
    }
}
