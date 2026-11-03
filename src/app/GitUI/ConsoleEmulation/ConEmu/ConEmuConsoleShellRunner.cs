using System.Globalization;
using ConEmu.WinForms;
using GitCommands;
using GitUI.ConsoleEmulation;
using GitUI.Shells;
using Microsoft;

namespace GitUI.ConsoleEmulation.ConEmu;

/// <summary>
///  Wraps <see cref="ConEmuControl"/> for the repository browser's terminal tab.
/// </summary>
internal sealed class ConEmuConsoleShellRunner(IShellProvider shellProvider, ConsoleEmulatorSettings settings) : IConsoleShellRunner
{
    private readonly ConEmuControl _conEmu = new()
    {
        Dock = DockStyle.Fill,
        IsStatusbarVisible = false
    };

    public Control Control => _conEmu;

    public bool IsShellRunning => _conEmu.IsConsoleEmulatorOpen;

    public void ChangeWorkingDirectory(string path)
    {
        if (_conEmu.RunningSession is not { } session || string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        string? shellType = AppSettings.ConEmuTerminal;
        IShellDescriptor shell = shellProvider.GetShell(shellType);
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

    public void FocusTerminal()
    {
        _conEmu.Focus();
    }

    public void StartShell(string workDir)
    {
        if (_conEmu.IsConsoleEmulatorOpen)
        {
            FocusTerminal();
            return;
        }

        ConEmuStartInfo startInfo = new()
        {
            StartupDirectory = workDir,
            WhenConsoleProcessExits = WhenConsoleProcessExits.CloseConsoleEmulator
        };

        string? shellType = AppSettings.ConEmuTerminal;
        startInfo.ConsoleProcessCommandLine = shellProvider.GetShellCommandLine(shellType);

        if (!string.IsNullOrEmpty(AppSettings.GitCommandValue))
        {
            string? dirGit = Path.GetDirectoryName(AppSettings.GitCommandValue);
            if (!string.IsNullOrEmpty(dirGit))
            {
                startInfo.SetEnv("PATH", $"{dirGit};%PATH%");
            }
        }

        try
        {
            Validates.NotNull(settings.Font);

            _conEmu.Start(
                startInfo,
                ThreadHelper.JoinableTaskFactory,
                settings.Theme,
                settings.Font.Name,
                settings.Font.Size.ToString("F0", CultureInfo.InvariantCulture));
        }
        catch (InvalidOperationException)
        {
#if DEBUG
            MessageBoxes.ShowError(_conEmu, "ConEmu appears to be missing. Please perform a full rebuild and try again.");
#else
            throw;
#endif
        }
    }
}
