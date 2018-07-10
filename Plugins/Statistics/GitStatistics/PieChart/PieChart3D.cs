using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace GitStatistics.PieChart
{
    /// <summary>
    ///   Object representing a pie chart.
    /// </summary>
    public class PieChart3D : IDisposable
    {
        /// <summary>
        ///   Initial angle from which chart is drawn.
        /// </summary>
        protected float InitialAngle;

        /// <summary>
        ///   Array of ordered pie slices constituting the chart, starting from
        ///   270 degrees axis.
        /// </summary>
        protected PieSlice[] PieSlices;

        /// <summary>
        ///   Collection of reordered pie slices mapped to original order.
        /// </summary>
        protected List<int> PieSlicesMapping { get; } = new List<int>();

        /// <summary>
        ///   Array of colors used for rendering.
        /// </summary>
        protected Color[] SliceColors =
            {
                    Color.Red,
                    Color.Green,
                    Color.Blue,
                    Color.Yellow,
                    Color.Purple,
                    Color.Olive,
                    Color.Navy,
                    Color.Aqua,
                    Color.Lime,
                    Color.Maroon,
                    Color.Teal,
                    Color.Fuchsia
                };

        /// <summary>
        ///   Array of relative displacements from the common center.
        /// </summary>
        protected float[] SliceRelativeDisplacements = { 0F };

        /// <summary>
        ///   Slice relative height.
        /// </summary>
        protected float SliceRelativeHeight;

        /// <summary>
        ///   Array of values to be presented by the chart.
        /// </summary>
        protected decimal[] Values = { };

        /// <summary>
        ///   Initializes an empty instance of <c>PieChart3D</c>.
        /// </summary>
        protected PieChart3D()
        {
            HighlightedIndex = -1;
            FitToBoundingRectangle = true;
            ShadowStyle = ShadowStyle.NoShadow;
            EdgeLineWidth = 1F;
            EdgeColorType = EdgeColorType.SystemColor;
        }

        /// <summary>
        ///   Initializes an instance of a flat <c>PieChart3D</c> with
        ///   specified bounds, values to chart and relative thickness.
        /// </summary>
        /// <param name = "xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle that
        ///   bounds the chart.
        /// </param>
        /// <param name = "yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle that
        ///   bounds the chart.
        /// </param>
        /// <param name = "widthBoundingRect">
        ///   Width of the rectangle that bounds the chart.
        /// </param>
        /// <param name = "heightBoundingRect">
        ///   Height of the rectangle that bounds the chart.
        /// </param>
        /// <param name = "values">
        ///   An array of <c>decimal</c> values to chart.
        /// </param>
        public PieChart3D(float xBoundingRect, float yBoundingRect, float widthBoundingRect, float heightBoundingRect,
                          decimal[] values) : this()
        {
            X = xBoundingRect;
            Y = yBoundingRect;
            Width = widthBoundingRect;
            Height = heightBoundingRect;
            SetValues(values);
        }

        /// <summary>
        ///   Initializes an instance of <c>PieChart3D</c> with specified
        ///   bounds, values to chart and relative thickness.
        /// </summary>
        /// <param name = "xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle bounding
        ///   the chart.
        /// </param>
        /// <param name = "yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle bounding
        ///   the chart.
        /// </param>
        /// <param name = "widthBoundingRect">
        ///   Width of the rectangle bounding the chart.
        /// </param>
        /// <param name = "heightBoundingRect">
        ///   Height of the rectangle bounding the chart.
        /// </param>
        /// <param name = "values">
        ///   An array of <c>decimal</c> values to chart.
        /// </param>
        /// <param name = "sliceRelativeHeight">
        ///   Thickness of the pie slice to chart relative to the height of the
        ///   bounding rectangle.
        /// </param>
        public PieChart3D(float xBoundingRect, float yBoundingRect, float widthBoundingRect, float heightBoundingRect,
                          decimal[] values, float sliceRelativeHeight)
            : this(xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect, values)
        {
            SliceRelativeHeight = sliceRelativeHeight;
        }

        /// <summary>
        ///   Initializes a new instance of <c>PieChart3D</c> with given bounds,
        ///   array of values and pie slice thickness.
        /// </summary>
        /// <param name = "boundingRectangle">
        ///   Bounding rectangle.
        /// </param>
        /// <param name = "values">
        ///   Array of values to initialize with.
        /// </param>
        public PieChart3D(RectangleF boundingRectangle, decimal[] values, float sliceRelativeHeight)
            : this(
                boundingRectangle.X, boundingRectangle.Y, boundingRectangle.Width, boundingRectangle.Y, values,
                sliceRelativeHeight)
        {
        }

        /// <summary>
        ///   Initializes a new instance of <c>PieChart3D</c> with given bounds,
        ///   array of values and relative pie slice height.
        /// </summary>
        /// <param name = "xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle bounding
        ///   the chart.
        /// </param>
        /// <param name = "yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle bounding
        ///   the chart.
        /// </param>
        /// <param name = "widthBoundingRect">
        ///   Width of the rectangle bounding the chart.
        /// </param>
        /// <param name = "heightBoundingRect">
        ///   Height of the rectangle bounding the chart.
        /// </param>
        /// <param name = "values">
        ///   An array of <c>decimal</c> values to chart.
        /// </param>
        /// <param name = "sliceColors">
        ///   An array of colors used to render slices.
        /// </param>
        /// <param name = "sliceRelativeHeight">
        ///   Thickness of the slice to chart relative to the height of the
        ///   bounding rectangle.
        /// </param>
        public PieChart3D(float xBoundingRect, float yBoundingRect, float widthBoundingRect, float heightBoundingRect,
                          decimal[] values, Color[] sliceColors, float sliceRelativeHeight)
            : this(xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect, values, sliceRelativeHeight)
        {
            SliceColors = sliceColors;
        }

        /// <summary>
        ///   Initializes a new instance of <c>PieChart3D</c> with given bounds,
        ///   array of values and corresponding colors.
        /// </summary>
        /// <param name = "boundingRectangle">
        ///   Bounding rectangle.
        /// </param>
        /// <param name = "values">
        ///   Array of values to chart.
        /// </param>
        /// <param name = "sliceColors">
        ///   Colors used for rendering individual slices.
        /// </param>
        /// <param name = "sliceRelativeHeight">
        ///   Pie slice relative height.
        /// </param>
        public PieChart3D(RectangleF boundingRectangle, decimal[] values, Color[] sliceColors, float sliceRelativeHeight)
            : this(boundingRectangle, values, sliceRelativeHeight)
        {
            SliceColors = sliceColors;
        }

        /// <summary>
        ///   Sets slice edge color mode. If set to <c>PenColor</c> (default),
        ///   then value set by <c>EdgeColor</c> property is used.
        /// </summary>
        public EdgeColorType EdgeColorType { protected get; set; }

        /// <summary>
        ///   Sets slice edge line width. If not set, default value is 1.
        /// </summary>
        public float EdgeLineWidth { protected get; set; }

        /// <summary>
        ///   Gets or sets the width of the bounding rectangle.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        ///   Gets or sets the height of the bounding rectangle.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        ///   Gets the y-coordinate of the bounding rectangle top edge.
        /// </summary>
        public float Top => Y;

        /// <summary>
        ///   Gets the y-coordinate of the bounding rectangle bottom edge.
        /// </summary>
        public float Bottom => Y + Height;

        /// <summary>
        ///   Gets the x-coordinate of the bounding rectangle left edge.
        /// </summary>
        public float Left => X;

        /// <summary>
        ///   Gets the x-coordinate of the bounding rectangle right edge.
        /// </summary>
        public float Right => X + Width;

        /// <summary>
        ///   Gets or sets the x-coordinate of the upper-left corner of the
        ///   bounding rectangle.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        ///   Gets or sets the y-coordinate of the upper-left corner of the
        ///   bounding rectangle.
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        ///   Sets the shadowing style used.
        /// </summary>
        public ShadowStyle ShadowStyle { protected get; set; }

        /// <summary>
        ///   Sets the flag that controls if chart is fit to bounding rectangle
        ///   exactly.
        /// </summary>
        public bool FitToBoundingRectangle { protected get; set; }

        public int HighlightedIndex { protected get; set; }

        /// <summary>
        ///   Finds the largest displacement.
        /// </summary>
        protected float LargestDisplacement
        {
            get
            {
                var value = 0F;
                for (var i = 0; i < SliceRelativeDisplacements.Length && i < Values.Length; ++i)
                {
                    if (SliceRelativeDisplacements[i] > value)
                    {
                        value = SliceRelativeDisplacements[i];
                    }
                }

                return value;
            }
        }

        /// <summary>
        ///   Gets the top ellipse size.
        /// </summary>
        protected SizeF TopEllipseSize
        {
            get
            {
                var factor = 1 + LargestDisplacement;
                var widthTopEllipse = Width / factor;
                var heightTopEllipse = Height / factor * (1 - SliceRelativeHeight);
                return new SizeF(widthTopEllipse, heightTopEllipse);
            }
        }

        /// <summary>
        ///   Gets the ellipse defined by largest displacement.
        /// </summary>
        protected SizeF LargestDisplacementEllipseSize
        {
            get
            {
                var factor = LargestDisplacement;
                var widthDisplacementEllipse = TopEllipseSize.Width * factor;
                var heightDisplacementEllipse = TopEllipseSize.Height * factor;
                return new SizeF(widthDisplacementEllipse, heightDisplacementEllipse);
            }
        }

        /// <summary>
        ///   Calculates the pie height.
        /// </summary>
        protected float PieHeight => Height / (1 + LargestDisplacement) * SliceRelativeHeight;

        #region IDisposable Members

        /// <summary>
        ///   Disposes of all pie slices.
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var slice in PieSlices)
            {
                slice.Dispose();
            }
        }

        #endregion

        /// <summary>
        ///   Sets values to be displayed on the chart.
        /// </summary>
        public void SetValues(decimal[] value)
        {
            Debug.Assert(value != null && value.Length > 0, "value != null && value.Length > 0");
            Values = value;
        }

        /// <summary>
        ///   Sets colors used for individual pie slices.
        /// </summary>
        public void SetColors(Color[] value)
        {
            SliceColors = value;
        }

        /// <summary>
        ///   Sets slice height, relative to the top ellipse semi-axis. Must be
        ///   less than or equal to 0.5.
        /// </summary>
        public void SetSliceRelativeHeight(float value)
        {
            Debug.Assert(value <= 0.5F, "value <= 0.5F");
            SliceRelativeHeight = value;
        }

        /// <summary>
        ///   Sets the slice displacement relative to the ellipse semi-axis.
        ///   Must be less than 1.
        /// </summary>
        public void SetSliceRelativeDisplacement(float value)
        {
            Debug.Assert(IsDisplacementValid(value), "IsDisplacementValid(value)");
            SliceRelativeDisplacements = new[] { value };
        }

        /// <summary>
        ///   Sets the slice displacement relative to the ellipse semi-axis.
        ///   Must be less than 1.
        /// </summary>
        public void SetSliceRelativeDisplacements(float[] value)
        {
            SliceRelativeDisplacements = value;
            Debug.Assert(AreDisplacementsValid(value), "AreDisplacementsValid(value)");
        }

        /// <summary>
        ///   Gets or sets the size of the entire pie chart.
        /// </summary>
        public void SetChartSize(SizeF value)
        {
            Width = value.Width;
            Height = value.Height;
        }

        /// <summary>
        ///   Sets the initial angle from which pies are placed.
        /// </summary>
        public void SetInitialAngle(float value)
        {
            InitialAngle = value % 360;
            if (InitialAngle < 0)
            {
                InitialAngle += 360;
            }
        }

        /// <summary>
        ///   Draws the chart.
        /// </summary>
        /// <param name = "graphics">
        ///   <c>Graphics</c> object used for drawing.
        /// </param>
        public void Draw(Graphics graphics)
        {
            Debug.Assert(Values != null && Values.Length > 0, "Values != null && Values.Length > 0");
            InitializePieSlices();
            if (FitToBoundingRectangle)
            {
                var newBoundingRectangle = GetFittingRectangle();
                ReadjustSlices(newBoundingRectangle);
            }

            if (SliceRelativeHeight > 0F)
            {
                DrawSliceSides(graphics);
            }

            DrawTops(graphics);
        }

        /// <summary>
        ///   Searches the chart to find the index of the pie slice which
        ///   contains point given. Search order goes in the direction opposite
        ///   to drawing order.
        /// </summary>
        /// <param name = "point">
        ///   <c>PointF</c> point for which pie slice is searched for.
        /// </param>
        /// <returns>
        ///   Index of the corresponding pie slice, or -1 if none is found.
        /// </returns>
        public int FindPieSliceUnderPoint(PointF point)
        {
            // first check tops
            for (var i = 0; i < PieSlices.Length; ++i)
            {
                var slice = PieSlices[i];
                if (slice.PieSliceContainsPoint(point))
                {
                    return PieSlicesMapping[i];
                }
            }

            // split the backmost (at 270 degrees) pie slice
            var pieSlices = new List<PieSlice>(PieSlices);
            var splitSlices = PieSlices[0].Split(270F);
            pieSlices[0] = splitSlices[0];
            if (splitSlices[1].SweepAngle > 0F)
            {
                pieSlices.Add(splitSlices[1]);
            }

            var indexFound = -1;

            // if not found yet, then check for peripheries
            var incrementIndex = 0;
            var decrementIndex = pieSlices.Count - 1;
            while (incrementIndex <= decrementIndex)
            {
                var sliceLeft = pieSlices[decrementIndex];
                var angle1 = 270 - sliceLeft.StartAngle;
                var sliceRight = pieSlices[incrementIndex];
                var angle2 = (sliceRight.EndAngle + 90) % 360;
                Debug.Assert(angle2 >= 0, "angle2 >= 0");
                if (angle2 < angle1)
                {
                    if (sliceRight.PeripheryContainsPoint(point))
                    {
                        indexFound = incrementIndex;
                    }

                    ++incrementIndex;
                }
                else
                {
                    if (sliceLeft.PeripheryContainsPoint(point))
                    {
                        indexFound = decrementIndex;
                    }

                    --decrementIndex;
                }
            }

            // check for start/stop sides, starting from the foremost
            if (indexFound < 0)
            {
                var foremostPieIndex = GetForemostPieSlice(pieSlices);

                // check for start sides from the foremost slice to the left
                // side
                var i = foremostPieIndex;
                while (i < pieSlices.Count)
                {
                    var sliceLeft = pieSlices[i];
                    if (sliceLeft.StartSideContainsPoint(point))
                    {
                        indexFound = i;
                        break;
                    }

                    ++i;
                }

                // if not found yet, check end sides from the foremost to the right
                // side
                if (indexFound < 0)
                {
                    i = foremostPieIndex;
                    while (i >= 0)
                    {
                        var sliceLeft = pieSlices[i];
                        if (sliceLeft.EndSideContainsPoint(point))
                        {
                            indexFound = i;
                            break;
                        }

                        --i;
                    }
                }
            }

            // finally search for bottom sides
            if (indexFound < 0)
            {
                for (var i = 0; i < PieSlices.Length; ++i)
                {
                    var slice = PieSlices[i];
                    if (slice.BottomSurfaceSectionContainsPoint(point))
                    {
                        return PieSlicesMapping[i];
                    }
                }
            }

            if (indexFound > -1)
            {
                indexFound %= PieSlicesMapping.Count;
                return PieSlicesMapping[indexFound];
            }

            return -1;
        }

        /// <summary>
        ///   Return the index of the foremost pie slice i.e. the one crossing
        ///   90 degrees boundary.
        /// </summary>
        /// <param name = "pieSlices">
        ///   Array of <c>PieSlice</c> objects to examine.
        /// </param>
        /// <returns>
        ///   Index of the foremost pie slice.
        /// </returns>
        private static int GetForemostPieSlice(IReadOnlyList<PieSlice> pieSlices)
        {
            Debug.Assert(pieSlices != null && pieSlices.Count > 0, "pieSlices != null && pieSlices.Count > 0");
            for (var i = 0; i < pieSlices.Count; ++i)
            {
                var pieSlice = pieSlices[i];
                if (((pieSlice.StartAngle <= 90) && ((pieSlice.StartAngle + pieSlice.SweepAngle) >= 90)) ||
                    ((pieSlice.StartAngle + pieSlice.SweepAngle > 360) && (pieSlice.StartAngle <= 450) &&
                     (pieSlice.StartAngle + pieSlice.SweepAngle) >= 450))
                {
                    return i;
                }
            }

            Debug.Assert(false, "Foremost pie slice not found");
            return -1;
        }

        /// <summary>
        ///   Finds the smallest rectangle int which chart fits entirely.
        /// </summary>
        /// <returns>
        ///   <c>RectangleF</c> into which all member slices fit.
        /// </returns>
        protected RectangleF GetFittingRectangle()
        {
            var boundingRectangle = PieSlices[0].GetFittingRectangle();
            for (var i = 1; i < PieSlices.Length; ++i)
            {
                boundingRectangle = RectangleF.Union(boundingRectangle, PieSlices[i].GetFittingRectangle());
            }

            return boundingRectangle;
        }

        /// <summary>
        ///   Readjusts each slice for new bounding rectangle.
        /// </summary>
        /// <param name = "newBoundingRectangle">
        ///   <c>RectangleF</c> representing new boundary.
        /// </param>
        protected void ReadjustSlices(RectangleF newBoundingRectangle)
        {
            var xResizeFactor = Width / newBoundingRectangle.Width;
            var yResizeFactor = Height / newBoundingRectangle.Height;
            var xOffset = newBoundingRectangle.X - X;
            var yOffset = newBoundingRectangle.Y - Y;
            foreach (var slice in PieSlices)
            {
                var x = slice.BoundingRectangle.X - xOffset;
                var y = slice.BoundingRectangle.Y - yOffset;
                var width = slice.BoundingRectangle.Width * xResizeFactor;
                var height = slice.BoundingRectangle.Height * yResizeFactor;
                var sliceHeight = slice.SliceHeight * yResizeFactor;
                slice.Readjust(x, y, width, height, sliceHeight);
            }
        }

        /// <summary>
        ///   Initializes pies.
        /// </summary>
        /// Creates a list of pies, starting with the pie that is crossing the
        /// 270 degrees boundary, i.e. "backmost" pie that always has to be
        /// drawn first to ensure correct surface overlapping.
        protected virtual void InitializePieSlices()
        {
            // calculates the sum of values required to evaluate sweep angles
            // for individual pies
            double sum = Values.Sum(itemValue => (double)itemValue);

            // some values and indices that will be used in the loop
            var topEllipseSize = TopEllipseSize;
            var largestDisplacementEllipseSize = LargestDisplacementEllipseSize;
            var maxDisplacementIndex = SliceRelativeDisplacements.Length - 1;
            var largestDisplacement = LargestDisplacement;
            var listPieSlices = new List<PieSlice>();
            PieSlicesMapping.Clear();
            var colorIndex = 0;
            var backPieIndex = -1;
            var displacementIndex = 0;
            double startAngle = InitialAngle;
            for (var i = 0; i < Values.Length; ++i)
            {
                var itemValue = Values[i];

                var sweepAngle = sum == 0 ? 0 : (double)itemValue / sum * 360;

                // displacement from the center of the ellipse
                var xDisplacement = SliceRelativeDisplacements[displacementIndex];
                var yDisplacement = SliceRelativeDisplacements[displacementIndex];
                if (xDisplacement > 0F)
                {
                    Debug.Assert(largestDisplacement > 0F, "largestDisplacement > 0F");
                    var pieDisplacement = GetSliceDisplacement((float)(startAngle + (sweepAngle / 2)),
                                                               SliceRelativeDisplacements[displacementIndex]);
                    xDisplacement = pieDisplacement.Width;
                    yDisplacement = pieDisplacement.Height;
                }

                PieSlice slice;
                if (i == HighlightedIndex)
                {
                    slice =
                        CreatePieSliceHighlighted(
                            X + (largestDisplacementEllipseSize.Width / 2) + xDisplacement,
                            Y + (largestDisplacementEllipseSize.Height / 2) + yDisplacement,
                            topEllipseSize.Width, topEllipseSize.Height, PieHeight, (float)(startAngle % 360),
                            (float)sweepAngle, SliceColors[colorIndex], ShadowStyle, EdgeColorType,
                            EdgeLineWidth);
                }
                else
                {
                    slice = CreatePieSlice(X + (largestDisplacementEllipseSize.Width / 2) + xDisplacement,
                                           Y + (largestDisplacementEllipseSize.Height / 2) + yDisplacement,
                                           topEllipseSize.Width, topEllipseSize.Height, PieHeight,
                                           (float)(startAngle % 360), (float)sweepAngle, SliceColors[colorIndex],
                                           ShadowStyle, EdgeColorType, EdgeLineWidth);
                }

                // the backmost pie is inserted to the front of the list for correct drawing
                if (backPieIndex > -1 || ((startAngle <= 270) && (startAngle + sweepAngle > 270)) ||
                    ((startAngle >= 270) && (startAngle + sweepAngle > 630)))
                {
                    ++backPieIndex;
                    listPieSlices.Insert(backPieIndex, slice);
                    PieSlicesMapping.Insert(backPieIndex, i);
                }
                else
                {
                    listPieSlices.Add(slice);
                    PieSlicesMapping.Add(i);
                }

                // increment displacementIndex only if there are more displacements available
                if (displacementIndex < maxDisplacementIndex)
                {
                    ++displacementIndex;
                }

                ++colorIndex;

                // if all colors have been exhausted, reset color index
                if (colorIndex >= SliceColors.Length)
                {
                    colorIndex = 0;
                }

                // prepare for the next pie slice
                startAngle += sweepAngle;
                if (startAngle > 360)
                {
                    startAngle -= 360;
                }
            }

            PieSlices = listPieSlices.ToArray();
        }

        /// <summary>
        ///   Creates a <c>PieSlice</c> object.
        /// </summary>
        /// <param name = "boundingRectLeft">
        ///   x-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the slice.
        /// </param>
        /// <param name = "boundingRectTop">
        ///   y-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the slice.
        /// </param>
        /// <param name = "boundingRectWidth">
        ///   Width of the rectangle that is used to draw the top surface of
        ///   the slice.
        /// </param>
        /// <param name = "boundingRectHeight">
        ///   Height of the rectangle that is used to draw the top surface of
        ///   the slice.
        /// </param>
        /// <param name = "sliceHeight">
        ///   Slice height.
        /// </param>
        /// <param name = "startAngle">
        ///   Starting angle.
        /// </param>
        /// <param name = "sweepAngle">
        ///   Sweep angle.
        /// </param>
        /// <param name = "color">
        ///   Color used for slice rendering.
        /// </param>
        /// <param name = "shadowStyle">
        ///   Shadow style used for slice rendering.
        /// </param>
        /// <param name = "edgeColorType">
        ///   Edge lines color type.
        /// </param>
        /// <param name = "edgeLineWidth">
        ///   Edge lines width.
        /// </param>
        /// <returns>
        ///   <c>PieSlice</c> object with given values.
        /// </returns>
        protected virtual PieSlice CreatePieSlice(float boundingRectLeft, float boundingRectTop, float boundingRectWidth,
                                                  float boundingRectHeight, float sliceHeight, float startAngle,
                                                  float sweepAngle, Color color, ShadowStyle shadowStyle,
                                                  EdgeColorType edgeColorType, float edgeLineWidth)
        {
            return new PieSlice(boundingRectLeft, boundingRectTop, boundingRectWidth, boundingRectHeight, sliceHeight,
                                startAngle % 360, sweepAngle, color, shadowStyle, edgeColorType, edgeLineWidth);
        }

        /// <summary>
        ///   Creates highlighted <c>PieSlice</c> object.
        /// </summary>
        /// <param name = "boundingRectLeft">
        ///   x-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the slice.
        /// </param>
        /// <param name = "boundingRectTop">
        ///   y-coordinate of the upper-left corner of the rectangle that is
        ///   used to draw the top surface of the slice.
        /// </param>
        /// <param name = "boundingRectWidth">
        ///   Width of the rectangle that is used to draw the top surface of
        ///   the slice.
        /// </param>
        /// <param name = "boundingRectHeight">
        ///   Height of the rectangle that is used to draw the top surface of
        ///   the slice.
        /// </param>
        /// <param name = "sliceHeight">
        ///   Slice height.
        /// </param>
        /// <param name = "startAngle">
        ///   Starting angle.
        /// </param>
        /// <param name = "sweepAngle">
        ///   Sweep angle.
        /// </param>
        /// <param name = "color">
        ///   Color used for slice rendering.
        /// </param>
        /// <param name = "shadowStyle">
        ///   Shadow style used for slice rendering.
        /// </param>
        /// <param name = "edgeColorType">
        ///   Edge lines color type.
        /// </param>
        /// <param name = "edgeLineWidth">
        ///   Edge lines width.
        /// </param>
        /// <returns>
        ///   <c>PieSlice</c> object with given values.
        /// </returns>
        protected virtual PieSlice CreatePieSliceHighlighted(float boundingRectLeft, float boundingRectTop,
                                                             float boundingRectWidth, float boundingRectHeight,
                                                             float sliceHeight, float startAngle, float sweepAngle,
                                                             Color color, ShadowStyle shadowStyle,
                                                             EdgeColorType edgeColorType, float edgeLineWidth)
        {
            var highLightedColor = ColorUtil.CreateColorWithCorrectedLightness(color,
                                                                               ColorUtil.BrightnessEnhancementFactor1);
            return new PieSlice(boundingRectLeft, boundingRectTop, boundingRectWidth, boundingRectHeight, sliceHeight,
                                startAngle % 360, sweepAngle, highLightedColor, shadowStyle, edgeColorType,
                                edgeLineWidth);
        }

        /// <summary>
        ///   Calculates the displacement for given angle.
        /// </summary>
        /// <param name = "angle">
        ///   Angle (in degrees).
        /// </param>
        /// <param name = "displacementFactor">
        ///   Displacement factor.
        /// </param>
        /// <returns>
        ///   <c>SizeF</c> representing displacement.
        /// </returns>
        protected SizeF GetSliceDisplacement(float angle, float displacementFactor)
        {
            Debug.Assert(displacementFactor > 0F && displacementFactor <= 1F, "displacementFactor > 0F && displacementFactor <= 1F");
            if (displacementFactor == 0F)
            {
                return SizeF.Empty;
            }

            var xDisplacement = (float)(TopEllipseSize.Width * displacementFactor / 2 * Math.Cos(angle * Math.PI / 180));
            var yDisplacement = (float)(TopEllipseSize.Height * displacementFactor / 2 * Math.Sin(angle * Math.PI / 180));
            return new SizeF(xDisplacement, yDisplacement);
        }

        /// <summary>
        ///   Draws outer peripheries of all slices.
        /// </summary>
        /// <param name = "graphics">
        ///   <c>Graphics</c> used for drawing.
        /// </param>
        protected void DrawSliceSides(Graphics graphics)
        {
            var pieSlices = new List<PieSlice>(PieSlices);

            // if the first slice spreads across 180 and 360 degrees boundaries it
            // will appear on both left and right edge so its periphery has to be
            // drawn twice
            var splitSlices = PieSlices[0].Split(270F);
            pieSlices[0] = splitSlices[0];
            if (splitSlices[1].SweepAngle > 0F)
            {
                pieSlices.Add(splitSlices[1]);
            }

            // finds the leftmost slice that is crossing 180 degrees boundary
            var decrementIndex = pieSlices.Count - 1;
            for (var i = decrementIndex; i > 0; --i)
            {
                if (pieSlices[i].StartAngle >= 180)
                {
                    continue;
                }

                decrementIndex = i;
                break;
            }

            // finds the rightmost slice that is crossing 0 degrees boundary
            var incrementIndex = decrementIndex;
            for (var i = 0; i < pieSlices.Count - 1; ++i)
            {
                var slice = pieSlices[i];
                if ((slice.StartAngle + slice.SweepAngle <= 360) && (slice.EndAngle >= 180 || slice.EndAngle <= 0))
                {
                    continue;
                }

                incrementIndex = i;
                break;
            }

            // draws visible start sides for slices from the backmost (at 270 degrees)
            // to the leftmost one
            for (var i = pieSlices.Count - 1; i > decrementIndex; --i)
            {
                var slice = pieSlices[i];
                slice.DrawVisibleStartSide(graphics);
            }

            // draws visible end sides for slices from the backmost (at 270 degrees)
            // to the rightmost one
            for (var i = 0; i < incrementIndex; ++i)
            {
                var slice = pieSlices[i];
                slice.DrawVisibleEndSide(graphics);
            }

            // draws from leftmost and rightmost alternatively, drawing the slice
            // with larger offset from 90 degrees first
            while (incrementIndex < decrementIndex)
            {
                var sliceLeft = pieSlices[decrementIndex];
                var angle1 = 270 - sliceLeft.StartAngle;
                var sliceRight = pieSlices[incrementIndex];
                var angle2 = (sliceRight.EndAngle + 90) % 360;
                Debug.Assert(angle2 >= 0, "angle2 >= 0");
                if (angle2 < angle1)
                {
                    sliceRight.DrawVisibleEndSide(graphics);
                    sliceRight.DrawVisiblePeriphery(graphics);
                    ++incrementIndex;
                }
                else
                {
                    sliceLeft.DrawVisibleStartSide(graphics);
                    sliceLeft.DrawVisiblePeriphery(graphics);
                    --decrementIndex;
                }
            }

            // for the foremost slice only periphery has to be drawn
            pieSlices[incrementIndex].DrawVisiblePeriphery(graphics);
        }

        /// <summary>
        ///   Draws top sides of all pie slices.
        /// </summary>
        /// <param name = "graphics">
        ///   <c>Graphics</c> used for drawing.
        /// </param>
        protected void DrawTops(Graphics graphics)
        {
            foreach (var slice in PieSlices)
            {
                slice.DrawTop(graphics);
            }
        }

        /// <summary>
        ///   Helper function used in assertions. Checks the validity of
        ///   slice displacements.
        /// </summary>
        /// <param name = "displacements">
        ///   Array of displacements to check.
        /// </param>
        /// <returns>
        ///   <c>true</c> if all displacements have a valid value; otherwise
        ///   <c>false</c>.
        /// </returns>
        private static bool AreDisplacementsValid(IEnumerable<float> displacements)
        {
            return displacements.All(IsDisplacementValid);
        }

        /// <summary>
        ///   Helper function used in assertions. Checks the validity of
        ///   a slice displacement.
        /// </summary>
        /// <param name = "value">
        ///   Displacement value to check.
        /// </param>
        /// <returns>
        ///   <c>true</c> if displacement has a valid value; otherwise
        ///   <c>false</c>.
        /// </returns>
        private static bool IsDisplacementValid(float value)
        {
            return value >= 0F && value <= 1F;
        }
    }
}