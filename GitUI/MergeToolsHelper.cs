using System;
using System.IO;
using GitCommands;
using Microsoft.Win32;

namespace GitUI
{
    class MergeToolsHelper
    {     
        private static MergeToolsHelper instance;

        public static MergeToolsHelper Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new MergeToolsHelper();
                }
                return instance;
            }
        }

        private static string GetGlobalSetting(string setting)
        {
            var configFile = GitCommandHelpers.GetGlobalConfig();
            return configFile.GetValue(setting);
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
                string path = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), location);
                if (Directory.Exists(path))
                {
                    string fullName = Path.Combine(path, fileName);
                    if (File.Exists(fullName))
                        return fullName;
                }
                if (8 == IntPtr.Size
                    || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
                {
                    path = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)"), location);
                    if (Directory.Exists(path))
                    {
                        string fullName = Path.Combine(path, fileName);
                        if (File.Exists(fullName))
                            return fullName;
                    }
                }
            }

            return "";
        }

        public static string FindPathForKDiff(string pathFromConfig)
        {
            if (string.IsNullOrEmpty(pathFromConfig) || !File.Exists(pathFromConfig))
            {
                string kdiff3path = pathFromConfig;
                if (Settings.RunningOnUnix())
                {
                    // Maybe command -v is better, but didn't work
                    kdiff3path = Settings.Module.RunCmd("which", "kdiff3").Replace("\n", string.Empty);
                    if (string.IsNullOrEmpty(kdiff3path))
                        return null;
                }
                else if (Settings.RunningOnWindows())
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

        public string FindDiffToolExeFile(string globalDifftoolText, out string exeName)
        {
            string globalDiffTool = globalDifftoolText.ToLowerInvariant();
            switch (globalDiffTool)
            {
                case "beyondcompare3":
                    string bcomppath = GetGlobalSetting("difftool.beyondcompare3.path");
                    
                    exeName = "bcomp.exe";

                    return FindFileInFolders(exeName, bcomppath,
                                                          @"Beyond Compare 3 (x86)\",
                                                          @"Beyond Compare 3\");
                case "kdiff3":
                    string kdiff3path = GetGlobalSetting("difftool.kdiff3.path");
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") + "\\kdiff3.exe";

                    exeName = "kdiff3.exe";

                    return FindFileInFolders(exeName, kdiff3path, @"KDiff3\",
                                                          regkdiff3path);
                case "tmerge":
                    exeName = "TortoiseMerge.exe";
                    string difftoolPath = FindFileInFolders(exeName, @"TortoiseSVN\bin\");
                    if (String.IsNullOrEmpty(difftoolPath))
                        difftoolPath = FindFileInFolders(exeName, @"TortoiseGit\bin\");
                    return difftoolPath;
                case "winmerge":
                    exeName = "winmergeu.exe";
                    string winmergepath = GetGlobalSetting("difftool.winmerge.path");

                    return FindFileInFolders("winmergeu.exe", winmergepath,
                                                          @"WinMerge\");
            }
            exeName = globalDifftoolText + ".exe";
            return GetFullPath(exeName);
        }

        public string DiffToolCmdSuggest(string globalDifftoolText, string exeFile)
        {
            string globalDiffTool = globalDifftoolText.ToLowerInvariant();
            switch (globalDiffTool)
            {
                case "beyondcompare3":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\"";
                case "kdiff3":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\"";
                case "tmerge":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\"";
                case "winmerge":
                    return "\"" + exeFile + "\" -e -u -dl \"Local\" -dr \"Remote\" \"$LOCAL\" \"$REMOTE\"";
            }
            return "";
        }

        public string FindMergeToolExeFile(string globalMergeToolText, out string exeName)
        {
            string globalMergeTool = globalMergeToolText.ToLowerInvariant();

            switch (globalMergeTool)
            {
                case "kdiff3":
                    exeName = "kdiff3.exe";
                    string kdiff3path = GetGlobalSetting("mergetool.kdiff3.path");
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "");
                    if (regkdiff3path != "")
                        regkdiff3path += "\\" + exeName;

                    return FindFileInFolders(exeName, kdiff3path, @"KDiff3\", regkdiff3path);
                case "winmerge":
                    string winmergepath = GetGlobalSetting("mergetool.winmerge.path");

                    exeName = "winmergeu.exe";
                    return FindFileInFolders(exeName, winmergepath, @"WinMerge\");
                case "beyondcompare3":
                    exeName = "bcomp.exe";
                    return FindFileInFolders(exeName,   @"Beyond Compare 3 (x86)\",
                                                        @"Beyond Compare 3\");
                case "p4merge":
                    exeName = "p4merge.exe";
                    return FindFileInFolders(exeName, @"Perforce\");
                case "araxis":
                    exeName = "Compare.exe";
                    return FindFileInFolders(exeName,   @"Araxis\Araxis Merge\",
                                                        @"Araxis 6.5\Araxis Merge\");
                case "tortoisemerge":
                    exeName = "TortoiseMerge.exe";
                    string path = FindFileInFolders(exeName, @"TortoiseSVN\bin\");
                    if (string.IsNullOrEmpty(path))
                        path = FindFileInFolders(exeName, @"TortoiseGit\bin\");
                    return path;
                case "diffmerge":
                    exeName = "DiffMerge.exe";
                    return FindFileInFolders(exeName, @"SourceGear\DiffMerge\");
            }
            exeName = globalMergeToolText + ".exe";
            return GetFullPath(exeName);
        }

        public string MergeToolcmdSuggest(string globalMergetoolText, string exeFile)
        {
            string globalMergeTool = globalMergetoolText.ToLowerInvariant();
            switch (globalMergeTool)
            {
                case "kdiff3":
                    return "";
                case "winmerge":
                    return "\"" + exeFile + "\" -e -u -dl \"Original\" -dr \"Modified\" \"$MERGED\" \"$REMOTE\"";
            }
            return AutoConfigMergeToolCmd(globalMergetoolText, exeFile);
        }

        public string AutoConfigMergeToolCmd(string globalMergetoolText, string exeFile)
        {
            string globalMergeTool = globalMergetoolText.ToLowerInvariant();
            switch (globalMergeTool)
            {
                case "beyondcompare3":
                    return "\"" + exeFile + "\" \"$LOCAL\" \"$REMOTE\" \"$BASE\" \"$MERGED\"";
                case "p4merge":
                    return "\"" + exeFile + "\" \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";
                case "araxis":
                    return "\"" + exeFile +
                                        "\" -wait -merge -3 -a1 \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";
                case "tortoisemerge":
                    string command = "\"{0}\" /base:\"$BASE\" /mine:\"$LOCAL\" /theirs:\"$REMOTE\" /merged:\"$MERGED\"";
                    if (exeFile.ToLower().Contains("tortoisegit"))
                        command = command.Replace("/", "-");

                    return String.Format(command, exeFile);
                case "diffmerge":
                    return "\"" + exeFile + "\" /m /r=\"$MERGED\" \"$LOCAL\" \"$BASE\" \"$REMOTE\"";
            }
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
