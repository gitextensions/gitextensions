using System;
using System.IO;
using GitCommands;
using GitCommands.Utils;
using Microsoft.Win32;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    static class MergeToolsHelper
    {
        private static string GetGlobalSetting(ConfigFileSettingsSet settings, string setting)
        {
            return settings.GlobalSettings.GetValue(setting);
        }

        public static string GetFullPath(string fileName)
        {
            string fullPath;
            PathUtil.TryFindFullPath(fileName, out fullPath);

            return fullPath;
        }

        public static string FindFileInFolders(string fileName, params string[] locations)
        {
            foreach (string location in locations)
            {
                if (string.IsNullOrEmpty(location))
                    continue;
                if (Path.IsPathRooted(location))
                {
                    if (File.Exists(location))
                        return location;
                    continue;
                }
                string programFilesPath = Environment.GetEnvironmentVariable("ProgramFiles");

                string path;

                if (!string.IsNullOrEmpty(programFilesPath))
                {
                    path = Path.Combine(programFilesPath, location);
                    if (Directory.Exists(path))
                    {
                        string fullName = Path.Combine(path, fileName);
                        if (File.Exists(fullName))
                            return fullName;
                    }
                }

                if (8 == IntPtr.Size
                    || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
                {
                    programFilesPath = Environment.GetEnvironmentVariable("ProgramFiles(x86)");

                    if (!string.IsNullOrEmpty(programFilesPath))
                    {
                        path = Path.Combine(programFilesPath, location);
                        if (Directory.Exists(path))
                        {
                            string fullName = Path.Combine(path, fileName);
                            if (File.Exists(fullName))
                                return fullName;
                        }
                    }
                }
            }

            return string.Empty;
        }

        private static string UnquoteString(string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            int length = str.Length;
            if (length > 1 && str[0] == '\"' && str[length - 1] == '\"')
                str = str.Substring(1, length - 2);

            return str;
        }

        public static string FindPathForKDiff(string pathFromConfig)
        {
            if (string.IsNullOrEmpty(pathFromConfig) || !File.Exists(pathFromConfig))
            {
                string kdiff3path = pathFromConfig;
                if (EnvUtils.RunningOnUnix())
                {
                    // Maybe command -v is better, but didn't work
                    kdiff3path = GitCommandHelpers.RunCmd("which", "kdiff3").Replace("\n", string.Empty);
                    if (string.IsNullOrEmpty(kdiff3path))
                        return null;
                }
                else if (EnvUtils.RunningOnWindows())
                {
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "");
                    if (regkdiff3path != "")
                        regkdiff3path += "\\kdiff3.exe";

                    kdiff3path = FindFileInFolders("kdiff3.exe", @"KDiff3\", regkdiff3path);
                    if (string.IsNullOrEmpty(kdiff3path))
                        return null;
                }
                return kdiff3path;
            }
            return null;
        }

        public static string GetDiffToolExeFile(string difftoolText)
        {
            string diffTool = difftoolText.ToLowerInvariant();
            switch (diffTool)
            {
                case "araxis":
                    return "Compare.exe";
                case "beyondcompare3":
                    return "bcomp.exe";
                case "beyondcompare4":
                    return "bcomp.exe";
                case "kdiff3":
                    return "kdiff3.exe";
                case "meld":
                    return "meld.exe";
                case "p4merge":
                    return "p4merge.exe";
                case "semanticdiff":
                    return "semanticmergetool.exe";
                case "tmerge":
                    return "TortoiseMerge.exe";
                case "winmerge":
                    return "winmergeu.exe";
                case "vsdiffmerge":
                    return "vsdiffmerge.exe";
                case "vscode":
                    return "code.exe";
            }
            return null;
        }

        public static string FindDiffToolFullPath(ConfigFileSettingsSet settings, string difftoolText, out string exeName)
        {
            string diffTool = difftoolText.ToLowerInvariant();
            exeName = GetDiffToolExeFile(difftoolText);
            switch (diffTool)
            {
                case "beyondcompare3":
                    string bcomppath = UnquoteString(GetGlobalSetting(settings, "difftool.beyondcompare3.path"));
                    return FindFileInFolders(exeName, bcomppath,
                                                          @"Beyond Compare 3 (x86)\",
                                                          @"Beyond Compare 3\");
                case "beyondcompare4":
                    string bcomppath4 = UnquoteString(GetGlobalSetting(settings, "difftool.beyondcompare4.path"));
                    return FindFileInFolders(exeName, bcomppath4,
                                                          @"Beyond Compare 4 (x86)\",
                                                          @"Beyond Compare 4\");
                case "kdiff3":
                    string kdiff3path = UnquoteString(GetGlobalSetting(settings, "difftool.kdiff3.path"));
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") + "\\kdiff3.exe";
                    return FindFileInFolders(exeName, kdiff3path, @"KDiff3\", regkdiff3path);
                case "p4merge":
                    string p4mergepath = UnquoteString(GetGlobalSetting(settings, "difftool.p4merge.path"));
                    return FindFileInFolders(exeName, p4mergepath, @"Perforce\");
                case "meld":
                    string difftoolMeldPath = UnquoteString(GetGlobalSetting(settings, "difftool.meld.path"));
                    return FindFileInFolders(exeName, difftoolMeldPath, @"Meld\", @"Meld (x86)\");
                case "semanticdiff":
                    string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    folder = Path.Combine(folder, @"PlasticSCM4\semanticmerge\");
                    return FindFileInFolders(exeName, folder);
                case "tmerge":
                    exeName = "TortoiseGitMerge.exe"; // TortoiseGit 1.8 use new names
                    string difftoolPath = FindFileInFolders(exeName, @"TortoiseGit\bin\");
                    if (String.IsNullOrEmpty(difftoolPath))
                    {
                        exeName = "TortoiseMerge.exe";
                        difftoolPath = FindFileInFolders(exeName, @"TortoiseGit\bin\", @"TortoiseSVN\bin\");
                    }
                    return difftoolPath;
                case "winmerge":
                    string winmergepath = UnquoteString(GetGlobalSetting(settings, "difftool.winmerge.path"));
                    return FindFileInFolders(exeName, winmergepath, @"WinMerge\");
                case "vsdiffmerge":
                    string vsdiffmergepath = UnquoteString(GetGlobalSetting(settings, "difftool.vsdiffmerge.path"));
                    string regvsdiffmergepath = GetVisualStudioPath() + exeName;
                    return FindFileInFolders(exeName, vsdiffmergepath, regvsdiffmergepath);
                case "vscode":
                    string vscodepath = UnquoteString(GetGlobalSetting(settings, "difftool.vscode.path"));
                    return FindFileInFolders(exeName, vscodepath, @"Microsoft VS Code");
            }
            exeName = difftoolText + ".exe";
            return GetFullPath(exeName);
        }

        public static string DiffToolCmdSuggest(string diffToolText, string exeFile)
        {
            string diffTool = diffToolText.ToLowerInvariant();
            switch (diffTool)
            {
                case "beyondcompare3":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\"";
                case "beyondcompare4":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\"";
                case "kdiff3":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\"";
                case "meld":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\"";
                case "p4merge":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\"";
                case "semanticdiff":
                    return "\"" + exeFile + "\" -s \"$LOCAL\" -d \"$REMOTE\"";
                case "tmerge":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\"";
                case "winmerge":
                    return "\"" + exeFile + "\" -e -u \"$LOCAL\" \"$REMOTE\"";
                case "vsdiffmerge":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\"";
                case "vscode":
                    return "\"" + exeFile + "\" --wait --diff \"$LOCAL\" \"$REMOTE\"";
            }
            return "";
        }

        public static string GetMergeToolExeFile(string mergeToolText)
        {
            string mergeTool = mergeToolText.ToLowerInvariant();

            switch (mergeTool)
            {
                case "araxis":
                    return "Compare.exe";
                case "beyondcompare3":
                    return "bcomp.exe";
                case "beyondcompare4":
                    return "bcomp.exe";
                case "diffmerge":
                    return "DiffMerge.exe";
                case "kdiff3":
                    return "kdiff3.exe";
                case "meld":
                    return "meld.exe";
                case "p4merge":
                    return "p4merge.exe";
                case "semanticmerge":
                    return "semanticmergetool.exe";
                case "tortoisemerge":
                    return "TortoiseMerge.exe";
                case "winmerge":
                    return "winmergeu.exe";
                case "vsdiffmerge":
                    return "vsdiffmerge.exe";
                case "vscode":
                    return "vscode.exe";
            }
            return null;
        }

        public static string FindMergeToolFullPath(ConfigFileSettingsSet settings, string mergeToolText, out string exeName)
        {
            string mergeTool = mergeToolText.ToLowerInvariant();
            exeName = GetMergeToolExeFile(mergeToolText);
            switch (mergeTool)
            {
                case "araxis":
                    return FindFileInFolders(exeName, @"Araxis\Araxis Merge\",
                                                        @"Araxis 6.5\Araxis Merge\",
                                                        @"Araxis\Araxis Merge v6.5\");
                case "beyondcompare3":
                    string bcomppath = UnquoteString(GetGlobalSetting(settings, "mergetool.beyondcompare3.path"));

                    return FindFileInFolders(exeName, bcomppath, @"Beyond Compare 3 (x86)\",
                                                                 @"Beyond Compare 3\");
                case "beyondcompare4":
                    string bcomppath4 = UnquoteString(GetGlobalSetting(settings, "mergetool.beyondcompare4.path"));

                    return FindFileInFolders(exeName, bcomppath4, @"Beyond Compare 4 (x86)\",
                                                                  @"Beyond Compare 4\");
                case "diffmerge":
                    return FindFileInFolders(exeName, @"SourceGear\Common\DiffMerge\", @"SourceGear\DiffMerge\");
                case "kdiff3":
                    string kdiff3path = UnquoteString(GetGlobalSetting(settings, "mergetool.kdiff3.path"));
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "");
                    if (regkdiff3path != "")
                        regkdiff3path += "\\" + exeName;

                    return FindFileInFolders(exeName, kdiff3path, @"KDiff3\", regkdiff3path);
                case "meld":
                    string mergetoolMeldPath = UnquoteString(GetGlobalSetting(settings, "mergetool.meld.path"));
                    return FindFileInFolders(exeName, mergetoolMeldPath, @"Meld\", @"Meld (x86)\");
                case "p4merge":
                    string p4mergepath = UnquoteString(GetGlobalSetting(settings, "mergetool.p4merge.path"));
                    return FindFileInFolders(exeName, p4mergepath, @"Perforce\");
                case "semanticmerge":
                    string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    folder = Path.Combine(folder, @"PlasticSCM4\semanticmerge\");
                    return FindFileInFolders(exeName, folder);
                case "tortoisemerge":
                    exeName = "TortoiseGitMerge.exe"; // TortoiseGit 1.8 use new names
                    string path = FindFileInFolders(exeName, @"TortoiseGit\bin\");
                    if (string.IsNullOrEmpty(path))
                    {
                        exeName = "TortoiseMerge.exe";
                        path = FindFileInFolders(exeName, @"TortoiseGit\bin\", @"TortoiseSVN\bin\");
                    }
                    return path;
                case "winmerge":
                    string winmergepath = UnquoteString(GetGlobalSetting(settings, "mergetool.winmerge.path"));

                    return FindFileInFolders(exeName, winmergepath, @"WinMerge\");
                case "vsdiffmerge":
                    string vsdiffmergepath = UnquoteString(GetGlobalSetting(settings, "mergetool.vsdiffmerge.path"));
                    string regvsdiffmergepath = GetVisualStudioPath() + exeName;
                    return FindFileInFolders(exeName, vsdiffmergepath, regvsdiffmergepath);
                case "vscode":
                    string vscodepath = UnquoteString(GetGlobalSetting(settings, "mergetool.vscode.path"));
                    return FindFileInFolders(exeName, vscodepath, @"Microsoft VS Code");
            }
            exeName = mergeToolText + ".exe";
            return GetFullPath(exeName);
        }

        public static string MergeToolcmdSuggest(string mergeToolText, string exeFile)
        {
            string mergeTool = mergeToolText.ToLowerInvariant();
            switch (mergeTool)
            {
                case "kdiff3":
                    return "";
                case "winmerge":
                    return "\"" + exeFile + "\" -e -u -dl \"Original\" -dr \"Modified\" \"$MERGED\" \"$REMOTE\"";
                case "vsdiffmerge":
                    return "\"" + exeFile + "\" /m \"$REMOTE\" \"$LOCAL\" \"$BASE\" \"$MERGED\" ";
            }
            return AutoConfigMergeToolCmd(mergeToolText, exeFile);
        }

        public static string AutoConfigMergeToolCmd(string mergetoolText, string exeFile)
        {
            string mergeTool = mergetoolText.ToLowerInvariant();
            switch (mergeTool)
            {
                case "beyondcompare3":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\" \"$BASE\" \"$MERGED\"";
                case "beyondcompare4":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\" \"$BASE\" \"$MERGED\"";
                case "diffmerge":
                    return "\"" + exeFile + "\" /m /r=\"$MERGED\" \"$LOCAL\" \"$BASE\" \"$REMOTE\"";
                case "meld":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$BASE\" \"$REMOTE\" --output \"$MERGED\"";
                case "p4merge":
                    return "\"" + exeFile + "\" \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";
                case "semanticmerge":
                    return "\"" + exeFile + "\" -s \"$REMOTE\" -d \"$LOCAL\" -b \"$BASE\" -r \"$MERGED\"";
                case "tortoisemerge":
                    string command = "\"{0}\" /base:\"$BASE\" /mine:\"$LOCAL\" /theirs:\"$REMOTE\" /merged:\"$MERGED\"";
                    if (exeFile.ToLower().Contains("tortoisegit"))
                        command = command.Replace("/", "-");

                    return String.Format(command, exeFile);
                case "vscode":
                    return "\"" + exeFile + "\" --wait \"$MERGED\" ";
                case "vsdiffmerge":
                    return "\"" + exeFile + "\" /m \"$REMOTE\" \"$LOCAL\" \"$BASE\" \"$MERGED\"";
            }
            // other commands supported natively by git for windows
            return "";
        }

        private static string GetRegistryValue(RegistryKey root, string subkey, string key)
        {
            string value = null;
            try
            {
                RegistryKey registryKey = root.OpenSubKey(subkey, false);
                if (registryKey != null)
                {
                    using (registryKey)
                    {
                        value = registryKey.GetValue(key) as string;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            return value ?? string.Empty;
        }

        private static string GetVisualStudioPath()
        {
            //%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
            var vsVersionNumber = new string[] { "2020", "2019", "2018", "2017" };
            var vsVersionType = new string[] { "Professional", "Community" };
            string pathFormat = @"Microsoft Visual Studio\{0}\{1}\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\";

            foreach (var number in vsVersionNumber)
            {
                foreach (var type in vsVersionType)
                {
                    var vsPath = string.Format(pathFormat, number, type);
                    string path = FindFileInFolders("vsDiffMerge.exe", vsPath);

                    if (!string.IsNullOrEmpty(path))
                    {
                        return path;
                    }
                }
            }

            var vsVersions = new string[] { "14.0", "12.0", "11.0" };

            foreach (var version in vsVersions)
            {
                string registryKeyString = string.Format(@"SOFTWARE{0}Microsoft\VisualStudio\{1}", Environment.Is64BitProcess ? @"\Wow6432Node\" : "\\", version);
                using (RegistryKey localMachineKey = Registry.LocalMachine.OpenSubKey(registryKeyString))
                {
                    if (localMachineKey != null)
                    {
                        var path = localMachineKey.GetValue("InstallDir") as string;
                        if (!string.IsNullOrEmpty(path))
                        {
                            return path;
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}
