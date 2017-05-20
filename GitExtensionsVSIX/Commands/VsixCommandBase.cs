using EnvDTE80;
using GitPluginShared.Commands;
using Microsoft.VisualStudio.Shell;

namespace GitExtensionsVSIX.Commands
{
    public class VsixCommandBase
    {
        public readonly CommandBase BaseCommand;

        public VsixCommandBase(CommandBase aBaseCommand)
        {
            BaseCommand = aBaseCommand;
        }

        virtual public void BeforeQueryStatus(DTE2 application, OleMenuCommand menuCommand)
        {
            menuCommand.Enabled = BaseCommand.IsEnabled(application);
        }

    }
}
