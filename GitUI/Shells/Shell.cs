#nullable enable

using System;

namespace GitUI.Shells
{
    public interface IShell : IHaveId<Guid>
    {
        /// <summary>
        /// Gets the user visible shell name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the shell icon key.
        /// </summary>
        public string Icon { get; }

        /// <summary>
        /// Gets the executable command.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Gets the executable command arguments.
        /// </summary>
        public string Arguments { get; }

        /// <summary>
        /// Gets the default flag.
        /// </summary>
        public bool Default { get; }

        /// <summary>
        /// Gets the enabled flag.
        /// </summary>
        public bool Enabled { get; }
    }

    internal sealed class Shell : IShell
    {
        /// <inheritdoc/>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <inheritdoc/>
        public string Name { get; set; } = ShellConstants.DefaultName;

        /// <inheritdoc/>
        public string Icon { get; set; } = ShellConstants.DefaultIcon;

        /// <inheritdoc/>
        public string Command { get; set; } = ShellConstants.DefaultCommand;

        /// <inheritdoc/>
        public string Arguments { get; set; } = ShellConstants.DefaultArguments;

        /// <inheritdoc/>
        public bool Default { get; set; }

        /// <inheritdoc/>
        public bool Enabled { get; set; }
    }
}
