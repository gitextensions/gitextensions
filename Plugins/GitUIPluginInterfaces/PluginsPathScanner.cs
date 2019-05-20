using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitUIPluginInterfaces
{
    /// <summary>
    /// A helper class for scanning for plugin files.
    /// </summary>
    public static class PluginsPathScanner
    {
        /// <summary>
        /// Searches for plugin files in the <paramref name="pluginsPaths"/>.
        /// Returns dll files
        /// - located in the folder for backward compatibility,
        /// - located in the first level of sub directories for the new plugin structure.
        /// </summary>
        /// <example>
        /// Plugins
        ///     \OldPlugin1.dll
        ///     \OldPlugin2.dll
        ///     \OldPlugin2.config
        ///     \NewPlugin1
        ///         \Plugin1.dll
        ///         \Plugin1.config
        ///     \NewPlugin2
        ///         \Plugin2.dll
        ///         \Plugin2.config
        /// </example>
        /// <param name="pluginsPaths">An array of filesystem paths to search for plugins in.</param>
        /// <returns>An enumeration of found dll files.</returns>
        public static IEnumerable<FileInfo> GetFiles(params string[] pluginsPaths)
        {
            var result = Enumerable.Empty<FileInfo>();

            foreach (string pluginsPath in pluginsPaths)
            {
                if (!string.IsNullOrEmpty(pluginsPath) && Directory.Exists(pluginsPath))
                {
                    DirectoryInfo directory = new DirectoryInfo(pluginsPath);

                    result = Enumerable.Concat(result, directory.GetFiles("*.dll"));

                    foreach (var childDirectory in directory.GetDirectories())
                    {
                        result = Enumerable.Concat(result, childDirectory.GetFiles("*.dll"));
                    }
                }
            }

            return result;
        }
    }
}
