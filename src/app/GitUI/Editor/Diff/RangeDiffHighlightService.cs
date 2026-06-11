using System.Text.RegularExpressions;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;

namespace GitUI.Editor.Diff;

/// <summary>
/// Highlight git-range-diff
/// </summary>
public partial class RangeDiffHighlightService : DiffHighlightService
{
    [GeneratedRegex(@"^(\u001b\[.*?m)?\s*(\d+|-):", RegexOptions.ExplicitCapture)]
    private static partial Regex RangeHeaderRegex { get; }

    private static readonly string[] _diffFullPrefixes = ["      ", "    ++", "    + ", "     +", "    --", "    - ", "     -", "    +-", "    -+", "    "];

    public RangeDiffHighlightService(ref string text, DiffViewerLineNumberControl lineNumbersControl)
        : base(ref text, useGitColoring: true)
    {
        _diffLinesInfo = new();
        int bufferLine = 0;
        ReadOnlySpan<char> textAsSpan = text.AsSpan();
        foreach (Range range in textAsSpan.Split(Delimiters.LineFeed))
        {
            ++bufferLine;
            _diffLinesInfo.Add(new DiffLineInfo
            {
                LineNumInDiff = bufferLine,
                LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                RightLineNumber = bufferLine,

                // Note that Git output occasionally corrupts context lines, so parse headers
                LineType = RangeHeaderRegex.IsMatch(textAsSpan[range]) ? DiffLineType.Header : DiffLineType.Context
            });
        }

        lineNumbersControl.DisplayLineNum(_diffLinesInfo, showLeftColumn: false);
    }

    // git-range-diff has an extended subset of git-diff options, base is the same
    public static IGitCommandConfiguration GetGitCommandConfiguration(IGitModule module)
        => DiffHighlightService.GetGitCommandConfiguration(module, useGitColoring: true, "range-diff");

    public override bool IsSearchMatch(DiffViewerLineNumberControl lineNumbersControl, int indexInText)
        => lineNumbersControl.GetLineInfo(indexInText)?.LineType is DiffLineType.Header;

    public override string[] GetFullDiffPrefixes() => _diffFullPrefixes;
}
