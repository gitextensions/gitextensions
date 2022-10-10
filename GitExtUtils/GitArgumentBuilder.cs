﻿using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace GitExtUtils
{
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
    /// GitArgumentBuilder args = new("commit")
    /// {
    ///     "-S",                       // added unconditionally
    ///     { isAmend, "--amend" },     // adds the option only if isAmend == true
    ///     { isUp, "--up", "--down" }, // selects the option based on the value of isUp
    /// };
    /// </code>
    /// </example>
    public sealed class GitArgumentBuilder : ArgumentBuilder
    {
        private static readonly Regex CommandRegex = new("^[a-z0-9_.-]+$", RegexOptions.Compiled);

        private readonly List<GitConfigItem> _configItems;
        private readonly ArgumentString _gitArgs;
        private readonly string _command;

        /// <summary>
        /// Initialises a new <see cref="GitArgumentBuilder"/> for the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The git command this builder is compiling arguments for.</param>
        /// <param name="commandConfiguration">Optional source for default command configuration items. Pass <c>null</c> to use the Git Extensions defaults.</param>
        /// <param name="gitOptions">Optional arguments that are for the git command.  EX: git --no-optional-locks status.</param>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="command"/> is an invalid string.</exception>
        public GitArgumentBuilder(string command, GitCommandConfiguration? commandConfiguration = null, ArgumentString gitOptions = default)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (!CommandRegex.IsMatch(command))
            {
                throw new ArgumentException($"Git command \"{command}\" contains invalid characters.", nameof(command));
            }

            _command = command;
            _gitArgs = gitOptions;
            commandConfiguration ??= GitCommandConfiguration.Default;

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

            StringBuilder str = new(capacity);

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
