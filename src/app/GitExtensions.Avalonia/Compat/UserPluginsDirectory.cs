using System.Diagnostics;
using System.Security;

namespace GitExtensions.Compat;

internal static class UserPluginsDirectory
{
    private const string ApplicationDirectoryName = "GitExtensions";
    internal const string DirectoryName = "UserPlugins.Avalonia";

    public static string? GetPath(string? localApplicationDataPath)
        => GetPath(
            localApplicationDataPath,
            Environment.GetEnvironmentVariable("FLATPAK_ID"),
            Environment.GetEnvironmentVariable("XDG_DATA_HOME"),
            EnsureAccessible,
            TraceFailure);

    private static string? GetPath(
        string? localApplicationDataPath,
        string? flatpakId,
        string? xdgDataHome,
        Action<string> ensureAccessible,
        Action<string> traceFailure)
    {
        string? applicationDataPath = localApplicationDataPath;
        if (!string.IsNullOrWhiteSpace(flatpakId))
        {
            if (string.IsNullOrWhiteSpace(xdgDataHome))
            {
                traceFailure("User plugin discovery is disabled because XDG_DATA_HOME is unavailable inside Flatpak.");
                return null;
            }

            applicationDataPath = Path.Join(xdgDataHome, ApplicationDirectoryName);
        }

        if (string.IsNullOrWhiteSpace(applicationDataPath))
        {
            traceFailure("User plugin discovery is disabled because the local application-data directory is unavailable.");
            return null;
        }

        string path = Path.Join(applicationDataPath, DirectoryName);
        try
        {
            ensureAccessible(path);
            return path;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SecurityException)
        {
            traceFailure($"User plugin discovery is disabled because '{path}' is inaccessible: {exception.Message}");
            return null;
        }
    }

    private static void EnsureAccessible(string path)
    {
        Directory.CreateDirectory(path);
        using IEnumerator<string> entries = Directory.EnumerateFileSystemEntries(path).GetEnumerator();
        _ = entries.MoveNext();
    }

    private static void TraceFailure(string message)
    {
        Trace.TraceWarning(message);
        Console.Error.WriteLine(message);
    }

    // parity-scaffolding: exposes deterministic environment and filesystem boundaries to focused tests.
    internal readonly struct TestAccessor
    {
        public string? GetPath(
            string? localApplicationDataPath,
            string? flatpakId,
            string? xdgDataHome,
            Action<string> ensureAccessible,
            Action<string> traceFailure)
            => UserPluginsDirectory.GetPath(
                localApplicationDataPath,
                flatpakId,
                xdgDataHome,
                ensureAccessible,
                traceFailure);
    }

    internal static TestAccessor GetTestAccessor() => new();
}
