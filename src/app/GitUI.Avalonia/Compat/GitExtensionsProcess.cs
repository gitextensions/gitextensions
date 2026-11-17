using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

/// <summary>
///  Launches another instance through the existing executable boundary.
/// </summary>
internal static class GitExtensionsProcess
{
    // parity-scaffolding: intercept the external process without replacing command routing.
    internal static Func<string, string, IProcess>? StartForTesting { get; set; }

    internal static IProcess Start(string arguments, string workingDirectory)
        => StartForTesting is { } start ? start(arguments, workingDirectory)
            : new Executable(Application.ExecutablePath, workingDirectory).Start(arguments);
}
