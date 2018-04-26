using System;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    public interface IGitDescribeProvider
    {
        /// <summary>
        /// Runs <c>git describe</c> to find the most recent tag that is reachable from a commit.
        /// If the tag points to the commit, then only the tag is shown. Otherwise, it suffixes the tag name with the number
        /// of additional commits on top of the tagged object and the abbreviated object name of the most recent commit.
        /// </summary>
        /// <param name="revision">A revision to describe.</param>
        /// <returns>Describe information.</returns>
        (string precedingTag, string commitCount) Get(string revision);
    }

    public sealed class GitDescribeProvider : IGitDescribeProvider
    {
        private readonly Func<IGitModule> _getModule;

        public GitDescribeProvider(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }

        /// <summary>
        /// Runs <c>git describe</c> to find the most recent tag that is reachable from a commit.
        /// If the tag points to the commit, then only the tag is shown. Otherwise, it suffixes the tag name with the number
        /// of additional commits on top of the tagged object and the abbreviated object name of the most recent commit.
        /// </summary>
        /// <param name="revision">A revision to describe.</param>
        /// <returns>Describe information.</returns>
        public (string precedingTag, string commitCount) Get(string revision)
        {
            string description = GetModule().GetDescribe(revision);
            if (string.IsNullOrEmpty(description))
            {
                return (string.Empty, string.Empty);
            }

            int commitHashPos = description.LastIndexOf("-g", description.Length - 1, StringComparison.OrdinalIgnoreCase);
            if (commitHashPos == -1)
            {
                return (description, string.Empty);
            }

            string commitHash = description.Substring(commitHashPos + 2);
            if (commitHash.Length == 0 || !revision.StartsWith(commitHash, StringComparison.OrdinalIgnoreCase))
            {
                return (description, string.Empty);
            }

            description = description.Substring(0, commitHashPos);
            int commitCountPos = description.LastIndexOf("-", description.Length - 1, StringComparison.Ordinal);
            if (commitCountPos == -1)
            {
                return (description, string.Empty);
            }

            string commitCount = description.Substring(commitCountPos + 1);
            description = description.Substring(0, commitCountPos);
            return (description, commitCount);
        }

        [NotNull]
        private IGitModule GetModule()
        {
            var module = _getModule();

            if (module == null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
            }

            return module;
        }
    }
}