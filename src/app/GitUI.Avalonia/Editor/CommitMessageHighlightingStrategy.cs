using AvaloniaEdit.Document;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using DrawingColor = System.Drawing.Color;
using MediaColor = Avalonia.Media.Color;

namespace GitUI.Editor;

internal sealed class CommitMessageHighlightingStrategy : GitHighlightingStrategyBase
{
    // Avalonia passes the original summary color and font weight separately to the text run.
    private static (MediaColor Color, bool Bold) ColorSummary => (ColorNormal, true);

    // AvaloniaEdit draws validation spans from immutable marker snapshots.
    private readonly List<CommitMessageValidationMarker> _overlengthDescriptionMarkers = [];

    private readonly CommitMessageValidationMarker _markerSummaryTooLong = new(0, 0, "Summary line is too long.");
    private readonly CommitMessageValidationMarker _markerSpacerNeeded = new(0, 0, "There must be a blank line after the summary.");

    private readonly List<CommitMessageValidationMarker> _validationMarkers = [];
    private ITextSourceVersion? _validationVersion;

    public CommitMessageHighlightingStrategy(IGitModule module)
        : base("GitCommitMessage", module)
    {
    }

    internal IReadOnlyList<CommitMessageValidationMarker> ValidationMarkers => _validationMarkers;

    internal void UpdateValidationMarkers(TextDocument document)
    {
        ITextSourceVersion version = document.Version;
        ITextSourceVersion? previousVersion = _validationVersion;
        if (ReferenceEquals(previousVersion, version))
        {
            return;
        }

        // AvaloniaEdit reports all changed lines together, so use its version to retain the source marker order.
        int previousSpacerIndex = _validationMarkers.FindIndex(marker => marker.ToolTip == _markerSpacerNeeded.ToolTip);
        int previousSummaryCount = _validationMarkers.Count > 0 && _validationMarkers[0].ToolTip == _markerSummaryTooLong.ToolTip ? 1 : 0;
        int descriptionMarkersBeforeSpacer = previousSpacerIndex < 0
            ? _overlengthDescriptionMarkers.Count
            : previousSpacerIndex - previousSummaryCount;

        _validationMarkers.Clear();
        (int summaryLineNumber, bool seenDividingSpace, int descriptionStartLineNumber) = Classify(document);

        const int maxSummaryLength = 50;
        const int maxDescriptionLength = 80;

        if (summaryLineNumber > 0)
        {
            DocumentLine summary = document.GetLineByNumber(summaryLineNumber);
            if (summary.Length > maxSummaryLength)
            {
                _validationMarkers.Add(_markerSummaryTooLong with
                {
                    Offset = summary.Offset + maxSummaryLength,
                    Length = summary.Length - maxSummaryLength,
                });
            }
        }

        CommitMessageValidationMarker? spacerMarker = null;
        if (descriptionStartLineNumber > 0 && !seenDividingSpace)
        {
            DocumentLine description = document.GetLineByNumber(descriptionStartLineNumber);
            spacerMarker = _markerSpacerNeeded with { Offset = description.Offset, Length = description.Length };
            if (WasEdited(description))
            {
                descriptionMarkersBeforeSpacer = _overlengthDescriptionMarkers.Count;
            }
        }

        if (descriptionStartLineNumber <= 0)
        {
            _overlengthDescriptionMarkers.Clear();
            _validationVersion = version;
            return;
        }

        int descriptionStartIndex = _validationMarkers.Count;
        int markerIndex = 0;

        foreach (DocumentLine line in document.Lines.Skip(descriptionStartLineNumber - 1))
        {
            if (line.Length > maxDescriptionLength)
            {
                int markerOffset = line.Offset + maxDescriptionLength;
                int markerLength = line.Length - maxDescriptionLength;

                CommitMessageValidationMarker overlengthMarker;
                if (markerIndex < _overlengthDescriptionMarkers.Count)
                {
                    overlengthMarker = _overlengthDescriptionMarkers[markerIndex] with { Offset = markerOffset, Length = markerLength };
                    _overlengthDescriptionMarkers[markerIndex] = overlengthMarker;
                }
                else
                {
                    overlengthMarker = new(markerOffset, markerLength, "Line is too long.");
                    _overlengthDescriptionMarkers.Add(overlengthMarker);
                }

                _validationMarkers.Add(overlengthMarker);
                markerIndex++;
            }
        }

        int toRemove = _overlengthDescriptionMarkers.Count - markerIndex;
        if (toRemove > 0)
        {
            _overlengthDescriptionMarkers.RemoveRange(markerIndex, toRemove);
        }

        if (spacerMarker is not null)
        {
            _validationMarkers.Insert(descriptionStartIndex + Math.Min(descriptionMarkersBeforeSpacer, markerIndex), spacerMarker);
        }

        _validationVersion = version;
        return;

        bool WasEdited(DocumentLine line)
        {
            if (previousVersion is null || !version.BelongsToSameDocumentAs(previousVersion))
            {
                return true;
            }

            int start = line.Offset;
            int end = line.EndOffset;
            foreach (TextChangeEventArgs change in version.GetChangesTo(previousVersion))
            {
                if (change.Offset <= end && change.Offset + change.RemovalLength >= start)
                {
                    return true;
                }

                start = change.GetNewOffset(start, AnchorMovementType.BeforeInsertion);
                end = change.GetNewOffset(end, AnchorMovementType.AfterInsertion);
            }

            return false;
        }
    }

    // TODO pending issue is that when text is pasted into the editor, validation markers are not updated until their lines are modified (seems like a bug in the editor)

    protected override void MarkTokens(TextDocument document, DocumentLine line)
    {
        // NOTE the pattern of removing then adding markers might look suboptimal, but tracking their presence isn't reliable as they can be removed without warning
        if (TryHighlightComment(document, line))
        {
            return;
        }

        (int summaryLineNumber, _, _) = Classify(document);
        (MediaColor color, bool bold) = line.LineNumber == summaryLineNumber ? ColorSummary : (ColorNormal, false);
        SetStyle(line.Offset, line.EndOffset, color, bold);
    }

    private (int SummaryLineNumber, bool SeenDividingSpace, int DescriptionStartLineNumber) Classify(TextDocument document)
    {
        int summaryLineNumber = -1;
        bool seenDividingSpace = false;
        int descriptionStartLineNumber = -1;

        foreach (DocumentLine line in document.Lines)
        {
            if (summaryLineNumber == -1)
            {
                if (!IsEmptyOrWhiteSpace(document, line) && !IsComment(document, line))
                {
                    summaryLineNumber = line.LineNumber;
                }
            }
            else if (!seenDividingSpace)
            {
                if (IsEmptyOrWhiteSpace(document, line))
                {
                    seenDividingSpace = true;
                }
                else if (!IsComment(document, line))
                {
                    descriptionStartLineNumber = line.LineNumber;
                    break;
                }
            }
            else if (descriptionStartLineNumber != -1)
            {
                if (!IsEmptyOrWhiteSpace(document, line) && !IsComment(document, line))
                {
                    descriptionStartLineNumber = line.LineNumber;
                    break;
                }
            }
        }

        return (summaryLineNumber, seenDividingSpace, descriptionStartLineNumber);
    }
}

internal sealed record CommitMessageValidationMarker(int Offset, int Length, string ToolTip)
{
    public MediaColor Color { get; } = AvaloniaThemeResources.ToMediaColor(DrawingColor.Red.AdaptBackColor());
}
