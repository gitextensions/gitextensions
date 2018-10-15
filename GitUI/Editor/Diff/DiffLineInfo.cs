namespace GitUI.Editor.Diff
{
    public class DiffLineInfo
    {
        public static readonly int NotApplicableLineNum = -1;
        public int LineNumInDiff { get; set; }
        public int LeftLineNumber { get; set; }
        public int RightLineNumber { get; set; }
        public DiffLineType LineType { get; set; }
    }
}