using System.Collections.Generic;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff
{
    public class RangeDiffHighlightService : DiffHighlightService
    {
        public static new RangeDiffHighlightService Instance { get; } = new RangeDiffHighlightService();

        protected RangeDiffHighlightService()
        {
        }

        protected override int GetDiffContentOffset()
        {
            // Four spaces and two space/+/-
            return 6;
        }

        public override void AddPatchHighlighting(IDocument document)
        {
            bool forceAbort = false;

            for (var line = 0; line < document.TotalNumberOfLines && !forceAbort; line++)
            {
                var lineSegment = document.GetLineSegment(line);

                if (lineSegment.TotalLength == 0)
                {
                    continue;
                }

                if (line == document.TotalNumberOfLines - 1)
                {
                    forceAbort = true;
                }

                line = TryHighlightAddedAndDeletedLines(document, line, lineSegment);

                ProcessLineSegment(document, ref line, lineSegment, "    ", AppColor.AuthoredHighlight.GetThemeColor(), true);
                ProcessLineSegment(document, ref line, lineSegment, "    @@", AppColor.DiffSection.GetThemeColor());
                ProcessLineSegment(document, ref line, lineSegment, "     @@", AppColor.DiffSection.GetThemeColor());
                ProcessLineSegment(document, ref line, lineSegment, "    -@@", AppColor.DiffSection.GetThemeColor());
                ProcessLineSegment(document, ref line, lineSegment, "    +@@", AppColor.DiffSection.GetThemeColor());
                ProcessLineSegment(document, ref line, lineSegment, "      ## ", AppColor.DiffSection.GetThemeColor());
                ProcessLineSegment(document, ref line, lineSegment, "       ##", AppColor.DiffSection.GetThemeColor());
            }
        }

        protected override int TryHighlightAddedAndDeletedLines(IDocument document, int line, LineSegment lineSegment)
        {
            // part of range-diff dual-color

            // Only changed in selected
            ProcessLineSegment(document, ref line, lineSegment, "    ++", AppColor.DiffAddedExtra.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "    +-", AppColor.DiffRemovedExtra.GetThemeColor());

            // Only changed in first or same change in both
            ProcessLineSegment(document, ref line, lineSegment, "    -+", AppColor.DiffAdded.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "    --", AppColor.DiffRemoved.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "     -", AppColor.DiffRemoved.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "     +", AppColor.DiffAdded.GetThemeColor());

            // No highlight for lines removed in both first/selected
            return line;
        }

        protected override List<ISegment> GetAddedLines(IDocument document, ref int line, ref bool found)
        {
            return LinePrefixHelper.GetLinesStartingWith(document, ref line, new[] { "+", " +" }, ref found);
        }

        protected override List<ISegment> GetRemovedLines(IDocument document, ref int line, ref bool found)
        {
            return LinePrefixHelper.GetLinesStartingWith(document, ref line, new[] { "-", " -" }, ref found);
        }
    }
}
