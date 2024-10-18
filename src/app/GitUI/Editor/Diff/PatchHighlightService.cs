using GitCommands;
using GitExtensions.Extensibility.Git;

namespace GitUI.Editor.Diff;

/// <summary>
/// Highlight for "patches", normal diff files.
/// </summary>
public class PatchHighlightService : DiffHighlightService
{
    // Patterns to check for patches in diff files
    private static readonly string[] _diffFullPrefixes = [" ", "+", "-"];

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
}
