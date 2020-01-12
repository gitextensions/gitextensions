using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal class BranchFinder
    {
        private static readonly Regex MergeRegex = new Regex("(?i)^merged? (pull request (.*) from )?(.*branch |tag )?'?([^ ']*[^ '.])'?( of [^ ]*[^ .])?( into (.*[^.]))?\\.?$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        internal BranchFinder([NotNull] RevisionGraphRevision node)
        {
            RevisionGraphRevision parent = null;
            while (!CheckForMerge(node, parent) && !FindBranch(node) && node.Children.Any())
            {
                // try the first child and its children
                parent = node;
                node = node.Children.Last(); // note: Children are stored in reverse order
            }
        }

        internal string CommittedTo { get; private set; }
        internal string MergedWith { get; private set; }

        private bool FindBranch([NotNull] RevisionGraphRevision node)
        {
            foreach (var gitReference in node.GitRevision.Refs)
            {
                if (gitReference.IsHead || gitReference.IsRemote || gitReference.IsStash)
                {
                    CommittedTo = gitReference.Name;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether the commit message is a merge message
        /// and then if its a merge message, sets CommittedTo and MergedWith.
        ///
        /// MergedWith is set if it is the current node, i.e. on the first call.
        /// MergedWith is set to string.Empty if it is no merge.
        /// First/second branch does not matter because it is the message of the current node.
        /// </summary>
        /// <param name="node">the node of the revision to evaluate</param>
        /// <param name="parent">
        /// the node's parent in the branch which is currently descended
        /// (used for the decision whether the node belongs to the first or second branch of the merge)
        /// </param>
        private bool CheckForMerge([NotNull] RevisionGraphRevision node, [CanBeNull] RevisionGraphRevision parent)
        {
            bool isTheFirstBranch = parent == null || node.Parents.IsEmpty || node.Parents.Last() == parent; // note: Parents are stored in reverse order
            string mergedInto;
            string mergedWith;
            (mergedInto, mergedWith) = ParseMergeMessage(node.GitRevision.Subject, appendPullRequest: isTheFirstBranch);

            if (mergedInto != null)
            {
                CommittedTo = isTheFirstBranch ? mergedInto : mergedWith;
            }

            if (MergedWith == null)
            {
                MergedWith = mergedWith ?? string.Empty;
            }

            return CommittedTo != null;
        }

        private static (string into, string with) ParseMergeMessage([NotNull] string commitSubject, bool appendPullRequest)
        {
            string into = null;
            string with = null;
            var match = MergeRegex.Match(commitSubject);
            if (match.Success)
            {
                var matchPullRequest = match.Groups[2];
                var matchWith = match.Groups[4];
                var matchInto = match.Groups[7];
                into = matchInto.Success ? matchInto.Value : "master";
                with = matchWith.Success ? matchWith.Value : "?";
                if (appendPullRequest && matchPullRequest.Success)
                {
                    with += string.Format(LaneInfoProvider.ByPullRequestText.Text, matchPullRequest);
                }
            }

            return (into, with);
        }
    }
}
