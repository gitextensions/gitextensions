using System;
using GitCommands;
using GitUI.Properties;

namespace GitUI.Shells
{
    public class CmdShell : ShellDescriptor
    {
        public CmdShell()
        {
            Name = "cmd";
            Icon = Images.cmd;

            ExecutableName = "cmd.exe";
            if (PathUtil.TryFindShellPath(ExecutableName, out var exePath))
            {
                ExecutablePath = exePath;
                ExecutableCommandLine = exePath.Quote();
            }
        }

        public override string GetChangeDirCommand(string path) => $"cd /D {path.QuoteNE()}";
    }
}
