using GitCommands.Logging;

namespace GitUI.ConsoleEmulation.Mintty;

internal sealed class MinttyStartInfo
{
    public string ConsoleProcessCommandLine { get; init; } = "";

    public required string StartupDirectory { get; init; }

    public Dictionary<string, string> EnvironmentVariables { get; } = [];

    public Action<int>? ProcessExitedCallback { get; init; }

    public Action? ConsoleClosedCallback { get; init; }

    public required ProcessOperation ProcessOperation { get; init; }

    public Action<string>? AnsiOutputLineCallback { get; init; }
}
