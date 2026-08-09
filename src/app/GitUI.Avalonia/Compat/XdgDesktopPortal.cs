using System.ComponentModel;
using System.Runtime.InteropServices;
using GitExtensions.Shims.WinForms;
using Microsoft.Win32.SafeHandles;
using Tmds.DBus.Protocol;

namespace GitUI.Compat;

/// <summary>
///  Minimal client for the freedesktop OpenURI portal.
/// </summary>
public interface IXdgDesktopPortal
{
    Task<bool> IsInterfaceAvailableAsync(string interfaceName);

    Task<bool> TryLaunchAsync(string target, OsShellLaunchKind kind);
}

/// <summary>
///  Routes file, directory, and URI launch requests to <c>org.freedesktop.portal.OpenURI</c>.
/// </summary>
public sealed class XdgDesktopPortal : IXdgDesktopPortal
{
    public const string FileChooserInterface = "org.freedesktop.portal.FileChooser";
    public const string OpenUriInterface = "org.freedesktop.portal.OpenURI";

    private const string Destination = "org.freedesktop.portal.Desktop";
    private const string DesktopPath = "/org/freedesktop/portal/desktop";
    private const int OpenReadOnly = 0;
    private const int OpenDirectory = 0x10000;
    private const int OpenCloseOnExec = 0x80000;
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(8);

    public async Task<bool> IsInterfaceAvailableAsync(string interfaceName)
    {
        try
        {
            using DBusConnection connection = await ConnectAsync();
            MessageWriter writer = connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                Destination,
                DesktopPath,
                "org.freedesktop.DBus.Properties",
                "Get",
                "ss");
            writer.WriteString(interfaceName);
            writer.WriteString("version");
            VariantValue version = await connection.CallMethodAsync(
                writer.CreateMessage(),
                static (message, _) => message.GetBodyReader().ReadVariantValue(),
                null).WaitAsync(RequestTimeout);
            return version.GetUInt32() > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> TryLaunchAsync(string target, OsShellLaunchKind kind)
    {
        try
        {
            using DBusConnection connection = await ConnectAsync();
            return kind switch
            {
                OsShellLaunchKind.OpenUri => await OpenUriAsync(connection, target),
                OsShellLaunchKind.ShowInDirectory => await OpenDirectoryAsync(
                    connection,
                    Path.GetDirectoryName(Path.GetFullPath(target))),
                OsShellLaunchKind.OpenDirectory => await OpenDirectoryAsync(connection, target),
                OsShellLaunchKind.OpenAs => await OpenFileAsync(connection, target, ask: true),
                _ => Directory.Exists(target)
                    ? await OpenDirectoryAsync(connection, target)
                    : await OpenFileAsync(connection, target, ask: false),
            };
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task<DBusConnection> ConnectAsync()
    {
        string address = DBusAddress.Session
            ?? throw new InvalidOperationException("No D-Bus session address is available.");
        DBusConnection connection = new(new DBusConnectionOptions(address)
        {
            AutoConnect = false,
        });

        try
        {
            await connection.ConnectAsync().AsTask().WaitAsync(RequestTimeout);
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    private static async Task<bool> OpenUriAsync(DBusConnection connection, string target)
    {
        if (!Uri.TryCreate(target, UriKind.Absolute, out Uri? uri))
        {
            return false;
        }

        MessageWriter writer = CreateWriter(connection, "OpenURI", "ssa{sv}");
        writer.WriteString(string.Empty);
        writer.WriteString(uri.AbsoluteUri);
        MessageBuffer message = WriteOptionsAndCreateMessage(connection, writer, ask: false, out string requestPath);
        return await WaitForResponseAsync(connection, message, requestPath);
    }

    private static Task<bool> OpenFileAsync(DBusConnection connection, string target, bool ask)
        => OpenHandleAsync(connection, target, "OpenFile", directory: false, ask: ask);

    private static Task<bool> OpenDirectoryAsync(DBusConnection connection, string? target)
        => string.IsNullOrWhiteSpace(target)
            ? Task.FromResult(false)
            : OpenHandleAsync(connection, target, "OpenDirectory", directory: true, ask: false);

    private static async Task<bool> OpenHandleAsync(
        DBusConnection connection,
        string target,
        string member,
        bool directory,
        bool ask)
    {
        string fullPath = Path.GetFullPath(target);
        if (directory ? !Directory.Exists(fullPath) : !File.Exists(fullPath))
        {
            return false;
        }

        using SafeFileHandle handle = directory
            ? OpenDirectoryHandle(fullPath)
            : File.OpenHandle(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        MessageWriter writer = CreateWriter(connection, member, "sha{sv}");
        writer.WriteString(string.Empty);
        writer.WriteHandle(handle);
        MessageBuffer message = WriteOptionsAndCreateMessage(connection, writer, ask, out string requestPath);
        return await WaitForResponseAsync(connection, message, requestPath);
    }

    private static MessageWriter CreateWriter(DBusConnection connection, string member, string signature)
    {
        MessageWriter writer = connection.GetMessageWriter();
        writer.WriteMethodCallHeader(Destination, DesktopPath, OpenUriInterface, member, signature);
        return writer;
    }

    private static MessageBuffer WriteOptionsAndCreateMessage(
        DBusConnection connection,
        MessageWriter writer,
        bool ask,
        out string requestPath)
    {
        string handleToken = $"gitextensions_{Guid.NewGuid():N}";
        string uniqueName = connection.UniqueName
            ?? throw new InvalidOperationException("The D-Bus connection has no unique name.");
        string sender = uniqueName.TrimStart(':').Replace('.', '_');
        requestPath = $"/org/freedesktop/portal/desktop/request/{sender}/{handleToken}";
        writer.WriteDictionary(new Dictionary<string, VariantValue>
        {
            ["ask"] = ask,
            ["handle_token"] = handleToken,
        });
        return writer.CreateMessage();
    }

    private static async Task<bool> WaitForResponseAsync(
        DBusConnection connection,
        MessageBuffer message,
        string requestPath)
    {
        TaskCompletionSource<uint> response = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using IDisposable observer = await connection.AddMatchAsync(
            new MatchRule
            {
                Type = MessageType.Signal,
                Path = requestPath,
                Interface = "org.freedesktop.portal.Request",
                Member = "Response",
            },
            static (message, _) => message.GetBodyReader().ReadUInt32(),
            notification =>
            {
                if (notification.HasValue)
                {
                    response.TrySetResult(notification.Value);
                }
                else if (notification.Exception is { } exception)
                {
                    response.TrySetException(exception);
                }
            },
            emitOnCapturedContext: false,
            ObserverFlags.EmitOnConnectionClosed,
            state: null);
        ObjectPath actualRequestPath = await connection.CallMethodAsync(
            message,
            static (message, _) => message.GetBodyReader().ReadObjectPath(),
            null).WaitAsync(RequestTimeout);
        if (!string.Equals(actualRequestPath.ToString(), requestPath, StringComparison.Ordinal))
        {
            return false;
        }

        return await response.Task == 0;
    }

    private static SafeFileHandle OpenDirectoryHandle(string path)
    {
        int handle = open(path, OpenReadOnly | OpenDirectory | OpenCloseOnExec);
        if (handle < 0)
        {
            throw new IOException($"Unable to open directory '{path}' for the XDG desktop portal.", new Win32Exception(Marshal.GetLastPInvokeError()));
        }

        return new SafeFileHandle(handle, ownsHandle: true);
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int open([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int flags);
}
