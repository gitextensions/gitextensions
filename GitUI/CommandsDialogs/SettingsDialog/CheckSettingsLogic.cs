using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitCommands.Config;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;
using ResourceManager.Translation;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class CheckSettingsLogic : Translate
    {
        public readonly TranslationString ToolSuggestPathText =
            new TranslationString("Please enter the path to {0} and press suggest.");

        public readonly TranslationString MergeToolSuggestCaption = new TranslationString("Suggest mergetool cmd");

        readonly CommonLogic _commonLogic;
        readonly GitModule _gitModule;

        public CheckSettingsLogic(CommonLogic commonLogic, GitModule gitModule)
        {
            _commonLogic = commonLogic;
            _gitModule = gitModule;
        }

        public bool AutoSolveAllSettings()
        {
            if (!Settings.RunningOnWindows())
                return SolveGitCommand();

            bool valid = true;
            valid = SolveGitCommand() && valid;
            valid = SolveLinuxToolsDir() && valid;
            valid = SolveMergeToolForKDiff() && valid;
            valid = SolveDiffToolForKDiff() && valid;
            valid = SolveGitExtensionsDir() && valid;
            valid = SolveEditor() && valid;
            valid = SolveGitCredentialStore() && valid;

            return valid;
        }

        private bool SolveEditor()
        {
            string editor = _commonLogic.GetGlobalEditor();
            if (string.IsNullOrEmpty(editor))
            {
                _gitModule.SetGlobalPathSetting("core.editor", "\"" + Settings.GetGitExtensionsFullPath() + "\" fileeditor");
            }

            return true;
        }

        public bool SolveGitCredentialStore()
        {
            if (!CheckGitCredentialStore())
            {
                string gcsFileName = Path.Combine(Settings.GetInstallDir(), @"GitCredentialWinStore\git-credential-winstore.exe");
                if (File.Exists(gcsFileName))
                {
                    ConfigFile config = GitCommandHelpers.GetGlobalConfig();
                    if (Settings.RunningOnWindows())
                        config.SetValue("credential.helper", "!\\\"" + GitCommandHelpers.FixPath(gcsFileName) + "\\\"");
                    else if (Settings.RunningOnMacOSX())
                        config.SetValue("credential.helper", "osxkeychain");
                    else
                        config.SetValue("credential.helper", "cache --timeout=300"); // 5 min
                    config.Save();
                    return true;
                }
                return false;
            }
            return true;
        }

        public bool CheckGitCredentialStore()
        {
            string value = GitCommandHelpers.GetGlobalConfig().GetValue("credential.helper");
            bool isValid;
            if (Settings.RunningOnWindows())
                isValid = value.Contains("git-credential-winstore.exe");
            else
                isValid = !string.IsNullOrEmpty(value);

            return isValid;
        }

        public bool SolveLinuxToolsDir()
        {
            if (!Settings.RunningOnWindows())
            {
                Settings.GitBinDir = "";
                return true;
            }

            string gitpath = Settings.GitCommand
                .Replace(@"\cmd\git.exe", @"\bin\")
                .Replace(@"\cmd\git.cmd", @"\bin\")
                .Replace(@"\bin\git.exe", @"\bin\");
            if (Directory.Exists(gitpath))
            {
                if (File.Exists(gitpath + "sh.exe") || File.Exists(gitpath + "sh"))
                {
                    Settings.GitBinDir = gitpath;
                    return true;
                }
            }

            if (CheckIfFileIsInPath("sh.exe") || CheckIfFileIsInPath("sh"))
            {
                Settings.GitBinDir = "";
                return true;
            }

            foreach (var path in GetGitLocations())
            {
                if (Directory.Exists(path + @"bin\"))
                {
                    if (File.Exists(path + @"bin\sh.exe") || File.Exists(path + @"bin\sh"))
                    {
                        Settings.GitBinDir = path + @"bin\";
                        return true;
                    }
                }
            }
            return false;
        }

        private IEnumerable<string> GetGitLocations()
        {
            yield return
                _commonLogic.GetRegistryValue(Registry.LocalMachine,
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
            yield return @"C:\cygwin\";
        }

        private IEnumerable<string> GetWindowsCommandLocations()
        {
            if (!string.IsNullOrEmpty(Settings.GitCommand) && File.Exists(Settings.GitCommand))
                yield return Settings.GitCommand;
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
            string fileName = Settings.GetGitExtensionsDirectory();

            if (Directory.Exists(fileName))
            {
                Settings.SetInstallDir(fileName);
                return true;
            }

            return false;
        }

        public bool SolveGitCommand()
        {
            if (Settings.RunningOnWindows())
            {
                var command = (from cmd in GetWindowsCommandLocations()
                               let output = _gitModule.RunCmd(cmd, string.Empty)
                               where !string.IsNullOrEmpty(output)
                               select cmd).FirstOrDefault();

                if (command != null)
                {
                    Settings.GitCommand = command;
                    return true;
                }
                return false;
            }
            Settings.GitCommand = "git";
            return !string.IsNullOrEmpty(_gitModule.RunGitCmd(""));
        }

        public static bool CheckIfFileIsInPath(string fileName)
        {
            string path = string.Concat(Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.User), ";",
                                        Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.Machine));

            return path.Split(';').Any(dir => File.Exists(dir + " \\" + fileName) || File.Exists(Path.Combine(dir, fileName)));
        }

        public bool SolveMergeToolForKDiff()
        {
            string mergeTool = _commonLogic.GetGlobalMergeTool();
            if (string.IsNullOrEmpty(mergeTool))
            {
                mergeTool = "kdiff3";
                _gitModule.SetGlobalSetting("merge.tool", mergeTool);
            }

            if (mergeTool.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                return SolveMergeToolPathForKDiff();

            return true;
        }

        public bool SolveDiffToolForKDiff()
        {
            string diffTool = GetGlobalDiffToolFromConfig();
            if (string.IsNullOrEmpty(diffTool))
            {
                diffTool = "kdiff3";
                ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();
                SetGlobalDiffToolToConfig(globalConfig, diffTool);
                globalConfig.Save();
            }

            if (diffTool.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                return SolveDiffToolPathForKDiff();

            return true;
        }

        public static string GetGlobalDiffToolFromConfig()
        {
            if (GitCommandHelpers.VersionInUse.GuiDiffToolExist)
                return GitCommandHelpers.GetGlobalConfig().GetValue("diff.guitool");
            return GitCommandHelpers.GetGlobalConfig().GetValue("diff.tool");
        }

        public static void SetGlobalDiffToolToConfig(ConfigFile configFile, string diffTool)
        {
            if (GitCommandHelpers.VersionInUse.GuiDiffToolExist)
            {
                configFile.SetValue("diff.guitool", diffTool);
                return;
            }
            configFile.SetValue("diff.tool", diffTool);
        }

        public bool SolveDiffToolPathForKDiff()
        {
            string kdiff3path = MergeToolsHelper.FindPathForKDiff(_gitModule.GetGlobalSetting("difftool.kdiff3.path"));
            if (string.IsNullOrEmpty(kdiff3path))
                return false;

            _gitModule.SetGlobalPathSetting("difftool.kdiff3.path", kdiff3path);
            return true;
        }

        public bool SolveMergeToolPathForKDiff()
        {
            string kdiff3path = MergeToolsHelper.FindPathForKDiff(_gitModule.GetGlobalSetting("mergetool.kdiff3.path"));
            if (string.IsNullOrEmpty(kdiff3path))
                return false;

            _gitModule.SetGlobalPathSetting("mergetool.kdiff3.path", kdiff3path);
            return true;
        }

        public bool CanFindGitCmd()
        {
            return !string.IsNullOrEmpty(_gitModule.RunGitCmd(""));
        }

        public void AutoConfigMergeToolCmd(bool silent)
        {
            string exeName;
            string exeFile = MergeToolsHelper.FindMergeToolFullPath(GetGlobalMergeToolText(), out exeName);
            if (String.IsNullOrEmpty(exeFile))
            {
                SetMergetoolPathText("");
                SetMergeToolCmdText("");
                if (!silent)
                    MessageBox.Show(/*this, */String.Format(ToolSuggestPathText.Text, exeName),
                        MergeToolSuggestCaption.Text);
                return;
            }

            SetMergetoolPathText(exeFile);
            SetMergeToolCmdText(MergeToolsHelper.AutoConfigMergeToolCmd(GetGlobalMergeToolText(), exeFile));
        }

        private void SetMergetoolPathText(string text)
        {
            ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();
            globalConfig.SetPathValue(string.Format("mergetool.{0}.path", GetGlobalMergeToolText()), text);
            // orig (TODO: remove comment and rename method):
            //// MergetoolPath.Text = ...
        }

        private void SetMergeToolCmdText(string text)
        {
            ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();
            globalConfig.SetPathValue(string.Format("mergetool.{0}.cmd", GetGlobalMergeToolText()), text);
            // orig (TODO: remove comment and rename method):
            //// MergeToolCmd.Text = ...
        }

        private string GetGlobalMergeToolText()
        {
            ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();
            return globalConfig.GetValue("merge.tool");
            // orig (TODO: remove comment and rename method):
            //// GlobalMergeTool.Text;
        }

        public string GetMergeToolCmdText()
        {
            ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();
            return globalConfig.GetPathValue(string.Format("mergetool.{0}.cmd", GetGlobalMergeToolText()));
            // orig (TODO: remove comment and rename method):
            //// MergeToolCmd.Text
        }
    }
}
