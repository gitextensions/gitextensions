using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;

namespace System.Drawing.PieChart {

    /// <summary>
	///   Object representing a pie chart.
	/// </summary>
	public class PieChart3D	: IDisposable {

        /// <summary>
        ///   Initializes an empty instance of <c>PieChart3D</c>.
        /// </summary>
        protected PieChart3D() {
		}

        /// <summary>
        ///   Initializes an instance of a flat <c>PieChart3D</c> with 
        ///   specified bounds, values to chart and relative thickness.
        /// </summary>
        /// <param name="xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle that 
        ///   bounds the chart.
        /// </param>
        /// <param name="yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle that 
        ///   bounds the chart.
        /// </param>
        /// <param name="widthBoundingRect">
        ///   Width of the rectangle that bounds the chart.
        /// </param>
        /// <param name="heightBoundingRect">
        ///   Height of the rectangle that bounds the chart.
        /// </param>
        /// <param name="values">
        ///   An array of <c>decimal</c> values to chart.
        /// </param>
        public PieChart3D(float xBoundingRect, float yBoundingRect, float widthBoundingRect, float heightBoundingRect, decimal[] values) : this() {
            m_xBoundingRect = xBoundingRect;
            m_yBoundingRect = yBoundingRect;
            m_widthBoundingRect = widthBoundingRect;
            m_heightBoundingRect = heightBoundingRect;
            Values = values;
        }
            
        /// <summary>
        ///   Initializes an instance of <c>PieChart3D</c> with specified 
        ///   bounds, values to chart and relative thickness.
        /// </summary>
        /// <param name="xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle bounding 
        ///   the chart.
        /// </param>
        /// <param name="yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle bounding
        ///   the chart.
        /// </param>
        /// <param name="widthBoundingRect">
        ///   Width of the rectangle bounding the chart.
        /// </param>
        /// <param name="heightBoundingRect">
        ///   Height of the rectangle bounding the chart.
        /// </param>
        /// <param name="values">
        ///   An array of <c>decimal</c> values to chart.
        /// </param>
        /// <param name="sliceRelativeHeight">
        ///   Thickness of the pie slice to chart relative to the height of the
        ///   bounding rectangle.
        /// </param>
        public PieChart3D(float xBoundingRect, float yBoundingRect, float widthBoundingRect, float heightBoundingRect, decimal[] values, float sliceRelativeHeight) 
            : this(xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect, values) {
            m_sliceRelativeHeight = sliceRelativeHeight;
        }

        /// <summary>
        ///   Initializes a new instance of <c>PieChart3D</c> with given bounds, 
        ///   array of values and pie slice thickness.
        /// </summary>
        /// <param name="boundingRectangle">
        ///   Bounding rectangle.
        /// </param>
        /// <param name="values">
        ///   Array of values to initialize with.
        /// </param>
        /// <param name="sliceRelativeHeight"></param>
        public PieChart3D(RectangleF boundingRectangle, decimal[] values, float sliceRelativeHeight)
            : this(boundingRectangle.X, boundingRectangle.Y, boundingRectangle.Width, boundingRectangle.Y, values, sliceRelativeHeight) {
        }

        /// <summary>
        ///   Initializes a new instance of <c>PieChart3D</c> with given bounds,
        ///   array of values and relative pie slice height.
        /// </summary>
        /// <param name="xBoundingRect">
        ///   x-coordinate of the upper-left corner of the rectangle bounding 
        ///   the chart.
        /// </param>
        /// <param name="yBoundingRect">
        ///   y-coordinate of the upper-left corner of the rectangle bounding
        ///   the chart.
        /// </param>
        /// <param name="widthBoundingRect">
        ///   Width of the rectangle bounding the chart.
        /// </param>
        /// <param name="heightBoundingRect">
        ///   Height of the rectangle bounding the chart.
        /// </param>
        /// <param name="values">
        ///   An array of <c>decimal</c> values to chart.
        /// </param>
        /// <param name="sliceColors">
        ///   An array of colors used to render slices.
        /// </param>
        /// <param name="sliceRelativeHeight">
        ///   Thickness of the slice to chart relative to the height of the
        ///   bounding rectangle.
        /// </param>
        public PieChart3D(float xBoundingRect, float yBoundingRect, float widthBoundingRect, float heightBoundingRect, decimal[] values, Color[] sliceColors, float sliceRelativeHeight)
            : this(xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect, values, sliceRelativeHeight) {
            m_sliceColors = sliceColors;
        }
        
        /// <summary>
        ///   Initializes a new instance of <c>PieChart3D</c> with given bounds,
        ///   array of values and corresponding colors.
        /// </summary>
        /// <param name="boundingRectangle">
        ///   Bounding rectangle.
        /// </param>
        /// <param name="values">
        ///   Array of values to chart.
        /// </param>
        /// <param name="sliceColors">
        ///   Colors used for rendering individual slices.
        /// </param>
        /// <param name="sliceRelativeHeight">
        ///   Pie slice relative height.
        /// </param>
        public PieChart3D(RectangleF boundingRectangle, decimal[] values, Color[] sliceColors, float sliceRelativeHeight)
            : this(boundingRectangle, values, sliceRelativeHeight) {
            m_sliceColors = sliceColors;
        }

        /// <summary>
        ///   Disposes of all pie slices.
        /// </summary>
        public virtual void Dispose() {
            foreach (PieSlice slice in m_pieSlices)
                slice.Dispose();
        }

        /// <summary>
        ///   Sets values to be displayed on the chart.
        /// </summary>
        public decimal[] Values {
            set {
                Debug.Assert(value != null && value.Length > 0);
                m_values = value;
            }
        }

        /// <summary>
        ///   Sets colors used for individual pie slices.
        /// </summary>
        public Color[] Colors {
            set { m_sliceColors = value; }
        }

        /// <summary>
        ///   Sets slice edge color mode. If set to <c>PenColor</c> (default),
        ///   then value set by <c>EdgeColor</c> property is used.
        /// </summary>
        public EdgeColorType EdgeColorType {
            set { m_edgeColorType = value; }
        }

        /// <summary>
        ///   Sets slice edge line width. If not set, default value is 1.
        /// </summary>
        public float EdgeLineWidth {
            set { m_edgeLineWidth = value; }
        }

        /// <summary>
        ///   Sets slice height, relative to the top ellipse semi-axis. Must be
        ///   less than or equal to 0.5.
        /// </summary>
        public float SliceRelativeHeight {
            set {
                Debug.Assert(value <= 0.5F);
                m_sliceRelativeHeight = value;
            }
        }

        /// <summary>
        ///   Sets the slice displacement relative to the ellipse semi-axis.
        ///   Must be less than 1.
        /// </summary>
        public float SliceRelativeDisplacement {
            set {
                Debug.Assert(IsDisplacementValid(value));
                m_sliceRelativeDisplacements = new float[] { value };
            }
        }

        /// <summary>
        ///   Sets the slice displacement relative to the ellipse semi-axis.
        ///   Must be less than 1.
        /// </summary>
        public float[] SliceRelativeDisplacements {
            set {
                m_sliceRelativeDisplacements = value;
                Debug.Assert(AreDisplacementsValid(value));
            }
        }

        /// <summary>
        ///   Gets or sets the size of the entire pie chart.
        /// </summary>
        public SizeF ChartSize {
            set {
                m_widthBoundingRect = value.Width;
                m_heightBoundingRect = value.Height;
            }
        }

        /// <summary>
        ///   Gets or sets the width of the bounding rectangle.
        /// </summary>
        public float Width {
            get { return m_widthBoundingRect; }
            set { m_widthBoundingRect = value; }
        }

        /// <summary>
        ///   Gets or sets the height of the bounding rectangle.
        /// </summary>
        public float Height {
            get { return m_heightBoundingRect; }
            set { m_heightBoundingRect = value; }
        }

        /// <summary>
        ///   Gets the y-coordinate of the bounding rectangle top edge.
        /// </summary>
        public float Top {
            get { return m_yBoundingRect; }
        }

        /// <summary>
        ///   Gets the y-coordinate of the bounding rectangle bottom edge.
        /// </summary>
        public float Bottom {
            get { return m_yBoundingRect + m_heightBoundingRect; }
        }

        /// <summary>
        ///   Gets the x-coordinate of the bounding rectangle left edge.
        /// </summary>
        public float Left {
            get { return m_xBoundingRect; }
        }

        /// <summary>
        ///   Gets the x-coordinate of the bounding rectangle right edge.
        /// </summary>
        public float Right {
            get { return m_xBoundingRect + m_widthBoundingRect; }
        }

        /// <summary>
        ///   Gets or sets the x-coordinate of the upper-left corner of the 
        ///   bounding rectangle.
        /// </summary>
        public float X {
            get { return m_xBoundingRect; }
            set { m_xBoundingRect = value; }
        }

        /// <summary>
        ///   Gets or sets the y-coordinate of the upper-left corner of the 
        ///   bounding rectangle.
        /// </summary>
        public float Y {
            get { return m_yBoundingRect; }
            set { m_yBoundingRect = value; }
        }

        /// <summary>
        ///   Sets the shadowing style used.
        /// </summary>
        public ShadowStyle ShadowStyle {
            set { m_shadowStyle = value; }
        }

        /// <summary>
        ///   Sets the flag that controls if chart is fit to bounding rectangle 
        ///   exactly.
        /// </summary>
        public bool FitToBoundingRectangle {
            set { m_fitToBoundingRectangle = value; }
        }

        /// <summary>
        ///   Sets the initial angle from which pies are placed.
        /// </summary>
        public float InitialAngle {
            set { 
                m_initialAngle = value % 360;
                if (m_initialAngle < 0)
                    m_initialAngle += 360;
            }
        }

        public int HighlightedIndex {
            set { m_highlightedIndex = value; }
        }
        
        /// <summary>
        ///   Draws the chart.
        /// </summary>
        /// <param name="graphics">
        ///   <c>Graphics</c> object used for drawing.
        /// </param>
        public void Draw(Graphics graphics) {
            Debug.Assert(m_values != null && m_values.Length > 0);
            InitializePieSlices();
            if (m_fitToBoundingRectangle) {
                RectangleF newBoundingRectangle = GetFittingRectangle();
                ReadjustSlices(newBoundingRectangle);
            }
            if (m_sliceRelativeHeight > 0F) {
                DrawSliceSides(graphics);
            }
            DrawTops(graphics);
        }

        /// <summary>
        ///   Searches the chart to find the index of the pie slice which 
        ///   contains point given. Search order goes in the direction opposite
        ///   to drawing order.
        /// </summary>
        /// <param name="point">
        ///   <c>PointF</c> point for which pie slice is searched for.
        /// </param>
        /// <returns>
        ///   Index of the corresponding pie slice, or -1 if none is found.
        /// </returns>
        public int FindPieSliceUnderPoint(PointF point) {
            // first check tops
            for (int i = 0; i < m_pieSlices.Length; ++i) {
                PieSlice slice = (PieSlice)m_pieSlices[i];
                if (slice.PieSliceContainsPoint(point))
                    return (int)m_pieSlicesMapping[i];
            }
            // split the backmost (at 270 degrees) pie slice
            ArrayList pieSlicesList = new ArrayList(m_pieSlices);
            PieSlice[] splitSlices = m_pieSlices[0].Split(270F);
            pieSlicesList[0] = splitSlices[0];
            if (splitSlices[1].SweepAngle > 0F) {
                pieSlicesList.Add(splitSlices[1]);
            }
            PieSlice[] pieSlices = (PieSlice[])pieSlicesList.ToArray(typeof(PieSlice));
            int indexFound = -1;
            // if not found yet, then check for periferies
            int incrementIndex = 0;
            int decrementIndex = pieSlices.Length - 1;
            while (incrementIndex <= decrementIndex) {
                PieSlice sliceLeft = pieSlices[decrementIndex];
                float angle1 = 270 - sliceLeft.StartAngle;
                PieSlice sliceRight = pieSlices[incrementIndex]; 
                float angle2 = (sliceRight.EndAngle + 90) % 360;
                Debug.Assert(angle2 >= 0);
                if (angle2 < angle1) {
                    if (sliceRight.PeripheryContainsPoint(point))
                        indexFound = incrementIndex;
                    ++incrementIndex;
                }
                else {
                    if (sliceLeft.PeripheryContainsPoint(point))
                        indexFound = decrementIndex;
                    --decrementIndex;
                }
            }
            // check for start/stop sides, starting from the foremost
            if (indexFound < 0) {
                int foremostPieIndex = GetForemostPieSlice(pieSlices);
                // check for start sides from the foremost slice to the left 
                // side
                int i = foremostPieIndex;
                while (i < pieSlices.Length) {
                    PieSlice sliceLeft = (PieSlice)pieSlices[i];
                    if (sliceLeft.StartSideContainsPoint(point)) {
                        indexFound = i;
                        break;
                    }
                    ++i;
                }
                // if not found yet, check end sides from the foremost to the right
                // side
                if (indexFound < 0) {
                    i = foremostPieIndex;
                    while (i >= 0) {
                        PieSlice sliceLeft = (PieSlice)pieSlices[i];
                        if (sliceLeft.EndSideContainsPoint(point)) {
                            indexFound = i;
                            break;
                        }
                        --i;
                    }
                }
            }
            // finally search for bottom sides
            if (indexFound < 0) {
                for (int i = 0; i < m_pieSlices.Length; ++i) {
                    PieSlice slice = (PieSlice)m_pieSlices[i];
                    if (slice.BottomSurfaceSectionContainsPoint(point))
                        return (int)m_pieSlicesMapping[i];
                }
            }
            if (indexFound > -1) {
                indexFound %= (m_pieSlicesMapping.Count);
                return (int)m_pieSlicesMapping[indexFound];
            }
            return -1;
        }

        /// <summary>
        ///   Return the index of the foremost pie slice i.e. the one crossing
        ///   90 degrees boundary.
        /// </summary>
        /// <param name="pieSlices">
        ///   Array of <c>PieSlice</c> objects to examine.
        /// </param>
        /// <returns>
        ///   Index of the foremost pie slice.
        /// </returns>
        private int GetForemostPieSlice(PieSlice[] pieSlices) {
            Debug.Assert(pieSlices != null && pieSlices.Length > 0);
            for (int i = 0; i < pieSlices.Length; ++i) {
                PieSlice pieSlice = pieSlices[i];
                if (((pieSlice.StartAngle <= 90) && ((pieSlice.StartAngle + pieSlice.SweepAngle) >= 90)) ||
                    ((pieSlice.StartAngle + pieSlice.SweepAngle > 360) && ((pieSlice.StartAngle) <= 450) && (pieSlice.StartAngle + pieSlice.SweepAngle) >= 450)) {
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
        protected RectangleF GetFittingRectangle() {
            RectangleF boundingRectangle = m_pieSlices[0].GetFittingRectangle();
            for (int i = 1; i < m_pieSlices.Length; ++i) {
                boundingRectangle = RectangleF.Union(boundingRectangle, m_pieSlices[i].GetFittingRectangle());
            }
            return boundingRectangle;
        }

        /// <summary>
        ///   Readjusts each slice for new bounding rectangle. 
        /// </summary>
        /// <param name="newBoundingRectangle">
        ///   <c>RectangleF</c> representing new boundary.
        /// </param>
        protected void ReadjustSlices(RectangleF newBoundingRectangle) {
            float xResizeFactor = m_widthBoundingRect / newBoundingRectangle.Width;
            float yResizeFactor = m_heightBoundingRect / newBoundingRectangle.Height;
            float xOffset = newBoundingRectangle.X - m_xBoundingRect;
            float yOffset = newBoundingRectangle.Y - m_yBoundingRect;
            foreach (PieSlice slice in m_pieSlices) {
                float x = slice.BoundingRectangle.X - xOffset;
                float y = slice.BoundingRectangle.Y - yOffset;
                float width = slice.BoundingRectangle.Width * xResizeFactor;
                float height = slice.BoundingRectangle.Height * yResizeFactor;
                float sliceHeight = slice.SliceHeight * yResizeFactor;
                slice.Readjust(x, y, width, height, sliceHeight);
            }
        }

        /// <summary>
        ///   Finds the largest displacement.
        /// </summary>
        protected float LargestDisplacement {
            get {
                float value = 0F;
                for (int i = 0; i < m_sliceRelativeDisplacements.Length && i < m_values.Length; ++i) {
                    if (m_sliceRelativeDisplacements[i] > value)
                        value = m_sliceRelativeDisplacements[i];
                }
                return value;
            }
        }

        /// <summary>
        ///   Gets the top ellipse size.
        /// </summary>
        protected SizeF TopEllipseSize {
            get {
                float factor = 1 + LargestDisplacement;
                float widthTopEllipse = m_widthBoundingRect / factor; 
                float heightTopEllipse = m_heightBoundingRect / factor * (1 - m_sliceRelativeHeight); 
                return new SizeF(widthTopEllipse, heightTopEllipse);
            }
        }

        /// <summary>
        ///   Gets the ellipse defined by largest displacement.
        /// </summary>
        protected SizeF LargestDisplacementEllipseSize {
            get {
                float factor = LargestDisplacement;
                float widthDisplacementEllipse = TopEllipseSize.Width * factor;
                float heightDisplacementEllipse = TopEllipseSize.Height * factor; 
                return new SizeF(widthDisplacementEllipse, heightDisplacementEllipse);
            }
        }

        /// <summary>
        ///   Calculates the pie height.
        /// </summary>
        protected float PieHeight {
            get {
                return m_heightBoundingRect / (1 + LargestDisplacement) * m_sliceRelativeHeight;
            }
        }

        /// <summary>
        ///   Initializes pies.
        /// </summary>
        /// Creates a list of pies, starting with the pie that is crossing the 
        /// 270 degrees boundary, i.e. "backmost" pie that always has to be 
        /// drawn first to ensure correct surface overlapping.
        protected virtual void InitializePieSlices() {
            // calculates the sum of values required to evaluate sweep angles 
            // for individual pies
            double sum = 0;
            foreach (decimal itemValue in m_values) 
                sum += (double)itemValue;
            // some values and indices that will be used in the loop
            SizeF topEllipeSize = TopEllipseSize;
            SizeF largestDisplacementEllipseSize = LargestDisplacementEllipseSize;
            int maxDisplacementIndex = m_sliceRelativeDisplacements.Length - 1;
            float largestDisplacement = LargestDisplacement;
            ArrayList listPieSlices = new ArrayList();
            m_pieSlicesMapping.Clear();
            int colorIndex = 0;
            int backPieIndex = -1;
            int displacementIndex = 0;
            double startAngle = (double)m_initialAngle;
            for (int i = 0; i < m_values.Length; ++i) {
                decimal itemValue = m_values[i];
                double sweepAngle = (double)itemValue / sum * 360;
                // displacement from the center of the ellipse
                float xDisplacement = m_sliceRelativeDisplacements[displacementIndex];
                float yDisplacement = m_sliceRelativeDisplacements[displacementIndex];
                if (xDisplacement > 0F) {
                    Debug.Assert(largestDisplacement > 0F);
                    SizeF pieDisplacement = GetSliceDisplacement((float)(startAngle + sweepAngle / 2), m_sliceRelativeDisplacements[displacementIndex]);
                    xDisplacement = pieDisplacement.Width;
                    yDisplacement = pieDisplacement.Height;
                }
                PieSlice slice = null;
                if (i == m_highlightedIndex) 
                    slice = CreatePieSliceHighlighted(m_xBoundingRect + largestDisplacementEllipseSize.Width / 2 + xDisplacement, m_yBoundingRect + largestDisplacementEllipseSize.Height / 2 + yDisplacement, topEllipeSize.Width, topEllipeSize.Height, PieHeight, (float)(startAngle % 360), (float)(sweepAngle), m_sliceColors[colorIndex], m_shadowStyle, m_edgeColorType, m_edgeLineWidth);
                else
                    slice = CreatePieSlice(m_xBoundingRect + largestDisplacementEllipseSize.Width / 2 + xDisplacement, m_yBoundingRect + largestDisplacementEllipseSize.Height / 2 + yDisplacement, topEllipeSize.Width, topEllipeSize.Height, PieHeight, (float)(startAngle % 360), (float)(sweepAngle), m_sliceColors[colorIndex], m_shadowStyle, m_edgeColorType, m_edgeLineWidth);
                // the backmost pie is inserted to the front of the list for correct drawing
                if (backPieIndex > -1 || ((startAngle <= 270) && (startAngle + sweepAngle > 270)) || ((startAngle >= 270) && (startAngle + sweepAngle > 630))) {
                    ++backPieIndex;
                    listPieSlices.Insert(backPieIndex, slice);
                    m_pieSlicesMapping.Insert(backPieIndex, i);
                }
                else {
                    listPieSlices.Add(slice);
                    m_pieSlicesMapping.Add(i);
                }
                // increment displacementIndex only if there are more displacements available
                if (displacementIndex < maxDisplacementIndex)
                    ++displacementIndex;
                ++colorIndex;
                // if all colors have been exhausted, reset color index
                if (colorIndex >= m_sliceColors.Length)
                    colorIndex = 0;
                // prepare for the next pie slice
                startAngle += sweepAngle;
                if (startAngle > 360)
                    startAngle -= 360;
            }
            m_pieSlices = (PieSlice[])listPieSlices.ToArray(typeof(PieSlice));
        }

        /// <summary>
        ///   Creates a <c>PieSlice</c> object.
        /// </summary>
        /// <param name="boundingRectLeft">
        ///   x-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the slice.
        /// </param>
        /// <param name="boundingRectTop">
        ///   y-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the slice.
        /// </param>
        /// <param name="boundingRectWidth">
        ///   Width of the rectangle that is used to draw the top surface of 
        ///   the slice.
        /// </param>
        /// <param name="boundingRectHeight">
        ///   Height of the rectangle that is used to draw the top surface of 
        ///   the slice.
        /// </param>
        /// <param name="sliceHeight">
        ///   Slice height.
        /// </param>
        /// <param name="startAngle">
        ///   Starting angle.
        /// </param>
        /// <param name="sweepAngle">
        ///   Sweep angle.
        /// </param>
        /// <param name="color">
        ///   Color used for slice rendering.
        /// </param>
        /// <param name="shadowStyle">
        ///   Shadow style used for slice rendering.
        /// </param>
        /// <param name="edgeColorType">
        ///   Edge lines color type.
        /// </param>
        /// <param name="edgeLineWidth">
        ///   Edge lines width.
        /// </param>
        /// <returns>
        ///   <c>PieSlice</c> object with given values.
        /// </returns>
        protected virtual PieSlice CreatePieSlice(float boundingRectLeft, float boundingRectTop, float boundingRectWidth, float boundingRectHeight, float sliceHeight, float startAngle, float sweepAngle, Color color, ShadowStyle shadowStyle, EdgeColorType edgeColorType, float edgeLineWidth) {
            return new PieSlice(boundingRectLeft, boundingRectTop, boundingRectWidth, boundingRectHeight, sliceHeight, (float)(startAngle % 360), sweepAngle, color, shadowStyle, edgeColorType, edgeLineWidth);
        }

        /// <summary>
        ///   Creates highlighted <c>PieSlice</c> object.
        /// </summary>
        /// <param name="boundingRectLeft">
        ///   x-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the slice.
        /// </param>
        /// <param name="boundingRectTop">
        ///   y-coordinate of the upper-left corner of the rectangle that is 
        ///   used to draw the top surface of the slice.
        /// </param>
        /// <param name="boundingRectWidth">
        ///   Width of the rectangle that is used to draw the top surface of 
        ///   the slice.
        /// </param>
        /// <param name="boundingRectHeight">
        ///   Height of the rectangle that is used to draw the top surface of 
        ///   the slice.
        /// </param>
        /// <param name="sliceHeight">
        ///   Slice height.
        /// </param>
        /// <param name="startAngle">
        ///   Starting angle.
        /// </param>
        /// <param name="sweepAngle">
        ///   Sweep angle.
        /// </param>
        /// <param name="color">
        ///   Color used for slice rendering.
        /// </param>
        /// <param name="shadowStyle">
        ///   Shadow style used for slice rendering.
        /// </param>
        /// <param name="edgeColorType">
        ///   Edge lines color type.
        /// </param>
        /// <param name="edgeLineWidth">
        ///   Edge lines width.
        /// </param>
        /// <returns>
        ///   <c>PieSlice</c> object with given values.
        /// </returns>
        protected virtual PieSlice CreatePieSliceHighlighted(float boundingRectLeft, float boundingRectTop, float boundingRectWidth, float boundingRectHeight, float sliceHeight, float startAngle, float sweepAngle, Color color, ShadowStyle shadowStyle, EdgeColorType edgeColorType, float edgeLineWidth) {
            Color highLightedColor = ColorUtil.CreateColorWithCorrectedLightness(color, ColorUtil.BrightnessEnhancementFactor1);
            return new PieSlice(boundingRectLeft, boundingRectTop, boundingRectWidth, boundingRectHeight, sliceHeight, (float)(startAngle % 360), sweepAngle, highLightedColor, shadowStyle, edgeColorType, edgeLineWidth);
        }

        /// <summary>
        ///   Calculates the displacement for given angle.
        /// </summary>
        /// <param name="angle">
        ///   Angle (in degrees).
        /// </param>
        /// <param name="displacementFactor">
        ///   Displacement factor.
        /// </param>
        /// <returns>
        ///   <c>SizeF</c> representing displacement.
        /// </returns>
        protected SizeF GetSliceDisplacement(float angle, float displacementFactor) {
            Debug.Assert(displacementFactor > 0F && displacementFactor <= 1F);
            if (displacementFactor == 0F)
                return SizeF.Empty;
            float xDisplacement = (float)(TopEllipseSize.Width * displacementFactor / 2 * Math.Cos(angle * Math.PI / 180));
            float yDisplacement = (float)(TopEllipseSize.Height * displacementFactor / 2 * Math.Sin(angle * Math.PI / 180));
            return new SizeF(xDisplacement, yDisplacement);
        }

        /// <summary>
        ///   Draws outer peripheries of all slices.
        /// </summary>
        /// <param name="graphics">
        ///   <c>Graphics</c> used for drawing.
        /// </param>
        protected void DrawSliceSides(Graphics graphics) {
            ArrayList pieSlices = new ArrayList(m_pieSlices);
            // if the first slice spreads across 180 and 360 degrees boundaries it
            // will appear on both left and right edge so its periphery has to be 
            // drawn twice
            PieSlice[] splitSlices = m_pieSlices[0].Split(270F);
            pieSlices[0] = splitSlices[0];
            if (splitSlices[1].SweepAngle > 0F)
                pieSlices.Add(splitSlices[1]);
            // finds the leftmost slice that is crossing 180 degrees boundary
            int decrementIndex = pieSlices.Count - 1;
            for (int i = decrementIndex; i > 0; --i) {
                if (((PieSlice)pieSlices[i]).StartAngle < 180) {
                    decrementIndex = i;
                    break;
                }
            }
            // finds the rightmost slice that is crossing 0 degrees boundary
            int incrementIndex = decrementIndex;
            for (int i = 0; i < pieSlices.Count - 1; ++i) {
                PieSlice slice = (PieSlice)pieSlices[i];
                if ((slice.StartAngle + slice.SweepAngle > 360) || (slice.EndAngle < 180 && slice.EndAngle > 0)) {
                    incrementIndex = i;
                    break;
                }
            }
            // draws visible start sides for slices from the backmost (at 270 degrees)
            // to the leftmost one
            for (int i = pieSlices.Count - 1; i > decrementIndex; --i) {
                PieSlice slice = (PieSlice)pieSlices[i];
                slice.DrawVisibleStartSide(graphics);
            }
            // draws visible end sides for slices from the backmost (at 270 degrees)
            // to the rightmost one
            for (int i = 0; i < incrementIndex; ++i) {
                PieSlice slice = (PieSlice)pieSlices[i];
                slice.DrawVisibleEndSide(graphics);
            }
            // draws from leftmost and rightmost alternatively, drawing the slice
            // with larger offset from 90 degrees first
            while (incrementIndex < decrementIndex) {
                PieSlice sliceLeft = (PieSlice)pieSlices[decrementIndex];
                float angle1 = 270 - sliceLeft.StartAngle;
                PieSlice sliceRight = (PieSlice)pieSlices[incrementIndex]; 
                float angle2 = (sliceRight.EndAngle + 90) % 360;
                Debug.Assert(angle2 >= 0);
                if (angle2 < angle1) {
                    sliceRight.DrawVisibleEndSide(graphics);
                    sliceRight.DrawVisiblePeriphery(graphics);
                    ++incrementIndex;
                }
                else {
                    sliceLeft.DrawVisibleStartSide(graphics);
                    sliceLeft.DrawVisiblePeriphery(graphics);
                    --decrementIndex;
                }
            }
            // for the foremost slice only periphery has to be drawn 
            ((PieSlice)pieSlices[incrementIndex]).DrawVisiblePeriphery(graphics);
        }

        /// <summary>
        ///   Draws top sides of all pie slices.
        /// </summary>
        /// <param name="graphics">
        ///   <c>Graphics</c> used for drawing.
        /// </param>
        protected void DrawTops(Graphics graphics) {
            foreach (PieSlice slice in m_pieSlices) {
                slice.DrawTop(graphics);
            }
        }

        /// <summary>
        ///   Helper function used in assertions. Checks the validity of 
        ///   slice displacements.
        /// </summary>
        /// <param name="displacements">
        ///   Array of displacements to check.
        /// </param>
        /// <returns>
        ///   <c>true</c> if all displacements have a valid value; otherwise 
        ///   <c>false</c>.
        /// </returns>
        private bool AreDisplacementsValid(float[] displacements) {
            foreach (float value in displacements) {
                if (!IsDisplacementValid(value))
                    return false;
            }
            return true;
        }

        /// <summary>
        ///   Helper function used in assertions. Checks the validity of 
        ///   a slice displacement.
        /// </summary>
        /// <param name="value">
        ///   Displacement value to check.
        /// </param>
        /// <returns>
        ///   <c>true</c> if displacement has a valid value; otherwise 
        ///   <c>false</c>.
        /// </returns>
        private bool IsDisplacementValid(float value) {
            return (value >= 0F && value <= 1F);
        }

        /// <summary>
        ///   x-coordinate of the top left corner of the bounding rectangle.
        /// </summary>
        protected float         m_xBoundingRect;
        /// <summary>
        ///   y-coordinate of the top left corner of the bounding rectangle.
        /// </summary>
        protected float         m_yBoundingRect;
        /// <summary>
        ///   Width of the bounding rectangle.
        /// </summary>
        protected float         m_widthBoundingRect;
        /// <summary>
        ///   Height of the bounding rectangle.
        /// </summary>
        protected float         m_heightBoundingRect;
        /// <summary>
        ///   Slice relative height.
        /// </summary>
        protected float         m_sliceRelativeHeight       = 0F;
        /// <summary>
        ///   Initial angle from which chart is drawn.
        /// </summary>
        protected float         m_initialAngle              = 0F;
        /// <summary>
        ///   Array of ordered pie slices constituting the chart, starting from 
        ///   270 degrees axis.
        /// </summary>
        protected PieSlice[]    m_pieSlices;
        /// <summary>
        ///   Collection of reordered pie slices mapped to original order.
        /// </summary>
        protected ArrayList     m_pieSlicesMapping = new ArrayList();
        /// <summary>
        ///   Array of values to be presented by the chart.
        /// </summary>
        protected decimal[]     m_values = new decimal[] {};
        /// <summary>
        ///   Array of colors used for rendering.
        /// </summary>
        protected Color[]       m_sliceColors = new Color[] {
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
        protected float[]       m_sliceRelativeDisplacements   = new float[] { 0F };
        /// <summary>
        ///   Edge color type used for rendering.
        /// </summary>
        protected EdgeColorType m_edgeColorType = EdgeColorType.SystemColor;
        /// <summary>
        ///   Edge line width.
        /// </summary>
        protected float         m_edgeLineWidth = 1F;
        /// <summary>
        ///   Shadow style.
        /// </summary>
        protected ShadowStyle   m_shadowStyle = ShadowStyle.NoShadow;
        /// <summary>
        ///   Should the chart fit the bounding rectangle exactly.
        /// </summary>
        protected bool          m_fitToBoundingRectangle = true;
        /// <summary>
        ///   Index of the currently highlighted pie slice.
        /// </summary>
        protected int           m_highlightedIndex = -1;
	}
}
