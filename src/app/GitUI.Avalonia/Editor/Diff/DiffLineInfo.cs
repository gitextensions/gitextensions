using AvaloniaEdit.Document;

namespace GitUI.Editor.Diff;

public class DiffLineInfo
{
    public static readonly int NotApplicableLineNum = -1;

    public int LineNumInDiff { get; set; }

    public int LeftLineNumber { get; set; }

    public int RightLineNumber { get; set; }

    public DiffLineType LineType { get; set; }

    /// <summary>
    /// Gets or sets the document range occupied by an added or removed line.
    /// </summary>
    public ISegment? LineSegment { get; set; }

    /// <summary>
    /// Gets or sets whether Git's ANSI colors identify this as a moved line.
    /// </summary>
    public bool IsMovedLine { get; set; }
}
