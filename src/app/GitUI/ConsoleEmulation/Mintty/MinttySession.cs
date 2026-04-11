using System.Diagnostics;
using Windows.Win32.Foundation;

namespace GitUI.ConsoleEmulation.Mintty;

internal sealed class MinttySession
{
    private Process? _minttyProcess;
    private Action<string>? _lineCallback;
    private Action<int>? _exitCallback;

    private MinttySession()
    {
    }

    internal static (MinttySession Session, MinttyConsoleRuntime.CommandLaunchParams LaunchParams) StartCommandSession(MinttyStartInfo startInfo)
    {
        MinttyConsoleRuntime.CommandLaunchParams launchParams = MinttyConsoleRuntime.CreateLaunchParams(startInfo);

        MinttySession session = new()
        {
            _lineCallback = startInfo.AnsiOutputLineCallback,
            _exitCallback = startInfo.ProcessExitedCallback,
        };

        return (session, launchParams);
    }

    internal static MinttySession StartInteractiveShellSession()
    {
        return new MinttySession();
    }

    internal Process? MinttyProcess => _minttyProcess;

    internal HWND WindowHandle { get; set; }

    internal bool IsExited
    {
        get
        {
            try
            {
                return _minttyProcess is null || _minttyProcess.HasExited;
            }
            catch
            {
                return true;
            }
        }
    }

    internal void AttachProcess(Process minttyProcess, CancellationToken ct)
    {
        _minttyProcess = minttyProcess;

        // The interactive shell launches with stdout not redirected; reading
        // Process.StandardOutput in that case throws InvalidOperationException.
        if (minttyProcess.StartInfo.RedirectStandardOutput)
        {
            MinttyConsoleRuntime.StartOutputReader(minttyProcess, _lineCallback, _exitCallback, ct);
        }
    }

    internal void Kill()
    {
        try
        {
            if (_minttyProcess is not null && !_minttyProcess.HasExited)
            {
                _minttyProcess.Kill();
            }
        }
        catch
        {
        }
    }
}
