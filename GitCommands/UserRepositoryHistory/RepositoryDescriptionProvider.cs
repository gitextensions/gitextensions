using System;
using System.IO;
using System.Linq;
using GitCommands.Git;
using JetBrains.Annotations;

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
        /// <returns>Short name for repository</returns>
        string Get(string repositoryDir);
    }

    public sealed class RepositoryDescriptionProvider : IRepositoryDescriptionProvider
    {
        private const string RepositoryDescriptionFileName = "description";
        private const string DefaultDescription = "Unnamed repository; edit this file 'description' to name the repository.";
        private readonly IGitDirectoryResolver _gitDirectoryResolver;

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
        /// <returns>Short name for repository</returns>
        public string Get(string repositoryDir)
        {
            var dirInfo = new DirectoryInfo(repositoryDir);
            if (!dirInfo.Exists)
            {
                return dirInfo.Name;
            }

            string desc = ReadRepositoryDescription(repositoryDir);
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
        [CanBeNull]
        private string ReadRepositoryDescription(string workingDir)
        {
            var gitDir = _gitDirectoryResolver.Resolve(workingDir);
            var descriptionFilePath = Path.Combine(gitDir, RepositoryDescriptionFileName);

            if (!File.Exists(descriptionFilePath))
            {
                return null;
            }

            try
            {
                var repositoryDescription = File.ReadLines(descriptionFilePath).FirstOrDefault();
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