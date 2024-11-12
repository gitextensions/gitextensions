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
    private static readonly Color _addedBackColor = AppColor.AnsiTerminalGreenBackNormal.GetThemeColor();
    private static readonly Color _addedForeColor = AppColor.AnsiTerminalGreenForeBold.GetThemeColor();
    private static readonly Color _removedBackColor = AppColor.AnsiTerminalRedBackNormal.GetThemeColor();
    private static readonly Color _removedForeColor = AppColor.AnsiTerminalRedForeBold.GetThemeColor();

    protected readonly bool _useGitColoring;
    protected readonly List<TextMarker> _textMarkers = [];
    protected DiffLinesInfo _diffLinesInfo;

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
            GitVersion supportsBrightColors = new("2.26.0.0");
            if (module.GitVersion >= supportsBrightColors)
            {
                SetIfUnsetInGit(key: "color.diff.oldMoved", value: "black brightmagenta");
                SetIfUnsetInGit(key: "color.diff.newMoved", value: "black brightblue");
                SetIfUnsetInGit(key: "color.diff.oldMovedAlternative", value: "black brightcyan");
                SetIfUnsetInGit(key: "color.diff.newMovedAlternative", value: "black brightyellow");
            }
            else
            {
                SetIfUnsetInGit(key: "color.diff.oldMoved", value: "reverse bold magenta");
                SetIfUnsetInGit(key: "color.diff.newMoved", value: "reverse bold blue");
                SetIfUnsetInGit(key: "color.diff.oldMovedAlternative", value: "reverse bold cyan");
                SetIfUnsetInGit(key: "color.diff.newMovedAlternative", value: "reverse bold yellow");
            }
        }

        // Set dimmed colors, default is gray dimmed/italic
        SetIfUnsetInGit(key: "color.diff.oldMovedDimmed", value: $"magenta dim {reverse}");
        SetIfUnsetInGit(key: "color.diff.newMovedDimmed", value: $"blue dim {reverse}");
        SetIfUnsetInGit(key: "color.diff.oldMovedAlternativeDimmed", value: $"cyan dim {reverse}");
        SetIfUnsetInGit(key: "color.diff.newMovedAlternativeDimmed", value: $"yellow dim {reverse}");

        // range-diff
        if (command == "range-diff")
        {
            // No override for contextBold, contextDimmed
            SetIfUnsetInGit(key: "color.diff.oldBold", value: $"brightred {reverse}");
            SetIfUnsetInGit(key: "color.diff.newBold", value: $"brightgreen {reverse}");
            SetIfUnsetInGit(key: "color.diff.oldDimmed", value: $"red dim {reverse}");
            SetIfUnsetInGit(key: "color.diff.newDimmed", value: $"green dim {reverse}");
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
        => document.MarkerStrategy.AddMarkers(_textMarkers);

    public override bool IsSearchMatch(DiffViewerLineNumberControl lineNumbersControl, int indexInText)
        => lineNumbersControl.GetLineInfo(indexInText)?.LineType is (DiffLineType.Minus or DiffLineType.Plus or DiffLineType.MinusPlus or DiffLineType.Grep);

    public abstract string[] GetFullDiffPrefixes();

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
            .Select(l => l.Value.LineSegment)
            ?? [];

    /// <summary>
    /// Get next block of diffLines following beginline
    /// </summary>
    /// <param name="diffLines">The parsed diffLines for the document.</param>
    /// <param name="diffLineType">The type of diffLines to find (e.g. added/removed).</param>
    /// <param name="index">The index in diffLines to start with.</param>
    /// <param name="found">If a lineInDiff was found. This is also used to get the added diffLines just after the removed.</param>
    /// <returns>The block of segments.</returns>
    private static List<ISegment> GetBlockOfLines(DiffLineInfo[] diffLines, DiffLineType diffLineType, ref int index, bool found)
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
                if (diffLine?.LineType == DiffLineType.Context && gapLines < maxGapLines)
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

    internal static void AddDifferenceMarkers(List<TextMarker> markers, Func<ISegment, string> getText, ISegment lineRemoved, ISegment lineAdded, int beginOffset, bool dimBackground)
    {
        ReadOnlySpan<char> textRemoved = getText(lineRemoved).AsSpan();
        ReadOnlySpan<char> textAdded = getText(lineAdded).AsSpan();
        int offsetRemoved = lineRemoved.Offset + beginOffset;
        int offsetAdded = lineAdded.Offset + beginOffset;
        (int lengthIdenticalAtStart, int lengthIdenticalAtEnd) = AddDifferenceMarkers(markers, textRemoved, textAdded, offsetRemoved, offsetAdded, dimBackground);

        if (lengthIdenticalAtStart > 0)
        {
            markers.Add(CreateDimmedMarker(offsetRemoved, lengthIdenticalAtStart, isRemoved: true, dimBackground));
            markers.Add(CreateDimmedMarker(offsetAdded, lengthIdenticalAtStart, isRemoved: false, dimBackground));
        }

        if (lengthIdenticalAtEnd > 0)
        {
            markers.Add(CreateDimmedMarker(offsetRemoved + textRemoved.Length - lengthIdenticalAtEnd, lengthIdenticalAtEnd, isRemoved: true, dimBackground));
            markers.Add(CreateDimmedMarker(offsetAdded + textAdded.Length - lengthIdenticalAtEnd, lengthIdenticalAtEnd, isRemoved: false, dimBackground));
        }
    }

    private static (int LengthIdenticalAtStart, int LengthIdenticalAtEnd) AddDifferenceMarkers(
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

        int lengthIdenticalAtStart = 0;
        int lengthIdenticalAtEnd = 0;

        int endRemoved = textRemoved.Length;
        int endAdded = textAdded.Length;
        if (endRemoved == endAdded && textRemoved == textAdded)
        {
            lengthIdenticalAtStart = endRemoved;
            return (lengthIdenticalAtStart, lengthIdenticalAtEnd);
        }

        (string? commonWord, int startIndexIdenticalRemoved, int startIndexIdenticalAdded) = LinesMatcher.FindBestMatch(textRemoved.ToString(), textAdded.ToString());
        if (commonWord is not null)
        {
            int lengthIdentical = commonWord.Length;

            // "LeftPart|CommonWord|RightPart"
            // "LeftPart|CommonWord|identical|Different|identical"
            // "LeftPart|CommonWord+identical" ignored  ^^^^^^^^^ -> lengthIdenticalAtEnd (final value)
            int startIndexRightPartRemoved = startIndexIdenticalRemoved + lengthIdentical;
            int startIndexRightPartAdded = startIndexIdenticalAdded + lengthIdentical;
            (int lengthIdenticalAtStartRightPart, lengthIdenticalAtEnd) = AddDifferenceMarkers(markers,
                textRemoved[startIndexRightPartRemoved..], textAdded[startIndexRightPartAdded..],
                offsetRemoved + startIndexRightPartRemoved, offsetAdded + startIndexRightPartAdded, dimBackground);
            lengthIdentical += lengthIdenticalAtStartRightPart;

            ////                                                             "LeftPart|CommonWord+identical"
            ////                                        "identical|Different|identical|CommonWord+identical"
            //// lengthIdenticalAtStart (final value) <- ^^^^^^^^^  ignored "identical+CommonWord+identical"
            (lengthIdenticalAtStart, int lengthIdenticalAtLeftPartEnd) = AddDifferenceMarkers(markers,
                textRemoved[..startIndexIdenticalRemoved], textAdded[..startIndexIdenticalAdded],
                offsetRemoved, offsetAdded, dimBackground);
            lengthIdentical += lengthIdenticalAtLeftPartEnd;
            startIndexIdenticalRemoved -= lengthIdenticalAtLeftPartEnd;
            startIndexIdenticalAdded -= lengthIdenticalAtLeftPartEnd;

            // join with identical part at start or end or dim the identical part
            if (startIndexIdenticalRemoved == lengthIdenticalAtStart && startIndexIdenticalAdded == lengthIdenticalAtStart)
            {
                lengthIdenticalAtStart += lengthIdentical;
            }
            else if (startIndexIdenticalRemoved + lengthIdentical + lengthIdenticalAtEnd == endRemoved
                && startIndexIdenticalAdded + lengthIdentical + lengthIdenticalAtEnd == endAdded)
            {
                lengthIdenticalAtEnd += lengthIdentical;
            }
            else
            {
                markers.Add(CreateDimmedMarker(offsetRemoved + startIndexIdenticalRemoved, lengthIdentical, isRemoved: true, dimBackground));
                markers.Add(CreateDimmedMarker(offsetAdded + startIndexIdenticalAdded, lengthIdentical, isRemoved: false, dimBackground));
            }
        }
        else
        {
            // find end of identical part at start
            int minEnd = Math.Min(endRemoved, endAdded);
            while (lengthIdenticalAtStart < minEnd
                && textRemoved[lengthIdenticalAtStart] == textAdded[lengthIdenticalAtStart])
            {
                ++lengthIdenticalAtStart;
            }

            // find start of identical part at end
            int startIndexIdenticalAtEndRemoved = endRemoved;
            int startIndexIdenticalAtEndAdded = endAdded;
            while (startIndexIdenticalAtEndRemoved > lengthIdenticalAtStart && startIndexIdenticalAtEndAdded > lengthIdenticalAtStart
                && textRemoved[startIndexIdenticalAtEndRemoved - 1] == textAdded[startIndexIdenticalAtEndAdded - 1])
            {
                --startIndexIdenticalAtEndRemoved;
                --startIndexIdenticalAtEndAdded;
                ++lengthIdenticalAtEnd;
            }

            int lengthDifferentRemoved = startIndexIdenticalAtEndRemoved - lengthIdenticalAtStart;
            int lengthDifferentAdded = startIndexIdenticalAtEndAdded - lengthIdenticalAtStart;
            if (lengthDifferentRemoved == 0 && lengthDifferentAdded > 0)
            {
                markers.Add(CreateAnchorMarker(offsetRemoved + lengthIdenticalAtStart, _addedForeColor));
            }
            else if (lengthDifferentRemoved > 0 && lengthDifferentAdded == 0)
            {
                markers.Add(CreateAnchorMarker(offsetAdded + lengthIdenticalAtStart, _removedForeColor));
            }
        }

        return (lengthIdenticalAtStart, lengthIdenticalAtEnd);
    }

    private static TextMarker CreateAnchorMarker(int offset, Color color)
        => new(offset, length: 0, TextMarkerType.InterChar, color);

    private static TextMarker CreateDimmedMarker(int offset, int length, bool isRemoved, bool dimBackground)
        => dimBackground
            ? CreateTextMarker(offset, length, ColorHelper.DimColor(ColorHelper.DimColor(isRemoved ? _removedBackColor : _addedBackColor)))
            : new(offset, length, TextMarkerType.SolidBlock, SystemColors.Window, ColorHelper.DimColor(isRemoved ? _removedForeColor : _addedForeColor));

    private static TextMarker CreateTextMarker(int offset, int length, Color color)
        => new(offset, length, TextMarkerType.SolidBlock, color, ColorHelper.GetForeColorForBackColor(color));

    internal class TestAccessor
    {
        internal static List<ISegment> GetBlockOfLines(DiffLineInfo[] diffLines, DiffLineType diffLineType, ref int index, bool found)
            => DiffHighlightService.GetBlockOfLines(diffLines, diffLineType, ref index, found);
    }
}
