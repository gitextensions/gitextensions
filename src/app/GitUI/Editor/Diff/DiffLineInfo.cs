using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

public class DiffLineInfo
{
    public static readonly int NotApplicableLineNum = -1;
    public int LineNumInDiff { get; set; }
    public int LeftLineNumber { get; set; }
    public int RightLineNumber { get; set; }
    public DiffLineType LineType { get; set; }

    /// <summary>
    /// offset and length in document, set for line type Minus/Plus.
    /// </summary>
    public ISegment? LineSegment { get; set; }
}
