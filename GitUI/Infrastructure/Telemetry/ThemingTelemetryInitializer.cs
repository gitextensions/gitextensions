using GitExtUtils.GitUI.Theming;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace GitUI.Infrastructure.Telemetry
{
    internal class ThemingTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            var properties = telemetry.Context.GlobalProperties;
            var themeSettings = Theming.ThemeModule.Settings;
            properties["Theme dark"] = themeSettings.Theme.Id.Name == "dark" ? 1 : 0;
            properties["Theme builtin"] = themeSettings.Theme.Id.IsBuiltin ? 1 : 0;
            properties["Theme systemstyles"] = themeSettings.UseSystemVisualStyle ? 1 : 0;
        }
    }
}
