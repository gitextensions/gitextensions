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

    private const string CloseAllMessageName = "Global.GitExtensions.CloseAllInstances";
    private static readonly List<GitExtensionsFormBase> OpenForms = [];

    // parity-scaffolding: substitute native message registration and hook I/O, never form or modal state.
    private static Func<string, uint>? _registerForTesting;

    // parity-scaffolding: substitutes only native hook installation in a headless host.
    private static Action<TopLevel, Win32Properties.CustomWndProcHookCallback>? _attachForTesting;

    // parity-scaffolding: observes only native hook removal when the actual window closes.
    private static Action<TopLevel, Win32Properties.CustomWndProcHookCallback>? _detachForTesting;

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

        if (window is Window form)
        {
            form.Closed += (_, _) =>
            {
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

    internal static GitExtensionsFormBase? FindModalForm()
    {
        IEnumerable<GitExtensionsFormBase> forms = OpenForms;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            forms = desktop.Windows.OfType<GitExtensionsFormBase>().Concat(forms).Distinct();
        }

        return forms.FirstOrDefault(form => form.Modal);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern uint RegisterWindowMessageW(string name);

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
    }
}
