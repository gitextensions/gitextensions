namespace GitStatistics.PieChart
{
    /// <summary>
    ///   Enumeration for different shadow styles
    /// </summary>
    public enum ShadowStyle
    {
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
}