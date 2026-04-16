using GitExtUtils.GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Theming;

internal static class HighlightingExtension
{
    public static HighlightColor Transform(this HighlightColor original)
    {
        Color originalBackColor = original.BackgroundColor;
        bool hasNoBackground = originalBackColor.IsEmpty || originalBackColor.A == 0;
        Color backColor = !original.Adaptable || originalBackColor.IsSystemColor || hasNoBackground
            ? originalBackColor
            : ColorHelper.AdaptBackColor(originalBackColor);

        Color effectiveBackColor = hasNoBackground
            ? AppColor.EditorBackground.GetThemeColor()
            : originalBackColor;
        Color foreColor = !original.Adaptable || original.Color.IsSystemColor
            ? original.Color
            : original.Color.AdaptForeColor(hasNoBackground ? effectiveBackColor : backColor);

        return new HighlightColor(original, foreColor, backColor);
    }
}
