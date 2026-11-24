using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using GitUI.Avatars;

namespace GitUI.Editor;

/// <summary>
/// This class display avatars in the gutter in a blame control.
/// </summary>
// Avalonia uses a native editor margin to draw the avatars and age markers.
public class BlameAuthorMargin : AbstractMargin
{
    private const int AgeBucketMarkerWidth = 4;
    private const double TextPadding = 3;

    private readonly Typeface _typeface;
    private readonly double _fontSize;
    private Bitmap?[] _avatars = [];
    private string[] _authorLines = [];
    private GitBlameEntry[] _blameLines = [];
    private readonly Dictionary<int, IBrush> _brushs = [];
    private int _contentVersion;
    private bool _showAvatars;
    private double _width;

    public BlameAuthorMargin(Typeface typeface, double fontSize)
    {
        _typeface = typeface;
        _fontSize = fontSize;
        ActualThemeVariantChanged += (_, _) => InvalidateVisual();
    }

    public int LineHeight => Math.Max(1, (int)Math.Ceiling(CreateFormattedText("Ag").Height));

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

        // Update the resolution otherwise the image is not drawn at the good size :(
        ReplaceAvatars([.. _blameLines.Select(entry => AvatarImage.Decode(entry.Avatar))]);

        // Build brushes
        RebuildAgeBrushes();

        double maxTextWidth = 0;
        foreach (string line in _authorLines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                maxTextWidth = Math.Max(maxTextWidth, CreateFormattedText(line).Width);
            }
        }

        double avatarWidth = _showAvatars ? LineHeight + TextPadding : 0;
        _width = AgeBucketMarkerWidth + avatarWidth + TextPadding + maxTextWidth + TextPadding;
        InvalidateMeasure();
        InvalidateVisual();
        return _contentVersion;
    }

    public void SetVisiblity(bool isVisible)
    {
        // Avalonia exposes visibility on the native margin.
        IsVisible = isVisible;
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
        InvalidateMeasure();
        InvalidateVisual();
    }

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
    {
        return new Avalonia.Size(_authorLines.Length == 0 ? 0 : _width, 0);
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

            RenderLine(context, textView, visualLine, index);
        }
    }

    private void RenderLine(DrawingContext context, TextView textView, VisualLine visualLine, int index)
    {
        double y = visualLine.VisualTop - textView.ScrollOffset.Y;
        if (index < _blameLines.Length)
        {
            context.FillRectangle(
                _brushs[_blameLines[index].AgeBucketIndex],
                new Avalonia.Rect(0, y, AgeBucketMarkerWidth, visualLine.Height));
        }

        double textX = RenderAvatar(context, visualLine, index, y);
        string text = _authorLines[index];
        if (!string.IsNullOrEmpty(text))
        {
            context.DrawText(CreateFormattedText(text), new Avalonia.Point(textX, y));
        }
    }

    private double RenderAvatar(DrawingContext context, VisualLine visualLine, int index, double y)
    {
        double textX = AgeBucketMarkerWidth + TextPadding;
        if (!_showAvatars)
        {
            return textX;
        }

        double avatarSize = Math.Min(LineHeight, visualLine.Height);
        if (index < _avatars.Length && _avatars[index] is Bitmap avatar)
        {
            double imageY = y + ((visualLine.Height - avatarSize) / 2);
            context.DrawImage(
                avatar,
                new Avalonia.Rect(avatar.Size),
                new Avalonia.Rect(AgeBucketMarkerWidth, imageY, avatarSize, avatarSize));
        }

        return textX + LineHeight + TextPadding;
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
}
