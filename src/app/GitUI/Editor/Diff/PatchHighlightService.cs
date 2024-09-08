using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
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

    public PatchHighlightService(ref string text, bool useGitColoring, DiffViewerLineNumberControl lineNumbersControl)
        : base(ref text, useGitColoring)
    {
        bool isGitWordDiff = _useGitColoring && AppSettings.DiffDisplayAppearance.Value == GitCommands.Settings.DiffDisplayAppearance.GitWordDiff;
        _diffLinesInfo = DiffLineNumAnalyzer.Analyze(text, _textMarkers, isCombinedDiff: false, isGitWordDiff);
        lineNumbersControl.DisplayLineNum(_diffLinesInfo, showLeftColumn: true);
    }

    public static IGitCommandConfiguration GetGitCommandConfiguration(IGitModule module, bool useGitColoring)
        => GetGitCommandConfiguration(module, useGitColoring, "diff");

    public override string[] GetFullDiffPrefixes() => _diffFullPrefixes;

    protected override List<ISegment> GetAddedLines(IDocument document, ref int line, ref bool found)
        => LinePrefixHelper.GetLinesStartingWith(document, _diffLinesInfo, DiffLineType.Plus, ref line, _addedLinePrefix, ref found);

    protected override List<ISegment> GetRemovedLines(IDocument document, ref int line, ref bool found)
        => LinePrefixHelper.GetLinesStartingWith(document, _diffLinesInfo, DiffLineType.Minus, ref line, _removedLinePrefix, ref found);

    protected override int TryHighlightAddedAndDeletedLines(IDocument document, int line, LineSegment lineSegment)
    {
        ProcessLineSegment(document, ref line, lineSegment, _addedLinePrefix, AppColor.AnsiTerminalGreenBackNormal.GetThemeColor());
        ProcessLineSegment(document, ref line, lineSegment, _removedLinePrefix, AppColor.AnsiTerminalRedBackNormal.GetThemeColor());
        return line;
    }
}
