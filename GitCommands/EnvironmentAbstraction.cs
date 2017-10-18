using System;

namespace GitCommands
{
    public interface IEnvironmentAbstraction
    {
        /// <summary>Terminates this process and returns an exit code to the operating system.</summary>
        /// <param name="exitCode">
        /// The exit code to return to the operating system. Use 0 (zero) to indicate that
        /// the process completed successfully.
        /// </param>
        void Exit(int exitCode);

        /// <summary>Returns a string array containing the command-line arguments for the current process.</summary>
        /// <returns>
        /// An array of string where each element contains a command-line argument. 
        /// The first element is the executable file name, and the following zero or more elements
        /// contain the remaining command-line arguments.
        /// </returns>
        string[] GetCommandLineArgs();

        /// <summary>
        /// Retrieves the value of an environment variable from the current process or
        /// from the Windows operating system registry key for the current user.
        /// </summary>
        /// <param name="variable">The name of an environment variable.</param>
        /// <returns>
        /// The value of the environment variable specified by the <paramref name="variable" />, or <see langword="null"/> if the environment variable is not found.
        /// </returns>
        string GetEnvironmentVariable(string variable);

        /// <summary>
        /// Retrieves the value of an environment variable from the current process or
        /// from the Windows operating system registry key for the current user or local machine.
        /// </summary>
        /// <param name="variable">The name of an environment variable.</param>
        /// <param name="target">One of the <see cref="T:System.EnvironmentVariableTarget" /> values.</param>
        /// <returns>
        /// The value of the environment variable specified by the <paramref name="variable" /> and <paramref name="target" /> parameters, or <see langword="null"/> if the environment variable is not found.
        /// </returns>
        string GetEnvironmentVariable(string variable, EnvironmentVariableTarget target);

        /// <summary>Gets the path to the system special folder that is identified by the specified enumeration.</summary>
        /// <returns>The path to the specified system special folder, if that folder physically exists on your computer; otherwise, an empty string ("").A folder will not physically exist if the operating system did not create it, the existing folder was deleted, or the folder is a virtual directory, such as My Computer, which does not correspond to a physical path.</returns>
        /// <param name="folder">An enumerated constant that identifies a system special folder.</param>
        string GetFolderPath(Environment.SpecialFolder folder);
    }

    public sealed class EnvironmentAbstraction : IEnvironmentAbstraction
    {
        /// <summary>Terminates this process and returns an exit code to the operating system.</summary>
        /// <param name="exitCode">
        /// The exit code to return to the operating system. Use 0 (zero) to indicate that
        /// the process completed successfully.
        /// </param>
        public void Exit(int exitCode)
        {
            Environment.Exit(exitCode);
        }

        /// <summary>Returns a string array containing the command-line arguments for the current process.</summary>
        /// <returns>
        /// An array of string where each element contains a command-line argument. 
        /// The first element is the executable file name, and the following zero or more elements
        /// contain the remaining command-line arguments.
        /// </returns>
        public string[] GetCommandLineArgs()
        {
            return Environment.GetCommandLineArgs();
        }

        /// <summary>Retrieves the value of an environment variable from the current process or
        /// from the Windows operating system registry key for the current user.</summary>
        /// <param name="variable">The name of an environment variable.</param>
        /// <returns>
        /// The value of the environment variable specified by the <paramref name="variable" />, or <see langword="null"/> if the environment variable is not found.
        /// </returns>
        public string GetEnvironmentVariable(string variable)
        {
            return Environment.GetEnvironmentVariable(variable);
        }

        /// <summary>Retrieves the value of an environment variable from the current process or
        /// from the Windows operating system registry key for the current user or local machine.</summary>
        /// <param name="variable">The name of an environment variable.</param>
        /// <param name="target">One of the <see cref="T:System.EnvironmentVariableTarget" /> values.</param>
        /// <returns>
        /// The value of the environment variable specified by the <paramref name="variable" /> and <paramref name="target" /> parameters, or <see langword="null"/> if the environment variable is not found.
        /// </returns>
        public string GetEnvironmentVariable(string variable, EnvironmentVariableTarget target)
        {
            return Environment.GetEnvironmentVariable(variable, target);
        }

        /// <summary>Gets the path to the system special folder that is identified by the specified enumeration.</summary>
        /// <returns>The path to the specified system special folder, if that folder physically exists on your computer; otherwise, an empty string ("").A folder will not physically exist if the operating system did not create it, the existing folder was deleted, or the folder is a virtual directory, such as My Computer, which does not correspond to a physical path.</returns>
        /// <param name="folder">An enumerated constant that identifies a system special folder.</param>
        public string GetFolderPath(Environment.SpecialFolder folder)
        {
            return Environment.GetFolderPath(folder);
        }
    }
}