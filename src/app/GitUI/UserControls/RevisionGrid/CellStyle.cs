using System.Collections.Frozen;

namespace GitUI.UserControls.RevisionGrid;

internal readonly struct CellStyle
{
    public readonly Brush BackBrush;
    public readonly Color ForeColor;
    public readonly Color CommitBodyForeColor;
    public readonly Font NormalFont;
    public readonly Font BoldFont;
    public readonly Font MonospaceFont;
    public readonly FrozenDictionary<string, Color> RemoteColors;

    public CellStyle(Brush backBrush, Color foreColor, Color commitBodyForeColor, Font normalFont, Font boldFont, Font monospaceFont, FrozenDictionary<string, Color> remoteColors)
    {
        BackBrush = backBrush;
        ForeColor = foreColor;
        CommitBodyForeColor = commitBodyForeColor;
        NormalFont = normalFont;
        BoldFont = boldFont;
        MonospaceFont = monospaceFont;
        RemoteColors = remoteColors;
    }
}
