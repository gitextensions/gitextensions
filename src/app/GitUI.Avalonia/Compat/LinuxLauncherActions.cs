using System.Diagnostics;
using Tmds.DBus.Protocol;

namespace GitUI.Compat;

/// <summary>
/// Delivers the desktop-file actions (Commit, Push, Pull, Close all) to running instances over the
/// session bus. Each Browse instance owns <c>&lt;desktop id&gt;.Instance&lt;unique id&gt;</c>, which a Flatpak
/// may own without an extra permission; process ids repeat across Flatpak sandboxes. A desktop action starts a short-lived process that forwards
/// the action and exits.
/// </summary>
internal static class LinuxLauncherActions
{
    /// <summary>The command-line argument that names the forwarded desktop action.</summary>
    internal const string Argument = "--launcher-action";

    internal const string Commit = "commit";
    internal const string Push = "push";
    internal const string Pull = "pull";
    internal const string CloseAll = "close-all";

    internal const string ObjectPath = "/com/github/gitextensions/Launcher";
    internal const string Interface = "com.github.gitextensions.GitExtensions.Launcher";

    private const string BusName = "org.freedesktop.DBus";
    private const string BusPath = "/org/freedesktop/DBus";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(8);

    internal static string GetInstancePrefix(string desktopId) => $"{desktopId}.Instance";

    /// <summary>
    /// Starts answering the desktop actions for this instance.
    /// </summary>
    /// <param name="address">The session bus address.</param>
    /// <param name="desktopId">The desktop file name without its extension.</param>
    /// <param name="invoke">Runs a received action.</param>
    /// <param name="lastActivation">Gets when a window of this instance was last activated, in UTC ticks.</param>
    internal static async Task<IDisposable> StartAsync(string address, string desktopId, Action<string> invoke, Func<long> lastActivation)
    {
        DBusConnection connection = await ConnectAsync(address);
        try
        {
            connection.AddMethodHandler(new Handler(invoke, lastActivation));
            await connection.RequestNameAsync($"{GetInstancePrefix(desktopId)}{Guid.NewGuid():N}", RequestNameOptions.None).WaitAsync(Timeout);
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Forwards a desktop action to the running instances: Close all to every instance, any other action
    /// to the instance whose window was activated last.
    /// </summary>
    /// <returns><see langword="true"/> when an instance received the action.</returns>
    internal static async Task<bool> ForwardAsync(string address, string desktopId, string action)
    {
        using DBusConnection connection = await ConnectAsync(address);
        string prefix = GetInstancePrefix(desktopId);
        string[] instances = [.. (await ListNamesAsync(connection)).Where(name => name.StartsWith(prefix, StringComparison.Ordinal))];

        if (action == CloseAll)
        {
            bool delivered = false;
            foreach (string instance in instances)
            {
                delivered |= await TryActivateAsync(connection, instance, action);
            }

            return delivered;
        }

        (string Name, long Activation)? latest = null;
        foreach (string instance in instances)
        {
            try
            {
                long activation = await GetLastActivationAsync(connection, instance);
                if (latest is null || activation > latest.Value.Activation)
                {
                    latest = (instance, activation);
                }
            }
            catch (DBusExceptionBase exception)
            {
                // An instance that exits meanwhile no longer competes.
                Trace.WriteLine(exception.Message, nameof(ForwardAsync));
            }
        }

        return latest is { } target && await TryActivateAsync(connection, target.Name, action);
    }

    private static async Task<DBusConnection> ConnectAsync(string address)
    {
        DBusConnection connection = new(new DBusConnectionOptions(address) { AutoConnect = false });
        try
        {
            await connection.ConnectAsync().AsTask().WaitAsync(Timeout);
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    private static Task<string[]> ListNamesAsync(DBusConnection connection)
    {
        MessageWriter writer = connection.GetMessageWriter();
        writer.WriteMethodCallHeader(BusName, BusPath, BusName, "ListNames");
        return connection.CallMethodAsync(writer.CreateMessage(), static (message, _) => message.GetBodyReader().ReadArrayOfString(), null).WaitAsync(Timeout);
    }

    private static Task<long> GetLastActivationAsync(DBusConnection connection, string instance)
    {
        MessageWriter writer = connection.GetMessageWriter();
        writer.WriteMethodCallHeader(instance, ObjectPath, Interface, "GetLastActivation");
        return connection.CallMethodAsync(writer.CreateMessage(), static (message, _) => message.GetBodyReader().ReadInt64(), null).WaitAsync(Timeout);
    }

    private static async Task<bool> TryActivateAsync(DBusConnection connection, string instance, string action)
    {
        MessageWriter writer = connection.GetMessageWriter();
        writer.WriteMethodCallHeader(instance, ObjectPath, Interface, "Activate", "s");
        writer.WriteString(action);
        try
        {
            await connection.CallMethodAsync(writer.CreateMessage()).WaitAsync(Timeout);
            return true;
        }
        catch (DBusExceptionBase exception)
        {
            Trace.WriteLine(exception.Message, nameof(TryActivateAsync));
            return false;
        }
    }

    private sealed class Handler : IPathMethodHandler
    {
        private readonly Action<string> _invoke;
        private readonly Func<long> _lastActivation;

        internal Handler(Action<string> invoke, Func<long> lastActivation)
        {
            _invoke = invoke;
            _lastActivation = lastActivation;
        }

        public string Path => ObjectPath;

        public bool HandlesChildPaths => false;

        public ValueTask HandleMethodAsync(MethodContext context)
        {
            Message request = context.Request;
            if (request.InterfaceAsString != Interface)
            {
                context.ReplyUnknownMethodError();
                return default;
            }

            switch (request.MemberAsString)
            {
                case "Activate" when request.SignatureAsString == "s":
                    _invoke(request.GetBodyReader().ReadString());
                    context.Reply(context.CreateReplyWriter(null).CreateMessage());
                    break;
                case "GetLastActivation":
                    MessageWriter writer = context.CreateReplyWriter("x");
                    writer.WriteInt64(_lastActivation());
                    context.Reply(writer.CreateMessage());
                    break;
                default:
                    context.ReplyUnknownMethodError();
                    break;
            }

            return default;
        }
    }
}
