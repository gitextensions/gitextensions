using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;
using AvaloniaToolTip = Avalonia.Controls.ToolTip;

namespace GitExtensions.Plugins.GitStatistics.PieChart;

/// <summary>
/// A pie-chart control for showing statistics.
/// </summary>
public partial class PieChartControl : UserControl
{
    public static readonly StyledProperty<double> InitialAngleProperty =
        AvaloniaProperty.Register<PieChartControl, double>(nameof(InitialAngle));

    private static readonly Color[] DefaultColors =
    [
        Colors.Red,
        Colors.Green,
        Colors.Blue,
        Colors.Yellow,
        Colors.Purple,
        Colors.Olive,
        Colors.Navy,
        Colors.Aqua,
        Colors.Lime,
        Colors.Maroon,
        Colors.Teal,
        Colors.Fuchsia,
    ];

    private double _bottomMargin;
    private Color[] _colors = DefaultColors;
    private EdgeColorType _edgeColorType = EdgeColorType.SystemColor;
    private double _edgeLineWidth = 1;
    private bool _fitChart;
    private int _highlightedIndex = -1;
    private double _leftMargin;
    private PieChart3D? _pieChart;
    private double[] _relativeSliceDisplacements = [0];
    private double _rightMargin;
    private ShadowStyle _shadowStyle = ShadowStyle.GradualShadow;
    private double _sliceRelativeHeight;
    private object[]? _tags;
    private double _topMargin;
    private decimal[] _values = [];

    static PieChartControl()
    {
        AffectsRender<PieChartControl>(InitialAngleProperty);
    }

    public PieChartControl()
    {
        InitializeComponent();
    }

    public string[]? ToolTips { get; set; }

    public double InitialAngle
    {
        get => GetValue(InitialAngleProperty);
        set => SetValue(InitialAngleProperty, value);
    }

    public event EventHandler<SliceSelectedArgs>? SliceSelected;

    public void SetLeftMargin(float value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        _leftMargin = value;
        InvalidateVisual();
    }

    public void SetRightMargin(float value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        _rightMargin = value;
        InvalidateVisual();
    }

    public void SetTopMargin(float value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        _topMargin = value;
        InvalidateVisual();
    }

    public void SetBottomMargin(float value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        _bottomMargin = value;
        InvalidateVisual();
    }

    public void SetFitChart(bool value)
    {
        _fitChart = value;
        InvalidateVisual();
    }

    public void SetValues(decimal[] value)
    {
        _values = value;
        InvalidateVisual();
    }

    public void SetTags(object[] value)
    {
        _tags = value;
        InvalidateVisual();
    }

    public void SetColors(Color[] value)
    {
        _colors = value.Length == 0 ? DefaultColors : value;
        InvalidateVisual();
    }

    public void SetSliceRelativeDisplacements(float[] value)
    {
        _relativeSliceDisplacements = value.Select(item => (double)item).ToArray();
        InvalidateVisual();
    }

    public void SetSliceRelativeHeight(float value)
    {
        _sliceRelativeHeight = value;
        InvalidateVisual();
    }

    public void SetShadowStyle(ShadowStyle value)
    {
        _shadowStyle = value;
        InvalidateVisual();
    }

    public void SetEdgeColorType(EdgeColorType value)
    {
        _edgeColorType = value;
        InvalidateVisual();
    }

    public void SetEdgeLineWidth(float value)
    {
        _edgeLineWidth = value;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        _pieChart = CreateChart();
        _pieChart?.Draw(context);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        AvaloniaToolTip.SetTip(this, null);
        if (_highlightedIndex != -1)
        {
            _highlightedIndex = -1;
            InvalidateVisual();
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        int index = FindSlice(e.GetPosition(this));
        UpdateHover(index);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        int index = FindSlice(e.GetPosition(this));
        UpdateHover(index);
        if (index != -1)
        {
            SelectSlice(index);
            e.Handled = true;
        }

        base.OnPointerPressed(e);
    }

    private PieChart3D? CreateChart()
    {
        if (_values.Length == 0 || _values.All(value => value == 0))
        {
            return null;
        }

        double availableWidth = Bounds.Width - _leftMargin - _rightMargin;
        double availableHeight = Bounds.Height - _topMargin - _bottomMargin;
        if (availableWidth <= 0 || availableHeight <= 0)
        {
            return null;
        }

        double size = Math.Min(availableWidth, availableHeight);
        PieChart3D chart = new(
            _leftMargin,
            _topMargin,
            size,
            size,
            _values,
            _colors,
            _sliceRelativeHeight)
        {
            FitToBoundingRectangle = _fitChart,
            EdgeColorType = _edgeColorType,
            EdgeLineWidth = _edgeLineWidth,
            ShadowStyle = _shadowStyle,
            HighlightedIndex = _highlightedIndex,
        };
        chart.SetInitialAngle(InitialAngle);
        chart.SetSliceRelativeDisplacements(_relativeSliceDisplacements);
        return chart;
    }

    private int FindSlice(Point point)
    {
        _pieChart ??= CreateChart();
        return _pieChart?.FindPieSliceUnderPoint(point) ?? -1;
    }

    private void UpdateHover(int index)
    {
        AvaloniaToolTip.SetTip(this, index == -1 ? null : GetToolTip(index));
        Cursor = index == -1 ? Cursor.Default : new Cursor(StandardCursorType.Hand);
        if (_highlightedIndex != index)
        {
            _highlightedIndex = index;
            InvalidateVisual();
        }
    }

    private string GetToolTip(int index)
        => ToolTips is not null && index < ToolTips.Length && ToolTips[index].Length > 0
            ? ToolTips[index]
            : _values[index].ToString(CultureInfo.CurrentCulture);

    private void SelectSlice(int index)
        => SliceSelected?.Invoke(
            this,
            new SliceSelectedArgs(
                _values[index],
                GetToolTip(index),
                _tags is not null && index < _tags.Length ? _tags[index] : null));

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(PieChartControl control)
    {
        public int HighlightedIndex => control._highlightedIndex;

        public int SliceCount
        {
            get
            {
                control._pieChart = control.CreateChart();
                return control._pieChart?.SliceCount ?? 0;
            }
        }

        public Point GetSliceHitPoint(int index)
        {
            control._pieChart = control.CreateChart();
            return control._pieChart?.GetSliceHitPoint(index)
                ?? throw new InvalidOperationException("The chart has no drawable slices.");
        }

        public int FindSlice(Point point) => control.FindSlice(point);

        public void Hover(Point point) => control.UpdateHover(control.FindSlice(point));

        public void Select(Point point)
        {
            int index = control.FindSlice(point);
            if (index != -1)
            {
                control.SelectSlice(index);
            }
        }
    }
}
