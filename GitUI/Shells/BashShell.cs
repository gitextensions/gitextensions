#nullable enable

using System;
using System.IO;
using GitCommands;
using GitUI.Properties;

namespace GitUI.Shells
{
    public class BashShell : ShellDescriptor
    {
        private const string GitBashExe = "git-bash.exe"; // Bash with git in the path, should generally be in the git dir
        private const string BashExe = "bash.exe"; // Fallback to generic bash, should generally be in the git bin dir
        private const string ShExe = "sh.exe";     // Fallback to SH
        public const string ShellName = "bash";

        public BashShell()
        {
            Name = ShellName;
            Icon = Images.GitForWindows;

            if (PathUtil.TryFindShellPath(GitBashExe, out var exePath))
            {
                ExecutableName = GitBashExe;
                ExecutablePath = exePath;

                // Try to find bash or sh below to set ExecutableCommandLine, as git-bash.exe cannot be connected to the built-in console.
            }

            if (PathUtil.TryFindShellPath(BashExe, out exePath))
            {
                if (ExecutablePath is null)
                {
                    ExecutableName = BashExe;
                    ExecutablePath = exePath;
                }

                ExecutableCommandLine = $"{exePath.Quote()} --login -i";
            }
            else if (PathUtil.TryFindShellPath(ShExe, out exePath))
            {
                if (ExecutablePath is null)
                {
                    ExecutableName = ShExe;
                    ExecutablePath = exePath;
                }

                ExecutableCommandLine = $"{exePath.Quote()} --login -i";
            }
        }

        public override string GetChangeDirCommand(string path)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(path);
                if (directoryInfo.Exists)
                {
                    string posixPath = "/" + directoryInfo.FullName.ToPosixPath().Remove(1, 1);
                    return $"cd {posixPath.QuoteNE()}";
                }
            }
            catch
            {
                // no-op
            }

            return string.Empty;
        }
    }
}
