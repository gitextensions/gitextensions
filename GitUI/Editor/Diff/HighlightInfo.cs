namespace GitUI.Editor.Diff;

public struct HighlightInfo
{
    public int DocOffset { get; set; }
    public int Length { get; set; }
    public Color? BackColor { get; set; }
    public Color? ForeColor { get; set; }
}
