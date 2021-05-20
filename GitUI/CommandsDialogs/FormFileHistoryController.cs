using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using GitCommands.Utils;
using Microsoft;

namespace GitUI.CommandsDialogs
{
    public sealed class FormFileHistoryController
    {
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
        public bool TryGetExactPath(string? path, [NotNullWhen(returnValue: true)] out string? exactPath)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                exactPath = null;
                return false;
            }

            Validates.NotNull(path);

            // The section below contains native windows (kernel32) calls
            // and breaks on Linux. Only use it on Windows. Casing is only
            // a Windows problem anyway.
            if (EnvUtils.RunningOnWindows())
            {
                // grab the 8.3 file path
                StringBuilder shortPath = new(4096);
                if (NativeMethods.GetShortPathNameW(path, shortPath, shortPath.Capacity) > 0)
                {
                    // use 8.3 file path to get properly cased full file path
                    StringBuilder longPath = new(4096);
                    if (NativeMethods.GetLongPathNameW(shortPath.ToString(), longPath, longPath.Capacity) > 0)
                    {
                        exactPath = longPath.ToString();
                        return true;
                    }
                }
            }

            exactPath = path;
            return true;
        }
    }
}
