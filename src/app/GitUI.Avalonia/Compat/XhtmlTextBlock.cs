using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;
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
    private static readonly Cursor _textCursor = new(StandardCursorType.Ibeam);
    private static readonly Cursor _linkCursor = new(StandardCursorType.Hand);
    private static readonly Regex _tokenRegex = TokenRegex();
    private string _xhtml = string.Empty;
    private string _plainText = string.Empty;
    private IReadOnlyList<double> _tabStops = [];
    private IReadOnlyList<double> _widthTabStops = [];
    private int _defaultTabInterval;
    private int _nativeFormattingInset;
    private int _pointerSelectionAnchor = -1;
    private Thickness? _nativeAnchorInsets;
    private int _contentsHeight;
    private int _reportedContentsHeight;
    private int _contentsResizeSuspendCount;
    private bool _usesNativeContentsHeightMeasurement;
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

    /// <summary>Reports the actual wrapped contents height to the source-shaped parent layout.</summary>
    internal event Action<int>? ContentsResized;

    /// <summary>Gets the measured contents height independently of the anchored client rectangle.</summary>
    internal int ContentsHeight => _contentsHeight;

    private bool UsesNativeRichEditText => _usesNativeWidthMeasurement || _usesNativeContentsHeightMeasurement;

    /// <summary>Gets the link most recently targeted by the pointer.</summary>
    public string? SelectedLinkUri { get; private set; }

    /// <summary>Gets the decoded plain text represented by the current XHTML.</summary>
    public string GetPlainText() => _plainText;

    /// <summary>Reads the source link at the nearest character for a fresh context request.</summary>
    internal string? GetContextLinkAtPoint(Point point)
    {
        EnsureNativePointerLayout();
        if (Inlines?.Text is not { Length: > 0 } text)
        {
            return null;
        }

        // RichTextBox.GetCharIndexFromPosition returns the nearest character even
        // outside its ink. Unlike pointer activation, context lookup must not reject
        // blank client space simply because TextLayout reports IsInside == false.
        point -= new Vector(NativeFormattingInset + Padding.Left, Padding.Top);
        TextHitTestResult hit = TextLayout.HitTestPoint(point);
        int position = Math.Clamp(hit.CharacterHit.FirstCharacterIndex, 0, text.Length - 1);
        int start = 0;
        foreach (Inline inline in Inlines)
        {
            int length = inline switch
            {
                Run run => run.Text?.Length ?? 0,
                LineBreak => Environment.NewLine.Length,
                InlineUIContainer => 1,
                _ => 0,
            };
            if (position >= start && position < start + length)
            {
                return inline switch
                {
                    XhtmlLinkRun link => link.LinkUri,
                    InlineUIContainer { Child: HyperlinkButton { Tag: string link } } => link,
                    _ => null,
                };
            }

            start += length;
        }

        return null;
    }

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

    /// <summary>Uses the source RichEdit font height while retaining the wrapped contents' automatic height.</summary>
    internal void UseNativeContentsHeightMeasurement()
    {
        _usesNativeContentsHeightMeasurement = true;
        SetXHTMLText(_xhtml);
    }

    /// <summary>Preserves the source panel's cached edge distances independently of its public Margin.</summary>
    internal void SetNativeAnchorInsets(Thickness insets)
    {
        _nativeAnchorInsets = insets;
        InvalidateMeasure();
    }

    /// <summary>Defers notifications while the parent chooses the actual wrapped viewport width.</summary>
    internal void SuspendContentsResized()
        => _contentsResizeSuspendCount++;

    /// <summary>Publishes only the final actual contents after the parent's width transaction.</summary>
    internal void ResumeContentsResized()
    {
        if (_contentsResizeSuspendCount == 0)
        {
            throw new InvalidOperationException("A contents-resize transaction must be suspended before it is resumed.");
        }

        _contentsResizeSuspendCount--;
        PublishContentsResized();
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
                AppendPlainLineBreak(plainText);
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
        if (!_usesNativeWidthMeasurement && !_usesNativeContentsHeightMeasurement)
        {
            return;
        }

        // The source assigns the whole ContentsResized rectangle to ClientSize. RichEdit
        // includes the final paragraph and uses its current native font's line metrics.
        // Do not constrain the parent to a newline count, which omits that final row.
        double lineHeight = WinFormsRichEditTextMeasurer.GetLineHeight(this);
        LineHeight = lineHeight;
        if (!_usesNativeWidthMeasurement)
        {
            // CommitInfo's body and refs keep WordWrap enabled. TextLayout measures the
            // actual visual lines at the available width, including the final paragraph;
            // forcing the header's paragraph-count Height would clip wrapped contents.
            MinHeight = lineHeight;
            Height = double.NaN;
            return;
        }

        int paragraphCount = _plainText.Count(character => character == '\n') + 1;
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
        if (_usesNativeWidthMeasurement && Inlines is { Count: > 0 })
        {
            double lineWidth = 0;
            int tabIndex = 0;
            foreach (Inline inline in Inlines)
            {
                switch (inline)
                {
                    case LineBreak:
                    case Run { Text: "\n" }:
                        maximumLineWidth = Math.Max(maximumLineWidth, lineWidth);
                        lineWidth = 0;
                        tabIndex = 0;
                        break;
                    case InlineUIContainer { Child: { Tag: "\t" } }:
                        lineWidth = GetNextTabStop(_widthTabStops, ref tabIndex, lineWidth, MeasureNativeInline("    "));
                        break;
                    case InlineUIContainer { Child: HyperlinkButton { Content: string caption, Tag: string uri } }:
                        lineWidth += MeasureNativeInline(caption, rtfRoundTrip: caption != uri);
                        break;
                    case XhtmlLinkRun link:
                        lineWidth += MeasureNativeInline(link.Text ?? string.Empty, rtfRoundTrip: link.Text != link.LinkUri);
                        break;
                    case Run run:
                        lineWidth += MeasureNativeInline(run.Text ?? string.Empty);
                        break;
                }
            }

            maximumLineWidth = Math.Max(maximumLineWidth, lineWidth);
        }
        else
        {
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
        if (UsesNativeRichEditText)
        {
            EnsureNativePointerLayout();
            _pointerSelectionAnchor = -1;
            PointerPointProperties properties = e.GetCurrentPoint(this).Properties;
            if (properties.IsXButton1Pressed || properties.IsXButton2Pressed)
            {
                // The source header's MouseDown dispatches history navigation. The
                // SelectableTextBlock base consumes every button before that instance
                // handler, although these presses do not start a text selection.
                return;
            }

            XhtmlLinkRun? link = GetLinkAtPoint(e.GetPosition(this));
            SelectedLinkUri = link?.LinkUri;
            if (properties.IsRightButtonPressed)
            {
                // The source read-only RichEdit preserves its selection both inside and
                // outside a right-button press. Do not acquire SelectableTextBlock's
                // capture, which would collapse that selection on release. The unhandled
                // release still raises the framework's ordinary context-menu request.
                Focus();
                return;
            }

            if (link is not null && properties.IsLeftButtonPressed)
            {
                // RichTextBox raises EN_LINK on WM_LBUTTONDOWN and consumes that
                // notification. A drag begun in ordinary text still uses text selection.
                Focus();
                LinkClicked?.Invoke(this, new LinkClickedEventArgs(link.LinkUri));
                e.Handled = true;
                return;
            }
        }
        else if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            SelectedLinkUri = null;
        }

        if (NativeFormattingInset == 0 || TopLevel.GetTopLevel(this) is not { } root)
        {
            base.OnPointerPressed(e);
            RememberNativePointerSelection(e);
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
        RememberNativePointerSelection(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (UsesNativeRichEditText)
        {
            EnsureNativePointerLayout();
            XhtmlLinkRun? link = GetLinkAtPoint(e.GetPosition(this));
            SetCurrentValue(CursorProperty, link is null ? _textCursor : _linkCursor);
            ToolTip.SetTip(this, link?.LinkUri);
        }

        // Selection invalidates TextBlock's measured inline runs. Resolve this input's
        // endpoint before the base changes selection and clears that text-layout cache.
        int? nativeSelectionPosition = GetNativePointerSelectionPosition(e);
        if (NativeFormattingInset == 0 || TopLevel.GetTopLevel(this) is not { } root)
        {
            base.OnPointerMoved(e);
            UpdateNativePointerSelection(nativeSelectionPosition);
            return;
        }

        PointerEventArgs textEvent = new(e.RoutedEvent, e.Source, e.Pointer, root,
            GetFormattingPointerPosition(e, root), e.Timestamp, e.Properties, e.KeyModifiers)
        {
            Handled = e.Handled,
        };
        base.OnPointerMoved(textEvent);
        e.Handled = textEvent.Handled;
        UpdateNativePointerSelection(nativeSelectionPosition);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _pointerSelectionAnchor = -1;
        if (UsesNativeRichEditText && e.InitialPressMouseButton == MouseButton.Right)
        {
            // MouseDevice implicitly captures the pressed source before routing input.
            // Release only this editor's capture so SelectableTextBlock does not collapse
            // the source RichEdit selection. Control still raises its ordinary context
            // request from the real event's source and unshifted pointer coordinates.
            if (e.Pointer.Captured == this)
            {
                e.Pointer.Capture(null);
            }

            base.OnPointerReleased(e);
            return;
        }

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

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_nativeAnchorInsets is { } insets)
        {
            // DefaultLayout anchors use the parent's initial bounds, not the child's
            // Margin. Keep this width consistent between wrapping and final arrangement.
            availableSize = availableSize.WithWidth(Math.Max(0,
                availableSize.Width - insets.Left - insets.Right + Margin.Left + Margin.Right));
        }

        // RichEdit's ContentsResized describes every wrapped line, independently of
        // the current client height. The parent retains its previous absolute row
        // height until this notification arrives; feeding that height back into
        // TextBlock's layout would truncate newly loaded refs to the previous row.
        Size contentsSize = _usesNativeContentsHeightMeasurement
            ? availableSize.WithHeight(double.PositiveInfinity)
            : availableSize;
        Size desiredSize = base.MeasureOverride(contentsSize);
        if (_usesNativeContentsHeightMeasurement)
        {
            int contentsHeight = (int)Math.Ceiling(TextLayout.Height);
            if (_contentsHeight != contentsHeight)
            {
                _contentsHeight = contentsHeight;
                PublishContentsResized();
            }
        }

        return desiredSize;
    }

    private void PublishContentsResized()
    {
        if (_contentsResizeSuspendCount == 0 && _reportedContentsHeight != _contentsHeight)
        {
            _reportedContentsHeight = _contentsHeight;
            ContentsResized?.Invoke(_contentsHeight);
        }
    }

    protected override void ArrangeCore(Rect finalRect)
    {
        if (_nativeAnchorInsets is { } insets)
        {
            finalRect = new Rect(new Point(
                finalRect.X + insets.Left - Margin.Left,
                finalRect.Y + insets.Top - Margin.Top), new Size(
                Math.Max(0, finalRect.Width - insets.Left - insets.Right + Margin.Left + Margin.Right),
                Math.Max(0, finalRect.Height - insets.Top - insets.Bottom + Margin.Top + Margin.Bottom)));
        }

        base.ArrangeCore(finalRect);
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

    protected override TextLayout CreateTextLayout(string? text)
    {
        if (!_usesNativeWidthMeasurement || _textRuns is null || FlowDirection != FlowDirection.LeftToRight || LineSpacing != 0)
        {
            return base.CreateTextLayout(text);
        }

        // ContentsResized and painting use the same native character advances. Merely
        // assigning a GDI-measured Width leaves Skia's wider advances clipping the final
        // characters. Preserve its glyphs and offsets; adapt advances before layout so
        // painting, selection and hit testing all see the same character positions.
        List<TextRun> runs = [];
        List<ValueSpan<TextRunProperties>>? selectionStyles = null;
        int sourcePosition = 0;
        foreach (TextRun run in _textRuns)
        {
            int runPosition = sourcePosition;
            sourcePosition += run.Length;
            int selectionStart = Math.Max(runPosition, Math.Min(SelectionStart, SelectionEnd));
            int selectionEnd = Math.Min(sourcePosition, Math.Max(SelectionStart, SelectionEnd));
            double nativeFontSize = GetLinkAtPosition(runPosition)?.FontSize ?? GetNativeFontSize();
            if (run is TextCharacters selectionCharacters && selectionEnd > selectionStart && SelectionForegroundBrush is not null)
            {
                // Retain SelectableTextBlock's foreground override for runs that use
                // framework fallback rather than the native shaped-buffer adaptation.
                selectionStyles ??= [];
                selectionStyles.Add(new ValueSpan<TextRunProperties>(selectionStart,
                    selectionEnd - selectionStart, GetSelectionProperties(selectionCharacters.Properties)));
            }

            if (run is not TextCharacters characters
                || !WinFormsRichEditTextMeasurer.TryGetCharacterAdvances(this, characters.Text.ToString(), out int[] advances, nativeFontSize)
                || !FontManager.Current.TryGetGlyphTypeface(characters.Properties.Typeface, out GlyphTypeface? glyphTypeface)
                || glyphTypeface is null)
            {
                runs.Add(run);
                continue;
            }

            GenericTextRunProperties runProperties = new(characters.Properties.Typeface, nativeFontSize,
                characters.Properties.TextDecorations, characters.Properties.ForegroundBrush,
                characters.Properties.BackgroundBrush, characters.Properties.BaselineAlignment,
                characters.Properties.CultureInfo, characters.Properties.FontFeatures);
            ShapedBuffer buffer = TextShaper.Current.ShapeText(characters.Text,
                new TextShaperOptions(glyphTypeface, nativeFontSize,
                    culture: characters.Properties.CultureInfo, fontFeatures: characters.Properties.FontFeatures));
            int firstCluster = buffer.Length == 0 ? 0 : buffer.Min(glyph => glyph.GlyphCluster);
            bool supported = buffer.Length > 0 && buffer.All(glyph => glyph.GlyphIndex != 0);
            for (int index = 0; supported && index < buffer.Length; index++)
            {
                int cluster = buffer[index].GlyphCluster - firstCluster;
                supported = cluster >= 0 && cluster < advances.Length
                    && (index == 0 || buffer[index].GlyphCluster >= buffer[index - 1].GlyphCluster);
            }

            if (!supported)
            {
                buffer.Dispose();
                runs.Add(run);
                continue;
            }

            for (int index = 0; index < buffer.Length;)
            {
                int cluster = buffer[index].GlyphCluster - firstCluster;
                double advance = 0;
                while (index < buffer.Length && buffer[index].GlyphCluster - firstCluster == cluster)
                {
                    advance += buffer[index++].GlyphAdvance;
                }

                int end = index < buffer.Length ? buffer[index].GlyphCluster - firstCluster : advances.Length;
                double nativeAdvance = advances[end - 1] - (cluster == 0 ? 0 : advances[cluster - 1]);
                GlyphInfo lastGlyph = buffer[index - 1];
                buffer[index - 1] = new GlyphInfo(lastGlyph.GlyphIndex, lastGlyph.GlyphCluster,
                    lastGlyph.GlyphAdvance + nativeAdvance - advance, lastGlyph.GlyphOffset);
            }

            AddNativeTextRuns(runs, buffer, runProperties, selectionStart - runPosition, selectionEnd - runPosition);
        }

        GenericTextRunProperties properties = new(new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
            FontSize, TextDecorations, Foreground, fontFeatures: FontFeatures);
        GenericTextParagraphProperties paragraph = new(FlowDirection,
            IsMeasureValid ? TextAlignment : TextAlignment.Left, true, false, properties,
            TextWrapping, LineHeight, 0, LetterSpacing);
        Size maximumSize = GetMaxSizeFromConstraint();
        return new TextLayout(new InlinesTextSource(runs, selectionStyles), paragraph, TextTrimming,
            maximumSize.Width, maximumSize.Height, MaxLines);
    }

    private void AddNativeTextRuns(List<TextRun> runs, ShapedBuffer buffer, TextRunProperties properties, int selectionStart, int selectionEnd)
    {
        if (SelectionForegroundBrush is null || selectionEnd <= selectionStart)
        {
            runs.Add(new ShapedTextRun(buffer, properties));
            return;
        }

        if (selectionStart > 0)
        {
            SplitResult<ShapedBuffer> prefix = buffer.Split(selectionStart);
            ShapedBuffer first = prefix.First
                ?? throw new InvalidOperationException("A native text prefix must contain its original glyphs.");
            if (!ReferenceEquals(first, buffer))
            {
                buffer.Dispose();
            }

            runs.Add(new ShapedTextRun(first, properties));
            selectionEnd -= first.Text.Length;
            if (prefix.Second is not { } remaining)
            {
                return;
            }

            buffer = remaining;
        }

        if (selectionEnd <= 0)
        {
            runs.Add(new ShapedTextRun(buffer, properties));
            return;
        }

        // Split the already shaped glyphs, not the source string: selecting text must
        // change its brush without reshaping it or changing the native advances/hit bounds.
        SplitResult<ShapedBuffer> selected = buffer.Split(selectionEnd);
        ShapedBuffer selectedBuffer = selected.First
            ?? throw new InvalidOperationException("A native text selection must contain its original glyphs.");
        if (!ReferenceEquals(selectedBuffer, buffer))
        {
            buffer.Dispose();
        }

        runs.Add(new ShapedTextRun(selectedBuffer, GetSelectionProperties(properties)));
        if (selected.Second is { } suffix)
        {
            runs.Add(new ShapedTextRun(suffix, properties));
        }
    }

    private TextRunProperties GetSelectionProperties(TextRunProperties properties)
        => new GenericTextRunProperties(properties.Typeface, properties.FontRenderingEmSize,
            properties.TextDecorations, SelectionForegroundBrush, properties.BackgroundBrush,
            properties.BaselineAlignment, properties.CultureInfo, properties.FontFeatures);

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

    private XhtmlLinkRun? GetLinkAtPosition(int position)
    {
        if (Inlines is null)
        {
            return null;
        }

        int start = 0;
        foreach (Inline inline in Inlines)
        {
            int length = inline switch
            {
                Run run => run.Text?.Length ?? 0,
                LineBreak => Environment.NewLine.Length,
                InlineUIContainer => 1,
                _ => 0,
            };
            if (inline is XhtmlLinkRun link && position >= start && position < start + length)
            {
                return link;
            }

            start += length;
        }

        return null;
    }

    private XhtmlLinkRun? GetLinkAtPoint(Point point)
    {
        point -= new Vector(NativeFormattingInset + Padding.Left, Padding.Top);
        TextHitTestResult hit = TextLayout.HitTestPoint(point);
        return hit.IsInside ? GetLinkAtPosition(hit.CharacterHit.FirstCharacterIndex) : null;
    }

    private void RememberNativePointerSelection(PointerPressedEventArgs e)
    {
        if (UsesNativeRichEditText && e.ClickCount == 1
            && !e.KeyModifiers.HasFlag(KeyModifiers.Shift)
            && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _pointerSelectionAnchor = SelectionStart;
        }
    }

    private void EnsureNativePointerLayout()
    {
        if (!IsMeasureValid && Bounds.Width > 0 && Bounds.Height > 0)
        {
            // Consecutive captured input can precede the normal layout pass. Rebuild
            // the same inline runs with the already arranged client constraint.
            Thickness insets = _nativeAnchorInsets ?? Margin;
            Measure(new Size(Bounds.Width + insets.Left + insets.Right,
                Bounds.Height + insets.Top + insets.Bottom));
        }
    }

    private int? GetNativePointerSelectionPosition(PointerEventArgs e)
    {
        if (!UsesNativeRichEditText || _pointerSelectionAnchor < 0 || e.Pointer.Captured != this
            || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed
            || Inlines?.Text is not { Length: > 0 } text
            || text.Any(character => character is not (>= ' ' and <= '~') and not '\r' and not '\n' and not '\uFFFC'))
        {
            return null;
        }

        Point point = e.GetPosition(this) - new Vector(NativeFormattingInset + Padding.Left, Padding.Top);
        return TextLayout.HitTestPoint(point).TextPosition;
    }

    private void UpdateNativePointerSelection(int? nativeSelectionPosition)
    {
        if (nativeSelectionPosition is not { } position || Inlines?.Text is not { Length: > 0 } text)
        {
            return;
        }

        // The source HWND retains ECO_AUTOWORDSELECTION despite the managed false
        // default. A drag within its first word selects characters; crossing a word
        // expands both endpoints, with a CFE_LINK caption acting as one native unit.
        (int start, int end) = GetNativeSelectionUnit(text, _pointerSelectionAnchor);
        if (position >= start && position < end)
        {
            SetCurrentValue(SelectionStartProperty, _pointerSelectionAnchor);
            return;
        }

        (int targetStart, int targetEnd) = GetNativeSelectionUnit(text, position);
        SetCurrentValue(SelectionStartProperty, position >= _pointerSelectionAnchor ? start : end);
        SetCurrentValue(SelectionEndProperty, position >= _pointerSelectionAnchor ? targetEnd : targetStart);
    }

    private (int Start, int End) GetNativeSelectionUnit(string text, int position)
    {
        position = Math.Clamp(position, 0, text.Length - 1);
        if (GetLinkAtPosition(position) is { } link && Inlines is not null)
        {
            int linkStart = 0;
            foreach (Inline inline in Inlines)
            {
                if (ReferenceEquals(inline, link))
                {
                    return (linkStart, linkStart + (link.Text?.Length ?? 0));
                }

                linkStart += inline switch
                {
                    Run run => run.Text?.Length ?? 0,
                    LineBreak => Environment.NewLine.Length,
                    InlineUIContainer => 1,
                    _ => 0,
                };
            }
        }

        int start = position;
        int end = position + 1;
        bool word = char.IsLetterOrDigit(text[position]) || text[position] == '_';
        while (start > 0 && GetLinkAtPosition(start - 1) is null
            && MatchesWordCategory(text[start - 1], word))
        {
            start--;
        }

        while (end < text.Length && GetLinkAtPosition(end) is null && MatchesWordCategory(text[end], word))
        {
            end++;
        }

        while (end < text.Length && text[end] is ' ' or '\t' or '\uFFFC')
        {
            end++;
        }

        return (start, end);
    }

    private static bool MatchesWordCategory(char character, bool word)
        => character is not (' ' or '\t' or '\r' or '\n' or '\uFFFC')
            && (char.IsLetterOrDigit(character) || character == '_') == word;

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
                AppendPlainLineBreak(plainText);
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
        if (_usesNativeWidthMeasurement
            && WinFormsRichEditTextMeasurer.TryGetCharacterAdvances(this, text, out int[] advances,
                GetNativeFontSize())
            && advances.Length > 0)
        {
            return advances[^1];
        }

        TextLayout textLayout = new(
            text,
            new Typeface(FontFamily, FontStyle, FontWeight),
            FontSize,
            foreground: null,
            letterSpacing: LetterSpacing);
        return textLayout.WidthIncludingTrailingWhitespace;
    }

    private double MeasureNativeInline(string text, bool rtfRoundTrip = false)
        => WinFormsRichEditTextMeasurer.TryGetCharacterAdvances(this, text, out int[] advances,
            GetNativeFontSize(rtfRoundTrip)) && advances.Length > 0
            ? advances[^1]
            : WinFormsTextMeasurer.Measure(this, text);

    private double GetNativeFontSize(bool rtfRoundTrip = false)
        => WinFormsRichEditTextMeasurer.GetFontSize(this, rtfRoundTrip);

    private void AddLineBreak()
    {
        // Native RichEdit exposes each paragraph as one LF position. Avalonia's
        // LineBreak uses Environment.NewLine, creating two caret positions on Windows.
        // Keep that framework behavior only for unrelated generic XHTML consumers.
        Inlines?.Add(UsesNativeRichEditText ? new Run("\n") : new LineBreak());
    }

    private void AppendPlainLineBreak(StringBuilder plainText)
        => plainText.Append(UsesNativeRichEditText ? "\n" : Environment.NewLine);

    private void AddLink(string caption, string uri, ref LineLayout lineLayout)
    {
        if (UsesNativeRichEditText)
        {
            // RichEdit CFE_LINK decorates real selectable characters. An embedded
            // button replaces the entire caption with one object position and cannot
            // preserve partial caption selection, source copy, or native press timing.
            XhtmlLinkRun run = new(caption, uri)
            {
                FontSize = GetNativeFontSize(rtfRoundTrip: caption != uri),
                TextDecorations = Avalonia.Media.TextDecorations.Underline,
            };
            run[!TextElement.ForegroundProperty] = new DynamicResourceExtension("GitExtensionsXhtmlLinkForegroundBrush");
            Inlines?.Add(run);
            lineLayout.Advance += MeasureNativeInline(caption, rtfRoundTrip: caption != uri);
            return;
        }

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

/// <summary>Preserves a source RichEdit link's selectable caption and URI.</summary>
internal sealed class XhtmlLinkRun(string caption, string linkUri) : Run(caption)
{
    /// <summary>Gets the actual target represented by the decorated source characters.</summary>
    public string LinkUri { get; } = linkUri;
}

/// <summary>Provides the target of an activated XHTML anchor.</summary>
public sealed class LinkClickedEventArgs(string linkUri) : EventArgs
{
    /// <summary>Gets the decoded absolute link URI.</summary>
    public string LinkUri { get; } = linkUri;
}
