using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommands;
using GitCommands.Config;
using System.IO;

namespace GitUI.SettingsDialog
{
    public class CheckSettingsLogic
    {
        CommonLogic _commonLogic;
        GitModule Module; // TODO: rename to gitModule

        public CheckSettingsLogic(CommonLogic commonLogic, GitModule gitModule)
        {
            _commonLogic = commonLogic;
            Module = gitModule;
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

        private bool CheckGitCredentialStore()
        {
            gitCredentialWinStore.Visible = true;
            bool isValid = !string.IsNullOrEmpty(GitCommandHelpers.GetGlobalConfig().GetValue("credential.helper"));

            if (isValid)
            {
                gitCredentialWinStore.BackColor = Color.LightGreen;
                gitCredentialWinStore.Text = _credentialHelperInstalled.Text;
                gitCredentialWinStore_Fix.Visible = false;
            }
            else
            {
                gitCredentialWinStore.BackColor = Color.LightSalmon;
                gitCredentialWinStore.Text = _noCredentialsHelperInstalled.Text;
                gitCredentialWinStore_Fix.Visible = true;
            }

            return isValid;
        }

        public bool SolveGitCredentialStore()
        {
            if (!CheckGitCredentialStore())
            {
                string gcsFileName = Settings.GetInstallDir() + @"\GitCredentialWinStore\git-credential-winstore.exe";
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

        public bool SolveMergeToolForKDiff()
        {
            string mergeTool = _commonLogic.GetMergeTool();
            if (string.IsNullOrEmpty(mergeTool))
            {
                mergeTool = "kdiff3";
                Module.SetGlobalSetting("merge.tool", mergeTool);
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

        public bool SolveDiffToolPathForKDiff()
        {
            string kdiff3path = MergeToolsHelper.FindPathForKDiff(Module.GetGlobalSetting("difftool.kdiff3.path"));
            if (string.IsNullOrEmpty(kdiff3path))
                return false;

            Module.SetGlobalPathSetting("difftool.kdiff3.path", kdiff3path);
            return true;
        }

        public bool SolveMergeToolPathForKDiff()
        {
            string kdiff3path = MergeToolsHelper.FindPathForKDiff(Module.GetGlobalSetting("mergetool.kdiff3.path"));
            if (string.IsNullOrEmpty(kdiff3path))
                return false;

            Module.SetGlobalPathSetting("mergetool.kdiff3.path", kdiff3path);
            return true;
        }
    }
}
