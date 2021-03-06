using System;

namespace GitUIPluginInterfaces
{
    public readonly struct ExecutionResult
    {
        public string StandardOutput { get; }
        public string StandardError { get; }
        public int? ExitCode { get; }

        public ExecutionResult(string standardOutput, string standardError, int? exitCode)
        {
            StandardOutput = standardOutput;
            StandardError = standardError;
            ExitCode = exitCode;
        }

        public bool ExitedSuccessfully => ExitCode == 0;

        public string AllOutput => string.Concat(StandardOutput, Environment.NewLine, StandardError);
    }
}
