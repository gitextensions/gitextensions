﻿using System;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils;
using Microsoft.WindowsAPICodePack.Dialogs;
namespace GitUI
{
    public static class OsShellUtil
    {
        /// <summary>
        /// Open a file with its associated default application.
        /// </summary>
        /// <param name="filePath">Pathname of the file to open.</param>
        public static void Open(string filePath)
        {
            try
            {
                new Executable(filePath).Start(useShellExecute: true);
            }
            catch (Exception)
            {
                OpenAs(filePath);
            }
        }

        /// <summary>
        /// Let the user chose an application to open a file.
        /// </summary>
        /// <param name="filePath">Pathname of the file to open.</param>
        public static void OpenAs(string filePath)
        {
            new Executable("rundll32.exe").Start("shell32.dll,OpenAs_RunDLL " + filePath, redirectOutput: true, outputEncoding: System.Text.Encoding.UTF8);
        }

        public static void SelectPathInFileExplorer(string filePath)
        {
            OpenWithFileExplorer("/select, " + filePath);
        }

        public static void OpenWithFileExplorer(string arguments)
        {
            new Executable("explorer.exe").Start(arguments);
        }

        /// <summary>
        /// opens urls even with anchor.
        /// </summary>
        public static void OpenUrlInDefaultBrowser(string? url)
        {
            if (!Strings.IsNullOrWhiteSpace(url))
            {
                new Executable(url).Start(useShellExecute: true);
            }
        }

        /// <summary>
        /// Prompts the user to select a directory.
        /// </summary>
        /// <param name="ownerWindow">The owner window.</param>
        /// <param name="selectedPath">The initially selected path.</param>
        /// <returns>The path selected by the user, or null if the user cancels the dialog.</returns>
        public static string? PickFolder(IWin32Window ownerWindow, string? selectedPath = null)
        {
            if (GitCommands.Utils.EnvUtils.IsWindowsVistaOrGreater())
            {
                // use Vista+ dialog
                using var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;

                if (selectedPath is not null)
                {
                    dialog.InitialDirectory = selectedPath;
                }

                var result = dialog.ShowDialog(ownerWindow.Handle);

                if (result == CommonFileDialogResult.Ok)
                {
                    return dialog.FileName;
                }
            }
            else
            {
                // use XP-era dialog
                using var dialog = new FolderBrowserDialog();
                if (selectedPath is not null)
                {
                    dialog.SelectedPath = selectedPath;
                }

                var result = dialog.ShowDialog(ownerWindow);

                if (result == DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }
            }

            // return null if the user cancelled
            return null;
        }
    }
}
