using System;
using System.IO;
using System.IO.Abstractions;

namespace GitCommands.Git.Tag
{
    public interface IGitTagController
    {
        /// <summary>
        /// Create the Tag depending on input parameter.
        /// </summary>
        /// <returns><see langword="true"/> if create tag command succeeded, <see langword="false"/> otherwise</returns>
        GitCreateTagCmd GetCreateTagCommand(GitCreateTagArgs args);
    }

    public class GitTagController : IGitTagController
    {
        private readonly Func<string> _getWorkingDir;
        private readonly IFileSystem _fileSystem;

        public GitTagController(Func<string> getWorkingDir, IFileSystem fileSystem)
        {
            _getWorkingDir = getWorkingDir;
            _fileSystem = fileSystem;
        }

        public GitTagController(Func<string> getWorkingDir)
            : this(getWorkingDir, new FileSystem())
        { }

        /// <summary>
        /// Create the Tag depending on input parameter.
        /// </summary>
        /// <returns>Output string from RunGitCmd.</returns>
        public GitCreateTagCmd GetCreateTagCommand(GitCreateTagArgs args)
        {
            string tagMessageFileName = null;
            if (args.Operation.CanProvideMessage())
            {
                tagMessageFileName = Path.Combine(_getWorkingDir(), "TAGMESSAGE");
                _fileSystem.File.WriteAllText(tagMessageFileName, args.TagMessage);
            }

            var createTagCmd = new GitCreateTagCmd(args, tagMessageFileName);
            return createTagCmd;
        }
    }
}
