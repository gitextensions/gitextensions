using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.IO;
using PatchApply;
using System.Diagnostics;

namespace GitCommands
{
    public class GitCommands
    {
        public static string FindGitWorkingDir(string startDir)
        {
            string dir = startDir;
            if (!dir.EndsWith("\\") && !dir.EndsWith("/"))
                dir += "\\";

            while (dir.LastIndexOfAny(new char[]{'\\', '/'}) > 0)
            {
                dir = dir.Substring(0, dir.LastIndexOfAny(new char[] { '\\', '/' }));

                if (Directory.Exists(dir + "\\" + ".git"))
                    return dir + "\\";
            }
            return startDir;
        }

        public static string RunCmd(string cmd)
        {
            return RunCmd(cmd, "");
        }

        public static void RunRealCmd(string cmd, string arguments)
        {
            Settings.GitLog += cmd + " " + arguments + "\n";
            //process used to execute external commands

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardInput = false;

            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.FileName = cmd;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = Settings.WorkingDir;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            process.StartInfo.LoadUserProfile = true;

            process.Start();
            process.WaitForExit();
            process.Close();
            
        }

        public static void RunRealCmdDetatched(string cmd, string arguments)
        {
            Settings.GitLog += cmd + " " + arguments + "\n";
            //process used to execute external commands

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardInput = false;

            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.FileName = cmd;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = Settings.WorkingDir;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            process.StartInfo.LoadUserProfile = true;

            process.Start();


        }

        public static void Run(string cmd, string arguments)
        {
            Settings.GitLog += cmd + " " + arguments + "\n";
            //process used to execute external commands

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = cmd;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = Settings.WorkingDir;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            process.StartInfo.LoadUserProfile = true;

            process.Start();
            //process.WaitForExit();

        }


        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static string RunCmd(string cmd, string arguments)
        {
            Settings.GitLog += cmd + " " + arguments + "\n";
            //process used to execute external commands

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = cmd;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = Settings.WorkingDir;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            process.StartInfo.LoadUserProfile = true;

            process.Start();

            string output;
            string error;
                
            output = process.StandardOutput.ReadToEnd();
            error = process.StandardError.ReadToEnd();

            process.WaitForExit();
            process.Close();
            // Read the output stream first and then wait. 

            if (!string.IsNullOrEmpty(error))
            {
                output += "\n" + error;
            }
            
            return output;
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static Process RunCmdAsync(string cmd, string arguments)
        {
            Settings.GitLog += cmd + " " + arguments + "\n";
            //process used to execute external commands

            Process process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.ErrorDialog = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardInput = false;
            process.StartInfo.RedirectStandardError = false;

            process.StartInfo.LoadUserProfile = true;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.FileName = cmd;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = Settings.WorkingDir;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            process.StartInfo.LoadUserProfile = true;

            process.Start();

            return process;
        }

        static public void RunGui()
        {
            Run(Settings.GitDir + "git.exe", "gui");
        }


        static public void RunBash()
        {
            RunRealCmdDetatched("C:\\Windows\\System32\\cmd.exe", "/c \"" + Settings.GitDir + "sh.exe\" --login -i");
        }

        static public string Reset()
        {
            return RunCmd(Settings.GitDir + "git.exe", "reset --hard");
        }

        public string FormatPatch(string from, string to, string output)
        {
            string result = RunCmd(Settings.GitDir + "git.exe", "format-patch -M -C -B " + from + ".." + to + " -o \"" + output + "\"");

            return result;
        }

        static public string Branch(string branchName, string revision)
        {
            string result = GitCommands.RunCmd(Settings.GitDir + "git.exe", "branch \"" + branchName + "\" \"" + revision + "\"");

            return result;
        }

        static public string Push(string path)
        {
            string result = GitCommands.RunCmd(Settings.GitDir + "git.exe", "push \"" + path + "\"");

            return result;
        }

        static public Process PushAsync(string path)
        {
            return RunCmdAsync(Settings.GitDir + "cmd.exe", " /k git.exe push \"" + path + "\"");

        }

        static public string Pull(string path, string branch)
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            GitCommands.RunRealCmd(Settings.GitDir + "cmd.exe", " /k git.exe pull \"" + path + "\" \"" + branch + "\"");

            return "Done";
        }

        static public string Resolved()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.exe", "am --3way --resolved");

            return result;
        }

        static public string Skip()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.exe", "am --3way --skip");

            return result;
        }

        static public string Abort()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.exe", "am --3way --abort");

            return result;
        }

        static public string Patch(string patchFile)
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.exe", "am --3way --signoff " + patchFile);

            return result;
        }

        public string GetGlobalSetting(string setting)
        {
            return RunCmd(Settings.GitDir + "git.exe", "config --global --get " + setting);
        }

        public void SetGlobalSetting(string setting, string value)
        {
            GitCommands.RunCmd(Settings.GitDir + "git.exe", "config --global --unset-all " + setting);
            GitCommands.RunCmd(Settings.GitDir + "git.exe", "config --global " + setting + " \"" + value + "\"");
        }

        static public string GetSetting(string setting)
        {
            return GitCommands.RunCmd(Settings.GitDir + "git.exe", "config --get " + setting);
        }

        static public void SetSetting(string setting, string value)
        {
            GitCommands.RunCmd(Settings.GitDir + "git.exe", "config --unset-all " + setting);
            GitCommands.RunCmd(Settings.GitDir + "git.exe", "config " + setting + " \"" + value + "\"");
        }

        static public Patch GetSingleDiff(string from, string to, string filter)
        {
            PatchManager patchManager = new PatchManager();
            patchManager.LoadPatch(GitCommands.RunCmd(Settings.GitDir + "git.exe", "diff --ignore-submodules " + to + " " + from + " -- " + filter), false);

            return patchManager.patches.FirstOrDefault();
        }

        static public List<Patch> GetDiff(string from, string to)
        {
            PatchManager patchManager = new PatchManager();
            patchManager.LoadPatch(GitCommands.RunCmd(Settings.GitDir + "git.exe", "diff " + from + " " + to), false);

            return patchManager.patches;
        }

        static public List<string> GetDiffFiles(string from)
        {
            string result = RunCmd(Settings.GitDir + "git.exe", "diff --name-only " + from);

            string[] files = result.Split('\n');

            return files.ToList<string>();
        }

        static public List<string> GetDiffFiles(string from, string to)
        {
            string result = RunCmd(Settings.GitDir + "git.exe", "diff --name-only " + from + " " + to);

            string[] files = result.Split('\n');

            return files.ToList<string>();
        }

        static public List<GitItemStatus> GitStatus()
        {
            string status = RunCmd(Settings.GitDir + "git.exe", "status --untracked=all");

            string[] statusStrings = status.Split('\n');

            List<GitItemStatus> gitItemStatusList = new List<GitItemStatus>();

            foreach (string statusString in statusStrings)
            {
                if (statusString.StartsWith("#\tnew file:"))
                {
                    GitItemStatus itemStatus = new GitItemStatus();
                    itemStatus.IsNew = true;
                    itemStatus.IsTracked = true;
                    itemStatus.Name = statusString.Substring(statusString.LastIndexOf(':') + 1).Trim();
                    gitItemStatusList.Add(itemStatus);
                } else
                if (statusString.StartsWith("#\tdeleted:"))
                {
                    GitItemStatus itemStatus = new GitItemStatus();
                    itemStatus.IsDeleted = true;
                    itemStatus.IsTracked = true;
                    itemStatus.Name = statusString.Substring(statusString.LastIndexOf(':')+1).Trim();
                    gitItemStatusList.Add(itemStatus);
                } else
                    if (statusString.StartsWith("#\tmodified:"))
                {
                    GitItemStatus itemStatus = new GitItemStatus();
                    itemStatus.IsChanged = true;
                    itemStatus.IsTracked = true;
                    itemStatus.Name = statusString.Substring(statusString.LastIndexOf(':') + 1).Trim();
                    gitItemStatusList.Add(itemStatus);
                } else
                if (statusString.StartsWith("#\t"))
                {
                    GitItemStatus itemStatus = new GitItemStatus();
                    itemStatus.IsNew = true;
                    itemStatus.Name = statusString.Substring(2).Trim();
                    gitItemStatusList.Add(itemStatus);
                }
            }

            return gitItemStatusList;
        }

        static public string GetCurrentChanges(string name)
        {
            //return RunCmd(Settings.GitDir + "git.exe", "diff --cached " + name);
            return RunCmd(Settings.GitDir + "git.exe", "diff " + name);
        }

        static public List<GitRevision> GitRevisionGraph()
        {
            List<GitHead> heads = GetHeads(true);

            string tree = RunCmd(Settings.GitDir + "git.exe", "log --graph --all --pretty=format:\"Commit %H%nTree:   %T%nAuthor: %an%nDate:   %cd%nParents:%P%n%s\"");

            string[] itemsStrings = tree.Split('\n');
            
            List<GitRevision> revisions = new List<GitRevision>();

            char[] graphChars = new char[]{'*','|','*','\\','/'};

            for (int n = 0; n < itemsStrings.Count(); )
            {
                GitRevision revision = new GitRevision();

                string line;

                line = itemsStrings[n];
                int graphIndex = 0;
                if (line.IndexOf("Commit ") > 0)
                {
                    graphIndex = line.IndexOf("Commit ");
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    revision.Name = revision.Guid = line.Substring(line.LastIndexOf("Commit ") + 7);
                    n++;
                    if (itemsStrings.Count() == n) break;
                }
                line = itemsStrings[n];

                if (line.IndexOf("Tree:   ") > 0)
                {
                    revision.TreeGuid = line.Substring(line.LastIndexOf("Tree:   ") + 8);
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    n++;
                }
                line = itemsStrings[n];

                if (line.IndexOf("Merge: ") > 0)
                {
                    //ignore
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    n++;
                }
                line = itemsStrings[n];

                if (line.IndexOf("Author: ") > 0)
                {
                    revision.Author = line.Substring(line.LastIndexOf("Author: ") + 8);
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    n++;
                    if (itemsStrings.Count() == n) break;
                }
                line = itemsStrings[n];

                if (line.IndexOf("Date:   ") > 0)
                {
                    revision.Date = line.Substring(line.LastIndexOf("Date:   ") + 8);
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    n++;
                    if (itemsStrings.Count() == n) break;
                }
                line = itemsStrings[n];

                if (line.IndexOf("Parents:") > 0)
                {
                    revision.ParentGuids = line.Substring(line.LastIndexOf("Parents:") + 8).Split(' ').ToList();
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    n++;
                    if (itemsStrings.Count() == n) break;
                }
                line = itemsStrings[n];

                var foundHeads = from GitHead h in heads
                                 where h.Guid == revision.Guid
                                 select h;

                foreach (var head in foundHeads)
                {
                    revision.Message += "[" + head.Name + "] ";
                }

                while (!(line.Length == line.LastIndexOf("Commit ") + 7 + 40) || (line.LastIndexOf("Commit ") < 0))
                {
                    if (line.Length > graphIndex)
                        revision.Message += line.Substring(graphIndex).Trim() + "\n";
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    n++;
                    if (itemsStrings.Count() == n)
                    {
                        break;
                    }
                    line = itemsStrings[n];
                }
                if (itemsStrings.Count() == n) break;
                //n--;

                revisions.Add(revision);
            }

            return revisions;
        }


        static public List<GitRevision> GitRevisions()
        {
            return GitRevisions("");
        }

        static public List<GitRevision> GitRevisions(string filter)
        {
            string tree;
            if (string.IsNullOrEmpty(filter))
                tree = RunCmd(Settings.GitDir + "git.exe", "rev-list --all --header --date-order");
            else
                tree = RunCmd(Settings.GitDir + "git.exe", "rev-list --header --topo-order " + filter);
            
            string[] itemsStrings = tree.Split('\n');

            List<GitRevision> revisions = new List<GitRevision>();

            for (int n = 0; n < itemsStrings.Count()-6;)
            {
                GitRevision revision = new GitRevision();
                revision.Guid = itemsStrings[n++].Trim('\0');
                revision.Name = revision.TreeGuid = itemsStrings[n++].Substring(4).Trim();
                while (itemsStrings[n].Contains("parent"))
                {
                    //Add parent
                    revision.ParentGuids.Add( itemsStrings[n++].Substring(6).Trim());
                }
                if (revision.ParentGuids.Count == 0)
                {
                    revision.ParentGuids.Add("0000000000000000000000000000000000000000");
                }
                revision.Author = itemsStrings[n++].Substring(6).Trim();
                revision.Committer = itemsStrings[n++].Substring(9).Trim();
                n++;

                while (itemsStrings.Count() > n + 1 &&
                    itemsStrings[n].Length > 0 &&
                    itemsStrings[n][0] == ' ')
                {
                    revision.Message += itemsStrings[n++].Trim() + "\n";
                }

                revisions.Add(revision);
            }

            return revisions;
        }

        static public List<GitHead> GetHeads()
        {
            return GetHeads(true);
        }

        static public string GetSelectedBranch()
        {
            string branches = RunCmd(Settings.GitDir + "git.exe", "branch");
            string[] branchStrings = branches.Split('\n');
            foreach (string branch in branchStrings)
            {
                if (branch.IndexOf('*') > -1)
                    return branch.Trim('*', ' ');
            }
            return "";
        }

        static public List<GitHead> GetHeads(bool tags)
        {
            string tree;
            if (tags)
                tree = RunCmd(Settings.GitDir + "git.exe", "show-ref --dereference");
            else
                tree = RunCmd(Settings.GitDir + "git.exe", "show-ref --dereference --heads");
            string[] itemsStrings = tree.Split('\n');

            List<GitHead> heads = new List<GitHead>();

            foreach (string itemsString in itemsStrings)
            {
                if (itemsString.Length > 42)
                {
                    GitHead head = new GitHead();
                    head.Guid = itemsString.Substring(0, 40);
                    head.Name = itemsString.Substring(41).Trim();
                    if (head.Name.Length > 0 && head.Name.LastIndexOf("/") >1)
                        head.Name = head.Name.Substring(head.Name.LastIndexOf("/") + 1);

                    heads.Add(head);
                }
            }

            return heads;
        }

        static public List<IGitItem> GetFileChanges(string file)
        {
            string tree = RunCmd(Settings.GitDir + "git.exe", "whatchanged --all --pretty=oneline " + file);

            string[] itemsStrings = tree.Split('\n');

            List<IGitItem> items = new List<IGitItem>();

            GitItem item = null;
            foreach (string itemsString in itemsStrings)
            {
                if (itemsString.Length > 43 && itemsString[0] != ':')
                {
                    item = new GitItem();

                    item.Guid = itemsString.Substring(0, 40);
                    item.Name = itemsString.Substring(41).Trim();

                    items.Add(item);
                }
                else
                {
                    if (item != null && itemsString.Length > 32)
                        item.Guid = itemsString.Substring(26, 7);
                        //item.Guid = itemsString.Substring(15, 7);
                }
            }

            return items;
        }


        static public List<IGitItem> GetTree(string id)
        {
            string tree = RunCmd(Settings.GitDir + "git.exe", "ls-tree " + id);

            string [] itemsStrings = tree.Split('\n');

            List<IGitItem> items = new List<IGitItem>();

            foreach (string itemsString in itemsStrings)
            {
                GitItem item = new GitItem();

                if (itemsString.Length > 53)
                {

                    item.Mode = itemsString.Substring(0, 6);
                    item.ItemType = itemsString.Substring(7, 4);
                    item.Guid = itemsString.Substring(12, 40);
                    item.Name = itemsString.Substring(53).Trim();
                    item.FileName = item.Name;

                    //if (item.ItemType == "tree")
                    //    item.SubItems = GetTree(item.Guid);

                    items.Add(item);
                }
            }

            return items;
        }

        public static string GetFileText(string id)
        {
            return RunCmd(Settings.GitDir + "git.exe", "cat-file blob " + id);
        }
    }
}
