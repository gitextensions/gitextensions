using GitCommands.UserRepositoryHistory;

namespace GitCommands
{
    /// <summary>
    /// Provides the ability to generate application title.
    /// </summary>
    public interface IAppTitleGenerator
    {
        /// <summary>
        /// Generates main window title according to given repository.
        /// </summary>
        /// <param name="workingDir">Path to repository.</param>
        /// <param name="isValidWorkingDir">Indicates whether the given path contains a valid repository.</param>
        /// <param name="branchName">Current branch name.</param>
        string Generate(string workingDir, bool isValidWorkingDir, string branchName);
    }

    /// <summary>
    /// Generates application title.
    /// </summary>
    public sealed class AppTitleGenerator : IAppTitleGenerator
    {
        private const string DefaultTitle = "Git Extensions";
        private const string RepositoryTitleFormat = "{0} ({1}) - Git Extensions";
        private readonly IRepositoryDescriptionProvider _repositoryDescriptionProvider;

        public AppTitleGenerator(IRepositoryDescriptionProvider repositoryDescriptionProvider)
        {
            _repositoryDescriptionProvider = repositoryDescriptionProvider;
        }

        /// <summary>
        /// Generates main window title according to given repository.
        /// </summary>
        /// <param name="workingDir">Path to repository.</param>
        /// <param name="isValidWorkingDir">Indicates whether the given path contains a valid repository.</param>
        /// <param name="branchName">Current branch name.</param>
        public string Generate(string workingDir, bool isValidWorkingDir, string branchName)
        {
            if (string.IsNullOrWhiteSpace(workingDir) || !isValidWorkingDir)
            {
                return DefaultTitle;
            }

            string repositoryDescription = _repositoryDescriptionProvider.Get(workingDir);
            var title = string.Format(RepositoryTitleFormat, repositoryDescription, (branchName ?? "no branch").Trim('(', ')'));
#if DEBUG
            title += " -> DEBUG <-";
#endif
            return title;
        }
    }
}