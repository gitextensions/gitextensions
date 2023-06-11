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

            if (string.IsNullOrWhiteSpace(branchName))
            {
                branchName = defaultBranchName;
            }

            // Pathname normally have quotes already
            pathName = GetFileName(pathName);

            var description = _descriptionProvider.Get(workingDir);

#if DEBUG
            return $"{pathName}{description} ({branchName}) - {AppSettings.ApplicationName}{_extraInfo}";
#else
            return $"{pathName}{description} ({branchName}) - {AppSettings.ApplicationName}";
#endif

            static string? GetFileName(string? path)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return null;
                }

                string filePart = Path.GetFileName(path.Trim('"')).QuoteNE();
                if (string.IsNullOrWhiteSpace(filePart))
                {
                    // No file, just quote the pathFilter
                    filePart = path.StartsWith(@"""") && path.EndsWith(@"""")
                        ? path
                        : $"{path.Quote()}";
                }

                return $"{filePart} ";
            }
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
