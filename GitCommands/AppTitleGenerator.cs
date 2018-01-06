using System;
using GitCommands.Repository;

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
        private readonly IAppInformationalVersionProvider _appInformationalVersionProvider;
        private readonly IRepositoryShortNameProvider _repositoryShortNameProvider;


        public AppTitleGenerator(IAppInformationalVersionProvider appInformationalVersionProvider, IRepositoryShortNameProvider repositoryShortNameProvider)
        {
            _appInformationalVersionProvider = appInformationalVersionProvider;
            _repositoryShortNameProvider = repositoryShortNameProvider;
        }


        /// <summary>
        /// Generates main window title according to given repository.
        /// </summary>
        /// <param name="workingDir">Path to repository.</param>
        /// <param name="isValidWorkingDir">Indicates whether the given path contains a valid repository.</param>
        /// <param name="branchName">Current branch name.</param>
        public string Generate(string workingDir, bool isValidWorkingDir, string branchName)
        {
            if (string.IsNullOrWhiteSpace(workingDir))
            {
                throw new ArgumentException(nameof(workingDir));
            }

            if (!isValidWorkingDir)
            {
                return DefaultTitle;
            }
            string repositoryDescription = _repositoryShortNameProvider.Get(workingDir);
            var title = string.Format(RepositoryTitleFormat, repositoryDescription, (branchName ?? "no branch").Trim('(', ')'));

            var informationalVersion = _appInformationalVersionProvider.Get();
            if (!string.IsNullOrWhiteSpace(informationalVersion))
            {
                title += $" v{informationalVersion}";
            }
#if DEBUG
            title += " -> DEBUG <-";
#endif
            return title;
        }
    }
}