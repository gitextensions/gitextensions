using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using MediaColor = Avalonia.Media.Color;

namespace GitUI.Editor;

internal abstract class GitHighlightingStrategyBase : DocumentColorizingTransformer
{
    protected static MediaColor ColorNormal
        => Avalonia.Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
            ? Colors.White
            : Colors.Black;

    private static MediaColor ColorComment { get; } = Colors.DarkGreen;

    private readonly char _commentChar;

    protected GitHighlightingStrategyBase(string name, IGitModule module)
    {
        Name = name;

        // By default, comments start with '#'.
        //
        // This can be overridden via the "core.commentchar" configuration setting.
        //
        // However, if "core.commentchar" is "auto", then git attempts to choose a
        // character from "#;@!$%^&|:" which is not present in the message.
        // In such cases it does not appear that the character is provided to the
        // editor. The only way to determine the character is to inspect the message,
        // potentially for a regex resembling "with '(.)' will be ignored", though
        // this likely changes with locale.
        //
        // An alternative approach would be to tally counts for the known set of
        // characters for each line[0] and take the character with most.
        // That would work well in practice.

        const string defaultValue = "#";
        string commentCharSetting = module.GetEffectiveSetting("core.commentchar", defaultValue);
        _commentChar = commentCharSetting.Length == 1 ? commentCharSetting[0] : defaultValue[0];
    }

    public string Name { get; }

    protected override void ColorizeLine(DocumentLine line)
    {
        TextDocument document = CurrentContext.Document;
        MarkTokens(document, line);
    }

    protected abstract void MarkTokens(TextDocument document, DocumentLine line);

    protected bool TryHighlightComment(TextDocument document, DocumentLine line)
    {
        if (IsComment(document, line))
        {
            SetStyle(line.Offset, line.EndOffset, ColorComment);
            return true;
        }

        return false;
    }

    protected void SetStyle(int startOffset, int endOffset, MediaColor color, bool bold = false)
    {
        if (endOffset <= startOffset)
        {
            return;
        }

        IBrush brush = new SolidColorBrush(color);
        ChangeLinePart(startOffset, endOffset, element =>
        {
            element.TextRunProperties.SetForegroundBrush(brush);
            if (bold)
            {
                element.TextRunProperties.SetTypeface(new Typeface(
                    element.TextRunProperties.Typeface.FontFamily,
                    element.TextRunProperties.Typeface.Style,
                    FontWeight.Bold));
            }
        });
    }

    #region Line classifiers

    protected bool IsComment(TextDocument document, DocumentLine line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            char c = document.GetCharAt(line.Offset + i);

            if (char.IsWhiteSpace(c))
            {
                continue;
            }

            return c == _commentChar;
        }

        return false;
    }

    protected static bool IsEmptyOrWhiteSpace(TextDocument document, DocumentLine line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            char c = document.GetCharAt(line.Offset + i);

            if (!char.IsWhiteSpace(c))
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}
