using System;
using JetBrains.Annotations;

namespace GitUIPluginInterfaces
{
    public readonly struct ExecutionResult
    {
        [CanBeNull] public string StandardOutput { get; }
        [CanBeNull] public string StandardError { get; }
        public int? ExitCode { get; }

        public ExecutionResult([NotNull] string standardOutput, [NotNull] string standardError, int? exitCode)
        {
            StandardOutput = standardOutput;
            StandardError = standardError;
            ExitCode = exitCode;
        }

        public bool ExitedSuccessfully => ExitCode == 0;

        [NotNull] public string AllOutput => string.Concat(StandardOutput, Environment.NewLine, StandardError);
    }
}