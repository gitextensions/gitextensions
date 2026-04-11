using System.Globalization;
using ConEmu.WinForms;
using GitCommands;
using GitExtensions.Extensibility.Plugins;
using GitUI.Shells;

namespace GitUI.UserControls;

/// <summary>
/// Implements <see cref="IConsoleShellController"/> by wrapping a <see cref="ConEmuControl"/>.
/// Used for the interactive terminal tab in the repository browser when no plugin is available.
/// </summary>
internal sealed class ConEmuConsoleShellController : IConsoleShellController
{
    private readonly ConEmuControl _conEmu;
    private readonly ShellProvider _shellProvider = new();

    internal ConEmuConsoleShellController()
    {
        _conEmu = new ConEmuControl
        {
            Dock = DockStyle.Fill,
            IsStatusbarVisible = false
        };
    }

    /// <inheritdoc/>
    public Control Control => _conEmu;

    public bool SupportsChangingWorkingDirectory => true;

    /// <inheritdoc/>
    public bool IsShellRunning => _conEmu.IsConsoleEmulatorOpen;

    /// <inheritdoc/>
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
                startInfo.SetEnv("PATH", dirGit + ";" + "%PATH%");
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
            MessageBoxes.Show(_conEmu, "ConEmu appears to be missing. Please perform a full rebuild and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
#else
            throw;
#endif
        }
    }

    public void ChangeWorkingDirectory(string path)
    {
        string? shellType = AppSettings.ConEmuTerminal.Value;
        IShellDescriptor shell = _shellProvider.GetShell(shellType);
        _conEmu.ChangeFolder(shell, path);
    }

    /// <inheritdoc/>
    public void FocusTerminal()
    {
        _conEmu.Focus();
    }
}
