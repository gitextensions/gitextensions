using System.Text.RegularExpressions;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

/// <summary>
/// Highlight git-range-diff
/// </summary>
public partial class RangeDiffHighlightService : DiffHighlightService
{
    [GeneratedRegex(@"^(\u001b\[.*?m)?\s*(\d+|-):", RegexOptions.ExplicitCapture)]
    private static partial Regex RangeHeaderRegex();

    private static readonly string[] _diffFullPrefixes = ["      ", "    ++", "    + ", "     +", "    --", "    - ", "     -", "    +-", "    -+", "    "];

    public RangeDiffHighlightService(ref string text)
        : base(ref text, useGitColoring: true)
    {
    }

    // git-range-diff has an extended subset of git-diff options, base is the same
    public static IGitCommandConfiguration GetGitCommandConfiguration(IGitModule module)
        => DiffHighlightService.GetGitCommandConfiguration(module, useGitColoring: true, "range-diff");

    public override bool IsSearchMatch(DiffViewerLineNumberControl lineNumbersControl, int indexInText)
        => lineNumbersControl.GetLineInfo(indexInText)?.LineType is DiffLineType.Header;

    public override void SetLineControl(DiffViewerLineNumberControl lineNumbersControl, TextEditorControl textEditor)
    {
        DiffLinesInfo result = new();
        int bufferLine = 0;
        foreach (string line in textEditor.Text.Split(Delimiters.LineFeed))
        {
            ++bufferLine;
            result.Add(new DiffLineInfo
            {
                LineNumInDiff = bufferLine,
                LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                RightLineNumber = bufferLine,

                // Note that Git output occasionally corrupts context lines, so parse headers
                LineType = RangeHeaderRegex().IsMatch(line) ? DiffLineType.Header : DiffLineType.Context
            });
        }

        lineNumbersControl.DisplayLineNum(result, showLeftColumn: false);
    }

    public override string[] GetFullDiffPrefixes() => _diffFullPrefixes;

    public override void AddTextHighlighting(IDocument document)
    {
        foreach (TextMarker tm in _textMarkers)
        {
            document.MarkerStrategy.AddMarker(tm);
        }
    }
}
