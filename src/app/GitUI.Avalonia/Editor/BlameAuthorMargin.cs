using System.Globalization;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using GitUI.Avatars;

namespace GitUI.Editor;

/// <summary>
/// This class displays the author gutter of a blame control.
/// </summary>
/// <remarks>
/// Twin of GitUI/Editor/BlameAuthorMargin.cs with an extended role: the WinForms margin
/// renders only the avatars while the author text is a second, scroll-synchronised
/// editor. This AvaloniaEdit margin renders the age marker and the author line itself,
/// which replaces that second editor and its scroll synchronisation. Encoded avatar bytes
/// remain owned by the entries; this margin owns and disposes the decoded Avalonia bitmaps.
/// </remarks>
public class BlameAuthorMargin : AbstractMargin, GitUI.IPersistedSplitter
{
    private const int AgeBucketMarkerWidth = 4;
    private const double MinimumWidth = 24;
    private const double ResizeGripWidth = 5;
    private const double TextPadding = 3;

    private static readonly Cursor ResizeCursor = new(StandardCursorType.SizeWestEast);

    private readonly Typeface _typeface;
    private readonly double _fontSize;
    private readonly Dictionary<int, IBrush> _brushs = [];
    private readonly List<(int StartLine, int EndLine, IBrush Brush)> _highlights = [];
    private string[] _authorLines = [];
    private Bitmap?[] _avatars = [];
    private GitBlameEntry[] _blameLines = [];
    private int _contentVersion;
    private bool _isResizing;
    private bool _showAvatars;
    private double _resizeStartPointerX;
    private double _resizeStartWidth;
    private double? _userWidth;
    private double _width;

    public BlameAuthorMargin(Typeface typeface, double fontSize)
    {
        _typeface = typeface;
        _fontSize = fontSize;
        ActualThemeVariantChanged += (_, _) => InvalidateVisual();
    }

    public int AvatarSize => Math.Max(1, (int)Math.Ceiling(CreateFormattedText("Ag").Height));

    /// <summary>
    ///  Shows the author gutter built by the blame control: one line of text per file
    ///  line (empty for continuation lines of the same commit) plus age-bucket markers.
    /// </summary>
    public int Initialize(string gutter, IEnumerable<GitBlameEntry> blameLines, bool showAvatars = false)
    {
        _contentVersion++;
        _authorLines = gutter.Split('\n').Select(line => line.TrimEnd('\r', ' ')).ToArray();
        _blameLines = [.. blameLines];
        _showAvatars = showAvatars;
        ReplaceAvatars([.. _blameLines.Select(entry => AvatarImage.Decode(entry.Avatar))]);

        RebuildAgeBrushes();

        double maxTextWidth = 0;
        foreach (string line in _authorLines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                maxTextWidth = Math.Max(maxTextWidth, CreateFormattedText(line).Width);
            }
        }

        double avatarWidth = _showAvatars ? AvatarSize + TextPadding : 0;
        _width = AgeBucketMarkerWidth + avatarWidth + TextPadding + maxTextWidth + TextPadding;
        InvalidateMeasure();
        InvalidateVisual();
        return _contentVersion;
    }

    public void UpdateAgeBuckets(IEnumerable<GitBlameEntry> blameLines)
    {
        GitBlameEntry[] updatedLines = [.. blameLines];
        for (int index = 0; index < Math.Min(_blameLines.Length, updatedLines.Length); index++)
        {
            _blameLines[index].AgeBucketIndex = updatedLines[index].AgeBucketIndex;
            _blameLines[index].AgeBucketColor = updatedLines[index].AgeBucketColor;
        }

        RebuildAgeBrushes();
        InvalidateVisual();
    }

    internal void SetAvatar(int lineIndex, byte[]? imageData, int contentVersion)
    {
        if (contentVersion != _contentVersion || lineIndex < 0 || lineIndex >= _avatars.Length)
        {
            return;
        }

        Bitmap? avatar = AvatarImage.Decode(imageData);
        Bitmap? previous = _avatars[lineIndex];
        _avatars[lineIndex] = avatar;
        _blameLines[lineIndex].Avatar = imageData;
        previous?.Dispose();
        InvalidateVisual();
    }

    /// <summary>
    ///  Clears the gutter while a new blame is loading.
    /// </summary>
    public void Clear()
    {
        _contentVersion++;
        _authorLines = [];
        _blameLines = [];
        _showAvatars = false;
        ReplaceAvatars([]);
        _highlights.Clear();
        InvalidateMeasure();
        InvalidateVisual();
    }

    /// <summary>
    ///  Adds a background highlight for an inclusive range of zero-based lines,
    ///  mirroring the FileViewer method so ported blame code applies to both.
    /// </summary>
    public void HighlightLines(int startLine, int endLine, System.Drawing.Color color)
    {
        _highlights.Add((startLine, endLine, ToBrush(color)));
    }

    /// <summary>
    ///  Removes all line highlights.
    /// </summary>
    public void ClearHighlighting()
    {
        _highlights.Clear();
    }

    /// <summary>
    ///  Redraws the gutter, like the WinForms control method.
    /// </summary>
    public void Refresh()
    {
        InvalidateVisual();
    }

    /// <summary>
    ///  Gets the zero-based line index at a y position relative to this margin,
    ///  or a value past the last line when no line is there (like WinForms).
    /// </summary>
    public int GetLineFromVisualPosY(double visualPosY)
    {
        TextView? textView = TextView;
        if (textView is null)
        {
            return int.MaxValue;
        }

        VisualLine? visualLine = textView.GetVisualLineFromVisualTop(visualPosY + textView.ScrollOffset.Y);
        return visualLine is null ? int.MaxValue : visualLine.FirstDocumentLine.LineNumber - 1;
    }

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
    {
        return new Avalonia.Size(_authorLines.Length == 0 ? 0 : _userWidth ?? _width, 0);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        double pointerX = e.GetPosition(this).X;
        if (_isResizing)
        {
            _userWidth = Math.Max(MinimumWidth, _resizeStartWidth + pointerX - _resizeStartPointerX);
            InvalidateMeasure();
            InvalidateVisual();
            e.Handled = true;
            return;
        }

        Cursor = pointerX >= Bounds.Width - ResizeGripWidth ? ResizeCursor : null;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.IsLeftButtonPressed && point.Position.X >= Bounds.Width - ResizeGripWidth)
        {
            _isResizing = true;
            _resizeStartPointerX = point.Position.X;
            _resizeStartWidth = Bounds.Width;
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isResizing)
        {
            _isResizing = false;
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
    {
        if (oldTextView is not null)
        {
            oldTextView.VisualLinesChanged -= TextView_VisualLinesChanged;
        }

        base.OnTextViewChanged(oldTextView, newTextView);

        if (newTextView is not null)
        {
            newTextView.VisualLinesChanged += TextView_VisualLinesChanged;
        }
    }

    private void TextView_VisualLinesChanged(object? sender, EventArgs e)
    {
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        // A filled background keeps the margin hit-testable for hover and context menu.
        context.FillRectangle(Brushes.Transparent, new Avalonia.Rect(Bounds.Size));

        TextView? textView = TextView;
        if (textView is null || !textView.VisualLinesValid || _authorLines.Length == 0)
        {
            return;
        }

        foreach (VisualLine visualLine in textView.VisualLines)
        {
            int index = visualLine.FirstDocumentLine.LineNumber - 1;
            if (index >= _authorLines.Length)
            {
                break;
            }

            double y = visualLine.VisualTop - textView.ScrollOffset.Y;

            foreach ((int startLine, int endLine, IBrush brush) in _highlights)
            {
                if (index >= startLine && index <= endLine)
                {
                    context.FillRectangle(brush, new Avalonia.Rect(0, y, Bounds.Width, visualLine.Height));
                    break;
                }
            }

            if (index < _blameLines.Length)
            {
                context.FillRectangle(
                    _brushs[_blameLines[index].AgeBucketIndex],
                    new Avalonia.Rect(0, y, AgeBucketMarkerWidth, visualLine.Height));
            }

            double textX = AgeBucketMarkerWidth + TextPadding;
            if (_showAvatars)
            {
                double avatarSize = Math.Min(AvatarSize, visualLine.Height);
                if (index < _avatars.Length && _avatars[index] is Bitmap avatar)
                {
                    double imageY = y + ((visualLine.Height - avatarSize) / 2);
                    context.DrawImage(
                        avatar,
                        new Avalonia.Rect(avatar.Size),
                        new Avalonia.Rect(AgeBucketMarkerWidth, imageY, avatarSize, avatarSize));
                }

                textX += AvatarSize + TextPadding;
            }

            string text = _authorLines[index];
            if (!string.IsNullOrEmpty(text))
            {
                context.DrawText(CreateFormattedText(text), new Avalonia.Point(textX, y));
            }
        }
    }

    private void ReplaceAvatars(Bitmap?[] avatars)
    {
        foreach (Bitmap? avatar in _avatars)
        {
            avatar?.Dispose();
        }

        _avatars = avatars;
    }

    private void RebuildAgeBrushes()
    {
        _brushs.Clear();
        foreach (GitBlameEntry blameLine in _blameLines)
        {
            if (!_brushs.ContainsKey(blameLine.AgeBucketIndex))
            {
                _brushs.Add(blameLine.AgeBucketIndex, ToBrush(blameLine.AgeBucketColor));
            }
        }
    }

    internal PixelSize? GetAvatarPixelSize(int lineIndex)
        => lineIndex >= 0 && lineIndex < _avatars.Length ? _avatars[lineIndex]?.PixelSize : null;

    private FormattedText CreateFormattedText(string text)
        => new(
            text,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            _typeface,
            _fontSize,
            GetResourceBrush("GitExtensionsBlameAuthorBrush", Brushes.Gray));

    private IBrush GetResourceBrush(string resourceKey, IBrush fallback)
        => Application.Current?.TryGetResource(resourceKey, ActualThemeVariant, out object? resource) == true
            && resource is IBrush brush
                ? brush
                : fallback;

    private static IBrush ToBrush(System.Drawing.Color color)
        => new SolidColorBrush(Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B)).ToImmutable();

    double GitUI.IPersistedSplitter.SplitterSize => TextView?.Bounds.Width ?? Bounds.Width;

    double GitUI.IPersistedSplitter.SplitterDistance
    {
        get => Bounds.Width > 0 ? Bounds.Width : _userWidth ?? _width;
        set
        {
            _userWidth = Math.Max(MinimumWidth, value);
            InvalidateMeasure();
            InvalidateVisual();
        }
    }
}
