using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GitCommands.Git;
using GitExtensions.Core.Commands;

namespace GitCommands
{
    /// <summary>
    /// Extension methods for working with <see cref="ArgumentBuilder"/>.
    /// </summary>
    public static class ArgumentBuilderExtensions
    {
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
                    case ResetMode.Keep:
                        return "--keep";
                    case ResetMode.Merge:
                        return "--merge";
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
            if (!baseLength.HasValue)
            {
                baseLength = AppSettings.GitCommand.Length + 3;
            }

            var baseArgument = builder.ToString();
            if (baseLength + baseArgument.Length >= maxLength)
            {
                throw new ArgumentException($"Git base command \"{baseArgument}\" always reached max length of {maxLength} characters.", nameof(baseArgument));
            }

            // Clone command as argument builder
            var batches = new List<BatchArgumentItem>();
            var currentBatchItemCount = 0;
            var currentArgumentLength = baseArgument.Length;
            var lastBatchBuilder = arguments.Aggregate(builder, (currentBatchBuilder, argument) =>
            {
                // 1: ' ' space character length will be added
                // When enumeration is finished, no need to add ' ' to length calculcation
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
