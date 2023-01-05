using ConEmu.WinForms;

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
                    // Use a ConEmu macro to send the sequence for clearing the bash command line
                    terminal.RunningSession.BeginGuiMacro("Keys").WithParam("^A").WithParam("^K").ExecuteSync();
                    terminal.RunningSession.WriteInputTextAsync(command);
                    break;

                default:
                    terminal.RunningSession.WriteInputTextAsync($"\x1B{command}");
                    break;
            }

            terminal.RunningSession.BeginGuiMacro("Keys").WithParam("Enter").ExecuteSync();
        }
    }
}
