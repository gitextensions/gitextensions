using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public interface ICommitTemplateManager
    {
        /// <summary>
        /// Gets the collection of all currently registered commit templates provided by plugins.
        /// </summary>
        IEnumerable<CommitTemplateItem> RegisteredTemplates { get; }

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

        /// <summary>
        /// Allows a plugin to register a new commit template.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        /// <param name="templateText">The body of the template.</param>
        void Register(string templateName, Func<string> templateText);

        /// <summary>
        /// Allows a plugin to unregister a commit template.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        void Unregister(string templateName);
    }

    public sealed class CommitTemplateManager : ICommitTemplateManager
    {
        private static readonly ConcurrentDictionary<string, Func<string>> RegisteredTemplatesDic = new ConcurrentDictionary<string, Func<string>>();
        private readonly IFileSystem _fileSystem;
        private readonly IGitModule _module;
        private readonly IFullPathResolver _fullPathResolver;

        public CommitTemplateManager(IGitModule module, IFullPathResolver fullPathResolver, IFileSystem fileSystem)
        {
            _module = module;
            _fullPathResolver = fullPathResolver;
            _fileSystem = fileSystem;
        }

        public CommitTemplateManager(IGitModule module)
            : this(module, new FullPathResolver(() => module.WorkingDir), new FileSystem())
        {
        }

        /// <summary>
        /// Gets the collection of all currently registered commit templates provided by plugins.
        /// </summary>
        public IEnumerable<CommitTemplateItem> RegisteredTemplates => RegisteredTemplatesDic.Select(item => new CommitTemplateItem(item.Key, item.Value()));

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

            fileName = _fullPathResolver.Resolve(fileName);
            if (!_fileSystem.File.Exists(fileName))
            {
                throw new FileNotFoundException("File not found", fileName);
            }

            string commitTemplate = _fileSystem.File.ReadAllText(fileName);
            return commitTemplate;
        }

        /// <summary>
        /// Allows a plugin to register a new commit template.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        /// <param name="templateText">The body of the template.</param>
        public void Register(string templateName, Func<string> templateText)
        {
            if (RegisteredTemplatesDic.ContainsKey(templateName))
            {
                return;
            }

            RegisteredTemplatesDic.TryAdd(templateName, templateText);
        }

        /// <summary>
        /// Allows a plugin to unregister a commit template.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        public void Unregister(string templateName)
        {
            RegisteredTemplatesDic.TryRemove(templateName, out _);
        }
    }
}