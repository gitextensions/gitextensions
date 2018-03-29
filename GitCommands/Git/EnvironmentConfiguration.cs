using System;
using System.IO;
using GitCommands.Utils;
using JetBrains.Annotations;

namespace GitCommands
{
    public static class EnvironmentConfiguration
    {
        private static readonly IEnvironmentAbstraction Env = new EnvironmentAbstraction();

        /// <summary>
        /// The <c>USER</c> environment variable's value for the user/machine.
        /// </summary>
        [CanBeNull]
        private static readonly string UserHomeDir
            = Env.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User)
           ?? Env.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.Machine);

        /// <summary>
        /// Sets <c>PATH</c>, <c>HOME</c>, <c>TERM</c> and <c>SSH_ASKPASS</c> environment variables
        /// for the current process.
        /// </summary>
        public static void SetEnvironmentVariables()
        {
            // PATH variable

            if (!string.IsNullOrEmpty(AppSettings.GitBinDir))
            {
                // Ensure the git binary dir is on the path
                string path = Env.GetEnvironmentVariable("PATH");

                if (path == null)
                {
                    Env.SetEnvironmentVariable("PATH", AppSettings.GitBinDir);
                }
                else if (!path.Contains(AppSettings.GitBinDir))
                {
                    Env.SetEnvironmentVariable("PATH", $"{path}{Path.PathSeparator}{AppSettings.GitBinDir}");
                }
            }

            // HOME variable
            Env.SetEnvironmentVariable("HOME", ComputeHomeLocation());

            // TERM variable

            // to prevent from leaking processes see issue #1092 for details
            Env.SetEnvironmentVariable("TERM", "msys");

            // SSH_ASKPASS variable

            if (EnvUtils.RunningOnWindows())
            {
                string sshAskPass = Path.Combine(AppSettings.GetInstallDir(), "GitExtSshAskPass.exe");

                if (File.Exists(sshAskPass))
                {
                    Env.SetEnvironmentVariable("SSH_ASKPASS", sshAskPass);
                }
            }
            else if (string.IsNullOrEmpty(Env.GetEnvironmentVariable("SSH_ASKPASS")))
            {
                Env.SetEnvironmentVariable("SSH_ASKPASS", "ssh-askpass");
            }

            return;

            string ComputeHomeLocation()
            {
                if (!string.IsNullOrEmpty(AppSettings.CustomHomeDir))
                {
                    return AppSettings.CustomHomeDir;
                }

                if (AppSettings.UserProfileHomeDir)
                {
                    return Env.GetEnvironmentVariable("USERPROFILE");
                }

                return GetDefaultHomeDir();
            }
        }

        /// <summary>
        /// Gets the value of the current process's <c>HOME</c> environment variable.
        /// </summary>
        /// <returns>The variable's value, or an empty string if it is not present.</returns>
        [NotNull]
        public static string GetHomeDir()
        {
            return Env.GetEnvironmentVariable("HOME") ?? "";
        }

        [CanBeNull]
        public static string GetDefaultHomeDir()
        {
            // Use the HOME property from the user or machine, as captured at startup
            if (!string.IsNullOrEmpty(UserHomeDir))
            {
                return UserHomeDir;
            }

            if (EnvUtils.RunningOnWindows())
            {
                // Use the Windows default home directory
                var homeDrive = Env.GetEnvironmentVariable("HOMEDRIVE");

                if (!string.IsNullOrEmpty(homeDrive))
                {
                    return homeDrive + Env.GetEnvironmentVariable("HOMEPATH");
                }

                return Env.GetEnvironmentVariable("USERPROFILE");
            }

            return Env.GetFolderPath(Environment.SpecialFolder.Personal);
        }
    }
}