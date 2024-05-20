using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtUtils;
using GitExtUtils.GitUI;

namespace GitUI
{
    public static partial class UserEnvironmentInformation
    {
        private static readonly string DOTNET_CMD = "dotnet";
        private static bool _alreadySet;
        private static bool _dirty;
        private static string? _sha;

        [GeneratedRegex(@"^(?=.*\bMicrosoft\.WindowsDesktop\.App\b)[^\n\r]*", RegexOptions.Multiline | RegexOptions.ExplicitCapture)]
        private static partial Regex DesktopAppRegex();
        [GeneratedRegex(@"^", RegexOptions.Multiline | RegexOptions.ExplicitCapture)]
        private static partial Regex LineStartRegex();

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
                gitVer = GitVersion.Current?.ToString();
            }
            catch (Exception)
            {
                gitVer = null;
            }

            string gitVersionInfo = GetGitVersionInfo(gitVer, GitVersion.LastSupportedVersion, GitVersion.LastRecommendedVersion);

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
                IEnumerable<Match> desktopAppMatches = DesktopAppRegex().Matches(output).Cast<Match>();
                string desktopAppLines = string.Join(Environment.NewLine, desktopAppMatches);

                desktopAppLines = LineStartRegex().Replace(desktopAppLines, "    ");
                sb.AppendLine($"{desktopAppLines}");
            }
            catch (Exception ex)
            {
                sb.AppendLine(LineStartRegex().Replace(ex.Message, "    "));
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
