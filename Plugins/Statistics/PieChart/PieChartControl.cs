using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace System.Drawing.PieChart 
{
	/// <summary>
	/// Summary description for PieChartControl.
	/// </summary>
	public class PieChartControl : System.Windows.Forms.Panel 
	{
		/// <summary>
		///   Initializes the <c>PieChartControl</c>.
		/// </summary>
		public PieChartControl() : base() 
		{
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.SetStyle(ControlStyles.DoubleBuffer, true);
			this.SetStyle(ControlStyles.ResizeRedraw, true);
		}

		/// <summary>
		///   Sets the left margin for the chart.
		/// </summary>
		public float LeftMargin 
		{
			set 
			{ 
				Debug.Assert(value >= 0);
				m_leftMargin = value; 
				Invalidate();
			}
		}

		/// <summary>
		///   Sets the right margin for the chart.
		/// </summary>
		public float RightMargin 
		{
			set 
			{ 
				Debug.Assert(value >= 0);
				m_rightMargin = value; 
				Invalidate();
			}
		}

		/// <summary>
		///   Sets the top margin for the chart.
		/// </summary>
		public float TopMargin 
		{
			set 
			{ 
				Debug.Assert(value >= 0);
				m_topMargin = value;
				Invalidate();
			}
		}

		/// <summary>
		///   Sets the bottom margin for the chart.
		/// </summary>
		public float BottomMargin 
		{
			set 
			{ 
				Debug.Assert(value >= 0);
				m_bottomMargin = value; 
				Invalidate();
			}
		}

		/// <summary>
		///   Sets the indicator if chart should fit the bounding rectangle
		///   exactly.
		/// </summary>
		public bool FitChart 
		{
			set 
			{ 
				m_fitChart = value; 
				Invalidate();
			}
		}

		/// <summary>
		///   Sets values to be represented by the chart.
		/// </summary>
		public decimal[] Values 
		{
			set 
			{ 
				m_values = value;
				Invalidate();
			}
		}


		public object[] Tags
		{
			set
			{
				m_tags = value;
				Invalidate();
			}
		}
		/// <summary>
		///   Sets colors to be used for rendering pie slices.
		/// </summary>
		public Color[] Colors 
		{
			set 
			{ 
				m_colors = value; 
				Invalidate();
			}
		}

		/// <summary>
		///   Sets values for slice displacements.
		/// </summary>
		public float[] SliceRelativeDisplacements 
		{
			set 
			{ 
				m_relativeSliceDisplacements = value; 
				Invalidate();
			}
		}

		public string[] ToolTips 
		{
			set { m_toolTipTexts = value; }
			get { return m_toolTipTexts; }
		}
		/// <summary>
		///   Sets pie slice reative height.
		/// </summary>
		public float SliceRelativeHeight 
		{
			set 
			{ 
				m_sliceRelativeHeight = value; 
				Invalidate();
			}
		}

		/// <summary>
		///   Sets the shadow style.
		/// </summary>
		public ShadowStyle ShadowStyle 
		{
			set 
			{ 
				m_shadowStyle = value; 
				Invalidate();
			}
		}

		/// <summary>
		///  Sets the edge color type.
		/// </summary>
		public EdgeColorType EdgeColorType 
		{
			set 
			{ 
				m_edgeColorType = value; 
				Invalidate();
			}
		}

		/// <summary>
		///   Sets the edge lines width.
		/// </summary>
		public float EdgeLineWidth 
		{
			set 
			{ 
				m_edgeLineWidth = value; 
				Invalidate();
			}
		}

		/// <summary>
		///   Sets the initial angle from which pies are drawn.
		/// </summary>
		public float InitialAngle 
		{
			get{ return m_initialAngle; }
			set 
			{ 
				m_initialAngle = value; 
				Invalidate();
			}
		}

		/// <summary>
		///   Handles <c>OnPaint</c> event.
		/// </summary>
		/// <param name="args">
		///   <c>PaintEventArgs</c> object.
		/// </param>
		protected override void OnPaint(PaintEventArgs args) 
		{
			DoDraw(args.Graphics);
		}

		/// <summary>
		///   Handles <c>OnResize</c> event.
		/// </summary>
		/// <param name="args">
		/// </param>
		protected override void OnResize(EventArgs args) 
		{
			this.Refresh();
		}

		/// <summary>
		///   Sets values for the chart and draws them.
		/// </summary>
		/// <param name="graphics">
		///   Graphics object used for drawing.
		/// </param>
		protected void DoDraw(Graphics graphics) 
		{
			if (m_values != null && m_values.Length > 0) 
			{
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				float width = ClientSize.Width - m_leftMargin - m_rightMargin;
				float height = ClientSize.Height - m_topMargin - m_bottomMargin;
				// if the width or height if <=0 an exception would be thrown -> exit method..
				if (width <= 0 || height <= 0)
					return;
				if (m_pieChart != null)
					m_pieChart.Dispose();
				if (m_colors != null && m_colors.Length > 0)
					m_pieChart = new PieChart3D(m_leftMargin, m_topMargin, width, height, m_values, m_colors, m_sliceRelativeHeight); 
				else
					m_pieChart = new PieChart3D(m_leftMargin, m_topMargin, width, height, m_values, m_sliceRelativeHeight); 
				m_pieChart.FitToBoundingRectangle = m_fitChart;
				m_pieChart.InitialAngle = m_initialAngle;
				m_pieChart.SliceRelativeDisplacements = m_relativeSliceDisplacements;
				m_pieChart.EdgeColorType = m_edgeColorType;
				m_pieChart.EdgeLineWidth = m_edgeLineWidth;
				m_pieChart.ShadowStyle = m_shadowStyle;
				m_pieChart.HighlightedIndex = m_highlightedIndex;
				m_pieChart.Draw(graphics);
			}
		}

		protected override void OnMouseEnter(System.EventArgs e) 
		{
			base.OnMouseEnter(e);
			m_defaultToolTipAutoPopDelay = m_toolTip.AutoPopDelay;
			m_toolTip.AutoPopDelay = Int16.MaxValue;
		}

		protected override void OnMouseLeave(System.EventArgs e) 
		{
			base.OnMouseLeave(e);
			m_toolTip.RemoveAll();
			m_toolTip.AutoPopDelay = m_defaultToolTipAutoPopDelay;
			m_highlightedIndex = -1;
			Refresh();
		}

		protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) 
		{
			base.OnMouseMove(e);
			if (m_pieChart != null) 
			{
				int index = m_pieChart.FindPieSliceUnderPoint(new PointF(e.X, e.Y));
				if (index != -1) 
				{
					if (m_toolTipTexts == null || m_toolTipTexts.Length <= index || m_toolTipTexts[index].Length == 0)
						m_toolTip.SetToolTip(this, m_values[index].ToString());
					else
						m_toolTip.SetToolTip(this, m_toolTipTexts[index]);
				}
				else 
				{
					m_toolTip.RemoveAll();
				}
				m_highlightedIndex = index;
				Refresh();
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (m_pieChart != null) 
			{
				int index = m_pieChart.FindPieSliceUnderPoint(new PointF(e.X, e.Y));
				if (index != -1) 
				{
					if (m_toolTipTexts == null || m_toolTipTexts.Length <= index || m_toolTipTexts[index].Length == 0)
						m_toolTip.SetToolTip(this, m_values[index].ToString());
					else
						m_toolTip.SetToolTip(this, m_toolTipTexts[index]);
					
					if( SliceSelected != null )
						SliceSelected( this, new SliceSelectedArgs( m_values[index], m_toolTip.GetToolTip( this ), (m_tags!=null?m_tags[index]:null) ) );
				}
				else 
				{
					m_toolTip.RemoveAll();
				}
				m_highlightedIndex = index;
				Refresh();
			}
			base.OnMouseDown( e );			 
		}

		public event SliceSelectedHandler SliceSelected;

		private PieChart3D      m_pieChart = null;
		private float           m_leftMargin;
		private float           m_topMargin;
		private float           m_rightMargin;
		private float           m_bottomMargin;
		private bool            m_fitChart = false;

		private decimal[]       m_values = null;
		private Color[]         m_colors = null;
		private float           m_sliceRelativeHeight;
		private float[]         m_relativeSliceDisplacements = new float[] { 0F };
		private string[]        m_toolTipTexts = null;
		private object[]        m_tags = null;
		private ShadowStyle     m_shadowStyle = ShadowStyle.GradualShadow;
		private EdgeColorType   m_edgeColorType = EdgeColorType.SystemColor;
		private float           m_edgeLineWidth = 1F;
		private float           m_initialAngle;
		private int             m_highlightedIndex = -1;
		private ToolTip         m_toolTip = new ToolTip();
		/// <summary>
		///   Default AutoPopDelay of the ToolTip control.
		/// </summary>
		private int             m_defaultToolTipAutoPopDelay;
	}

	public delegate void SliceSelectedHandler( object sender, SliceSelectedArgs e );
	public class SliceSelectedArgs : EventArgs
	{
		public SliceSelectedArgs( decimal val, string hint )
		{
			value = val;
			tooltip = hint;
		}
		
		public SliceSelectedArgs( decimal val, string hint, Object t )
		{
			value = val;
			tooltip = hint;
			tag = t;
		}

		public decimal value;
		public string tooltip;
		public object tag;
	}
}
