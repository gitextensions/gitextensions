using System;
using System.Collections.Generic;

namespace GitUI.Commands
{
    public sealed class GitExtensionCommandFactory : IGitExtensionCommandFactory
    {
        private const string AboutCommandName = "about";
        private const string BrowseCommandName = "browse";

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

                // [path] [-filter]
                [BrowseCommandName] = CreateBrowseCommand
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
    }
}
