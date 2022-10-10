﻿using System.IO.Abstractions;
using GitCommands.Git.Commands;
using GitCommands.Git.Extensions;
using GitUIPluginInterfaces;

namespace GitCommands.Git.Tag
{
    public interface IGitTagController
    {
        /// <summary>
        /// Create the Tag depending on input parameter.
        /// </summary>
        /// <param name="args">tag creation arguments</param>
        /// <param name="parentWindow">the UI window to act as the parent of the create tag dialog</param>
        /// <returns>the true if the tag is created.</returns>
        bool CreateTag(GitCreateTagArgs args, IWin32Window parentWindow);
    }

    public class GitTagController : IGitTagController
    {
        private readonly IFileSystem _fileSystem;
        private readonly IGitUICommands _uiCommands;

        public GitTagController(IGitUICommands uiCommands, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _uiCommands = uiCommands;
        }

        public GitTagController(IGitUICommands uiCommands)
            : this(uiCommands, new FileSystem())
        {
        }

        /// <summary>
        /// Create the Tag depending on input parameter.
        /// </summary>
        /// <param name="args">tag creation arguments</param>
        /// <param name="parentWindow">the UI window to act as the parent of the create tag dialog</param>
        /// <returns>the true if the tag is created.</returns>
        public bool CreateTag(GitCreateTagArgs args, IWin32Window parentWindow)
        {
            if (parentWindow is null)
            {
                throw new ArgumentNullException(nameof(parentWindow));
            }

            string? tagMessageFileName = null;
            if (args.Operation.CanProvideMessage())
            {
                tagMessageFileName = Path.Combine(GetWorkingDirPath(), "TAGMESSAGE");
                _fileSystem.File.WriteAllText(tagMessageFileName, args.TagMessage);
            }

            GitCreateTagCmd createTagCmd = new(args, _uiCommands.GitModule.GetGitExecPath(tagMessageFileName));
            try
            {
                return _uiCommands.StartCommandLineProcessDialog(parentWindow, createTagCmd);
            }
            finally
            {
                if (tagMessageFileName is not null && _fileSystem.File.Exists(tagMessageFileName))
                {
                    _fileSystem.File.Delete(tagMessageFileName);
                }
            }
        }

        private string GetWorkingDirPath()
        {
            return _uiCommands.GitModule.WorkingDir;
        }
    }
}
