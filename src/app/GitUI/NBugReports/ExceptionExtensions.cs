using System.Text;
using GitExtensions.Extensibility;

namespace GitUI.NBugReports;

internal static class ExceptionExtensions
{
    /// <summary>
    /// Get the exception data.
    /// </summary>
    /// <param name="exception">An Exception to describe.</param>
    internal static StringBuilder GetExceptionInfo(this Exception exception)
    {
        StringBuilder text = new();

        if (exception is UserExternalOperationException userExternalOperationException && !string.IsNullOrWhiteSpace(userExternalOperationException.Context))
        {
            // Context contains an error message as UserExternalOperationException is currently used. So append just "<context>"
            text.AppendLine(userExternalOperationException.Context);
        }

        if (exception is ExternalOperationException externalOperationException)
        {
            // Exit code: <n>
            if (externalOperationException.ExitCode is int exitCode)
            {
                AppendIfNotEmpty(ExecutionResult.FormatExitCode(exitCode), TranslatedStrings.ExitCode);
            }

            // Command: <command>
            AppendIfNotEmpty(externalOperationException.Command, TranslatedStrings.Command);

            // Arguments: <args>
            AppendIfNotEmpty(externalOperationException.Arguments, TranslatedStrings.Arguments);

            // Directory: <dir>
            AppendIfNotEmpty(externalOperationException.WorkingDirectory, TranslatedStrings.WorkingDirectory);
        }

        return text;

        void AppendIfNotEmpty(string? value, string designation)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                text.Append(designation).Append(": ").AppendLine(value);
            }
        }
    }
}
