using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

/// <summary>
/// Highlight for "patches", normal diff files.
/// </summary>
public class PatchHighlightService : DiffHighlightService
{
    // Patterns to check for patches in diff files
    private const string _addedLinePrefix = "+";
    private const string _removedLinePrefix = "-";
    private static readonly string[] _diffFullPrefixes = [" ", _addedLinePrefix, _removedLinePrefix];
    private static readonly string[] _diffSearchPrefixes = [_addedLinePrefix, _removedLinePrefix];

    public PatchHighlightService()
    {
    }

    public override void SetLineControl(DiffViewerLineNumberControl lineNumbersControl, TextEditorControl textEditor)
    {
        // Note: This is the fourth time the text is parsed...
        DiffLinesInfo result = new DiffLineNumAnalyzer().Analyze(textEditor.Text, isCombinedDiff: false);
        lineNumbersControl.DisplayLineNum(result);
    }

    public override string[] GetFullDiffPrefixes() => _diffFullPrefixes;

    public override bool IsSearchMatch(string line) => line.StartsWithAny(_diffSearchPrefixes);

    protected override List<ISegment> GetAddedLines(IDocument document, ref int line, ref bool found)
        => LinePrefixHelper.GetLinesStartingWith(document, ref line, _addedLinePrefix, ref found);

    protected override List<ISegment> GetRemovedLines(IDocument document, ref int line, ref bool found)
        => LinePrefixHelper.GetLinesStartingWith(document, ref line, _removedLinePrefix, ref found);

    protected override int TryHighlightAddedAndDeletedLines(IDocument document, int line, LineSegment lineSegment)
    {
        ProcessLineSegment(document, ref line, lineSegment, _addedLinePrefix, AppColor.DiffAdded.GetThemeColor());
        ProcessLineSegment(document, ref line, lineSegment, _removedLinePrefix, AppColor.DiffRemoved.GetThemeColor());
        return line;
    }
}
