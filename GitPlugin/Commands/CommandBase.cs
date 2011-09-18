using EnvDTE;
using EnvDTE80;
using GitPlugin.Git;

namespace GitPlugin.Commands
{
    public abstract class CommandBase
    {
        abstract public void OnCommand(DTE2 application, OutputWindowPane pane);

        abstract public bool IsEnabled(DTE2 application);

        protected static void RunGitEx(string command, string filename)
        {
            GitPluginCommands.RunGitEx(command, filename);
        }
    }
}