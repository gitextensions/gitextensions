using GitCommands;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace GitUI.Infrastructure.Telemetry
{
    internal class AppEnvironmentTelemetryInitializer : ITelemetryInitializer
    {
        private readonly ISshPathLocator _sshPathLocator = new SshPathLocator();

        public void Initialize(ITelemetry telemetry)
        {
            string sshClient;
            var sshPath = _sshPathLocator.Find(AppSettings.GitBinDir);
            if (string.IsNullOrEmpty(sshPath))
            {
                sshClient = "OpenSSH";
            }
            else if (GitCommandHelpers.Plink())
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
