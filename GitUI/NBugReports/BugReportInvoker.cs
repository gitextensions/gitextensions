using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using BugReporter;
using BugReporter.Serialization;
using GitExtUtils;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace GitUI.NBugReports
{
    public static class BugReportInvoker
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
                AppendIfNotEmpty(externalOperationException.Command, TranslatedStrings.Command);

                // Arguments: <args>
                AppendIfNotEmpty(externalOperationException.Arguments, TranslatedStrings.Arguments);

                // Directory: <dir>
                AppendIfNotEmpty(externalOperationException.WorkingDirectory, TranslatedStrings.WorkingDirectory);
            }

            return rootError;

            void AppendIfNotEmpty(string? value, string designation)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    text.Append(designation).Append(": ").AppendLine(value);
                }
            }
        }

        public static void Report(Exception exception, bool isTerminating)
        {
            if (isTerminating)
            {
                // TODO: this is not very efficient
                SerializableException serializableException = new(exception);
                string xml = serializableException.ToXmlString();
                string encoded = Base64Encode(xml);
                Process.Start("BugReporter.exe", encoded);

                return;
            }

            bool isUserExternalOperation = exception is UserExternalOperationException;
            bool isExternalOperation = exception is ExternalOperationException;

            StringBuilder text = new();
            string rootError = Append(text, exception);

            using var taskDialog = new Microsoft.WindowsAPICodePack.Dialogs.TaskDialog
            {
                OwnerWindowHandle = OwnerFormHandle,
                Icon = TaskDialogStandardIcon.Error,
                Caption = TranslatedStrings.Error,
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
                text.AppendLine().AppendLine(TranslatedStrings.ReportBug);
            }

            string buttonText = isUserExternalOperation ? TranslatedStrings.ButtonViewDetails : TranslatedStrings.ButtonReportBug;
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
                string buttonText = isTerminating ? TranslatedStrings.ButtonCloseApp : TranslatedStrings.ButtonIgnore;
                TaskDialogCommandLink taskDialogCommandLink = new(buttonText, buttonText);
                taskDialogCommandLink.Click += (s, e) => taskDialog.Close();
                taskDialog.Controls.Add(taskDialogCommandLink);
            }
        }

        private static void ShowNBug(IWin32Window? owner, Exception exception, bool isTerminating)
        {
            using BugReportForm form = new();
            DialogResult result = form.ShowDialog(owner, new SerializableException(exception),
                UserEnvironmentInformation.GetInformation(),
                canIgnore: !isTerminating,
                showIgnore: exception is ExternalOperationException,
                focusDetails: exception is UserExternalOperationException);
            if (isTerminating || result == DialogResult.Abort)
            {
                Environment.Exit(-1);
            }
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
