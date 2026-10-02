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
using GitExtUtils;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

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
    private int _nativeFormattingInset;
    private bool _usesNativeWidthMeasurement;

    /// <summary>Initializes the XHTML renderer with source-shaped clipboard semantics.</summary>
    public XhtmlTextBlock()
    {
        // SelectableTextBlock's keyboard and context-flyout Copy routes share this event.
        // Its raw selection contains one object-replacement character for each embedded control.
        CopyingToClipboard += XhtmlTextBlock_CopyingToClipboard;
    }

    /// <summary>Gets or sets the source RichEdit contents-width edge allowance for this instance.</summary>
    public double NativeContentOverhang { get; set; } = TextRendererOverhang + RichTextContentOverhang;

    /// <summary>Gets or sets the native formatting origin inside the unchanged control frame.</summary>
    internal int NativeFormattingInset
    {
        get => _nativeFormattingInset;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            if (_nativeFormattingInset == value)
            {
                return;
            }

            _nativeFormattingInset = value;
            InvalidateArrange();
            InvalidateVisual();
        }
    }

    /// <summary>Occurs when an XHTML anchor is activated.</summary>
    public event EventHandler<LinkClickedEventArgs>? LinkClicked;

    /// <summary>Gets the link most recently targeted by the pointer.</summary>
    public string? SelectedLinkUri { get; private set; }

    /// <summary>Gets the decoded plain text represented by the current XHTML.</summary>
    public string GetPlainText() => _plainText;

    /// <summary>Gets the decoded selected text.</summary>
    public string GetSelectionPlainText()
    {
        int start = Math.Max(0, Math.Min(SelectionStart, SelectionEnd));
        int end = Math.Max(SelectionStart, SelectionEnd);
        if (start >= end)
        {
            return string.Empty;
        }

        if (Inlines is null || Inlines.Count == 0)
        {
            string text = Text ?? string.Empty;
            return start >= text.Length ? string.Empty : text[start..Math.Min(end, text.Length)];
        }

        StringBuilder selectedText = new();
        int position = 0;
        foreach (Inline inline in Inlines)
        {
            string sourceText = inline switch
            {
                Run run => run.Text ?? string.Empty,
                LineBreak => Environment.NewLine,
                InlineUIContainer { Child: HyperlinkButton { Content: string caption } } => caption,
                InlineUIContainer { Child: { Tag: string text } } => text,
                _ => throw new InvalidOperationException("The XHTML renderer has an unsupported inline selection."),
            };
            int layoutLength = inline is InlineUIContainer ? 1 : sourceText.Length;
            int overlapStart = Math.Max(start, position);
            int overlapEnd = Math.Min(end, position + layoutLength);
            if (overlapStart < overlapEnd)
            {
                // The caption occupies one selectable layout position. Its complete source
                // text is copied; selecting individual caption characters is a separate gap.
                selectedText.Append(inline is InlineUIContainer
                    ? sourceText
                    : sourceText[(overlapStart - position)..(overlapEnd - position)]);
            }

            position += layoutLength;
            if (position >= end)
            {
                break;
            }
        }

        return selectedText.ToString();
    }

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
            UpdateNativeContentsHeight();
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
        UpdateNativeContentsHeight();
        UpdateTabbedMinimumWidth();
    }

    private void UpdateNativeContentsHeight()
    {
        if (!_usesNativeWidthMeasurement)
        {
            return;
        }

        // The source assigns the whole ContentsResized rectangle to ClientSize. RichEdit
        // includes the final paragraph and uses its current native font's line metrics.
        // Do not constrain the parent to a newline count, which omits that final row.
        double lineHeight = WinFormsRichEditTextMeasurer.GetLineHeight(this);
        int paragraphCount = _plainText.Count(character => character == '\n') + 1;
        LineHeight = lineHeight;
        MinHeight = Height = lineHeight * paragraphCount;
        if (_plainText.Length == 0)
        {
            // Native empty ContentsResized retains only the formatting rectangle's edges.
            MinWidth = Width = NativeContentOverhang;
        }
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

        if (NativeFormattingInset == 0 || TopLevel.GetTopLevel(this) is not { } root)
        {
            base.OnPointerPressed(e);
            return;
        }

        // SelectableTextBlock hit-tests its unshifted TextLayout. Adapt only the arguments
        // passed to that base handler; the real routed event and embedded-link bounds stay intact.
        PointerPressedEventArgs textEvent = new(e.Source, e.Pointer, root,
            GetFormattingPointerPosition(e, root), e.Timestamp, e.Properties, e.KeyModifiers, e.ClickCount)
        {
            Handled = e.Handled,
        };
        base.OnPointerPressed(textEvent);
        e.Handled = textEvent.Handled;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (NativeFormattingInset == 0 || TopLevel.GetTopLevel(this) is not { } root)
        {
            base.OnPointerMoved(e);
            return;
        }

        PointerEventArgs textEvent = new(e.RoutedEvent, e.Source, e.Pointer, root,
            GetFormattingPointerPosition(e, root), e.Timestamp, e.Properties, e.KeyModifiers)
        {
            Handled = e.Handled,
        };
        base.OnPointerMoved(textEvent);
        e.Handled = textEvent.Handled;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (NativeFormattingInset == 0 || TopLevel.GetTopLevel(this) is not { } root)
        {
            base.OnPointerReleased(e);
            return;
        }

        PointerReleasedEventArgs textEvent = new(e.Source, e.Pointer, root,
            GetFormattingPointerPosition(e, root), e.Timestamp, e.Properties, e.KeyModifiers, e.InitialPressMouseButton)
        {
            Handled = e.Handled,
        };
        base.OnPointerReleased(textEvent);
        e.Handled = textEvent.Handled;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Size arrangedSize = base.ArrangeOverride(finalSize);
        if (NativeFormattingInset > 0)
        {
            // TextBlock arranges embedded controls separately from RenderTextLayout.
            // Their actual bounds must use the same formatting origin as painted text.
            foreach (Control child in VisualChildren.OfType<Control>())
            {
                child.Arrange(child.Bounds.Translate(new Vector(NativeFormattingInset, 0)));
            }
        }

        return arrangedSize;
    }

    protected override void RenderTextLayout(DrawingContext context, Point origin)
    {
        // Borderless RichEdit's EM_GETRECT retains an internal formatting inset even with
        // public Padding=0. Keep the background/frame fixed and move text and selection only.
        if (NativeFormattingInset == 0)
        {
            base.RenderTextLayout(context, origin);
            return;
        }

        // Native formatting clips ink to the text rectangle too, including negative glyph
        // bearings. Moving the origin alone exposes that ink in the unchanged client edge.
        using (context.PushClip(new Rect(NativeFormattingInset, 0,
            Math.Max(0, Bounds.Width - (NativeFormattingInset * 2)), Bounds.Height)))
        {
            base.RenderTextLayout(context, origin + new Vector(NativeFormattingInset, 0));
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FontFamilyProperty
            || change.Property == FontSizeProperty
            || change.Property == FontStyleProperty
            || change.Property == FontWeightProperty
            || change.Property == LetterSpacingProperty)
        {
            if (string.IsNullOrEmpty(_xhtml))
            {
                UpdateNativeContentsHeight();
                return;
            }

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

    private Point GetFormattingPointerPosition(PointerEventArgs e, TopLevel root)
        => this.TranslatePoint(e.GetPosition(this) - new Vector(NativeFormattingInset, 0), root)
            ?? e.GetPosition(root);

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

            // RichEdit marks the text CFE_LINK; Fluent's button border is not part of that run.
            BorderThickness = new Thickness(0),
            MinWidth = 0,
            MinHeight = 0,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            LetterSpacing = LetterSpacing,
            Tag = uri,
        };
        ToolTip.SetTip(link, uri);
        link.Click += Link_Click;

        // Button handles the press before ordinary instance handlers; retain the link target
        // for both activation and the source's right-click Copy link context-menu route.
        link.AddHandler(PointerPressedEvent, Link_PointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
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

    private void XhtmlTextBlock_CopyingToClipboard(object? sender, RoutedEventArgs e)
    {
        ClipboardUtil.TrySetText(GetSelectionPlainText());
        e.Handled = true;
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
