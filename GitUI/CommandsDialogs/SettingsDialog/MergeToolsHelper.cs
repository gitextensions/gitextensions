using System;
using System.IO;
using GitCommands;
using GitCommands.Utils;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    internal static class MergeToolsHelper
    {
        private static string GetGlobalSetting(ConfigFileSettingsSet settings, string setting)
        {
            return settings.GlobalSettings.GetValue(setting);
        }

        [CanBeNull]
        public static string GetFullPath(string fileName)
        {
            PathUtil.TryFindFullPath(fileName, out var fullPath);

            return fullPath;
        }

        public static string FindFileInFolders(string fileName, params string[] locations)
        {
            foreach (string location in locations)
            {
                if (string.IsNullOrEmpty(location))
                {
                    continue;
                }

                if (Path.IsPathRooted(location))
                {
                    if (File.Exists(location))
                    {
                        return location;
                    }

                    continue;
                }

                string fullName;
                string programFilesPath = Environment.GetEnvironmentVariable("ProgramFiles") ?? "";

                if (CheckFileExists(programFilesPath))
                {
                    return fullName;
                }

                programFilesPath = Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? "";

                if ((IntPtr.Size == 8 ||
                    !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))) &&
                    CheckFileExists(programFilesPath))
                {
                    return fullName;
                }

                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                if (CheckFileExists(localAppDataPath))
                {
                    return fullName;
                }

                string localAppDataProgramsPath = Path.Combine(localAppDataPath, "Programs");

                if (CheckFileExists(localAppDataProgramsPath))
                {
                    return fullName;
                }

                bool CheckFileExists(string path)
                {
                    fullName = Path.Combine(path, location, fileName);
                    return File.Exists(fullName);
                }
            }

            return "";
        }

        [CanBeNull]
        private static string UnquoteString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            int length = str.Length;
            if (length > 1 && str[0] == '\"' && str[length - 1] == '\"')
            {
                str = str.Substring(1, length - 2);
            }

            return str;
        }

        [CanBeNull]
        public static string FindPathForKDiff(string pathFromConfig)
        {
            if (string.IsNullOrEmpty(pathFromConfig) || !File.Exists(pathFromConfig))
            {
                string kdiff3path = pathFromConfig;
                if (EnvUtils.RunningOnUnix())
                {
                    // Maybe command -v is better, but didn't work
                    kdiff3path = new Executable("which").GetOutput("kdiff3").Replace("\n", "");
                    if (string.IsNullOrEmpty(kdiff3path))
                    {
                        return null;
                    }
                }
                else if (EnvUtils.RunningOnWindows())
                {
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "");
                    if (regkdiff3path != "")
                    {
                        regkdiff3path += "\\kdiff3.exe";
                    }

                    kdiff3path = FindFileInFolders("kdiff3.exe", @"KDiff3\", regkdiff3path);
                    if (string.IsNullOrEmpty(kdiff3path))
                    {
                        return null;
                    }
                }

                return kdiff3path;
            }

            return null;
        }

        [CanBeNull]
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
                case "diffmerge":
                    return "sgdm.exe";
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
                    return FindDiffToolFullPath(settings, exeName, "difftool.beyondcompare3.path",
                                                          @"Beyond Compare 3 (x86)\",
                                                          @"Beyond Compare 3\");
                case "beyondcompare4":
                    return FindDiffToolFullPath(settings, exeName, "difftool.beyondcompare4.path",
                                                          @"Beyond Compare 4 (x86)\",
                                                          @"Beyond Compare 4\");
                case "diffmerge":
                    return FindDiffToolFullPath(settings, exeName, "difftool.diffmerge.path",
                        @"SourceGear\Common\DiffMerge\", @"SourceGear\DiffMerge\");
                case "kdiff3":
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") + "\\kdiff3.exe";
                    return FindDiffToolFullPath(settings, exeName, "difftool.kdiff3.path", @"KDiff3\", regkdiff3path);
                case "p4merge":
                    return FindDiffToolFullPath(settings, exeName, "difftool.p4merge.path", @"Perforce\");
                case "meld":
                    return FindDiffToolFullPath(settings, exeName, "difftool.meld.path", @"Meld\", @"Meld (x86)\");
                case "semanticdiff":
                    string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    folder = Path.Combine(folder, @"PlasticSCM4\semanticmerge\");
                    return FindFileInFolders(exeName, folder);
                case "tmerge":
                    exeName = "TortoiseGitMerge.exe"; // TortoiseGit 1.8 use new names
                    string difftoolPath = FindFileInFolders(exeName, @"TortoiseGit\bin\");
                    if (string.IsNullOrEmpty(difftoolPath))
                    {
                        exeName = "TortoiseMerge.exe";
                        difftoolPath = FindFileInFolders(exeName, @"TortoiseGit\bin\", @"TortoiseSVN\bin\");
                    }

                    return difftoolPath;
                case "winmerge":
                    return FindDiffToolFullPath(settings, exeName, "difftool.winmerge.path", @"WinMerge\");
                case "vsdiffmerge":
                    return FindDiffToolFullPath(settings, exeName, "difftool.vsdiffmerge.path", GetVsDiffMergePath());
                case "vscode":
                    return FindDiffToolFullPath(settings, exeName, "difftool.vscode.path", @"Microsoft VS Code");
            }

            exeName = difftoolText + ".exe";
            return GetFullPath(exeName);
        }

        private static string FindDiffToolFullPath(ConfigFileSettingsSet settings, string exeName, string settingsKey, params string[] pathToSearch)
        {
            string exePathFromSettings = UnquoteString(GetGlobalSetting(settings, settingsKey));
            var paths = new string[pathToSearch.Length + 1];
            paths[0] = exePathFromSettings;
            Array.Copy(pathToSearch, 0, paths, 1, pathToSearch.Length);
            return FindFileInFolders(exeName, paths);
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
                case "diffmerge":
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

        [CanBeNull]
        public static string GetMergeToolExeFile(string mergeToolText)
        {
            string mergeTool = mergeToolText.ToLowerInvariant();
            var exeName = GetDiffToolExeFile(mergeTool);
            if (exeName != null)
            {
                return exeName;
            }

            switch (mergeTool)
            {
                case "tortoisemerge":
                    return "TortoiseMerge.exe";
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
                    return FindDiffToolFullPath(settings, exeName, "mergetool.beyondcompare3.path",
                        @"Beyond Compare 3 (x86)\",
                        @"Beyond Compare 3\");
                case "beyondcompare4":
                    return FindDiffToolFullPath(settings, exeName, "mergetool.beyondcompare4.path",
                        @"Beyond Compare 4 (x86)\",
                        @"Beyond Compare 4\");
                case "diffmerge":
                    return FindDiffToolFullPath(settings, exeName, "mergetool.diffmerge.path",
                        @"SourceGear\Common\DiffMerge\",
                        @"SourceGear\DiffMerge\");
                case "kdiff3":
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "");
                    if (regkdiff3path != "")
                    {
                        regkdiff3path += "\\" + exeName;
                    }

                    return FindDiffToolFullPath(settings, exeName, "mergetool.kdiff3.path", @"KDiff3\", regkdiff3path);
                case "meld":
                    return FindDiffToolFullPath(settings, exeName, "mergetool.meld.path", @"Meld\", @"Meld (x86)\");
                case "p4merge":
                    return FindDiffToolFullPath(settings, exeName, "mergetool.p4merge.path", @"Perforce\");
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
                    return FindDiffToolFullPath(settings, exeName, "mergetool.winmerge.path", @"WinMerge\");
                case "vsdiffmerge":
                    return FindDiffToolFullPath(settings, exeName, "mergetool.vsdiffmerge.path", GetVsDiffMergePath());
                case "vscode":
                    return FindDiffToolFullPath(settings, exeName, "mergetool.vscode.path", @"Microsoft VS Code");
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
                    return "\"" + exeFile + "\" -merge -result=\"$MERGED\" \"$LOCAL\" \"$BASE\" \"$REMOTE\"";
                case "meld":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$BASE\" \"$REMOTE\" --output \"$MERGED\"";
                case "p4merge":
                    return "\"" + exeFile + "\" \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";
                case "semanticmerge":
                    return "\"" + exeFile + "\" -s \"$REMOTE\" -d \"$LOCAL\" -b \"$BASE\" -r \"$MERGED\"";
                case "tortoisemerge":
                    string command = "\"{0}\" /base:\"$BASE\" /mine:\"$LOCAL\" /theirs:\"$REMOTE\" /merged:\"$MERGED\"";
                    if (exeFile.ToLower().Contains("tortoisegit"))
                    {
                        command = command.Replace("/", "-");
                    }

                    return string.Format(command, exeFile);
                case "vscode":
                    return "\"" + exeFile + "\" --wait \"$MERGED\" ";
                case "vsdiffmerge":
                    return "\"" + exeFile + "\" /m \"$REMOTE\" \"$LOCAL\" \"$BASE\" \"$MERGED\"";
                case "winmerge":
                    return "\"" + exeFile + "\" -e -u  -wl -wr -fm -dl \"Mine: $LOCAL\" -dm \"Merged: $BASE\" -dr \"Theirs: $REMOTE\" \"$LOCAL\" \"$BASE\" \"$REMOTE\" -o \"$MERGED\"";
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

        private static string GetVsDiffMergePath()
        {
            const string exeName = "vsDiffMerge.exe";
            if (!EnvUtils.RunningOnWindows())
            {
                return exeName;
            }

            var vsVersions = new[] { "14.0", "12.0", "11.0" };

            foreach (var version in vsVersions)
            {
                string registryKeyString = string.Format(@"SOFTWARE{0}Microsoft\VisualStudio\{1}", Environment.Is64BitProcess ? @"\Wow6432Node\" : "\\", version);
                using (RegistryKey localMachineKey = Registry.LocalMachine.OpenSubKey(registryKeyString))
                {
                    var path = localMachineKey?.GetValue("InstallDir") as string;
                    if (!string.IsNullOrEmpty(path))
                    {
                        return Path.Combine(path, exeName);
                    }
                }
            }

            return exeName;
        }
    }
}
