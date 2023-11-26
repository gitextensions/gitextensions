using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public sealed class GitModuleEventArgs : EventArgs
    {
        public GitModuleEventArgs(IGitModule gitModule)
        {
            GitModule = gitModule;
        }

        public IGitModule GitModule { get; }
    }
}
