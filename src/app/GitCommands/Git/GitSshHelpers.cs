namespace GitCommands
{
    public static class GitSshHelpers
    {
        /// <summary>Sets the git SSH command path.</summary>
        public static void SetGitSshEnvironmentVariable(string path)
        {
            // Git will use the embedded OpenSSH ssh.exe if empty/unset
            Environment.SetEnvironmentVariable("GIT_SSH", path, EnvironmentVariableTarget.Process);
        }

        // Note that variants like TortoisePlink.exe are supported too
        public static bool IsPlink => AppSettings.SshPath.EndsWith("plink.exe", StringComparison.CurrentCultureIgnoreCase);
    }
}
