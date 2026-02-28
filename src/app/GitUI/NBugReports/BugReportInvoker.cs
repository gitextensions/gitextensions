#nullable enable

using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Text;
using BugReporter.Serialization;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;

namespace GitUI.NBugReports;

/// <summary>
///  Provides methods for reporting unhandled exceptions to the user
///  via UI dialogs or by launching the external bug reporter process.
/// </summary>
public static class BugReportInvoker
{
    /// <summary>
    ///  The git config command fragment used to detect dubious ownership security errors.
    /// </summary>
    public const string DubiousOwnershipSecurityConfigString = "config --global --add safe.directory";

    private static readonly IBugReporter _bugReporter = new UIReporter();
    private static bool _isReportingDubiousOwnershipSecurity;

    /// <summary>
    ///  Set to <see langword="true" /> on application exit
    ///  in order to suppress the popup to restart the app on missing runtime assembly.
    /// </summary>
    public static bool IgnoreFailedToLoadAnAssembly
    {
        get => UIReporter.IgnoreFailedToLoadAnAssembly;
        set => UIReporter.IgnoreFailedToLoadAnAssembly = value;
    }

    /// <summary>
    ///  Gets the root error.
    /// </summary>
    /// <param name="exception">An Exception to describe.</param>
    /// <returns>The inner-most exception message.</returns>
    internal static string GetRootError(Exception exception)
    {
        string rootError = exception.Message;
        for (Exception? innerException = exception.InnerException; innerException is not null; innerException = innerException.InnerException)
        {
            if (!string.IsNullOrEmpty(innerException.Message))
            {
                rootError = innerException.Message;
            }
        }

        return rootError;
    }

    /// <summary>
    ///  Logs the exception details to a temporary file.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="isTerminating">Indicates whether the exception is terminating the application.</param>
    public static void LogError(Exception exception, bool isTerminating = false)
    {
        string tempFolder = Path.GetTempPath();
        string tempFileName = $"{AppSettings.ApplicationId}.{AppSettings.AppVersion}.{DateTime.Now:yyyyMMdd.HHmmssfff}.log";
        string tempFile = Path.Combine(tempFolder, tempFileName);

        try
        {
            string content = $"Is fatal: {isTerminating}\r\n{exception}";
            File.WriteAllText(tempFile, content);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to log error to {tempFile}\r\n{ex.Message}", "Error writing log", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    ///  Reports the specified exception to the user via the appropriate UI dialog.
    /// </summary>
    /// <param name="exception">The exception to report.</param>
    /// <param name="isTerminating">Indicates whether the exception is terminating the application.</param>
    public static void Report(Exception exception, bool isTerminating)
    {
        if (AppSettings.WriteErrorLog)
        {
            LogError(exception, isTerminating);
        }

        if (_bugReporter.ReportFailedToLoadAnAssembly(exception, isTerminating))
        {
            return;
        }

        // Ignore accessibility-specific exception (refer to https://github.com/gitextensions/gitextensions/issues/11385)
        if (exception is InvalidOperationException && exception.StackTrace?.Contains("ListViewGroup.get_AccessibilityObject") is true)
        {
            Trace.WriteLine(exception);
            return;
        }

        // Do not report cancellation of async implementations awaited by the UI thread (refer to https://github.com/gitextensions/gitextensions/issues/11636)
        if (exception is OperationCanceledException or TaskCanceledException)
        {
            Debug.WriteLine(exception);
            return;
        }

        ExternalOperationException? externalOperationException = exception as ExternalOperationException;

        if (externalOperationException?.InnerException?.Message?.Contains(DubiousOwnershipSecurityConfigString) is true)
        {
            ReportDubiousOwnership(externalOperationException);
            return;
        }

        if (isTerminating)
        {
            // TODO: this is not very efficient
            // GetExceptionInfo() info is lost
            SerializableException serializableException = new(exception);
            string xml = serializableException.ToXmlString();
            string encoded = Base64Encode(xml);
            Process.Start("BugReporter.exe", encoded);

            return;
        }

        bool isUserExternalOperation
            = exception is UserExternalOperationException
                        or GitConfigFormatException

            // Treat all git errors as user issues
            || string.Equals(AppSettings.GitCommand, externalOperationException?.Command, StringComparison.InvariantCultureIgnoreCase)
            || string.Equals(AppSettings.WslCommand, externalOperationException?.Command, StringComparison.InvariantCultureIgnoreCase);
        bool isExternalOperation = isUserExternalOperation
            || exception is ExternalOperationException
                         or IOException
                         or SecurityException
                         or FileNotFoundException
                         or DirectoryNotFoundException
                         or PathTooLongException
                         or Win32Exception;

        StringBuilder text = exception.GetExceptionInfo();
        string rootError = GetRootError(exception);

        _bugReporter.ReportError(
            exception,
            rootError,
            text,
            new()
            {
                IsExternalOperation = isExternalOperation,
                IsUserExternalOperation = isUserExternalOperation,
            });

        return;
    }

    private static void ReportDubiousOwnership(ExternalOperationException exception)
    {
        if (_isReportingDubiousOwnershipSecurity)
        {
            return;
        }

        try
        {
            _isReportingDubiousOwnershipSecurity = true;
            _bugReporter.ReportDubiousOwnership(exception);
        }
        finally
        {
            _isReportingDubiousOwnershipSecurity = false;
        }
    }

    private static string Base64Encode(string plainText)
    {
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
}
