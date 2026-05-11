using GitCommands.Logging;

namespace GitUI.ConsoleEmulation.Mintty;

internal sealed class MinttyStartInfo
{
    private readonly Dictionary<string, string> _environmentVariables = new();

    internal string ConsoleProcessCommandLine { get; init; } = "";

    internal required string StartupDirectory { get; init; }

    internal IReadOnlyDictionary<string, string> EnvironmentVariables => _environmentVariables;

    internal Action<int>? ProcessExitedCallback { get; init; }

    internal Action? ConsoleClosedCallback { get; init; }

    internal required ProcessOperation ProcessOperation { get; init; }

    internal Action<string>? AnsiOutputLineCallback { get; init; }

    internal void SetEnv(string name, string value) => _environmentVariables[name] = value;
}
