using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

internal static class LinesMatcher
{
    internal static IEnumerable<(ISegment RemovedLine, ISegment AddedLine)> FindLinePairs(
        IReadOnlyList<ISegment> removedLines, IReadOnlyList<ISegment> addedLines)
    {
        int minCount = Math.Min(removedLines.Count, addedLines.Count);
        for (int i = 0; i < minCount; ++i)
        {
            yield return (removedLines[i], addedLines[i]);
        }
    }
}
