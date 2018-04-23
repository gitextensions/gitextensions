using System;

namespace GitCommands.Git
{
    public class GitCommandResult
    {
        public GitCommandResult(string output)
        {
            Output = output;
        }

        public bool IsError => GitModule.IsGitErrorMessage(Output);
        public string Output { get; }
        public string[] Lines => Output.SplitLines();
    }
}