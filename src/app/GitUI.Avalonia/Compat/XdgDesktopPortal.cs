using System.ComponentModel;
using System.Runtime.InteropServices;
using GitExtensions.Shims.WinForms;
using Microsoft.Win32.SafeHandles;
using Tmds.DBus.Protocol;

namespace GitUI.Compat;

/// <summary>
///  Minimal client for the freedesktop FileChooser and OpenURI portals.
/// </summary>
public interface IXdgDesktopPortal
{
    Task<bool> IsInterfaceAvailableAsync(string interfaceName);

    Task<XdgFileChooserResult> ShowFileChooserAsync(XdgFileChooserRequest request);

    Task<bool> TryLaunchAsync(string target, OsShellLaunchKind kind);
}

/// <summary>
///  Describes one native XDG FileChooser request without depending on a toolkit dialog.
/// </summary>
public sealed record XdgFileChooserRequest(
    string Title,
    bool Directory,
    bool Multiple,
    bool Save,
    string? CurrentFolder,
    string? SuggestedFileName,
    IReadOnlyList<XdgFileChooserFilter> Filters);

/// <summary>
///  Describes one XDG FileChooser filter and its glob and MIME rules.
/// </summary>
public sealed record XdgFileChooserFilter(
    string Name,
    IReadOnlyList<string> Patterns,
    IReadOnlyList<string> MimeTypes);

/// <summary>
///  Contains the portal response code and any selected file URIs.
/// </summary>
public sealed record XdgFileChooserResult(uint Response, IReadOnlyList<Uri> Uris)
{
    public bool Accepted => Response == 0;
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
    private const uint FileChooserRuleGlob = 0;
    private const uint FileChooserRuleMimeType = 1;
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

    public async Task<XdgFileChooserResult> ShowFileChooserAsync(XdgFileChooserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        using DBusConnection connection = await ConnectAsync();
        MessageWriter writer = connection.GetMessageWriter();
        writer.WriteMethodCallHeader(
            Destination,
            DesktopPath,
            FileChooserInterface,
            request.Save ? "SaveFile" : "OpenFile",
            "ssa{sv}");
        writer.WriteString(string.Empty);
        writer.WriteString(request.Title);
        string requestPath = WriteFileChooserOptions(connection, ref writer, request);
        return await WaitForFileChooserResponseAsync(connection, writer.CreateMessage(), requestPath);
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

    private static string WriteFileChooserOptions(
        DBusConnection connection,
        ref MessageWriter writer,
        XdgFileChooserRequest request)
    {
        string handleToken = $"gitextensions_{Guid.NewGuid():N}";
        string uniqueName = connection.UniqueName
            ?? throw new InvalidOperationException("The D-Bus connection has no unique name.");
        string sender = uniqueName.TrimStart(':').Replace('.', '_');
        string requestPath = $"/org/freedesktop/portal/desktop/request/{sender}/{handleToken}";

        ArrayStart dictionary = writer.WriteDictionaryStart();
        WriteBooleanOption(ref writer, "directory", request.Directory);
        WriteStringOption(ref writer, "handle_token", handleToken);
        WriteBooleanOption(ref writer, "modal", true);
        WriteBooleanOption(ref writer, "multiple", request.Multiple);

        if (!string.IsNullOrWhiteSpace(request.CurrentFolder))
        {
            WriteByteArrayOption(ref writer, "current_folder", PathToNullTerminatedBytes(request.CurrentFolder));
        }

        if (!string.IsNullOrWhiteSpace(request.SuggestedFileName))
        {
            WriteStringOption(ref writer, "current_name", request.SuggestedFileName);
        }

        if (request.Filters.Count > 0)
        {
            writer.WriteDictionaryEntryStart();
            writer.WriteString("filters");
            writer.WriteSignature("a(sa(us))");
            ArrayStart filters = writer.WriteArrayStart(DBusType.Struct);
            foreach (XdgFileChooserFilter filter in request.Filters)
            {
                writer.WriteStructureStart();
                writer.WriteString(filter.Name);
                ArrayStart rules = writer.WriteArrayStart(DBusType.Struct);
                foreach (string pattern in filter.Patterns)
                {
                    WriteFilterRule(ref writer, FileChooserRuleGlob, pattern);
                }

                foreach (string mimeType in filter.MimeTypes)
                {
                    WriteFilterRule(ref writer, FileChooserRuleMimeType, mimeType);
                }

                writer.WriteArrayEnd(rules);
            }

            writer.WriteArrayEnd(filters);
        }

        writer.WriteDictionaryEnd(dictionary);
        return requestPath;
    }

    private static byte[] PathToNullTerminatedBytes(string path)
        => System.Text.Encoding.UTF8.GetBytes(Path.GetFullPath(path) + '\0');

    private static void WriteBooleanOption(ref MessageWriter writer, string name, bool value)
    {
        writer.WriteDictionaryEntryStart();
        writer.WriteString(name);
        writer.WriteVariantBool(value);
    }

    private static void WriteByteArrayOption(ref MessageWriter writer, string name, byte[] value)
    {
        writer.WriteDictionaryEntryStart();
        writer.WriteString(name);
        writer.WriteSignature("ay");
        writer.WriteArray(value);
    }

    private static void WriteFilterRule(ref MessageWriter writer, uint kind, string value)
    {
        writer.WriteStructureStart();
        writer.WriteUInt32(kind);
        writer.WriteString(value);
    }

    private static void WriteStringOption(ref MessageWriter writer, string name, string value)
    {
        writer.WriteDictionaryEntryStart();
        writer.WriteString(name);
        writer.WriteVariantString(value);
    }

    private static async Task<XdgFileChooserResult> WaitForFileChooserResponseAsync(
        DBusConnection connection,
        MessageBuffer message,
        string requestPath)
    {
        TaskCompletionSource<XdgFileChooserResult> response = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using IDisposable observer = await connection.AddMatchAsync(
            new MatchRule
            {
                Type = MessageType.Signal,
                Path = requestPath,
                Interface = "org.freedesktop.portal.Request",
                Member = "Response",
            },
            static (message, _) =>
            {
                Reader reader = message.GetBodyReader();
                uint responseCode = reader.ReadUInt32();
                Dictionary<string, VariantValue> results = reader.ReadDictionaryOfStringToVariantValue();
                List<Uri> uris = [];
                if (results.TryGetValue("uris", out VariantValue uriValue))
                {
                    foreach (string value in uriValue.GetArray<string>())
                    {
                        if (Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
                        {
                            uris.Add(uri);
                        }
                    }
                }

                return new XdgFileChooserResult(responseCode, uris);
            },
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
            return new XdgFileChooserResult(2, []);
        }

        return await response.Task;
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
