﻿using System.Collections.Concurrent;

namespace GitExtUtils
{
    public sealed class GitCommandConfiguration
    {
        private readonly ConcurrentDictionary<string, GitConfigItem[]> _configByCommand
            = new(StringComparer.Ordinal);

        /// <summary>
        /// Gets the default configuration for git commands used by Git Extensions.
        /// </summary>
        public static GitCommandConfiguration Default { get; }

        static GitCommandConfiguration()
        {
            // The set of default configuration items for Git Extensions
            Default = new GitCommandConfiguration();

            Default.Add(new GitConfigItem("rebase.autoSquash", "false"), "rebase");

            Default.Add(new GitConfigItem("log.showSignature", "false"), "log", "show", "whatchanged");

            Default.Add(new GitConfigItem("color.ui", "never"), "diff", "range-diff");
            Default.Add(new GitConfigItem("diff.submodule", "short"), "diff");
            Default.Add(new GitConfigItem("diff.noprefix", "false"), "diff");
            Default.Add(new GitConfigItem("diff.mnemonicprefix", "false"), "diff");
            Default.Add(new GitConfigItem("diff.ignoreSubmodules", "none"), "diff", "status");
            Default.Add(new GitConfigItem("core.safecrlf", "false"), "diff");
        }

        /// <summary>
        /// Registers <paramref name="configItem"/> against one or more command names.
        /// </summary>
        /// <param name="configItem">The config item to register.</param>
        /// <param name="commands">One or more command names to register this config item against.</param>
        public void Add(GitConfigItem configItem, params string[] commands)
        {
            foreach (var command in commands)
            {
                _configByCommand.AddOrUpdate(
                    command,
                    addValueFactory: _ => new[] { configItem },
                    updateValueFactory: (_, items) => items.AppendTo(configItem));
            }
        }

        /// <summary>
        /// Retrieves the set of default config items for the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command to retrieve default config items for.</param>
        /// <returns>The default config items for <paramref name="command"/>.</returns>
        public IReadOnlyList<GitConfigItem> Get(string command)
        {
            return _configByCommand.TryGetValue(command, out var items)
                ? items
                : Array.Empty<GitConfigItem>();
        }
    }
}
