using System.Diagnostics;
using System.Text;
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

    public MinttyControl()
    {
        // Make the panel focusable so Focus() works programmatically and
        // OnGotFocus runs whenever WinForms restores focus here (e.g. after
        // alt-tabbing back to the host form). OnGotFocus then forwards focus
        // to the embedded mintty hwnd — centralising what used to be scattered
        // FocusWindowWithAttachedInput calls at every entry point.
        SetStyle(ControlStyles.Selectable, true);
    }

    internal MinttySession? RunningSession => _runningSession;

    internal bool IsShellRunning => _runningSession is not null && !_runningSession.IsExited;

    public MinttySession StartCommand(MinttyStartInfo startInfo, string minttyPath, string bashPath, ConsoleEmulatorSettings consoleSettings)
    {
        ResetSession();

        CancellationTokenSource sessionCts = _sessionCts!;
        CancellationToken ct = sessionCts.Token;

        (MinttySession session, MinttyConsoleRuntime.CommandLaunchParams launchParams) = MinttySession.StartCommandSession(startInfo);
        _runningSession = session;

        string minttyArgs = $"{BuildMinttyThemingArgs(consoleSettings)}--nodaemon --window hide --log - \"{bashPath}\" -c \"{launchParams.BashBootstrapCommand}\"";

        LaunchAndEmbed(session, minttyPath, minttyArgs, startInfo.StartupDirectory, launchParams.EnvironmentVariables, OnExited, ct, redirectStdout: true);

        return session;

        void OnExited()
        {
            // If ResetSession already cancelled this CTS, skip the callback —
            // the old session was intentionally killed, not a host termination.
            if (!sessionCts.IsCancellationRequested)
            {
                sessionCts.Cancel();
                startInfo.ConsoleClosedCallback?.Invoke();
            }
        }
    }

    public MinttySession StartInteractiveShell(string minttyPath, string bashPath, string workDir, ConsoleEmulatorSettings consoleSettings, Action? shellExitedCallback = null)
    {
        ResetSession();
        CancellationTokenSource sessionCts = _sessionCts!;
        CancellationToken ct = sessionCts.Token;

        string minttyArgs = $"{BuildMinttyThemingArgs(consoleSettings)}--nodaemon --window hide \"{bashPath}\" --login -i";

        MinttySession session = MinttySession.StartInteractiveShellSession();
        _runningSession = session;

        LaunchAndEmbed(session, minttyPath, minttyArgs, workDir, new Dictionary<string, string>(), OnExited, ct);

        return session;

        void OnExited()
        {
            // If ResetSession already cancelled this CTS, skip the callback —
            // the old session was intentionally killed, not a host termination.
            if (!sessionCts.IsCancellationRequested)
            {
                sessionCts.Cancel();
                shellExitedCallback?.Invoke();
            }
        }
    }

    public void SendConsoleInput(string text)
    {
        HWND hwnd = _runningSession?.WindowHandle ?? HWND.Null;
        if (hwnd.IsNull)
        {
            return;
        }

        NativeMethods.FocusWindowWithAttachedInput(hwnd);

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

    private static string BuildMinttyThemingArgs(ConsoleEmulatorSettings consoleSettings)
    {
        StringBuilder args = new();
        if (!string.IsNullOrEmpty(consoleSettings.Theme))
        {
            args.Append($"-o \"ThemeFile={consoleSettings.Theme}\" ");
        }

        if (consoleSettings.Font is { } font)
        {
            args.Append($"-o \"Font={font.Name}\" -o \"FontHeight={(int)(font.Size + 0.5f)}\" ");
        }

        return args.ToString();
    }

    private void ResetSession()
    {
        _sessionCts?.Cancel();
        _sessionCts?.Dispose();
        _sessionCts = new CancellationTokenSource();
        _runningSession?.Dispose();
        _runningSession = null;
    }

    private void LaunchAndEmbed(MinttySession session,
        string minttyPath,
        string minttyArgs,
        string? workDir,
        IReadOnlyDictionary<string, string> envVariables,
        Action onExited,
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

        // Subscribe before enabling events so a near-instant exit (e.g. mintty fails to start)
        // can't fire and be lost before the handler is attached.
        minttyProcess.Exited += (_, _) => onExited();
        minttyProcess.EnableRaisingEvents = true;

        session.AttachProcess(minttyProcess);

        ThreadHelper.FileAndForget(() => FindAndEmbedWindowAsync(minttyProcess, session, ct));
    }

    private async Task FindAndEmbedWindowAsync(Process minttyProcess, MinttySession session, CancellationToken sessionCt)
    {
        ThreadHelper.ThrowIfOnUIThread();

        TimeSpan findWindowTimeout = TimeSpan.FromSeconds(15);
        using CancellationTokenSource timeoutCts = new(findWindowTimeout);
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(sessionCt, timeoutCts.Token);
        CancellationToken ct = linkedCts.Token;

        try
        {
            // Wait until mintty's message pump is idle before embedding — otherwise
            // mintty may still be running startup code that calls ShowWindow/SetFocus
            // and causes the host dialog's focus to flicker as it races with our embed.
            // If the wait times out under load, proceed anyway: the embed may still
            // succeed, and killing mintty here would also kill the spawned git process.
            TimeSpan idleTimeout = TimeSpan.FromSeconds(10);
            bool idle = await Task.Run(() => minttyProcess.WaitForInputIdle(idleTimeout), ct);
            if (!idle)
            {
                throw new InvalidOperationException($"Waiting for mintty to get idle timed out after {idleTimeout.TotalSeconds} seconds for PID {minttyProcess.Id}.");
            }

            HWND hwnd = HWND.Null;

            while (!ct.IsCancellationRequested)
            {
                hwnd = NativeMethods.FindMinttyWindowForProcess(minttyProcess.Id);
                if (!hwnd.IsNull)
                {
                    break;
                }

                // It is essential to capture original synchronization context
                await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
            }

            if (hwnd.IsNull)
            {
                throw new InvalidOperationException($"Could not locate mintty window for PID {minttyProcess.Id} within {findWindowTimeout.TotalSeconds} seconds.");
            }

            await EmbedWindowAsync(hwnd);

            // Set WindowHandle only after the embed completes so OnResize doesn't
            // post a SetWindowPos to mintty mid-reparent.
            session.WindowHandle = hwnd;
        }
        catch (OperationCanceledException) when (sessionCt.IsCancellationRequested)
        {
            // Session was reset/disposed while we were waiting — no error to surface.
        }
        catch (Exception ex)
        {
            // Don't kill mintty: the spawned git process is in the same job and would be
            // terminated mid-write, leaving traces like index.lock behind. Let the command finish in
            // the hidden mintty window — its output still reaches the host via stdout.
            Trace.WriteLine($"MinttyControl: Failed to embed mintty window for PID {minttyProcess.Id}. The command will continue running invisibly until completion. Error: {ex}");
            this.InvokeAndForget(() => ShowEmbedFailure(ex, minttyProcess));
        }
    }

    private async Task EmbedWindowAsync(HWND hwnd)
    {
        // Capture WinForms state on the UI thread - as it's required by the control.
        TaskCompletionSource<(HWND Handle, int Width, int Height)> panelStateTcs = new();
        BeginInvoke(() => panelStateTcs.SetResult(((HWND)Handle, Width, Height)));

        (HWND panelHwnd, int width, int height) = await panelStateTcs.Task;

        // The synchronous cross-process Win32 calls below all send messages to
        // mintty's thread. Running them on the UI thread freezes the entire app
        // when mintty's pump stalls.
        ThreadHelper.ThrowIfOnUIThread();

        // Probe mintty's pump before any blocking cross-process call.
        // SendMessageTimeout(SMTO_ABORTIFHUNG) is bounded and cannot itself
        // hang. If this fails, none of the cross-process calls below are safe.
        TimeSpan responsivenessTimeout = TimeSpan.FromSeconds(5);
        if (!NativeMethods.IsWindowResponsive(hwnd, responsivenessTimeout))
        {
            throw new InvalidOperationException($"Mintty window did not respond within {responsivenessTimeout.TotalSeconds} seconds.");
        }

        WINDOW_STYLE style = (WINDOW_STYLE)PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
        style &= ~(WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_THICKFRAME | WINDOW_STYLE.WS_BORDER);
        style |= WINDOW_STYLE.WS_CHILD;
        PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)style);

        WINDOW_EX_STYLE extendedStyle = (WINDOW_EX_STYLE)PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        extendedStyle &= ~(WINDOW_EX_STYLE.WS_EX_CLIENTEDGE | WINDOW_EX_STYLE.WS_EX_WINDOWEDGE | WINDOW_EX_STYLE.WS_EX_APPWINDOW);
        extendedStyle |= WINDOW_EX_STYLE.WS_EX_TOOLWINDOW;
        PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)extendedStyle);

        PInvoke.SetParent(hwnd, panelHwnd);

        // SWP_ASYNCWINDOWPOS posts the size/show change to mintty's thread instead
        // of a synchronous cross-process SendMessage. SWP_SHOWWINDOW combined with
        // SWP_NOACTIVATE is equivalent to SW_SHOWNA. See OnResize for the same pattern.
        PInvoke.SetWindowPos(hwnd, HWND.Null, 0, 0, width, height,
            SET_WINDOW_POS_FLAGS.SWP_NOZORDER
            | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
            | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
            | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW
            | SET_WINDOW_POS_FLAGS.SWP_ASYNCWINDOWPOS);

        // Defer Focus via BeginInvoke so our pump gets one cycle to drain. Mintty
        // is now parented to our panel — that means cross-process messages
        // (WM_PARENTNOTIFY, WM_MOUSEACTIVATE, focus-traversal traffic, etc.) can
        // flow synchronously from mintty into our pump. Anything mintty posts
        // while our continuation is dispatched gets handled before our queued
        // Focus runs, instead of being processed reentrantly underneath SetFocus.
        BeginInvoke(Focus);
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);

        HWND hwnd = _runningSession?.WindowHandle ?? HWND.Null;
        if (!hwnd.IsNull)
        {
            NativeMethods.FocusWindowWithAttachedInput(hwnd);
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
            _runningSession?.Dispose();
            _runningSession = null;

            if (!_jobHandle.IsNull)
            {
                try
                {
                    PInvoke.TerminateJobObject(_jobHandle, 0);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }

                PInvoke.CloseHandle(_jobHandle);
                _jobHandle = HANDLE.Null;
            }
        }

        base.Dispose(disposing);
    }

    private void ShowEmbedFailure(Exception ex, Process minttyProcess)
    {
        if (IsDisposed)
        {
            return;
        }

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            BackColor = SystemColors.Control,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(20),
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Label errorLabel = new()
        {
            // ReSharper disable once LocalizableElement - rare error, no need to localize
            Text = $"Failed to display the embedded console:{Environment.NewLine}{ex.Message}",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            ForeColor = SystemColors.GrayText,
        };

        Button showButton = new()
        {
            // ReSharper disable once LocalizableElement - rare error, no need to localize
            Text = "Show mintty window externally",
            AutoSize = true,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0, 8, 0, 0),
        };
        showButton.Click += (_, _) =>
        {
            try
            {
                if (minttyProcess.HasExited)
                {
                    return;
                }

                // Try the targeted lookup first; only fall back to enumerating every top-level window of the process
                // if it fails (e.g. mintty's pump was so slow we never saw its window appear).
                HWND mainHwnd = NativeMethods.FindMinttyWindowForProcess(minttyProcess.Id);
                List<HWND> windows = mainHwnd.IsNull
                    ? NativeMethods.EnumerateProcessWindows(minttyProcess.Id)
                    : [mainHwnd];

                foreach (HWND hwnd in windows)
                {
                    // mintty was started with --window hide; if embedding failed, it threw
                    // before we touched any styles, so the window keeps its original chrome
                    // and just needs to be unhidden.
                    PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWNORMAL);
                }
            }
            catch (Exception clickEx)
            {
                Trace.WriteLine($"MinttyControl: Failed to surface mintty window externally: {clickEx}");
            }
        };

        layout.Controls.Add(errorLabel, 0, 0);
        layout.Controls.Add(showButton, 0, 1);
        Controls.Add(layout);
    }
}
