using System.Globalization;
using ConEmu.WinForms;
using GitCommands;
using GitUI.Shells;

namespace GitUI.ConsoleEmulation.ConEmu;

/// <summary>
///  Wraps <see cref="ConEmuControl"/> for the repository browser's terminal tab.
/// </summary>
internal sealed class ConEmuConsoleShellRunner : IConsoleShellRunner
{
    private readonly ConEmuControl _conEmu;
    private readonly ShellProvider _shellProvider = new();

    internal ConEmuConsoleShellRunner()
    {
        _conEmu = new ConEmuControl
        {
            Dock = DockStyle.Fill,
            IsStatusbarVisible = false
        };
    }

    public Control Control => _conEmu;

    public bool IsShellRunning => _conEmu.IsConsoleEmulatorOpen;

    public void ChangeWorkingDirectory(string path)
    {
        string? shellType = AppSettings.ConEmuTerminal.Value;
        IShellDescriptor shell = _shellProvider.GetShell(shellType);
        _conEmu.ChangeFolder(shell, path);
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

        string? shellType = AppSettings.ConEmuTerminal.Value;
        startInfo.ConsoleProcessCommandLine = _shellProvider.GetShellCommandLine(shellType);

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
            _conEmu.Start(
                startInfo,
                ThreadHelper.JoinableTaskFactory,
                AppSettings.GetEffectiveConEmuStyle(),
                AppSettings.ConEmuConsoleFont.Name,
                AppSettings.ConEmuConsoleFont.Size.ToString("F0", CultureInfo.InvariantCulture));
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
