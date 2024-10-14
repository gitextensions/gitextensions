using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

public struct Segment : ISegment
{
    public int Offset { get; set; }
    public int Length { get; set; }
}
