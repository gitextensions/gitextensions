using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using GitUIPluginInterfaces;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

/// <summary>
/// Highlight git-range-diff
/// </summary>
public class RangeDiffHighlightService : DiffHighlightService
{
    private static readonly string[] _diffFullPrefixes = ["      ", "    ++", "    + ", "     +", "    --", "    - ", "     -", "    +-", "    -+", "    "];
    private static readonly string[] _diffSearchPrefixes = ["    "];
    private static readonly string[] _addedLinePrefixes = ["+", " +"];
    private static readonly string[] _removedLinePrefixes = ["-", " -"];

    public RangeDiffHighlightService(ref string text, bool useGitColoring)
        : base(ref text, useGitColoring)
    {
    }

    // git-range-diff has an extended subset of git-diff options, base is the same
    public static GitCommandConfiguration GetGitCommandConfiguration(IGitModule module, bool useGitColoring)
        => GetGitCommandConfiguration(module, useGitColoring, "range-diff");

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

    // For range-diff, this is a negative check, search is for headers
    public override bool IsSearchMatch(string line) => !line.StartsWithAny(_diffSearchPrefixes);

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
