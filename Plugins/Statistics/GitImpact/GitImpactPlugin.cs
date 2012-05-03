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

        public bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            if (string.IsNullOrEmpty(gitUiCommands.GitWorkingDir))
                return false;

            FormImpact form = new FormImpact();
            form.ShowDialog(gitUiCommands.OwnerForm as IWin32Window);
            return false;
        }

        #endregion
    }
}
