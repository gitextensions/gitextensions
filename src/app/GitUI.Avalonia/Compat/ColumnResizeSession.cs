using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace GitUI.Compat;

// Framework constraint: ListBox lacks DataGridView's captured column-divider preview and release commit.
internal sealed class ColumnResizeSession
{
    internal static readonly Cursor ResizeCursor = new(StandardCursorType.SizeWestEast);

    private readonly Control _owner;
    private readonly Panel _overlay;
    private readonly IPointer _pointer;
    private readonly Border _preview;
    private readonly Action<double> _commit;
    private readonly Action _restore;
    private readonly double _startX;
    private readonly double _startWidth;
    private readonly double _minimum;
    private readonly double _maximum;
    private readonly double _edge;
    private readonly Avalonia.Point _origin;
    private double _width;
    private bool _finished;

    internal ColumnResizeSession(Control owner, Panel overlay, PointerPressedEventArgs e,
        double width, double edge, double minimum, double maximum, Action<double> commit, Action restore)
    {
        _owner = owner;
        _overlay = overlay;
        _pointer = e.Pointer;
        _commit = commit;
        _restore = restore;
        _startX = e.GetPosition(owner).X;
        _startWidth = _width = width;
        _minimum = minimum;
        _maximum = Math.Max(minimum, maximum);
        _edge = edge;
        _origin = owner.TranslatePoint(default, overlay) ?? default;
        _preview = new Border
        {
            Name = "columnResizePreview",
            Width = 1,
            Height = owner.Bounds.Height,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            IsHitTestVisible = false,
            Margin = new Thickness(_origin.X + edge, _origin.Y, 0, 0),
        };
        _preview[!Border.BackgroundProperty] = new DynamicResourceExtension("GitExtensionsControlForegroundBrush");
        overlay.Children.Add(_preview);
        owner.Cursor = ResizeCursor;
        owner.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
        owner.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        owner.PointerCaptureLost += OnPointerCaptureLost;
        owner.DetachedFromVisualTree += OnDetached;
        e.Handled = true;
        e.Pointer.Capture(owner);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        _width = Math.Clamp(_startWidth + e.GetPosition(_owner).X - _startX, _minimum, _maximum);
        _preview.Margin = new Thickness(_origin.X + _edge + _width - _startWidth, _origin.Y, 0, 0);
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            OnPointerMoved(sender, e);
            try
            {
                _commit(_width);
            }
            finally
            {
                e.Handled = true;
                _pointer.Capture(null);
            }
        }
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e) => Finish();

    private void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _pointer.Capture(null);
        Finish();
    }

    private void Finish()
    {
        if (_finished)
        {
            return;
        }

        _finished = true;
        _owner.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        _owner.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        _owner.PointerCaptureLost -= OnPointerCaptureLost;
        _owner.DetachedFromVisualTree -= OnDetached;
        _overlay.Children.Remove(_preview);
        _owner.Cursor = null;
        _restore();
    }
}
