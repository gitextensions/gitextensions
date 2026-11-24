using GitExtensions.Plugins.GitStatistics.PieChart;

namespace WinFormsParityCapture;

// parity-scaffolding: Render the original chart with the same public inputs as the content-color regression.
internal sealed class StatisticsChartCaptureSurface : Control
{
    public StatisticsChartCaptureSurface()
    {
        Size = new Size(320, 240);
        BackColor = Color.White;
        SetStyle(ControlStyles.UserPaint, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using PieChart3D chart = new(
            20,
            20,
            280,
            200,
            [5, 3, 2],
            [Color.Red, Color.Green, Color.Blue],
            sliceRelativeHeight: 0.2f)
        {
            FitToBoundingRectangle = true,
            EdgeColorType = EdgeColorType.DarkerThanSurface,
            ShadowStyle = ShadowStyle.GradualShadow,
        };
        chart.SetInitialAngle(-30);
        chart.Draw(e.Graphics);
    }
}
