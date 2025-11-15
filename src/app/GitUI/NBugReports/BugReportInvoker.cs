using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Text;
using BugReporter.Serialization;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;

namespace GitUI.NBugReports;

public static class BugReportInvoker
{
    public const string DubiousOwnershipSecurityConfigString = "config --global --add safe.directory";

    private static IBugReporter _bugReporter = new UIReporter();
    private static bool _isReportingDubiousOwnershipSecurity;

    /// <summary>
    /// Gets the root error.
    /// </summary>
    /// <param name="exception">An Exception to describe.</param>
    /// <returns>The inner-most exception message.</returns>
    private static string GetRootError(Exception exception)
    {
        string rootError = exception.Message;
        for (Exception innerException = exception.InnerException; innerException is not null; innerException = innerException.InnerException)
        {
            if (!string.IsNullOrEmpty(innerException.Message))
            {
                rootError = innerException.Message;
            }
        }

        return rootError;
    }

    // Determines if the assembly name is a .NET framework assembly.
    // .NET framework assemblies are typically affected by Patch Tuesday updates,
    // causing transient FileNotFoundException errors that are resolved by restarting the application.
    private static bool IsDotNetFrameworkAssembly(string assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return false;
        }

        // .NET framework assemblies typically start with "System." or "Microsoft."
        // These are the assemblies commonly affected by Patch Tuesday updates
        return assemblyName.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
            || assemblyName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase);
    }

    // Determines if the DLL name is a VC Runtime DLL.
    // VC Runtime DLL loading errors are typically caused by Windows updates.
    private static bool IsVCRuntimeDll(string dllName)
    {
        if (string.IsNullOrWhiteSpace(dllName))
        {
            return false;
        }

        // VC Runtime DLLs typically contain "vcruntime"
        return dllName.Contains("vcruntime", StringComparison.OrdinalIgnoreCase);
    }

    public static void LogError(Exception exception, bool isTerminating = false)
    {
        string tempFolder = Path.GetTempPath();
        string tempFileName = $"{AppSettings.ApplicationId}.{AppSettings.AppVersion}.{DateTime.Now.ToString("yyyyMMdd.HHmmssfff")}.log";
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

    public static void Report(Exception exception, bool isTerminating)
    {
        if (AppSettings.WriteErrorLog)
        {
            LogError(exception, isTerminating);
        }

        if (exception is FileNotFoundException fileNotFoundException
            && fileNotFoundException.Message.StartsWith("Could not load file or assembly"))
        {
            _bugReporter.ReportFailedToLoadAnAssembly(
                fileNotFoundException,
                isTerminating,
                isRuntimeAssembly: IsDotNetFrameworkAssembly(fileNotFoundException.FileName) || IsVCRuntimeDll(fileNotFoundException.FileName));
            return;
        }

        // Handle VC Runtime DLL loading errors (refer to https://github.com/gitextensions/gitextensions/issues/12511)
        // These are transient errors typically caused by Windows updates, similar to .NET assembly loading errors
        if (exception is DllNotFoundException dllNotFoundException && IsVCRuntimeDll(dllNotFoundException.Message))
        {
            _bugReporter.ReportFailedToLoadAnAssembly(
                dllNotFoundException,
                isTerminating,
                isRuntimeAssembly: true);
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

        ExternalOperationException externalOperationException = exception as ExternalOperationException;

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
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(xml);
            Process.Start("BugReporter.exe", Convert.ToBase64String(plainTextBytes));

            return;
        }

        bool isUserExternalOperation = exception is UserExternalOperationException
                                                 or GitConfigFormatException;
        bool isExternalOperation = exception is ExternalOperationException
                                             or IOException
                                             or SecurityException
                                             or FileNotFoundException
                                             or DirectoryNotFoundException
                                             or PathTooLongException
                                             or Win32Exception;
        bool isMsysGitIssue = externalOperationException is null &&
                              exception.Message.Contains("mingw64/libexec", StringComparison.InvariantCultureIgnoreCase);

        bool isGitIssue = false;

        // Treat all git errors as user issues
        if (string.Equals(AppSettings.GitCommand, externalOperationException?.Command, StringComparison.InvariantCultureIgnoreCase)
         || string.Equals(AppSettings.WslCommand, externalOperationException?.Command, StringComparison.InvariantCultureIgnoreCase)
         || isMsysGitIssue)
        {
            isUserExternalOperation = true;
            isGitIssue = true;
        }

        StringBuilder text = exception.GetExceptionInfo();
        string rootError = GetRootError(exception);

        _bugReporter.ReportError(
            exception,
            rootError,
            text,
            new()
            {
                IsExternalOperation = isExternalOperation,
                IsGitOperation = isGitIssue,
                IsUserExternalOperation = isUserExternalOperation,
            });
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

    internal readonly struct TestAccessor
    {
        internal static IBugReporter BugReporterInstance
        {
            get => BugReportInvoker._bugReporter;
            set => BugReportInvoker._bugReporter = value;
        }

        internal static bool IsDotNetFrameworkAssembly(string assemblyName) => BugReportInvoker.IsDotNetFrameworkAssembly(assemblyName);
        internal static bool IsVCRuntimeDll(string dllName) => BugReportInvoker.IsVCRuntimeDll(dllName);
        internal static string GetRootError(Exception exception) => BugReportInvoker.GetRootError(exception);
    }
}
