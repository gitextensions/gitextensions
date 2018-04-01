using System;
using System.Collections;
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
    /// <para>The type accepts strings, however conversion from other types (eg. <see cref="ForcePushOptions"/>
    /// enum) is achieved via extension methods on <see cref="ArgumentBuilderExtensions"/> by adding a
    /// method named <c>Add</c> that accepts the required type.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var args = new ArgumentBuilder
    /// {
    ///     "commit",                   // adds the string unnconditionally
    ///     { isAmend, "--amend" },     // adds the option only if isAmend == true
    ///     { isUp, "--up", "--down" }, // selects the option based on the value of isUp
    /// };
    /// </code>
    /// </example>
    public sealed class ArgumentBuilder : IEnumerable
    {
        private readonly StringBuilder _command = new StringBuilder(capacity: 16);

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

            if (_command.Length != 0)
            {
                _command.Append(' ');
            }

            _command.Append(s);
        }

        /// <summary>
        /// Returns the composed argument list as a string.
        /// </summary>
        public override string ToString()
        {
            return _command.ToString();
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