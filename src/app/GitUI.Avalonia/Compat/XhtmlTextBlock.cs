using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

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
    private const double RichTextContentOverhang = 12;
    private const double TextRendererOverhang = 7;
    private static readonly Regex _tokenRegex = TokenRegex();
    private string _xhtml = string.Empty;
    private string _plainText = string.Empty;
    private IReadOnlyList<double> _tabStops = [];
    private IReadOnlyList<double> _widthTabStops = [];
    private int _defaultTabInterval;
    private bool _usesNativeWidthMeasurement;

    /// <summary>Gets or sets the source RichEdit contents-width edge allowance for this instance.</summary>
    public double NativeContentOverhang { get; set; } = TextRendererOverhang + RichTextContentOverhang;

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

    /// <summary>Applies explicit tab stops and the native default interval used after the last stop.</summary>
    public void SetTabStops(IEnumerable<int> tabStops, IEnumerable<int>? widthTabStops = null, int defaultTabInterval = 0)
    {
        _tabStops = [.. tabStops.Select(value => (double)value)];
        _defaultTabInterval = defaultTabInterval;
        _usesNativeWidthMeasurement = widthTabStops is not null;
        if (!_usesNativeWidthMeasurement)
        {
            Width = double.NaN;
        }

        _widthTabStops = widthTabStops is null
            ? _tabStops
            : [.. widthTabStops.Select(value => (double)value)];
        SetXHTMLText(_xhtml);
    }

    /// <summary>Renders the supported XHTML subset.</summary>
    public void SetXHTMLText(string? xhtml)
    {
        _xhtml = xhtml ?? string.Empty;
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
        LineLayout lineLayout = default;
        foreach (Match match in _tokenRegex.Matches(xhtml))
        {
            if (match.Groups["break"].Success)
            {
                AddLineBreak();
                lineLayout = default;
                plainText.AppendLine();
                continue;
            }

            if (match.Groups["anchor"].Success)
            {
                string caption = DecodeAndStripMarkup(match.Groups["anchorText"].Value);
                string uri = WebUtility.HtmlDecode(match.Groups["href"].Value);
                AddLink(caption, uri, ref lineLayout);
                plainText.Append(caption);
                continue;
            }

            if (match.Groups["underline"].Success)
            {
                string underlinedText = DecodeAndStripMarkup(match.Groups["underline"].Value);
                AddText(underlinedText, plainText, ref lineLayout, Avalonia.Media.TextDecorations.Underline);
                continue;
            }

            string text = WebUtility.HtmlDecode(match.Groups["text"].Value);
            AddText(text, plainText, ref lineLayout);
        }

        _plainText = plainText.ToString();
        UpdateTabbedMinimumWidth();
    }

    private void UpdateTabbedMinimumWidth()
    {
        if ((_widthTabStops.Count == 0 && _defaultTabInterval == 0) || string.IsNullOrEmpty(_plainText))
        {
            return;
        }

        double maximumLineWidth = 0;
        foreach (string line in _plainText.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            double lineWidth = 0;
            int tabIndex = 0;
            string[] parts = line.Split('\t');
            for (int index = 0; index < parts.Length; index++)
            {
                if (index > 0)
                {
                    lineWidth = GetNextTabStop(_widthTabStops, ref tabIndex, lineWidth,
                        WinFormsTextMeasurer.Measure(this, "    "));
                }

                lineWidth += WinFormsTextMeasurer.Measure(this, parts[index]);
            }

            maximumLineWidth = Math.Max(maximumLineWidth, lineWidth);
        }

        // The native formatting rectangle contributes its own edge allowance; it is not
        // TextRenderer glyph padding or an offset applied to the measured tab stops.
        MinWidth = Math.Ceiling(maximumLineWidth + NativeContentOverhang);
        if (_usesNativeWidthMeasurement)
        {
            // RichEdit sizes its contents from native glyph metrics; Avalonia's inline
            // spacers and text rasterizer otherwise determine a different preferred width.
            Width = MinWidth;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            SelectedLinkUri = null;
        }

        base.OnPointerPressed(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (!string.IsNullOrEmpty(_xhtml)
            && (change.Property == FontFamilyProperty
                || change.Property == FontSizeProperty
                || change.Property == FontStyleProperty
                || change.Property == FontWeightProperty
                || change.Property == LetterSpacingProperty))
        {
            // RichEdit invalidates its contents rectangle when font metrics change.
            // Avalonia inheritance can settle only after the seeded XHTML is attached.
            int selectionStart = SelectionStart;
            int selectionEnd = SelectionEnd;
            string? selectedLinkUri = SelectedLinkUri;
            SetXHTMLText(_xhtml);
            SelectionStart = selectionStart;
            SelectionEnd = selectionEnd;
            SelectedLinkUri = selectedLinkUri;
        }
    }

    private static string DecodeAndStripMarkup(string value)
        => WebUtility.HtmlDecode(Regex.Replace(value, "<[^>]+>", string.Empty));

    private void AddText(string text, StringBuilder plainText, ref LineLayout lineLayout, TextDecorationCollection? decorations = null)
    {
        string normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        string[] lines = normalized.Split('\n');
        for (int index = 0; index < lines.Length; index++)
        {
            if (index > 0)
            {
                AddLineBreak();
                lineLayout = default;
                plainText.AppendLine();
            }

            string[] tabParts = _tabStops.Count > 0 || _defaultTabInterval > 0 ? lines[index].Split('\t') : [lines[index]];
            for (int partIndex = 0; partIndex < tabParts.Length; partIndex++)
            {
                if (partIndex > 0)
                {
                    AddTab(ref lineLayout);
                    plainText.Append('\t');
                }

                if (tabParts[partIndex].Length > 0)
                {
                    Inlines?.Add(new Run(tabParts[partIndex]) { TextDecorations = decorations });
                    plainText.Append(tabParts[partIndex]);
                    lineLayout.Advance += MeasureInline(tabParts[partIndex]);
                }
            }
        }
    }

    private void AddTab(ref LineLayout lineLayout)
    {
        double nextStop = GetNextTabStop(_tabStops, ref lineLayout.TabIndex, lineLayout.Advance, MeasureInline("    "));
        double spacerWidth = Math.Max(0, nextStop - lineLayout.Advance);
        Inlines?.Add(new InlineUIContainer(new Border
        {
            Width = spacerWidth,
            Height = 0,
            IsHitTestVisible = false,
            Tag = "\t",
        }));
        lineLayout.Advance += spacerWidth;
    }

    private double GetNextTabStop(IReadOnlyList<double> stops, ref int tabIndex, double advance, double fallbackWidth)
    {
        while (tabIndex < stops.Count && stops[tabIndex] <= advance)
        {
            tabIndex++;
        }

        if (tabIndex < stops.Count)
        {
            return stops[tabIndex++];
        }

        return _defaultTabInterval > 0
            ? (Math.Floor(advance / _defaultTabInterval) + 1) * _defaultTabInterval
            : advance + fallbackWidth;
    }

    private double MeasureInline(string text)
    {
        TextLayout textLayout = new(
            text,
            new Typeface(FontFamily, FontStyle, FontWeight),
            FontSize,
            foreground: null,
            letterSpacing: LetterSpacing);
        return textLayout.WidthIncludingTrailingWhitespace;
    }

    private void AddLineBreak()
        => Inlines?.Add(new LineBreak());

    private void AddLink(string caption, string uri, ref LineLayout lineLayout)
    {
        HyperlinkButton link = new()
        {
            Content = caption,
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            MinWidth = 0,
            MinHeight = 0,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            LetterSpacing = LetterSpacing,
            Tag = uri,
        };
        ToolTip.SetTip(link, uri);
        link.Click += Link_Click;
        link.PointerPressed += Link_PointerPressed;
        Inlines?.Add(new InlineUIContainer(link));
        lineLayout.Advance += MeasureInline(caption);
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

    private struct LineLayout
    {
        public double Advance;

        public int TabIndex;
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
