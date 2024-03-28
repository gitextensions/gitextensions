using System.Text.RegularExpressions;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using GitUIPluginInterfaces;
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
    private static readonly string[] _addedLinePrefixes = ["+", " +"];
    private static readonly string[] _removedLinePrefixes = ["-", " -"];

    public RangeDiffHighlightService(ref string text, bool useGitColoring)
        : base(ref text, useGitColoring)
    {
    }

    // git-range-diff has an extended subset of git-diff options, base is the same
    public static GitCommandConfiguration GetGitCommandConfiguration(IGitModule module, bool useGitColoring)
        => GetGitCommandConfiguration(module, useGitColoring, "range-diff");

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

    public override bool IsSearchMatch(DiffViewerLineNumberControl lineNumbersControl, int indexInText)
        => lineNumbersControl.GetLineInfo(indexInText)?.LineType is DiffLineType.Header;

    public override void AddTextHighlighting(IDocument document)
    {
        if (_useGitColoring)
        {
            foreach (TextMarker tm in _textMarkers)
            {
                document.MarkerStrategy.AddMarker(tm);
            }

            return;
        }

        bool forceAbort = false;

        for (int line = 0; line < document.TotalNumberOfLines && !forceAbort; line++)
        {
            LineSegment lineSegment = document.GetLineSegment(line);

            if (lineSegment.TotalLength == 0)
            {
                continue;
            }

            if (line == document.TotalNumberOfLines - 1)
            {
                forceAbort = true;
            }

            line = TryHighlightAddedAndDeletedLines(document, line, lineSegment);

            ProcessLineSegment(document, ref line, lineSegment, "    ", AppColor.AuthoredHighlight.GetThemeColor(), true);
            ProcessLineSegment(document, ref line, lineSegment, "    @@", AppColor.DiffSection.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "     @@", AppColor.DiffSection.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "    -@@", AppColor.DiffSection.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "    +@@", AppColor.DiffSection.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "      ## ", AppColor.DiffSection.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "       ##", AppColor.DiffSection.GetThemeColor());
        }
    }

    public override string[] GetFullDiffPrefixes() => _diffFullPrefixes;

    protected override List<ISegment> GetAddedLines(IDocument document, ref int line, ref bool found)
        => LinePrefixHelper.GetLinesStartingWith(document, ref line, _addedLinePrefixes, ref found);

    protected override List<ISegment> GetRemovedLines(IDocument document, ref int line, ref bool found)
        => LinePrefixHelper.GetLinesStartingWith(document, ref line, _removedLinePrefixes, ref found);

    protected override int TryHighlightAddedAndDeletedLines(IDocument document, int line, LineSegment lineSegment)
    {
        // part of range-diff dual-color

        // Only changed in selected
        ProcessLineSegment(document, ref line, lineSegment, "    ++", AppColor.DiffAddedExtra.GetThemeColor());
        ProcessLineSegment(document, ref line, lineSegment, "    +-", AppColor.DiffRemovedExtra.GetThemeColor());

        // Only changed in first or same change in both
        ProcessLineSegment(document, ref line, lineSegment, "    -+", AppColor.DiffAdded.GetThemeColor());
        ProcessLineSegment(document, ref line, lineSegment, "    --", AppColor.DiffRemoved.GetThemeColor());
        ProcessLineSegment(document, ref line, lineSegment, "     -", AppColor.DiffRemoved.GetThemeColor());
        ProcessLineSegment(document, ref line, lineSegment, "     +", AppColor.DiffAdded.GetThemeColor());

        // No highlight for lines removed in both first/selected
        return line;
    }
}
