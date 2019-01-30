using System;

namespace GitCommands.Git
{
    public sealed class GitModuleEventArgs : EventArgs
    {
        public GitModuleEventArgs(GitModule gitModule)
        {
            GitModule = gitModule;
        }

        public GitModule GitModule { get; }
    }
}