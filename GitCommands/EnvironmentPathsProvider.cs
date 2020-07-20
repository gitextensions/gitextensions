using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitCommands.Utils;

namespace GitCommands
{
    public interface IEnvironmentPathsProvider
    {
        /// <summary>
        /// Gets the list of paths defined under %PATH% environment variable.
        /// </summary>
        /// <returns>The list of paths defined under %PATH% environment variable.</returns>
        IEnumerable<string> GetEnvironmentPaths();

        /// <summary>
        /// Gets the list of valid paths defined under %PATH% environment variable.
        /// </summary>
        /// <returns>The list of valid paths defined under %PATH% environment variable.</returns>
        IEnumerable<string> GetEnvironmentValidPaths();
    }

    public sealed class EnvironmentPathsProvider : IEnvironmentPathsProvider
    {
        private readonly IEnvironmentAbstraction _environment;

        public EnvironmentPathsProvider(IEnvironmentAbstraction environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// Gets the list of paths defined under %PATH% environment variable.
        /// </summary>
        /// <returns>The list of paths defined under %PATH% environment variable.</returns>
        public IEnumerable<string> GetEnvironmentPaths()
        {
            string pathVariable = _environment.GetEnvironmentVariable("PATH");

            if (string.IsNullOrWhiteSpace(pathVariable))
            {
                yield break;
            }

            foreach (string rawDir in pathVariable.Split(EnvUtils.EnvVariableSeparator))
            {
                string dir = rawDir;

                // Usually, paths with spaces are not quoted on %PATH%, but it's well possible, and .NET won't consume a quoted path
                // This does not handle the full grammar of the %PATH%, but at least prevents Illegal Characters in Path exceptions (see #2924)
                dir = dir.Trim(' ', '"', '\t');
                if (dir.Length == 0)
                {
                    continue;
                }

                yield return dir;
            }
        }

        /// <summary>
        /// Gets the list of valid paths defined under %PATH% environment variable.
        /// </summary>
        /// <returns>The list of valid paths defined under %PATH% environment variable.</returns>
        public IEnumerable<string> GetEnvironmentValidPaths()
        {
            var envPaths = GetEnvironmentPaths();
            return envPaths.Where(IsValidPath);
        }

        // TODO: optimise?
        internal static bool IsValidPath(string path)
        {
            try
            {
                _ = new FileInfo(path).Attributes;
                return true;
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }
            catch (NotSupportedException)
            {
            }

            return false;
        }
    }
}
