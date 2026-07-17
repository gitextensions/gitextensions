namespace GitUI.Editor.Diff;

public class PatchHighlightService : DiffHighlightService
{
    public PatchHighlightService(ref string text, bool useGitColoring, bool isGitWordDiff = false)
        : base(ref text, useGitColoring)
    {
        LinesInfo = DiffLineNumAnalyzer.Analyze(text, TextMarkers, isCombinedDiff: false, isGitWordDiff);
        SetHighlighting(text);
    }
}
