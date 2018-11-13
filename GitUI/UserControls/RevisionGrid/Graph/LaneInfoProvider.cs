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
        }
    }
}