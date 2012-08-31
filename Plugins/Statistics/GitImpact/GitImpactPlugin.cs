using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitImpact
{
    public class GitImpactPlugin : IGitPluginForRepository
    {
        #region IGitPlugin Members

        public string Description
        {
            get { return "Impact Graph"; }
        }

        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
        }

        public bool Execute(GitUIBaseEventArgs gitUIEventArgs)
        {
            if (string.IsNullOrEmpty(gitUIEventArgs.Module.GitWorkingDir))
                return false;

            using (FormImpact form = new FormImpact())
                form.ShowDialog(gitUIEventArgs.OwnerForm as IWin32Window);
            return false;
        }

        #endregion
    }
}
