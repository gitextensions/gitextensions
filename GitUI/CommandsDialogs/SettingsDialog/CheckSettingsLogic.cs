﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitCommands;
using GitCommands.Settings;
using GitCommands.Utils;
using Microsoft.Win32;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class CheckSettingsLogic
    {
        public readonly CommonLogic CommonLogic;
        private GitModule Module { get { return CommonLogic.Module; } }
        private ConfigFileSettings GlobalConfigFileSettings { get { return CommonLogic.ConfigFileSettingsSet.GlobalSettings; } }

        public CheckSettingsLogic(CommonLogic commonLogic)
        {
            CommonLogic = commonLogic;
        }

        public bool AutoSolveAllSettings()
        {
            if (!EnvUtils.RunningOnWindows())
                return SolveGitCommand();

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
                GlobalConfigFileSettings.SetPathValue("core.editor", "\"" + AppSettings.GetGitExtensionsFullPath() + "\" fileeditor");
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
            if (!String.IsNullOrWhiteSpace(possibleNewPath))
            {
                gitpath = possibleNewPath.Trim();
            }

            foreach (var toolsPath in new[] { @"usr\bin\", @"bin\" })
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

        private IEnumerable<string> GetGitLocations()
        {
            string envVariable = Environment.GetEnvironmentVariable("GITEXT_GIT");
            if (!String.IsNullOrEmpty(envVariable)) yield return envVariable;
            yield return
                CommonLogic.GetRegistryValue(Registry.LocalMachine,
                                 "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation");
            string programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            string programFilesX86 = null;
            if (8 == IntPtr.Size
                || !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
                programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            if (programFilesX86 != null)
                yield return programFilesX86 + @"\Git\";
            yield return programFiles + @"\Git\";
            if (programFilesX86 != null)
                yield return programFilesX86 + @"\msysgit\";
            yield return programFiles + @"\msysgit\";
            yield return @"C:\msysgit\";
            // cygwin has old git version on windows and bash has a lot of bugs
            yield return @"C:\cygwin\";
        }

        private IEnumerable<string> GetWindowsCommandLocations(string possibleNewPath = null)
        {
            if (!string.IsNullOrEmpty(possibleNewPath) && File.Exists(possibleNewPath))
                yield return possibleNewPath;
            if (!string.IsNullOrEmpty(AppSettings.GitCommandValue) && File.Exists(AppSettings.GitCommandValue))
                yield return AppSettings.GitCommandValue;
            foreach (var path in GetGitLocations())
            {
                if (Directory.Exists(path + @"bin\"))
                    yield return path + @"bin\git.exe";
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

        public bool SolveGitCommand(string possibleNewPath = null)
        {
            if (EnvUtils.RunningOnWindows())
            {
                var command = (from cmd in GetWindowsCommandLocations(possibleNewPath)
                               let output = Module.RunCmd(cmd, string.Empty)
                               where !string.IsNullOrEmpty(output)
                               select cmd).FirstOrDefault();

                if (command != null)
                {
                    AppSettings.GitCommandValue = command;
                    return true;
                }
                return false;
            }
            AppSettings.GitCommandValue = "git";
            return !string.IsNullOrEmpty(Module.RunGitCmd(""));
        }

        public static bool CheckIfFileIsInPath(string fileName)
        {
            string path = Environment.GetEnvironmentVariable("PATH");

            foreach(string rawdir in path.Split(';'))
            {
                string dir = rawdir;
                dir = dir.Trim();
                // Usually, paths with spaces are not quoted on %PATH%, but it's well possible, and .NET won't consume a quoted path
                // This does not handle the full grammar of the %PATH%, but at least prevents Illegal Characters in Path exceptions (see #2924)
                if((dir.Length >= 2) && (dir[0] == '"') && (dir[dir.Length - 1] == '"'))
                    dir = dir.Substring(1, dir.Length - 2);
                if(dir.Length == 0)
                    continue;
                try
                {
                    if(File.Exists(dir + " \\" + fileName) || File.Exists(Path.Combine(dir, fileName)))
                        return true;
                }
                catch(ArgumentException /* e.g. illegal characters in path — might get any from %PATH% */)
                {
                }
                catch(IOException /* problems checking file existence in FS */)
                {
                }
            }

            return false;
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
                return SolveMergeToolPathForKDiff();

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
                return SolveDiffToolPathForKDiff();

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
                return false;

            GlobalConfigFileSettings.SetPathValue("difftool.kdiff3.path", kdiff3path);
            return true;
        }

        public bool SolveMergeToolPathForKDiff()
        {
            string kdiff3path = MergeToolsHelper.FindPathForKDiff(GlobalConfigFileSettings.GetValue("mergetool.kdiff3.path"));
            if (string.IsNullOrEmpty(kdiff3path))
                return false;

            GlobalConfigFileSettings.SetPathValue("mergetool.kdiff3.path", kdiff3path);
            return true;
        }

        public bool CanFindGitCmd()
        {
            return !string.IsNullOrEmpty(Module.RunGitCmd(""));
        }

        public void AutoConfigMergeToolCmd()
        {
            string exeName;
            string exeFile = MergeToolsHelper.FindMergeToolFullPath(CommonLogic.ConfigFileSettingsSet, GetGlobalMergeToolText(), out exeName);
            if (String.IsNullOrEmpty(exeFile))
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
