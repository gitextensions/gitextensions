using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using GitCommands;
using GitExtensions.Extensibility;
using Microsoft.Win32;

namespace GitUI.CommandsDialogs.SettingsDialog.ShellExtension;

public static class ShellExtensionManager
{
    internal const string GitExtensionsShellEx32Name = "GitExtensionsShellEx32.dll";
    internal const string GitExtensionsShellEx64Name = "GitExtensionsShellEx64.dll";

    public static bool FilesExist()
    {
        string path32 = FindFileInBinFolders(GitExtensionsShellEx32Name);
        string path64 = FindFileInBinFolders(GitExtensionsShellEx64Name);
        return !(string.IsNullOrEmpty(path32)
                 || (Environment.Is64BitOperatingSystem && string.IsNullOrEmpty(path64)));
    }

    public static bool IsRegistered()
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        return !string.IsNullOrEmpty(CommonLogic.GetRegistryValue(
               Registry.LocalMachine,
               @"Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved",
               "{3C16B20A-BA16-4156-916F-0A375ECFFE24}"))
           && !string.IsNullOrEmpty(CommonLogic.GetRegistryValue(
               Registry.ClassesRoot,
               @"*\shellex\ContextMenuHandlers\GitExtensions2"))
           && !string.IsNullOrEmpty(CommonLogic.GetRegistryValue(
               Registry.ClassesRoot,
               @"Directory\shellex\ContextMenuHandlers\GitExtensions2"))
           && !string.IsNullOrEmpty(CommonLogic.GetRegistryValue(
               Registry.ClassesRoot,
               @"Directory\Background\shellex\ContextMenuHandlers\GitExtensions2"));
    }

    public static void Register()
    {
        AppSettings.SetInstallDir(AppSettings.GetGitExtensionsDirectory()!);
        RunRegSvrForShellExtensionDlls("/s {0}");
    }

    public static void Unregister()
        => RunRegSvrForShellExtensionDlls("/s /u {0}");

    private static void RunRegSvrForShellExtensionDlls(string argumentsPattern)
    {
        if (Environment.Is64BitOperatingSystem)
        {
            RunRegSvrForSingleDll(GitExtensionsShellEx64Name, argumentsPattern);
        }

        RunRegSvrForSingleDll(GitExtensionsShellEx32Name, argumentsPattern);

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
                ProcessStartInfo processStartInfo = new()
                {
                    FileName = "regsvr32",
                    Arguments = arguments,
                    Verb = "RunAs",
                    UseShellExecute = true,
                };
                using Process? process = Process.Start(processStartInfo);
                process?.WaitForExit();
                if (process?.ExitCode is not 0)
                {
                    throw new ExternalOperationException(
                        processStartInfo.FileName,
                        processStartInfo.Arguments,
                        exitCode: process?.ExitCode);
                }
            }
            catch (ExternalOperationException exception)
            {
                throw new UserExternalOperationException(context: null!, exception);
            }
            catch (Exception exception)
            {
                throw new UserExternalOperationException(exception);
            }
        }
    }

    private static string FindFileInBinFolders(string fileName)
    {
        foreach (string binDirectory in GetBinDirectories())
        {
            string filePath = Path.Join(binDirectory, fileName);
            if (File.Exists(filePath))
            {
                return filePath;
            }
        }

        return string.Empty;

        static IEnumerable<string> GetBinDirectories()
        {
            string? installDir = AppSettings.GetInstallDir();
            if (!string.IsNullOrEmpty(installDir))
            {
                yield return installDir;
            }

            string? assemblyPath = Assembly.GetAssembly(typeof(ShellExtensionManager))?.Location;
            string? assemblyDirectory = Path.GetDirectoryName(assemblyPath);
            if (!string.IsNullOrEmpty(assemblyDirectory))
            {
                yield return assemblyDirectory;
            }
        }
    }
}
