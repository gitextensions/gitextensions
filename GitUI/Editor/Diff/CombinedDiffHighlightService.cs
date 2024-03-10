using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

public class CombinedDiffHighlightService : DiffHighlightService
{
    private static readonly string[] _diffFullPrefixes = ["  ", "++", "+ ", " +", "--", "- ", " -"];
    private static readonly string[] _addedLinePrefixes = ["+", " +"];
    private static readonly string[] _removedLinePrefixes = ["-", " -"];
    private static readonly string[] _diffSearchPrefixes = [.. _addedLinePrefixes, .. _removedLinePrefixes];

    public CombinedDiffHighlightService()
    {
    }

    public override void SetLineControl(DiffViewerLineNumberControl lineNumbersControl, TextEditorControl textEditor)
    {
        DiffLinesInfo result = new DiffLineNumAnalyzer().Analyze(textEditor.Text, isCombinedDiff: true);
        lineNumbersControl.DisplayLineNum(result);
    }

    public override string[] GetFullDiffPrefixes() => _diffFullPrefixes;

    public override bool IsSearchMatch(string line) => line.StartsWithAny(_diffSearchPrefixes);

    protected override List<ISegment> GetAddedLines(IDocument document, ref int line, ref bool found)
        => LinePrefixHelper.GetLinesStartingWith(document, ref line, _addedLinePrefixes, ref found);

    protected override List<ISegment> GetRemovedLines(IDocument document, ref int line, ref bool found)
        => LinePrefixHelper.GetLinesStartingWith(document, ref line, _removedLinePrefixes, ref found);

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
}
