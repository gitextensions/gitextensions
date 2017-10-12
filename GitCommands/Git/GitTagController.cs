using System.IO;
using System.IO.Abstractions;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public interface IGitTagController
    {
        /// <summary>
        /// Create the Tag depending on input parameter.
        /// </summary>
        /// <returns><see langword="true"/> if create tag command succeeded, <see langword="false"/> otherwise</returns>
        bool CreateTag(GitCreateTagArgs args, IWin32Window parentForm);
    }


    public class GitTagController : IGitTagController
    {
        private readonly IGitUICommands _uiCommands;
        private readonly IGitModule _module;
        private readonly IFileSystem _fileSystem;

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
        /// <returns>Output string from RunGitCmd.</returns>
        public bool CreateTag(GitCreateTagArgs args, IWin32Window parentForm)
        {
            string tagMessageFileName = null;
            if (args.Operation.CanProvideMessage())
            {
                tagMessageFileName = Path.Combine(_module.WorkingDirGitDir, "TAGMESSAGE");
                _fileSystem.File.WriteAllText(tagMessageFileName, args.TagMessage);
            }

            var createTagCmd = new GitCreateTagCmd(args, tagMessageFileName);
            return _uiCommands.StartCommandLineProcessDialog(createTagCmd, parentForm);
        }
    }
}
