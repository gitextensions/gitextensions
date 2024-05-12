using System.Collections.Concurrent;
using GitExtensions.Extensibility.Git;

namespace GitExtUtils
{
    public sealed class GitCommandConfiguration : IGitCommandConfiguration
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

            Default.Add(new GitConfigItem("color.ui", "never"), "diff", "range-diff", "grep");
            Default.Add(new GitConfigItem("diff.submodule", "short"), "diff");
            Default.Add(new GitConfigItem("diff.noprefix", "false"), "diff");
            Default.Add(new GitConfigItem("diff.mnemonicprefix", "false"), "diff");
            Default.Add(new GitConfigItem("diff.ignoreSubmodules", "none"), "diff", "status");
            Default.Add(new GitConfigItem("core.safecrlf", "false"), "diff");
        }

        public void Add(GitConfigItem configItem, params string[] commands)
        {
            foreach (string command in commands)
            {
                _configByCommand.AddOrUpdate(
                    command,
                    addValueFactory: _ => new[] { configItem },
                    updateValueFactory: (_, items) => items.AppendTo(configItem));
            }
        }

        public IReadOnlyList<GitConfigItem> Get(string command)
        {
            return _configByCommand.TryGetValue(command, out GitConfigItem[] items)
                ? items
                : Array.Empty<GitConfigItem>();
        }
    }
}
