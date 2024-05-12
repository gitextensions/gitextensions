using GitExtensions.Extensibility.Git;

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
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Describe information.</returns>
        (string precedingTag, string commitCount) Get(ObjectId revision, CancellationToken cancellationToken = default);
    }

    public sealed class GitDescribeProvider : IGitDescribeProvider
    {
        private readonly Func<IGitModule> _getModule;

        public GitDescribeProvider(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }

        /// <inheritdoc />
        public (string precedingTag, string commitCount) Get(ObjectId revision, CancellationToken cancellationToken = default)
        {
            string? description = GetModule().GetDescribe(revision, cancellationToken);
            if (string.IsNullOrEmpty(description))
            {
                return (string.Empty, string.Empty);
            }

            int commitHashPos = description.LastIndexOf("-g", StringComparison.OrdinalIgnoreCase);
            if (commitHashPos == -1)
            {
                return (description, string.Empty);
            }

            string commitHash = description[(commitHashPos + 2)..];
            if (commitHash.Length == 0 || revision.ToString() != commitHash)
            {
                return (description, string.Empty);
            }

            description = description[..commitHashPos];
            int commitCountPos = description.LastIndexOf("-", StringComparison.Ordinal);
            if (commitCountPos == -1)
            {
                return (description, string.Empty);
            }

            string commitCount = description[(commitCountPos + 1)..];
            description = description[..commitCountPos];
            return (description, commitCount);

            IGitModule GetModule()
            {
                IGitModule module = _getModule();

                if (module is null)
                {
                    throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
                }

                return module;
            }
        }
    }
}
