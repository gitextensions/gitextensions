using System.Text.RegularExpressions;
using GitExtensions.Extensibility.Git;
using Microsoft;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal partial class BranchFinder
    {
        [GeneratedRegex(@"^merged? (pull request (?<pr>.*) from )?(.*branch |tag )?'?(?<with>[^ ']*[^ '.])'?( of [^ ]*[^ .])?( into (?<into>.*[^.]))?\.?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex MergeRegex();

        internal BranchFinder(RevisionGraphRevision node)
        {
            RevisionGraphRevision? parent = null;
            while (!CheckForMerge(node, parent) && !FindBranch(node) && node.Children.Any())
            {
                // try the first child and its children
                parent = node;
                node = node.Children.Last(); // note: Children are stored in reverse order
            }
        }

        internal string? CommittedTo { get; private set; }
        internal string? MergedWith { get; private set; }

        private bool FindBranch(RevisionGraphRevision node)
        {
            Validates.NotNull(node.GitRevision);

            foreach (IGitRef gitReference in node.GitRevision.Refs)
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
        /// <param name="node">the node of the revision to evaluate.</param>
        /// <param name="parent">
        /// the node's parent in the branch which is currently descended
        /// (used for the decision whether the node belongs to the first or second branch of the merge).
        /// </param>
        private bool CheckForMerge(RevisionGraphRevision node, RevisionGraphRevision? parent)
        {
            Validates.NotNull(node.GitRevision);

            bool isTheFirstBranch = parent is null || node.Parents.FirstOrDefault() == parent;
            string? mergedInto;
            string? mergedWith;
            (mergedInto, mergedWith) = ParseMergeMessage(node.GitRevision.Subject, appendPullRequest: isTheFirstBranch);

            if (mergedInto is not null)
            {
                CommittedTo = isTheFirstBranch ? mergedInto : mergedWith;
            }

            MergedWith ??= mergedWith ?? string.Empty;

            return CommittedTo is not null;
        }

        private static (string? into, string? with) ParseMergeMessage(string commitSubject, bool appendPullRequest)
        {
            string? into = null;
            string? with = null;
            Match match = MergeRegex().Match(commitSubject);
            if (match.Success)
            {
                Group matchPullRequest = match.Groups["pr"];
                Group matchWith = match.Groups["with"];
                Group matchInto = match.Groups["into"];
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
