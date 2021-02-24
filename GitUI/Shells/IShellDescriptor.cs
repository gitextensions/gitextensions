using System.Drawing;

namespace GitUI.Shells
{
    public interface IShellDescriptor
    {
        /// <summary>
        /// Gets the executable command line.
        /// </summary>
        public string? ExecutableCommandLine { get; }

        /// <summary>
        /// Gets the executable name.
        /// </summary>
        public string ExecutableName { get; }

        /// <summary>
        /// Gets the executable resolved path.
        /// </summary>
        public string? ExecutablePath { get; }

        /// <summary>
        /// Indicates whether the executable for the given shell is located and the shell can be run.
        /// </summary>
        public bool HasExecutable { get; }

        /// <summary>
        /// Gets the shell icon.
        /// </summary>
        public Image Icon { get; }

        /// <summary>
        /// Gets the user visible shell name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the command to execute when a working directory is switched.
        /// </summary>
        /// <param name="path">The working directory path.</param>
        /// <returns>The chdir command.</returns>
        public string GetChangeDirCommand(string path);
    }
}
