using GitCommands;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace GitUI.Infrastructure.Telemetry
{
    internal class AppEnvironmentTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            string sshClient;
            var sshPath = AppSettings.SshPath;
            if (string.IsNullOrEmpty(sshPath))
            {
                sshClient = "OpenSSH";
            }
            else if (GitSshHelpers.IsPlink)
            {
                sshClient = "PuTTY";
            }
            else
            {
                sshClient = "Other";
            }

            telemetry.Context.GlobalProperties["Git"] = GitVersion.Current.ToString();
            telemetry.Context.GlobalProperties["SSH"] = sshClient;
            telemetry.Context.GlobalProperties["SSH.Path"] = sshPath;
        }
    }
}
