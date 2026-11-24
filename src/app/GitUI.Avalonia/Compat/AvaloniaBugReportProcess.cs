using System.Diagnostics;
using System.Reflection;

namespace GitUI.Compat;

// Framework constraint: the portable apphost runs the independent BugReporter role instead of a Windows-only executable.
internal static class AvaloniaBugReportProcess
{
    internal const string Command = "--bugreport";

    // parity-scaffolding: substitutes only native process I/O when testing the real fatal callback.
    private static Action<ProcessStartInfo>? _startForTesting;

    // parity-scaffolding: substitutes command-line I/O for the real independent receiver in a headless test.
    private static Func<string[]>? _argumentsForTesting;

    /// <summary>
    ///  Starts the independent report process with the original serialized exception payload.
    /// </summary>
    internal static void Start(string encoded)
    {
        string processPath = Environment.ProcessPath
            ?? throw new InvalidOperationException("The error reporter cannot find the current application executable.");
        ProcessStartInfo start = new(processPath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        if (string.Equals(Path.GetFileNameWithoutExtension(processPath), "dotnet", StringComparison.OrdinalIgnoreCase))
        {
            string? entry = Assembly.GetEntryAssembly()?.Location;
            if (string.IsNullOrEmpty(entry))
            {
                throw new InvalidOperationException("The error reporter cannot find the current application entry assembly.");
            }

            start.ArgumentList.Add(entry);
        }

        start.ArgumentList.Add(Command);
        start.ArgumentList.Add(encoded);
        if (_startForTesting is not null)
        {
            _startForTesting(start);
            return;
        }

        using Process reporter = Process.Start(start)
            ?? throw new InvalidOperationException("The independent error reporter could not start.");
    }

    internal static string[] GetCommandLineArgs() => _argumentsForTesting?.Invoke() ?? Environment.GetCommandLineArgs();

    // parity-scaffolding: observes production-provided process inputs without starting a real app from a headless test.
    internal readonly struct TestAccessor
    {
        internal static Func<string[]>? CommandLineArguments
        {
            get => _argumentsForTesting;
            set => _argumentsForTesting = value;
        }

        internal static Action<ProcessStartInfo>? StartProcess
        {
            get => _startForTesting;
            set => _startForTesting = value;
        }
    }
}
