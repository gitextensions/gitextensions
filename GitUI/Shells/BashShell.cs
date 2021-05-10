using System;
using System.IO;
using GitCommands;
using GitUI.Properties;

namespace GitUI.Shells
{
    public class BashShell : ShellDescriptor
    {
        private const string BashExe = "bash.exe"; // Generic bash, should generally be in the git dir
        private const string ShExe = "sh.exe";     // Fallback to SH
        public const string ShellName = "bash";

        public BashShell()
        {
            Name = ShellName;
            Icon = Images.GitForWindows;

            ExecutableName = BashExe;
            if (PathUtil.TryFindShellPath(ExecutableName, out var exePath))
            {
                ExecutablePath = exePath;
                ExecutableCommandLine = $"{exePath.Quote()} --login -i";
            }
            else if (PathUtil.TryFindShellPath(ShExe, out exePath))
            {
                ExecutableName = ShExe;
                ExecutablePath = exePath;
                ExecutableCommandLine = $"{exePath.Quote()} --login -i";
            }
        }

        public override string GetChangeDirCommand(string path)
        {
            try
            {
                DirectoryInfo directoryInfo = new(path);
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
