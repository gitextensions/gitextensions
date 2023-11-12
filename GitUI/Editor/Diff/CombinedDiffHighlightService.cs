using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff
{
    public class CombinedDiffHighlightService : DiffHighlightService
    {
        private static readonly string[] _diffFullPrefixes = { "  ", "++", "+ ", " +", "--", "- ", " -" };
        private static readonly string[] _diffSearchPrefixes = { "+", "-", " +", " -" };

        public static new CombinedDiffHighlightService Instance { get; } = new();

        protected CombinedDiffHighlightService()
        {
        }

        protected override int GetDiffContentOffset()
        {
            return 2;
        }

        public override string[] GetFullDiffPrefixes()
        {
            return _diffFullPrefixes;
        }

        public override bool IsSearchMatch(string line)
        {
            return line.StartsWithAny(_diffSearchPrefixes);
        }

        protected override int TryHighlightAddedAndDeletedLines(IDocument document, int line, LineSegment lineSegment)
        {
            ProcessLineSegment(document, ref line, lineSegment, "++", AppColor.DiffAdded.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "+ ", AppColor.DiffAdded.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, " +", AppColor.DiffAdded.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "--", AppColor.DiffRemoved.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "- ", AppColor.DiffRemoved.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, " -", AppColor.DiffRemoved.GetThemeColor());
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
