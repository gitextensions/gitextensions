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
            return !(string.IsNullOrEmpty(path32) || (Environment.Is64BitOperatingSystem && string.IsNullOrEmpty(path64)));
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
        /// Register shell extensions
        /// </summary>
        /// <exception cref="FileNotFoundException">If at least one necessary for registration file wasn't found</exception>
        /// <exception cref="Win32Exception">If user canceled elevation dialog</exception>
        public static void Register() => RunRegSvrForShellExtensionDlls("{0}");

        /// <summary>
        /// Unregister shell extensions
        /// </summary>
        /// <exception cref="FileNotFoundException">If at least one necessary for registration file wasn't found</exception>
        /// <exception cref="Win32Exception">If user canceled elevation dialog</exception>
        public static void Unregister() => RunRegSvrForShellExtensionDlls("/u {0}");

        private static void RunRegSvrForShellExtensionDlls(string argumentsPattern)
        {
            RunRegSvrForSingleDll(CommonLogic.GitExtensionsShellEx32Name, argumentsPattern);
            if (Environment.Is64BitOperatingSystem)
            {
                RunRegSvrForSingleDll(CommonLogic.GitExtensionsShellEx64Name, argumentsPattern);
            }
        }

        private static void RunRegSvrForSingleDll(string dllName, string argumentsPattern)
        {
            string path = FindFileInBinFolders(dllName);
            if (string.IsNullOrEmpty(path))
            {
                throw new FileNotFoundException(null, dllName);
            }

            string arguments = string.Format(argumentsPattern, path);
            ProcessStartInfo pi = new ProcessStartInfo()
            {
                FileName = "regsvr32",
                Arguments = arguments,
                Verb = "RunAs",
                UseShellExecute = true
            };
            Process.Start(pi)?.WaitForExit();
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
