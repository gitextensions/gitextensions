using System.Drawing;

namespace GitUI.Shells
{
    public abstract class ShellDescriptor : IShellDescriptor
    {
        /// <inheritdoc/>
        public string? ExecutableCommandLine { get; protected set; }

        /// <inheritdoc/>
        public string ExecutableName { get; protected set; } = null!;

        /// <inheritdoc/>
        public string? ExecutablePath { get; protected set; }

        /// <inheritdoc/>
        public bool HasExecutable => ExecutablePath is not null;

        /// <inheritdoc/>
        public Image Icon { get; protected set; } = null!;

        /// <inheritdoc/>
        public string Name { get; protected set; } = null!;

        /// <inheritdoc/>
        public abstract string GetChangeDirCommand(string path);
    }
}
