using System.Threading;
using EnvDTE;
using GitExtensionsVSIX.Git;

namespace GitExtensionsVSIX.Commands
{
    public sealed class FindFile : ItemCommandBase
    {
        public override void OnCommand(_DTE application, OutputWindowPane pane)
        {
            ThreadPool.QueueUserWorkItem(
                        o =>
                        {
                            string file = GitCommands.RunGitExWait("searchfile", application.Solution.FullName);

                            if (string.IsNullOrEmpty(file?.Trim()))
                            {
                                return;
                            }

                            application.ExecuteCommand("File.OpenFile", file);
                        });
        }

        protected override CommandTarget SupportedTargets => CommandTarget.SolutionExplorerFileItem;

        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            throw new System.NotImplementedException();
        }
    }
}
