using System;
using System.Drawing;
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

            if (Theming.ThemeModule.Settings.Theme != Theme.Default)
            {
                foreach ((AppColor appColor, Color color) in Theming.ThemeModule.Settings.Theme.AppColorValues)
                {
                    properties[$"Theme application color {appColor}"] = Format(color);
                }

                foreach ((KnownColor sysColor, Color color) in Theming.ThemeModule.Settings.Theme.SysColorValues)
                {
                    properties[$"Theme system color {sysColor}"] = Format(color);
                }
            }

            return;

            static string Format(Color c) =>
                c.ToArgb().ToString("x8");
        }
    }
}
