using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.IO;
using PatchApply;

namespace GitCommands
{
    public class GitCommands
    {
        public static string FindGitWorkingDir(string startDir)
        {
            string dir = startDir + "\\";

            while (dir.LastIndexOf('\\') > 0)
            {
                dir = dir.Substring(0, dir.LastIndexOf('\\'));

                if (Directory.Exists(dir + "\\" + ".git"))
                    return dir + "\\";
            }
            return startDir;
        }

        public static string RunCmd(string cmd)
        {
            return RunCmd(cmd, "");
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static string RunCmd(string cmd, string arguments)
        {
            //process used to execute external commands

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = cmd;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = Settings.WorkingDir;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            process.StartInfo.LoadUserProfile = true;

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            // Read the output stream first and then wait. 


            return output;
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
            patchManager.LoadPatch(GitCommands.RunCmd(Settings.GitDir + "git.exe", "diff " + from + " " + to + " " + filter), false);

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

        static public List<GitRevision> GitRevisions()
        {
            string tree = RunCmd(Settings.GitDir + "git.exe", "rev-list --all --header --date-order");
            
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
                    revision.parentGuid = itemsStrings[n++].Substring(6).Trim();
                }
                if (string.IsNullOrEmpty(revision.parentGuid))
                {
                    revision.parentGuid = "0000000000000000000000000000000000000000";
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
            string tree = RunCmd(Settings.GitDir + "git.exe", "show-ref --dereference");
            string[] itemsStrings = tree.Split('\n');

            List<GitHead> heads = new List<GitHead>();

            foreach (string itemsString in itemsStrings)
            {
                if (itemsString.Length > 42)
                {
                    GitHead head = new GitHead();
                    head.Guid = itemsString.Substring(0, 40);
                    head.Name = itemsString.Substring(41).Trim();

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
