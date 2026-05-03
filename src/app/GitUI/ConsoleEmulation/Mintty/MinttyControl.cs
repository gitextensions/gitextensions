using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace GitUI.ConsoleEmulation.Mintty;

internal sealed class MinttyControl : Panel
{
    private MinttySession? _runningSession;
    private CancellationTokenSource? _sessionCts;
    private HANDLE _jobHandle = NativeMethods.CreateKillOnCloseJob();

    internal MinttySession? RunningSession => _runningSession;

    internal bool IsShellRunning => _runningSession is not null && !_runningSession.IsExited;

    public MinttySession StartCommand(MinttyStartInfo startInfo, string minttyPath, string bashPath, string? theme)
    {
        ResetSession();

        CancellationTokenSource sessionCts = _sessionCts!;
        CancellationToken ct = sessionCts.Token;

        (MinttySession session, MinttyConsoleRuntime.CommandLaunchParams launchParams) = MinttySession.StartCommandSession(startInfo);
        _runningSession = session;

        string minttyArgs = $"{BuildThemeArg(theme)}--nodaemon --window hide --log - \"{bashPath}\" -c \"{launchParams.BashBootstrapCommand}\"";

        LaunchAndEmbed(session, minttyPath, minttyArgs, startInfo.StartupDirectory, launchParams.EnvironmentVariables, ct, redirectStdout: true);

        session.MinttyProcess!.Exited += (_, _) =>
        {
            // If ResetSession already cancelled this CTS, skip the callback —
            // the old session was intentionally killed, not a host termination.
            if (!sessionCts.IsCancellationRequested)
            {
                sessionCts.Cancel();
                startInfo.ConsoleClosedCallback?.Invoke();
            }
        };

        return session;
    }

    public MinttySession StartInteractiveShell(string minttyPath, string bashPath, string? theme, string workDir, Action? shellExitedCallback = null)
    {
        ResetSession();
        CancellationTokenSource sessionCts = _sessionCts!;
        CancellationToken ct = sessionCts.Token;

        string minttyArgs = $"{BuildThemeArg(theme)}--nodaemon --window hide \"{bashPath}\" --login -i";

        MinttySession session = MinttySession.StartInteractiveShellSession();
        _runningSession = session;

        LaunchAndEmbed(session, minttyPath, minttyArgs, workDir, new Dictionary<string, string>(), ct);

        session.MinttyProcess!.Exited += (_, _) =>
        {
            // If ResetSession already cancelled this CTS, skip the callback —
            // the old session was intentionally killed, not a host termination.
            if (!sessionCts.IsCancellationRequested)
            {
                sessionCts.Cancel();
                shellExitedCallback?.Invoke();
            }
        };

        return session;
    }

    public void SendConsoleInput(string text)
    {
        HWND hwnd = _runningSession?.WindowHandle ?? HWND.Null;
        if (hwnd.IsNull)
        {
            return;
        }

        FocusWithAttachedInput(hwnd);

        foreach (char c in text)
        {
            if (c is '\r' or '\n')
            {
                PInvoke.PostMessage(hwnd, NativeMethods.WM_KEYDOWN, (nuint)VIRTUAL_KEY.VK_RETURN, (nint)0);
                PInvoke.PostMessage(hwnd, NativeMethods.WM_CHAR, (nuint)'\r', (nint)0);
                PInvoke.PostMessage(hwnd, NativeMethods.WM_KEYUP, (nuint)VIRTUAL_KEY.VK_RETURN, (nint)0);
            }
            else
            {
                PInvoke.PostMessage(hwnd, NativeMethods.WM_CHAR, (nuint)c, (nint)0);
            }
        }
    }

    public void FocusConsole()
    {
        HWND hwnd = _runningSession?.WindowHandle ?? HWND.Null;
        if (!hwnd.IsNull)
        {
            FocusWithAttachedInput(hwnd);
        }
    }

    private static string BuildThemeArg(string? theme)
    {
        return string.IsNullOrEmpty(theme) ? "" : $"-o \"ThemeFile={theme}\" ";
    }

    private void ResetSession()
    {
        _sessionCts?.Cancel();
        _sessionCts?.Dispose();
        _sessionCts = new CancellationTokenSource();
        _runningSession?.Kill();
        _runningSession = null;
    }

    private void LaunchAndEmbed(
        MinttySession session,
        string minttyPath,
        string minttyArgs,
        string? workDir,
        IReadOnlyDictionary<string, string> envVariables,
        CancellationToken ct,
        bool redirectStdout = false)
    {
        ProcessStartInfo psi = new()
        {
            FileName = minttyPath,
            Arguments = minttyArgs,
            WorkingDirectory = workDir ?? Environment.CurrentDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = redirectStdout,
            CreateNoWindow = true,
        };

        foreach ((string key, string value) in envVariables)
        {
            psi.EnvironmentVariables[key] = value;
        }

        Process minttyProcess = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start mintty.exe");

        if (!_jobHandle.IsNull)
        {
            PInvoke.AssignProcessToJobObject(_jobHandle, new HANDLE(minttyProcess.Handle));
        }

        minttyProcess.EnableRaisingEvents = true;

        session.AttachProcess(minttyProcess, ct);

        FindAndEmbedWindow(minttyProcess, session, ct);
    }

#pragma warning disable VSTHRD100
    private async void FindAndEmbedWindow(Process minttyProcess, MinttySession session, CancellationToken ct)
#pragma warning restore VSTHRD100
    {
        using CancellationTokenSource timeoutCts = new(TimeSpan.FromSeconds(30));
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
        ct = linkedCts.Token;

        try
        {
            // Wait until mintty's message pump is idle before embedding — otherwise
            // mintty may still be running startup code that calls ShowWindow/SetFocus
            // and causes the host dialog's focus to flicker as it races with our embed.
            await Task.Run(() => minttyProcess.WaitForInputIdle(5000), ct).ConfigureAwait(true);

            HWND hwnd = HWND.Null;

            while (!ct.IsCancellationRequested)
            {
                hwnd = NativeMethods.FindMinttyWindowForProcess(minttyProcess.Id);
                if (!hwnd.IsNull)
                {
                    break;
                }

                // It is essential to capture original synchronization context
                await Task.Delay(TimeSpan.FromMilliseconds(50), ct).ConfigureAwait(true);
            }

            session.WindowHandle = hwnd;

            if (!hwnd.IsNull)
            {
                EmbedWindow(hwnd);
            }
        }
        catch (Exception)
        {
           session.Kill();
        }
    }

    private void EmbedWindow(HWND hwnd)
    {
        if (!IsHandleCreated || hwnd.IsNull)
        {
            return;
        }

        // Probe mintty's pump before any blocking cross-process call. SetWindowLong
        // on GWL_STYLE/GWL_EXSTYLE synchronously sends WM_STYLECHANGING/CHANGED to
        // mintty's thread; SetParent and AttachThreadInput are similarly synchronous.
        // Under system load mintty's pump can stall — without this probe the host
        // UI thread blocks indefinitely inside one of those calls. If the probe
        // fails, kill mintty so the session resets cleanly via Process.Exited.
        if (!IsWindowResponsive(hwnd, TimeSpan.FromSeconds(5)))
        {
            _runningSession?.Kill();
            return;
        }

        // Capture the hosting form before embedding: mintty's process startup can
        // shift foreground/activation away from the dialog that triggered the command,
        // so we need a handle to restore activation after the embed completes.
        Form? hostForm = FindForm();

        WINDOW_STYLE style = (WINDOW_STYLE)PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
        style &= ~(WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_THICKFRAME | WINDOW_STYLE.WS_BORDER);
        style |= WINDOW_STYLE.WS_CHILD;
        PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)style);

        WINDOW_EX_STYLE extendedStyle = (WINDOW_EX_STYLE)PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        extendedStyle &= ~(WINDOW_EX_STYLE.WS_EX_CLIENTEDGE | WINDOW_EX_STYLE.WS_EX_WINDOWEDGE | WINDOW_EX_STYLE.WS_EX_APPWINDOW);
        extendedStyle |= WINDOW_EX_STYLE.WS_EX_TOOLWINDOW;
        PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)extendedStyle);

        PInvoke.SetParent(hwnd, new HWND(Handle));

        // SWP_ASYNCWINDOWPOS posts the size/show change to mintty's thread instead of
        // a synchronous cross-process SendMessage, so a slow mintty pump under system
        // load cannot freeze our UI thread during embed. SWP_SHOWWINDOW replaces the
        // separate ShowWindow call (which has no async variant) — combined with
        // SWP_NOACTIVATE this is equivalent to SW_SHOWNA. See OnResize for the same
        // pattern.
        PInvoke.SetWindowPos(hwnd, HWND.Null, 0, 0, Width, Height,
            SET_WINDOW_POS_FLAGS.SWP_NOZORDER
            | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
            | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
            | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW
            | SET_WINDOW_POS_FLAGS.SWP_ASYNCWINDOWPOS);

        if (hostForm is { IsHandleCreated: true, IsDisposed: false })
        {
            hostForm.Activate();
        }

        FocusWithAttachedInput(hwnd);
    }

    /// <summary>
    /// Pings the target window with WM_NULL via SendMessageTimeout to verify its
    /// message pump is alive. SMTO_ABORTIFHUNG returns immediately if the OS has
    /// already marked the window as hung; otherwise the call returns once the pump
    /// dispatches the no-op message or the timeout expires. Returns false on
    /// failure — caller must not proceed with synchronous cross-process calls.
    /// </summary>
    private static bool IsWindowResponsive(HWND hwnd, TimeSpan timeout)
    {
        nint result = PInvoke.SendMessageTimeout(
            hwnd,
            Msg: 0, // WM_NULL
            wParam: default,
            lParam: default,
            SEND_MESSAGE_TIMEOUT_FLAGS.SMTO_ABORTIFHUNG,
            (uint)timeout.TotalMilliseconds,
            out _);
        return result != 0;
    }

    /// <summary>
    /// SetFocus only works when the target hwnd belongs to the calling thread, or when
    /// the calling thread's input queue is attached to the target window's thread.
    /// Mintty runs in another process, so we must attach input queues for the call to
    /// take effect — otherwise SetFocus is silently dropped.
    /// </summary>
    private static void FocusWithAttachedInput(HWND hwnd)
    {
        uint targetThreadId = PInvoke.GetWindowThreadProcessId(hwnd, out _);
        if (targetThreadId == 0)
        {
            return;
        }

        uint currentThreadId = PInvoke.GetCurrentThreadId();
        if (targetThreadId == currentThreadId)
        {
            PInvoke.SetFocus(hwnd);
            return;
        }

        if (!PInvoke.AttachThreadInput(currentThreadId, targetThreadId, true))
        {
            return;
        }

        try
        {
            PInvoke.SetFocus(hwnd);
        }
        finally
        {
            PInvoke.AttachThreadInput(currentThreadId, targetThreadId, false);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        HWND hwnd = _runningSession?.WindowHandle ?? HWND.Null;
        if (!hwnd.IsNull && Width > 0 && Height > 0)
        {
            // SWP_ASYNCWINDOWPOS posts the size change to mintty's thread instead of a
            // synchronous cross-process SendMessage. MoveWindow (and plain SetWindowPos)
            // blocks on mintty's message pump, which can deadlock with the modal resize
            // loop if mintty has a SendMessage pending into our window.
            PInvoke.SetWindowPos(hwnd, HWND.Null, 0, 0, Width, Height,
                SET_WINDOW_POS_FLAGS.SWP_NOZORDER
                | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
                | SET_WINDOW_POS_FLAGS.SWP_ASYNCWINDOWPOS);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sessionCts?.Cancel();
            _sessionCts?.Dispose();
            _sessionCts = null;
            _runningSession?.Kill();
            _runningSession = null;

            if (!_jobHandle.IsNull)
            {
                try
                {
                    PInvoke.TerminateJobObject(_jobHandle, 0);
                }
                catch
                {
                }

                PInvoke.CloseHandle(_jobHandle);
                _jobHandle = HANDLE.Null;
            }
        }

        base.Dispose(disposing);
    }
}
