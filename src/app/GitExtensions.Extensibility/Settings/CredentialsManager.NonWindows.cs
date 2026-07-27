using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace GitExtensions.Extensibility.Settings;

public interface ICredentialsManager
{
    void Save();
}

internal class CredentialsManager : ICredentialsManager
{
    private static ConcurrentDictionary<string, PendingCredential> Credentials { get; } = new();

    private readonly Func<string?>? _getWorkingDir;
    private readonly IGitCredentialProcess _credentialProcess;

    public CredentialsManager()
        : this(getWorkingDir: null, new GitCredentialProcess())
    {
    }

    protected internal CredentialsManager(Func<string?> getWorkingDir)
        : this(getWorkingDir, new GitCredentialProcess())
    {
    }

    internal CredentialsManager(Func<string?>? getWorkingDir, IGitCredentialProcess credentialProcess)
    {
        _getWorkingDir = getWorkingDir;
        _credentialProcess = credentialProcess;
    }

    public void Save()
    {
        List<KeyValuePair<string, PendingCredential>> credentials = [.. Credentials];
        if (credentials.Count == 0)
        {
            return;
        }

        Credentials.Clear();

        foreach ((string target, PendingCredential pending) in credentials)
        {
            NetworkCredential? credential = pending.Credential;
            string? input = CreateCredentialInput(target, credential);
            if (input is null)
            {
                continue;
            }

            _credentialProcess.Run(
                credential is null ? "reject" : "approve",
                input,
                pending.WorkingDirectory);
        }
    }

    protected internal NetworkCredential GetCredentialOrDefault(SettingLevel settingLevel, string name, NetworkCredential defaultValue)
    {
        string? target = GetCredentialsTarget(name, settingLevel);
        if (string.IsNullOrWhiteSpace(target))
        {
            return defaultValue;
        }

        if (Credentials.TryGetValue(target, out PendingCredential queuedCredential))
        {
            return queuedCredential.Credential ?? defaultValue;
        }

        GitCredentialProcessResult result = _credentialProcess.Run(
            "fill",
            CreateCredentialInput(target, credential: null)!,
            GetWorkingDirectory());
        return result.ExitCode == 0 && TryParseCredential(result.Output, out NetworkCredential? credential)
            ? credential!
            : defaultValue;
    }

    protected internal void SetCredentials(SettingLevel settingLevel, string name, NetworkCredential? value)
    {
        string? target = GetCredentialsTarget(name, settingLevel);
        ArgumentNullException.ThrowIfNull(target);
        PendingCredential pending = new(value, GetWorkingDirectory());
        Credentials.AddOrUpdate(target, pending, (_, _) => pending);
    }

    private static string? CreateCredentialInput(string target, NetworkCredential? credential)
    {
        if (credential is not null
            && (ContainsLineBreak(credential.UserName) || ContainsLineBreak(credential.Password)))
        {
            return null;
        }

        string hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(target)));
        StringBuilder input = new();
        input.Append("protocol=https\n");
        input.Append($"host={hash[..32]}.{hash[32..]}.credentials.gitextensions\n");
        if (credential is not null)
        {
            input.Append($"username={credential.UserName}\n");
            input.Append($"password={credential.Password}\n");
        }

        input.Append('\n');
        return input.ToString();
    }

    private static bool ContainsLineBreak(string value)
        => value.Contains('\r') || value.Contains('\n');

    private static bool TryParseCredential(string output, out NetworkCredential? credential)
    {
        string? userName = null;
        string? password = null;
        using StringReader reader = new(output);
        while (reader.ReadLine() is { } line)
        {
            int separator = line.IndexOf('=');
            if (separator < 0)
            {
                continue;
            }

            string key = line[..separator];
            string value = line[(separator + 1)..];
            if (key == "username")
            {
                userName = value;
            }
            else if (key == "password")
            {
                password = value;
            }
        }

        credential = userName is not null && password is not null
            ? new NetworkCredential(userName, password)
            : null;
        return credential is not null;
    }

    private string? GetCredentialsTarget(string name, SettingLevel settingLevel)
    {
        if (settingLevel == SettingLevel.Global)
        {
            return name;
        }

        ArgumentNullException.ThrowIfNull(_getWorkingDir);
        string? suffix = _getWorkingDir();
        return string.IsNullOrWhiteSpace(suffix) ? null : $"{name}_{suffix}";
    }

    private string? GetWorkingDirectory()
    {
        string? workingDirectory = _getWorkingDir?.Invoke();
        return Directory.Exists(workingDirectory) ? workingDirectory : null;
    }

    private readonly record struct PendingCredential(NetworkCredential? Credential, string? WorkingDirectory);
}

internal interface IGitCredentialProcess
{
    GitCredentialProcessResult Run(string operation, string input, string? workingDirectory);
}

internal readonly record struct GitCredentialProcessResult(int ExitCode, string Output);

internal sealed class GitCredentialProcess : IGitCredentialProcess
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    public GitCredentialProcessResult Run(string operation, string input, string? workingDirectory)
    {
        ProcessStartInfo startInfo = new("git")
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        if (workingDirectory is not null)
        {
            startInfo.WorkingDirectory = workingDirectory;
        }

        startInfo.ArgumentList.Add("credential");
        startInfo.ArgumentList.Add(operation);
        startInfo.Environment["GCM_INTERACTIVE"] = "Never";
        startInfo.Environment["GIT_ASKPASS"] = string.Empty;
        startInfo.Environment["GIT_TERMINAL_PROMPT"] = "0";

        Process? process = null;
        try
        {
            process = Process.Start(startInfo);
            if (process is null)
            {
                return new GitCredentialProcessResult(-1, string.Empty);
            }

            StringBuilder output = new();
            process.OutputDataReceived += (_, args) =>
            {
                if (args.Data is not null)
                {
                    output.AppendLine(args.Data);
                }
            };
            process.ErrorDataReceived += (_, _) => { };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.StandardInput.Write(input);
            process.StandardInput.Close();

            if (!process.WaitForExit((int)Timeout.TotalMilliseconds))
            {
                TryKill(process);
                return new GitCredentialProcessResult(-1, string.Empty);
            }

            process.WaitForExit();
            return new GitCredentialProcessResult(process.ExitCode, output.ToString());
        }
        catch (Exception exception) when (exception is IOException
                                            or InvalidOperationException
                                            or OperationCanceledException
                                            or System.ComponentModel.Win32Exception)
        {
            TryKill(process);
            return new GitCredentialProcessResult(-1, string.Empty);
        }
        finally
        {
            process?.Dispose();
        }
    }

    private static void TryKill(Process? process)
    {
        try
        {
            if (process is { HasExited: false })
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception exception) when (exception is InvalidOperationException
                                            or NotSupportedException
                                            or System.ComponentModel.Win32Exception)
        {
        }
    }
}
