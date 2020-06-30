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
            ThemeId themeId = Theming.ThemeModule.Settings.Theme.Id;
            properties["Theme name"] = themeId.Name;
            properties["Theme is builtin"] = themeId.IsBuiltin.ToString();
            properties["Theme use system visual style"] = Theming.ThemeModule.Settings.UseSystemVisualStyle.ToString();
        }
    }
}
