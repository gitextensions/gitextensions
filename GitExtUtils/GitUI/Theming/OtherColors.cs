using System.Drawing;

namespace GitExtUtils.GitUI.Theming
{
    public static class OtherColors
    {
        public static readonly Color AmendButtonForcedColor = Color.FromArgb(230, 99, 99).AdaptBackColor();
        public static readonly Color BackgroundColor = Color.FromArgb(251, 251, 251).AdaptBackColor();
        public static readonly Color InactiveSelectionHighlightColor = Color.FromArgb(230, 230, 230).AdaptBackColor();
        public static readonly Color MergeConflictsColor = Color.FromArgb(230, 99, 99).AdaptBackColor();
        public static readonly Color PanelBorderColor = Color.FromArgb(224, 224, 224).AdaptBackColor();
        public static readonly Color PanelMessageWarningColor = Color.FromArgb(230, 99, 99).AdaptBackColor();

        public static readonly SolidBrush InactiveSelectionHighlightBrush = new(InactiveSelectionHighlightColor);
    }
}
