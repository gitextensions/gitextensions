using System.Diagnostics;
using GitCommands.Logging;
using Windows.Win32.Foundation;

namespace GitUI.ConsoleEmulation.Mintty;

internal sealed class MinttySession
{
    private Action<string>? _lineCallback;
    private Action<int>? _exitCallback;

    public ProcessOperation? ProcessOperation { get; private set; }

    public HWND WindowHandle { get; set; }

    public Process? MinttyProcess { get; private set; }

    public int? ExitCode { get; private set; }

    private MinttySession()
    {
    }

    public static (MinttySession Session, MinttyConsoleRuntime.CommandLaunchParams LaunchParams) StartCommandSession(MinttyStartInfo startInfo)
    {
        MinttyConsoleRuntime.CommandLaunchParams launchParams = MinttyConsoleRuntime.CreateLaunchParams(startInfo);

        MinttySession session = new()
        {
            ProcessOperation = startInfo.ProcessOperation,
            _lineCallback = startInfo.AnsiOutputLineCallback,
            _exitCallback = startInfo.ProcessExitedCallback,
        };

        return (session, launchParams);
    }

    public static MinttySession StartInteractiveShellSession()
    {
        return new MinttySession();
    }

    public bool IsExited
    {
        get
        {
            try
            {
                return MinttyProcess is null || MinttyProcess.HasExited;
            }
            catch
            {
                return true;
            }
        }
    }

    public void AttachProcess(Process minttyProcess)
    {
        MinttyProcess = minttyProcess;
        ProcessOperation?.SetProcessId(minttyProcess.Id);

        // The interactive shell launches with stdout not redirected; reading
        // Process.StandardOutput in that case throws InvalidOperationException.
        if (minttyProcess.StartInfo.RedirectStandardOutput)
        {
            MinttyConsoleRuntime.StartOutputReader(
                minttyProcess,
                _lineCallback,
                exitCallback: exitCode =>
                {
                    ExitCode = exitCode;
                    _exitCallback?.Invoke(exitCode);
                });
        }
    }

    public void Kill()
    {
        try
        {
            if (MinttyProcess is not null && !MinttyProcess.HasExited)
            {
                MinttyProcess.Kill();
            }
        }
        catch
        {
        }
    }
}
