using System.Drawing;
using GitExtUtils.GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Theming
{
    internal static class HighlightingExtension
    {
        public static HighlightColor Transform(this HighlightColor original)
        {
            var backReplacement = Adapt(original.BackgroundColor, isForeground: false);
            var replacement = Adapt(original.Color, isForeground: true);
            return new HighlightColor(original, replacement, backReplacement);

            Color Adapt(Color c, bool isForeground) =>
                c.IsSystemColor
                    ? c
                    : ColorHelper.AdaptColor(c, isForeground);
        }
    }
}
