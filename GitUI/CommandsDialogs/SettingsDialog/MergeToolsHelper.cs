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
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(';'))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
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
                case "tmerge":
                    return "TortoiseMerge.exe";
                case "winmerge":
                    return "winmergeu.exe";
            }
            return null;
        }

        public static string FindDiffToolFullPath(ConfigFileSettingsSet settings, string difftoolText, out string exeName)
        {
            string diffTool = difftoolText.ToLowerInvariant();
            switch (diffTool)
            {
                case "beyondcompare3":
                    string bcomppath = UnquoteString(GetGlobalSetting(settings, "difftool.beyondcompare3.path"));
                    
                    exeName = "bcomp.exe";

                    return FindFileInFolders(exeName, bcomppath,
                                                          @"Beyond Compare 3 (x86)\",
                                                          @"Beyond Compare 3\");
                case "beyondcompare4":
                    string bcomppath4 = UnquoteString(GetGlobalSetting(settings, "difftool.beyondcompare4.path"));

                    exeName = "bcomp.exe";

                    return FindFileInFolders(exeName, bcomppath4,
                                                          @"Beyond Compare 4 (x86)\",
                                                          @"Beyond Compare 4\");
                case "kdiff3":
                    string kdiff3path = UnquoteString(GetGlobalSetting(settings, "difftool.kdiff3.path"));
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") + "\\kdiff3.exe";

                    exeName = "kdiff3.exe";

                    return FindFileInFolders(exeName, kdiff3path, @"KDiff3\",
                                                          regkdiff3path);
                case "meld":
                    string difftoolMeldPath = UnquoteString(GetGlobalSetting(settings, "difftool.meld.path"));
                    string programFilesMeldPath = @"Meld\meld\";
                    exeName = "meld.exe";
                    return FindFileInFolders(exeName, difftoolMeldPath, programFilesMeldPath);

                case "tmerge":
                    exeName = "TortoiseGitMerge.exe"; // TortoiseGit 1.8 use new names
                    string difftoolPath = FindFileInFolders(exeName, @"TortoiseGit\bin\");
                    if (String.IsNullOrEmpty(difftoolPath))
                    {
                        exeName = "TortoiseMerge.exe";
                        difftoolPath = FindFileInFolders(exeName, @"TortoiseGit\bin\");
                        if (String.IsNullOrEmpty(difftoolPath))
                            difftoolPath = FindFileInFolders(exeName, @"TortoiseSVN\bin\");
                    }
                    return difftoolPath;
                case "winmerge":
                    exeName = "winmergeu.exe";
                    string winmergepath = UnquoteString(GetGlobalSetting(settings, "difftool.winmerge.path"));

                    return FindFileInFolders("winmergeu.exe", winmergepath,
                                                          @"WinMerge\");
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
                case "tmerge":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\"";
                case "winmerge":
                    return "\"" + exeFile + "\" -e -u \"$LOCAL\" \"$REMOTE\"";
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
                case "p4merge":
                    return "p4merge.exe";
                case "tortoisemerge":
                    return "TortoiseMerge.exe";
                case "winmerge":
                    return "winmergeu.exe";
            }
            return null;
        }

        public static string FindMergeToolFullPath(ConfigFileSettingsSet settings, string mergeToolText, out string exeName)
        {
            string mergeTool = mergeToolText.ToLowerInvariant();

            switch (mergeTool)
            {
                case "araxis":
                    exeName = "Compare.exe";
                    return FindFileInFolders(exeName, @"Araxis\Araxis Merge\",
                                                        @"Araxis 6.5\Araxis Merge\",
                                                        @"Araxis\Araxis Merge v6.5\");
                case "beyondcompare3":
                    string bcomppath = UnquoteString(GetGlobalSetting(settings, "mergetool.beyondcompare3.path"));

                    exeName = "bcomp.exe";
                    return FindFileInFolders(exeName, bcomppath, @"Beyond Compare 3 (x86)\",
                                                                 @"Beyond Compare 3\");
                case "beyondcompare4":
                    string bcomppath4 = UnquoteString(GetGlobalSetting(settings, "mergetool.beyondcompare4.path"));

                    exeName = "bcomp.exe";
                    return FindFileInFolders(exeName, bcomppath4, @"Beyond Compare 4 (x86)\",
                                                                  @"Beyond Compare 4\");
                case "diffmerge":
                    exeName = "DiffMerge.exe";
                    return FindFileInFolders(exeName, @"SourceGear\DiffMerge\");
                case "kdiff3":
                    exeName = "kdiff3.exe";
                    string kdiff3path = UnquoteString(GetGlobalSetting(settings, "mergetool.kdiff3.path"));
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "");
                    if (regkdiff3path != "")
                        regkdiff3path += "\\" + exeName;

                    return FindFileInFolders(exeName, kdiff3path, @"KDiff3\", regkdiff3path);
                case "p4merge":
                    string p4mergepath = UnquoteString(GetGlobalSetting(settings, "mergetool.p4merge.path"));
                    exeName = "p4merge.exe";
                    return FindFileInFolders(exeName, p4mergepath, @"Perforce\");
                case "tortoisemerge":
                    exeName = "TortoiseGitMerge.exe"; // TortoiseGit 1.8 use new names
                    string path = FindFileInFolders(exeName, @"TortoiseGit\bin\");
                    if (string.IsNullOrEmpty(path))
                    {
                        exeName = "TortoiseMerge.exe";
                        path = FindFileInFolders(exeName, @"TortoiseGit\bin\");
                        if (string.IsNullOrEmpty(path))
                            path = FindFileInFolders(exeName, @"TortoiseSVN\bin\");
                    }
                    return path;
                case "winmerge":
                    string winmergepath = UnquoteString(GetGlobalSetting(settings, "mergetool.winmerge.path"));

                    exeName = "winmergeu.exe";
                    return FindFileInFolders(exeName, winmergepath, @"WinMerge\");
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
                case "p4merge":
                    return "\"" + exeFile + "\" \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";
                case "tortoisemerge":
                    string command = "\"{0}\" /base:\"$BASE\" /mine:\"$LOCAL\" /theirs:\"$REMOTE\" /merged:\"$MERGED\"";
                    if (exeFile.ToLower().Contains("tortoisegit"))
                        command = command.Replace("/", "-");

                    return String.Format(command, exeFile);
            }
            // other commands supported natively by msysgit
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
    }
}
