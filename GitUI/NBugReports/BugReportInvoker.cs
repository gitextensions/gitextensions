﻿#nullable enable

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;
using System.Windows.Forms;
using BugReporter;
using BugReporter.Serialization;
using GitCommands;
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
                AppendIfNotEmpty(externalOperationException.Command, GitUI.Strings.Command);

                // Arguments: <args>
                AppendIfNotEmpty(externalOperationException.Arguments, GitUI.Strings.Arguments);

                // Directory: <dir>
                AppendIfNotEmpty(externalOperationException.WorkingDirectory, GitUI.Strings.WorkingDirectory);
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

        private static void LogError(Exception exception, bool isTerminating)
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
            bool isExternalOperation = exception is ExternalOperationException
                                                 or IOException
                                                 or SecurityException
                                                 or FileNotFoundException
                                                 or DirectoryNotFoundException
                                                 or PathTooLongException
                                                 or Win32Exception;

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
                ShowNBug(OwnerForm, exception, isExternalOperation, isTerminating);
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

        private static void ShowNBug(IWin32Window? owner, Exception exception, bool isExternalOperation, bool isTerminating)
        {
            using BugReportForm form = new();
            DialogResult result = form.ShowDialog(owner, new SerializableException(exception),
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
