using GitCommands.UserRepositoryHistory;
using GitUIPluginInterfaces;

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
#if DEBUG
        private static string _extraInfo;
#endif

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
                return AppSettings.ApplicationName;
            }

            branchName = branchName?.Trim('(', ')') ?? "no branch";

            var description = _description.Get(workingDir);

#if DEBUG
            return $"{description} ({branchName}) - {AppSettings.ApplicationName}{_extraInfo}";
#else
            return $"{description} ({branchName}) - {AppSettings.ApplicationName}";
#endif
        }

        public static void Initialise(string sha, string buildBranch)
        {
#if DEBUG
            if (ObjectId.TryParse(sha, out var objectId))
            {
                _extraInfo = $" {objectId.ToShortString()}";
                if (!string.IsNullOrWhiteSpace(buildBranch))
                {
                    _extraInfo += $" ({buildBranch})";
                }
            }
            else
            {
                _extraInfo = " [DEBUG]";
            }
#endif
        }
    }
}
