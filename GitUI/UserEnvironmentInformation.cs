using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitExtUtils;
using GitExtUtils.GitUI;

namespace GitUI
{
    public static class UserEnvironmentInformation
    {
        private static readonly string DOTNET_CMD = "dotnet";
        private static bool _alreadySet;
        private static bool _dirty;
        private static string? _sha;

        public static void CopyInformation() => ClipboardUtil.TrySetText(GetInformation() + GetDotnetVersionInfo());

        public static string GetInformation()
        {
            if (!_alreadySet)
            {
                throw new InvalidOperationException($"{nameof(Initialise)} must be called first");
            }

            string? gitVer;
            try
            {
                gitVer = GitVersion.Current?.Full;
            }
            catch (Exception)
            {
                gitVer = null;
            }

            var gitVersionInfo = GetGitVersionInfo(gitVer, GitVersion.LastSupportedVersion, GitVersion.LastRecommendedVersion);

            // Build and open FormAbout design to make sure info still looks good if you change this code.
            StringBuilder sb = new();

            sb.AppendLine($"- Git Extensions {AppSettings.ProductVersion}");
            sb.AppendLine($"- Build {_sha}{(_dirty ? " (Dirty)" : "")}");
            sb.AppendLine($"- Git {gitVersionInfo}");
            sb.AppendLine($"- {Environment.OSVersion}");
            sb.AppendLine($"- {RuntimeInformation.FrameworkDescription}");
            sb.AppendLine($"- DPI {DpiUtil.DpiX}dpi ({(DpiUtil.ScaleX == 1 ? "no" : $"{Math.Round(DpiUtil.ScaleX * 100)}%")} scaling)");
            sb.AppendLine($"- Portable: {AppSettings.IsPortable()}");

            return sb.ToString();
        }

        public static string GetGitVersionInfo(string? gitVersion, GitVersion lastSupportedVersion, GitVersion recommendedVersion)
        {
            if (string.IsNullOrWhiteSpace(gitVersion))
            {
                return $"- (minimum: {lastSupportedVersion}, recommended: {recommendedVersion})";
            }

            GitVersion actualVersion = new(gitVersion);
            if (actualVersion < lastSupportedVersion)
            {
                return $"{gitVersion} (minimum: {lastSupportedVersion}, please update!)";
            }

            if (actualVersion < recommendedVersion)
            {
                return $"{gitVersion} (recommended: {recommendedVersion} or later)";
            }

            return gitVersion;
        }

        private static string GetDotnetVersionInfo()
        {
            StringBuilder sb = new();
            Executable dotnet = new(DOTNET_CMD);
            ArgumentString args = new ArgumentBuilder()
            {
                "--list-runtimes"
            };

            sb.AppendLine("- Microsoft.WindowsDesktop.App Versions");
            sb.AppendLine();
            sb.AppendLine("```");
            try
            {
                string output = dotnet.GetOutput(args);
                var desktopAppMatches = Regex.Matches(output, @"^(?=.*\bMicrosoft\.WindowsDesktop\.App\b)[^\n\r]*", RegexOptions.Multiline).Cast<Match>();
                var desktopAppLines = string.Join(Environment.NewLine, desktopAppMatches);

                desktopAppLines = Regex.Replace(desktopAppLines, "^", "    ", RegexOptions.Multiline);
                sb.AppendLine($"{desktopAppLines}");
            }
            catch (Exception ex)
            {
                sb.AppendLine(Regex.Replace(ex.Message, "^", "    ", RegexOptions.Multiline));
            }
            finally
            {
                sb.AppendLine("```");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static void Initialise(string sha, bool isDirty)
        {
            if (_alreadySet)
            {
                return;
            }

            _alreadySet = true;
            _sha = sha;
            _dirty = isDirty;
        }
    }
}
