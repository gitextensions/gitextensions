namespace GitUI.Editor.Diff;

public class CombinedDiffHighlightService : DiffHighlightService
{
    public CombinedDiffHighlightService(ref string text, bool useGitColoring)
        : base(ref text, useGitColoring)
    {
        LinesInfo = DiffLineNumAnalyzer.Analyze(text, TextMarkers, isCombinedDiff: true);
        SetHighlighting(text);
    }
}
