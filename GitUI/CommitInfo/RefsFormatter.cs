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
        private const int MaximumDisplayedRefs = 20;

        [NotNull]
        private readonly ILinkFactory _linkFactory;

        public RefsFormatter([NotNull] ILinkFactory linkFactory)
        {
            _linkFactory = linkFactory ?? throw new ArgumentNullException("RefsFormatter requires an ILinkFactory instance");
        }

        public string FormatBranches(List<string> branches, bool showAsLinks, bool limit)
            => ToString(branches, l => FormatBranches(l, showAsLinks), Strings.ContainedInBranches, Strings.ContainedInNoBranch, limit);

        public string FormatTags(List<string> tags, bool showAsLinks, bool limit)
            => ToString(tags, l => FormatTags(l, showAsLinks), Strings.ContainedInTags, Strings.ContainedInNoTag, limit);

        private IEnumerable<string> FormatBranches(IEnumerable<string> branches, bool showAsLinks)
        {
            const string remotesPrefix = "remotes/";

            // Include local branches if explicitly requested or when needed to decide whether to show remotes
            bool getLocal = AppSettings.CommitInfoShowContainedInBranchesLocal ||
                            AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;

            // Include remote branches if requested
            bool getRemote = AppSettings.CommitInfoShowContainedInBranchesRemote ||
                             AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            var links = new List<string>();
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

                    links.Add(branchText);
                }

                if (branchIsLocal && AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal)
                {
                    allowRemote = false;
                }
            }

            return links;
        }

        public IEnumerable<string> FormatTags(IEnumerable<string> tags, bool showAsLinks)
        {
            return tags.Select(s => showAsLinks ? _linkFactory.CreateTagLink(s) : WebUtility.HtmlEncode(s));
        }

        private static void Limit(List<string> refs, bool limit = true)
        {
            if (limit && refs.Count > MaximumDisplayedRefs)
            {
                refs[MaximumDisplayedRefs - 2] = "…";
                refs[MaximumDisplayedRefs - 1] = refs[refs.Count - 1];
                refs.RemoveRange(MaximumDisplayedRefs, refs.Count - MaximumDisplayedRefs);
            }
        }

        private static string ToString(List<string> refs, Func<IEnumerable<string>, IEnumerable<string>> formatRefs, string prefix, string textIfEmpty, bool limit)
        {
            Limit(refs, limit);
            var links = formatRefs(refs);
            if (links.Any())
            {
                return WebUtility.HtmlEncode(prefix) + " " + links.Join(", ");
            }

            return WebUtility.HtmlEncode(textIfEmpty);
        }
    }
}