using GitExtensions.Extensibility.Git;
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

    public CombinedDiffHighlightService(ref string text, bool useGitColoring)
        : base(ref text, useGitColoring)
    {
    }

    public override void SetLineControl(DiffViewerLineNumberControl lineNumbersControl, TextEditorControl textEditor)
    {
        DiffLinesInfo result = DiffLineNumAnalyzer.Analyze(textEditor, isCombinedDiff: true);
        lineNumbersControl.DisplayLineNum(result, showLeftColumn: true);
    }

    public static IGitCommandConfiguration GetGitCommandConfiguration(IGitModule module, bool useGitColoring)
        => GetGitCommandConfiguration(module, useGitColoring, "diff-tree");

    public override string[] GetFullDiffPrefixes() => _diffFullPrefixes;

    protected override List<ISegment> GetAddedLines(IDocument document, ref int line, ref bool found)
        => LinePrefixHelper.GetLinesStartingWith(document, ref line, _addedLinePrefixes, ref found);

    protected override List<ISegment> GetRemovedLines(IDocument document, ref int line, ref bool found)
        => LinePrefixHelper.GetLinesStartingWith(document, ref line, _removedLinePrefixes, ref found);

    protected override int TryHighlightAddedAndDeletedLines(IDocument document, int line, LineSegment lineSegment)
    {
        ProcessLineSegment(document, ref line, lineSegment, "++", AppColor.AnsiTerminalGreenBackNormal.GetThemeColor());
        ProcessLineSegment(document, ref line, lineSegment, "+ ", AppColor.AnsiTerminalGreenBackNormal.GetThemeColor());
        ProcessLineSegment(document, ref line, lineSegment, " +", AppColor.AnsiTerminalGreenBackNormal.GetThemeColor());
        ProcessLineSegment(document, ref line, lineSegment, "--", AppColor.AnsiTerminalRedBackNormal.GetThemeColor());
        ProcessLineSegment(document, ref line, lineSegment, "- ", AppColor.AnsiTerminalRedBackNormal.GetThemeColor());
        ProcessLineSegment(document, ref line, lineSegment, " -", AppColor.AnsiTerminalRedBackNormal.GetThemeColor());
        return line;
    }
}
