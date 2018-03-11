using EnvDTE;
using EnvDTE80;
using GitPluginShared.Git;

namespace GitPluginShared.Commands
{
    public abstract class CommandBase
    {
        public abstract void OnCommand(DTE2 application, OutputWindowPane pane);

        public abstract bool IsEnabled(DTE2 application);

        protected static void RunGitEx(string command, string filename, string[] arguments = null)
        {
            GitCommands.RunGitEx(command, filename, arguments);
        }

        public bool RunForSelection { get; set; }
    }
}