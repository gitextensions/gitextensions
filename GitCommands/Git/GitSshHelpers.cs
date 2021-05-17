using System;
using System.IO;

namespace GitCommands
{
    public static class GitSshHelpers
    {
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

        /// <summary>Sets the git SSH command path.</summary>
        public static void SetSsh(string path)
        {
            // Git will use the embedded OpenSSH ssh.exe if empty/unset
            if (!string.IsNullOrEmpty(path) && !File.Exists(path))
            {
                path = "";
            }

            Environment.SetEnvironmentVariable("GIT_SSH", path, EnvironmentVariableTarget.Process);
        }

        public static bool Plink()
            => AppSettings.SshPath.EndsWith("plink.exe", StringComparison.CurrentCultureIgnoreCase);
    }
}
