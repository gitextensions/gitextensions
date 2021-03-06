using System;
using System.Text;
using System.Windows.Forms;
using GitExtUtils;
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

        private static string FormatText(ExternalOperationException exception, bool canRaiseBug)
        {
            StringBuilder sb = new();

            // Command: <command>
            if (!string.IsNullOrWhiteSpace(exception.Object))
            {
                sb.AppendLine($"{GitUI.Strings.Command}: {exception.Object}");
            }

            // Arguments: <args>
            if (!string.IsNullOrWhiteSpace(exception.Arguments))
            {
                sb.AppendLine($"{GitUI.Strings.Arguments}: {exception.Arguments}");
            }

            // Working directory: <working dir>
            sb.AppendLine($"{GitUI.Strings.WorkingDirectory}: {exception.Directory}");

            if (canRaiseBug)
            {
                // Directions to raise a bug
                sb.AppendLine();
                sb.AppendLine(GitUI.Strings.ReportBug);
            }

            return sb.ToString();
        }

        public static void Report(Exception exception, bool isTerminating)
        {
            if (exception is UserExternalOperationException userExternalException)
            {
                // Something happened that was likely caused by the user
                ReportUserException(userExternalException, isTerminating);
                return;
            }

            if (exception is ExternalOperationException externalException)
            {
                // Something happened either in the app - can submit the bug to GitHub
                ReportAppException(externalException, isTerminating);
                return;
            }

            // Report any other exceptions
            ReportGenericException(exception, isTerminating);
        }

        private static void ReportAppException(ExternalOperationException exception, bool isTerminating)
        {
            // UserExternalOperationException wraps an actual exception, but be cautious just in case
            string instructionText = exception.InnerException?.Message ?? GitUI.Strings.InstructionOperationFailed;

            ShowException(FormatText(exception, canRaiseBug: true), instructionText, exception, isTerminating);
        }

        private static void ReportGenericException(Exception exception, bool isTerminating)
        {
            // This exception is arbitrary, see if there's additional information
            string? moreInfo = exception.InnerException?.Message;
            if (moreInfo is not null)
            {
                moreInfo += Environment.NewLine + Environment.NewLine;
            }

            ShowException($"{moreInfo}{GitUI.Strings.ReportBug}", exception.Message, exception, isTerminating);
        }

        private static void ReportUserException(UserExternalOperationException exception, bool isTerminating)
            => ShowException(FormatText(exception, canRaiseBug: false), exception.Context ?? string.Empty, exception: null, isTerminating);

        private static void ShowException(string text, string instructionText, Exception? exception, bool isTerminating)
        {
            using var dialog = new TaskDialog
            {
                OwnerWindowHandle = OwnerFormHandle,
                Text = text,
                InstructionText = instructionText,
                Caption = GitUI.Strings.Error,
                Icon = TaskDialogStandardIcon.Error,
                Cancelable = true,
            };

            if (exception is not null)
            {
                var btnReport = new TaskDialogCommandLink("Report", GitUI.Strings.ButtonReportBug);
                btnReport.Click += (s, e) =>
                {
                    dialog.Close();
                    ShowNBug(OwnerForm, exception, isTerminating);
                };

                dialog.Controls.Add(btnReport);
            }

            var btnIgnoreOrClose = new TaskDialogCommandLink("IgnoreOrClose", isTerminating ? GitUI.Strings.ButtonCloseApp : GitUI.Strings.ButtonIgnore);
            btnIgnoreOrClose.Click += (s, e) =>
            {
                dialog.Close();
            };
            dialog.Controls.Add(btnIgnoreOrClose);

            dialog.Show();
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
