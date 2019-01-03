using System;
using System.Collections.Generic;
using System.ComponentModel;
using GitCommands.Git;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands
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
        /// Adds the git argument syntax for members of the <see cref="ForcePushOptions"/> enum.
        /// </summary>
        /// <param name="builder">The <see cref="ArgumentBuilder"/> to add arguments to.</param>
        /// <param name="option">The enum member to add to the builder.</param>
        public static void Add(this ArgumentBuilder builder, ForcePushOptions option)
        {
            builder.Add(GetArgument());

            string GetArgument()
            {
                switch (option)
                {
                    case ForcePushOptions.Force:
                        return "-f";
                    case ForcePushOptions.ForceWithLease:
                        return "--force-with-lease";
                    case ForcePushOptions.DoNotForce:
                        return null;
                    default:
                        throw new InvalidEnumArgumentException(nameof(option), (int)option, typeof(ForcePushOptions));
                }
            }
        }

        /// <summary>
        /// Adds the git argument syntax for members of the <see cref="UntrackedFilesMode"/> enum.
        /// </summary>
        /// <param name="builder">The <see cref="ArgumentBuilder"/> to add arguments to.</param>
        /// <param name="mode">The enum member to add to the builder.</param>
        public static void Add(this ArgumentBuilder builder, UntrackedFilesMode mode)
        {
            builder.Add(GetArgument());

            string GetArgument()
            {
                switch (mode)
                {
                    case UntrackedFilesMode.Default:
                        return "--untracked-files";
                    case UntrackedFilesMode.No:
                        return "--untracked-files=no";
                    case UntrackedFilesMode.Normal:
                        return "--untracked-files=normal";
                    case UntrackedFilesMode.All:
                        return "--untracked-files=all";
                    default:
                        throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(UntrackedFilesMode));
                }
            }
        }

        /// <summary>
        /// Adds the git argument syntax for members of the <see cref="IgnoreSubmodulesMode"/> enum.
        /// </summary>
        /// <param name="builder">The <see cref="ArgumentBuilder"/> to add arguments to.</param>
        /// <param name="mode">The enum member to add to the builder.</param>
        public static void Add(this ArgumentBuilder builder, IgnoreSubmodulesMode mode)
        {
            builder.Add(GetArgument());

            string GetArgument()
            {
                switch (mode)
                {
                    case IgnoreSubmodulesMode.Default:
                        return "--ignore-submodules";
                    case IgnoreSubmodulesMode.None:
                        return "--ignore-submodules=none";
                    case IgnoreSubmodulesMode.Untracked:
                        return "--ignore-submodules=untracked";
                    case IgnoreSubmodulesMode.Dirty:
                        return "--ignore-submodules=dirty";
                    case IgnoreSubmodulesMode.All:
                        return "--ignore-submodules=all";
                    default:
                        throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(IgnoreSubmodulesMode));
                }
            }
        }

        /// <summary>
        /// Adds the git argument syntax for members of the <see cref="CleanMode"/> enum.
        /// </summary>
        /// <param name="builder">The <see cref="ArgumentBuilder"/> to add arguments to.</param>
        /// <param name="mode">The enum member to add to the builder.</param>
        public static void Add(this ArgumentBuilder builder, CleanMode mode)
        {
            builder.Add(GetArgument());

            string GetArgument()
            {
                switch (mode)
                {
                    case CleanMode.OnlyNonIgnored:
                        return "";
                    case CleanMode.OnlyIgnored:
                        return "-X";
                    case CleanMode.All:
                        return "-x";
                    default:
                        throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(CleanMode));
                }
            }
        }

        /// <summary>
        /// Adds the git argument syntax for members of the <see cref="ResetMode"/> enum.
        /// </summary>
        /// <param name="builder">The <see cref="ArgumentBuilder"/> to add arguments to.</param>
        /// <param name="mode">The enum member to add to the builder.</param>
        public static void Add(this ArgumentBuilder builder, ResetMode mode)
        {
            builder.Add(GetArgument());

            string GetArgument()
            {
                switch (mode)
                {
                    case ResetMode.ResetIndex:
                        return "";
                    case ResetMode.Soft:
                        return "--soft";
                    case ResetMode.Mixed:
                        return "--mixed";
                    case ResetMode.Hard:
                        return "--hard";
                    default:
                        throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(ResetMode));
                }
            }
        }

        /// <summary>
        /// Adds the git argument syntax for members of the <see cref="GitBisectOption"/> enum.
        /// </summary>
        /// <param name="builder">The <see cref="ArgumentBuilder"/> to add arguments to.</param>
        /// <param name="option">The enum member to add to the builder.</param>
        public static void Add(this ArgumentBuilder builder, GitBisectOption option)
        {
            builder.Add(GetArgument());

            string GetArgument()
            {
                switch (option)
                {
                    case GitBisectOption.Good:
                        return "good";
                    case GitBisectOption.Bad:
                        return "bad";
                    case GitBisectOption.Skip:
                        return "skip";
                    default:
                        throw new InvalidEnumArgumentException(nameof(option), (int)option, typeof(GitBisectOption));
                }
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