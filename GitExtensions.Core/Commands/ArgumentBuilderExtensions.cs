using System;
using System.Collections.Generic;
using GitExtensions.Core.Module;
using JetBrains.Annotations;

namespace GitExtensions.Core.Commands
{
    /// <summary>
    /// Extension methods for working with <see cref="ArgumentBuilder"/>.
    /// </summary>
    public static class ArgumentBuilderExtensions
    {
        /// <summary>
        /// Adds <paramref name="ifConditionTrue"/> to the argument list if <paramref name="condition"/>
        /// is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// If <paramref name="ifConditionTrue"/> is <c>null</c> or white-space, then no change is made
        /// to the argument list, regardless of the value of <paramref name="condition"/>.
        /// </remarks>
        /// <param name="builder">The <see cref="ArgumentBuilder"/> to add arguments to.</param>
        /// <param name="condition">Whether or not to add <paramref name="ifConditionTrue"/> to the argument list.</param>
        /// <param name="ifConditionTrue">The string to add if <paramref name="condition"/> is <c>true</c>.</param>
        public static void Add(this ArgumentBuilder builder, bool condition, [CanBeNull] string ifConditionTrue)
        {
            if (condition)
            {
                builder.Add(ifConditionTrue);
            }
        }

        /// <summary>
        /// Adds either <paramref name="ifConditionTrue"/> or <paramref name="ifConditionFalse"/> to the
        /// argument list, depending upon the value of <paramref name="condition"/>.
        /// </summary>
        /// <remarks>
        /// If <paramref name="ifConditionTrue"/> is <c>null</c> or white-space, then no change is made
        /// to the argument list, regardless of the value of <paramref name="condition"/>.
        /// </remarks>
        /// <param name="builder">The <see cref="ArgumentBuilder"/> to add arguments to.</param>
        /// <param name="condition">Whether or not to add <paramref name="ifConditionTrue"/> to the argument list.</param>
        /// <param name="ifConditionTrue">The string to add if <paramref name="condition"/> is <c>true</c>.</param>
        /// <param name="ifConditionFalse">The string to add if <paramref name="condition"/> is <c>false</c>.</param>
        public static void Add(this ArgumentBuilder builder, bool condition, [CanBeNull] string ifConditionTrue, [CanBeNull] string ifConditionFalse)
        {
            builder.Add(condition ? ifConditionTrue : ifConditionFalse);
        }

        /// <summary>
        /// Adds all non-<c>null</c> <paramref name="values"/> to the argument list.
        /// </summary>
        /// <remarks>
        /// <para>If <paramref name="values"/> is <c>null</c> or empty, then no change is made to the argument list.</para>
        ///
        /// <para>Any <c>null</c> values in the enumeration will be skipped, which can be convenient in calling constructions.</para>
        /// </remarks>
        /// <param name="builder">The <see cref="ArgumentBuilder"/> to add arguments to.</param>
        /// <param name="values">A sequence of strings to add.</param>
        public static void Add(this ArgumentBuilder builder, [CanBeNull, ItemCanBeNull] IEnumerable<string> values)
        {
            if (values == null)
            {
                return;
            }

            foreach (var value in values)
            {
                builder.Add(value);
            }
        }

        /// <summary>
        /// Adds all non-<c>null</c> <paramref name="ifConditionTrue"/> values to the argument list if <paramref name="condition"/>
        /// is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// <para>If <paramref name="ifConditionTrue"/> is <c>null</c> or empty, then no change is made
        /// to the argument list, regardless of the value of <paramref name="condition"/>.</para>
        ///
        /// <para>Any <c>null</c> values in the enumeration will be skipped, which can be convenient in calling constructions.</para>
        /// </remarks>
        /// <param name="builder">The <see cref="ArgumentBuilder"/> to add arguments to.</param>
        /// <param name="condition">Whether or not to add <paramref name="ifConditionTrue"/> to the argument list.</param>
        /// <param name="ifConditionTrue">A sequence of strings to add if <paramref name="condition"/> is <c>true</c>.</param>
        public static void Add(this ArgumentBuilder builder, bool condition, [CanBeNull, ItemCanBeNull] IEnumerable<string> ifConditionTrue)
        {
            if (!condition || ifConditionTrue == null)
            {
                return;
            }

            foreach (var value in ifConditionTrue)
            {
                builder.Add(value);
            }
        }

        /// <summary>
        /// Adds <paramref name="objectId"/> as a SHA-1 argument.
        /// </summary>
        /// <remarks>
        /// If <paramref name="objectId"/> is <c>null</c> then no change is made to the arguments.
        /// </remarks>
        /// <param name="builder">The <see cref="ArgumentBuilder"/> to add arguments to.</param>
        /// <param name="objectId">The SHA-1 object ID to add to the builder, or <c>null</c>.</param>
        /// <exception cref="ArgumentException"><paramref name="objectId"/> represents an artificial commit.</exception>
        public static void Add(this ArgumentBuilder builder, [CanBeNull] ObjectId objectId)
        {
            if (objectId == null)
            {
                return;
            }

            if (objectId.IsArtificial)
            {
                throw new ArgumentException("Unexpected artificial commit in Git command: " + objectId);
            }

            builder.Add(objectId.ToString());
        }

        /// <summary>
        /// Adds a sequence of <paramref name="objectIds"/> to the builder.
        /// </summary>
        /// <remarks>
        /// If <paramref name="objectIds"/> is <c>null</c> then no change is made to the arguments.
        /// </remarks>
        /// <param name="builder">The <see cref="ArgumentBuilder"/> to add arguments to.</param>
        /// <param name="objectIds">A sequence of SHA-1 object IDs to add to the builder, or <c>null</c>.</param>
        /// <exception cref="ArgumentException"><paramref name="objectIds"/> contains an artificial commit.</exception>
        public static void Add(this ArgumentBuilder builder, [CanBeNull, ItemCanBeNull] IEnumerable<ObjectId> objectIds)
        {
            if (objectIds == null)
            {
                return;
            }

            foreach (var objectId in objectIds)
            {
                builder.Add(objectId);
            }
        }
    }
}
