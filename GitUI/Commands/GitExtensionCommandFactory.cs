using System;
using System.Collections.Generic;
using GitCommands;

namespace GitUI.Commands
{
    public sealed class GitExtensionCommandFactory : IGitExtensionCommandFactory
    {
        private const string AboutCommandName = "about";
        private const string AddCommandName = "add";
        private const string AddFilesCommandName = "addfiles";
        private const string ApplyCommandName = "apply";
        private const string ApplyPatchCommandName = "applypatch";
        private const string BlameCommandName = "blame";
        private const string BrowseCommandName = "browse";
        private const string ViewDiffCommandName = "viewdiff";
        private const string ViewPatchCommandName = "viewpatch";
        private const string UninstallCommandName = "uninstall";

        private readonly string[] _arguments;
        private readonly GitUICommands _gitUICommands;
        private readonly Dictionary<string, Func<IGitExtensionCommand>> _factories;

        public GitExtensionCommandFactory(
            string[] arguments,
            GitUICommands gitUICommands)
        {
            _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));

            _factories = new Dictionary<string, Func<IGitExtensionCommand>>
            {
                [AboutCommandName] = CreateAboutCommand,

                // [filename]
                [AddCommandName] = CreateAddFilesCommand,
                [AddFilesCommandName] = CreateAddFilesCommand,

                // [filename]
                [ApplyCommandName] = CreateApplyPatchCommand,
                [ApplyPatchCommandName] = CreateApplyPatchCommand,

                // [filename]
                [BlameCommandName] = CreateBlameCommand,

                // [path] [-filter]
                [BrowseCommandName] = CreateBrowseCommand,
                [ViewDiffCommandName] = CreateViewDiffCommand,

                // [filename]
                [ViewPatchCommandName] = CreateViewPatchCommand,
                [UninstallCommandName] = CreateUninstallCommand
            };
        }

        public IGitExtensionCommand Create()
        {
            if (_arguments.Length <= 1)
            {
                return CreateBrowseCommand();
            }

            var command = _arguments[1];

            if (_factories.TryGetValue(command, out Func<IGitExtensionCommand> factory))
            {
                return factory();
            }

            // until we complete the migration
            return null;
        }

        private IGitExtensionCommand CreateAboutCommand()
        {
            return new AboutGitExtensionCommand();
        }

        private IGitExtensionCommand CreateAddFilesCommand()
        {
            return new AddFilesGitExtensionCommand(_gitUICommands, filter: _arguments.Length == 3 ? _arguments[2] : ".");
        }

        private IGitExtensionCommand CreateApplyPatchCommand()
        {
            return new ApplyPatchGitExtensionCommand(_gitUICommands, fileName: _arguments.Length == 3 ? _arguments[2] : string.Empty);
        }

        private IGitExtensionCommand CreateBlameCommand()
        {
            if (_arguments.Length <= 2)
            {
                throw new InvalidOperationException("Cannot open blame, there is no file selected.|Blame");
            }

            string blameFileName = NormalizeFileName(fileName: _arguments[2]);
            int? initialLine = null;

            if (_arguments.Length > 3 && int.TryParse(_arguments[3], out var temp))
            {
                initialLine = temp;
            }

            return new BlameGitExtensionCommand(_gitUICommands, blameFileName, initialLine);
        }

        private IGitExtensionCommand CreateBrowseCommand()
        {
            var filterParameter = GetParameterOrEmptyStringAsDefault(_arguments, paramName: "-filter");
            var selectedCommitParameter = GetParameterOrEmptyStringAsDefault(_arguments, paramName: "-commit");

            if (selectedCommitParameter == string.Empty)
            {
                return new BrowseGitExtensionCommand(_gitUICommands, filterParameter);
            }

            if (_gitUICommands.Module.TryResolvePartialCommitId(selectedCommitParameter, out var objectId))
            {
                return new BrowseGitExtensionCommand(_gitUICommands, filterParameter, selectedCommit: objectId);
            }

            Console.Error.WriteLine($"No commit found matching: {_arguments}.");

            throw new InvalidOperationException($"No commit found matching: {_arguments}.");
        }

        private IGitExtensionCommand CreateViewDiffCommand()
        {
            return new ViewDiffGitExtensionCommand(_gitUICommands);
        }

        private IGitExtensionCommand CreateViewPatchCommand()
        {
            return new ViewPatchGitExtensionCommand(_gitUICommands, patchFile: _arguments.Length == 3 ? _arguments[2] : string.Empty);
        }

        private IGitExtensionCommand CreateUninstallCommand()
        {
            return new UninstallGitExtensionCommand();
        }

        private static string GetParameterOrEmptyStringAsDefault(IReadOnlyList<string> args, string paramName)
        {
            var withEquals = paramName + "=";

            for (var i = 2; i < args.Count; i++)
            {
                var arg = args[i];

                if (arg.StartsWith(withEquals))
                {
                    return arg.Replace(withEquals, string.Empty);
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Remove working directory from filename and convert to POSIX path.
        /// This is to prevent filenames that are too long while there is room left when the workingdir was not in the path.
        /// </summary>
        private string NormalizeFileName(string fileName)
        {
            fileName = fileName.ToPosixPath();

            return string.IsNullOrEmpty(_gitUICommands.Module.WorkingDir)
                ? fileName
                : fileName.Replace(_gitUICommands.Module.WorkingDir.ToPosixPath(), string.Empty);
        }
    }
}
