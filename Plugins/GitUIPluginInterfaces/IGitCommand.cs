namespace GitUIPluginInterfaces
{
    public interface IGitCommand
    {
        /// <summary>
        ///
        /// </summary>
        /// <returns>if command accesses remote repository</returns>
        bool AccessesRemote();

        /// <summary>
        ///
        /// </summary>
        /// <returns>true if repo state changes after executing this command</returns>
        bool ChangesRepoState();

        /// <summary>
        ///
        /// </summary>
        /// <returns>git command arguments as single line</returns>
        string ToLine();
    }
}