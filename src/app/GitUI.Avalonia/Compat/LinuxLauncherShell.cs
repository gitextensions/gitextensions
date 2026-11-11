using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using GitUI.UserControls;
using Tmds.DBus.Protocol;
using Color = Avalonia.Media.Color;

namespace GitUI.Compat;

/// <summary>
/// The state a Linux launcher shows for the application.
/// </summary>
internal readonly record struct LauncherEntryUpdate(double Progress, bool ProgressVisible, bool Urgent);

/// <summary>
/// Carries the Windows taskbar features to Linux launchers through the cross-desktop routes that exist:
/// the <c>com.canonical.Unity.LauncherEntry</c> signal for progress and the urgent flag (Ubuntu Dock,
/// Dash to Dock, Dash to Panel, KDE Plasma, Plank), the window icon for the status dot, and the shared
/// <c>recently-used.xbel</c> file for recent repositories, which KDE's task manager lists.
/// No XDG portal covers launcher progress, badges or recent items, and Avalonia's Wayland backend
/// does not yet set window icons; revisit these routes when a portal or a newer Avalonia provides one.
/// </summary>
// Cross-platform constraint: Linux has no thumbnail toolbar, jump list or overlay icon.
internal sealed class LinuxLauncherShell : ITaskbarShell
{
    internal const string PackageDesktopId = "com.github.gitextensions.GitExtensions.Avalonia";

    private readonly Action<string, LauncherEntryUpdate> _sendUpdate;
    private readonly Func<string> _recentlyUsedFile;
    private readonly string _desktopId;
    private readonly string _openCommand;
    private readonly object _gate = new();
    private TaskbarProgressBarState _progressState;
    private double _progress;
    private bool _urgent;
    private LauncherEntryUpdate? _sent;
    private Window? _iconWindow;
    private WindowIcon? _originalIcon;

    internal LinuxLauncherShell()
        : this(
            LauncherEntrySignal.Send,
            GetRecentlyUsedFile,
            Environment.GetEnvironmentVariable("FLATPAK_ID") ?? PackageDesktopId,
            GetOpenCommand())
    {
    }

    internal LinuxLauncherShell(Action<string, LauncherEntryUpdate> sendUpdate, Func<string> recentlyUsedFile, string desktopId, string openCommand)
    {
        _sendUpdate = sendUpdate;
        _recentlyUsedFile = recentlyUsedFile;
        _desktopId = desktopId;
        _openCommand = openCommand;
    }

    public bool IsPlatformSupported => true;

    public bool HasMainWindow => ApplicationForms.FindShownWindow() is not null;

    public nint GetOwnerHandle()
    {
        // A launcher entry belongs to the application, so any shown window stands for it like a Windows main window.
        Window window = ApplicationForms.FindShownWindow() ?? throw new InvalidOperationException("A valid active Window is needed to update the Taskbar.");
        return ApplicationForms.GetHandle(window);
    }

    // The desktop file name identifies the launcher entry; an explicit application id has no equivalent.
    public void SetApplicationId(string applicationId)
    {
    }

    // Linux launchers have no per-window thumbnail toolbar.
    public void AddButtons(nint windowHandle, IReadOnlyList<ThumbnailToolBarButton> buttons)
    {
    }

    public void UpdateButton(nint windowHandle, ThumbnailToolBarButton button)
    {
    }

    // Launchers read the shared recently-used file themselves.
    public void RefreshJumpList(string? applicationId, JumpListKnownCategoryType knownCategory)
    {
    }

    public void AddToRecent(string path)
    {
        // The repository file holds the working directory; the launcher opens that directory with Browse.
        string workingDir = File.ReadAllText(path).Trim();
        if (!Directory.Exists(workingDir))
        {
            return;
        }

        try
        {
            RecentlyUsedBookmarks.AddDirectory(_recentlyUsedFile(), workingDir, Path.GetFileNameWithoutExtension(path), _desktopId, _openCommand, DateTime.UtcNow);
        }
        catch (XmlException exception)
        {
            // Another application's damaged file is left as it is.
            Trace.WriteLine(exception.Message, nameof(AddToRecent));
        }
    }

    // Launcher state carries no images, so no native icon handle exists.
    public nint CreateIcon(byte[] pngData, int size) => throw new PlatformNotSupportedException();

    public void DestroyIcon(nint icon) => throw new PlatformNotSupportedException();

    public void SetProgressState(nint windowHandle, TaskbarProgressBarState state)
    {
        lock (_gate)
        {
            _progressState = state;
            if (state == TaskbarProgressBarState.NoProgress)
            {
                _progress = 0;
            }

            SendUpdate();
        }
    }

    public void SetProgressValue(nint windowHandle, ulong completed, ulong total)
    {
        lock (_gate)
        {
            // Like the Windows taskbar, a value shows a normal bar unless an error or paused state is set.
            if (_progressState is not (TaskbarProgressBarState.Error or TaskbarProgressBarState.Paused))
            {
                _progressState = TaskbarProgressBarState.Normal;
            }

            _progress = total == 0 ? 0 : Math.Clamp((double)completed / total, 0, 1);
            SendUpdate();
        }
    }

    public void SetOverlayIcon(nint windowHandle, TaskbarIcon? icon, string description)
    {
        byte[]? overlay = icon?.PngData;

        // A launcher entry cannot show the status dot, so a status other than clean requests attention.
        lock (_gate)
        {
            _urgent = overlay is not null && !ShowsCleanStatus(overlay);
            SendUpdate();
        }

        OnUIThread(() => SetWindowIcon(windowHandle, overlay));
    }

    /// <summary>
    /// Gets whether an overlay icon shows the clean working-directory color. The opaque dot pixels carry
    /// the status color, because the launcher entry receives only the drawn icon.
    /// </summary>
    internal static bool ShowsCleanStatus(byte[] overlay)
    {
        Color clean = ((ISolidColorBrush)RepoStateVisualiser.Clean.Item2).Color;
        using Bitmap bitmap = new(new MemoryStream(overlay));
        int width = bitmap.PixelSize.Width;
        int height = bitmap.PixelSize.Height;
        byte[] pixels = new byte[width * height * 4];
        GCHandle pinned = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            bitmap.CopyPixels(new PixelRect(0, 0, width, height), pinned.AddrOfPinnedObject(), pixels.Length, width * 4);
        }
        finally
        {
            pinned.Free();
        }

        int opaque = 0;
        int cleanPixels = 0;
        for (int i = 0; i < pixels.Length; i += 4)
        {
            if (pixels[i + 3] != 0xff)
            {
                continue;
            }

            opaque++;
            if (pixels[i] == clean.B && pixels[i + 1] == clean.G && pixels[i + 2] == clean.R)
            {
                cleanPixels++;
            }
        }

        return opaque > 0 && cleanPixels * 2 > opaque;
    }

    private static string GetRecentlyUsedFile()
    {
        // A Flatpak's own data directory is private, so the host launcher's file is written through the
        // host file system grant.
        string? dataHome = Environment.GetEnvironmentVariable(
            Environment.GetEnvironmentVariable("FLATPAK_ID") is null ? "XDG_DATA_HOME" : "HOST_XDG_DATA_HOME");
        if (string.IsNullOrEmpty(dataHome))
        {
            dataHome = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
        }

        return Path.Join(dataHome, "recently-used.xbel");
    }

    private static string GetOpenCommand()
        => Environment.GetEnvironmentVariable("FLATPAK_ID") is { } flatpakId
            ? $"flatpak run {flatpakId} browse %f"
            : $"\"{Environment.ProcessPath}\" browse %f";

    private static void OnUIThread(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
        }
        else
        {
            Dispatcher.UIThread.Post(action);
        }
    }

    private void SendUpdate()
    {
        LauncherEntryUpdate update = new(
            _progress,
            ProgressVisible: _progressState is TaskbarProgressBarState.Normal or TaskbarProgressBarState.Error or TaskbarProgressBarState.Paused,
            _urgent);
        if (_sent == update)
        {
            return;
        }

        _sent = update;
        _sendUpdate($"application://{_desktopId}.desktop", update);
    }

    private void SetWindowIcon(nint windowHandle, byte[]? overlay)
    {
        if (overlay is null)
        {
            RestoreWindowIcon();
            return;
        }

        if ((ApplicationForms.FindWindow(windowHandle) ?? ApplicationForms.FindShownWindow()) is not { } window)
        {
            return;
        }

        if (!ReferenceEquals(window, _iconWindow))
        {
            RestoreWindowIcon();
            _iconWindow = window;
            _originalIcon = window.Icon;
        }

        // Shown where the platform honors window icons today, and on Wayland once Avalonia supports it.
        window.Icon = new WindowIcon(new MemoryStream(TaskbarTileImage.Render(TaskbarProgressBarState.NoProgress, 0, overlay)));
    }

    private void RestoreWindowIcon()
    {
        if (_iconWindow is not null)
        {
            _iconWindow.Icon = _originalIcon;
            _iconWindow = null;
            _originalIcon = null;
        }
    }

    /// <summary>
    /// Sends launcher updates on the session bus, in order and without blocking the caller.
    /// </summary>
    private static class LauncherEntrySignal
    {
        private const string ObjectPath = "/com/canonical/unity/launcherentry/gitextensions";
        private const string Interface = "com.canonical.Unity.LauncherEntry";
        private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(8);
        private static readonly object Gate = new();
        private static Task _tail = Task.CompletedTask;
        private static DBusConnection? _connection;

        internal static void Send(string applicationUri, LauncherEntryUpdate update)
        {
            lock (Gate)
            {
                _tail = _tail.ContinueWith(_ => SendAsync(applicationUri, update), TaskScheduler.Default).Unwrap();
            }
        }

        private static async Task SendAsync(string applicationUri, LauncherEntryUpdate update)
        {
            try
            {
                _connection ??= await ConnectAsync();
                MessageWriter writer = _connection.GetMessageWriter();
                writer.WriteSignalHeader(null, ObjectPath, Interface, "Update", "sa{sv}");
                writer.WriteString(applicationUri);
                writer.WriteDictionary(new Dictionary<string, VariantValue>
                {
                    ["progress"] = update.Progress,
                    ["progress-visible"] = update.ProgressVisible,
                    ["urgent"] = update.Urgent,
                });
                _connection.TrySendMessage(writer.CreateMessage());
            }
            catch (Exception exception)
            {
                // A session without a bus or launcher keeps working without launcher state.
                Trace.WriteLine(exception.Message, nameof(LauncherEntrySignal));
                _connection?.Dispose();
                _connection = null;
            }
        }

        private static async Task<DBusConnection> ConnectAsync()
        {
            string address = DBusAddress.Session
                ?? throw new InvalidOperationException("No D-Bus session address is available.");
            DBusConnection connection = new(new DBusConnectionOptions(address) { AutoConnect = false });
            try
            {
                await connection.ConnectAsync().AsTask().WaitAsync(ConnectTimeout);
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }
    }
}
