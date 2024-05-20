using System.Diagnostics;
using GitCommands;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace GitUI.Infrastructure.Telemetry
{
    internal class AppInfoTelemetryInitializer : ITelemetryInitializer
    {
        private readonly bool _isDirty;
        private static readonly string AppCurrentTranslationKey = nameof(AppSettings.CurrentTranslation).FormatKey();
        private static readonly string AppEnvironmentKey = "Environment".FormatKey();
        private static readonly string AppIsPortableKey = "IsPortable".FormatKey();
        private static readonly string AppIsReleaseKey = "IsRelease".FormatKey();
        private static readonly string AppStartWithRecentWorkingDirKey = nameof(AppSettings.StartWithRecentWorkingDir).FormatKey();

        public AppInfoTelemetryInitializer(bool isDirty)
        {
            _isDirty = isDirty;
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Component.Version = AppSettings.ProductVersion;
            telemetry.Context.GlobalProperties[AppIsReleaseKey] = (!_isDirty).ToString();
            telemetry.Context.GlobalProperties[AppIsPortableKey] = AppSettings.IsPortable().ToString();
            telemetry.Context.GlobalProperties[AppCurrentTranslationKey] = AppSettings.CurrentTranslation;
            telemetry.Context.GlobalProperties[AppStartWithRecentWorkingDirKey] = AppSettings.StartWithRecentWorkingDir.ToString();

            // Always default to development if we're in the debugger
            if (Debugger.IsAttached)
            {
                telemetry.Context.GlobalProperties[AppEnvironmentKey] = "development";
            }
        }
    }

    internal static class TelemetryKeyExtensions
    {
        public static string FormatKey(this string key)
        {
            return string.Concat("GE.", key);
        }
    }
}
