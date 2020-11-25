using System;
using System.Collections.Generic;

namespace GitUI.Commands
{
    public sealed class GitExtensionCommandFactory : IGitExtensionCommandFactory
    {
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
            { };
        }

        public IGitExtensionCommand Create()
        {
            if (_arguments.Length <= 1)
            {
                // until we complete the migration
                return null;
            }

            var command = _arguments[1];

            if (_factories.TryGetValue(command, out Func<IGitExtensionCommand> factory))
            {
                return factory();
            }

            // until we complete the migration
            return null;
        }
    }
}
