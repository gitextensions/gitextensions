using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GitCommands.Git;
using GitExtUtils;
using GitUIPluginInterfaces;

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
        public static void Add(this ArgumentBuilder builder, bool condition, string? ifConditionTrue)
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
        public static void Add(this ArgumentBuilder builder, bool condition, string? ifConditionTrue, string? ifConditionFalse)
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
        public static void Add(this ArgumentBuilder builder, IEnumerable<string?>? values)
        {
            if (values is null)
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
        public static void Add(this ArgumentBuilder builder, bool condition, IEnumerable<string?>? ifConditionTrue)
        {
            if (!condition || ifConditionTrue is null)
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

            string? GetArgument()
            {
                return option switch
                {
                    ForcePushOptions.Force => "-f",
                    ForcePushOptions.ForceWithLease => "--force-with-lease",
                    ForcePushOptions.DoNotForce => null,
                    _ => throw new InvalidEnumArgumentException(nameof(option), (int)option, typeof(ForcePushOptions))
                };
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
                return mode switch
                {
                    UntrackedFilesMode.Default => "--untracked-files",
                    UntrackedFilesMode.No => "--untracked-files=no",
                    UntrackedFilesMode.Normal => "--untracked-files=normal",
                    UntrackedFilesMode.All => "--untracked-files=all",
                    _ => throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(UntrackedFilesMode))
                };
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
                return mode switch
                {
                    IgnoreSubmodulesMode.Default => "--ignore-submodules",
                    IgnoreSubmodulesMode.None => "--ignore-submodules=none",
                    IgnoreSubmodulesMode.Untracked => "--ignore-submodules=untracked",
                    IgnoreSubmodulesMode.Dirty => "--ignore-submodules=dirty",
                    IgnoreSubmodulesMode.All => "--ignore-submodules=all",
                    _ => throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(IgnoreSubmodulesMode))
                };
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
                return mode switch
                {
                    CleanMode.OnlyNonIgnored => "",
                    CleanMode.OnlyIgnored => "-X",
                    CleanMode.All => "-x",
                    _ => throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(CleanMode))
                };
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
                return mode switch
                {
                    ResetMode.ResetIndex => "",
                    ResetMode.Soft => "--soft",
                    ResetMode.Mixed => "--mixed",
                    ResetMode.Keep => "--keep",
                    ResetMode.Merge => "--merge",
                    ResetMode.Hard => "--hard",
                    _ => throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(ResetMode))
                };
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
                return option switch
                {
                    GitBisectOption.Good => "good",
                    GitBisectOption.Bad => "bad",
                    GitBisectOption.Skip => "skip",
                    _ => throw new InvalidEnumArgumentException(nameof(option), (int)option, typeof(GitBisectOption))
                };
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
        public static void Add(this ArgumentBuilder builder, ObjectId? objectId)
        {
            if (objectId is null)
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
        public static void Add(this ArgumentBuilder builder, IEnumerable<ObjectId?>? objectIds)
        {
            if (objectIds is null)
            {
                return;
            }

            foreach (var objectId in objectIds)
            {
                builder.Add(objectId);
            }
        }

        /// <summary>
        /// Split arguments exceeding max length into multiple batches.
        /// Windows by default limit arguments length less than 32767 <see cref="short.MaxValue"/>.
        /// Implementation by <see cref="System.Diagnostics.Process"/> will have file path included in command line arguments,
        /// as well as added quotation and space characters, so we need base length to account for all these added characters
        /// <see href="https://referencesource.microsoft.com/#system/services/monitoring/system/diagnosticts/Process.cs,1944"/>
        /// </summary>
        /// <param name="builder">Argument builder instance.</param>
        /// <param name="arguments">Arguments.</param>
        /// <param name="baseLength">Base executable file and command line length.</param>
        /// <param name="maxLength">Command line max length. Default is 32767 - 1 on Windows.</param>
        /// <returns>Array of batch arguments split by max length.</returns>
        public static List<BatchArgumentItem> BuildBatchArguments(this ArgumentBuilder builder, IEnumerable<string> arguments, int? baseLength = null, int maxLength = short.MaxValue)
        {
            // 3: double quotes + ' '
            // '"git.exe" ' is always included in final command line arguments
            baseLength ??= AppSettings.GitCommand.Length + 3;

            var baseArgument = builder.ToString();
            if (baseLength + baseArgument.Length >= maxLength)
            {
                throw new ArgumentException($"Git base command \"{baseArgument}\" always reached max length of {maxLength} characters.", nameof(baseArgument));
            }

            // Clone command as argument builder
            List<BatchArgumentItem> batches = new();
            var currentBatchItemCount = 0;
            var currentArgumentLength = baseArgument.Length;
            var lastBatchBuilder = arguments.Aggregate(builder, (currentBatchBuilder, argument) =>
            {
                // 1: ' ' space character length will be added
                // When enumeration is finished, no need to add ' ' to length calculation
                if (baseLength + currentArgumentLength + 1 + argument.Length >= maxLength)
                {
                    // Handle abnormal case when base command and a single argument exceed max length
                    if (currentBatchItemCount == 0)
                    {
                        throw new ArgumentException($"Git command \"{currentBatchBuilder}\" always exceeded max length of {maxLength} characters.", nameof(arguments));
                    }

                    // Finish current command line
                    batches.Add(new BatchArgumentItem(currentBatchBuilder, currentBatchItemCount));

                    // Return new argument builder
                    currentBatchItemCount = 1;
                    currentArgumentLength = baseArgument.Length + 1 + argument.Length;
                    return new ArgumentBuilder() { baseArgument, argument };
                }

                currentBatchBuilder.Add(argument);
                currentArgumentLength += argument.Length + 1;
                currentBatchItemCount++;
                return currentBatchBuilder;
            });

            // Handle rare case when last argument length exceed max length
            if (baseLength + currentArgumentLength >= maxLength)
            {
                throw new ArgumentException($"Git command \"{lastBatchBuilder}\" always exceeded max length of {maxLength} characters.", nameof(arguments));
            }

            // Add last commandline batch
            if (!lastBatchBuilder.IsEmpty)
            {
                batches.Add(new BatchArgumentItem(lastBatchBuilder, currentBatchItemCount));
            }

            return batches;
        }

        public static List<BatchArgumentItem> BuildBatchArgumentsForFiles(this ArgumentBuilder builder, IEnumerable<string> files)
            => builder.BuildBatchArguments(files.Select(f => f.ToPosixPath().QuoteNE()));
    }
}
