using System;
using System.Diagnostics;

namespace System.Drawing.PieChart {

    /// <summary>
    ///   Enumeration for different shadow styles
    /// </summary>
    public enum ShadowStyle {
        /// <summary>
        ///   No shadow. Sides are drawn in the same color as the top od the 
        ///   pie.
        /// </summary>
        NoShadow,
        /// <summary>
        ///   Uniform shadow. Sides are drawn somewhat darker.
        /// </summary>
        UniformShadow,
        /// <summary>
        ///   Gradual shadow is used to fully simulate 3-D shadow.
        /// </summary>
        GradualShadow
    }

    /// <summary>
    ///   Enumeration for edge color types.
    /// </summary>
    public enum EdgeColorType {
        /// <summary>
        ///   Edges are not drawn at all.
        /// </summary>
        NoEdge,
        /// <summary>
        ///   System (window text) color is used to draw edges.
        /// </summary>
        SystemColor,
        /// <summary>
        ///   Surface color is used for edges.
        /// </summary>
        SurfaceColor,
        /// <summary>
        ///   A color that is little darker than surface color is used for
        ///   edges.
        /// </summary>
        DarkerThanSurface,
        /// <summary>
        ///   A color that is significantly darker than surface color is used 
        ///   for edges.
        /// </summary>
        DarkerDarkerThanSurface,
        /// <summary>
        ///   A color that is little lighter than surface color is used for
        ///   edges.
        /// </summary>
        LighterThanSurface,
        /// <summary>
        ///   A color that is significantly lighter than surface color is used 
        ///   for edges.
        /// </summary>
        LighterLighterThanSurface,
        /// <summary>
        ///   Contrast color is used for edges.
        /// </summary>
        Contrast,
        /// <summary>
        ///   Enhanced contrast color is used for edges.
        /// </summary>
        EnhancedContrast,
        /// <summary>
        ///   Black color is used for light surfaces and white for dark 
        ///   surfaces.
        /// </summary>
        FullContrast
    }

    /// <summary>
    ///   Structure representing edge color used for rendering.
    /// </summary>
    public struct EdgeColor {

        /// <summary>
        ///   Gets the actual color used for rendering.
        /// </summary>
        public static Color GetRenderingColor(EdgeColorType edgeColorType, Color color) {
            Debug.Assert(color != Color.Empty);
            if (edgeColorType == EdgeColorType.Contrast || edgeColorType == EdgeColorType.EnhancedContrast)
                edgeColorType = GetContrastColorType(color, edgeColorType);
            float correctionFactor = 0;
            switch (edgeColorType) {
            case EdgeColorType.SystemColor:
                return SystemColors.WindowText;
            case EdgeColorType.SurfaceColor:
                return color;
            case EdgeColorType.FullContrast:
                return GetFullContrastColor(color);
            case EdgeColorType.DarkerThanSurface:
                correctionFactor = -ColorUtil.BrightnessEnhancementFactor1;
                break;
            case EdgeColorType.DarkerDarkerThanSurface:
                correctionFactor = -ColorUtil.BrightnessEnhancementFactor2;
                break;
            case EdgeColorType.LighterThanSurface:
                correctionFactor = +ColorUtil.BrightnessEnhancementFactor1;
                break;
            case EdgeColorType.LighterLighterThanSurface:
                correctionFactor = +ColorUtil.BrightnessEnhancementFactor2;
                break;
            case EdgeColorType.NoEdge:
                return System.Drawing.Color.Transparent;
            }
            return ColorUtil.CreateColorWithCorrectedLightness(color, correctionFactor);
        }

        private static EdgeColorType GetContrastColorType(Color color, EdgeColorType colorType) {
            Debug.Assert(colorType == EdgeColorType.Contrast || colorType == EdgeColorType.EnhancedContrast);
            if (color.GetBrightness() > s_brightnessThreshold) {
                if (colorType == EdgeColorType.Contrast)
                    return EdgeColorType.DarkerThanSurface;
                else 
                    return EdgeColorType.DarkerDarkerThanSurface;
            }
            if (colorType == EdgeColorType.Contrast)
                return EdgeColorType.LighterThanSurface;
            else
                return EdgeColorType.LighterLighterThanSurface;
        }

        private static Color GetFullContrastColor(Color color) {
            if (color.GetBrightness() > s_brightnessThreshold)
                return Color.Black;
            return Color.White;
        }

        private static readonly float s_brightnessThreshold = 0.4F;
    }

    /// <summary>
    ///   Color utility structure.
    /// </summary>
    public struct ColorUtil {
        /// <summary>
        ///   Creates color with corrected lightness.
        /// </summary>
        /// <param name="color">
        ///   Color to correct.
        /// </param>
        /// <param name="correctionFactor">
        ///   Correction factor, with a value between -1 and 1. Negative values
        ///   create darker color, positive values lighter color. Zero value
        ///   returns the current color.
        /// </param>
        /// <returns>
        ///   Corrected <c>Color</c> structure.
        /// </returns>
        public static Color CreateColorWithCorrectedLightness(Color color, float correctionFactor) {
            Debug.Assert(correctionFactor <= 1 && correctionFactor >= -1);
            if (correctionFactor == 0)
                return color;
            float red   = (float)color.R;
            float green = (float)color.G;
            float blue  = (float)color.B;
            if (correctionFactor < 0) {
                correctionFactor = 1 + correctionFactor;
                red   *= correctionFactor;
                green *= correctionFactor;
                blue  *= correctionFactor;
            }
            else {
                red   = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue  = (255 - blue) * correctionFactor + blue;
            }
            return System.Drawing.Color.FromArgb((int)red, (int)green, (int)blue);
        }

        /// <summary>
        ///   Small brightness change factor.
        /// </summary>
        public static readonly float BrightnessEnhancementFactor1 = 0.3F;
        /// <summary>
        ///   Large brightness change factor.
        /// </summary>
        public static readonly float BrightnessEnhancementFactor2 = 0.5F;
    }
}