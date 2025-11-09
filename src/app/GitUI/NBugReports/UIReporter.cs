using System.Diagnostics;
using System.Text;
using BugReporter;
using BugReporter.Serialization;
using GitCommands;
using GitCommands.Utils;
using GitExtensions.Extensibility;
using GitUI.CommandsDialogs;

namespace GitUI.NBugReports;

internal interface IBugReporter
{
    void ReportDubiousOwnership(ExternalOperationException exception);
    void ReportError(Exception exception, string rootError, StringBuilder text, OperationInfo operationInfo);
    void ReportFailedToLoadAnAssembly(Exception exception, bool isTerminating, bool isRuntimeAssembly);
}

internal class UIReporter : IBugReporter
{
    private static Form? OwnerForm
        => Form.ActiveForm ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null);

    private static IntPtr OwnerFormHandle
        => OwnerForm?.Handle ?? IntPtr.Zero;

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

    public void ReportDubiousOwnership(ExternalOperationException exception)
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
    }

    public void ReportError(Exception exception, string rootError, StringBuilder text, OperationInfo operationInfo)
    {
        TaskDialogPage page = new()
        {
            Icon = operationInfo.Icon,
            Caption = TranslatedStrings.Error,
            Heading = rootError,
            AllowCancel = true,
            SizeToContent = true
        };

        bool isUserExternalOrGitOperation = operationInfo.IsUserExternalOperation || operationInfo.IsGitOperation;

        // prefer to ignore failed external operations
        if (isUserExternalOrGitOperation)
        {
            AddIgnoreButton(TranslatedStrings.ExternalErrorDescription);
        }
        else
        {
            // directions and button to raise a bug
            text.AppendLine().AppendLine(TranslatedStrings.ReportBug);
        }

        // no bug reports for user configured operations
        TaskDialogCommandLinkButton taskDialogCommandLink
            = operationInfo.IsUserExternalOperation ? new(TranslatedStrings.ButtonViewDetails)
                : operationInfo.IsExternalOperation ? new(TranslatedStrings.ReportIssue, TranslatedStrings.ReportIssueDescription)
                : new(TranslatedStrings.ButtonReportBug);
        taskDialogCommandLink.Click += (s, e) =>
        {
            ShowNBug(OwnerForm, exception, operationInfo.IsExternalOperation, operationInfo.IsTerminating);
        };
        page.Buttons.Add(taskDialogCommandLink);

        // let the user decide whether to report the bug
        if (!operationInfo.IsExternalOperation && !isUserExternalOrGitOperation)
        {
            AddIgnoreButton();
        }

        page.Text = text.ToString().Trim();
        TaskDialog.ShowDialog(OwnerFormHandle, page);
        return;

        void AddIgnoreButton(string descriptionText = null)
        {
            TaskDialogCommandLinkButton taskDialogCommandLink = new(TranslatedStrings.ButtonIgnore, descriptionText);
            page.Buttons.Add(taskDialogCommandLink);
        }
    }

    public void ReportFailedToLoadAnAssembly(Exception exception, bool isTerminating, bool isRuntimeAssembly)
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
        restartButton.Click += (_, _) => RestartGE();
        page.Buttons.Add(restartButton);

        // Only show the report button for non-.NET framework assemblies and non-VC Runtime DLLs
        // .NET framework assembly errors and VC Runtime DLL errors are typically transient (caused by Windows updates)
        // and should not generate NBug reports
        if (!isRuntimeAssembly)
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
    }

    private static void ShowNBug(IWin32Window? owner, Exception exception, bool isExternalOperation, bool isTerminating)
    {
        using BugReportForm form = new();
        DialogResult result = form.ShowDialog(owner,
            new SerializableException(exception),
            exception.GetExceptionInfo().ToString(),
            UserEnvironmentInformation.GetInformation(),
            canIgnore: !isTerminating,
            showIgnore: isExternalOperation,
            focusDetails: exception is UserExternalOperationException);
        if (isTerminating || result == DialogResult.Abort)
        {
            Environment.Exit(-1);
        }
    }

    internal readonly struct TestAccessor
    {
        internal static string GetFileName(Exception exception) => UIReporter.GetFileName(exception);
    }
}
