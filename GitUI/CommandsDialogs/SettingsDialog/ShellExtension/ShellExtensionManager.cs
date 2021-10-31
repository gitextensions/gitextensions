using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using GitCommands;
using Microsoft.Win32;

namespace GitUI.CommandsDialogs.SettingsDialog.ShellExtension
{
    public static class ShellExtensionManager
    {
        /// <summary>
        ///  Checks if the shell extension files are found at the known location.
        /// </summary>
        /// <returns><see langword="true"/> the extension files are found at the known location; otherwise <see langword="false"/>.</returns>
        public static bool CheckFilesFound()
        {
            string path32 = FindFileInBinFolders(CommonLogic.GitExtensionsShellEx32Name);
            string path64 = FindFileInBinFolders(CommonLogic.GitExtensionsShellEx64Name);
            return !(string.IsNullOrEmpty(path32) || (IntPtr.Size == 8 && string.IsNullOrEmpty(path64)));
        }

        /// <summary>
        ///  Checks if the shell extension is properly registered in the system.
        /// </summary>
        /// <returns><see langword="true"/> if the extension is registered; otherwise <see langword="false"/>.</returns>
        public static bool CheckIfRegistered()
        {
            return !string.IsNullOrEmpty(CommonLogic.GetRegistryValue(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved", "{3C16B20A-BA16-4156-916F-0A375ECFFE24}"))
                   && !string.IsNullOrEmpty(CommonLogic.GetRegistryValue(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\GitExtensions2"))
                   && !string.IsNullOrEmpty(CommonLogic.GetRegistryValue(Registry.ClassesRoot, @"Directory\shellex\ContextMenuHandlers\GitExtensions2"))
                   && !string.IsNullOrEmpty(CommonLogic.GetRegistryValue(Registry.ClassesRoot, @"Directory\Background\shellex\ContextMenuHandlers\GitExtensions2"));
        }

        /// <summary>
        /// Performs shell extensions registration
        /// </summary>
        /// <exception cref="FileNotFoundException">If at least one necessary for registration file wasn't found</exception>
        /// <exception cref="Win32Exception">If user canceled elevation dialog</exception>
        public static void Register()
        {
            string path32 = FindFileInBinFolders(CommonLogic.GitExtensionsShellEx32Name);
            if (string.IsNullOrEmpty(path32))
            {
                throw new FileNotFoundException(null, CommonLogic.GitExtensionsShellEx32Name);
            }

            ProcessStartInfo pi = new()
            {
                FileName = "regsvr32",
                Arguments = path32.Quote(),
                Verb = "RunAs",
                UseShellExecute = true
            };

            Process.Start(pi)?.WaitForExit();

            if (IntPtr.Size == 8)
            {
                string path64 = FindFileInBinFolders(CommonLogic.GitExtensionsShellEx64Name);
                if (string.IsNullOrEmpty(path64))
                {
                    throw new FileNotFoundException(null, CommonLogic.GitExtensionsShellEx64Name);
                }

                pi.Arguments = path64.Quote();

                Process.Start(pi)?.WaitForExit();
            }
        }

        private static IEnumerable<string> BinDirectories()
        {
            string installDir = AppSettings.GetInstallDir();
            if (!string.IsNullOrEmpty(installDir))
            {
                yield return installDir;
            }

            string assemblyPath = Assembly.GetAssembly(typeof(ShellExtensionManager))?.Location;
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                string assemblyDir = Path.GetDirectoryName(assemblyPath);
                if (!string.IsNullOrEmpty(assemblyDir))
                {
                    yield return assemblyDir;
                }
            }
        }

        private static string FindFileInBinFolders(string fileName)
        {
            foreach (string binDirectory in BinDirectories())
            {
                string filePath = Path.Combine(binDirectory, fileName);
                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }

            return string.Empty;
        }
    }
}
