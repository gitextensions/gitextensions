using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace CreateLocalBranches
{
	public class CreateLocalBranchesPlugin : IGitPluginForRepository
    {
        public string Description
        {
            get { return "Create local tracking branches"; }
        }

        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
        }

        public bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            new CreateLocalBranchesForm(gitUiCommands).ShowDialog(gitUiCommands.OwnerForm as IWin32Window);
            return true;
        }
    }
}
