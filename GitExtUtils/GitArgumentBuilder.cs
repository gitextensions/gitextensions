using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
namespace GitCommands
{
    /// <summary>
    /// A configuration key/value pair for use in git command lines.
    /// </summary>
    public readonly struct GitConfigItem
    {
        public string Key { get; }
        public string Value { get; }

        public GitConfigItem(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public void Deconstruct(out string key, out string value)
        {
            key = Key;
            value = Value;
        }
    }

    public sealed class GitCommandConfiguration
    {
        private readonly ConcurrentDictionary<string, GitConfigItem[]> _configByCommand
            = new ConcurrentDictionary<string, GitConfigItem[]>(StringComparer.Ordinal);

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

            Default.Add(new GitConfigItem("diff.submodule", "short"), "diff");
            Default.Add(new GitConfigItem("diff.noprefix", "false"), "diff");
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
                    updateValueFactory: (_, items) => items.Append(configItem));
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

    /// <summary>
    /// Builds a git command line string from config items, a command name and that command's arguments.
    /// </summary>
    /// <remarks>
    /// <para>Derives from <see cref="ArgumentBuilder"/>, so read that class's documentation to learn more about
    /// its usage.</para>
    ///
    /// <para>
    /// A git command line is built from:
    /// <list type="number">
    ///   <item>Zero or more config items, each of form <c>-c key=value</c></item>
    ///   <item>A command name, such as <c>log</c></item>
    ///   <item>Zero or more arguments specific to that command</item>
    /// </list>
    /// </para>
    ///
    /// <para>Git Extensions defines a set of config items per command in the <see cref="GitCommandConfiguration"/> class.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var args = new GitArgumentBuilder("commit")
    /// {
    ///     "-S",                       // added unconditionally
    ///     { isAmend, "--amend" },     // adds the option only if isAmend == true
    ///     { isUp, "--up", "--down" }, // selects the option based on the value of isUp
    /// };
    /// </code>
    /// </example>
    public sealed class GitArgumentBuilder : ArgumentBuilder
    {
        private static readonly Regex _commandRegex = new Regex("^[a-z0-9_.-]+$", RegexOptions.Compiled);

        private readonly List<GitConfigItem> _configItems;
        private readonly ArgumentString _gitArgs;
        private readonly string _command;

        /// <summary>
        /// Initialises a new <see cref="GitArgumentBuilder"/> for the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The git command this builder is compiling arguments for.</param>
        /// <param name="commandConfiguration">Optional source for default command configuration items. Pass <c>null</c> to use the Git Extensions defaults.</param>
        /// <param name="gitOptions">Optional arguments that are for the git command.  EX: git --no-optional-locks status </param>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="command"/> is an invalid string.</exception>
        public GitArgumentBuilder([NotNull] string command, [CanBeNull] GitCommandConfiguration commandConfiguration = null, [CanBeNull] ArgumentString gitOptions = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (!_commandRegex.IsMatch(command))
            {
                throw new ArgumentException($"Git command \"{command}\" contains invalid characters.", nameof(command));
            }

            _command = command;
            _gitArgs = gitOptions;
            commandConfiguration = commandConfiguration ?? GitCommandConfiguration.Default;

            var defaultConfig = commandConfiguration.Get(command);
            _configItems = new List<GitConfigItem>(capacity: defaultConfig.Count + 2);
            if (defaultConfig.Count != 0)
            {
                _configItems.AddRange(defaultConfig);
            }
        }

        /// <summary>
        /// Add <paramref name="configItem"/> to this builder.
        /// </summary>
        /// <remarks>
        /// Any prior config item with the same key will be replaced.
        /// </remarks>
        /// <param name="configItem">The config item to add to the builder.</param>
        public void Add(GitConfigItem configItem)
        {
            // Append or replace config item based upon its key
            var index = _configItems.IndexOf(item => string.Equals(item.Key, configItem.Key, StringComparison.OrdinalIgnoreCase));

            if (index == -1)
            {
                _configItems.Add(configItem);
            }
            else
            {
                _configItems[index] = configItem;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var arguments = base.ToString();
            var gitArgsLength = _gitArgs.Length;
            if (gitArgsLength == 0)
            {
                gitArgsLength = -1; // prevent extra capacity when length is 0
            }

            // 7 = "-c " + "=" + " " + 2 (for possible quotes of value)
            var capacity = _configItems.Sum(i => i.Key.Length + i.Value.Length + 7) + _command.Length + 1 + arguments.Length;
            capacity += gitArgsLength + 1;

            var str = new StringBuilder(capacity);

            if (gitArgsLength > 0)
            {
                str.Append(_gitArgs.ToString());
                str.Append(' ');
            }

            foreach (var (key, value) in _configItems)
            {
                str.Append("-c ").Append(key).Append('=').AppendQuoted(value).Append(' ');
            }

            str.Append(_command);

            if (arguments.Length != 0)
            {
                str.Append(' ').Append(arguments);
            }

            Debug.Assert(str.Capacity == capacity, $"Did not allocate enough capacity for string buffer. Allocated {capacity} but final capacity was {str.Capacity}.");

            return str.ToString();
        }
    }
}