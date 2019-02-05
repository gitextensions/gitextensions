using System;
using System.Collections.Generic;
using System.IO;
using GitCommands;
using GitCommands.Settings;
using GitCommands.Utils;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using Microsoft.Win32;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class CheckSettingsLogic
    {
        public readonly CommonLogic CommonLogic;
        private GitModule Module => CommonLogic.Module;
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
            valid = SolveMergeToolForKDiff() && valid;
            valid = SolveDiffToolForKDiff() && valid;
            valid = SolveGitExtensionsDir() && valid;
            valid = SolveEditor() && valid;

            CommonLogic.ConfigFileSettingsSet.EffectiveSettings.Save();
            CommonLogic.RepoDistSettingsSet.EffectiveSettings.Save();

            return valid;
        }

        private bool SolveEditor()
        {
            string editor = CommonLogic.GetGlobalEditor();

            if (string.IsNullOrEmpty(editor))
            {
                GlobalConfigFileSettings.SetPathValue("core.editor", EditorHelper.FileEditorCommand);
            }

            return true;
        }

        public bool SolveLinuxToolsDir(string possibleNewPath = null)
        {
            if (!EnvUtils.RunningOnWindows())
            {
                AppSettings.GitBinDir = "";
                return true;
            }

            string gitpath = AppSettings.GitCommandValue;
            if (!string.IsNullOrWhiteSpace(possibleNewPath))
            {
                gitpath = possibleNewPath.Trim();
            }

            foreach (var toolsPath in new[] { @"bin\", @"usr\bin\" })
            {
                gitpath = gitpath.Replace(@"\cmd\git.exe", @"\" + toolsPath)
                    .Replace(@"\cmd\git.cmd", @"\" + toolsPath)
                    .Replace(@"\bin\git.exe", @"\" + toolsPath);

                if (Directory.Exists(gitpath))
                {
                    if (File.Exists(gitpath + "sh.exe") || File.Exists(gitpath + "sh"))
                    {
                        AppSettings.GitBinDir = gitpath;
                        return true;
                    }
                }

                if (CheckIfFileIsInPath("sh.exe") || CheckIfFileIsInPath("sh"))
                {
                    AppSettings.GitBinDir = "";
                    return true;
                }

                foreach (var path in GetGitLocations())
                {
                    if (Directory.Exists(path + toolsPath))
                    {
                        if (File.Exists(path + toolsPath + "sh.exe") || File.Exists(path + toolsPath + "sh"))
                        {
                            AppSettings.GitBinDir = path + toolsPath;
                            return true;
                        }
                    }
                }
            }

            return false;
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
            string programFilesX86 = null;
            if (IntPtr.Size == 8
                || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
            {
                programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            if (programFilesX86 != null)
            {
                yield return programFilesX86 + @"\Git\";
            }

            yield return programFiles + @"\Git\";
            if (programFilesX86 != null)
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
            string fileName = AppSettings.GetGitExtensionsDirectory();

            if (Directory.Exists(fileName))
            {
                AppSettings.SetInstallDir(fileName);
                return true;
            }

            return false;
        }

        public static bool SolveGitCommand(string possibleNewPath = null)
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
                    string output = new Executable(command).GetOutput();
                    if (!string.IsNullOrEmpty(output))
                    {
                        if (command != null)
                        {
                            AppSettings.GitCommandValue = command;
                            return true;
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore expection, we are trying to find a way to execute git.exe
                }

                return false;
            }

            IEnumerable<string> GetWindowsCommandLocations()
            {
                if (File.Exists(possibleNewPath))
                {
                    yield return possibleNewPath;
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

        public bool SolveMergeToolForKDiff()
        {
            string mergeTool = CommonLogic.GetGlobalMergeTool();
            if (string.IsNullOrEmpty(mergeTool))
            {
                mergeTool = "kdiff3";
                GlobalConfigFileSettings.SetValue("merge.tool", mergeTool);
            }

            if (mergeTool.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                return SolveMergeToolPathForKDiff();
            }

            return true;
        }

        public bool SolveDiffToolForKDiff()
        {
            string diffTool = GetDiffToolFromConfig(GlobalConfigFileSettings);
            if (string.IsNullOrEmpty(diffTool))
            {
                diffTool = "kdiff3";
                SetDiffToolToConfig(GlobalConfigFileSettings, diffTool);
            }

            if (diffTool.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                return SolveDiffToolPathForKDiff();
            }

            return true;
        }

        public static string GetDiffToolFromConfig(ConfigFileSettings settings)
        {
            return settings.GetValue("diff.guitool");
        }

        public static void SetDiffToolToConfig(ConfigFileSettings settings, string diffTool)
        {
            settings.SetValue("diff.guitool", diffTool);
        }

        public bool SolveDiffToolPathForKDiff()
        {
            string kdiff3path = MergeToolsHelper.FindPathForKDiff(GlobalConfigFileSettings.GetValue("difftool.kdiff3.path"));
            if (string.IsNullOrEmpty(kdiff3path))
            {
                return false;
            }

            GlobalConfigFileSettings.SetPathValue("difftool.kdiff3.path", kdiff3path);
            return true;
        }

        public bool SolveMergeToolPathForKDiff()
        {
            string kdiff3path = MergeToolsHelper.FindPathForKDiff(GlobalConfigFileSettings.GetValue("mergetool.kdiff3.path"));
            if (string.IsNullOrEmpty(kdiff3path))
            {
                return false;
            }

            GlobalConfigFileSettings.SetPathValue("mergetool.kdiff3.path", kdiff3path);
            return true;
        }

        public bool CanFindGitCmd()
        {
            return !string.IsNullOrEmpty(Module.GitExecutable.GetOutput(""));
        }

        public void AutoConfigMergeToolCmd()
        {
            string exeFile = MergeToolsHelper.FindMergeToolFullPath(CommonLogic.ConfigFileSettingsSet, GetGlobalMergeToolText(), out _);

            if (string.IsNullOrEmpty(exeFile))
            {
                SetMergetoolPathText("");
                SetMergeToolCmdText("");
            }

            SetMergetoolPathText(exeFile);
            SetMergeToolCmdText(MergeToolsHelper.AutoConfigMergeToolCmd(GetGlobalMergeToolText(), exeFile));
        }

        private void SetMergetoolPathText(string text)
        {
            GlobalConfigFileSettings.SetPathValue(string.Format("mergetool.{0}.path", GetGlobalMergeToolText()), text);

            // orig (TODO: remove comment and rename method):
            //// MergetoolPath.Text = ...
        }

        private void SetMergeToolCmdText(string text)
        {
            GlobalConfigFileSettings.SetPathValue(string.Format("mergetool.{0}.cmd", GetGlobalMergeToolText()), text);

            // orig (TODO: remove comment and rename method):
            //// MergeToolCmd.Text = ...
        }

        private string GetGlobalMergeToolText()
        {
            return GlobalConfigFileSettings.GetValue("merge.tool");

            // orig (TODO: remove comment and rename method):
            //// GlobalMergeTool.Text;
        }

        public string GetMergeToolCmdText()
        {
            return GlobalConfigFileSettings.GetValue(string.Format("mergetool.{0}.cmd", GetGlobalMergeToolText()));

            // orig (TODO: remove comment and rename method):
            //// MergeToolCmd.Text
        }
    }
}
