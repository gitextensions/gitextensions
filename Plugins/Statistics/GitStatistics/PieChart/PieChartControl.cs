using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace GitStatistics.PieChart
{
    /// <summary>
    ///   A PieChartControl for showing statistics.
    /// </summary>
    public class PieChartControl : Panel
    {
        private readonly ToolTip _toolTip = new ToolTip();
        private float _bottomMargin;
        private Color[] _colors;
        private int _defaultToolTipAutoPopDelay;
        private EdgeColorType _edgeColorType = EdgeColorType.SystemColor;
        private float _edgeLineWidth = 1F;
        private bool _fitChart;
        private int _highlightedIndex = -1;
        private float _initialAngle;
        private float _leftMargin;
        private PieChart3D _pieChart;
        private float[] _relativeSliceDisplacements = { 0F };
        private float _rightMargin;
        private ShadowStyle _shadowStyle = ShadowStyle.GradualShadow;
        private float _sliceRelativeHeight;
        private object[] _tags;
        private float _topMargin;
        private decimal[] _values;

        /// <summary>
        ///   Initializes the <c>PieChartControl</c>.
        /// </summary>
        public PieChartControl()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        /// <summary>
        ///   Gets or sets the tool tips.
        /// </summary>
        /// <value>The tool tips.</value>
        public string[] ToolTips { get; set; }

        /// <summary>
        ///   Sets the initial angle from which pies are drawn.
        /// </summary>
        public float InitialAngle
        {
            get => _initialAngle;
            set
            {
                _initialAngle = value;
                Invalidate();
            }
        }

        /// <summary>
        ///   Sets the left margin for the chart.
        /// </summary>
        public void SetLeftMargin(float value)
        {
            Debug.Assert(value >= 0, "value >= 0");
            _leftMargin = value;
            Invalidate();
        }

        /// <summary>
        ///   Sets the right margin for the chart.
        /// </summary>
        public void SetRightMargin(float value)
        {
            Debug.Assert(value >= 0, "value >= 0");
            _rightMargin = value;
            Invalidate();
        }

        /// <summary>
        ///   Sets the top margin for the chart.
        /// </summary>
        public void SetTopMargin(float value)
        {
            Debug.Assert(value >= 0, "value >= 0");
            _topMargin = value;
            Invalidate();
        }

        /// <summary>
        ///   Sets the bottom margin for the chart.
        /// </summary>
        public void SetBottomMargin(float value)
        {
            Debug.Assert(value >= 0, "value >= 0");
            _bottomMargin = value;
            Invalidate();
        }

        /// <summary>
        ///   Sets the indicator if chart should fit the bounding rectangle
        ///   exactly.
        /// </summary>
        public void SetFitChart(bool value)
        {
            _fitChart = value;
            Invalidate();
        }

        /// <summary>
        ///   Sets values to be represented by the chart.
        /// </summary>
        public void SetValues(decimal[] value)
        {
            _values = value;
            Invalidate();
        }

        public void SetTags(object[] value)
        {
            _tags = value;
            Invalidate();
        }

        /// <summary>
        ///   Sets colors to be used for rendering pie slices.
        /// </summary>
        public void SetColors(Color[] value)
        {
            _colors = value;
            Invalidate();
        }

        /// <summary>
        ///   Sets values for slice displacements.
        /// </summary>
        public void SetSliceRelativeDisplacements(float[] value)
        {
            _relativeSliceDisplacements = value;
            Invalidate();
        }

        /// <summary>
        ///   Sets pie slice relative height.
        /// </summary>
        public void SetSliceRelativeHeight(float value)
        {
            _sliceRelativeHeight = value;
            Invalidate();
        }

        /// <summary>
        ///   Sets the shadow style.
        /// </summary>
        public void SetShadowStyle(ShadowStyle value)
        {
            _shadowStyle = value;
            Invalidate();
        }

        /// <summary>
        ///   Sets the edge color type.
        /// </summary>
        public void SetEdgeColorType(EdgeColorType value)
        {
            _edgeColorType = value;
            Invalidate();
        }

        /// <summary>
        ///   Sets the edge lines width.
        /// </summary>
        public void SetEdgeLineWidth(float value)
        {
            _edgeLineWidth = value;
            Invalidate();
        }

        /// <summary>
        ///   Handles <c>OnPaint</c> event.
        /// </summary>
        /// <param name = "args">
        ///   <c>PaintEventArgs</c> object.
        /// </param>
        protected override void OnPaint(PaintEventArgs args)
        {
            DoDraw(args.Graphics);
        }

        /// <summary>
        ///   Handles <c>OnResize</c> event.
        /// </summary>
        protected override void OnResize(EventArgs args)
        {
            Refresh();
        }

        /// <summary>
        ///   Sets values for the chart and draws them.
        /// </summary>
        /// <param name = "graphics">
        ///   Graphics object used for drawing.
        /// </param>
        protected void DoDraw(Graphics graphics)
        {
            if (_values == null || _values.Length <= 0 || !HasNonZeroValue())
            {
                return;
            }

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var width = ClientSize.Width - _leftMargin - _rightMargin;
            var height = ClientSize.Height - _topMargin - _bottomMargin;

            // if the width or height if <=0 an exception would be thrown -> exit method..
            if (width <= 0 || height <= 0)
            {
                return;
            }

            _pieChart?.Dispose();
            if (_colors != null && _colors.Length > 0)
            {
                _pieChart = new PieChart3D(_leftMargin, _topMargin, width, height, _values, _colors,
                                           _sliceRelativeHeight);
            }
            else
            {
                _pieChart = new PieChart3D(_leftMargin, _topMargin, width, height, _values, _sliceRelativeHeight);
            }

            _pieChart.FitToBoundingRectangle = _fitChart;
            _pieChart.SetInitialAngle(_initialAngle);
            _pieChart.SetSliceRelativeDisplacements(_relativeSliceDisplacements);
            _pieChart.EdgeColorType = _edgeColorType;
            _pieChart.EdgeLineWidth = _edgeLineWidth;
            _pieChart.ShadowStyle = _shadowStyle;
            _pieChart.HighlightedIndex = _highlightedIndex;
            _pieChart.Draw(graphics);
        }

        private bool HasNonZeroValue()
        {
            return _values.Any(value => value != 0);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _defaultToolTipAutoPopDelay = _toolTip.AutoPopDelay;
            _toolTip.AutoPopDelay = short.MaxValue;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _toolTip.RemoveAll();
            _toolTip.AutoPopDelay = _defaultToolTipAutoPopDelay;
            _highlightedIndex = -1;
            Refresh();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_pieChart == null)
            {
                return;
            }

            var index = _pieChart.FindPieSliceUnderPoint(new PointF(e.X, e.Y));
            if (index != -1)
            {
                if (ToolTips == null || ToolTips.Length <= index || ToolTips[index].Length == 0)
                {
                    _toolTip.SetToolTip(this, _values[index].ToString());
                }
                else
                {
                    _toolTip.SetToolTip(this, ToolTips[index]);
                }
            }
            else
            {
                _toolTip.RemoveAll();
            }

            _highlightedIndex = index;
            Refresh();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (_pieChart != null)
            {
                var index = _pieChart.FindPieSliceUnderPoint(new PointF(e.X, e.Y));
                if (index != -1)
                {
                    if (ToolTips == null || ToolTips.Length <= index || ToolTips[index].Length == 0)
                    {
                        _toolTip.SetToolTip(this, _values[index].ToString());
                    }
                    else
                    {
                        _toolTip.SetToolTip(this, ToolTips[index]);
                    }

                    SliceSelected?.Invoke(this,
                        new SliceSelectedArgs(_values[index], _toolTip.GetToolTip(this), _tags?[index]));
                }
                else
                {
                    _toolTip.RemoveAll();
                }

                _highlightedIndex = index;
                Refresh();
            }

            base.OnMouseDown(e);
        }

        public event EventHandler<SliceSelectedArgs> SliceSelected;
    }
}