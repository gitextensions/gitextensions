namespace GitCommands.UserRepositoryHistory
{
    /// <summary>
    /// Provides a convinient and centralised way of dealing with histories of local and remote repositories.
    /// </summary>
    public static class RepositoryHistoryManager
    {
        static RepositoryHistoryManager()
        {
            var repositoryStorage = new RepositoryStorage();
            Locals = new LocalRepositoryManager(repositoryStorage, new Legacy.RepositoryHistoryMigrator());
            Remotes = new RemoteRepositoryManager(repositoryStorage);
        }

        /// <summary>
        /// Provides an access to the local repositories history manager.
        /// </summary>
        public static ILocalRepositoryManager Locals { get; }

        /// <summary>
        /// Provides an access to the remote repositories history manager.
        /// </summary>
        public static IRepositoryManager Remotes { get; }
    }
}
