﻿using GitCommands;
using GitCommands.Settings;
using GitCommands.Utils;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using Microsoft.Win32;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class CheckSettingsLogic
    {
        public readonly CommonLogic CommonLogic;
        private GitModule? Module => CommonLogic.Module;
        private ConfigFileSettings GlobalConfigFileSettings => CommonLogic.ConfigFileSettingsSet.GlobalSettings;

        public CheckSettingsLogic(CommonLogic commonLogic)
        {
            CommonLogic = commonLogic;
        }

        public bool AutoSolveAllSettings()
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return SolveGitCommand();
            }

            bool valid = SolveGitCommand();
            valid = SolveLinuxToolsDir() && valid;
            valid = SolveGitExtensionsDir() && valid;
            valid = SolveEditor() && valid;

            CommonLogic.ConfigFileSettingsSet.EffectiveSettings.Save();
            CommonLogic.RepoDistSettingsSet.EffectiveSettings.Save();

            return valid;
        }

        private bool SolveEditor()
        {
            string? editor = CommonLogic.GetGlobalEditor();

            if (string.IsNullOrEmpty(editor))
            {
                GlobalConfigFileSettings.SetPathValue("core.editor", EditorHelper.FileEditorCommand);
            }

            return true;
        }

        public bool SolveLinuxToolsDir(string? possibleNewPath = null)
        {
            if (!EnvUtils.RunningOnWindows())
            {
                AppSettings.GitBinDir = string.Empty;
                return true;
            }

            string gitpath = AppSettings.GitCommandValue;
            if (!string.IsNullOrWhiteSpace(possibleNewPath))
            {
                gitpath = possibleNewPath.Trim();
            }

            foreach (string toolsPath in new[] { @"usr\bin\", @"bin\" })
            {
                string linuxToolsPath = gitpath.Replace(@"\cmd\git.exe", @"\" + toolsPath)
                    .Replace(@"\cmd\git.cmd", @"\" + toolsPath)
                    .Replace(@"\bin\git.exe", @"\" + toolsPath);

                if (ContainsSh(linuxToolsPath))
                {
                    AppSettings.GitBinDir = linuxToolsPath;
                    return true;
                }

                if (CheckIfFileIsInPath("sh.exe") || CheckIfFileIsInPath("sh"))
                {
                    if (ContainsSh(AppSettings.GitBinDir))
                    {
                        return true;
                    }

                    AppSettings.GitBinDir = string.Empty;
                    return true;
                }

                foreach (string path in GetGitLocations())
                {
                    linuxToolsPath = path + toolsPath;
                    if (ContainsSh(gitpath))
                    {
                        AppSettings.GitBinDir = gitpath;
                        return true;
                    }
                }
            }

            return false;

            static bool ContainsSh(string path) => Directory.Exists(path) && (File.Exists(path + "sh.exe") || File.Exists(path + "sh"));
        }

        private static IEnumerable<string> GetGitLocations()
        {
            string envVariable = Environment.GetEnvironmentVariable("GITEXT_GIT");
            if (!string.IsNullOrEmpty(envVariable))
            {
                yield return envVariable;
            }

            yield return
                CommonLogic.GetRegistryValue(Registry.LocalMachine,
                                 "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation");
            string programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            string? programFilesX86 = null;
            if (IntPtr.Size == 8
                || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
            {
                programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            if (programFilesX86 is not null)
            {
                yield return programFilesX86 + @"\Git\";
            }

            yield return programFiles + @"\Git\";
            if (programFilesX86 is not null)
            {
                yield return programFilesX86 + @"\msysgit\";
            }

            yield return programFiles + @"\msysgit\";
            yield return @"C:\msysgit\";

            // cygwin has old git version on windows and bash has a lot of bugs
            yield return @"C:\cygwin\";
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs", "Git\\");
        }

        public bool SolveGitExtensionsDir()
        {
            string? fileName = AppSettings.GetGitExtensionsDirectory();

            if (Directory.Exists(fileName))
            {
                AppSettings.SetInstallDir(fileName!);
                return true;
            }

            return false;
        }

        public static bool SolveGitCommand(string? possibleNewPath = null)
        {
            if (EnvUtils.RunningOnWindows())
            {
                foreach (var command in GetWindowsCommandLocations())
                {
                    if (TestGitCommand(command))
                    {
                        return true;
                    }
                }

                return false;
            }

            AppSettings.GitCommandValue = "git";
            return TestGitCommand(AppSettings.GitCommandValue);

            bool TestGitCommand(string command)
            {
                try
                {
                    // Use cached version if possible
                    if (AppSettings.GitCommand == command && GitVersion.Current?.IsUnknown is false)
                    {
                        return true;
                    }

                    string output = new Executable(command).GetOutput(arguments: "--version");
                    if (!string.IsNullOrEmpty(output))
                    {
                        if (command is not null)
                        {
                            AppSettings.GitCommandValue = command;
                            return true;
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore exception, we are trying to find a way to execute git.exe
                }

                return false;
            }

            IEnumerable<string> GetWindowsCommandLocations()
            {
                if (File.Exists(possibleNewPath))
                {
                    yield return possibleNewPath!;
                }

                if (File.Exists(AppSettings.GitCommandValue))
                {
                    yield return AppSettings.GitCommandValue;
                }

                foreach (var path in GetGitLocations())
                {
                    if (Directory.Exists(path + @"bin\"))
                    {
                        yield return path + @"bin\git.exe";
                    }
                }

                foreach (var path in GetGitLocations())
                {
                    if (Directory.Exists(path + @"cmd\"))
                    {
                        yield return path + @"cmd\git.exe";
                        yield return path + @"cmd\git.cmd";
                    }
                }

                yield return "git";
                yield return "git.cmd";
            }
        }

        public static bool CheckIfFileIsInPath(string fileName)
        {
            return PathUtil.TryFindFullPath(fileName, out _);
        }

        public bool CanFindGitCmd()
        {
            return !string.IsNullOrEmpty(Module?.GitExecutable.GetOutput(arguments: "--version"));
        }
    }
}
