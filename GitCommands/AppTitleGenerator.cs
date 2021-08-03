using System;
using System.ComponentModel.Composition;
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
        /// <param name="defaultBranchName">Default branch name if <paramref name="branchName"/> is null (but not empty).</param>
        /// <param name="pathName">Current pathfilter.</param>
        string Generate(string? workingDir = null, bool isValidWorkingDir = false, string? branchName = null, string defaultBranchName = "", string? pathName = null);
    }

    /// <summary>
    /// Generates application title.
    /// </summary>
    [Export(typeof(IAppTitleGenerator))]
    public sealed class AppTitleGenerator : IAppTitleGenerator
    {
        private readonly IRepositoryDescriptionProvider _descriptionProvider;
#if DEBUG
        private static string? _extraInfo;
#endif

        [ImportingConstructor]
        public AppTitleGenerator(IRepositoryDescriptionProvider descriptionProvider)
        {
            _descriptionProvider = descriptionProvider;
        }

        /// <inheritdoc />
        public string Generate(string? workingDir = null, bool isValidWorkingDir = false, string? branchName = null, string defaultBranchName = "", string? pathName = null)
        {
            if (string.IsNullOrWhiteSpace(workingDir) || !isValidWorkingDir)
            {
                return AppSettings.ApplicationName;
            }

            branchName = branchName?.Trim('(', ')') ?? defaultBranchName;

            // Pathname normally have quotes already
            pathName = string.IsNullOrWhiteSpace(pathName)
                ? ""
                    : pathName.StartsWith(@"""") && pathName.EndsWith(@"""")
                        ? $"{pathName} "
                        : $"{pathName.Quote()} ";

            var description = _descriptionProvider.Get(workingDir);

#if DEBUG
            return $"{description} ({branchName}) {pathName}- {AppSettings.ApplicationName}{_extraInfo}";
#else
            return $"{description} ({branchName}) {pathName}- {AppSettings.ApplicationName}";
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
