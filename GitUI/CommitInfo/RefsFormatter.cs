using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GitCommands;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommitInfo
{
    public sealed class RefsFormatter
    {
        /// <summary>
        /// The number of displayed lines if the list is limited.
        /// </summary>
        private const int MaximumDisplayedLinesIfLimited = 12;

        /// <summary>
        /// The number of displayed refs if the list is limited.
        ///
        /// If limited, the line "[Show all]" and an empty line are added.
        /// Hence the list needs to be limited only if it exceeds MaximumDisplayedRefsIfLimited + 2.
        /// </summary>
        private const int MaximumDisplayedRefsIfLimited = MaximumDisplayedLinesIfLimited - 2;

        [NotNull]
        private readonly ILinkFactory _linkFactory;

        public RefsFormatter([NotNull] ILinkFactory linkFactory)
        {
            _linkFactory = linkFactory ?? throw new ArgumentNullException("RefsFormatter requires an ILinkFactory instance");
        }

        public string FormatBranches(IEnumerable<string> branches, bool showAsLinks, bool limit)
        {
            if (branches == null)
            {
                return string.Empty;
            }

            var (formattedBranches, truncated) = FilterAndFormatBranches(branches, showAsLinks, limit);
            return ToString(formattedBranches, Strings.ContainedInBranches, Strings.ContainedInNoBranch, "branches", truncated);
        }

        public string FormatTags(IReadOnlyList<string> tags, bool showAsLinks, bool limit)
        {
            if (tags == null)
            {
                return string.Empty;
            }

            bool truncate = limit && tags.Count > MaximumDisplayedLinesIfLimited;
            var formattedTags = FormatTags(truncate ? tags.Take(MaximumDisplayedRefsIfLimited) : tags);
            return ToString(formattedTags, Strings.ContainedInTags, Strings.ContainedInNoTag, "tags", truncate);

            IEnumerable<string> FormatTags(IEnumerable<string> selectedTags)
            {
                return selectedTags.Select(s => showAsLinks ? _linkFactory.CreateTagLink(s) : WebUtility.HtmlEncode(s));
            }
        }

        private (IEnumerable<string> formattedBranches, bool truncated) FilterAndFormatBranches(IEnumerable<string> branches, bool showAsLinks, bool limit)
        {
            var formattedBranches = new List<string>();
            bool truncated = false;

            const string remotesPrefix = "remotes/";

            // Include local branches if explicitly requested or when needed to decide whether to show remotes
            bool getLocal = AppSettings.CommitInfoShowContainedInBranchesLocal ||
                            AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;

            // Include remote branches if requested
            bool getRemote = AppSettings.CommitInfoShowContainedInBranchesRemote ||
                             AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            bool allowLocal = AppSettings.CommitInfoShowContainedInBranchesLocal;
            bool allowRemote = getRemote;

            foreach (var branch in branches)
            {
                string noPrefixBranch = branch;
                bool branchIsLocal;
                if (getLocal && getRemote)
                {
                    // "git branch -a" prefixes remote branches with "remotes/"
                    // It is possible to create a local branch named "remotes/origin/something"
                    // so this check is not 100% reliable.
                    // This shouldn't be a big problem if we're only displaying information.
                    // This could be solved by listing local and remote branches separately.
                    branchIsLocal = !branch.StartsWith(remotesPrefix);
                    if (!branchIsLocal)
                    {
                        noPrefixBranch = branch.Substring(remotesPrefix.Length);
                    }
                }
                else
                {
                    branchIsLocal = !getRemote;
                }

                if ((branchIsLocal && allowLocal) || (!branchIsLocal && allowRemote))
                {
                    var branchText = showAsLinks
                        ? _linkFactory.CreateBranchLink(noPrefixBranch)
                        : WebUtility.HtmlEncode(noPrefixBranch);

                    if (limit && formattedBranches.Count == MaximumDisplayedLinesIfLimited)
                    {
                        formattedBranches.RemoveRange(MaximumDisplayedRefsIfLimited, MaximumDisplayedLinesIfLimited - MaximumDisplayedRefsIfLimited);
                        truncated = true;
                        break; // from foreach
                    }

                    formattedBranches.Add(branchText);
                }

                if (branchIsLocal && AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal)
                {
                    allowRemote = false;
                }
            }

            return (formattedBranches, truncated);
        }

        private string ToString(IEnumerable<string> formattedRefs, string prefix, string textIfEmpty, string refsType, bool truncated)
        {
            string linksJoined = formattedRefs?.Join(Environment.NewLine);
            if (linksJoined.IsNullOrEmpty())
            {
                return WebUtility.HtmlEncode(textIfEmpty);
            }

            var sb = new StringBuilder()
                .AppendLine(WebUtility.HtmlEncode(prefix))
                .Append(linksJoined);
            if (truncated)
            {
                sb.AppendLine()
                  .AppendLine(_linkFactory.CreateShowAllLink(refsType));
            }

            return sb.ToString();
        }
    }
}