using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        struct MergeToolData
        {
            private readonly string name;
            private readonly string exeFile;
            private readonly ReadOnlyCollection<string> exePathes;

            public MergeToolData(string name, string exeFile, string[] exePathes)
            {
                this.name = name;
                this.exeFile = exeFile;
                this.exePathes = new ReadOnlyCollection<string>(exePathes);
            }

            public MergeToolData(string name, string exeFile, string exePath)
                : this(name, exeFile, new string[] { exePath })
            {
            }

            public string Name { get { return name; } }
            public string ExeFile { get { return exeFile; } }
            public ReadOnlyCollection<string> ExePathes { get { return exePathes; } }
        }

        private class KeyEqualityComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, object> keyExtractor;

            public KeyEqualityComparer(Func<T, object> keyExtractor)
            {
                this.keyExtractor = keyExtractor;
            }

            public bool Equals(T x, T y)
            {
                return this.keyExtractor(x).Equals(this.keyExtractor(y));
            }

            public int GetHashCode(T obj)
            {
                return this.keyExtractor(obj).GetHashCode();
            }
        }

        readonly Dictionary<string, MergeToolData> mergeToolData;

        private MergeToolsHelper() 
        {
            var tempData = new[] {
             new MergeToolData ("BeyondCompare3", "bcomp.exe", "Beyond Compare 3"),
             new MergeToolData ("p4merge", "p4merge.exe", "Perforce"),
             new MergeToolData ("Araxis", "Compare.exe", new string[] {@"Araxis\Araxis Merge", @"Araxis 6.5\Araxis Merge"})
            };
            mergeToolData = new Dictionary<string, MergeToolData>(new KeyEqualityComparer<string>( x => x.ToLowerInvariant()));
            foreach (var data in tempData)
                mergeToolData.Add(data.Name, data);
        }   
            
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

        private static string FindFileInFolders(string fileName, params string[] locations)
        {
            foreach (string location in locations)
            {
                if (!string.IsNullOrEmpty(location) && File.Exists(location))
                    return location;
                if (!string.IsNullOrEmpty(location) && File.Exists(location + fileName))
                    return location + fileName;
                if (!string.IsNullOrEmpty(location) && File.Exists(location + "\\" + fileName))
                    return location + "\\" + fileName;
            }

            return "";
        }

        public string MergeToolcmdSuggest(string globalMergetoolText, ref string mergetoolPath)
        {
            string globalMergeTool = globalMergetoolText.ToLowerInvariant();
            switch (globalMergeTool)
            {
                case "kdiff3":
                    string kdiff3path = GetGlobalSetting("mergetool.kdiff3.path");
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") + "\\kdiff3.exe";

                    mergetoolPath = FindFileInFolders("kdiff3.exe", kdiff3path,
                                                           @"c:\Program Files\KDiff3\",
                                                           @"c:\Program Files (x86)\KDiff3\",
                                                           regkdiff3path);
                    break;
                case "winmerge":
                    string winmergepath = GetGlobalSetting("mergetool.winmerge.path");

                    mergetoolPath = FindFileInFolders("winmergeu.exe", winmergepath,
                                                           @"c:\Program Files\winmerge\",
                                                           @"c:\Program Files (x86)\winmerge\");
                    break;
            }
            return AutoConfigMergeToolcmd(globalMergetoolText, ref mergetoolPath);
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
                                                          @"C:\Program Files\Beyond Compare 3 (x86)\",
                                                          @"C:\Program Files\Beyond Compare 3\");

                    if (!File.Exists(difftoolPath))
                    {
                        difftoolPath = "";
                        throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "bcomp.exe"));
                    }
                    return "\"" + difftoolPath + "\" \"$LOCAL\" \"$REMOTE\"";
                case "kdiff3":
                    string kdiff3path = GetGlobalSetting("difftool.kdiff3.path");
                    string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") + "\\kdiff3.exe";

                    difftoolPath = FindFileInFolders("kdiff3.exe", kdiff3path,
                                                          @"c:\Program Files\KDiff3\",
                                                          @"c:\Program Files (x86)\KDiff3\",
                                                          regkdiff3path);
                    if (!File.Exists(difftoolPath))
                    {
                        difftoolPath = "";
                        throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "bcomp.exe"));
                    }
                    return "\"" + difftoolPath + "\" \"$LOCAL\" \"$REMOTE\"";
                case "tmerge":
                    string tortoisemergepath = FindFileInFolders("TortoiseMerge.exe",
                                                           @"c:\Program Files (x86)\TortoiseSVN\bin\",
                                                           @"c:\Program Files\TortoiseSVN\bin\");
                    if (string.IsNullOrEmpty(tortoisemergepath))
                    {
                        tortoisemergepath = FindFileInFolders("TortoiseMerge.exe",
                                                           @"c:\Program Files (x86)\TortoiseGit\bin\",
                                                           @"c:\Program Files\TortoiseGit\bin\");
                    }
                    difftoolPath = tortoisemergepath;
                    if (!File.Exists(difftoolPath))
                    {
                        difftoolPath = "";
                        throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "bcomp.exe"));
                    }
                    return "\"" + difftoolPath + "\" \"$LOCAL\" \"$REMOTE\"";
                case "winmerge":
                    string winmergepath = GetGlobalSetting("difftool.winmerge.path");

                    difftoolPath = FindFileInFolders("winmergeu.exe", winmergepath,
                                                          @"c:\Program Files\winmerge\",
                                                          @"c:\Program Files (x86)\winmerge\");
                    if (!File.Exists(difftoolPath))
                    {
                        difftoolPath = "";
                        throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "bcomp.exe"));
                    }
                    return "\"" + difftoolPath + "\" \"$LOCAL\" \"$REMOTE\"";
            }
            return null;
        }

        public string AutoConfigMergeToolcmd(string globalMergetoolText, ref string mergetoolPath)
        {
            string globalMergeTool = globalMergetoolText.ToLowerInvariant();
            switch (globalMergeTool)
            {
                case "beyondcompare3":
                    if (mergetoolPath.Contains("kdiff3") || mergetoolPath.Contains("TortoiseMerge"))
                        mergetoolPath = "";
                    if (string.IsNullOrEmpty(mergetoolPath) || !File.Exists(mergetoolPath))
                    {
                        mergetoolPath = @"C:\Program Files\Beyond Compare 3\bcomp.exe";

                        mergetoolPath = FindFileInFolders("bcomp.exe",
                                                               @"C:\Program Files\Beyond Compare 3 (x86)\",
                                                               @"C:\Program Files\Beyond Compare 3\");

                        if (!File.Exists(mergetoolPath))
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
                        mergetoolPath = @"c:\Program Files\Perforce\p4merge.exe";

                        mergetoolPath = FindFileInFolders("p4merge.exe",
                                                               @"c:\Program Files (x86)\Perforce\",
                                                               @"c:\Program Files\Perforce\");

                        if (!File.Exists(mergetoolPath))
                        {
                            mergetoolPath = "";
                            throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "bcomp.exe"));
                        }
                    }

                    return "\"" + mergetoolPath + "\" \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";
                case "araxis":
                    if (mergetoolPath.Contains("kdiff3") || mergetoolPath.Contains("TortoiseMerge"))
                        mergetoolPath = "";
                    if (string.IsNullOrEmpty(mergetoolPath) || !File.Exists(mergetoolPath))
                    {
                        mergetoolPath = FindFileInFolders("Compare.exe",
                                                               @"C:\Program Files (x86)\Araxis\Araxis Merge\",
                                                               @"C:\Program Files\Araxis\Araxis Merge\",
                                                               @"C:\Program Files\Araxis 6.5\Araxis Merge\");

                        if (!File.Exists(mergetoolPath))
                        {
                            mergetoolPath = "";
                            throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "bcomp.exe"));
                        }
                    }

                    return "\"" + mergetoolPath +
                                        "\" -wait -merge -3 -a1 \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";
                case "tortoisemerge":
                    string command = "";

                    if (mergetoolPath.ToLower().Contains("kdiff3") || mergetoolPath.ToLower().Contains("p4merge"))
                        mergetoolPath = "";
                    if (string.IsNullOrEmpty(mergetoolPath) || !File.Exists(mergetoolPath))
                    {
                        string path = FindFileInFolders("TortoiseMerge.exe",
                                                               @"c:\Program Files (x86)\TortoiseSVN\bin\",
                                                               @"c:\Program Files\TortoiseSVN\bin\");
                        command = "\"" + path +
                                        "\" /base:\"$BASE\" /mine:\"$LOCAL\" /theirs:\"$REMOTE\" /merged:\"$MERGED\"";
                        if (string.IsNullOrEmpty(path))
                        {
                            path = FindFileInFolders("TortoiseMerge.exe",
                                                               @"c:\Program Files (x86)\TortoiseGit\bin\",
                                                               @"c:\Program Files\TortoiseGit\bin\");
                            command = "\"" + path +
                                        "\" -base:\"$BASE\" -mine:\"$LOCAL\" -theirs:\"$REMOTE\" -merged:\"$MERGED\"";
                        }

                        if (!File.Exists(path))
                        {
                            mergetoolPath = "";
                            throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "bcomp.exe"));
                        }
                        mergetoolPath = path;
                    }

                    return command;
                case "diffmerge":
                    if (mergetoolPath.ToLower().Contains("kdiff3") || mergetoolPath.ToLower().Contains("p4merge"))
                        mergetoolPath = "";
                    if (string.IsNullOrEmpty(mergetoolPath) || !File.Exists(mergetoolPath))
                    {
                        mergetoolPath = FindFileInFolders("DiffMerge.exe",
                                                               @"C:\Program Files (x86)\SourceGear\DiffMerge\",
                                                               @"C:\Program Files\SourceGear\DiffMerge\");

                        if (!File.Exists(mergetoolPath))
                        {
                            mergetoolPath = "";
                            throw new FileNotFoundException(String.Format(_mergeToolSuggest.Text, "bcomp.exe"));
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
