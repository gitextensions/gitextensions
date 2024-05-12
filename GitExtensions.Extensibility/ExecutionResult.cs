using GitExtUtils;

namespace GitExtensions.Extensibility;

public readonly struct ExecutionResult
{
    public const int Success = 0;

    public string Command { get; }
    public string Arguments { get; }
    public string WorkingDir { get; }
    public string StandardOutput { get; }
    public string StandardError { get; }
    public int? ExitCode { get; }

    public ExecutionResult(IExecutable executable, string arguments, string standardOutput, string standardError, int? exitCode)
    {
        Command = executable.Command;
        WorkingDir = executable.WorkingDir;
        Arguments = arguments;
        StandardOutput = standardOutput;
        StandardError = standardError;
        ExitCode = exitCode;
    }

    public bool ExitedSuccessfully => ExitCode == Success;

    public string AllOutput => string.Concat(StandardOutput, Environment.NewLine, StandardError);

    public void ThrowIfErrorExit(string? errorMessage = null)
    {
        if (ExitedSuccessfully)
        {
            return;
        }

        string output = string.Concat(errorMessage, Environment.NewLine, StandardError, Environment.NewLine, StandardOutput);
        throw new ExternalOperationException(Command, Arguments, WorkingDir, ExitCode, new Exception(output));
    }
}
