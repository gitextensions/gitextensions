using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace GitExtensionsVSIX.Commands
{
    public class VsixCommandBase
    {
        public readonly CommandBase BaseCommand;

        public VsixCommandBase(CommandBase baseCommand)
        {
            BaseCommand = baseCommand;
        }

        public virtual void BeforeQueryStatus(_DTE application, OleMenuCommand menuCommand)
        {
            menuCommand.Enabled = BaseCommand.IsEnabled(application);
        }
    }
}
