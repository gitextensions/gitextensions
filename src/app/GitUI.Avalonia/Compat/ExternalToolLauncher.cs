using System.Diagnostics;
using GitCommands;
using GitExtensions.Extensibility;

namespace GitUI.Compat;

/// <summary>
///  Retains native process argument handling while crossing the explicitly granted Flatpak host boundary.
/// </summary>
public sealed class ExternalToolLauncher
{
    private static ExternalToolLauncher _current = new();
    private readonly Func<bool> _isFlatpak;
    private readonly Func<string, string?> _resolveHost;
    private readonly Func<string, string, IExecutable> _createExecutable;

    public ExternalToolLauncher()
        : this(FlatpakEnvironment.IsFlatpak, FlatpakHost.Default.ResolveExecutable,
            (command, directory) => new Executable(command, directory))
    {
    }

    internal ExternalToolLauncher(Func<bool> isFlatpak, Func<string, string?> resolveHost,
        Func<string, string, IExecutable> createExecutable)
    {
        _isFlatpak = isFlatpak;
        _resolveHost = resolveHost;
        _createExecutable = createExecutable;
    }

    public static ExternalToolLauncher Current => _current;

    public bool TryFindFullPath(string? command, out string? path)
    {
        path = null;
        if (string.IsNullOrWhiteSpace(command))
        {
            return false;
        }

        if (!_isFlatpak())
        {
            return PathUtil.TryFindFullPath(command, out path);
        }

        if (Environment.GetEnvironmentVariable(FlatpakToolEnvironment.ErrorVariable) is string error)
        {
            throw new PlatformNotSupportedException(error);
        }

        path = _resolveHost(command);
        return path is not null;
    }

    public ProcessStartInfo CreateStartInfo(string command, string arguments, string workingDirectory)
    {
        (string executable, string translatedArguments) = PrepareLaunch(command, arguments);
        return new ProcessStartInfo(executable, translatedArguments)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
        };
    }

    internal IExecutable CreateExecutable(string command, string workingDirectory, ref string arguments)
    {
        (string executable, string translatedArguments) = PrepareLaunch(command, arguments);
        arguments = translatedArguments;
        return _createExecutable(executable, workingDirectory);
    }

    private (string Executable, string Arguments) PrepareLaunch(string command, string arguments)
    {
        if (!_isFlatpak())
        {
            return (command, arguments);
        }

        if (!TryFindFullPath(command, out string? path))
        {
            throw new FileNotFoundException("The configured host tool was not found.", command);
        }

        return (Path.Combine(AppContext.BaseDirectory, "gitextensions-host-tool"), path!.Quote() + " " + arguments);
    }

    // parity-scaffolding: substitutes only the host/process boundaries while exercising actual dialog command routing.
    internal static class TestAccessor
    {
        internal static ExternalToolLauncher Current
        {
            get => _current;
            set => _current = value;
        }
    }
}
