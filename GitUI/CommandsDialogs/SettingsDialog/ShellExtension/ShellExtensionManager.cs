using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using GitCommands;
using GitExtUtils;
using GitUI.NBugReports;
using Microsoft.Win32;

namespace GitUI.CommandsDialogs.SettingsDialog.ShellExtension
{
    public static class ShellExtensionManager
    {
        internal const string GitExtensionsShellEx32Name = "GitExtensionsShellEx32.dll";
        internal const string GitExtensionsShellEx64Name = "GitExtensionsShellEx64.dll";

        /// <summary>
        ///  Checks if the shell extension files are found at the known location.
        /// </summary>
        /// <returns><see langword="true"/> the extension files are found at the known location; otherwise <see langword="false"/>.</returns>
        public static bool FilesExist()
        {
            string path32 = FindFileInBinFolders(GitExtensionsShellEx32Name);
            string path64 = FindFileInBinFolders(GitExtensionsShellEx64Name);
            return !(string.IsNullOrEmpty(path32) || (Environment.Is64BitOperatingSystem && string.IsNullOrEmpty(path64)));
        }

        /// <summary>
        ///  Checks if the shell extension is properly registered in the system.
        /// </summary>
        /// <returns><see langword="true"/> if the extension is registered; otherwise <see langword="false"/>.</returns>
        public static bool IsRegistered()
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
        /// <exception cref="Win32Exception">If user canceled elevation dialog (when ex.NativeErrorCode == 1223)</exception>
        /// <exception cref="Exception">Other potential error</exception>
        public static void Register() => RunRegSvrForShellExtensionDlls("/s {0}");

        /// <summary>
        /// Unregister shell extensions
        /// </summary>
        /// <exception cref="FileNotFoundException">If at least one necessary for registration file wasn't found</exception>
        /// <exception cref="Win32Exception">If user canceled elevation dialog (when ex.NativeErrorCode == 1223)</exception>
        /// <exception cref="Exception">Other potential error</exception>
        public static void Unregister() => RunRegSvrForShellExtensionDlls("/s /u {0}");

        private static void RunRegSvrForShellExtensionDlls(string argumentsPattern)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                RunRegSvrForSingleDll(GitExtensionsShellEx64Name, argumentsPattern);
            }

            RunRegSvrForSingleDll(GitExtensionsShellEx32Name, argumentsPattern);

            return;

            static void RunRegSvrForSingleDll(string dllName, string argumentsPattern)
            {
                try
                {
                    string path = FindFileInBinFolders(dllName);
                    if (string.IsNullOrEmpty(path))
                    {
                        throw new FileNotFoundException(null, dllName);
                    }

                    string arguments = string.Format(argumentsPattern, path.Quote());
                    ProcessStartInfo pi = new()
                    {
                        FileName = "regsvr32",
                        Arguments = arguments,
                        Verb = "RunAs",
                        UseShellExecute = true
                    };
                    Process? process = Process.Start(pi);
                    process?.WaitForExit();
                    if (process?.ExitCode is not 0)
                    {
                        throw new ExternalOperationException(pi.FileName, pi.Arguments, exitCode: process?.ExitCode);
                    }
                }
                catch (ExternalOperationException ex)
                {
                    throw new UserExternalOperationException(context: null, ex);
                }
                catch (Exception ex)
                {
                    throw new UserExternalOperationException(ex);
                }
            }
        }

        private static string FindFileInBinFolders(string fileName)
        {
            foreach (string binDirectory in GetBinDirectories())
            {
                string filePath = Path.Combine(binDirectory, fileName);
                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }

            return string.Empty;

            static IEnumerable<string> GetBinDirectories()
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
        }
    }
}
