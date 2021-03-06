#nullable enable

using System;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI;
using GitUI.NBugReports;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace GitExtensions
{
    public static class BugReporter
    {
        private static Form? OwnerForm
            => Form.ActiveForm ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null);

        private static IntPtr OwnerFormHandle
            => OwnerForm?.Handle ?? IntPtr.Zero;

        /// <summary>
        /// Appends the exception data and gets the root error.
        /// </summary>
        /// <param name="text">A StringBuilder to which the exception data is appended.</param>
        /// <param name="exception">An Exception to describe.</param>
        /// <returns>The inner-most exception message.</returns>
        internal static string Append(StringBuilder text, Exception exception)
        {
            string rootError = exception.Message;
            for (Exception innerException = exception.InnerException; innerException is not null; innerException = innerException.InnerException)
            {
                if (!string.IsNullOrEmpty(innerException.Message))
                {
                    rootError = innerException.Message;
                }
            }

            if (exception is UserExternalOperationException userExternalOperationException && !string.IsNullOrWhiteSpace(userExternalOperationException.Context))
            {
                // Context contains an error message as UserExternalOperationException is currently used. So append just "<context>"
                text.AppendLine(userExternalOperationException.Context);
            }

            if (exception is ExternalOperationException externalOperationException)
            {
                // Command: <command>
                AppendUnlessEmpty(externalOperationException.Command, GitUI.Strings.Command);

                // Arguments: <args>
                AppendUnlessEmpty(externalOperationException.Arguments, GitUI.Strings.Arguments);

                // Directory: <dir>
                AppendUnlessEmpty(externalOperationException.WorkingDirectory, GitUI.Strings.WorkingDirectory);
            }

            return rootError;

            void AppendUnlessEmpty(string? value, string designation)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    text.Append(designation).Append(": ").AppendLine(value);
                }
            }
        }

        public static void Report(Exception exception, bool isTerminating)
        {
            bool isUserExternalOperation = exception is UserExternalOperationException;
            bool isExternalOperation = exception is ExternalOperationException;

            StringBuilder text = new();
            string rootError = Append(text, exception);

            using var taskDialog = new TaskDialog
            {
                OwnerWindowHandle = OwnerFormHandle,
                Icon = TaskDialogStandardIcon.Error,
                Caption = GitUI.Strings.Error,
                InstructionText = rootError,
                Cancelable = true,
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
                text.AppendLine().AppendLine(GitUI.Strings.ReportBug);
            }

            string buttonText = isUserExternalOperation ? GitUI.Strings.ButtonViewDetails : GitUI.Strings.ButtonReportBug;
            TaskDialogCommandLink taskDialogCommandLink = new(buttonText, buttonText);
            taskDialogCommandLink.Click += (s, e) =>
                {
                    taskDialog.Close();
                    ShowNBug(OwnerForm, exception, isTerminating);
                };
            taskDialog.Controls.Add(taskDialogCommandLink);

            // let the user decide whether to report the bug
            if (!isExternalOperation)
            {
                AddIgnoreOrCloseButton();
            }

            taskDialog.Text = text.ToString().Trim();
            taskDialog.Show();
            return;

            void AddIgnoreOrCloseButton()
            {
                string buttonText = isTerminating ? GitUI.Strings.ButtonCloseApp : GitUI.Strings.ButtonIgnore;
                TaskDialogCommandLink taskDialogCommandLink = new(buttonText, buttonText);
                taskDialogCommandLink.Click += (s, e) => taskDialog.Close();
                taskDialog.Controls.Add(taskDialogCommandLink);
            }
        }

        private static void ShowNBug(IWin32Window? owner, Exception exception, bool isTerminating)
        {
            var envInfo = UserEnvironmentInformation.GetInformation();

            using var form = new GitUI.NBugReports.BugReportForm();
            var result = form.ShowDialog(owner, exception, envInfo);
            if (isTerminating || result == DialogResult.Abort)
            {
                Environment.Exit(-1);
            }
        }
    }
}
