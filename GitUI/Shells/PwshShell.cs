using System;
using GitCommands;
using GitUI.Properties;

namespace GitUI.Shells
{
    public class PwshShell : ShellDescriptor
    {
        public PwshShell()
        {
            Name = "pwsh";
            Icon = Images.pwsh;

            ExecutableName = "pwsh.exe";
            if (PathUtil.TryFindShellPath(ExecutableName, out var exePath))
            {
                ExecutablePath = exePath;
                ExecutableCommandLine = exePath.Quote();
            }
        }

        public override string GetChangeDirCommand(string path) => $"cd {path.QuoteNE()}";
    }
}
