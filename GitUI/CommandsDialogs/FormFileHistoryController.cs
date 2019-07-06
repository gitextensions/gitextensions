using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using GitCommands;
using GitCommands.Utils;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public sealed class FormFileHistoryController
    {
        private readonly Func<IGitModule> _getModule;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFileSystem _fileSystem;

        public FormFileHistoryController(Func<IGitModule> getModule, IFullPathResolver fullPathResolver, IFileSystem fileSystem)
        {
            _getModule = getModule;
            _fullPathResolver = fullPathResolver;
            _fileSystem = fileSystem;
        }

        public FormFileHistoryController(Func<IGitModule> getModule, IFullPathResolver fullPathResolver)
            : this(getModule, fullPathResolver, new FileSystem())
        {
        }

        /// <summary>
        /// Gets the exact case used on the file system for an existing file or directory.
        /// </summary>
        /// <param name="path">A relative or absolute path.</param>
        /// <param name="exactPath">The full path using the correct case if the path exists.  Otherwise, null.</param>
        /// <returns>True if the exact path was found.  False otherwise.</returns>
        /// <remarks>
        /// This supports drive-lettered paths and UNC paths, but a UNC root
        /// will be returned in lowercase (e.g., \\server\share).
        /// </remarks>
        [ContractAnnotation("=>false,exactPath:null")]
        [ContractAnnotation("=>true,exactPath:notnull")]
        [ContractAnnotation("path:null=>false,exactPath:null")]
        public bool TryGetExactPath(string path, out string exactPath)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                exactPath = null;
                return false;
            }

            // The section below contains native windows (kernel32) calls
            // and breaks on Linux. Only use it on Windows. Casing is only
            // a Windows problem anyway.
            if (EnvUtils.RunningOnWindows())
            {
                // grab the 8.3 file path
                var shortPath = new StringBuilder(4096);
                if (NativeMethods.GetShortPathName(path, shortPath, shortPath.Capacity) > 0)
                {
                    // use 8.3 file path to get properly cased full file path
                    var longPath = new StringBuilder(4096);
                    if (NativeMethods.GetLongPathName(shortPath.ToString(), longPath, longPath.Capacity) > 0)
                    {
                        exactPath = longPath.ToString();
                        return true;
                    }
                }
            }

            exactPath = path;
            return true;
        }

        public string SanitiseRelativeFilePath(string relativeFilePath, string fullFilePath)
        {
            if (TryGetExactPath(fullFilePath, out var exactFileName))
            {
                var module = GetModule();
                relativeFilePath = exactFileName.Substring(module.WorkingDir.Length);
            }

            // Replace windows path separator to Linux path separator.
            // This is needed to keep the file history working when started from file tree in
            // browse dialog.
            return relativeFilePath.ToPosixPath();
        }

        public (string revision, string path) BuildFilter(string relativeFilePath, string fullFilePath)
        {
            var res = (revision: (string)null, path: $" \"{relativeFilePath}\"");

            if (AppSettings.FollowRenamesInFileHistory && !Directory.Exists(fullFilePath))
            {
                // git log --follow is not working as expected (see  http://kerneltrap.org/mailarchive/git/2009/1/30/4856404/thread)
                //
                // But we can take a more complicated path to get reasonable results:
                //  1. use git log --follow to get all previous filenames of the file we are interested in
                //  2. use git log "list of files names" to get the history graph
                //
                // note: This implementation is quite a quick hack (by someone who does not speak C# fluently).
                //

                var args = new GitArgumentBuilder("log")
                    {
                        "--format=\"%n\"",
                        "--name-only",
                        "--follow",
                        GitCommandHelpers.FindRenamesAndCopiesOpts(),
                        "--",
                        relativeFilePath.Quote()
                    };

                var listOfFileNames = new StringBuilder(relativeFilePath.Quote());

                // keep a set of the file names already seen
                var setOfFileNames = new HashSet<string> { relativeFilePath };

                var module = GetModule();
                var lines = module.GitExecutable.GetOutputLines(args, outputEncoding: GitModule.LosslessEncoding);

                foreach (var line in lines.Select(GitModule.ReEncodeFileNameFromLossless))
                {
                    if (!string.IsNullOrEmpty(line) && setOfFileNames.Add(line))
                    {
                        listOfFileNames.Append(" \"");
                        listOfFileNames.Append(line);
                        listOfFileNames.Append('\"');
                    }
                }

                // here we need --name-only to get the previous filenames in the revision graph
                res.path = listOfFileNames.ToString();
                res.revision += " --name-only --parents" + GitCommandHelpers.FindRenamesAndCopiesOpts();
            }
            else if (AppSettings.FollowRenamesInFileHistory)
            {
                // history of a directory
                // --parents doesn't work with --follow enabled, but needed to graph a filtered log
                res.revision = " " + GitCommandHelpers.FindRenamesOpt() + " --follow --parents";
            }
            else
            {
                // rename following disabled
                res.revision = " --parents";
            }

            if (AppSettings.FullHistoryInFileHistory)
            {
                res.revision = string.Concat(" --full-history ", AppSettings.SimplifyMergesInFileHistory ? "--simplify-merges " : string.Empty, res.revision);
            }

            return res;
        }

        private IGitModule GetModule()
        {
            var module = _getModule();
            if (module == null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
            }

            return module;
        }
    }
}