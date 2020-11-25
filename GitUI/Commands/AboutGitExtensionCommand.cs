using System.Windows.Forms;
using GitUI.CommandsDialogs;

namespace GitUI.Commands
{
    internal sealed class AboutGitExtensionCommand : IGitExtensionCommand
    {
        public bool Execute()
        {
            Application.Run(new FormAbout
            {
                StartPosition = FormStartPosition.CenterScreen
            });

            return true;
        }
    }
}
