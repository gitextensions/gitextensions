using System;
using System.IO;
using System.IO.Abstractions;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public interface ICommitTemplateManager
    {
        /// <summary>
        /// Loads commit template from the file specified in .git/config
        /// under <c>commit.template</c> setting.
        /// </summary>
        /// <exception cref="FileNotFoundException">The specified template file cannot be found.</exception>
        /// <returns>The commit template, if it is specified; otherwise <see langword="null"/>.</returns>
        /// <remarks>
        /// Template file can be set by the following command:
        /// <c>$ git config --global commit.template ~/.git_commit_msg.txt</c>
        /// </remarks>
        string LoadGitCommitTemplate();
    }

    public sealed class CommitTemplateManager : ICommitTemplateManager
    {
        private readonly IFileSystem _fileSystem;
        private readonly IGitModule _module;


        public CommitTemplateManager(IGitModule module, IFileSystem fileSystem)
        {
            _module = module;
            _fileSystem = fileSystem;
        }

        public CommitTemplateManager(IGitModule module)
            : this(module, new FileSystem())
        {
        }


        /// <summary>
        /// Loads commit template from the file specified in .git/config
        /// under <c>commit.template</c> setting.
        /// </summary>
        /// <exception cref="FileNotFoundException">The specified template file cannot be found.</exception>
        /// <returns>The commit template, if it is specified; otherwise <see langword="null"/>.</returns>
        /// <remarks>
        /// Template file can be set by the following command:
        /// <c>$ git config --global commit.template ~/.git_commit_msg.txt</c>
        /// </remarks>
        public string LoadGitCommitTemplate()
        {
            string fileName = _module.GetEffectiveSetting("commit.template");
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            if (!Path.IsPathRooted(fileName))
            {
                var path = Path.GetFullPath(Path.Combine(_module.WorkingDir ?? "", fileName));
                var uri = new Uri(path);
                fileName = uri.LocalPath;
            }

            if (!_fileSystem.File.Exists(fileName))
            {
                throw new FileNotFoundException("File not found", fileName);
            }

            string commitTemplate = _fileSystem.File.ReadAllText(fileName, _module.CommitEncoding).Replace("\r", "");
            return commitTemplate;
        }
    }
}