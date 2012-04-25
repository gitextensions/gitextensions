using System;
using System.IO;
using GitCommands;
using Microsoft.Win32;
using ResourceManager.Translation;

namespace GitUI
{
    class MergeToolsHelper : Translate
    {
        #region Translation strings
        private readonly TranslationString _mergeToolSuggest =
            new TranslationString("Please enter the path to {0} and press suggest.");
        #endregion
     
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
        
        public string DiffToolCmdSuggest(string globalDifftoolText, ref string difftoolPath)
        {
            string globalDiffTool = globalDifftoolText.ToLowerInvariant();
            switch (globalDiffTool)
            {
                case "beyondcompare3":
                    string bcomppath = GetGlobalSetting("difftool.beyondcompare3.path");

                    difftoolPath = FindFileInFolders("bcomp.exe",
                                                          bcomppath,
                                                          @"Beyond Compare 3 (x86)\",
                                                          @"Beyond Compare 3\");

                    if (String.IsNullOrEmpty(difftoolPath))
                    {
                        difftoolPath = "";
                        throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "bcomp.exe"));
                    }
                    return "\"" + difftoolPath + "\" \"$LOCAL\" \"$REMOTE\"";
                case "kdiff3":
                    string kdiff3path = GetGlobalSetting("difftool.kdiff3.path");
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") + "\\kdiff3.exe";

                    difftoolPath = FindFileInFolders("kdiff3.exe", kdiff3path, @"KDiff3\",
                                                          regkdiff3path);

                    if (String.IsNullOrEmpty(difftoolPath))
                    {
                        difftoolPath = "";
                        throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "kdiff3.exe"));
                    }
                    return "\"" + difftoolPath + "\" \"$LOCAL\" \"$REMOTE\"";
                case "tmerge":
                    difftoolPath = FindFileInFolders("TortoiseMerge.exe", @"TortoiseSVN\bin\");
                    if (String.IsNullOrEmpty(difftoolPath))
                        difftoolPath = FindFileInFolders("TortoiseMerge.exe", @"TortoiseGit\bin\");

                    if (String.IsNullOrEmpty(difftoolPath))
                    {
                        difftoolPath = "";
                        throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "TortoiseMerge.exe"));
                    }
                    return "\"" + difftoolPath + "\" \"$LOCAL\" \"$REMOTE\"";
                case "winmerge":
                    string winmergepath = GetGlobalSetting("difftool.winmerge.path");

                    difftoolPath = FindFileInFolders("winmergeu.exe", winmergepath,
                                                          @"WinMerge\");

                    if (String.IsNullOrEmpty(difftoolPath))
                    {
                        difftoolPath = "";
                        throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "winmergeu.exe"));
                    }
                    return "\"" + difftoolPath + "\" \"$LOCAL\" \"$REMOTE\"";
            }
            return null;
        }

        public string MergeToolcmdSuggest(string globalMergetoolText, ref string mergetoolPath)
        {
            string globalMergeTool = globalMergetoolText.ToLowerInvariant();
            switch (globalMergeTool)
            {
                case "kdiff3":
                    string kdiff3path = GetGlobalSetting("mergetool.kdiff3.path");
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "");
                    if (regkdiff3path != "")
                        regkdiff3path += "\\kdiff3.exe";

                    mergetoolPath = FindFileInFolders("kdiff3.exe", kdiff3path, @"KDiff3\", regkdiff3path);
                    break;
                case "winmerge":
                    string winmergepath = GetGlobalSetting("mergetool.winmerge.path");

                    mergetoolPath = FindFileInFolders("winmergeu.exe", winmergepath, @"WinMerge\");
                    break;
            }
            return AutoConfigMergeToolCmd(globalMergetoolText, ref mergetoolPath);
        }

        public string AutoConfigMergeToolCmd(string globalMergetoolText, ref string mergetoolPath)
        {
            string globalMergeTool = globalMergetoolText.ToLowerInvariant();
            switch (globalMergeTool)
            {
                case "beyondcompare3":
                    if (mergetoolPath.Contains("kdiff3") || mergetoolPath.Contains("TortoiseMerge"))
                        mergetoolPath = "";
                    if (string.IsNullOrEmpty(mergetoolPath) || !File.Exists(mergetoolPath))
                    {
                        mergetoolPath = FindFileInFolders("bcomp.exe",
                                                               @"Beyond Compare 3 (x86)\",
                                                               @"Beyond Compare 3\");

                        if (String.IsNullOrEmpty(mergetoolPath))
                        {
                            mergetoolPath = "";
                            throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "bcomp.exe"));
                        }
                    }

                    return "\"" + mergetoolPath + "\" \"$LOCAL\" \"$REMOTE\" \"$BASE\" \"$MERGED\"";
                case "p4merge":
                    if (mergetoolPath.Contains("kdiff3") || mergetoolPath.Contains("TortoiseMerge"))
                        mergetoolPath = "";
                    if (string.IsNullOrEmpty(mergetoolPath) || !File.Exists(mergetoolPath))
                    {
                        mergetoolPath = FindFileInFolders("p4merge.exe", @"Perforce\");

                        if (String.IsNullOrEmpty(mergetoolPath))
                        {
                            mergetoolPath = "";
                            throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "p4merge.exe"));
                        }
                    }

                    return "\"" + mergetoolPath + "\" \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";
                case "araxis":
                    if (mergetoolPath.Contains("kdiff3") || mergetoolPath.Contains("TortoiseMerge"))
                        mergetoolPath = "";
                    if (string.IsNullOrEmpty(mergetoolPath) || !File.Exists(mergetoolPath))
                    {
                        mergetoolPath = FindFileInFolders("Compare.exe",
                                                               @"Araxis\Araxis Merge\",
                                                               @"Araxis 6.5\Araxis Merge\");

                        if (String.IsNullOrEmpty(mergetoolPath))
                        {
                            mergetoolPath = "";
                            throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "Compare.exe"));
                        }
                    }

                    return "\"" + mergetoolPath +
                                        "\" -wait -merge -3 -a1 \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";
                case "tortoisemerge":
                    if (mergetoolPath.ToLower().Contains("kdiff3") || mergetoolPath.ToLower().Contains("p4merge"))
                        mergetoolPath = "";
                    if (string.IsNullOrEmpty(mergetoolPath) || !File.Exists(mergetoolPath))
                    {
                        string path = FindFileInFolders("TortoiseMerge.exe", @"TortoiseSVN\bin\");
                        if (string.IsNullOrEmpty(path))
                            path = FindFileInFolders("TortoiseMerge.exe", @"TortoiseGit\bin\");

                        if (String.IsNullOrEmpty(mergetoolPath))
                        {
                            mergetoolPath = "";
                            throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "TortoiseMerge.exe"));
                        }
                        mergetoolPath = path;
                    }

                    string command = "\"{0}\" /base:\"$BASE\" /mine:\"$LOCAL\" /theirs:\"$REMOTE\" /merged:\"$MERGED\"";
                    if (mergetoolPath.ToLower().Contains("tortoisegit"))
                        command = command.Replace("/", "-");

                    return String.Format(command, mergetoolPath);
                case "diffmerge":
                    if (mergetoolPath.ToLower().Contains("kdiff3") || mergetoolPath.ToLower().Contains("p4merge"))
                        mergetoolPath = "";
                    if (string.IsNullOrEmpty(mergetoolPath) || !File.Exists(mergetoolPath))
                    {
                        mergetoolPath = FindFileInFolders("DiffMerge.exe", @"SourceGear\DiffMerge\");

                        if (String.IsNullOrEmpty(mergetoolPath))
                        {
                            mergetoolPath = "";
                            throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "DiffMerge.exe"));
                        }
                    }

                    // /m /r=%merged /t1=%yname /t2=%bname /t3=%tname /c=%mname %mine %base %theirs
                    return "\"" + mergetoolPath + "\" /m /r=\"$MERGED\" \"$LOCAL\" \"$BASE\" \"$REMOTE\"";
            }
            return null;
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
