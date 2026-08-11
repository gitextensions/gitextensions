using System.Text;
using AvaloniaEdit.Document;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace GitUI.Editor.Diff;

/// <summary>
/// Common class for highlighting of diff style files.
/// </summary>
public abstract class DiffHighlightService : TextHighlightService
{
    private static readonly Color _addedBackColor = AppColor.AnsiTerminalGreenBackNormal.GetThemeColor();
    private static readonly Color _addedForeColor = AppColor.AnsiTerminalGreenForeBold.GetThemeColor();
    private static readonly Color _removedBackColor = AppColor.AnsiTerminalRedBackNormal.GetThemeColor();
    private static readonly Color _removedForeColor = AppColor.AnsiTerminalRedForeBold.GetThemeColor();

    protected readonly bool _useGitColoring;
    protected readonly List<TextMarker> _textMarkers = [];
    protected DiffLinesInfo _diffLinesInfo = null!;

    private readonly List<DiffInlineMarker> _inlineMarkers = [];
    private readonly List<DiffTextMarker> _renderTextMarkers = [];

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
            return null!;
        }

        GitCommandConfiguration commandConfiguration = new();
        IReadOnlyList<GitConfigItem> items = GitCommandConfiguration.Default.Get(command);
        foreach (GitConfigItem cfg in items)
        {
            commandConfiguration.Add(cfg, command);
        }

        // https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---color-moved-wsltmodesgt
        // Disable by default, document that this can be enabled.
        SetIfUnsetInGit(key: "diff.colormovedws", value: "no");

        // https://git-scm.com/docs/git-diff#Documentation/git-diff.txt-diffwordRegex
        // Set to "minimal" diff unless configured.
        SetIfUnsetInGit(key: "diff.wordregex", value: "\"[a-z0-9_]+|.\"");

        // dimmed-zebra highlights borders better than the default "zebra"
        SetIfUnsetInGit(key: "diff.colormoved", value: "dimmed-zebra");

        // Use reverse color to follow GE theme
        string reverse = AppSettings.ReverseGitColoring.Value ? "reverse" : "";

        SetIfUnsetInGit(key: "color.diff.old", value: $"red {reverse}");
        SetIfUnsetInGit(key: "color.diff.new", value: $"green {reverse}");

        if (AppSettings.ReverseGitColoring.Value)
        {
            // Fix: Force black foreground to avoid that foreground is calculated to white
            GitVersion supportsBrightColors = new("2.26.0.0");
            if (module.GitVersion >= supportsBrightColors)
            {
                SetIfUnsetInGit(key: "color.diff.oldmoved", value: "black brightmagenta");
                SetIfUnsetInGit(key: "color.diff.newmoved", value: "black brightblue");
                SetIfUnsetInGit(key: "color.diff.oldmovedalternative", value: "black brightcyan");
                SetIfUnsetInGit(key: "color.diff.newmovedalternative", value: "black brightyellow");
            }
            else
            {
                SetIfUnsetInGit(key: "color.diff.oldmoved", value: "reverse bold magenta");
                SetIfUnsetInGit(key: "color.diff.newmoved", value: "reverse bold blue");
                SetIfUnsetInGit(key: "color.diff.oldmovedalternative", value: "reverse bold cyan");
                SetIfUnsetInGit(key: "color.diff.newmovedalternative", value: "reverse bold yellow");
            }
        }

        // Set dimmed colors, default is gray dimmed/italic
        SetIfUnsetInGit(key: "color.diff.oldmoveddimmed", value: $"magenta dim {reverse}");
        SetIfUnsetInGit(key: "color.diff.newmoveddimmed", value: $"blue dim {reverse}");
        SetIfUnsetInGit(key: "color.diff.oldmovedalternativedimmed", value: $"cyan dim {reverse}");
        SetIfUnsetInGit(key: "color.diff.newmovedalternativedimmed", value: $"yellow dim {reverse}");

        // range-diff
        if (command == "range-diff")
        {
            // No override for contextBold, contextDimmed
            SetIfUnsetInGit(key: "color.diff.oldbold", value: $"brightred {reverse}");
            SetIfUnsetInGit(key: "color.diff.newbold", value: $"brightgreen {reverse}");
            SetIfUnsetInGit(key: "color.diff.olddimmed", value: $"red dim {reverse}");
            SetIfUnsetInGit(key: "color.diff.newdimmed", value: $"green dim {reverse}");
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

    public DiffLinesInfo LinesInfo
    {
        get => _diffLinesInfo;
        protected set => _diffLinesInfo = value;
    }

    internal IReadOnlyList<DiffInlineMarker> InlineMarkers => _inlineMarkers;

    internal IReadOnlyList<DiffTextMarker> TextMarkers => _renderTextMarkers;

    internal bool UseGitColoring => _useGitColoring;

    internal bool UseBackgroundColoring
        => !_useGitColoring || AppSettings.ReverseGitColoring.Value;

    protected void AddTextMarkers(IEnumerable<TextMarker> markers)
    {
        foreach (TextMarker marker in markers)
        {
            _renderTextMarkers.Add(new DiffTextMarker(
                marker.Offset,
                marker.Length,
                GetMarkerKind(marker),
                marker.Color,
                marker.ForeColor));
        }
    }

    public override bool IsSearchMatch(DiffViewerLineNumberControl lineNumbersControl, int indexInText)
        => lineNumbersControl.GetLineInfo(indexInText)?.LineType is DiffLineType.Plus
            or DiffLineType.Minus
            or DiffLineType.MinusPlus
            or DiffLineType.MinusLeft
            or DiffLineType.PlusRight;

    public virtual string[] GetFullDiffPrefixes() => [];

    public override void AddTextHighlighting(TextDocument document)
    {
        // Avalonia framework constraint: the native background and text renderers consume the marker model directly.
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
        AddTextMarkers(_textMarkers);
    }

    private static DiffMarkerKind GetMarkerKind(TextMarker marker)
    {
        Color color = marker.ForeColor ?? marker.Color;
        if (color.R > color.G)
        {
            return DiffMarkerKind.Removed;
        }

        return color.G > color.R ? DiffMarkerKind.Added : DiffMarkerKind.MovedAdded;
    }

    /// <summary>
    /// Set highlighting for <paramref name="text"/>.
    /// The parsed added/removed lines in <see cref="_diffLinesInfo"/> is used as well as
    /// the highlighting in <see cref="_textMarkers"/> (if Git highlighting <see cref="_useGitColoring"/>),
    /// is used to mark inline differences (dim unchanged part of lines).
    /// </summary>
    /// <param name="text">The text to process.</param>
    internal void SetHighlighting(string text)
    {
        // Apply GE word highlighting for Patch display (may apply to Difftastic setting, if not available for a repo)
        if (!_useGitColoring || AppSettings.DiffDisplayAppearance.Value != GitCommands.Settings.DiffDisplayAppearance.GitWordDiff)
        {
            List<TextMarker> markers = _useGitColoring ? [] : _textMarkers;
            AddInlineDifferenceMarkers(markers, text);
            if (_useGitColoring)
            {
                // The in-line diffs must be inserted before the diff to override the markings (the original markers are not changed).
                _textMarkers.InsertRange(0, markers);
            }
        }

        if (!_useGitColoring)
        {
            HighlightAddedAndDeletedLines(_textMarkers);
        }
    }

    /// <summary>
    /// Highlight lines that are added, removed and header lines.
    /// This is an alternative configuration to use the Git diff coloring (that has more features).
    /// </summary>
    /// <param name="textMarkers">The markers to append to.</param>
    private void HighlightAddedAndDeletedLines(List<TextMarker> textMarkers)
    {
        foreach (ISegment segment in GetAllLines(DiffLineType.Minus))
        {
            textMarkers.Add(CreateTextMarker(segment.Offset, segment.Length, _removedBackColor));
        }

        foreach (ISegment segment in GetAllLines(DiffLineType.Plus))
        {
            textMarkers.Add(CreateTextMarker(segment.Offset, segment.Length, _addedBackColor));
        }

        foreach (ISegment segment in GetAllLines(DiffLineType.Header))
        {
            textMarkers.Add(CreateTextMarker(segment.Offset, segment.Length, AppColor.DiffSection.GetThemeColor()));
        }
    }

    /// <summary>
    ///  Matches related removed and added lines in a consecutive block of a patch document and marks identical parts dimmed.
    /// </summary>
    private void AddInlineDifferenceMarkers(List<TextMarker> textMarkers, string text)
    {
        int index = 0;
        DiffLineInfo[] diffLines = [.. _diffLinesInfo.DiffLines.Values.OrderBy(l => l.LineNumInDiff)];
        const int diffContentOffset = 1; // in order to skip the prefixes '-' / '+' (this is only for normal patch format)
        bool dimBackground = !_useGitColoring || AppSettings.ReverseGitColoring.Value;

        // Process the next blocks of removed / added diffLines and mark in-line differences
        while (index < diffLines.Length)
        {
            // git-diff presents the removed lines directly followed by the added in a "block"
            IReadOnlyList<ISegment> linesRemoved = GetBlockOfLines(diffLines, DiffLineType.Minus, ref index, found: false);
            if (linesRemoved.Count == 0)
            {
                continue;
            }

            IReadOnlyList<ISegment> linesAdded = GetBlockOfLines(diffLines, DiffLineType.Plus, ref index, found: true);
            if (linesAdded.Count == 0)
            {
                continue;
            }

            foreach ((ISegment lineRemoved, ISegment lineAdded) in LinesMatcher.FindLinePairs(GetText, linesRemoved, linesAdded))
            {
                AddDifferenceMarkers(textMarkers, GetText, lineRemoved, lineAdded, diffContentOffset, dimBackground);
            }
        }

        return;

        string GetText(ISegment line)
            => text[(line.Offset + diffContentOffset)..(line.Offset + line.Length)];
    }

    private IEnumerable<ISegment> GetAllLines(DiffLineType diffLineType)
        => _diffLinesInfo?.DiffLines.Where(i => i.Value.LineType == diffLineType && i.Value.LineSegment is not null)
            .Select(l => l.Value.LineSegment!)
            ?? [];

    internal void AddDifferenceMarkers(List<TextMarker> markers, Func<ISegment, string> getText, ISegment lineRemoved, ISegment lineAdded, int beginOffset, bool dimBackground)
    {
        const int maxLength = 2000;
        string removed = getText(lineRemoved);
        string added = getText(lineAdded);
        ReadOnlySpan<char> removedText = removed.AsSpan(0, Math.Min(removed.Length, maxLength));
        ReadOnlySpan<char> addedText = added.AsSpan(0, Math.Min(added.Length, maxLength));
        int removedOffset = lineRemoved.Offset + beginOffset;
        int addedOffset = lineAdded.Offset + beginOffset;
        (int identicalAtStart, int identicalAtEnd) = AddDifferenceMarkers(markers, removedText, addedText, removedOffset, addedOffset, dimBackground);

        AddPair(identicalAtStart, removedOffset, addedOffset);
        AddPair(
            identicalAtEnd,
            removedOffset + removedText.Length - identicalAtEnd,
            addedOffset + addedText.Length - identicalAtEnd);

        void AddPair(int length, int removedMarkerOffset, int addedMarkerOffset)
        {
            if (length <= 0)
            {
                return;
            }

            _inlineMarkers.Add(new DiffInlineMarker(removedMarkerOffset, length, IsRemoved: true));
            _inlineMarkers.Add(new DiffInlineMarker(addedMarkerOffset, length, IsRemoved: false));
            markers.Add(CreateDimmedMarker(removedMarkerOffset, length, isRemoved: true, dimBackground));
            markers.Add(CreateDimmedMarker(addedMarkerOffset, length, isRemoved: false, dimBackground));
        }
    }

    private (int LengthIdenticalAtStart, int LengthIdenticalAtEnd) AddDifferenceMarkers(
        List<TextMarker> markers, ReadOnlySpan<char> textRemoved, ReadOnlySpan<char> textAdded, int offsetRemoved, int offsetAdded, bool dimBackground)
    {
        // removed:             added:              "d" stands for "deleted" / "i" for "inserted" -> anchor marker in added / removed
        // "d b R a "           " b A a i"          split at "b" (stands for "before")
        // 1.                   1.
        // "d ""b"" R a "       " ""b"" A a i"      split at "a" (stands for "after")
        // 5.     2.            5.    2.
        // "d ""b"" R ""a"" "   " ""b"" A ""a"" i"  join identical
        //        4.      3.          4.      3.
        // "d"" b ""R"" a """   """ b ""A"" a ""i"

        int identicalAtStart = 0;
        int identicalAtEnd = 0;
        if (textRemoved.Length == textAdded.Length && textRemoved.SequenceEqual(textAdded))
        {
            return (textRemoved.Length, 0);
        }

        (string? commonWord, int removedCommonStart, int addedCommonStart) = LinesMatcher.FindBestMatch(
            textRemoved.ToString(),
            textAdded.ToString());
        if (commonWord is not null)
        {
            int identicalLength = commonWord.Length;

            // "LeftPart|CommonWord|RightPart"
            // "LeftPart|CommonWord|identical|Different|identical"
            // "LeftPart|CommonWord+identical" ignored  ^^^^^^^^^ -> lengthIdenticalAtEnd (final value)
            int removedRightStart = removedCommonStart + identicalLength;
            int addedRightStart = addedCommonStart + identicalLength;
            (int rightStartIdentical, identicalAtEnd) = AddDifferenceMarkers(markers,
                textRemoved[removedRightStart..],
                textAdded[addedRightStart..],
                offsetRemoved + removedRightStart,
                offsetAdded + addedRightStart,
                dimBackground);
            identicalLength += rightStartIdentical;

            ////                                                             "LeftPart|CommonWord+identical"
            ////                                        "identical|Different|identical|CommonWord+identical"
            //// lengthIdenticalAtStart (final value) <- ^^^^^^^^^  ignored "identical+CommonWord+identical"
            (identicalAtStart, int leftEndIdentical) = AddDifferenceMarkers(markers,
                textRemoved[..removedCommonStart],
                textAdded[..addedCommonStart],
                offsetRemoved,
                offsetAdded,
                dimBackground);
            identicalLength += leftEndIdentical;
            removedCommonStart -= leftEndIdentical;
            addedCommonStart -= leftEndIdentical;

            // join with identical part at start or end or dim the identical part
            if (removedCommonStart == identicalAtStart && addedCommonStart == identicalAtStart)
            {
                identicalAtStart += identicalLength;
            }
            else if (removedCommonStart + identicalLength + identicalAtEnd == textRemoved.Length
                     && addedCommonStart + identicalLength + identicalAtEnd == textAdded.Length)
            {
                identicalAtEnd += identicalLength;
            }
            else
            {
                markers.Add(CreateDimmedMarker(offsetRemoved + removedCommonStart, identicalLength, isRemoved: true, dimBackground));
                markers.Add(CreateDimmedMarker(offsetAdded + addedCommonStart, identicalLength, isRemoved: false, dimBackground));
                _inlineMarkers.Add(new DiffInlineMarker(offsetRemoved + removedCommonStart, identicalLength, IsRemoved: true));
                _inlineMarkers.Add(new DiffInlineMarker(offsetAdded + addedCommonStart, identicalLength, IsRemoved: false));
            }
        }
        else
        {
            // find end of identical part at start
            int minimumLength = Math.Min(textRemoved.Length, textAdded.Length);
            while (identicalAtStart < minimumLength && textRemoved[identicalAtStart] == textAdded[identicalAtStart])
            {
                ++identicalAtStart;
            }

            // find start of identical part at end
            int removedEnd = textRemoved.Length;
            int addedEnd = textAdded.Length;
            while (removedEnd > identicalAtStart
                   && addedEnd > identicalAtStart
                   && textRemoved[removedEnd - 1] == textAdded[addedEnd - 1])
            {
                --removedEnd;
                --addedEnd;
                ++identicalAtEnd;
            }

            int removedDifferentLength = removedEnd - identicalAtStart;
            int addedDifferentLength = addedEnd - identicalAtStart;
            if (removedDifferentLength == 0 && addedDifferentLength > 0)
            {
                markers.Add(CreateAnchorMarker(offsetRemoved + identicalAtStart, _addedForeColor));
                _inlineMarkers.Add(new DiffInlineMarker(
                    offsetRemoved + identicalAtStart,
                    Length: 0,
                    IsRemoved: false,
                    IsAnchor: true));
            }
            else if (removedDifferentLength > 0 && addedDifferentLength == 0)
            {
                markers.Add(CreateAnchorMarker(offsetAdded + identicalAtStart, _removedForeColor));
                _inlineMarkers.Add(new DiffInlineMarker(
                    offsetAdded + identicalAtStart,
                    Length: 0,
                    IsRemoved: true,
                    IsAnchor: true));
            }
        }

        return (identicalAtStart, identicalAtEnd);
    }

    /// <summary>
    /// Get next block of diffLines following beginline
    /// </summary>
    /// <param name="diffLines">The parsed diffLines for the document.</param>
    /// <param name="diffLineType">The type of diffLines to find (e.g. added/removed).</param>
    /// <param name="index">The index in diffLines to start with.</param>
    /// <param name="found">If a lineInDiff was found. This is also used to get the added diffLines just after the removed.</param>
    /// <returns>The block of segments.</returns>
    private static List<ISegment> GetBlockOfLines(DiffLineInfo[] diffLines,
        DiffLineType diffLineType,
        ref int index,
        bool found)
    {
        List<ISegment> result = [];
        int gapLines = 0;
        for (; index < diffLines.Length; ++index)
        {
            DiffLineInfo diffLine = diffLines[index];
            if (diffLine.LineType != diffLineType)
            {
                if (!found)
                {
                    // Start of block not found yet
                    continue;
                }

                const int maxGapLines = 5;
                if (diffLine.LineType == DiffLineType.Context && gapLines < maxGapLines)
                {
                    // A gap context diffLines, the block can be extended
                    ++gapLines;
                    continue;
                }

                // Block ended, no more to add (next start search here)
                break;
            }

            ArgumentNullException.ThrowIfNull(diffLine.LineSegment);
            gapLines = 0;
            if (diffLine.IsMovedLine)
            {
                // Ignore this line, seem to be moved
                continue;
            }

            // In block, continue to add
            found = true;
            result.Add(diffLine.LineSegment);
        }

        return result;
    }

    private static TextMarker CreateAnchorMarker(int offset, Color color)
        => new(offset, length: 0, color);

    private static TextMarker CreateDimmedMarker(int offset, int length, bool isRemoved, bool dimBackground)
        => dimBackground
            ? CreateTextMarker(offset, length, (isRemoved ? _removedBackColor : _addedBackColor).DimColor().DimColor())
            : new(offset, length, AppColor.EditorBackground.GetThemeColor(), (isRemoved ? _removedForeColor : _addedForeColor).DimColor());

    private static TextMarker CreateTextMarker(int offset, int length, Color backColor)
        => new(offset, length, backColor, backColor.GetTextColor());

    internal static class TestAccessor
    {
        internal static List<ISegment> GetBlockOfLines(DiffLineInfo[] diffLines, DiffLineType diffLineType, ref int index, bool found)
            => DiffHighlightService.GetBlockOfLines(diffLines, diffLineType, ref index, found);
    }
}
