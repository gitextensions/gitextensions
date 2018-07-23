namespace GitUIPluginInterfaces
{
    public interface IGitCommand
    {
        /// <value>if command accesses remote repository</value>
        bool AccessesRemote { get; }

        /// <value>true if repo state changes after executing this command</value>
        bool ChangesRepoState { get; }

        /// <returns>git command arguments as single line</returns>
        string Arguments { get; }
    }
}