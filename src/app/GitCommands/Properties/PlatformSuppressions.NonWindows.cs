using System.Diagnostics.CodeAnalysis;

// The Windows-only command-line lookup is called from a lambda inside an OperatingSystem.IsWindows()
// branch. The platform analyzer does not carry that guard into the lambda, so the portable
// target reports a call site that cannot run off Windows.
[assembly: SuppressMessage(
    "Interoperability",
    "CA1416:Validate platform compatibility",
    Justification = "The call is inside an OperatingSystem.IsWindows() branch.",
    Scope = "member",
    Target = "~M:GitCommands.GitModule.IsRunningGitProcess~System.Boolean")]
