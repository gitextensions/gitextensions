using ConEmu.WinForms;

namespace GitUI.Shells;

public static class ConEmuControlExtensions
{
    public static void ChangeFolder(this ConEmuControl? terminal, IShellDescriptor? shell, string? path)
    {
        if (terminal?.RunningSession is not { } session || shell is null || string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        string command = shell.GetChangeDirCommand(path);
        if (string.IsNullOrWhiteSpace(command))
        {
            // Submitting the line without a command to run would execute whatever the user has typed so far
            return;
        }

        switch (shell.Name)
        {
            case BashShell.ShellName:
                // Use a ConEmu macro to send the sequence for clearing the bash command line
                session.BeginGuiMacro("Keys").WithParam("^A").WithParam("^K").ExecuteSync();
                WriteInput(command);
                break;

            default:
                WriteInput($"\x1B{command}");
                break;
        }

        session.BeginGuiMacro("Keys").WithParam("Enter").ExecuteSync();

        return;

        // Writing the input is a ConEmu macro of its own, and each macro is dispatched on a separate
        // thread pool work item. Fire and forget would therefore allow the Enter above to be executed
        // first, leaving the command in the prompt to be executed later together with whatever the
        // user types next.
        void WriteInput(string text) => ThreadHelper.JoinableTaskFactory.Run(() => session.WriteInputTextAsync(text));
    }
}
