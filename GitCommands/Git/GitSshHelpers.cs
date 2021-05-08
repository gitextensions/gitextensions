using System;
using GitExtUtils;

namespace GitCommands
{
    public static class GitSshHelpers
    {
        private static readonly ISshPathLocator _sshPathLocatorInstance = new SshPathLocator();

        public static bool UseSsh(string arguments)
        {
            var x = !Plink() && DoArgumentsRequireSsh();
            return x || arguments.Contains("plink");

            bool DoArgumentsRequireSsh()
            {
                return (arguments.Contains("@") && arguments.Contains("://")) ||
                       (arguments.Contains("@") && arguments.Contains(":")) ||
                       arguments.Contains("ssh://") ||
                       arguments.Contains("http://") ||
                       arguments.Contains("git://") ||
                       arguments.Contains("push") ||
                       arguments.Contains("remote") ||
                       arguments.Contains("fetch") ||
                       arguments.Contains("pull");
            }
        }

        /// <summary>Un-sets the git SSH command path.</summary>
        public static void UnsetSsh()
        {
            Environment.SetEnvironmentVariable("GIT_SSH", "", EnvironmentVariableTarget.Process);
        }

        /// <summary>Sets the git SSH command path.</summary>
        public static void SetSsh(string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Environment.SetEnvironmentVariable("GIT_SSH", path, EnvironmentVariableTarget.Process);
            }
        }

        public static bool Plink()
        {
            var sshString = _sshPathLocatorInstance.Find(AppSettings.GitBinDir);

            return sshString.EndsWith("plink.exe", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
