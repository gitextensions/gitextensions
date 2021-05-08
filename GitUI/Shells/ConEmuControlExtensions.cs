using System;
using ConEmu.WinForms;
using GitExtUtils;

namespace GitUI.Shells
{
    public static class ConEmuControlExtensions
    {
        public static void ChangeFolder(this ConEmuControl? terminal, IShellDescriptor? shell, string? path)
        {
            if (terminal?.RunningSession is null || shell is null || string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            string command = shell.GetChangeDirCommand(path);

            switch (shell.Name)
            {
                case BashShell.ShellName:
                    terminal.RunningSession.BeginGuiMacro("Keys").WithParam("^A").WithParam("^K").ExecuteSync();
                    terminal.RunningSession.WriteInputTextAsync(command + Environment.NewLine);
                    break;

                default:
                    terminal.RunningSession.WriteInputTextAsync($"\x1B{command}{Environment.NewLine}");
                    break;
            }
        }
    }
}
