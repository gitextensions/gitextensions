using System;
using System.Text;
using GitCommands;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal sealed class LaneInfoProvider
    {
        private static readonly TranslationString NoInfoText = new TranslationString("Sorry, this commit seems to be not loaded.");
        private static readonly TranslationString MergedWithText = new TranslationString(" (merged with {0})");
        internal static readonly TranslationString ByPullRequestText = new TranslationString(" by pull request {0}");
        private readonly ILaneNodeLocator _nodeLocator;
        private readonly IGitRevisionSummaryBuilder _gitRevisionSummaryBuilder;

        public LaneInfoProvider(ILaneNodeLocator nodeLocator, IGitRevisionSummaryBuilder gitRevisionSummaryBuilder)
        {
            _nodeLocator = nodeLocator;
            _gitRevisionSummaryBuilder = gitRevisionSummaryBuilder;
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
                if (!string.IsNullOrWhiteSpace(branch.CommittedTo))
                {
                    laneInfoText.AppendFormat("\n{0}: {1}", Strings.Branch, branch.CommittedTo);
                    if (!string.IsNullOrWhiteSpace(branch.MergedWith))
                    {
                        laneInfoText.AppendFormat(MergedWithText.Text, branch.MergedWith);
                    }
                }

                laneInfoText.AppendLine();
            }

            if (node.GitRevision.Body != null)
            {
                laneInfoText.Append(_gitRevisionSummaryBuilder.BuildSummary(node.GitRevision.Body));
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
}
