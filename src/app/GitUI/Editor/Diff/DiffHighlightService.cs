using System.Text;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

/// <summary>
/// Common class for highlighting of diff style files.
/// </summary>
public abstract class DiffHighlightService : TextHighlightService
{
    protected readonly bool _useGitColoring;
    protected readonly List<TextMarker> _textMarkers = [];

    public DiffHighlightService(ref string text, bool useGitColoring)
    {
        _useGitColoring = useGitColoring;
        SetText(ref text);
    }

    public static IGitCommandConfiguration GetGitCommandConfiguration(IGitModule module, bool useGitColoring, string command)
    {
        if (!useGitColoring)
        {
            // Use default
            return null;
        }

        GitCommandConfiguration commandConfiguration = new();
        IReadOnlyList<GitConfigItem> items = GitCommandConfiguration.Default.Get(command);
        foreach (GitConfigItem cfg in items)
        {
            commandConfiguration.Add(cfg, command);
        }

        // https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---color-moved-wsltmodesgt
        // Disable by default, document that this can be enabled.
        SetIfUnsetInGit(key: "diff.colorMovedWS", value: "no");

        // https://git-scm.com/docs/git-diff#Documentation/git-diff.txt-diffwordRegex
        // Set to "minimal" diff unless configured.
        SetIfUnsetInGit(key: "diff.wordRegex", value: "\"[a-z0-9_]+|.\"");

        // dimmed-zebra highlights borders better than the default "zebra"
        SetIfUnsetInGit(key: "diff.colorMoved", value: "dimmed-zebra");

        // Use reverse color to follow GE theme
        string reverse = AppSettings.ReverseGitColoring.Value ? "reverse" : "";

        SetIfUnsetInGit(key: "color.diff.old", value: $"red {reverse}");
        SetIfUnsetInGit(key: "color.diff.new", value: $"green {reverse}");

        if (AppSettings.ReverseGitColoring.Value)
        {
            // Fix: Force black foreground to avoid that foreground is calculated to white
            SetIfUnsetInGit(key: "color.diff.oldMoved", value: "black brightmagenta");
            SetIfUnsetInGit(key: "color.diff.newMoved", value: "black brightblue");
            SetIfUnsetInGit(key: "color.diff.oldMovedAlternative", value: "black brightcyan");
            SetIfUnsetInGit(key: "color.diff.newMovedAlternative", value: "black brightyellow");
        }

        // Set dimmed colors, default is gray dimmed/italic
        SetIfUnsetInGit(key: "color.diff.oldMovedDimmed", value: $"magenta dim {reverse}");
        SetIfUnsetInGit(key: "color.diff.newMovedDimmed", value: $"blue dim {reverse}");
        SetIfUnsetInGit(key: "color.diff.oldMovedAlternativeDimmed", value: $"cyan dim {reverse}");
        SetIfUnsetInGit(key: "color.diff.newMovedAlternativeDimmed", value: $"yellow dim {reverse}");

        // range-diff
        if (command == "range-diff")
        {
            SetIfUnsetInGit(key: "color.diff.contextBold", value: $"normal bold {reverse}");
            SetIfUnsetInGit(key: "color.diff.oldBold", value: $"brightred {reverse}");
            SetIfUnsetInGit(key: "color.diff.newBold", value: $"brightgreen  {reverse}");
        }

        return commandConfiguration;

        void SetIfUnsetInGit(string key, string value)
        {
            // Note: Only check Windows, not WSL settings
            if (string.IsNullOrEmpty(module.GetEffectiveSetting(key)))
            {
                commandConfiguration.Add(new GitConfigItem(key, value), command);
            }
        }
    }

    public override void AddTextHighlighting(IDocument document)
    {
        if (_useGitColoring)
        {
            // Apply GE word highlighting for Patch display (may apply to Difftastic setting, if not available for a repo)
            if (AppSettings.DiffDisplayAppearance.Value != GitCommands.Settings.DiffDisplayAppearance.GitWordDiff)
            {
                AddExtraPatchHighlighting(document);
            }

            foreach (TextMarker tm in _textMarkers)
            {
                document.MarkerStrategy.AddMarker(tm);
            }

            return;
        }

        bool forceAbort = false;

        AddExtraPatchHighlighting(document);

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

            ProcessLineSegment(document, ref line, lineSegment, "@", AppColor.DiffSection.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "\\", AppColor.DiffSection.GetThemeColor());
        }
    }

    public override bool IsSearchMatch(DiffViewerLineNumberControl lineNumbersControl, int indexInText)
        => lineNumbersControl.GetLineInfo(indexInText)?.LineType is (DiffLineType.Minus or DiffLineType.Plus or DiffLineType.MinusPlus or DiffLineType.Grep);

    public abstract string[] GetFullDiffPrefixes();

    protected readonly LinePrefixHelper LinePrefixHelper = new(new LineSegmentGetter());

    /// <summary>
    /// Parse the text in the document from line and return the added lines directly following.
    /// Overridden in the HighlightServices where GE coloring is used (AddTextHighlighting() for Patch and CombinedDiff).
    /// </summary>
    /// <param name="document">The document to analyze.</param>
    /// <param name="line">The line number to start with, updated with the last line processed.</param>
    /// <param name="found">Ref updated if any added lines were found.</param>
    /// <returns>List with the segments of added lines.</returns>
    protected virtual List<ISegment> GetAddedLines(IDocument document, ref int line, ref bool found)
        => [];

    /// <summary>
    /// Parse the text in the document from line and return the removed lines directly following.
    /// Overridden in the HighlightServices where GE coloring is used (AddTextHighlighting() for Patch and CombinedDiff).
    /// </summary>
    /// <param name="document">The document to analyze.</param>
    /// <param name="line">The line number to start with, updated with the last line processed.</param>
    /// <param name="found">Ref updated if any removed lines were found.</param>
    /// <returns>List with the segments of removed lines.</returns>
    protected virtual List<ISegment> GetRemovedLines(IDocument document, ref int line, ref bool found)
        => [];

    /// <summary>
    /// Highlight the directly following lines.
    /// Overridden in the HighlightServices where GE coloring is used (AddTextHighlighting() for Patch and CombinedDiff).
    /// </summary>
    /// <param name="document">The document to analyze.</param>
    /// <param name="line">The line number to start with.</param>
    /// <param name="lineSegment">The segment for the starting line.</param>
    /// <returns>The last line number processed.</returns>
    protected virtual int TryHighlightAddedAndDeletedLines(IDocument document, int line, LineSegment lineSegment)
        => line;

    protected void ProcessLineSegment(IDocument document, ref int line,
        LineSegment lineSegment, string prefixStr, Color color, bool invertMatch = false)
    {
        if (!DoesLineStartWith(document, lineSegment.Offset, prefixStr, invertMatch))
        {
            return;
        }

        LineSegment endLine = document.GetLineSegment(line);

        for (;
            line < document.TotalNumberOfLines
            && DoesLineStartWith(document, endLine.Offset, prefixStr, invertMatch);
            line++)
        {
            endLine = document.GetLineSegment(line);
        }

        line = Math.Max(0, line - 2);
        endLine = document.GetLineSegment(line);

        document.MarkerStrategy.AddMarker(new TextMarker(lineSegment.Offset,
            (endLine.Offset + endLine.TotalLength) -
            lineSegment.Offset, TextMarkerType.SolidBlock, color,
            ColorHelper.GetForeColorForBackColor(color)));

        return;

        bool DoesLineStartWith(IDocument document, int offset, string prefixStr, bool invertMatch)
            => invertMatch ^ LinePrefixHelper.DoesLineStartWith(document, offset, prefixStr);
    }

    private void SetText(ref string text)
    {
        if (!_useGitColoring)
        {
            return;
        }

        StringBuilder sb = new(text.Length);
        AnsiEscapeUtilities.ParseEscape(text, sb, _textMarkers);

        text = sb.ToString();
    }

    private static void MarkDifference(IDocument document, List<ISegment> linesRemoved, List<ISegment> linesAdded, int beginOffset)
    {
        int count = Math.Min(linesRemoved.Count, linesAdded.Count);

        for (int i = 0; i < count; i++)
        {
            MarkDifference(document, linesRemoved[i], linesAdded[i], beginOffset);
        }
    }

    private static void MarkDifference(IDocument document, ISegment lineRemoved,
        ISegment lineAdded, int beginOffset)
    {
        int lineRemovedEndOffset = lineRemoved.Length;
        int lineAddedEndOffset = lineAdded.Length;
        int endOffsetMin = Math.Min(lineRemovedEndOffset, lineAddedEndOffset);
        int reverseOffset = 0;

        while (beginOffset < endOffsetMin)
        {
            char a = document.GetCharAt(lineAdded.Offset + beginOffset);
            char r = document.GetCharAt(lineRemoved.Offset + beginOffset);

            if (a != r)
            {
                break;
            }

            beginOffset++;
        }

        while (lineAddedEndOffset >= beginOffset && lineRemovedEndOffset >= beginOffset)
        {
            reverseOffset = lineAdded.Length - lineAddedEndOffset;

            int addedOffset = lineAdded.Length - 1 - reverseOffset;
            int removedOffset = lineRemoved.Length - 1 - reverseOffset;

            if (addedOffset < beginOffset || removedOffset < beginOffset)
            {
                break;
            }

            char a = document.GetCharAt(lineAdded.Offset + addedOffset);
            char r = document.GetCharAt(lineRemoved.Offset + removedOffset);

            if (a != r)
            {
                break;
            }

            lineRemovedEndOffset--;
            lineAddedEndOffset--;
        }

        Color color;
        MarkerStrategy markerStrategy = document.MarkerStrategy;

        if (lineAdded.Length - beginOffset - reverseOffset > 0)
        {
            color = AppColor.AnsiTerminalGreenBackBold.GetThemeColor();
            markerStrategy.AddMarker(new TextMarker(lineAdded.Offset + beginOffset,
                                                    lineAdded.Length - beginOffset - reverseOffset,
                                                    TextMarkerType.SolidBlock, color,
                                                    ColorHelper.GetForeColorForBackColor(color)));
        }

        if (lineRemoved.Length - beginOffset - reverseOffset > 0)
        {
            color = AppColor.AnsiTerminalRedBackBold.GetThemeColor();
            markerStrategy.AddMarker(new TextMarker(lineRemoved.Offset + beginOffset,
                                                    lineRemoved.Length - beginOffset - reverseOffset,
                                                    TextMarkerType.SolidBlock, color,
                                                    ColorHelper.GetForeColorForBackColor(color)));
        }
    }

    private void AddExtraPatchHighlighting(IDocument document)
    {
        int line = 0;

        bool found = false;
        int diffContentOffset;
        List<ISegment> linesRemoved = GetRemovedLines(document, ref line, ref found);
        List<ISegment> linesAdded = GetAddedLines(document, ref line, ref found);
        if (linesAdded.Count == 1 && linesRemoved.Count == 1)
        {
            ISegment lineA = linesRemoved[0];
            ISegment lineB = linesAdded[0];
            if (lineA.Length > 4 && lineB.Length > 4 &&
                document.GetCharAt(lineA.Offset + 4) == 'a' &&
                document.GetCharAt(lineB.Offset + 4) == 'b')
            {
                diffContentOffset = 5;
            }
            else
            {
                diffContentOffset = 4;
            }

            MarkDifference(document, linesRemoved, linesAdded, diffContentOffset);
        }

        // overlap when marking
        diffContentOffset = 1;
        while (line < document.TotalNumberOfLines)
        {
            found = false;
            linesRemoved = GetRemovedLines(document, ref line, ref found);
            linesAdded = GetAddedLines(document, ref line, ref found);

            MarkDifference(document, linesRemoved, linesAdded, diffContentOffset);
        }
    }
}
