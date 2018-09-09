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
        private readonly ILaneNodeLocator _nodeLocator;

        internal readonly struct TestAccessor
        {
            internal static TranslationString NoInfoText => LaneInfoProvider.NoInfoText;
        }

        public LaneInfoProvider(ILaneNodeLocator nodeLocator)
        {
            _nodeLocator = nodeLocator;
        }

        public string GetLaneInfo(int x, int rowIndex, int laneWidth)
        {
            (var node, bool isAtNode) = _nodeLocator.FindPrevNode(x, rowIndex, laneWidth);
            if (node == null)
            {
                return string.Empty;
            }

            if (node.Revision == null)
            {
                return NoInfoText.Text;
            }

            var laneInfoText = new StringBuilder();
            if (!node.Revision.IsArtificial)
            {
                if (isAtNode)
                {
                    laneInfoText.Append("* ");
                }

                laneInfoText.AppendLine(node.Revision.Guid);

                var branch = new BranchFinder(node);
                if (branch.CommittedTo.IsNotNullOrWhitespace())
                {
                    laneInfoText.AppendFormat("\nBranch: {0}", branch.CommittedTo);
                    if (branch.MergedWith.IsNotNullOrWhitespace())
                    {
                        laneInfoText.AppendFormat(" (merged with {0})", branch.MergedWith);
                    }
                }

                laneInfoText.AppendLine();
            }

            if (node.Revision.Body != null)
            {
                laneInfoText.Append(node.Revision.Body.TrimEnd());
            }
            else
            {
                laneInfoText.Append(node.Revision.Subject);
                if (node.Revision.HasMultiLineMessage)
                {
                    laneInfoText.Append("\n\nFull message text is not present in older commits.\nSelect this commit to populate the full message.");
                }
            }

            return laneInfoText.ToString();
        }

        private class BranchFinder
        {
            private static readonly Regex MergeRegex = new Regex("(?i)^merged? (pull request (.*) from )?(.*branch |tag )?'?([^ ']*[^ '.])'?( of [^ ]*[^ .])?( into (.*[^.]))?\\.?$",
                                                                 RegexOptions.Compiled | RegexOptions.CultureInvariant);
            private readonly HashSet<Node> _visitedNodes = new HashSet<Node>();

            internal BranchFinder([NotNull] Node node)
            {
                FindBranchRecursively(node, previousDescJunction: null);
            }

            internal string CommittedTo { get; private set; }
            internal string MergedWith { get; private set; }

            private bool FindBranchRecursively([NotNull] Node node, [CanBeNull] Junction previousDescJunction)
            {
                if (!_visitedNodes.Add(node))
                {
                    return false;
                }

                if (CheckForMerge(node, previousDescJunction) || FindBranch(node))
                {
                    return true;
                }

                foreach (var descJunction in node.Descendants)
                {
                    // iterate the inner nodes (i.e. excluding the youngest) beginning with the oldest
                    bool nodeFound = false;
                    for (int nodeIndex = descJunction.NodeCount - 1; nodeIndex > 0; --nodeIndex)
                    {
                        var innerNode = descJunction[nodeIndex];
                        if (nodeFound)
                        {
                            if (CheckForMerge(innerNode, descJunction) || FindBranch(innerNode))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            nodeFound = innerNode == node;
                        }
                    }

                    // handle the youngest and its descendants
                    if (FindBranchRecursively(descJunction.Youngest, descJunction))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool FindBranch([NotNull] Node node)
            {
                foreach (var gitReference in node.Revision.Refs)
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
            /// <param name="descJunction">
            /// the descending junction the node is part of
            /// (used for the decision whether the node belongs the first or second branch of the merge)
            /// </param>
            private bool CheckForMerge([NotNull] Node node, [CanBeNull] Junction descJunction)
            {
                bool isTheFirstBranch = descJunction == null || node.Ancestors.Count == 0 || node.Ancestors.First() == descJunction;
                string mergedInto;
                string mergedWith;
                (mergedInto, mergedWith) = ParseMergeMessage(node.Revision.Subject, appendPullRequest: isTheFirstBranch);

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
                        with += string.Format(" by pull request {0}", matchPullRequest);
                    }
                }

                return (into, with);
            }
        }
    }
}