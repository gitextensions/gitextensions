using System.Diagnostics;
using GitCommands.Logging;
using Windows.Win32.Foundation;

namespace GitUI.ConsoleEmulation.Mintty;

internal sealed class MinttySession : IDisposable
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
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
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

    /// <summary>
    ///  Deliberately aborts the session and reports the command as failed.
    ///  Use <see cref="Dispose"/> for a silent teardown without exit notification.
    /// </summary>
    public void Kill()
    {
        if (IsExited)
        {
            return;
        }

        KillProcess();

        if (ExitCode is null)
        {
            // Killing the host means the exit sentinel never arrives, so no exit
            // notification would fire and the aborted command would be mistaken
            // for a successful one. Report a non-zero exit code, like ConEmu does
            // when the command is interrupted.
            ExitCode = -1;
            _exitCallback?.Invoke(-1);
        }
    }

    private void KillProcess()
    {
        try
        {
            if (MinttyProcess is not null && !MinttyProcess.HasExited)
            {
                MinttyProcess.Kill();
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    // Kills the process and releases its handle. The output reader (if any) holds the
    // stdout stream; killing closes the pipe so it drains and exits, and any read still
    // in flight when the Process is disposed fails into the reader's own try/catch.
    public void Dispose()
    {
        KillProcess();
        MinttyProcess?.Dispose();
        MinttyProcess = null;
    }
}
