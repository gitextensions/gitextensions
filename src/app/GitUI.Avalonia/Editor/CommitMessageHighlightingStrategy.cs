using Avalonia.Media;
using AvaloniaEdit.Document;
using GitExtensions.Extensibility.Git;

namespace GitUI.Editor;

internal sealed class CommitMessageHighlightingStrategy : GitHighlightingStrategyBase
{
    private const int MaxSummaryLength = 50;
    private const int MaxDescriptionLength = 80;

    private readonly List<CommitMessageValidationMarker> _validationMarkers = [];

    public CommitMessageHighlightingStrategy(IGitModule module)
        : base("GitCommitMessage", module)
    {
    }

    // TODO pending issue is that when text is pasted into the editor, validation markers are not updated until their lines are modified (seems like a bug in the editor)

    internal IReadOnlyList<CommitMessageValidationMarker> ValidationMarkers => _validationMarkers;

    internal void UpdateValidationMarkers(TextDocument document)
    {
        _validationMarkers.Clear();
        (int summaryLineNumber, bool seenDividingSpace, int descriptionStartLineNumber) = Classify(document);
        if (summaryLineNumber > 0)
        {
            DocumentLine summary = document.GetLineByNumber(summaryLineNumber);
            if (summary.Length > MaxSummaryLength)
            {
                _validationMarkers.Add(new(
                    summary.Offset + MaxSummaryLength,
                    summary.Length - MaxSummaryLength,
                    "Summary line is too long."));
            }
        }

        if (descriptionStartLineNumber > 0 && !seenDividingSpace)
        {
            DocumentLine description = document.GetLineByNumber(descriptionStartLineNumber);
            _validationMarkers.Add(new(description.Offset, description.Length, "There must be a blank line after the summary."));
        }

        if (descriptionStartLineNumber <= 0)
        {
            return;
        }

        foreach (DocumentLine line in document.Lines.Skip(descriptionStartLineNumber - 1))
        {
            if (line.Length > MaxDescriptionLength)
            {
                _validationMarkers.Add(new(
                    line.Offset + MaxDescriptionLength,
                    line.Length - MaxDescriptionLength,
                    "Line is too long."));
            }
        }
    }

    protected override void MarkTokens(TextDocument document, DocumentLine line)
    {
        if (TryHighlightComment(document, line))
        {
            return;
        }

        (int summaryLineNumber, _, _) = Classify(document);
        SetStyle(line.Offset, line.EndOffset, ColorNormal, bold: line.LineNumber == summaryLineNumber);
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
            else if (!IsEmptyOrWhiteSpace(document, line) && !IsComment(document, line))
            {
                descriptionStartLineNumber = line.LineNumber;
                break;
            }
        }

        return (summaryLineNumber, seenDividingSpace, descriptionStartLineNumber);
    }
}

internal sealed record CommitMessageValidationMarker(int Offset, int Length, string ToolTip);
