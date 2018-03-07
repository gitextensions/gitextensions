namespace GitUIPluginInterfaces
{
    public interface IGitCommand
    {
        /// <returns>if command accesses remote repository</returns>
        bool AccessesRemote();

        /// <returns>true if repo state changes after executing this command</returns>
        bool ChangesRepoState();

        /// <returns>git command arguments as single line</returns>
        string ToLine();
    }
}