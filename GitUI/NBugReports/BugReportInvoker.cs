using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Text;
using BugReporter;
using BugReporter.Serialization;
using GitCommands;
using GitExtUtils;
using GitUI.CommandsDialogs;

namespace GitUI.NBugReports
{
    public static class BugReportInvoker
    {
        private const string _dubiousOwnershipSecurityConfigString = "config --global --add safe.directory";

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
                AppendIfNotEmpty(externalOperationException.ExitCode.ToString(), TranslatedStrings.ExitCode);

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

            ExternalOperationException externalOperationException = exception as ExternalOperationException;

            if (externalOperationException?.InnerException?.Message?.Contains(_dubiousOwnershipSecurityConfigString) is true)
            {
                ReportDubiousOwnership(externalOperationException.InnerException);
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

            bool isUserExternalOperation = exception is UserExternalOperationException;
            bool isExternalOperation = exception is ExternalOperationException
                                                 or IOException
                                                 or SecurityException
                                                 or FileNotFoundException
                                                 or DirectoryNotFoundException
                                                 or PathTooLongException
                                                 or Win32Exception;

            // Treat all git errors as user issues
            if (string.Equals(AppSettings.GitCommand, externalOperationException?.Command, StringComparison.InvariantCultureIgnoreCase)
             || string.Equals(AppSettings.WslGitCommand, externalOperationException?.Command, StringComparison.InvariantCultureIgnoreCase))
            {
                isUserExternalOperation = true;
            }

            StringBuilder text = GetExceptionInfo(exception);
            string rootError = GetRootError(exception);

            TaskDialogPage page = new()
            {
                Icon = TaskDialogIcon.Error,
                Caption = TranslatedStrings.Error,
                Heading = rootError,
                AllowCancel = true,
                SizeToContent = true
            };

            // prefer to ignore failed external operations
            if (isExternalOperation)
            {
                AddIgnoreOrCloseButton();
            }

            // no bug reports for user configured operations
            if (!isUserExternalOperation)
            {
                // directions and button to raise a bug
                text.AppendLine().AppendLine(TranslatedStrings.ReportBug);
            }

            string buttonText = isUserExternalOperation ? TranslatedStrings.ButtonViewDetails : TranslatedStrings.ButtonReportBug;
            TaskDialogCommandLinkButton taskDialogCommandLink = new(buttonText);
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

            void AddIgnoreOrCloseButton()
            {
                string buttonText = isTerminating ? TranslatedStrings.ButtonCloseApp : TranslatedStrings.ButtonIgnore;
                TaskDialogCommandLinkButton taskDialogCommandLink = new(buttonText);
                page.Buttons.Add(taskDialogCommandLink);
            }
        }

        private static void ReportDubiousOwnership(Exception exception)
        {
            string error = exception.Message;
            TaskDialogPage pageSecurity = new()
            {
                Icon = TaskDialogIcon.Error,
                Caption = TranslatedStrings.GitSecurityError,
                Heading = TranslatedStrings.GitDubiousOwnershipHeader,
                Text = TranslatedStrings.GitDubiousOwnershipText,
                AllowCancel = true,
                SizeToContent = true,
            };
            int startIndex = error.IndexOf(_dubiousOwnershipSecurityConfigString);
            string gitConfigTrustRepoCommand = error[startIndex..];
            string folderPath = error[(startIndex + _dubiousOwnershipSecurityConfigString.Length + 1)..];

            TaskDialogCommandLinkButton openExplorerButton = new(TranslatedStrings.GitDubiousOwnershipOpenRepositoryFolder, allowCloseDialog: false);
            openExplorerButton.Click += (_, _) => OsShellUtil.OpenWithFileExplorer(PathUtil.ToNativePath(folderPath));
            pageSecurity.Buttons.Add(openExplorerButton);

            AddTrustRepoButton(TranslatedStrings.GitDubiousOwnershipTrustRepository, gitConfigTrustRepoCommand);
            AddTrustAllReposButton(TranslatedStrings.GitDubiousOwnershipTrustAllRepositories);

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

            void AddTrustRepoButton(string buttonText, string command)
            {
                TaskDialogCommandLinkButton button = new(buttonText)
                {
                    DescriptionText = $"git {command}"
                };

                button.Click += (_, _) =>
                {
                    new GitModule(".").GitExecutable.Start(command);
                    ShowGitRepo(OwnerForm, folderPath);
                };

                pageSecurity.Buttons.Add(button);
            }

            void AddTrustAllReposButton(string buttonText)
            {
                TaskDialogCommandLinkButton button = new(buttonText, allowCloseDialog: false)
                {
                    DescriptionText = $"git {_dubiousOwnershipSecurityConfigString} *",
                };

                button.Click += (_, _) =>
                {
                    string tempFile = Path.GetTempFileName();
                    File.WriteAllText(tempFile, $"{TranslatedStrings.GitDubiousOwnershipTrustAllInstruction}\r\n\r\ngit {_dubiousOwnershipSecurityConfigString} \"*\"");

                    using FormEditor formEditor = new(new GitUICommands(new GitModule(null)), tempFile, showWarning: false, readOnly: true);
                    formEditor.ShowDialog();

                    try
                    {
                        File.Delete(tempFile);
                    }
                    catch
                    {
                        // no-op
                    }
                };

                pageSecurity.Buttons.Add(button);
            }

            static void ShowGitRepo(Form? ownerForm, string? workingDir)
            {
                if (ownerForm is FormBrowse formBrowse)
                {
                    formBrowse.Invoke(() => formBrowse.SetWorkingDir(workingDir));
                }
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
}
