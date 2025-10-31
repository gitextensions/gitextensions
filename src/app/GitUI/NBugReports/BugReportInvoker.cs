using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Text;
using BugReporter;
using BugReporter.Serialization;
using GitCommands;
using GitCommands.Utils;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;
using GitUI.CommandsDialogs;

namespace GitUI.NBugReports;

public static class BugReportInvoker
{
    public const string DubiousOwnershipSecurityConfigString = "config --global --add safe.directory";

    private static bool _isReportingDubiousOwnershipSecurity;

    private static Form? OwnerForm
        => Form.ActiveForm ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null);

    private static IntPtr OwnerFormHandle
        => OwnerForm?.Handle ?? IntPtr.Zero;

    /// <summary>
    /// Gets the root error.
    /// </summary>
    /// <param name="exception">An Exception to describe.</param>
    /// <returns>The inner-most exception message.</returns>
    internal static string GetRootError(Exception exception)
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

    /// <summary>
    /// Get the exception data.
    /// </summary>
    /// <param name="exception">An Exception to describe.</param>
    internal static StringBuilder GetExceptionInfo(Exception exception)
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
            ReportFailedToLoadAnAssembly(fileNotFoundException, isTerminating);
            return;
        }

        // Handle VC Runtime DLL loading errors (refer to https://github.com/gitextensions/gitextensions/issues/12511)
        // These are transient errors typically caused by Windows updates, similar to .NET assembly loading errors
        if (exception is DllNotFoundException dllNotFoundException
            && IsVCRuntimeDll(dllNotFoundException.Message))
        {
            ReportFailedToLoadAnAssembly(dllNotFoundException, isTerminating);
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
            string encoded = Base64Encode(xml);
            Process.Start("BugReporter.exe", encoded);

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

        // Treat all git errors as user issues
        if (string.Equals(AppSettings.GitCommand, externalOperationException?.Command, StringComparison.InvariantCultureIgnoreCase)
         || string.Equals(AppSettings.WslCommand, externalOperationException?.Command, StringComparison.InvariantCultureIgnoreCase))
        {
            isUserExternalOperation = true;
        }

        StringBuilder text = GetExceptionInfo(exception);
        string rootError = GetRootError(exception);

        TaskDialogPage page = new()
        {
            Icon = isExternalOperation || isUserExternalOperation ? TaskDialogIcon.Warning : TaskDialogIcon.Error,
            Caption = TranslatedStrings.Error,
            Heading = rootError,
            AllowCancel = true,
            SizeToContent = true
        };

        // prefer to ignore failed external operations
        if (isExternalOperation)
        {
            AddIgnoreOrCloseButton(TranslatedStrings.ExternalErrorDescription);
        }
        else
        {
            // directions and button to raise a bug
            text.AppendLine().AppendLine(TranslatedStrings.ReportBug);
        }

        // no bug reports for user configured operations
        TaskDialogCommandLinkButton taskDialogCommandLink
            = isUserExternalOperation ? new(TranslatedStrings.ButtonViewDetails)
                : isExternalOperation ? new(TranslatedStrings.ReportIssue, TranslatedStrings.ReportIssueDescription)
                : new(TranslatedStrings.ButtonReportBug);
        taskDialogCommandLink.Click += (s, e) =>
        {
            ShowNBug(OwnerForm, exception, isExternalOperation, isTerminating);
        };
        page.Buttons.Add(taskDialogCommandLink);

        // let the user decide whether to report the bug
        if (!isExternalOperation)
        {
            AddIgnoreOrCloseButton();
        }

        page.Text = text.ToString().Trim();
        TaskDialog.ShowDialog(OwnerFormHandle, page);
        return;

        void AddIgnoreOrCloseButton(string descriptionText = null)
        {
            string buttonText = isTerminating ? TranslatedStrings.ButtonCloseApp : TranslatedStrings.ButtonIgnore;
            TaskDialogCommandLinkButton taskDialogCommandLink = new(buttonText, descriptionText);
            page.Buttons.Add(taskDialogCommandLink);
        }
    }

    private static void ReportFailedToLoadAnAssembly(Exception exception, bool isTerminating)
    {
        string fileName;

        if (exception is FileNotFoundException fileNotFoundException)
        {
            fileName = fileNotFoundException.FileName ?? "";
            int uninterestingIndex = fileName.IndexOf(", version=", StringComparison.InvariantCultureIgnoreCase);
            if (uninterestingIndex > 0)
            {
                fileName = fileName[..uninterestingIndex];
            }
        }
        else if (exception is DllNotFoundException dllNotFoundException)
        {
            // Extract DLL name from message like "Unable to load DLL 'vcruntime140_cor3.dll'"
            fileName = dllNotFoundException.Message;
            int startIndex = fileName.IndexOf('\'');
            if (startIndex >= 0)
            {
                int endIndex = fileName.IndexOf('\'', startIndex + 1);
                if (endIndex > startIndex)
                {
                    fileName = fileName.Substring(startIndex + 1, endIndex - startIndex - 1);
                }
            }
        }
        else
        {
            fileName = "unknown";
        }

        TaskDialogPage page = new()
        {
            Icon = TaskDialogIcon.Warning,
            Caption = TranslatedStrings.FailedToLoadAnAssembly,
            Heading = string.Format(TranslatedStrings.FailedToLoadFileOrAssemblyFormat, fileName),
            Text = TranslatedStrings.FailedToLoadFileOrAssemblyText,
            AllowCancel = false,
            SizeToContent = true,
        };

        TaskDialogCommandLinkButton restartButton = new(text: TranslatedStrings.RestartApplication, descriptionText: TranslatedStrings.RestartApplicationDescription);
        restartButton.Click += (_, _) => RestartGE();
        page.Buttons.Add(restartButton);

        // Only show the report button for non-.NET framework assemblies and non-VC Runtime DLLs
        // .NET framework assembly errors and VC Runtime DLL errors are typically transient (caused by Windows updates)
        // and should not generate NBug reports
        if (!IsDotNetFrameworkAssembly(fileName) && !IsVCRuntimeDll(fileName))
        {
            TaskDialogCommandLinkButton reportButton = new(text: TranslatedStrings.ReportIssue, descriptionText: TranslatedStrings.ReportReproducedIssueDescription);
            reportButton.Click += (_, _) => ShowNBug(OwnerForm, exception, isExternalOperation: false, isTerminating);
            page.Buttons.Add(reportButton);
        }

        page.Expander = new TaskDialogExpander
        {
            CollapsedButtonText = TranslatedStrings.SeeErrorMessage,
            ExpandedButtonText = TranslatedStrings.HideErrorMessage,
            Position = TaskDialogExpanderPosition.AfterFootnote,
            Text = exception.Message
        };

        TaskDialog.ShowDialog(OwnerFormHandle, page);

        return;

        static void RestartGE()
        {
            // Skipping the 1st parameter that, starting from .net core, contains the path to application dll (instead of exe)
            string arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1));
            ProcessStartInfo pi = new(Environment.ProcessPath, arguments);
            pi.WorkingDirectory = Environment.CurrentDirectory;
            Process.Start(pi);
            Environment.Exit(0);
        }

        // Determines if the assembly name is a .NET framework assembly.
        // .NET framework assemblies are typically affected by Patch Tuesday updates,
        // causing transient FileNotFoundException errors that are resolved by restarting the application.
        static bool IsDotNetFrameworkAssembly(string assemblyName)
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

    private static void ReportDubiousOwnership(ExternalOperationException exception)
    {
        if (_isReportingDubiousOwnershipSecurity)
        {
            return;
        }

        try
        {
            _isReportingDubiousOwnershipSecurity = true;
            ReportDubiousOwnershipImpl(exception);
        }
        finally
        {
            _isReportingDubiousOwnershipSecurity = false;
        }
    }

    private static void ReportDubiousOwnershipImpl(ExternalOperationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception.InnerException);
        string error = exception.InnerException.Message;
        TaskDialogPage pageSecurity = new()
        {
            Icon = TaskDialogIcon.Error,
            Caption = TranslatedStrings.GitSecurityError,
            Heading = TranslatedStrings.GitDubiousOwnershipHeader,
            Text = TranslatedStrings.GitDubiousOwnershipText,
            AllowCancel = true,
            SizeToContent = true,
        };
        int startIndex = error.IndexOf(DubiousOwnershipSecurityConfigString);
        string gitConfigTrustRepoCommand = ReplaceRepoPathQuotes(error[startIndex..].Trim());
        string folderPath = exception.WorkingDirectory ?? error[(startIndex + DubiousOwnershipSecurityConfigString.Length + 1)..];

        TaskDialogCommandLinkButton openExplorerButton = new(TranslatedStrings.GitDubiousOwnershipOpenRepositoryFolder, allowCloseDialog: false);
        openExplorerButton.Click += (_, _) => OsShellUtil.OpenWithFileExplorer(PathUtil.ToNativePath(folderPath));
        pageSecurity.Buttons.Add(openExplorerButton);

        AddTrustRepoButton(TranslatedStrings.GitDubiousOwnershipTrustRepository, gitConfigTrustRepoCommand, exception.WorkingDirectory ?? ".");

        TaskDialogButton helpButton = TaskDialogButton.Help;
        helpButton.Click += (_, _) =>
        {
            OsShellUtil.OpenUrlInDefaultBrowser("https://git-scm.com/docs/git-config/#Documentation/git-config.txt-safedirectory");
        };

        pageSecurity.Buttons.Add(helpButton);
        pageSecurity.Buttons.Add(TaskDialogButton.Close);

        pageSecurity.Expander = new TaskDialogExpander
        {
            CollapsedButtonText = TranslatedStrings.GitDubiousOwnershipSeeGitCommandOutput,
            ExpandedButtonText = TranslatedStrings.GitDubiousOwnershipHideGitCommandOutput,
            Position = TaskDialogExpanderPosition.AfterFootnote,
            Text = error,
        };

        TaskDialogButton button = TaskDialog.ShowDialog(OwnerFormHandle, pageSecurity);
        if (button == TaskDialogButton.Cancel || button == TaskDialogButton.Close)
        {
            ShowGitRepo(OwnerForm, workingDir: null);
        }

        return;

        void AddTrustRepoButton(string buttonText, string command, string workingDir)
        {
            TaskDialogCommandLinkButton button = new(buttonText)
            {
                DescriptionText = $"git {command}"
            };

            button.Click += (_, _) =>
            {
                new GitModule(workingDir).GitExecutable.Start(command).WaitForExit();
                ShowGitRepo(OwnerForm, workingDir);
            };

            pageSecurity.Buttons.Add(button);
        }

        static void ShowGitRepo(Form? ownerForm, string? workingDir)
        {
            if (ownerForm is FormBrowse formBrowse)
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                    {
                        await formBrowse.SwitchToMainThreadAsync();
                        formBrowse.SetWorkingDir(workingDir);
                    });
            }
        }

        // Turns single quotes to double quotes on Windows if there are any at all around the repo path.
        // in : git config --global -add safe.directory '%(prefix)///unc_machine/folder/to/repo'
        // out: git config --global -add safe.directory "%(prefix)///unc_machine/folder/to/repo"
        // as well as:
        // in : git config --global -add safe.directory 'd:/folder/to/repo with space in name'
        // out: git config --global -add safe.directory "d:/folder/to/repo with space in name"
        static string ReplaceRepoPathQuotes(string command)
        {
            if (!EnvUtils.RunningOnWindows() || !command.EndsWith('\''))
            {
                return command;
            }

            int quoteIndex = command.IndexOf('\'');
            return quoteIndex < 0 ? command : @$"{command[..quoteIndex]}""{command[(quoteIndex + 1)..^1]}""";
        }
    }

    private static void ShowNBug(IWin32Window? owner, Exception exception, bool isExternalOperation, bool isTerminating)
    {
        using BugReportForm form = new();
        DialogResult result = form.ShowDialog(owner,
            new SerializableException(exception),
            GetExceptionInfo(exception).ToString(),
            UserEnvironmentInformation.GetInformation(),
            canIgnore: !isTerminating,
            showIgnore: isExternalOperation,
            focusDetails: exception is UserExternalOperationException);
        if (isTerminating || result == DialogResult.Abort)
        {
            Environment.Exit(-1);
        }
    }

    private static string Base64Encode(string plainText)
    {
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
}
