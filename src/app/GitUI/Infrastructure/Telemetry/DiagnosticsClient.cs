// The original idea and the implementation are borrowed from  https://github.com/NuGetPackageExplorer/NuGetPackageExplorer
// Credits to https://github.com/clairernovotny

using GitCommands;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace GitUI.Infrastructure.Telemetry
{
    public static class DiagnosticsClient
    {
        private static bool _initialized;
        private static TelemetryClient? _client;
        private static TelemetryConfiguration _telemetryConfiguration = TelemetryConfiguration.CreateDefault();

        private static bool Enabled => _initialized && (AppSettings.TelemetryEnabled ?? false);

        public static void Initialize(bool isDirty)
        {
            _telemetryConfiguration.TelemetryInitializers.Add(new AppEnvironmentTelemetryInitializer());
            _telemetryConfiguration.TelemetryInitializers.Add(new AppInfoTelemetryInitializer(isDirty));
            _telemetryConfiguration.TelemetryInitializers.Add(new MonitorsTelemetryInitializer());
            _telemetryConfiguration.TelemetryInitializers.Add(new ThemingTelemetryInitializer());

            Application.ApplicationExit += (s, e) =>
            {
                TrackEvent("AppExit");
                OnExit();
            };

            _client = new TelemetryClient(_telemetryConfiguration);

            // override capture of the hostname
            // https://github.com/Microsoft/ApplicationInsights-dotnet/blob/80025b5d79cc52485510d422cfa5a0a8159dac83/src/Microsoft.ApplicationInsights/TelemetryClient.cs#L544
            _client.Context.Cloud.RoleInstance = AppSettings.ApplicationName;
            _client.Context.Cloud.RoleName = AppSettings.ApplicationName;

            _initialized = true;
        }

        public static void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            if (!Enabled)
            {
                return;
            }

            _client!.TrackEvent(eventName, properties, metrics);
        }

        public static void TrackTrace(string evt)
        {
            if (!Enabled)
            {
                return;
            }

            _client!.TrackTrace(evt);
        }

        public static void Notify(Exception exception)
        {
            if (!Enabled)
            {
                return;
            }

            _client!.TrackException(exception);
        }

        public static void TrackPageView(string pageName)
        {
            if (!Enabled)
            {
                return;
            }

            _client!.TrackPageView(pageName);
        }

        private static void OnExit()
        {
            if (!Enabled)
            {
                return;
            }

            _client!.Flush();

            // Allow time for flushing:
            System.Threading.Thread.Sleep(1000);
        }
    }
}
