using System.Windows.Forms;
using GitExtUtils.GitUI;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace GitUI.Infrastructure.Telemetry
{
    internal class MonitorsTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            var properties = telemetry.Context.GlobalProperties;
            properties["Monitor count"] = Screen.AllScreens.Length.ToString();
            properties["Monitor primary DPI"] = DpiUtil.DpiX.ToString();

            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                var key = Screen.AllScreens[i].Primary ? "primary" : $"secondary{i}";

                var bounds = Screen.AllScreens[i].Bounds;
                properties[$"Monitor {key} resolution"] = $"{bounds.Width}x{bounds.Height}";
            }
        }
    }
}
