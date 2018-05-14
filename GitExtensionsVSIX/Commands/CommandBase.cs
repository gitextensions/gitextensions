using EnvDTE;
using GitExtensionsVSIX.Git;

namespace GitExtensionsVSIX.Commands
{
    public abstract class CommandBase
    {
        public abstract void OnCommand(_DTE application, OutputWindowPane pane);

        public abstract bool IsEnabled(_DTE application);

        protected static void RunGitEx(string command, string filename, string[] arguments = null)
        {
            GitCommands.RunGitEx(command, filename, arguments);
        }

        public bool RunForSelection { get; set; }
    }
}