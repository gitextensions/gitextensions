using GitUIPluginInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitCommands.Git
{
    public interface IGitTagController
    {
        /// <summary>
        /// Create the Tag depending on input parameter.
        /// </summary>
        /// <param name="revision">Commit revision to be tagged</param>
        /// <param name="tagName">Name of tag</param>
        /// <param name="force">Force parameter</param>
        /// <param name="operationType">The operation to perform on the tag  (Lightweight, Annotate, Sign with defaul key, Sign with specific key)</param>
        /// <param name="tagMessage">Tag Message</param>
        /// <param name="keyId">Specific Key ID to be used instead of default one</param>
        /// <returns>True if create tag command succeeded, False otherwise</returns>
        bool CreateTag(GitCreateTagArgs args, IWin32Window parentForm);
    }


    public class GitTagController : IGitTagController
    {
        private IGitUICommands _uiCommands;
        private IGitModule _module;
        private IFileSystem _fileSystem;

        public GitTagController(IGitUICommands uiCommands, IGitModule module, IFileSystem fileSystem)
        {
            _module = module;
            _uiCommands = uiCommands;
            _fileSystem = fileSystem;
        }

        public GitTagController(IGitUICommands uiCommands, IGitModule module)
            : this(uiCommands, module, new FileSystem())
        { }

        /// <summary>
        /// Create the Tag depending on input parameter.
        /// </summary>
        /// <param name="revision">Commit revision to be tagged</param>
        /// <param name="inputTagName">Name of tag</param>
        /// <param name="force">Force parameter</param>
        /// <param name="operationType">The operation to perform on the tag (Lightweight, Annotate, Sign with defaul key, Sign with specific key)</param>
        /// <param name="tagMessage">Tag Message</param>
        /// <param name="keyId">Specific Key ID to be used instead of default one</param>
        /// <returns>Output string from RunGitCmd.</returns>
        public bool CreateTag(GitCreateTagArgs args, IWin32Window parentForm)
        {
            GitCreateTagCmd createTagCmd = new GitCreateTagCmd(args);
            if (args.OperationType.CanProvideMessage())
            {
                createTagCmd.TagMessageFileName = Path.Combine(_module.GetGitDirectory(), "TAGMESSAGE");
                _fileSystem.File.WriteAllText(createTagCmd.TagMessageFileName, args.TagMessage);
            }

            return _uiCommands.StartCommandLineProcessDialog(createTagCmd, parentForm);
        }
    }
}
