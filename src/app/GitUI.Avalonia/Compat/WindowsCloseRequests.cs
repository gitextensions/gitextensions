using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ResourceManager;

namespace GitUI.Compat;

// Framework constraint: Avalonia exposes native message hooks instead of a virtual WinForms WndProc.
internal static class WindowsCloseRequests
{
    internal const uint SystemCommand = 0x0112;
    internal const int CloseCommand = 0xf060;

    private const nint BroadcastWindow = 0xffff;

    private const string CloseAllMessageName = "Global.GitExtensions.CloseAllInstances";
    private static readonly List<GitExtensionsFormBase> OpenForms = [];
    private static readonly ConditionalWeakTable<TopLevel, List<Win32Properties.CustomWndProcHookCallback>> InstalledHooks = new();

    // parity-scaffolding: substitute native message registration and hook I/O, never form or modal state.
    private static Func<string, uint>? _registerForTesting;

    // parity-scaffolding: substitutes only native hook installation in a headless host.
    private static Action<TopLevel, Win32Properties.CustomWndProcHookCallback>? _attachForTesting;

    // parity-scaffolding: observes only native hook removal when the actual window closes.
    private static Action<TopLevel, Win32Properties.CustomWndProcHookCallback>? _detachForTesting;

    // parity-scaffolding: substitutes only the native broadcast, which would reach every running instance.
    private static Action<uint>? _broadcastForTesting;

    static WindowsCloseRequests()
    {
        Window.WindowOpenedEvent.AddClassHandler<GitExtensionsFormBase>((form, _) =>
        {
            if (!OpenForms.Contains(form))
            {
                OpenForms.Add(form);
                form.Closed += (_, _) => OpenForms.Remove(form);
            }
        });
    }

    internal static uint RegisterMessage()
        => _registerForTesting?.Invoke(CloseAllMessageName)
            ?? (OperatingSystem.IsWindows() ? RegisterWindowMessageW(CloseAllMessageName) : 0);

    /// <summary>
    /// Posts the close-all request to every top-level window, so every running instance closes.
    /// </summary>
    internal static void BroadcastCloseAll(uint message)
    {
        if (_broadcastForTesting is not null)
        {
            _broadcastForTesting(message);
        }
        else if (OperatingSystem.IsWindows())
        {
            PostMessageW(BroadcastWindow, message, 0, 0);
        }
        else
        {
            // Cross-platform constraint: other platforms have no broadcast window message, so the request
            // reaches the windows of this instance only.
            DispatchToInstalledWindows(message);
        }
    }

    /// <summary>
    /// Delivers a message to every installed window callback of this instance, as the native window
    /// procedure would.
    /// </summary>
    internal static void DispatchToInstalledWindows(uint message)
    {
        foreach ((TopLevel window, List<Win32Properties.CustomWndProcHookCallback> callbacks) in InstalledHooks.ToArray())
        {
            foreach (Win32Properties.CustomWndProcHookCallback callback in callbacks.ToArray())
            {
                bool handled = false;
                callback(ApplicationForms.GetHandle(window), message, 0, 0, ref handled);
            }
        }
    }

    internal static void Install(TopLevel window, Win32Properties.CustomWndProcHookCallback callback)
    {
        if (_attachForTesting is not null)
        {
            _attachForTesting(window, callback);
        }
        else
        {
            Win32Properties.AddWndProcHookCallback(window, callback);
        }

        InstalledHooks.GetOrCreateValue(window).Add(callback);
        if (window is Window form)
        {
            form.Closed += (_, _) =>
            {
                InstalledHooks.GetOrCreateValue(window).Remove(callback);
                if (_detachForTesting is not null)
                {
                    _detachForTesting(window, callback);
                }
                else
                {
                    Win32Properties.RemoveWndProcHookCallback(window, callback);
                }
            };
        }
    }

    internal static GitExtensionsFormBase? FindModalForm() => GetOpenForms().FirstOrDefault(form => form.Modal);

    /// <summary>
    /// Gets the application's open forms, like WinForms <c>Application.OpenForms</c>.
    /// </summary>
    internal static IReadOnlyList<GitExtensionsFormBase> GetOpenForms()
    {
        IEnumerable<GitExtensionsFormBase> forms = OpenForms;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            forms = desktop.Windows.OfType<GitExtensionsFormBase>().Concat(forms).Distinct();
        }

        return [.. forms];
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern uint RegisterWindowMessageW(string name);

    [DllImport("user32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessageW(nint window, uint message, nint wordParameter, nint longParameter);

    // parity-scaffolding: observes only the external registration boundary supplied by the real Browse constructor.
    internal readonly struct TestAccessor
    {
        internal static Func<string, uint>? Register
        {
            get => _registerForTesting;
            set => _registerForTesting = value;
        }

        internal static Action<TopLevel, Win32Properties.CustomWndProcHookCallback>? Attach
        {
            get => _attachForTesting;
            set => _attachForTesting = value;
        }

        internal static Action<TopLevel, Win32Properties.CustomWndProcHookCallback>? Detach
        {
            get => _detachForTesting;
            set => _detachForTesting = value;
        }

        internal static Action<uint>? Broadcast
        {
            get => _broadcastForTesting;
            set => _broadcastForTesting = value;
        }
    }
}
