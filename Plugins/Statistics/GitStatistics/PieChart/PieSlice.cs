using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Drawing.PieChart {
	/// <summary>
	///   Object representing 3D pie.
	/// </summary>
    public class PieSlice : Object, IDisposable {

        /// <summary>
        ///   Initializes an empty instance of <c>PieSlice</c>.
        /// </summary>
        protected PieSlice() : base() {
        }

        /// <summary>
        ///   Initializes a new instance of flat <c>PieSlice</c> class with given 
        ///   bounds and visual style.
        /// </summary>
        /// <param name="xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name="yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name="widthBoundingRect">
        ///   Width of the rectangle that is used to draw the top surface of 
        ///   the pie slice.
        /// </param>
        /// <param name="heightBoundingRect">
        ///   Height of the rectangle that is used to draw the top surface of 
        ///   the pie slice.
        /// </param>
        /// <param name="startAngle">
        ///   Starting angle (in degrees) of the pie slice.
        /// </param>
        /// <param name="sweepAngle">
        ///   Sweep angle (in degrees) of the pie slice.
        /// </param>
        /// <param name="surfaceColor">
        ///   Color used to paint the pie slice.
        /// </param>
        public PieSlice(float xBoundingRect, float yBoundingRect, float widthBoundingRect, float heightBoundingRect, float startAngle, float sweepAngle, Color surfaceColor) 
            : this (xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect, 0F, startAngle, sweepAngle, surfaceColor, ShadowStyle.NoShadow, EdgeColorType.NoEdge) {
        }
            
        /// <summary>
        ///   Initializes a new instance of <c>PieSlice</c> class with given 
        ///   bounds and visual style.
        /// </summary>
        /// <param name="xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name="yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name="widthBoundingRect">
        ///   Width of the rectangle that is used to draw the top surface of 
        ///   the pie slice.
        /// </param>
        /// <param name="heightBoundingRect">
        ///   Height of the rectangle that is used to draw the top surface of 
        ///   the pie slice.
        /// </param>
        /// <param name="sliceHeight">
        ///   Height of the pie slice.
        /// </param>
        /// <param name="startAngle">
        ///   Starting angle (in degrees) of the pie slice.
        /// </param>
        /// <param name="sweepAngle">
        ///   Sweep angle (in degrees) of the pie slice.
        /// </param>
        /// <param name="surfaceColor">
        ///   Color used to paint the pie slice.
        /// </param>
        /// <param name="shadowStyle">
        ///   Shadow style used for slice rendering.
        /// </param>
        /// <param name="edgeColorType">
        ///   Edge color style used for slice rendering.
        /// </param>
        public PieSlice(float xBoundingRect, float yBoundingRect, float widthBoundingRect, float heightBoundingRect, float sliceHeight, float startAngle, float sweepAngle, Color surfaceColor, ShadowStyle shadowStyle, EdgeColorType edgeColorType) : this() {
            // set some persistent values
            m_actualStartAngle = startAngle;
            m_actualSweepAngle = sweepAngle;
            m_surfaceColor = surfaceColor;
            m_shadowStyle = shadowStyle;
            // create pens for rendering
            Color edgeLineColor = EdgeColor.GetRenderingColor(edgeColorType, surfaceColor);
            m_pen = new Pen(edgeLineColor);
            m_pen.LineJoin = LineJoin.Round;
            InitializePieSlice(xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect, sliceHeight);
        }

        /// <summary>
        ///   Initializes a new instance of <c>PieSlice</c> class with given 
        ///   bounds and visual style.
        /// </summary>
        /// <param name="boundingRect">
        ///   Bounding rectangle used to draw the top surface of the slice.
        /// </param>
        /// <param name="sliceHeight">
        ///   Pie slice height.
        /// </param>
        /// <param name="startAngle">
        ///   Starting angle (in degrees) of the pie slice.
        /// </param>
        /// <param name="sweepAngle">
        ///   Sweep angle (in degrees) of the pie slice.
        /// </param>
        /// <param name="surfaceColor">
        ///   Color used to render pie slice surface.
        /// </param>
        /// <param name="shadowStyle">
        ///   Shadow style used in rendering.
        /// </param>
        /// <param name="edgeColorType">
        ///   Edge color type used for rendering.
        /// </param>
        public PieSlice(RectangleF boundingRect, float sliceHeight, float startAngle, float sweepAngle, Color surfaceColor, ShadowStyle shadowStyle, EdgeColorType edgeColorType) 
            : this(boundingRect.X, boundingRect.Y, boundingRect.Width, boundingRect.Height, sliceHeight, startAngle, sweepAngle, surfaceColor, shadowStyle, edgeColorType) {
        }

        /// <summary>
        ///   Initializes a new instance of <c>PieSlice</c> class with given 
        ///   bounds and visual style.
        /// </summary>
        /// <param name="xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name="yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name="widthBoundingRect">
        ///   Width of the rectangle that is used to draw the top surface of 
        ///   the pie slice.
        /// </param>
        /// <param name="heightBoundingRect">
        ///   Height of the rectangle that is used to draw the top surface of 
        ///   the pie slice.
        /// </param>
        /// <param name="sliceHeight">
        ///   Height of the pie slice.
        /// </param>
        /// <param name="startAngle">
        ///   Starting angle (in degrees) of the pie slice.
        /// </param>
        /// <param name="sweepAngle">
        ///   Sweep angle (in degrees) of the pie slice.
        /// </param>
        /// <param name="surfaceColor">
        ///   Color used to render pie slice surface.
        /// </param>
        /// <param name="shadowStyle">
        ///   Shadow style used in rendering.
        /// </param>
        /// <param name="edgeColorType">
        ///   Edge color type used for rendering.
        /// </param>
        /// <param name="edgeLineWidth">
        ///   Edge line width.
        /// </param>
        public PieSlice(float xBoundingRect, float yBoundingRect, float widthBoundingRect, float heightBoundingRect, float sliceHeight, float startAngle, float sweepAngle, Color surfaceColor, ShadowStyle shadowStyle, EdgeColorType edgeColorType, float edgeLineWidth) 
            : this (xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect, sliceHeight, startAngle, sweepAngle, surfaceColor, shadowStyle, edgeColorType) {
            m_pen.Width = edgeLineWidth;
        }

        /// <summary>
        ///   Initializes a new instance of <c>PieSlice</c> class with given 
        ///   bounds and visual style.
        /// </summary>
        /// <param name="boundingRect">
        ///   Bounding rectangle used to draw the top surface of the pie slice.
        /// </param>
        /// <param name="sliceHeight">
        ///   Pie slice height.
        /// </param>
        /// <param name="startAngle">
        ///   Starting angle (in degrees) of the pie slice.
        /// </param>
        /// <param name="sweepAngle">
        ///   Sweep angle (in degrees) of the pie slice.
        /// </param>
        /// <param name="surfaceColor">
        ///   Color used to render pie slice surface.
        /// </param>
        /// <param name="shadowStyle">
        ///   Shadow style used in rendering.
        /// </param>
        /// <param name="edgeColorType">
        ///   Edge color type used for rendering.
        /// </param>
        /// <param name="edgeLineWidth">
        ///   Edge line width.
        /// </param>
        public PieSlice(Rectangle boundingRect, float sliceHeight, float startAngle, float sweepAngle, Color surfaceColor, ShadowStyle shadowStyle, EdgeColorType edgeColorType, float edgeLineWidth) 
            : this(boundingRect.X, boundingRect.Y, boundingRect.Width, boundingRect.Height, sliceHeight, startAngle, sweepAngle, surfaceColor, shadowStyle, edgeColorType, edgeLineWidth) {
        }

        /// <summary>
        ///   Disposes of all resources used by <c>PieSlice</c> object.
        /// </summary>
        public virtual void Dispose() {
            if (m_pen != null) {
                m_pen.Dispose();
                m_pen = null;
            }
            DisposeBrushes();
        }

        /// <summary>
        ///   Gets starting angle (in degrees) of the pie slice.
        /// </summary>
        public float StartAngle {
            get { return m_startAngle; }
        }

        /// <summary>
        ///   Gets sweep angle (in degrees) of the pie slice.
        /// </summary>
        public float SweepAngle {
            get { return m_sweepAngle; }
        }

        /// <summary>
        ///   Gets ending angle (in degrees) of the pie slice.
        /// </summary>
        public float EndAngle {
            get { return (m_startAngle + m_sweepAngle) % 360; } 
        }

        /// <summary>
        ///   Draws the pie slice.
        /// </summary>
        /// <param name="graphics">
        ///   <c>Graphics</c> used to draw the pie slice.
        /// </param>
        public void Draw(Graphics graphics) {
            // first draw wegde sides
            DrawVisibleStartSide(graphics);
            DrawVisibleEndSide(graphics);
            DrawVisiblePeriphery(graphics);
            // draw the top pie slice
            DrawTop(graphics);
        }

        /// <summary>
        ///   Checks if given pie slice contains given point.
        /// </summary>
        /// <param name="point">
        ///   <c>PointF</c> to check.
        /// </param>
        /// <returns>
        ///   <c>true</c> if point given is contained within the slice.
        /// </returns>
        public bool Contains(PointF point) {
            if (PieSliceContainsPoint(point))
                return true;
            if (PeripheryContainsPoint(point))
                return true;
            if (m_startSide.Contains(point))
                return true;
            if (m_endSide.Contains(point))
                return true;
            return false;
        }

        /// <summary>
        ///   Gets or sets the bounding rectangle.
        /// </summary>
        internal RectangleF BoundingRectangle {
            get { return m_boundingRectangle; }
            set { m_boundingRectangle = value; }
        }

        /// <summary>
        ///   Gets or sets the slice height.
        /// </summary>
        internal float SliceHeight {
            get { return m_sliceHeight; }
            set { m_sliceHeight = value; }
        }

        /// <summary>
        ///   Splits a pie slice into two on the split angle.
        /// </summary>
        /// <param name="splitAngle">
        ///   Angle at which splitting is performed.
        /// </param>
        /// <returns>
        ///   An array of two pie  slices.
        /// </returns>
        internal PieSlice[] Split(float splitAngle) {
            float transformedSplitAngle = TransformAngle(splitAngle);
            PieSlice pieSlice1 = (PieSlice)this.MemberwiseClone();
            pieSlice1.m_startAngle = transformedSplitAngle;
            pieSlice1.m_sweepAngle = (StartAngle + SweepAngle - transformedSplitAngle) % 360;
            pieSlice1.InitializeSides();
            PieSlice pieSlice2 = (PieSlice)this.MemberwiseClone();
            pieSlice2.m_sweepAngle = (transformedSplitAngle - StartAngle + 360) % 360;
            pieSlice2.InitializeSides();
            return new PieSlice[] { pieSlice1, pieSlice2 };
        }
            
        /// <summary>
        ///   Reajusts the pie slice to fit new bounding rectangle provided.
        /// </summary>
        /// <param name="xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name="yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name="widthBoundingRect">
        ///   Width of the rectangle that is used to draw the top surface of 
        ///   the pie slice.
        /// </param>
        /// <param name="heightBoundingRect">
        ///   Height of the rectangle that is used to draw the top surface of 
        ///   the pie slice.
        /// </param>
        /// <param name="sliceHeight">
        ///   Height of the pie slice.
        /// </param>
        internal void Readjust(float xBoundingRect, float yBoundingRect, float widthBoundingRect, float heightBoundingRect, float sliceHeight) {
            InitializePieSlice(xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect, sliceHeight);
        }

        /// <summary>
        ///   Draws visible start side.
        /// </summary>
        /// <param name="graphics">
        ///   <c>Graphics</c> used to draw the pie slice.
        /// </param>
        internal void DrawVisibleStartSide(Graphics graphics) {
            if (m_startSide != null) {
                m_startSide.Draw(graphics, m_pen, m_brushStartSide);
            }
        }

        /// <summary>
        ///   Draws visible end side.
        /// </summary>
        /// <param name="graphics">
        ///   <c>Graphics</c> used to draw the pie slice.
        /// </param>
        internal void DrawVisibleEndSide(Graphics graphics) {
            if (m_endSide != null) {
                m_endSide.Draw(graphics, m_pen, m_brushEndSide);
            }
        }

        /// <summary>
        ///   Draws visible outer periphery of the pie slice.
        /// </summary>
        /// <param name="graphics">
        ///   <c>Graphics</c> used to draw the pie slice.
        /// </param>
        internal void DrawVisiblePeriphery(Graphics graphics) {
            PeripherySurfaceBounds[] peripherySurfaceBounds = GetVisiblePeripherySurfaceBounds();
            foreach (PeripherySurfaceBounds surfaceBounds in peripherySurfaceBounds) {
                DrawCylinderSurfaceSection(graphics, m_pen, m_brushPeripherySurface, surfaceBounds.StartAngle, surfaceBounds.EndAngle, surfaceBounds.StartPoint, surfaceBounds.EndPoint);
            }
        }

        /// <summary>
        ///   Draws the top of the pie slice.
        /// </summary>
        /// <param name="graphics">
        ///   <c>Graphics</c> used to draw the pie slice.
        /// </param>
        internal void DrawTop(Graphics graphics) {
            graphics.FillPie(m_brushSurface, m_boundingRectangle.X, m_boundingRectangle.Y, m_boundingRectangle.Width, m_boundingRectangle.Height, m_startAngle, m_sweepAngle);
            graphics.DrawPie(m_pen, m_boundingRectangle, m_startAngle, m_sweepAngle);
        }

        /// <summary>
        ///   Calculates the smallest rectangle into which this pie slice fits.
        /// </summary>
        /// <returns>
        ///   <c>RectangleF</c> into which this pie slice fits exactly.
        /// </returns>
        internal RectangleF GetFittingRectangle() {
            RectangleF boundingRectangle = new RectangleF(m_pointStart.X, m_pointStart.Y, 0, 0);
            if ((m_startAngle == 0F) || (m_startAngle + m_sweepAngle >= 360)) 
                GraphicsUtil.IncludePointX(ref boundingRectangle, m_boundingRectangle.Right);
            if ((m_startAngle <= 90) && (m_startAngle + m_sweepAngle >= 90) || (m_startAngle + m_sweepAngle >= 450))
                GraphicsUtil.IncludePointY(ref boundingRectangle, m_boundingRectangle.Bottom + SliceHeight);
            if ((m_startAngle <= 180) && (m_startAngle + m_sweepAngle >= 180) || (m_startAngle + m_sweepAngle >= 540)) 
                GraphicsUtil.IncludePointX(ref boundingRectangle, m_boundingRectangle.Left);
            if ((m_startAngle <= 270) && (m_startAngle + m_sweepAngle >= 270) || (m_startAngle + m_sweepAngle >= 630)) 
                GraphicsUtil.IncludePointY(ref boundingRectangle, m_boundingRectangle.Top);
            GraphicsUtil.IncludePoint(ref boundingRectangle, m_center);
            GraphicsUtil.IncludePoint(ref boundingRectangle, m_centerBelow);
            GraphicsUtil.IncludePoint(ref boundingRectangle, m_pointStart);
            GraphicsUtil.IncludePoint(ref boundingRectangle, m_pointStartBelow);
            GraphicsUtil.IncludePoint(ref boundingRectangle, m_pointEnd);
            GraphicsUtil.IncludePoint(ref boundingRectangle, m_pointEndBelow);
            return boundingRectangle;
        }

        /// <summary>
        ///   Checks if given point is contained inside the pie slice.
        /// </summary>
        /// <param name="point">
        ///   <c>PointF</c> to check for.
        /// </param>
        /// <returns>
        ///   <c>true</c> if given point is inside the pie slice.
        /// </returns>
        internal bool PieSliceContainsPoint(PointF point) {
            return PieSliceContainsPoint(point, m_boundingRectangle.X, m_boundingRectangle.Y, m_boundingRectangle.Width, m_boundingRectangle.Height, m_startAngle, m_sweepAngle);
        }

        /// <summary>
        ///   Checks if given point is contained by cylinder periphery.
        /// </summary>
        /// <param name="point">
        ///   <c>PointF</c> to check for.
        /// </param>
        /// <returns>
        ///   <c>true</c> if given point is inside the cylinder periphery.
        /// </returns>
        internal bool PeripheryContainsPoint(PointF point) {
            PeripherySurfaceBounds[] peripherySurfaceBounds = GetVisiblePeripherySurfaceBounds();
            foreach (PeripherySurfaceBounds surfaceBounds in peripherySurfaceBounds) {
                if (CylinderSurfaceSectionContainsPoint(point, surfaceBounds.StartAngle, surfaceBounds.EndAngle, surfaceBounds.StartPoint, surfaceBounds.EndPoint))
                    return true;
            }
            return false;
        }

        /// <summary>
        ///   Checks if point provided is inside pie slice start cut side.
        /// </summary>
        /// <param name="point">
        ///   <c>PointF</c> to check.
        /// </param>
        /// <returns>
        ///   <c>true</c> if point is inside the start side.
        /// </returns>
        internal bool StartSideContainsPoint(PointF point) {
            if (m_sliceHeight > 0)
                return (m_startSide.Contains(point));
            return false;
        }

        /// <summary>
        ///   Checks if point provided is inside pie slice end cut side.
        /// </summary>
        /// <param name="point">
        ///   <c>PointF</c> to check.
        /// </param>
        /// <returns>
        ///   <c>true</c> if point is inside the end side.
        /// </returns>
        internal bool EndSideContainsPoint(PointF point) {
            if (m_sliceHeight > 0)
                return (m_endSide.Contains(point));
            return false;
        }

        /// <summary>
        ///   Checks if bottom side of the pie slice contains the point.
        /// </summary>
        /// <param name="point">
        ///   <c>PointF</c> to check.
        /// </param>
        /// <returns>
        ///   <c>true</c> if point is inside the bottom of the pie slice.
        /// </returns>
        internal bool BottomSurfaceSectionContainsPoint(PointF point) {
            if (m_sliceHeight > 0) {
                return (PieSliceContainsPoint(point, m_boundingRectangle.X, m_boundingRectangle.Y + m_sliceHeight, m_boundingRectangle.Width, m_boundingRectangle.Height, m_startAngle, m_sweepAngle));
            }
            return false;
        }

        /// <summary>
        ///   Creates brushes used to render the pie slice.
        /// </summary>
        /// <param name="surfaceColor">
        ///   Color used for rendering.
        /// </param>
        /// <param name="shadowStyle">
        ///   Shadow style used for rendering.
        /// </param>
        protected virtual void CreateSurfaceBrushes(Color surfaceColor, ShadowStyle shadowStyle) {
            DisposeBrushes();
            m_brushSurface = new SolidBrush(surfaceColor);
            m_brushSurfaceHighlighted = new SolidBrush(ColorUtil.CreateColorWithCorrectedLightness(surfaceColor, ColorUtil.BrightnessEnhancementFactor1));
            switch (shadowStyle) {
            case ShadowStyle.NoShadow:
                m_brushStartSide = m_brushEndSide = m_brushPeripherySurface = new SolidBrush(surfaceColor);
                break;
            case ShadowStyle.UniformShadow:
                m_brushStartSide = m_brushEndSide = m_brushPeripherySurface = new SolidBrush(ColorUtil.CreateColorWithCorrectedLightness(surfaceColor, -ColorUtil.BrightnessEnhancementFactor1));
                break;
            case ShadowStyle.GradualShadow:
                double angle = m_startAngle - 180 - s_shadowAngle;
                if (angle < 0)
                    angle += 360;
                m_brushStartSide = CreateBrushForSide(surfaceColor, angle);
                angle = m_startAngle + m_sweepAngle - s_shadowAngle;
                if (angle < 0)
                    angle += 360;
                m_brushEndSide = CreateBrushForSide(surfaceColor, angle);
                m_brushPeripherySurface = CreateBrushForPeriphery(surfaceColor);
                break;
            }
        }

        /// <summary>
        ///   Disposes of brush objects.
        /// </summary>
        protected void DisposeBrushes() {
            if (m_brushSurface != null) {
                m_brushSurface.Dispose();
                m_brushSurface = null;
            }
            if (m_brushStartSide != null) {
                m_brushStartSide.Dispose();
                m_brushStartSide = null;
            }
            if (m_brushEndSide != null) {
                m_brushEndSide.Dispose();
                m_brushEndSide = null;
            }
            if (m_brushPeripherySurface != null) {
                m_brushPeripherySurface.Dispose();
                m_brushPeripherySurface = null;
            }
        }

        /// <summary>
        ///   Creates a brush for start and end sides of the pie slice for 
        ///   gradual  shade.
        /// </summary>
        /// <param name="color">
        ///   Color used for pie slice rendering.
        /// </param>
        /// <param name="angle">
        ///   Angle of the surface.
        /// </param>
        /// <returns>
        ///   <c>Brush</c> object.
        /// </returns>
        protected virtual Brush CreateBrushForSide(Color color, double angle) {
            return new SolidBrush(ColorUtil.CreateColorWithCorrectedLightness(color, -(float)(ColorUtil.BrightnessEnhancementFactor1 * (1 - 0.8 * Math.Cos(angle * Math.PI / 180)))));
        }

        /// <summary>
        ///   Creates a brush for outer periphery of the pie slice used for 
        ///   gradual shadow.
        /// </summary>
        /// <param name="color">
        ///   Color used for pie slice rendering.
        /// </param>
        /// <returns>
        ///   <c>Brush</c> object.
        /// </returns>
        protected virtual Brush CreateBrushForPeriphery(Color color) {
            ColorBlend colorBlend = new ColorBlend();
            colorBlend.Colors = new Color[] { 
                                                ColorUtil.CreateColorWithCorrectedLightness(color, -ColorUtil.BrightnessEnhancementFactor1 / 2), 
                                                color, 
                                                ColorUtil.CreateColorWithCorrectedLightness(color, -ColorUtil.BrightnessEnhancementFactor1), 
            };
            colorBlend.Positions = new float[] { 0F, 0.1F, 1.0F };
            LinearGradientBrush brush = new LinearGradientBrush(m_boundingRectangle, Color.Blue, Color.White, LinearGradientMode.Horizontal);
            brush.InterpolationColors = colorBlend;
            return brush;
        }

        /// <summary>
        ///   Draws the outer periphery of the pie slice.
        /// </summary>
        /// <param name="graphics">
        ///   <c>Graphics</c> object used to draw the surface.
        /// </param>
        /// <param name="pen">
        ///   <c>Pen</c> used to draw outline.
        /// </param>
        /// <param name="brush">
        ///   <c>Brush</c> used to fill the quadrilateral.
        /// </param>
        /// <param name="boundingRect">
        ///   Bounding rectangle that is used to draw the top surface of the 
        ///   pie slice.
        /// </param>
        /// <param name="startAngle">
        ///   Start angle (in degrees) of the periphery section.
        /// </param>
        /// <param name="endAngle">
        ///   End angle (in degrees) of the periphery section.
        /// </param>
        /// <param name="pointStart">
        ///   Point representing the start of the periphery.
        /// </param>
        /// <param name="pointEnd">
        ///   Point representing the end of the periphery.
        /// </param>
        protected void DrawCylinderSurfaceSection(Graphics graphics, Pen pen, Brush brush, float startAngle, float endAngle, PointF pointStart, PointF pointEnd) {
            GraphicsPath path = CreatePathForCylinderSurfaceSection(startAngle, endAngle, pointStart, pointEnd);
            graphics.FillPath(brush, path);
            graphics.DrawPath(pen, path);
        }

        /// <summary>
        ///   Transforms actual angle to angle used for rendering. They are 
        ///   different because of perspective.
        /// </summary>
        /// <param name="angle">
        ///   Actual angle.
        /// </param>
        /// <returns>
        ///   Rendering angle.
        /// </returns>
        protected float TransformAngle(float angle) {
            double x = m_boundingRectangle.Width * Math.Cos(angle * Math.PI / 180);
            double y = m_boundingRectangle.Height * Math.Sin(angle * Math.PI / 180);
            float result = (float)(Math.Atan2(y, x) * 180 / Math.PI);
            if (result < 0)
                return result + 360;
            return result;
        }

        /// <summary>
        ///   Calculates the point on ellipse periphery for angle.
        /// </summary>
        /// <param name="xCenter">
        ///   x-coordinate of the center of the ellipse.
        /// </param>
        /// <param name="yCenter">
        ///   y-coordinate of the center of the ellipse.
        /// </param>
        /// <param name="semiMajor">
        ///   Horizontal semi-axis.
        /// </param>
        /// <param name="semiMinor">
        ///   Vertical semi-axis.
        /// </param>
        /// <param name="angleDegrees">
        ///   Angle (in degrees) for which corresponding periphery point has to 
        ///   be obtained.
        /// </param>
        /// <returns>
        ///   <c>PointF</c> on the ellipse.
        /// </returns>
        protected PointF PeripheralPoint(float xCenter, float yCenter, float semiMajor, float semiMinor, float angleDegrees) {
            double angleRadians = angleDegrees * Math.PI / 180;
            return new PointF(xCenter + (float)(semiMajor * Math.Cos(angleRadians)), yCenter + (float)(semiMinor * Math.Sin(angleRadians)));
        }

        /// <summary>
        ///   Initializes pie bounding rectangle, pie height, corners 
        ///   coordinates and brushes used for rendering.
        /// </summary>
        /// <param name="xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name="yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name="widthBoundingRect">
        ///   Width of the rectangle that is used to draw the top surface of 
        ///   the pie slice.
        /// </param>
        /// <param name="heightBoundingRect">
        ///   Height of the rectangle that is used to draw the top surface of 
        ///   the pie slice.
        /// </param>
        /// <param name="sliceHeight">
        ///   Height of the pie slice.
        /// </param>
        private void InitializePieSlice(float xBoundingRect, float yBoundingRect, float widthBoundingRect, float heightBoundingRect, float sliceHeight) {
            // stores bounding rectangle and pie slice height
            m_boundingRectangle = new RectangleF(xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect);
            m_sliceHeight = sliceHeight;
            // recalculates start and sweep angle used for rendering
            m_startAngle = TransformAngle(m_actualStartAngle);
            m_sweepAngle = m_actualSweepAngle;
            if (m_sweepAngle % 180 != 0F)
                m_sweepAngle = TransformAngle(m_actualStartAngle + m_actualSweepAngle) - m_startAngle;
            if (m_sweepAngle < 0)
                m_sweepAngle += 360;
            // recreates brushes
            CreateSurfaceBrushes(m_surfaceColor, m_shadowStyle);
            // calculates center and end points on periphery
            float xCenter = xBoundingRect + widthBoundingRect / 2;
            float yCenter = yBoundingRect + heightBoundingRect / 2;
            m_center = new PointF(xCenter, yCenter);
            m_centerBelow = new PointF(xCenter, yCenter + sliceHeight);
            m_pointStart = PeripheralPoint(xCenter, yCenter, widthBoundingRect / 2, heightBoundingRect / 2, m_actualStartAngle);
            m_pointStartBelow = new PointF(m_pointStart.X, m_pointStart.Y + sliceHeight);
            m_pointEnd = PeripheralPoint(xCenter, yCenter, widthBoundingRect / 2, heightBoundingRect / 2, m_actualStartAngle + m_actualSweepAngle);
            m_pointEndBelow = new PointF(m_pointEnd.X, m_pointEnd.Y + sliceHeight);
            InitializeSides();
        }

        /// <summary>
        ///   Initializes start and end pie slice sides.
        /// </summary>
        private void InitializeSides() {
            if (m_startAngle > 90 && m_startAngle < 270)
                m_startSide = new Quadrilateral(m_center, m_pointStart, m_pointStartBelow, m_centerBelow, m_sweepAngle != 180);
            else 
                m_startSide = Quadrilateral.Empty;
            if (EndAngle > 270 || EndAngle < 90)
                m_endSide = new Quadrilateral(m_center, m_pointEnd, m_pointEndBelow, m_centerBelow, m_sweepAngle != 180);
            else 
                m_endSide = Quadrilateral.Empty;
        }
        
        /// <summary>
        ///   Gets an array of visible periphery bounds.
        /// </summary>
        /// <returns>
        ///   Array of <c>PeripherySurfaceBounds</c> objects.
        /// </returns>
        private PeripherySurfaceBounds[] GetVisiblePeripherySurfaceBounds() {
            ArrayList peripherySurfaceBounds = new ArrayList();
            // outer periphery side is visible only when startAngle or endAngle 
            // is between 0 and 180 degrees
            if (!(m_sweepAngle == 0 || (m_startAngle >= 180 && m_startAngle + m_sweepAngle <= 360))) {
                // draws the periphery from start angle to the end angle or left
                // edge, whichever comes first
                if (StartAngle < 180) {
                    float fi1 = m_startAngle;
                    PointF x1 = new PointF(m_pointStart.X, m_pointStart.Y);
                    float fi2 = EndAngle;
                    PointF x2 = new PointF(m_pointEnd.X, m_pointEnd.Y);
                    if (m_startAngle + m_sweepAngle > 180) {
                        fi2 = 180;
                        x2.X = m_boundingRectangle.X;
                        x2.Y = m_center.Y;
                    }
                    peripherySurfaceBounds.Add(new PeripherySurfaceBounds(fi1, fi2, x1, x2));
                }
                // if lateral surface is visible from the right edge 
                if (m_startAngle + m_sweepAngle > 360) {
                    float fi1 = 0;
                    PointF x1 = new PointF(m_boundingRectangle.Right, m_center.Y);
                    float fi2 = EndAngle;
                    PointF x2 = new PointF(m_pointEnd.X, m_pointEnd.Y);
                    if (fi2 > 180) {
                        fi2 = 180;
                        x2.X = m_boundingRectangle.Left;
                        x2.Y = m_center.Y;
                    }
                    peripherySurfaceBounds.Add(new PeripherySurfaceBounds(fi1, fi2, x1, x2));
                }
            }
            return (PeripherySurfaceBounds[])peripherySurfaceBounds.ToArray(typeof(PeripherySurfaceBounds));
        }

        /// <summary>
        ///   Creates <c>GraphicsPath</c> for cylinder surface section. This 
        ///   path consists of two arcs and two vertical lines.
        /// </summary>
        /// <param name="startAngle">
        ///   Starting angle of the surface.
        /// </param>
        /// <param name="endAngle">
        ///   Ending angle of the surface.
        /// </param>
        /// <param name="pointStart">
        ///   Starting point on the cylinder surface.
        /// </param>
        /// <param name="pointEnd">
        ///   Ending point on the cylinder surface.
        /// </param>
        /// <returns>
        ///   <c>GraphicsPath</c> object representing the cylinder surface.
        /// </returns>
        private GraphicsPath CreatePathForCylinderSurfaceSection(float startAngle, float endAngle, PointF pointStart, PointF pointEnd) {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(m_boundingRectangle, startAngle, endAngle - startAngle);
            path.AddLine(pointEnd.X, pointEnd.Y, pointEnd.X, pointEnd.Y + m_sliceHeight);
            path.AddArc(m_boundingRectangle.X, m_boundingRectangle.Y + m_sliceHeight, m_boundingRectangle.Width, m_boundingRectangle.Height, endAngle, startAngle - endAngle);
            path.AddLine(pointStart.X, pointStart.Y + m_sliceHeight, pointStart.X, pointStart.Y);
            return path;
        }

        /// <summary>
        ///   Checks if given point is contained within upper and lower pie 
        ///   slice surfaces or within the outer slice brink.
        /// </summary>
        /// <param name="point">
        ///   <c>PointF</c> structure to check for.
        /// </param>
        /// <param name="startAngle">
        ///   Start angle of the slice.
        /// </param>
        /// <param name="endAngle">
        ///   End angle of the slice.
        /// </param>
        /// <param name="point1">
        ///   Starting point on the periphery.
        /// </param>
        /// <param name="point2">
        ///   Ending point on the periphery.
        /// </param>
        /// <returns>
        ///   <c>true</c> if point given is contained.
        /// </returns>
        private bool CylinderSurfaceSectionContainsPoint(PointF point, float startAngle, float endAngle, PointF point1, PointF point2) {
            if (m_sliceHeight > 0) {
                return Quadrilateral.Contains(point, new PointF[] { point1, new PointF(point1.X, point1.Y + m_sliceHeight), new PointF(point2.X, point2.Y + m_sliceHeight), point2 } );
            }
            return false;
        }

        /// <summary>
        ///   Checks if point given is contained within the pie slice.
        /// </summary>
        /// <param name="point">
        ///   <c>PointF</c> to check for.
        /// </param>
        /// <param name="xBoundingRectangle">
        ///   x-coordinate of the rectangle that bounds the ellipse from which
        ///   slice is cut.
        /// </param>
        /// <param name="yBoundingRectangle">
        ///   y-coordinate of the rectangle that bounds the ellipse from which
        ///   slice is cut.
        /// </param>
        /// <param name="widthBoundingRectangle"> 
        ///   Width of the rectangle that bounds the ellipse from which
        ///   slice is cut.
        /// </param>
        /// <param name="heightBoundingRectangle">
        ///   Height of the rectangle that bounds the ellipse from which
        ///   slice is cut.
        /// </param>
        /// <param name="startAngle">
        ///   Start angle of the slice.
        /// </param>
        /// <param name="sweepAngle">
        ///   Sweep angle of the slice.
        /// </param>
        /// <returns>
        ///   <c>true</c> if point is contained within the slice.
        /// </returns>
        private bool PieSliceContainsPoint(PointF point, float xBoundingRectangle, float yBoundingRectangle, float widthBoundingRectangle, float heightBoundingRectangle, float startAngle, float sweepAngle) {
            double a = widthBoundingRectangle / 2;
            double b = heightBoundingRectangle / 2;
            double x = point.X - xBoundingRectangle - a;
            double y = point.Y - yBoundingRectangle - b;
            double angle = Math.Atan2(y, x);
            if (angle < 0)
                angle += (2 * Math.PI);
            double angleDegrees = angle * 180 / Math.PI;
            // point is inside the pie slice only if between start and end angle
            if ((angleDegrees >= startAngle && angleDegrees <= (startAngle + sweepAngle)) ||
                (startAngle + sweepAngle > 360) && ((angleDegrees + 360) <= (startAngle + sweepAngle))) {
                // distance of the point from the ellipse centre
                double r = Math.Sqrt(y * y + x * x);
                double a2 = a * a;
                double b2 = b * b;
                double cosFi = Math.Cos(angle);
                double sinFi = Math.Sin(angle);
                // distance of the ellipse perimeter point
                double ellipseRadius = (b * a) / Math.Sqrt(b2 * cosFi * cosFi + a2 * sinFi * sinFi);
                return ellipseRadius > r;
            }
            return false;
        }

        /// <summary>
        ///   Internal structure used to store periphery bounds data.
        /// </summary>
        private struct PeripherySurfaceBounds {
            public PeripherySurfaceBounds(float startAngle, float endAngle, PointF startPoint, PointF endPoint) {
                m_startAngle = startAngle;
                m_endAngle = endAngle;
                m_startPoint = startPoint;
                m_endPoint = endPoint;
            }

            public float StartAngle {
                get { return m_startAngle; }
            }

            public float EndAngle {
                get { return m_endAngle; }
            }

            public PointF StartPoint {
                get { return m_startPoint; }
            }

            public PointF EndPoint {
                get { return m_endPoint; }
            }

            private float m_startAngle;
            private float m_endAngle;
            private PointF m_startPoint;
            private PointF m_endPoint;
        }

        /// <summary>
        ///   Bounding rectangle that bounds the ellipse from which pie slice 
        ///   is cut.
        /// </summary>
        protected RectangleF  m_boundingRectangle;
        /// <summary>
        ///   Pie slice height.
        /// </summary>
        protected float       m_sliceHeight;
        /// <summary>
        ///   Start angle.
        /// </summary>
        protected float       m_startAngle;
        /// <summary>
        ///   Sweep angle.
        /// </summary>
        protected float       m_sweepAngle;
        /// <summary>
        ///   Actual start angle.
        /// </summary>
        private   float       m_actualStartAngle;
        /// <summary>
        ///   Actual sweep angle.
        /// </summary>
        private   float       m_actualSweepAngle;
        /// <summary>
        ///   Color of the surface.
        /// </summary>
        private   Color       m_surfaceColor = Color.Empty;
        /// <summary>
        ///   Style used for shadow.
        /// </summary>
        private   ShadowStyle m_shadowStyle = ShadowStyle.NoShadow;
        /// <summary>
        ///   <c>Brush</c> used to render slice top surface.
        /// </summary>
        protected Brush       m_brushSurface = null;
        /// <summary>
        ///   <c>Brush</c> used to render slice top surface when highlighted.
        /// </summary>
        protected Brush       m_brushSurfaceHighlighted = null;
        /// <summary>
        ///   <c>Brush</c> used to render slice starting cut side.
        /// </summary>
        protected Brush       m_brushStartSide = null;
        /// <summary>
        ///   <c>Brush</c> used to render slice ending cut side.
        /// </summary>
        protected Brush       m_brushEndSide = null;
        /// <summary>
        ///   <c>Brush</c> used to render pie slice periphery (cylinder outer surface).
        /// </summary>
        protected Brush       m_brushPeripherySurface = null;
        /// <summary>
        ///   <c>Pen</c> object used to draw pie slice edges.
        /// </summary>
        protected Pen         m_pen = null;
        /// <summary>
        ///   <c>PointF</c> corresponding to pie slice center.
        /// </summary>
        protected PointF      m_center;
        /// <summary>
        ///   <c>PointF</c> corresponding to the lower pie slice center.
        /// </summary>
        protected PointF      m_centerBelow;
        /// <summary>
        ///   <c>PointF</c> on the periphery corresponding to the start cut 
        ///   side.
        /// </summary>
        protected PointF      m_pointStart;
        /// <summary>
        ///   <c>PointF</c> on the periphery corresponding to the start cut 
        ///   side.
        /// </summary>
        protected PointF      m_pointStartBelow;
        /// <summary>
        ///   <c>PointF</c> on the periphery corresponding to the end cut 
        ///   side.
        /// </summary>
        protected PointF      m_pointEnd;
        /// <summary>
        ///   <c>PointF</c> on the periphery corresponding to the end cut 
        ///   side.
        /// </summary>
        protected PointF      m_pointEndBelow;
        /// <summary>
        ///   <c>Quadrilateral</c> representing the start side.
        /// </summary>
        protected Quadrilateral m_startSide = Quadrilateral.Empty;
        /// <summary>
        ///   <c>Quadrilateral</c> representing the end side.
        /// </summary>
        protected Quadrilateral m_endSide   = Quadrilateral.Empty;
        /// <summary>
        ///   Angle offset used to define reference angle for gradual shadow.
        /// </summary>
        private static float s_shadowAngle = 20F;
    }
}