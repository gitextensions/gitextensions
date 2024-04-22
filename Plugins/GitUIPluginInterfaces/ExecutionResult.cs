using GitExtUtils;

namespace GitUIPluginInterfaces
{
    public readonly struct ExecutionResult
    {
        public const int Success = 0;

        public string Arguments { get; }
        public string StandardOutput { get; }
        public string StandardError { get; }
        public int? ExitCode { get; }

        public ExecutionResult(string arguments, string standardOutput, string standardError, int? exitCode)
        {
            Arguments = arguments;
            StandardOutput = standardOutput;
            StandardError = standardError;
            ExitCode = exitCode;
        }

        public bool ExitedSuccessfully => ExitCode == Success;

        public string AllOutput => string.Concat(StandardOutput, Environment.NewLine, StandardError);

        public void ThrowIfErrorExit(string? command = null, string? workingDir = null, string? errorMessage = null)
        {
            if (ExitedSuccessfully)
            {
                return;
            }

            string output = string.Concat(errorMessage, Environment.NewLine, StandardError, Environment.NewLine, StandardOutput);
            throw new ExternalOperationException(command, Arguments, workingDir, ExitCode, new Exception(output));
        }
    }
}
