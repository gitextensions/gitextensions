using System.Windows.Forms;
using GitUI.CommandsDialogs;

namespace GitUI.Commands
{
    internal sealed class CommandlineHelpGitExtensionCommand : IGitExtensionCommand
    {
        public bool Execute()
        {
            Application.Run(new FormCommandlineHelp
            {
                StartPosition = FormStartPosition.CenterScreen
            });

            return true;
        }
    }
}
