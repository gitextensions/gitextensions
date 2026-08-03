namespace GitUI.Editor.Diff;

public struct HighlightInfo
{
    public int DocOffset { get; set; }
    public int Length { get; set; }
    public Color? BackColor { get; set; }
    public Color? ForeColor { get; set; }
}

/// <summary>Describes an ANSI-colored range after escape sequences are removed.</summary>
public sealed class TextMarker
{
    public TextMarker(int offset, int length, Color color, Color? foreColor = null)
    {
        Offset = offset;
        Length = length;
        Color = color;
        ForeColor = foreColor;
    }

    public int Offset { get; set; }
    public int Length { get; set; }
    public int EndOffset => Offset + Length;
    public Color Color { get; }
    public Color? ForeColor { get; }
}
