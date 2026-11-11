using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;

namespace GitUI.Compat;

// Framework constraint: the original Windows API Code Pack taskbar types require the Windows desktop
// framework, so the port supplies the same small surface over the Windows shell.
internal sealed class TaskbarManager
{
    private static ITaskbarShell _shell = OperatingSystem.IsWindows() ? new WindowsTaskbarShell() : new UnsupportedTaskbarShell();
    private string? _applicationId;

    private TaskbarManager()
    {
    }

    internal static TaskbarManager Instance { get; } = new();

    internal static bool IsPlatformSupported => _shell.IsPlatformSupported;

    internal static ITaskbarShell Shell => _shell;

    internal string? ApplicationId
    {
        get => _applicationId;
        set
        {
            _applicationId = value;
            if (value is not null)
            {
                _shell.SetApplicationId(value);
            }
        }
    }

    internal ThumbnailToolBarManager ThumbnailToolBars { get; } = new();

    /// <summary>
    /// Gets the process's main window, which owns the taskbar button, like the Code Pack's OwnerHandle.
    /// </summary>
    internal static nint OwnerHandle
    {
        get
        {
            using Process process = Process.GetCurrentProcess();
            nint handle = process.MainWindowHandle;
            if (handle == 0)
            {
                throw new InvalidOperationException("A valid active Window is needed to update the Taskbar.");
            }

            return handle;
        }
    }

    internal void SetProgressState(TaskbarProgressBarState state) => _shell.SetProgressState(OwnerHandle, state);

    internal void SetProgressValue(int currentValue, int maximumValue)
        => _shell.SetProgressValue(OwnerHandle, Convert.ToUInt64(currentValue), Convert.ToUInt64(maximumValue));

    internal void SetOverlayIcon(TaskbarIcon? icon, string accessibilityText)
        => _shell.SetOverlayIcon(OwnerHandle, icon, accessibilityText);

    // parity-scaffolding: replaces only the native shell boundary, never the taskbar state above it.
    internal readonly struct TestAccessor
    {
        internal static ITaskbarShell Shell
        {
            get => _shell;
            set => _shell = value;
        }
    }
}

internal sealed class ThumbnailToolBarManager
{
    private const uint CommandMessage = 0x0111;
    private const int ThumbnailButtonClicked = 0x1800;

    private uint _nextButtonId = 1;

    /// <summary>
    /// Raises the click of the thumbnail button a WM_COMMAND message names. The shell reports a click
    /// with THBN_CLICKED in the high word and the button id in the low word.
    /// </summary>
    internal static bool DispatchClick(IReadOnlyList<ThumbnailToolBarButton> buttons, uint message, nint wordParameter)
    {
        if (message != CommandMessage || ((int)((long)wordParameter >> 16) & 0xffff) != ThumbnailButtonClicked)
        {
            return false;
        }

        uint id = (uint)((long)wordParameter & 0xffff);
        if (buttons.FirstOrDefault(button => button.Id == id) is not { } clicked)
        {
            return false;
        }

        clicked.OnClick();
        return true;
    }

    internal void AddButtons(nint windowHandle, params ThumbnailToolBarButton[] buttons)
    {
        ArgumentNullException.ThrowIfNull(buttons);
        if (windowHandle == 0)
        {
            throw new ArgumentException("The window handle must be nonzero.", nameof(windowHandle));
        }

        foreach (ThumbnailToolBarButton button in buttons)
        {
            button.Id = _nextButtonId++;
            button.WindowHandle = windowHandle;
        }

        TaskbarManager.Shell.AddButtons(windowHandle, buttons);
        foreach (ThumbnailToolBarButton button in buttons)
        {
            button.AddedToTaskbar = true;
        }
    }
}

internal sealed class ThumbnailToolBarButton : IDisposable
{
    private TaskbarIcon? _icon;
    private bool _enabled = true;

    internal ThumbnailToolBarButton(TaskbarIcon icon, string tooltip)
    {
        _icon = icon;
        Tooltip = tooltip;
    }

    internal event EventHandler<ThumbnailButtonClickedEventArgs>? Click;

    internal string Tooltip { get; }

    internal TaskbarIcon? Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            Update();
        }
    }

    internal bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled != value)
            {
                _enabled = value;
                Update();
            }
        }
    }

    internal uint Id { get; set; }

    internal nint WindowHandle { get; set; }

    internal bool AddedToTaskbar { get; set; }

    public void Dispose()
    {
        _icon?.Dispose();
        AddedToTaskbar = false;
    }

    internal void OnClick() => Click?.Invoke(this, new ThumbnailButtonClickedEventArgs(WindowHandle, this));

    private void Update()
    {
        if (AddedToTaskbar)
        {
            TaskbarManager.Shell.UpdateButton(WindowHandle, this);
        }
    }
}

/// <summary>
/// Describes a click on a thumbnail toolbar button.
/// </summary>
public sealed class ThumbnailButtonClickedEventArgs : EventArgs
{
    internal ThumbnailButtonClickedEventArgs(nint windowHandle, ThumbnailToolBarButton thumbnailButton)
    {
        WindowHandle = windowHandle;
        ThumbnailButton = thumbnailButton;
    }

    /// <summary>Gets the handle of the window whose thumbnail toolbar was clicked.</summary>
    public nint WindowHandle { get; }

    internal ThumbnailToolBarButton ThumbnailButton { get; }
}

/// <summary>
/// The taskbar progress states, with the Windows TBPFLAG values.
/// </summary>
public enum TaskbarProgressBarState
{
    NoProgress = 0,
    Indeterminate = 0x1,
    Normal = 0x2,
    Error = 0x4,
    Paused = 0x8,
}

internal enum JumpListKnownCategoryType
{
    Neither,
    Recent,
    Frequent,
}

internal sealed class JumpList
{
    private JumpList()
    {
    }

    internal JumpListKnownCategoryType KnownCategoryToDisplay { get; set; }

    internal static JumpList CreateJumpList() => new();

    internal static void AddToRecent(string destination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        TaskbarManager.Shell.AddToRecent(destination);
    }

    // The port never adds user tasks, so every committed list already has an empty task list.
    internal void ClearAllUserTasks()
    {
    }

    internal void Refresh() => TaskbarManager.Shell.RefreshJumpList(TaskbarManager.Instance.ApplicationId, KnownCategoryToDisplay);
}

/// <summary>
/// An icon for the Windows taskbar, created from a rendered bitmap. The native handle is created
/// on first use and destroyed with the icon.
/// </summary>
internal sealed class TaskbarIcon : IDisposable
{
    private nint _handle;

    private TaskbarIcon(byte[] pngData, int size)
    {
        PngData = pngData;
        Size = size;
    }

    internal byte[] PngData { get; }

    internal int Size { get; }

    internal bool IsDisposed { get; private set; }

    public void Dispose()
    {
        if (_handle != 0)
        {
            TaskbarManager.Shell.DestroyIcon(_handle);
            _handle = 0;
        }

        IsDisposed = true;
    }

    internal static TaskbarIcon FromBitmap(RenderTargetBitmap bitmap)
    {
        using MemoryStream stream = new();
        bitmap.Save(stream, PngBitmapEncoderOptions.Default);
        return new TaskbarIcon(stream.ToArray(), bitmap.PixelSize.Width);
    }

    internal nint GetHandle()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (_handle == 0)
        {
            _handle = TaskbarManager.Shell.CreateIcon(PngData, Size);
        }

        return _handle;
    }
}

/// <summary>
/// The error the Windows shell reported for a taskbar or jump list request.
/// </summary>
internal sealed class ShellException : Exception
{
    internal ShellException(string operation, int hresult)
        : base($"{operation} failed with HRESULT 0x{hresult:X8}.")
    {
        HResult = hresult;
    }
}

/// <summary>
/// The native boundary behind the taskbar types. The Windows shell implements it; other platforms
/// report no support, so callers keep their original IsPlatformSupported guards.
/// </summary>
internal interface ITaskbarShell
{
    bool IsPlatformSupported { get; }

    void SetApplicationId(string applicationId);

    void AddButtons(nint windowHandle, IReadOnlyList<ThumbnailToolBarButton> buttons);

    void UpdateButton(nint windowHandle, ThumbnailToolBarButton button);

    void RefreshJumpList(string? applicationId, JumpListKnownCategoryType knownCategory);

    void AddToRecent(string path);

    nint CreateIcon(byte[] pngData, int size);

    void DestroyIcon(nint icon);

    void SetProgressState(nint windowHandle, TaskbarProgressBarState state);

    void SetProgressValue(nint windowHandle, ulong completed, ulong total);

    void SetOverlayIcon(nint windowHandle, TaskbarIcon? icon, string description);
}

internal sealed class UnsupportedTaskbarShell : ITaskbarShell
{
    public bool IsPlatformSupported => false;

    public void SetApplicationId(string applicationId) => throw new PlatformNotSupportedException();

    public void AddButtons(nint windowHandle, IReadOnlyList<ThumbnailToolBarButton> buttons) => throw new PlatformNotSupportedException();

    public void UpdateButton(nint windowHandle, ThumbnailToolBarButton button) => throw new PlatformNotSupportedException();

    public void RefreshJumpList(string? applicationId, JumpListKnownCategoryType knownCategory) => throw new PlatformNotSupportedException();

    public void AddToRecent(string path) => throw new PlatformNotSupportedException();

    public nint CreateIcon(byte[] pngData, int size) => throw new PlatformNotSupportedException();

    public void DestroyIcon(nint icon) => throw new PlatformNotSupportedException();

    public void SetProgressState(nint windowHandle, TaskbarProgressBarState state) => throw new PlatformNotSupportedException();

    public void SetProgressValue(nint windowHandle, ulong completed, ulong total) => throw new PlatformNotSupportedException();

    public void SetOverlayIcon(nint windowHandle, TaskbarIcon? icon, string description) => throw new PlatformNotSupportedException();
}

[SupportedOSPlatform("windows")]
internal sealed unsafe class WindowsTaskbarShell : ITaskbarShell
{
    private const uint InProcessServer = 1;
    private const uint ThumbButtonIcon = 0x2;
    private const uint ThumbButtonTooltip = 0x4;
    private const uint ThumbButtonFlags = 0x8;
    private const uint ThumbButtonDisabled = 0x1;
    private const uint AddPathToRecentDocuments = 3;
    private const int TooltipCapacity = 260;

    private static readonly Guid TaskbarListClass = new("56FDF344-FD6D-11d0-958A-006097C9A090");
    private static readonly Guid TaskbarList3Interface = new("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFAF");
    private static readonly Guid DestinationListClass = new("77F10CF0-3DB5-4966-B520-B7C54FD35ED6");
    private static readonly Guid CustomDestinationListInterface = new("6332DEBF-87B5-4670-90C0-5E57B408A49E");
    private static readonly Guid ObjectArrayInterface = new("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9");

    private readonly Dictionary<nint, List<ThumbnailToolBarButton>> _buttonsByWindow = [];
    private nint _taskbarList;

    public bool IsPlatformSupported => OperatingSystem.IsWindowsVersionAtLeast(6, 1);

    public void SetApplicationId(string applicationId)
        => Check(SetCurrentProcessExplicitAppUserModelID(applicationId), nameof(SetApplicationId));

    public void AddButtons(nint windowHandle, IReadOnlyList<ThumbnailToolBarButton> buttons)
    {
        ThumbButton[] nativeButtons = [.. buttons.Select(ToNative)];
        nint taskbarList = GetTaskbarList();
        fixed (ThumbButton* first = nativeButtons)
        {
            Check(((delegate* unmanaged[Stdcall]<nint, nint, uint, ThumbButton*, int>)Method(taskbarList, 15))(taskbarList, windowHandle, (uint)nativeButtons.Length, first), nameof(AddButtons));
        }

        _buttonsByWindow[windowHandle] = [.. buttons];
        HookWindow(windowHandle);
    }

    public void UpdateButton(nint windowHandle, ThumbnailToolBarButton button)
    {
        ThumbButton nativeButton = ToNative(button);
        nint taskbarList = GetTaskbarList();
        Check(((delegate* unmanaged[Stdcall]<nint, nint, uint, ThumbButton*, int>)Method(taskbarList, 16))(taskbarList, windowHandle, 1, &nativeButton), nameof(UpdateButton));
    }

    public void RefreshJumpList(string? applicationId, JumpListKnownCategoryType knownCategory)
    {
        nint destinationList = Create(DestinationListClass, CustomDestinationListInterface, nameof(RefreshJumpList));
        try
        {
            if (applicationId is not null)
            {
                fixed (char* id = applicationId)
                {
                    Check(((delegate* unmanaged[Stdcall]<nint, char*, int>)Method(destinationList, 3))(destinationList, id), nameof(RefreshJumpList));
                }
            }

            uint minimumSlots;
            nint removedDestinations;
            Guid objectArray = ObjectArrayInterface;
            Check(((delegate* unmanaged[Stdcall]<nint, uint*, Guid*, nint*, int>)Method(destinationList, 4))(destinationList, &minimumSlots, &objectArray, &removedDestinations), nameof(RefreshJumpList));
            Release(removedDestinations);

            int result = knownCategory switch
            {
                JumpListKnownCategoryType.Recent => AppendKnownCategory(destinationList, 2),
                JumpListKnownCategoryType.Frequent => AppendKnownCategory(destinationList, 1),
                _ => 0,
            };

            if (result < 0)
            {
                ((delegate* unmanaged[Stdcall]<nint, int>)Method(destinationList, 11))(destinationList);
                Check(result, nameof(RefreshJumpList));
            }

            Check(((delegate* unmanaged[Stdcall]<nint, int>)Method(destinationList, 8))(destinationList), nameof(RefreshJumpList));
        }
        finally
        {
            Release(destinationList);
        }
    }

    public void AddToRecent(string path) => SHAddToRecentDocs(AddPathToRecentDocuments, path);

    public nint CreateIcon(byte[] pngData, int size)
    {
        fixed (byte* data = pngData)
        {
            nint icon = CreateIconFromResourceEx(data, (uint)pngData.Length, true, 0x00030000, size, size, 0);
            if (icon == 0)
            {
                throw new ShellException(nameof(CreateIcon), Marshal.GetHRForLastWin32Error());
            }

            return icon;
        }
    }

    public void DestroyIcon(nint icon) => DestroyIconNative(icon);

    public void SetProgressState(nint windowHandle, TaskbarProgressBarState state)
    {
        nint taskbarList = GetTaskbarList();
        Check(((delegate* unmanaged[Stdcall]<nint, nint, int, int>)Method(taskbarList, 10))(taskbarList, windowHandle, (int)state), nameof(SetProgressState));
    }

    public void SetProgressValue(nint windowHandle, ulong completed, ulong total)
    {
        nint taskbarList = GetTaskbarList();
        Check(((delegate* unmanaged[Stdcall]<nint, nint, ulong, ulong, int>)Method(taskbarList, 9))(taskbarList, windowHandle, completed, total), nameof(SetProgressValue));
    }

    public void SetOverlayIcon(nint windowHandle, TaskbarIcon? icon, string description)
    {
        nint taskbarList = GetTaskbarList();
        nint iconHandle = icon?.GetHandle() ?? 0;
        fixed (char* text = description)
        {
            Check(((delegate* unmanaged[Stdcall]<nint, nint, nint, char*, int>)Method(taskbarList, 18))(taskbarList, windowHandle, iconHandle, text), nameof(SetOverlayIcon));
        }
    }

    private static int AppendKnownCategory(nint destinationList, int category)
        => ((delegate* unmanaged[Stdcall]<nint, int, int>)Method(destinationList, 6))(destinationList, category);

    private static void* Method(nint instance, int slot) => (*(void***)instance)[slot];

    private static void Release(nint instance)
    {
        if (instance != 0)
        {
            ((delegate* unmanaged[Stdcall]<nint, uint>)Method(instance, 2))(instance);
        }
    }

    private static void Check(int hresult, string operation)
    {
        if (hresult < 0)
        {
            throw new ShellException(operation, hresult);
        }
    }

    private static nint Create(Guid classId, Guid interfaceId, string operation)
    {
        Check(CoCreateInstance(&classId, 0, InProcessServer, &interfaceId, out nint instance), operation);
        return instance;
    }

    private static ThumbButton ToNative(ThumbnailToolBarButton button)
    {
        ThumbButton nativeButton = default;
        nativeButton.Mask = ThumbButtonIcon | ThumbButtonTooltip | ThumbButtonFlags;
        nativeButton.Id = button.Id;
        nativeButton.Icon = button.Icon?.GetHandle() ?? 0;
        nativeButton.Flags = button.Enabled ? 0 : ThumbButtonDisabled;
        int length = Math.Min(button.Tooltip.Length, TooltipCapacity - 1);
        for (int i = 0; i < length; i++)
        {
            nativeButton.Tooltip[i] = button.Tooltip[i];
        }

        return nativeButton;
    }

    private nint GetTaskbarList()
    {
        if (_taskbarList == 0)
        {
            nint taskbarList = Create(TaskbarListClass, TaskbarList3Interface, nameof(GetTaskbarList));
            int result = ((delegate* unmanaged[Stdcall]<nint, int>)Method(taskbarList, 3))(taskbarList);
            if (result < 0)
            {
                Release(taskbarList);
                Check(result, nameof(GetTaskbarList));
            }

            _taskbarList = taskbarList;
        }

        return _taskbarList;
    }

    private void HookWindow(nint windowHandle)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop
            || desktop.Windows.FirstOrDefault(window => ApplicationForms.GetHandle(window) == windowHandle) is not { } window)
        {
            return;
        }

        Win32Properties.CustomWndProcHookCallback callback = (nint handle, uint message, nint wordParameter, nint longParameter, ref bool handled) =>
        {
            if (_buttonsByWindow.TryGetValue(handle, out List<ThumbnailToolBarButton>? buttons)
                && ThumbnailToolBarManager.DispatchClick(buttons, message, wordParameter))
            {
                handled = true;
            }

            return 0;
        };

        Win32Properties.AddWndProcHookCallback(window, callback);
        window.Closed += (_, _) =>
        {
            Win32Properties.RemoveWndProcHookCallback(window, callback);
            _buttonsByWindow.Remove(windowHandle);
        };
    }

    [DllImport("ole32.dll", ExactSpelling = true)]
    private static extern int CoCreateInstance(Guid* classId, nint outer, uint context, Guid* interfaceId, out nint instance);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string applicationId);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern void SHAddToRecentDocs(uint flags, string path);

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern nint CreateIconFromResourceEx(byte* data, uint size, [MarshalAs(UnmanagedType.Bool)] bool icon, uint version, int width, int height, uint flags);

    [DllImport("user32.dll", EntryPoint = "DestroyIcon", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIconNative(nint icon);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ThumbButton
    {
        public uint Mask;
        public uint Id;
        public uint Bitmap;
        public nint Icon;
        public fixed char Tooltip[TooltipCapacity];
        public uint Flags;
    }
}
