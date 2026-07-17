using AvaloniaEdit.Document;

namespace GitUI.Editor.Diff;

public class RangeDiffHighlightService : DiffHighlightService
{
    public RangeDiffHighlightService(ref string text)
        : base(ref text, useGitColoring: true)
    {
        TextDocument document = new(text);
        foreach (DocumentLine line in document.Lines)
        {
            string lineText = document.GetText(line.Offset, line.Length);
            LinesInfo.Add(new DiffLineInfo
            {
                LineNumInDiff = line.LineNumber,
                LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                RightLineNumber = line.LineNumber,
                LineType = IsRangeHeader(lineText) ? DiffLineType.Header : DiffLineType.Context,
            });
        }
    }

    private static bool IsRangeHeader(string text)
    {
        ReadOnlySpan<char> trimmed = text.AsSpan().TrimStart();
        int colon = trimmed.IndexOf(':');
        if (colon <= 0)
        {
            return false;
        }

        ReadOnlySpan<char> prefix = trimmed[..colon];
        return prefix.SequenceEqual("-") || int.TryParse(prefix, out _);
    }
}
