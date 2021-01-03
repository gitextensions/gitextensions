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

        private static string FormatText(ExternalOperationException ex, bool canRaiseBug)
        {
            StringBuilder sb = new StringBuilder();

            // Command: <command>
            if (!string.IsNullOrWhiteSpace(ex.Command))
            {
                sb.AppendLine($"{Strings.Command}: {ex.Command}");
            }

            // Arguments: <args>
            if (!string.IsNullOrWhiteSpace(ex.Arguments))
            {
                sb.AppendLine($"{Strings.Arguments}: {ex.Arguments}");
            }

            // Working directory: <working dir>
            sb.AppendLine($"{Strings.WorkingDirectory}: {ex.WorkingDirectory}");

            if (canRaiseBug)
            {
                // Directions to raise a bug
                sb.AppendLine();
                sb.AppendLine(Strings.ReportBug);
            }

            return sb.ToString();
        }

        public static void Report(Exception ex, bool isTerminating)
        {
            if (ex is UserExternalOperationException userExternalException)
            {
                // Something happened that was likely caused by the user
                ReportUserException(userExternalException, isTerminating);
                return;
            }

            if (ex is ExternalOperationException externalException)
            {
                // Something happened either in the app - can submit the bug to GitHub
                ReportAppException(externalException, isTerminating);
                return;
            }

            // Report any other exceptions
            ReportGenericException(ex, isTerminating);
        }

        private static void ReportAppException(ExternalOperationException ex, bool isTerminating)
        {
            // UserExternalOperationException wraps an actual exception, but be cautious just in case
            string instructionText = ex.InnerException?.Message ?? Strings.InstructionOperationFailed;

            ReportException(FormatText(ex, canRaiseBug: true), instructionText, ex, isTerminating);
        }

        private static void ReportException(string text, string instructionText, Exception ex, bool isTerminating)
        {
            using var dialog = new TaskDialog
            {
                OwnerWindowHandle = OwnerFormHandle,
                Text = text,
                InstructionText = instructionText,
                Caption = Strings.Error,
                Icon = TaskDialogStandardIcon.Error,
                Cancelable = true,
            };
            var btnReport = new TaskDialogCommandLink("Report", Strings.ButtonReportBug);
            btnReport.Click += (s, e) =>
            {
                dialog.Close();
                ShowNBug(OwnerForm, ex, isTerminating);
            };
            var btnIgnoreOrClose = new TaskDialogCommandLink("IgnoreOrClose", isTerminating ? Strings.ButtonCloseApp : Strings.ButtonIgnore);
            btnIgnoreOrClose.Click += (s, e) =>
            {
                dialog.Close();
            };
            dialog.Controls.Add(btnReport);
            dialog.Controls.Add(btnIgnoreOrClose);

            dialog.Show();
        }

        private static void ReportGenericException(Exception ex, bool isTerminating)
        {
            // This exception is arbitrary, see if there's additional information
            string? moreInfo = ex.InnerException?.Message;
            if (moreInfo != null)
            {
                moreInfo += Environment.NewLine + Environment.NewLine;
            }

            ReportException($"{moreInfo}{Strings.ReportBug}", ex.Message, ex, isTerminating);
        }

        private static void ReportUserException(UserExternalOperationException ex, bool isTerminating)
        {
            using var dialog = new TaskDialog
            {
                OwnerWindowHandle = OwnerFormHandle,
                Text = FormatText(ex, canRaiseBug: false),
                InstructionText = ex.Context,
                Caption = Strings.CaptionFailedExecute,
                Icon = TaskDialogStandardIcon.Error,
                Cancelable = true,
            };
            var btnIgnoreOrClose = new TaskDialogCommandLink("IgnoreOrClose", isTerminating ? Strings.ButtonCloseApp : Strings.ButtonIgnore);
            btnIgnoreOrClose.Click += (s, e) =>
            {
                dialog.Close();
            };
            dialog.Controls.Add(btnIgnoreOrClose);

            dialog.Show();
        }

        private static void ShowNBug(IWin32Window? owner, Exception ex, bool isTerminating)
        {
            var envInfo = UserEnvironmentInformation.GetInformation();

            using (var form = new GitUI.NBugReports.BugReportForm())
            {
                var result = form.ShowDialog(owner, ex, envInfo);
                if (isTerminating || result == DialogResult.Abort)
                {
                    Environment.Exit(-1);
                }
            }
        }
    }
}
