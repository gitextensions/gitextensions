using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff
{
    public class LineSegmentGetter
    {
        public virtual ISegment GetSegment(IDocument doc, int lineNumber)
        {
            return doc.GetLineSegment(lineNumber);
        }
    }
}