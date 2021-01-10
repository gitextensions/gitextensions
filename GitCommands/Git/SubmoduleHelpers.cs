using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitCommands.Patches;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    public static class SubmoduleHelpers
    {
        [CanBeNull]
        public static async Task<GitSubmoduleStatus> GetCurrentSubmoduleChangesAsync(GitModule module, string fileName, string oldFileName, bool staged, bool noLocks = false)
        {
            Patch patch = await module.GetCurrentChangesAsync(fileName, oldFileName, staged, "", noLocks: noLocks).ConfigureAwait(false);
            string text = patch is not null ? patch.Text : "";
            return ParseSubmoduleStatus(text, module, fileName);
        }

        [CanBeNull]
        public static Task<GitSubmoduleStatus> GetCurrentSubmoduleChangesAsync(GitModule module, string submodule, bool noLocks = false)
        {
            return GetCurrentSubmoduleChangesAsync(module, submodule, submodule, false, noLocks: noLocks);
        }

        public static GitSubmoduleStatus ParseSubmoduleStatus(string text, GitModule module, string fileName)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            string name = null;
            string oldName = null;
            bool isDirty = false;
            ObjectId commitId = null;
            ObjectId oldCommitId = null;
            int? addedCommits = null;
            int? removedCommits = null;

            using (var reader = new StringReader(text))
            {
                string line = reader.ReadLine();

                if (line is not null)
                {
                    var match = Regex.Match(line, @"diff --git [abic]/(.+)\s[abwi]/(.+)");
                    if (match.Groups.Count > 1)
                    {
                        name = match.Groups[1].Value;
                        oldName = match.Groups[2].Value;
                    }
                    else
                    {
                        match = Regex.Match(line, @"diff --cc (.+)");
                        if (match.Groups.Count > 1)
                        {
                            name = match.Groups[1].Value;
                            oldName = match.Groups[1].Value;
                        }
                    }
                }

                while ((line = reader.ReadLine()) is not null)
                {
                    // We are looking for lines resembling:
                    //
                    // -Subproject commit bfef4454fc51e345051ee5bf66686dc28deed627
                    // +Subproject commit 8b20498b954609770205c2cc794b868b4ac3ee69-dirty

                    if (!line.Contains("Subproject"))
                    {
                        continue;
                    }

                    char c = line[0];
                    const string commitStr = "commit ";
                    string hash = "";
                    int pos = line.IndexOf(commitStr);
                    if (pos >= 0)
                    {
                        hash = line.Substring(pos + commitStr.Length);
                    }

                    bool endsWithDirty = hash.EndsWith("-dirty");
                    hash = hash.Replace("-dirty", "");
                    if (c == '-')
                    {
                        oldCommitId = ObjectId.Parse(hash);
                    }
                    else if (c == '+')
                    {
                        commitId = ObjectId.Parse(hash);
                        isDirty = endsWithDirty;
                    }

                    // TODO: Support combined merge
                }
            }

            if (oldCommitId is not null && commitId is not null)
            {
                if (oldCommitId == commitId)
                {
                    addedCommits = 0;
                    removedCommits = 0;
                }
                else
                {
                    var submodule = module.GetSubmodule(fileName);
                    addedCommits = submodule.GetCommitCount(commitId.ToString(), oldCommitId.ToString());
                    removedCommits = submodule.GetCommitCount(oldCommitId.ToString(), commitId.ToString());
                }
            }

            return new GitSubmoduleStatus(name, oldName, isDirty, commitId, oldCommitId, addedCommits, removedCommits);
        }
    }
}
