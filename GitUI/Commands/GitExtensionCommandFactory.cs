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
        private const string BranchCommandName = "branch";
        private const string BrowseCommandName = "browse";
        private const string CheckoutCommandName = "checkout";
        private const string CheckoutBranchCommandName = "checkoutbranch";
        private const string CheckoutRevisionCommandName = "checkoutrevision";
        private const string CherryCommandName = "cherry";
        private const string CleanupCommandName = "cleanup";
        private const string CloneCommandName = "clone";
        private const string CommitCommandName = "commit";
        private const string DifftoolCommandName = "difftool";
        private const string RemotesCommandName = "remotes";
        private const string RevertCommandName = "revert";
        private const string ResetCommandName = "reset";
        private const string SearchFileCommandName = "searchfile";
        private const string SettingsCommandName = "settings";
        private const string StashCommandName = "stash";
        private const string TagCommandName = "tag";
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
                [BranchCommandName] = CreateBranchCommand,

                // [path] [-filter]
                [BrowseCommandName] = CreateBrowseCommand,
                [CheckoutCommandName] = CreateCheckoutCommand,
                [CheckoutBranchCommandName] = CreateCheckoutCommand,
                [CheckoutRevisionCommandName] = CreateCheckoutRevisionCommand,
                [CherryCommandName] = CreateCherryCommand,
                [CleanupCommandName] = CreateCleanupCommand,

                // [path]
                [CloneCommandName] = CreateCloneCommand,

                // [--quiet]
                [CommitCommandName] = CreateCommitCommand,

                // filename
                [DifftoolCommandName] = CreateDifftoolCommand,
                [RemotesCommandName] = CreateRemotesCommand,

                // [filename]
                [RevertCommandName] = CreateRevertCommand,
                [ResetCommandName] = CreateResetCommand,
                [SearchFileCommandName] = CreateSearchFileCommand,
                [SettingsCommandName] = CreateSettingsCommand,
                [StashCommandName] = CreateStashCommand,
                [TagCommandName] = CreateTagCommand,
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

            if (_arguments[1].StartsWith("git://") || _arguments[1].StartsWith("http://") || _arguments[1].StartsWith("https://"))
            {
                return CreateCloneCommand(url: _arguments[1], openedFromProtocolHandler: true);
            }

            if (_arguments[1].StartsWith("github-windows://openRepo/"))
            {
                return CreateCloneCommand(url: _arguments[1].Replace("github-windows://openRepo/", string.Empty), openedFromProtocolHandler: true);
            }

            if (_arguments[1].StartsWith("github-mac://openRepo/"))
            {
                return CreateCloneCommand(url: _arguments[1].Replace("github-mac://openRepo/", string.Empty), openedFromProtocolHandler: true);
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

        private IGitExtensionCommand CreateBranchCommand()
        {
            return new BranchGitExtensionCommand(_gitUICommands);
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

        private IGitExtensionCommand CreateCheckoutCommand()
        {
            return new CheckoutGitExtensionCommand(_gitUICommands);
        }

        private IGitExtensionCommand CreateCheckoutRevisionCommand()
        {
            return new CheckoutRevisionGitExtensionCommand(_gitUICommands);
        }

        private IGitExtensionCommand CreateCherryCommand()
        {
            return new CherryGitExtensionCommand(_gitUICommands);
        }

        private IGitExtensionCommand CreateCleanupCommand()
        {
            return new CleanupGitExtensionCommand(_gitUICommands);
        }

        private IGitExtensionCommand CreateCloneCommand()
        {
            return CreateCloneCommand(url: null, openedFromProtocolHandler: false);
        }

        private IGitExtensionCommand CreateCloneCommand(string url = null, bool openedFromProtocolHandler = false)
        {
            return new CloneGitExtensionCommand(_gitUICommands, url ?? (_arguments.Length > 2 ? _arguments[2] : null), openedFromProtocolHandler);
        }

        private IGitExtensionCommand CreateCommitCommand()
        {
            var arguments = InitializeArguments(_arguments);

            arguments.TryGetValue("message", out string overridingMessage);

            var showOnlyWhenChanges = arguments.ContainsKey("quiet");

            return new CommitGitExtensionCommand(_gitUICommands, overridingMessage, showOnlyWhenChanges);
        }

        private IGitExtensionCommand CreateDifftoolCommand()
        {
            if (_arguments.Length <= 2)
            {
                throw new InvalidOperationException("Cannot open difftool, there is no file selected.|Difftool");
            }

            return new DifftoolGitExtensionCommand(_gitUICommands, fileName: _arguments[2]);
        }

        private IGitExtensionCommand CreateRemotesCommand()
        {
            return new RemotesGitExtensionCommand(_gitUICommands);
        }

        private IGitExtensionCommand CreateRevertCommand()
        {
            if (_arguments.Length <= 2)
            {
                throw new InvalidOperationException("Cannot open revert, there is no file selected.|Revert");
            }

            return new ResetGitExtensionCommand(_gitUICommands, fileName: _arguments[2]);
        }

        private IGitExtensionCommand CreateResetCommand()
        {
            return new ResetGitExtensionCommand(_gitUICommands, fileName: string.Empty);
        }

        private IGitExtensionCommand CreateSearchFileCommand()
        {
            return new SearchFileGitExtensionCommand(_gitUICommands);
        }

        private IGitExtensionCommand CreateSettingsCommand()
        {
            return new SettingsGitExtensionCommand(_gitUICommands);
        }

        private IGitExtensionCommand CreateStashCommand()
        {
            return new StashGitExtensionCommand(_gitUICommands);
        }

        private IGitExtensionCommand CreateTagCommand()
        {
            return new TagGitExtensionCommand(_gitUICommands);
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

        private static IReadOnlyDictionary<string, string> InitializeArguments(IReadOnlyList<string> args)
        {
            var arguments = new Dictionary<string, string>();

            for (int i = 2; i < args.Count; i++)
            {
                if (args[i].StartsWith("--") && i + 1 < args.Count && !args[i + 1].StartsWith("--"))
                {
                    arguments.Add(args[i].TrimStart('-'), args[++i]);
                }
                else if (args[i].StartsWith("--"))
                {
                    arguments.Add(args[i].TrimStart('-'), null);
                }
            }

            return arguments;
        }
    }
}
