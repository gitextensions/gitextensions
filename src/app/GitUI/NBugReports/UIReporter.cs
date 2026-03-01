using System.Diagnostics;
using System.Text;
using BugReporter;
using BugReporter.Serialization;
using GitCommands;
using GitCommands.Utils;
using GitExtensions.Extensibility;
using GitUI.CommandsDialogs;

namespace GitUI.NBugReports;

/// <summary>
///  Provides methods for reporting various types of errors to the user.
/// </summary>
internal interface IBugReporter
{
    /// <summary>
    ///  Reports a dubious ownership security error detected by git.
    /// </summary>
    /// <param name="exception">The external operation exception containing the dubious ownership error.</param>
    void ReportDubiousOwnership(ExternalOperationException exception);

    /// <summary>
    ///  Reports an error to the user via a task dialog.
    /// </summary>
    /// <param name="exception">The exception to report.</param>
    /// <param name="rootError">The inner-most exception message.</param>
    /// <param name="text">Additional exception information text.</param>
    /// <param name="operationInfo">Metadata about the operation that caused the exception.</param>
    void ReportError(Exception exception, string rootError, StringBuilder text, OperationInfo operationInfo);

    /// <summary>
    ///  Reports a failed assembly or DLL load error, typically caused by Windows updates.
    /// </summary>
    /// <param name="exception">The exception to evaluate and potentially report.</param>
    /// <param name="isTerminating">Indicates whether the exception is terminating the application.</param>
    /// <returns><see langword="true" /> if the exception was handled; otherwise, <see langword="false" />.</returns>
    /// <remarks>
    ///  Handles errors loading .NET assemblies or VC Runtime DLL (refer to https://github.com/gitextensions/gitextensions/issues/12511).
    ///  These are transient errors typically caused by Windows updates.
    /// </remarks>
    bool ReportFailedToLoadAnAssembly(Exception exception, bool isTerminating);
}

/// <summary>
///  Implements <see cref="IBugReporter" /> by displaying task dialogs
///  and optionally launching the NBug report form.
/// </summary>
internal class UIReporter : IBugReporter
{
    /// <summary>
    ///  Set to <see langword="true" /> on application exit
    ///  in order to suppress the popup to restart the app on missing runtime assembly.
    /// </summary>
    public static bool IgnoreFailedToLoadAnAssembly { get; set; } = false;

    private static Form? OwnerForm
        => Form.ActiveForm ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null);

    private static IntPtr OwnerFormHandle
        => OwnerForm?.Handle ?? IntPtr.Zero;

    private static TaskDialogPage CreateDubiousOwnershipReport(ExternalOperationException exception)
    {
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
        int startIndex = error.IndexOf(BugReportInvoker.DubiousOwnershipSecurityConfigString);
        string gitConfigTrustRepoCommand = ReplaceRepoPathQuotes(error[startIndex..].Trim());
        string folderPath = exception.WorkingDirectory ?? error[(startIndex + BugReportInvoker.DubiousOwnershipSecurityConfigString.Length + 1)..];

        TaskDialogCommandLinkButton openExplorerButton = new(TranslatedStrings.GitDubiousOwnershipOpenRepositoryFolder, allowCloseDialog: false);
        openExplorerButton.Click += (_, _) => OsShellUtil.OpenWithFileExplorer(PathUtil.ToNativePath(folderPath));
        pageSecurity.Buttons.Add(openExplorerButton);

        AddTrustRepoButton(TranslatedStrings.GitDubiousOwnershipTrustRepository, gitConfigTrustRepoCommand, exception.WorkingDirectory ?? ".");

        TaskDialogButton helpButton = TaskDialogButton.Help;
        helpButton.Click += (_, _) =>
        {
#pragma warning disable S1075 // Stable documentation URL
            OsShellUtil.OpenUrlInDefaultBrowser("https://git-scm.com/docs/git-config/#Documentation/git-config.txt-safedirectory");
#pragma warning restore S1075
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

        return pageSecurity;

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

    private static TaskDialogPage CreateErrorReport(Exception exception, string rootError, StringBuilder text, OperationInfo operationInfo)
    {
        TaskDialogPage page = new()
        {
            Icon = operationInfo.Icon,
            Caption = TranslatedStrings.Error,
            Heading = rootError,
            AllowCancel = true,
            SizeToContent = true
        };

        // prefer to ignore failed external operations
        if (operationInfo.IsExternalOperation)
        {
            AddIgnoreButton(TranslatedStrings.ExternalErrorDescription);
        }
        else
        {
            // directions and button to raise a bug
            text.AppendLine().AppendLine(TranslatedStrings.ReportBug);
        }

        // no bug reports for user configured operations
        TaskDialogCommandLinkButton taskDialogCommandLink = operationInfo switch
        {
            { IsUserExternalOperation: true } => new(TranslatedStrings.ButtonViewDetails),
            { IsExternalOperation: true } => new(TranslatedStrings.ReportIssue, TranslatedStrings.ReportIssueDescription),
            _ => new(TranslatedStrings.ButtonReportBug),
        };
        taskDialogCommandLink.Click += (s, e) =>
        {
            ShowNBug(OwnerForm, exception, operationInfo.IsExternalOperation, operationInfo.IsUserExternalOperation, isTerminating: false);
        };
        page.Buttons.Add(taskDialogCommandLink);

        // let the user decide whether to report the bug
        if (!operationInfo.IsExternalOperation)
        {
            AddIgnoreButton();
        }

        page.Text = text.ToString().Trim();

        return page;

        void AddIgnoreButton(string? descriptionText = null)
        {
            string buttonText = TranslatedStrings.ButtonIgnore;
            TaskDialogCommandLinkButton taskDialogCommandLink = new(buttonText, descriptionText);
            page.Buttons.Add(taskDialogCommandLink);
        }
    }

    private static TaskDialogPage CreateFailedToLoadAnAssemblyReport(Exception exception, bool isTerminating)
    {
        string fileName = GetFileName(exception);

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
        restartButton.Click += (_, _) => InvokeRestartGE();
        page.Buttons.Add(restartButton);

        // Only show the report button for non-.NET framework assemblies and non-VC Runtime DLLs
        // .NET framework assembly errors and VC Runtime DLL errors are typically transient (caused by Windows updates)
        // and should not generate NBug reports
        if (!IsDotNetFrameworkAssembly(fileName) && !IsVCRuntimeDll(fileName))
        {
            TaskDialogCommandLinkButton reportButton = new(text: TranslatedStrings.ReportIssue, descriptionText: TranslatedStrings.ReportReproducedIssueDescription);
            reportButton.Click += (_, _) => ShowNBug(OwnerForm, exception, isExternalOperation: false, isUserExternalOperation: false, isTerminating);
            page.Buttons.Add(reportButton);
        }

        page.Expander = new TaskDialogExpander
        {
            CollapsedButtonText = TranslatedStrings.SeeErrorMessage,
            ExpandedButtonText = TranslatedStrings.HideErrorMessage,
            Position = TaskDialogExpanderPosition.AfterFootnote,
            Text = exception.Message
        };

        return page;

        static void InvokeRestartGE()
        {
            // Use Invoke to queue the restart on the message loop to avoid deadlocks
            // when the new process calls Application.SetColorMode() which broadcasts system events
            if (OwnerForm is Control control)
            {
                control.InvokeAndForget(RestartGE);
            }
            else
            {
                ThreadHelper.FileAndForget(RestartGE);
            }

            static void RestartGE()
            {
                // Skipping the 1st parameter that, starting from .NET, contains the path to application dll (instead of exe)
                string arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1));
                ProcessStartInfo pi = new(Environment.ProcessPath!, arguments)
                {
                    WorkingDirectory = Environment.CurrentDirectory
                };
                Process.Start(pi);
                Environment.Exit(0);
            }
        }
    }

    private static string GetFileName(Exception exception)
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

        return fileName;
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

    /// <inheritdoc />
    public void ReportDubiousOwnership(ExternalOperationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception.InnerException);

        TaskDialogButton button = TaskDialog.ShowDialog(OwnerFormHandle, CreateDubiousOwnershipReport(exception));
        if (button == TaskDialogButton.Cancel || button == TaskDialogButton.Close)
        {
            ShowGitRepo(OwnerForm, workingDir: null);
        }
    }

    /// <inheritdoc />
    public void ReportError(Exception exception, string rootError, StringBuilder text, OperationInfo operationInfo)
    {
        TaskDialog.ShowDialog(OwnerFormHandle, CreateErrorReport(exception, rootError, text, operationInfo));
    }

    /// <inheritdoc />
    public bool ReportFailedToLoadAnAssembly(Exception exception, bool isTerminating)
    {
        if (!HasFailedToLoadAnAssembly(exception))
        {
            return false;
        }

        if (IgnoreFailedToLoadAnAssembly)
        {
            return true;
        }

        IgnoreFailedToLoadAnAssembly = true;
        TaskDialog.ShowDialog(OwnerFormHandle, CreateFailedToLoadAnAssemblyReport(exception, isTerminating));

        return true;

        static bool HasFailedToLoadAnAssembly(Exception exception)
           => (exception is FileNotFoundException fileNotFoundException && fileNotFoundException.Message.StartsWith("Could not load file or assembly"))
           || (exception is DllNotFoundException dllNotFoundException && IsVCRuntimeDll(dllNotFoundException.Message))
           || (exception.InnerException is not null && HasFailedToLoadAnAssembly(exception.InnerException));
    }

    private static void ShowGitRepo(Form? ownerForm, string? workingDir)
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

    private static void ShowNBug(IWin32Window? owner, Exception exception, bool isExternalOperation, bool isUserExternalOperation, bool isTerminating)
    {
        using BugReportForm form = new();
        DialogResult result = form.ShowDialog(owner,
                                              new SerializableException(exception),
                                              exception.GetExceptionInfo().ToString(),
                                              UserEnvironmentInformation.GetInformation(),
                                              canIgnore: !isTerminating,
                                              showIgnore: isExternalOperation,
                                              focusDetails: isUserExternalOperation);
        if (isTerminating || result == DialogResult.Abort)
        {
            Environment.Exit(-1);
        }
    }

    internal readonly struct TestAccessor
    {
        internal static TaskDialogPage CreateDubiousOwnershipReport(ExternalOperationException exception) => UIReporter.CreateDubiousOwnershipReport(exception);
        internal static TaskDialogPage CreateErrorReport(Exception exception, string rootError, StringBuilder text, OperationInfo operationInfo) => UIReporter.CreateErrorReport(exception, rootError, text, operationInfo);
        internal static TaskDialogPage CreateFailedToLoadAnAssemblyReport(Exception exception, bool isTerminating) => UIReporter.CreateFailedToLoadAnAssemblyReport(exception, isTerminating);
        internal static string GetFileName(Exception exception) => UIReporter.GetFileName(exception);
        internal static bool IsDotNetFrameworkAssembly(string assemblyName) => UIReporter.IsDotNetFrameworkAssembly(assemblyName);
        internal static bool IsVCRuntimeDll(string dllName) => UIReporter.IsVCRuntimeDll(dllName);
    }
}
