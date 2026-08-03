using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>
///  Presents the small XHTML subset emitted by Git Extensions commit-data renderers.
/// </summary>
/// <remarks>
///  Avalonia has no RichTextBox XHTML loader. This framework adapter deliberately supports
///  only encoded text, anchors, underline, and line breaks: the complete markup emitted by
///  <c>CommitDataHeaderRenderer</c>, <c>CommitDataBodyRenderer</c>, and <c>RefsFormatter</c>.
/// </remarks>
public sealed partial class XhtmlTextBlock : SelectableTextBlock
{
    private static readonly Regex _tokenRegex = TokenRegex();
    private string _plainText = string.Empty;

    /// <summary>Occurs when an XHTML anchor is activated.</summary>
    public event EventHandler<LinkClickedEventArgs>? LinkClicked;

    /// <summary>Gets the link most recently targeted by the pointer.</summary>
    public string? SelectedLinkUri { get; private set; }

    /// <summary>Gets the decoded plain text represented by the current XHTML.</summary>
    public string GetPlainText() => _plainText;

    /// <summary>Gets the decoded selected text.</summary>
    public string GetSelectionPlainText() => SelectedText ?? string.Empty;

    /// <summary>Clears the rendered content.</summary>
    public void Clear() => SetXHTMLText(string.Empty);

    /// <summary>Renders the supported XHTML subset.</summary>
    public void SetXHTMLText(string? xhtml)
    {
        Inlines?.Clear();
        SelectedLinkUri = null;

        if (string.IsNullOrEmpty(xhtml))
        {
            Text = string.Empty;
            _plainText = string.Empty;
            return;
        }

        Text = null;
        StringBuilder plainText = new();
        foreach (Match match in _tokenRegex.Matches(xhtml))
        {
            if (match.Groups["break"].Success)
            {
                AddLineBreak();
                plainText.AppendLine();
                continue;
            }

            if (match.Groups["anchor"].Success)
            {
                string caption = DecodeAndStripMarkup(match.Groups["anchorText"].Value);
                string uri = WebUtility.HtmlDecode(match.Groups["href"].Value);
                AddLink(caption, uri);
                plainText.Append(caption);
                continue;
            }

            if (match.Groups["underline"].Success)
            {
                string underlinedText = DecodeAndStripMarkup(match.Groups["underline"].Value);
                Inlines?.Add(new Run(underlinedText) { TextDecorations = Avalonia.Media.TextDecorations.Underline });
                plainText.Append(underlinedText);
                continue;
            }

            string text = WebUtility.HtmlDecode(match.Groups["text"].Value);
            AddText(text, plainText);
        }

        _plainText = plainText.ToString();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            SelectedLinkUri = null;
        }

        base.OnPointerPressed(e);
    }

    private static string DecodeAndStripMarkup(string value)
        => WebUtility.HtmlDecode(Regex.Replace(value, "<[^>]+>", string.Empty));

    private void AddText(string text, StringBuilder plainText)
    {
        string normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        string[] lines = normalized.Split('\n');
        for (int index = 0; index < lines.Length; index++)
        {
            if (index > 0)
            {
                AddLineBreak();
                plainText.AppendLine();
            }

            if (lines[index].Length > 0)
            {
                Inlines?.Add(new Run(lines[index]));
                plainText.Append(lines[index]);
            }
        }
    }

    private void AddLineBreak()
        => Inlines?.Add(new LineBreak());

    private void AddLink(string caption, string uri)
    {
        HyperlinkButton link = new()
        {
            Content = caption,
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            MinWidth = 0,
            MinHeight = 0,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Tag = uri,
        };
        ToolTip.SetTip(link, uri);
        link.Click += Link_Click;
        link.PointerPressed += Link_PointerPressed;
        Inlines?.Add(new InlineUIContainer(link));
    }

    private void Link_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is HyperlinkButton { Tag: string uri })
        {
            LinkClicked?.Invoke(this, new LinkClickedEventArgs(uri));
        }
    }

    private void Link_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is HyperlinkButton { Tag: string uri })
        {
            SelectedLinkUri = uri;
        }
    }

    [GeneratedRegex("(?is)(?<anchor><a\\s+href\\s*=\\s*['\"](?<href>.*?)['\"]\\s*>(?<anchorText>.*?)</a>)|<u>(?<underline>.*?)</u>|(?<break><br\\s*/?>)|(?<text>[^<]+)|<[^>]+>", RegexOptions.ExplicitCapture)]
    private static partial Regex TokenRegex();
}

/// <summary>Provides the target of an activated XHTML anchor.</summary>
public sealed class LinkClickedEventArgs(string linkUri) : EventArgs
{
    /// <summary>Gets the decoded absolute link URI.</summary>
    public string LinkUri { get; } = linkUri;
}
