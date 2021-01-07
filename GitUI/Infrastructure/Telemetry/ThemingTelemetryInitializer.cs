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
            properties["Theme dark"] = FlagString(themeSettings.Theme.Id.Name == "dark");
            properties["Theme builtin"] = FlagString(themeSettings.Theme.Id.IsBuiltin);
            properties["Theme systemstyles"] = FlagString(themeSettings.UseSystemVisualStyle);
        }

        private static string FlagString(bool value) =>
            value ? "1" : "0";
    }
}
