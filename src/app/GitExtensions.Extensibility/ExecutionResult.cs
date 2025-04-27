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

    public ExecutionResult(IExecutable executable, ArgumentString arguments, string standardOutput, string standardError, int? exitCode)
    {
        Command = executable.Command;
        WorkingDir = executable.WorkingDir;
        Arguments = arguments;
        StandardOutput = standardOutput;
        StandardError = standardError;
        ExitCode = exitCode;
    }

    /// <summary>
    ///  Gets a verbose string with the <see cref="ExitCode"/> using <see cref="FormatExitCode"/>.
    /// </summary>
    public string? ExitCodeDisplay => ExitCode is int exitCode ? FormatExitCode(exitCode) : "null";

    public bool ExitedSuccessfully => ExitCode == Success;

    public string AllOutput => string.Concat(StandardOutput, Environment.NewLine, StandardError);

    /// <summary>
    ///  Returns a verbose string of an <paramref name="exitCode"/>.
    ///  Absolute values from 128 are presented as hexadecimal and decimal numbers.
    /// </summary>
    public static string FormatExitCode(int exitCode) => Math.Abs(exitCode) <= 128 ? $"{exitCode}" : $"0x{exitCode:X8} ({exitCode} dec)";

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
