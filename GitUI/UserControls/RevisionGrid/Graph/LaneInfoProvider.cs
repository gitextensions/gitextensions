using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal sealed class LaneInfoProvider
    {
        private static readonly TranslationString NoInfoText = new TranslationString("Sorry, this commit seems to be not loaded.");
        private static readonly TranslationString MergedWithText = new TranslationString(" (merged with {0})");
        internal static readonly TranslationString ByPullRequestText = new TranslationString(" by pull request {0}");
        private readonly ILaneNodeLocator _nodeLocator;

        public LaneInfoProvider(ILaneNodeLocator nodeLocator)
        {
            _nodeLocator = nodeLocator;
        }

        public string GetLaneInfo(int rowIndex, int lane)
        {
            (var node, bool isAtNode) = _nodeLocator.FindPrevNode(rowIndex, lane);
            if (node == null)
            {
                return string.Empty;
            }

            if (node.GitRevision == null)
            {
                return NoInfoText.Text;
            }

            var laneInfoText = new StringBuilder();
            if (!node.GitRevision.IsArtificial)
            {
                if (isAtNode)
                {
                    laneInfoText.Append("* ");
                }

                laneInfoText.AppendLine(node.GitRevision.Guid);

                var branch = new BranchFinder(node);
                if (branch.CommittedTo.IsNotNullOrWhitespace())
                {
                    laneInfoText.AppendFormat("\n{0}: {1}", Strings.Branch, branch.CommittedTo);
                    if (branch.MergedWith.IsNotNullOrWhitespace())
                    {
                        laneInfoText.AppendFormat(MergedWithText.Text, branch.MergedWith);
                    }
                }

                laneInfoText.AppendLine();
            }

            if (node.GitRevision.Body != null)
            {
                laneInfoText.Append(node.GitRevision.Body.TrimEnd());
            }
            else
            {
                laneInfoText.Append(node.GitRevision.Subject);
                if (node.GitRevision.HasMultiLineMessage)
                {
                    laneInfoText.Append(Strings.BodyNotLoaded);
                }
            }

            return laneInfoText.ToString();
        }

        internal readonly struct TestAccessor
        {
            internal static TranslationString NoInfoText => LaneInfoProvider.NoInfoText;
            internal static TranslationString MergedWithText => LaneInfoProvider.MergedWithText;
            internal static TranslationString ByPullRequestText => LaneInfoProvider.ByPullRequestText;
        }
    }

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