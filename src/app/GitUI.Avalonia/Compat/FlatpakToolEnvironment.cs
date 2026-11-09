using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using GitCommands.DiffMergeTools;

namespace GitUI.Compat;

/// <summary>
///  Installs sandbox-local launch proxies and host-visible temporary storage without changing saved Git settings.
/// </summary>
public static class FlatpakToolEnvironment
{
    internal const string ProxyDirectoryVariable = "GITEXTENSIONS_HOST_PROXY_DIR";
    internal const string ErrorVariable = "GITEXTENSIONS_HOST_TOOLS_ERROR";

    public static Task<bool> InitializeAsync()
        => InitializeAsync(FlatpakEnvironment.IsFlatpak(), Environment.GetEnvironmentVariable,
            Environment.SetEnvironmentVariable, FlatpakHost.QueryAsync);

    public static string GetBuiltInEditorCommand()
        => PlatformToolLocator.GetEditorCommand(
            new LocatedTool("flatpak", ["run", Environment.GetEnvironmentVariable("FLATPAK_ID") ?? "com.github.gitextensions.GitExtensions.Avalonia"]), "fileeditor");

    internal static async Task<bool> InitializeAsync(
        bool isFlatpak,
        Func<string, string?> getEnvironment,
        Action<string, string?> setEnvironment,
        Func<ProcessStartInfo, Task<(int ExitCode, string Output, string Error)>> query)
    {
        if (!isFlatpak)
        {
            return false;
        }

        try
        {
            string appCache = getEnvironment("XDG_CACHE_HOME")
                ?? throw new IOException("The application cache directory is unavailable.");
            if (!Path.IsPathFullyQualified(appCache))
            {
                throw new IOException("The application cache directory is not an absolute path.");
            }

            string proxyDirectory = Path.Combine(appCache, "GitExtensions", "HostTools", "bin");
            CreatePrivateDirectory(proxyDirectory);
            string[] knownExecutables = [.. GetKnownExecutables()];
            setEnvironment(ProxyDirectoryVariable, proxyDirectory);
            ProcessStartInfo request = new("/bin/sh");
            request.ArgumentList.Add("-c");
            request.ArgumentList.Add("printf '%s\\0%s\\0' \"${XDG_CACHE_HOME:-$HOME/.cache}\" \"$HOME/.cache\"; for name do if command -v \"$name\" >/dev/null 2>&1; then printf '%s\\0' \"$name\"; fi; done");
            request.ArgumentList.Add("gitextensions-tool-environment");
            foreach (string executable in knownExecutables)
            {
                request.ArgumentList.Add(executable);
            }

            FlatpakHost.ForwardToHost(request);
            (int exitCode, string output, string error) = await query(request);
            if (exitCode != 0)
            {
                throw new IOException("The host tool environment is unavailable. " + error);
            }

            string[] fields = output.Split('\0');
            if (fields.Length < 2)
            {
                throw new IOException("The host cache directories are unavailable.");
            }

            HashSet<string> installed = fields.Skip(2).ToHashSet(StringComparer.Ordinal);
            foreach (string executable in knownExecutables)
            {
                if (Path.GetFileName(executable) != executable || executable is "." or "..")
                {
                    throw new IOException("An external-tool name is not a file name.");
                }

                string proxy = Path.Combine(proxyDirectory, executable);
                if (!installed.Contains(executable))
                {
                    File.Delete(proxy);
                    continue;
                }

                string script = "#!/bin/sh\nexec \"${GITEXTENSIONS_HOST_TOOL:-/app/bin/gitextensions-host-tool}\" "
                    + QuoteShell(executable) + " \"$@\"\n";
                string temporaryProxy = proxy + "." + Guid.NewGuid().ToString("N");
                try
                {
                    File.WriteAllText(temporaryProxy, script);
                    if (!OperatingSystem.IsWindows())
                    {
                        File.SetUnixFileMode(temporaryProxy, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
                    }

                    File.Move(temporaryProxy, proxy, overwrite: true);
                }
                finally
                {
                    File.Delete(temporaryProxy);
                }
            }

            string appId = getEnvironment("FLATPAK_ID") ?? "com.github.gitextensions.GitExtensions.Avalonia";
            if (appId.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '.' and not '-' and not '_'))
            {
                throw new IOException("The Flatpak application ID is invalid.");
            }

            string temporaryDirectory = await CreateSharedTemporaryDirectoryAsync(string.Join('\0', fields.Take(2)), appId, query);
            setEnvironment("TMPDIR", temporaryDirectory);
            setEnvironment(ErrorVariable, null);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or PlatformNotSupportedException or Win32Exception)
        {
            string message = "Host tools are not available in this Flatpak installation. " + exception.Message;
            Trace.TraceWarning(message);
            setEnvironment(ErrorVariable, message);
            return false;
        }
    }

    private static async Task<string> CreateSharedTemporaryDirectoryAsync(
        string candidates,
        string appId,
        Func<ProcessStartInfo, Task<(int ExitCode, string Output, string Error)>> query)
    {
        foreach (string cacheDirectory in candidates.Split('\0', StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.Ordinal))
        {
            if (!Path.IsPathRooted(cacheDirectory))
            {
                continue;
            }

            string temporaryDirectory = Path.Combine(cacheDirectory, "GitExtensions", "ExternalTools", appId, "tmp");
            string nonce = Guid.NewGuid().ToString("N");
            string marker = Path.Combine(temporaryDirectory, nonce + ".probe");
            bool created = false;
            try
            {
                CreatePrivateDirectory(temporaryDirectory);
                await using (FileStream stream = new(marker, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                {
                    created = true;
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(nonce));
                }

                ProcessStartInfo request = new("/bin/cat");
                request.ArgumentList.Add(marker);
                FlatpakHost.ForwardToHost(request);
                (int exitCode, string output, _) = await query(request);
                if (exitCode == 0 && output == nonce)
                {
                    return temporaryDirectory;
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            finally
            {
                if (created)
                {
                    File.Delete(marker);
                }
            }
        }

        throw new IOException("No host cache directory exposes the same temporary files inside this sandbox.");
    }

    private static IEnumerable<string> GetKnownExecutables()
        => RegisteredDiffMergeTools.All(DiffMergeToolType.Diff)
            .Concat(RegisteredDiffMergeTools.All(DiffMergeToolType.Merge))
            .Select(tool => PlatformToolLocator.GetUnixName(new DiffMergeToolConfigurationManager(() => null).LoadDiffMergeToolConfig(tool, null).ExeFileName))
            .Concat(["vi", "vim", "nano"])
            .Distinct(StringComparer.Ordinal);

    private static void CreatePrivateDirectory(string path)
    {
        Directory.CreateDirectory(path);
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }

    internal static string QuoteShell(string text)
        => "'" + text.Replace("'", "'\\''", StringComparison.Ordinal) + "'";
}
