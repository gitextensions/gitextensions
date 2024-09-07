using GitExtensions.Extensibility.Git;

namespace GitUI.Editor.Diff;

public class CombinedDiffHighlightService : DiffHighlightService
{
    private static readonly string[] _diffFullPrefixes = ["  ", "++", "+ ", " +", "--", "- ", " -"];

    public CombinedDiffHighlightService(ref string text, bool useGitColoring, DiffViewerLineNumberControl lineNumbersControl)
        : base(ref text, useGitColoring)
    {
        _diffLinesInfo = DiffLineNumAnalyzer.Analyze(text, _textMarkers, isCombinedDiff: true);
        lineNumbersControl.DisplayLineNum(_diffLinesInfo, showLeftColumn: true);
        SetHighlighting(text);
    }

    public static IGitCommandConfiguration GetGitCommandConfiguration(IGitModule module, bool useGitColoring)
        => GetGitCommandConfiguration(module, useGitColoring, "diff-tree");

    public override string[] GetFullDiffPrefixes() => _diffFullPrefixes;
}
