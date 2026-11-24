using AvaloniaEdit.Document;

namespace GitUI.Editor.Diff;

public class LineSegmentGetter
{
    public virtual ISegment GetSegment(TextDocument doc, int lineNumber)
    {
        return doc.GetLineByNumber(lineNumber + 1);
    }
}
