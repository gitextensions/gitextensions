using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using GitCommands.Git;

namespace GitCommands.UserRepositoryHistory
{
    public interface IRepositoryDescriptionProvider
    {
        /// <summary>
        /// Returns a short name for repository.
        /// If the repository contains a description it is returned,
        /// otherwise the last part of path is returned.
        /// </summary>
        /// <param name="repositoryDir">Path to repository.</param>
        /// <returns>Short name for repository.</returns>
        string Get(string repositoryDir);
    }

    /// <summary>
    ///  Provides the ability to read .git/description file.
    /// </summary>
    /// <remarks>
    ///  https://git-scm.com/book/en/v2/Git-Internals-Plumbing-and-Porcelain:
    ///  ...The description file is used only by the GitWeb program, so don’t worry about it...
    /// </remarks>
    [Export(typeof(IRepositoryDescriptionProvider))]
    public sealed class RepositoryDescriptionProvider : IRepositoryDescriptionProvider
    {
        private const string RepositoryDescriptionFileName = "description";
        private const string DefaultDescription = "Unnamed repository; edit this file 'description' to name the repository.";
        private readonly IGitDirectoryResolver _gitDirectoryResolver;

        [ImportingConstructor]
        public RepositoryDescriptionProvider(IGitDirectoryResolver gitDirectoryResolver)
        {
            _gitDirectoryResolver = gitDirectoryResolver;
        }

        /// <summary>
        /// Returns a short name for repository.
        /// If the repository contains a description it is returned,
        /// otherwise the last part of path is returned.
        /// </summary>
        /// <param name="repositoryDir">Path to repository.</param>
        /// <returns>Short name for repository.</returns>
        public string Get(string repositoryDir)
        {
            DirectoryInfo dirInfo = new(repositoryDir);
            if (!dirInfo.Exists)
            {
                return dirInfo.Name;
            }

            string? desc = ReadRepositoryDescription(repositoryDir);
            if (!string.IsNullOrWhiteSpace(desc))
            {
                return desc;
            }

            return dirInfo.Name;
        }

        /// <summary>
        /// Reads repository description's first line from ".git\description" file.
        /// </summary>
        /// <param name="workingDir">Path to repository.</param>
        /// <returns>If the repository has description, returns that description, else returns <c>null</c>.</returns>
        private string? ReadRepositoryDescription(string workingDir)
        {
            string gitDir = _gitDirectoryResolver.Resolve(workingDir);
            string descriptionFilePath = Path.Combine(gitDir, RepositoryDescriptionFileName);

            if (!File.Exists(descriptionFilePath))
            {
                return null;
            }

            try
            {
                string repositoryDescription = File.ReadLines(descriptionFilePath).FirstOrDefault();
                return string.Equals(repositoryDescription, DefaultDescription, StringComparison.CurrentCulture)
                    ? null
                    : repositoryDescription;
            }
            catch (IOException)
            {
                return null;
            }
        }
    }
}
