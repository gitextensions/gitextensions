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
        string Generate(string workingDir = null, bool isValidWorkingDir = false, string branchName = null);
    }

    /// <summary>
    /// Generates application title.
    /// </summary>
    public sealed class AppTitleGenerator : IAppTitleGenerator
    {
        private readonly IRepositoryDescriptionProvider _description;

        public AppTitleGenerator(IRepositoryDescriptionProvider description)
        {
            _description = description;
        }

        /// <summary>
        /// Generates main window title according to given repository.
        /// </summary>
        /// <param name="workingDir">Path to repository.</param>
        /// <param name="isValidWorkingDir">Indicates whether the given path contains a valid repository.</param>
        /// <param name="branchName">Current branch name.</param>
        public string Generate(string workingDir = null, bool isValidWorkingDir = false, string branchName = null)
        {
            if (string.IsNullOrWhiteSpace(workingDir) || !isValidWorkingDir)
            {
                return "Git Extensions";
            }

            branchName = branchName?.Trim('(', ')') ?? "no branch";

            var description = _description.Get(workingDir);

#if DEBUG
            return $"{description} ({branchName}) - Git Extensions [DEBUG]";
#else
            return $"{description} ({branchName}) - Git Extensions";
#endif
        }
    }
}