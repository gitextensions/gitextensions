using System.Diagnostics;
using System.Drawing;

namespace GitStatistics.PieChart
{
    /// <summary>
    ///   Structure representing edge color used for rendering.
    /// </summary>
    public static class EdgeColor
    {
        private const float BrightnessThreshold = 0.4F;

        /// <summary>
        ///   Gets the actual color used for rendering.
        /// </summary>
        public static Color GetRenderingColor(EdgeColorType edgeColorType, Color color)
        {
            Debug.Assert(color != Color.Empty, "color != Color.Empty");
            if (edgeColorType == EdgeColorType.Contrast || edgeColorType == EdgeColorType.EnhancedContrast)
            {
                edgeColorType = GetContrastColorType(color, edgeColorType);
            }

            float correctionFactor = 0;
            switch (edgeColorType)
            {
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
                    return Color.Transparent;
            }

            return ColorUtil.CreateColorWithCorrectedLightness(color, correctionFactor);
        }

        private static EdgeColorType GetContrastColorType(Color color, EdgeColorType colorType)
        {
            Debug.Assert(colorType == EdgeColorType.Contrast || colorType == EdgeColorType.EnhancedContrast, "colorType == EdgeColorType.Contrast || colorType == EdgeColorType.EnhancedContrast");
            if (color.GetBrightness() > BrightnessThreshold)
            {
                return colorType ==
                       EdgeColorType.Contrast
                           ? EdgeColorType.DarkerThanSurface
                           : EdgeColorType.DarkerDarkerThanSurface;
            }

            return colorType ==
                   EdgeColorType.Contrast
                       ? EdgeColorType.LighterThanSurface
                       : EdgeColorType.LighterLighterThanSurface;
        }

        private static Color GetFullContrastColor(Color color)
        {
            return
                color.GetBrightness() > BrightnessThreshold
                    ? Color.Black
                    : Color.White;
        }
    }
}