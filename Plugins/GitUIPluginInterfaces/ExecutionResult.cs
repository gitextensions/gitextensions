using System;

namespace GitUIPluginInterfaces
{
    public readonly struct ExecutionResult
    {
        public readonly string? StandardOutput { get; }
        public readonly string? StandardError { get; }
        public readonly int? ExitCode { get; }

        public ExecutionResult(string standardOutput, string standardError, int? exitCode)
        {
            StandardOutput = standardOutput;
            StandardError = standardError;
            ExitCode = exitCode;
        }

        public readonly bool ExitedSuccessfully => ExitCode == 0;

        public readonly string AllOutput => string.Concat(StandardOutput, Environment.NewLine, StandardError);
    }
}