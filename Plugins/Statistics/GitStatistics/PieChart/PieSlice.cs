using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GitStatistics.PieChart
{
    /// <summary>
    ///   Object representing 3D pie.
    /// </summary>
    public class PieSlice : IDisposable
    {
        /// <summary>
        ///   Angle offset used to define reference angle for gradual shadow.
        /// </summary>
        private const float ShadowAngle = 20F;

        /// <summary>
        ///   Actual start angle.
        /// </summary>
        private readonly float _actualStartAngle;

        /// <summary>
        ///   Actual sweep angle.
        /// </summary>
        private readonly float _actualSweepAngle;

        /// <summary>
        ///   Style used for shadow.
        /// </summary>
        private readonly ShadowStyle _shadowStyle;

        /// <summary>
        ///   Color of the surface.
        /// </summary>
        private readonly Color _surfaceColor;

        /// <summary>
        ///   <c>Brush</c> used to render slice ending cut side.
        /// </summary>
        protected Brush BrushEndSide;

        /// <summary>
        ///   <c>Brush</c> used to render pie slice periphery (cylinder outer surface).
        /// </summary>
        protected Brush BrushPeripherySurface;

        /// <summary>
        ///   <c>Brush</c> used to render slice starting cut side.
        /// </summary>
        protected Brush BrushStartSide;

        /// <summary>
        ///   <c>Brush</c> used to render slice top surface.
        /// </summary>
        protected Brush BrushSurface;

        /// <summary>
        ///   <c>Brush</c> used to render slice top surface when highlighted.
        /// </summary>
        protected Brush BrushSurfaceHighlighted;

        /// <summary>
        ///   <c>PointF</c> corresponding to pie slice center.
        /// </summary>
        protected PointF Center;

        /// <summary>
        ///   <c>PointF</c> corresponding to the lower pie slice center.
        /// </summary>
        protected PointF CenterBelow;

        /// <summary>
        ///   <c>Quadrilateral</c> representing the end side.
        /// </summary>
        protected Quadrilateral EndSide = Quadrilateral.Empty;

        /// <summary>
        ///   <c>Pen</c> object used to draw pie slice edges.
        /// </summary>
        protected Pen Pen;

        /// <summary>
        ///   <c>PointF</c> on the periphery corresponding to the end cut
        ///   side.
        /// </summary>
        protected PointF PointEnd;

        /// <summary>
        ///   <c>PointF</c> on the periphery corresponding to the end cut
        ///   side.
        /// </summary>
        protected PointF PointEndBelow;

        /// <summary>
        ///   <c>PointF</c> on the periphery corresponding to the start cut
        ///   side.
        /// </summary>
        protected PointF PointStart;

        /// <summary>
        ///   <c>PointF</c> on the periphery corresponding to the start cut
        ///   side.
        /// </summary>
        protected PointF PointStartBelow;

        /// <summary>
        ///   <c>Quadrilateral</c> representing the start side.
        /// </summary>
        protected Quadrilateral StartSide = Quadrilateral.Empty;

        /// <summary>
        ///   Initializes a new instance of flat <c>PieSlice</c> class with given
        ///   bounds and visual style.
        /// </summary>
        /// <param name = "xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name = "yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name = "widthBoundingRect">
        ///   Width of the rectangle that is used to draw the top surface of
        ///   the pie slice.
        /// </param>
        /// <param name = "heightBoundingRect">
        ///   Height of the rectangle that is used to draw the top surface of
        ///   the pie slice.
        /// </param>
        /// <param name = "startAngle">
        ///   Starting angle (in degrees) of the pie slice.
        /// </param>
        /// <param name = "sweepAngle">
        ///   Sweep angle (in degrees) of the pie slice.
        /// </param>
        /// <param name = "surfaceColor">
        ///   Color used to paint the pie slice.
        /// </param>
        public PieSlice(
            float xBoundingRect, float yBoundingRect, float widthBoundingRect,
            float heightBoundingRect, float startAngle, float sweepAngle, Color surfaceColor)
            : this(
                xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect, 0F, startAngle, sweepAngle,
                surfaceColor, ShadowStyle.NoShadow, EdgeColorType.NoEdge)
        {
        }

        /// <summary>
        ///   Initializes a new instance of <c>PieSlice</c> class with given
        ///   bounds and visual style.
        /// </summary>
        /// <param name = "xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name = "yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name = "widthBoundingRect">
        ///   Width of the rectangle that is used to draw the top surface of
        ///   the pie slice.
        /// </param>
        /// <param name = "heightBoundingRect">
        ///   Height of the rectangle that is used to draw the top surface of
        ///   the pie slice.
        /// </param>
        /// <param name = "sliceHeight">
        ///   Height of the pie slice.
        /// </param>
        /// <param name = "startAngle">
        ///   Starting angle (in degrees) of the pie slice.
        /// </param>
        /// <param name = "sweepAngle">
        ///   Sweep angle (in degrees) of the pie slice.
        /// </param>
        /// <param name = "surfaceColor">
        ///   Color used to paint the pie slice.
        /// </param>
        /// <param name = "shadowStyle">
        ///   Shadow style used for slice rendering.
        /// </param>
        /// <param name = "edgeColorType">
        ///   Edge color style used for slice rendering.
        /// </param>
        public PieSlice(
            float xBoundingRect, float yBoundingRect, float widthBoundingRect,
            float heightBoundingRect, float sliceHeight, float startAngle, float sweepAngle,
            Color surfaceColor, ShadowStyle shadowStyle, EdgeColorType edgeColorType)
        {
            // set some persistent values
            _actualStartAngle = startAngle;
            _actualSweepAngle = sweepAngle;
            _surfaceColor = surfaceColor;
            _shadowStyle = shadowStyle;

            // create pens for rendering
            var edgeLineColor = EdgeColor.GetRenderingColor(edgeColorType, surfaceColor);
            Pen = new Pen(edgeLineColor) { LineJoin = LineJoin.Round };
            InitializePieSlice(xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect, sliceHeight);
        }

        /// <summary>
        ///   Initializes a new instance of <c>PieSlice</c> class with given
        ///   bounds and visual style.
        /// </summary>
        /// <param name = "boundingRect">
        ///   Bounding rectangle used to draw the top surface of the slice.
        /// </param>
        /// <param name = "sliceHeight">
        ///   Pie slice height.
        /// </param>
        /// <param name = "startAngle">
        ///   Starting angle (in degrees) of the pie slice.
        /// </param>
        /// <param name = "sweepAngle">
        ///   Sweep angle (in degrees) of the pie slice.
        /// </param>
        /// <param name = "surfaceColor">
        ///   Color used to render pie slice surface.
        /// </param>
        /// <param name = "shadowStyle">
        ///   Shadow style used in rendering.
        /// </param>
        /// <param name = "edgeColorType">
        ///   Edge color type used for rendering.
        /// </param>
        public PieSlice(
            RectangleF boundingRect, float sliceHeight, float startAngle, float sweepAngle,
            Color surfaceColor, ShadowStyle shadowStyle, EdgeColorType edgeColorType)
            : this(
                boundingRect.X, boundingRect.Y, boundingRect.Width, boundingRect.Height, sliceHeight, startAngle,
                sweepAngle, surfaceColor, shadowStyle, edgeColorType)
        {
        }

        /// <summary>
        ///   Initializes a new instance of <c>PieSlice</c> class with given
        ///   bounds and visual style.
        /// </summary>
        /// <param name = "xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name = "yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name = "widthBoundingRect">
        ///   Width of the rectangle that is used to draw the top surface of
        ///   the pie slice.
        /// </param>
        /// <param name = "heightBoundingRect">
        ///   Height of the rectangle that is used to draw the top surface of
        ///   the pie slice.
        /// </param>
        /// <param name = "sliceHeight">
        ///   Height of the pie slice.
        /// </param>
        /// <param name = "startAngle">
        ///   Starting angle (in degrees) of the pie slice.
        /// </param>
        /// <param name = "sweepAngle">
        ///   Sweep angle (in degrees) of the pie slice.
        /// </param>
        /// <param name = "surfaceColor">
        ///   Color used to render pie slice surface.
        /// </param>
        /// <param name = "shadowStyle">
        ///   Shadow style used in rendering.
        /// </param>
        /// <param name = "edgeColorType">
        ///   Edge color type used for rendering.
        /// </param>
        /// <param name = "edgeLineWidth">
        ///   Edge line width.
        /// </param>
        public PieSlice(
            float xBoundingRect, float yBoundingRect, float widthBoundingRect,
            float heightBoundingRect, float sliceHeight, float startAngle, float sweepAngle,
            Color surfaceColor, ShadowStyle shadowStyle, EdgeColorType edgeColorType, float edgeLineWidth)
            : this(
                xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect,
                sliceHeight, startAngle, sweepAngle, surfaceColor, shadowStyle, edgeColorType)
        {
            Pen.Width = edgeLineWidth;
        }

        /// <summary>
        ///   Initializes a new instance of <c>PieSlice</c> class with given
        ///   bounds and visual style.
        /// </summary>
        /// <param name = "boundingRect">
        ///   Bounding rectangle used to draw the top surface of the pie slice.
        /// </param>
        /// <param name = "sliceHeight">
        ///   Pie slice height.
        /// </param>
        /// <param name = "startAngle">
        ///   Starting angle (in degrees) of the pie slice.
        /// </param>
        /// <param name = "sweepAngle">
        ///   Sweep angle (in degrees) of the pie slice.
        /// </param>
        /// <param name = "surfaceColor">
        ///   Color used to render pie slice surface.
        /// </param>
        /// <param name = "shadowStyle">
        ///   Shadow style used in rendering.
        /// </param>
        /// <param name = "edgeColorType">
        ///   Edge color type used for rendering.
        /// </param>
        /// <param name = "edgeLineWidth">
        ///   Edge line width.
        /// </param>
        public PieSlice(
            Rectangle boundingRect, float sliceHeight, float startAngle, float sweepAngle,
            Color surfaceColor, ShadowStyle shadowStyle, EdgeColorType edgeColorType,
            float edgeLineWidth)
            : this(
                boundingRect.X, boundingRect.Y, boundingRect.Width, boundingRect.Height, sliceHeight, startAngle,
                sweepAngle, surfaceColor, shadowStyle, edgeColorType, edgeLineWidth)
        {
        }

        /// <summary>
        ///   Gets starting angle (in degrees) of the pie slice.
        /// </summary>
        public float StartAngle { get; protected set; }

        /// <summary>
        ///   Gets sweep angle (in degrees) of the pie slice.
        /// </summary>
        public float SweepAngle { get; protected set; }

        /// <summary>
        ///   Gets ending angle (in degrees) of the pie slice.
        /// </summary>
        public float EndAngle => (StartAngle + SweepAngle) % 360;

        /// <summary>
        ///   Gets or sets the bounding rectangle.
        /// </summary>
        protected internal RectangleF BoundingRectangle { get; set; }

        /// <summary>
        ///   Gets or sets the slice height.
        /// </summary>
        protected internal float SliceHeight { get; set; }

        #region IDisposable Members

        /// <summary>
        ///   Disposes of all resources used by <c>PieSlice</c> object.
        /// </summary>
        public virtual void Dispose()
        {
            if (Pen != null)
            {
                Pen.Dispose();
                Pen = null;
            }

            DisposeBrushes();
        }

        #endregion

        /// <summary>
        ///   Draws the pie slice.
        /// </summary>
        /// <param name = "graphics">
        ///   <c>Graphics</c> used to draw the pie slice.
        /// </param>
        public void Draw(Graphics graphics)
        {
            // first draw wedge sides
            DrawVisibleStartSide(graphics);
            DrawVisibleEndSide(graphics);
            DrawVisiblePeriphery(graphics);

            // draw the top pie slice
            DrawTop(graphics);
        }

        /// <summary>
        ///   Checks if given pie slice contains given point.
        /// </summary>
        /// <param name = "point">
        ///   <c>PointF</c> to check.
        /// </param>
        /// <returns>
        ///   <c>true</c> if point given is contained within the slice.
        /// </returns>
        public bool Contains(PointF point)
        {
            if (PieSliceContainsPoint(point))
            {
                return true;
            }

            if (PeripheryContainsPoint(point))
            {
                return true;
            }

            return StartSide.Contains(point) || EndSide.Contains(point);
        }

        /// <summary>
        ///   Splits a pie slice into two on the split angle.
        /// </summary>
        /// <param name = "splitAngle">
        ///   Angle at which splitting is performed.
        /// </param>
        /// <returns>
        ///   An array of two pie  slices.
        /// </returns>
        internal PieSlice[] Split(float splitAngle)
        {
            var transformedSplitAngle = TransformAngle(splitAngle);
            var pieSlice1 = (PieSlice)MemberwiseClone();
            pieSlice1.StartAngle = transformedSplitAngle;
            pieSlice1.SweepAngle = (StartAngle + SweepAngle - transformedSplitAngle) % 360;
            pieSlice1.InitializeSides();
            var pieSlice2 = (PieSlice)MemberwiseClone();
            pieSlice2.SweepAngle = (transformedSplitAngle - StartAngle + 360) % 360;
            pieSlice2.InitializeSides();
            return new[] { pieSlice1, pieSlice2 };
        }

        /// <summary>
        ///   Readjusts the pie slice to fit new bounding rectangle provided.
        /// </summary>
        /// <param name = "xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name = "yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name = "widthBoundingRect">
        ///   Width of the rectangle that is used to draw the top surface of
        ///   the pie slice.
        /// </param>
        /// <param name = "heightBoundingRect">
        ///   Height of the rectangle that is used to draw the top surface of
        ///   the pie slice.
        /// </param>
        /// <param name = "sliceHeight">
        ///   Height of the pie slice.
        /// </param>
        internal void Readjust(
            float xBoundingRect, float yBoundingRect, float widthBoundingRect,
            float heightBoundingRect, float sliceHeight)
        {
            InitializePieSlice(xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect, sliceHeight);
        }

        /// <summary>
        ///   Draws visible start side.
        /// </summary>
        /// <param name = "graphics">
        ///   <c>Graphics</c> used to draw the pie slice.
        /// </param>
        internal void DrawVisibleStartSide(Graphics graphics)
        {
            StartSide?.Draw(graphics, Pen, BrushStartSide);
        }

        /// <summary>
        ///   Draws visible end side.
        /// </summary>
        /// <param name = "graphics">
        ///   <c>Graphics</c> used to draw the pie slice.
        /// </param>
        internal void DrawVisibleEndSide(Graphics graphics)
        {
            EndSide?.Draw(graphics, Pen, BrushEndSide);
        }

        /// <summary>
        ///   Draws visible outer periphery of the pie slice.
        /// </summary>
        /// <param name = "graphics">
        ///   <c>Graphics</c> used to draw the pie slice.
        /// </param>
        internal void DrawVisiblePeriphery(Graphics graphics)
        {
            var peripherySurfaceBounds = GetVisiblePeripherySurfaceBounds();
            foreach (var surfaceBounds in peripherySurfaceBounds)
            {
                DrawCylinderSurfaceSection(
                    graphics,
                    Pen,
                    BrushPeripherySurface,
                    surfaceBounds.StartAngle,
                    surfaceBounds.EndAngle,
                    surfaceBounds.StartPoint,
                    surfaceBounds.EndPoint);
            }
        }

        /// <summary>
        ///   Draws the top of the pie slice.
        /// </summary>
        /// <param name = "graphics">
        ///   <c>Graphics</c> used to draw the pie slice.
        /// </param>
        internal void DrawTop(Graphics graphics)
        {
            graphics.FillPie(
                BrushSurface,
                BoundingRectangle.X,
                BoundingRectangle.Y,
                BoundingRectangle.Width,
                BoundingRectangle.Height,
                StartAngle,
                SweepAngle);
            graphics.DrawPie(Pen, BoundingRectangle, StartAngle, SweepAngle);
        }

        /// <summary>
        ///   Calculates the smallest rectangle into which this pie slice fits.
        /// </summary>
        /// <returns>
        ///   <c>RectangleF</c> into which this pie slice fits exactly.
        /// </returns>
        internal RectangleF GetFittingRectangle()
        {
            var boundingRectangle = new RectangleF(PointStart.X, PointStart.Y, 0, 0);
            if ((StartAngle == 0F) || (StartAngle + SweepAngle >= 360))
            {
                GraphicsUtil.IncludePointX(ref boundingRectangle, BoundingRectangle.Right);
            }

            if (((StartAngle <= 90) && (StartAngle + SweepAngle >= 90)) || (StartAngle + SweepAngle >= 450))
            {
                GraphicsUtil.IncludePointY(ref boundingRectangle, BoundingRectangle.Bottom + SliceHeight);
            }

            if (((StartAngle <= 180) && (StartAngle + SweepAngle >= 180)) || (StartAngle + SweepAngle >= 540))
            {
                GraphicsUtil.IncludePointX(ref boundingRectangle, BoundingRectangle.Left);
            }

            if (((StartAngle <= 270) && (StartAngle + SweepAngle >= 270)) || (StartAngle + SweepAngle >= 630))
            {
                GraphicsUtil.IncludePointY(ref boundingRectangle, BoundingRectangle.Top);
            }

            GraphicsUtil.IncludePoint(ref boundingRectangle, Center);
            GraphicsUtil.IncludePoint(ref boundingRectangle, CenterBelow);
            GraphicsUtil.IncludePoint(ref boundingRectangle, PointStart);
            GraphicsUtil.IncludePoint(ref boundingRectangle, PointStartBelow);
            GraphicsUtil.IncludePoint(ref boundingRectangle, PointEnd);
            GraphicsUtil.IncludePoint(ref boundingRectangle, PointEndBelow);
            return boundingRectangle;
        }

        /// <summary>
        ///   Checks if given point is contained inside the pie slice.
        /// </summary>
        /// <param name = "point">
        ///   <c>PointF</c> to check for.
        /// </param>
        /// <returns>
        ///   <c>true</c> if given point is inside the pie slice.
        /// </returns>
        internal bool PieSliceContainsPoint(PointF point)
        {
            return PieSliceContainsPoint(
                point,
                BoundingRectangle.X,
                BoundingRectangle.Y,
                BoundingRectangle.Width,
                BoundingRectangle.Height,
                StartAngle,
                SweepAngle);
        }

        /// <summary>
        ///   Checks if given point is contained by cylinder periphery.
        /// </summary>
        /// <param name = "point">
        ///   <c>PointF</c> to check for.
        /// </param>
        /// <returns>
        ///   <c>true</c> if given point is inside the cylinder periphery.
        /// </returns>
        internal bool PeripheryContainsPoint(PointF point)
        {
            var peripherySurfaceBounds = GetVisiblePeripherySurfaceBounds();
            foreach (var surfaceBounds in peripherySurfaceBounds)
            {
                if (CylinderSurfaceSectionContainsPoint(
                    point,
                    surfaceBounds.StartPoint,
                    surfaceBounds.EndPoint))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///   Checks if point provided is inside pie slice start cut side.
        /// </summary>
        /// <param name = "point">
        ///   <c>PointF</c> to check.
        /// </param>
        /// <returns>
        ///   <c>true</c> if point is inside the start side.
        /// </returns>
        internal bool StartSideContainsPoint(PointF point)
        {
            return SliceHeight > 0 && StartSide.Contains(point);
        }

        /// <summary>
        ///   Checks if point provided is inside pie slice end cut side.
        /// </summary>
        /// <param name = "point">
        ///   <c>PointF</c> to check.
        /// </param>
        /// <returns>
        ///   <c>true</c> if point is inside the end side.
        /// </returns>
        internal bool EndSideContainsPoint(PointF point)
        {
            return SliceHeight > 0 && EndSide.Contains(point);
        }

        /// <summary>
        ///   Checks if bottom side of the pie slice contains the point.
        /// </summary>
        /// <param name = "point">
        ///   <c>PointF</c> to check.
        /// </param>
        /// <returns>
        ///   <c>true</c> if point is inside the bottom of the pie slice.
        /// </returns>
        internal bool BottomSurfaceSectionContainsPoint(PointF point)
        {
            return SliceHeight > 0 &&
                   PieSliceContainsPoint(
                       point, BoundingRectangle.X, BoundingRectangle.Y + SliceHeight,
                       BoundingRectangle.Width, BoundingRectangle.Height, StartAngle,
                       SweepAngle);
        }

        /// <summary>
        ///   Creates brushes used to render the pie slice.
        /// </summary>
        /// <param name = "surfaceColor">
        ///   Color used for rendering.
        /// </param>
        /// <param name = "shadowStyle">
        ///   Shadow style used for rendering.
        /// </param>
        protected virtual void CreateSurfaceBrushes(Color surfaceColor, ShadowStyle shadowStyle)
        {
            DisposeBrushes();
            BrushSurface = new SolidBrush(surfaceColor);
            BrushSurfaceHighlighted =
                new SolidBrush(
                    ColorUtil.CreateColorWithCorrectedLightness(
                        surfaceColor,
                        ColorUtil.BrightnessEnhancementFactor1));
            switch (shadowStyle)
            {
                case ShadowStyle.NoShadow:
                    BrushStartSide = BrushEndSide = BrushPeripherySurface = new SolidBrush(surfaceColor);
                    break;
                case ShadowStyle.UniformShadow:
                    BrushStartSide =
                        BrushEndSide =
                            BrushPeripherySurface =
                                new SolidBrush(
                                    ColorUtil.CreateColorWithCorrectedLightness(
                                        surfaceColor,
                                        -ColorUtil.BrightnessEnhancementFactor1));
                    break;
                case ShadowStyle.GradualShadow:
                    double angle = StartAngle - 180 - ShadowAngle;
                    if (angle < 0)
                    {
                        angle += 360;
                    }

                    BrushStartSide = CreateBrushForSide(surfaceColor, angle);
                    angle = StartAngle + SweepAngle - ShadowAngle;
                    if (angle < 0)
                    {
                        angle += 360;
                    }

                    BrushEndSide = CreateBrushForSide(surfaceColor, angle);
                    BrushPeripherySurface = CreateBrushForPeriphery(surfaceColor);
                    break;
            }
        }

        /// <summary>
        ///   Disposes of brush objects.
        /// </summary>
        protected void DisposeBrushes()
        {
            if (BrushSurface != null)
            {
                BrushSurface.Dispose();
                BrushSurface = null;
            }

            if (BrushStartSide != null)
            {
                BrushStartSide.Dispose();
                BrushStartSide = null;
            }

            if (BrushEndSide != null)
            {
                BrushEndSide.Dispose();
                BrushEndSide = null;
            }

            if (BrushPeripherySurface != null)
            {
                BrushPeripherySurface.Dispose();
                BrushPeripherySurface = null;
            }
        }

        /// <summary>
        ///   Creates a brush for start and end sides of the pie slice for
        ///   gradual  shade.
        /// </summary>
        /// <param name = "color">
        ///   Color used for pie slice rendering.
        /// </param>
        /// <param name = "angle">
        ///   Angle of the surface.
        /// </param>
        /// <returns>
        ///   <c>Brush</c> object.
        /// </returns>
        protected virtual Brush CreateBrushForSide(Color color, double angle)
        {
            var d = 1 - (0.8 * Math.Cos(angle * Math.PI / 180));
            return
                new SolidBrush(
                    ColorUtil.CreateColorWithCorrectedLightness(
                        color,
                        -(float)(ColorUtil.BrightnessEnhancementFactor1 * d)));
        }

        /// <summary>
        ///   Creates a brush for outer periphery of the pie slice used for
        ///   gradual shadow.
        /// </summary>
        /// <param name = "color">
        ///   Color used for pie slice rendering.
        /// </param>
        /// <returns>
        ///   <c>Brush</c> object.
        /// </returns>
        protected virtual Brush CreateBrushForPeriphery(Color color)
        {
            var color1 =
                ColorUtil.CreateColorWithCorrectedLightness(
                    color,
                    -ColorUtil.BrightnessEnhancementFactor1 / 2);
            var color2 =
                ColorUtil.CreateColorWithCorrectedLightness(
                    color,
                    -ColorUtil.BrightnessEnhancementFactor1);
            return
                new LinearGradientBrush(
                    BoundingRectangle,
                    Color.Blue,
                    Color.White,
                    LinearGradientMode.Horizontal)
                {
                    InterpolationColors =
                        new ColorBlend
                        {
                            Colors = new[] { color1, color, color2 },
                            Positions = new[] { 0F, 0.1F, 1.0F }
                        }
                };
        }

        /// <summary>
        ///   Draws the outer periphery of the pie slice.
        /// </summary>
        /// <param name = "graphics">
        ///   <c>Graphics</c> object used to draw the surface.
        /// </param>
        /// <param name = "pen">
        ///   <c>Pen</c> used to draw outline.
        /// </param>
        /// <param name = "brush">
        ///   <c>Brush</c> used to fill the quadrilateral.
        /// </param>
        /// <param name = "startAngle">
        ///   Start angle (in degrees) of the periphery section.
        /// </param>
        /// <param name = "endAngle">
        ///   End angle (in degrees) of the periphery section.
        /// </param>
        /// <param name = "pointStart">
        ///   Point representing the start of the periphery.
        /// </param>
        /// <param name = "pointEnd">
        ///   Point representing the end of the periphery.
        /// </param>
        protected void DrawCylinderSurfaceSection(
            Graphics graphics, Pen pen, Brush brush, float startAngle,
            float endAngle, PointF pointStart, PointF pointEnd)
        {
            var path = CreatePathForCylinderSurfaceSection(startAngle, endAngle, pointStart, pointEnd);
            graphics.FillPath(brush, path);
            graphics.DrawPath(pen, path);
        }

        /// <summary>
        ///   Transforms actual angle to angle used for rendering. They are
        ///   different because of perspective.
        /// </summary>
        /// <param name = "angle">
        ///   Actual angle.
        /// </param>
        /// <returns>
        ///   Rendering angle.
        /// </returns>
        protected float TransformAngle(float angle)
        {
            var x = BoundingRectangle.Width * Math.Cos(angle * Math.PI / 180);
            var y = BoundingRectangle.Height * Math.Sin(angle * Math.PI / 180);
            var result = (float)(Math.Atan2(y, x) * 180 / Math.PI);
            if (result < 0)
            {
                return result + 360;
            }

            return result;
        }

        /// <summary>
        ///   Calculates the point on ellipse periphery for angle.
        /// </summary>
        /// <param name = "xCenter">
        ///   x-coordinate of the center of the ellipse.
        /// </param>
        /// <param name = "yCenter">
        ///   y-coordinate of the center of the ellipse.
        /// </param>
        /// <param name = "semiMajor">
        ///   Horizontal semi-axis.
        /// </param>
        /// <param name = "semiMinor">
        ///   Vertical semi-axis.
        /// </param>
        /// <param name = "angleDegrees">
        ///   Angle (in degrees) for which corresponding periphery point has to
        ///   be obtained.
        /// </param>
        /// <returns>
        ///   <c>PointF</c> on the ellipse.
        /// </returns>
        protected PointF PeripheralPoint(
            float xCenter, float yCenter, float semiMajor, float semiMinor,
            float angleDegrees)
        {
            var angleRadians = angleDegrees * Math.PI / 180;
            return new PointF(
                xCenter + (float)(semiMajor * Math.Cos(angleRadians)),
                yCenter + (float)(semiMinor * Math.Sin(angleRadians)));
        }

        /// <summary>
        ///   Initializes pie bounding rectangle, pie height, corners
        ///   coordinates and brushes used for rendering.
        /// </summary>
        /// <param name = "xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name = "yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the pie slice.
        /// </param>
        /// <param name = "widthBoundingRect">
        ///   Width of the rectangle that is used to draw the top surface of
        ///   the pie slice.
        /// </param>
        /// <param name = "heightBoundingRect">
        ///   Height of the rectangle that is used to draw the top surface of
        ///   the pie slice.
        /// </param>
        /// <param name = "sliceHeight">
        ///   Height of the pie slice.
        /// </param>
        private void InitializePieSlice(
            float xBoundingRect, float yBoundingRect, float widthBoundingRect,
            float heightBoundingRect, float sliceHeight)
        {
            // stores bounding rectangle and pie slice height
            BoundingRectangle =
                new RectangleF(xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect);
            SliceHeight = sliceHeight;

            // recalculates start and sweep angle used for rendering
            StartAngle = TransformAngle(_actualStartAngle);
            SweepAngle = _actualSweepAngle;
            if (SweepAngle % 180 != 0F)
            {
                SweepAngle = TransformAngle(_actualStartAngle + _actualSweepAngle) - StartAngle;
            }

            if (SweepAngle < 0)
            {
                SweepAngle += 360;
            }

            // recreates brushes
            CreateSurfaceBrushes(_surfaceColor, _shadowStyle);

            // calculates center and end points on periphery
            var xCenter = xBoundingRect + (widthBoundingRect / 2);
            var yCenter = yBoundingRect + (heightBoundingRect / 2);
            Center = new PointF(xCenter, yCenter);
            CenterBelow = new PointF(xCenter, yCenter + sliceHeight);
            PointStart = PeripheralPoint(
                xCenter, yCenter, widthBoundingRect / 2, heightBoundingRect / 2,
                _actualStartAngle);
            PointStartBelow = new PointF(PointStart.X, PointStart.Y + sliceHeight);
            PointEnd = PeripheralPoint(
                xCenter, yCenter, widthBoundingRect / 2, heightBoundingRect / 2,
                _actualStartAngle + _actualSweepAngle);
            PointEndBelow = new PointF(PointEnd.X, PointEnd.Y + sliceHeight);
            InitializeSides();
        }

        /// <summary>
        ///   Initializes start and end pie slice sides.
        /// </summary>
        private void InitializeSides()
        {
            if (StartAngle > 90 && StartAngle < 270)
            {
                StartSide =
                    new Quadrilateral(
                        Center, PointStart, PointStartBelow, CenterBelow,
                        SweepAngle != 180);
            }
            else
            {
                StartSide = Quadrilateral.Empty;
            }

            if (EndAngle > 270 || EndAngle < 90)
            {
                EndSide = new Quadrilateral(
                    Center, PointEnd, PointEndBelow, CenterBelow,
                    SweepAngle != 180);
            }
            else
            {
                EndSide = Quadrilateral.Empty;
            }
        }

        /// <summary>
        ///   Gets an array of visible periphery bounds.
        /// </summary>
        /// <returns>
        ///   Array of <c>PeripherySurfaceBounds</c> objects.
        /// </returns>
        private IEnumerable<PeripherySurfaceBounds> GetVisiblePeripherySurfaceBounds()
        {
            var peripherySurfaceBounds = new List<PeripherySurfaceBounds>();

            // outer periphery side is visible only when startAngle or endAngle
            // is between 0 and 180 degrees
            if (!(SweepAngle == 0 || (StartAngle >= 180 && StartAngle + SweepAngle <= 360)))
            {
                // draws the periphery from start angle to the end angle or left
                // edge, whichever comes first
                if (StartAngle < 180)
                {
                    var fi1 = StartAngle;
                    var x1 = new PointF(PointStart.X, PointStart.Y);
                    var fi2 = EndAngle;
                    var x2 = new PointF(PointEnd.X, PointEnd.Y);
                    if (StartAngle + SweepAngle > 180)
                    {
                        fi2 = 180;
                        x2.X = BoundingRectangle.X;
                        x2.Y = Center.Y;
                    }

                    peripherySurfaceBounds.Add(new PeripherySurfaceBounds(fi1, fi2, x1, x2));
                }

                // if lateral surface is visible from the right edge
                if (StartAngle + SweepAngle > 360)
                {
                    const float fi1 = 0;
                    var x1 = new PointF(BoundingRectangle.Right, Center.Y);
                    var fi2 = EndAngle;
                    var x2 = new PointF(PointEnd.X, PointEnd.Y);
                    if (fi2 > 180)
                    {
                        fi2 = 180;
                        x2.X = BoundingRectangle.Left;
                        x2.Y = Center.Y;
                    }

                    peripherySurfaceBounds.Add(new PeripherySurfaceBounds(fi1, fi2, x1, x2));
                }
            }

            return peripherySurfaceBounds;
        }

        /// <summary>
        ///   Creates <c>GraphicsPath</c> for cylinder surface section. This
        ///   path consists of two arcs and two vertical lines.
        /// </summary>
        /// <param name = "startAngle">
        ///   Starting angle of the surface.
        /// </param>
        /// <param name = "endAngle">
        ///   Ending angle of the surface.
        /// </param>
        /// <param name = "pointStart">
        ///   Starting point on the cylinder surface.
        /// </param>
        /// <param name = "pointEnd">
        ///   Ending point on the cylinder surface.
        /// </param>
        /// <returns>
        ///   <c>GraphicsPath</c> object representing the cylinder surface.
        /// </returns>
        private GraphicsPath CreatePathForCylinderSurfaceSection(
            float startAngle, float endAngle,
            PointF pointStart, PointF pointEnd)
        {
            var path = new GraphicsPath();
            path.AddArc(BoundingRectangle, startAngle, endAngle - startAngle);
            path.AddLine(pointEnd.X, pointEnd.Y, pointEnd.X, pointEnd.Y + SliceHeight);
            path.AddArc(
                BoundingRectangle.X, BoundingRectangle.Y + SliceHeight, BoundingRectangle.Width,
                BoundingRectangle.Height, endAngle, startAngle - endAngle);
            path.AddLine(pointStart.X, pointStart.Y + SliceHeight, pointStart.X, pointStart.Y);
            return path;
        }

        /// <summary>
        ///   Checks if given point is contained within upper and lower pie
        ///   slice surfaces or within the outer slice brink.
        /// </summary>
        /// <param name = "point">
        ///   <c>PointF</c> structure to check for.
        /// </param>
        /// <param name = "point1">
        ///   Starting point on the periphery.
        /// </param>
        /// <param name = "point2">
        ///   Ending point on the periphery.
        /// </param>
        /// <returns>
        ///   <c>true</c> if point given is contained.
        /// </returns>
        private bool CylinderSurfaceSectionContainsPoint(PointF point, PointF point1, PointF point2)
        {
            if (SliceHeight > 0)
            {
                return Quadrilateral
                    .Contains(
                        point,
                        new[]
                        {
                            point1,
                            new PointF(point1.X, point1.Y + SliceHeight),
                            new PointF(point2.X, point2.Y + SliceHeight),
                            point2
                        });
            }

            return false;
        }

        /// <summary>
        ///   Checks if point given is contained within the pie slice.
        /// </summary>
        /// <param name = "point">
        ///   <c>PointF</c> to check for.
        /// </param>
        /// <param name = "xBoundingRectangle">
        ///   x-coordinate of the rectangle that bounds the ellipse from which
        ///   slice is cut.
        /// </param>
        /// <param name = "yBoundingRectangle">
        ///   y-coordinate of the rectangle that bounds the ellipse from which
        ///   slice is cut.
        /// </param>
        /// <param name = "widthBoundingRectangle">
        ///   Width of the rectangle that bounds the ellipse from which
        ///   slice is cut.
        /// </param>
        /// <param name = "heightBoundingRectangle">
        ///   Height of the rectangle that bounds the ellipse from which
        ///   slice is cut.
        /// </param>
        /// <param name = "startAngle">
        ///   Start angle of the slice.
        /// </param>
        /// <param name = "sweepAngle">
        ///   Sweep angle of the slice.
        /// </param>
        /// <returns>
        ///   <c>true</c> if point is contained within the slice.
        /// </returns>
        private static bool PieSliceContainsPoint(
            PointF point, float xBoundingRectangle, float yBoundingRectangle,
            float widthBoundingRectangle, float heightBoundingRectangle,
            float startAngle,
            float sweepAngle)
        {
            double a = widthBoundingRectangle / 2;
            double b = heightBoundingRectangle / 2;
            var x = point.X - xBoundingRectangle - a;
            var y = point.Y - yBoundingRectangle - b;
            var angle = Math.Atan2(y, x);
            if (angle < 0)
            {
                angle += 2 * Math.PI;
            }

            var angleDegrees = angle * 180 / Math.PI;

            // point is inside the pie slice only if between start and end angle
            if ((angleDegrees >= startAngle && angleDegrees <= (startAngle + sweepAngle)) ||
                ((startAngle + sweepAngle > 360) && ((angleDegrees + 360) <= (startAngle + sweepAngle))))
            {
                // distance of the point from the ellipse centre
                var r = Math.Sqrt((y * y) + (x * x));
                var a2 = a * a;
                var b2 = b * b;
                var cosFi = Math.Cos(angle);
                var sinFi = Math.Sin(angle);

                // distance of the ellipse perimeter point
                var ellipseRadius = (b * a) / Math.Sqrt((b2 * cosFi * cosFi) + (a2 * sinFi * sinFi));
                return ellipseRadius > r;
            }

            return false;
        }

        #region Nested type: PeripherySurfaceBounds

        /// <summary>
        ///   Internal structure used to store periphery bounds data.
        /// </summary>
        private readonly struct PeripherySurfaceBounds
        {
            public PeripherySurfaceBounds(float startAngle, float endAngle, PointF startPoint, PointF endPoint)
                : this()
            {
                StartAngle = startAngle;
                EndAngle = endAngle;
                StartPoint = startPoint;
                EndPoint = endPoint;
            }

            public float StartAngle { get; }

            public float EndAngle { get; }

            public PointF StartPoint { get; }

            public PointF EndPoint { get; }
        }

        #endregion
    }
}