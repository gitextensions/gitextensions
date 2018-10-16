using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace GitCommands
{
    /// <summary>
    /// Builds a command line argument string from zero or more arguments.
    /// </summary>
    /// <remarks>
    /// <para>To retrieve the constructed argument list string, call <see cref="ToString"/>.</para>
    ///
    /// <para>Arguments are separated by a single space character.</para>
    ///
    /// <para>Adding <c>null</c> or white-space strings has no effect on the output, which can be
    /// useful in some calling constructions.</para>
    ///
    /// <para>This class has been designed to work with C# collection initialiser syntax which makes
    /// its use quite ergonomic. See the example for more information.</para>
    ///
    /// <para>The type accepts strings, however conversion from other types is achieved via extension
    /// methods by adding a method named <c>Add</c> that accepts the required type.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var args = new ArgumentBuilder
    /// {
    ///     "commit",                   // adds the string unconditionally
    ///     { isAmend, "--amend" },     // adds the option only if isAmend == true
    ///     { isUp, "--up", "--down" }, // selects the option based on the value of isUp
    /// };
    /// </code>
    /// </example>
    public class ArgumentBuilder : IEnumerable
    {
        private readonly StringBuilder _arguments = new StringBuilder(capacity: 16);

        public bool IsEmpty => _arguments.Length == 0;
        public int Length => _arguments.Length;

        /// <summary>
        /// Adds <paramref name="s"/> to the argument list.
        /// </summary>
        /// <remarks>
        /// If <paramref name="s"/> is <c>null</c> or white-space, then no change is made
        /// to the argument list.
        /// </remarks>
        /// <param name="s">The string to add.</param>
        public void Add([CanBeNull] string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return;
            }

            if (_arguments.Length != 0)
            {
                _arguments.Append(' ');
            }

            _arguments.Append(s);
        }

        /// <summary>
        /// Adds a range of arguments
        /// </summary>
        /// <param name="args">The arguments to add to this builder</param>
        public void AddRange(IEnumerable<string> args)
        {
            args = args.Where(a => !string.IsNullOrEmpty(a));
            foreach (string s in args)
            {
                Add(s);
            }
        }

        /// <summary>
        /// Returns the composed argument list as a string.
        /// </summary>
        public override string ToString()
        {
            return _arguments.ToString();
        }

        /// <summary>
        /// This method is only implemented to support collection initialiser syntax, and always
        /// throws if called.
        /// </summary>
        /// <exception cref="InvalidOperationException">Always thrown.</exception>
        [ContractAnnotation("=>halt")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new InvalidOperationException($"{nameof(IEnumerable)} only implemented to support collection initialiser syntax.");
        }
    }
}