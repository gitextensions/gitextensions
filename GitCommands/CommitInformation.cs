using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Web;

namespace GitCommands
{
    public class CommitInformation
    {
        private const string COMMIT_LABEL = "commit ";
        private const string TREE_LABEL = "tree ";
        private const string PARENT_LABEL = "parent ";
        private const string AUTHOR_LABEL = "author ";
        private const string COMMITTER_LABEL = "committer ";
        private const int COMMITHEADER_STRING_LENGTH = 16;

        /// <summary>
        /// Private constructor
        /// </summary>
        private CommitInformation(string header, string body)
        {
            Header = header;
            Body = body;
        }

        public string Header {get; private set;}
        public string Body{get; private set;}

        /// <summary>
        /// Gets branches which contain the given commit.
        /// If both local and remote branches are requested, remote branches are prefixed with "remotes/"
        /// (as returned by git branch -a)
        /// </summary>
        /// <param name="sha1">The sha1.</param>
        /// <param name="getLocal">Pass true to include local branches</param>
        /// <param name="getLocal">Pass true to include remote branches</param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllBranchesWhichContainGivenCommit(string sha1, bool getLocal, bool getRemote) 
        {
            string args = "--contains " + sha1;
            if (getRemote && getLocal)
                args = "-a "+args;
            else if (getRemote)
                args = "-r "+args;
            else if (!getLocal)
                return new string[]{};
            string info = Settings.Module.RunGitCmd("branch " + args, Settings.SystemEncoding);
            if (info.Trim().StartsWith("fatal") || info.Trim().StartsWith("error:"))
                return new List<string>();

            string[] result = info.Split(new[] { '\r', '\n', '*' }, StringSplitOptions.RemoveEmptyEntries);

            // Remove symlink targets as in "origin/HEAD -> origin/master"
            for (int i = 0; i < result.Length; i++)
            {
                string item = result[i].Trim();
                int idx;
                if (getRemote && ((idx = item.IndexOf(" ->")) >= 0))
                {
                    item = item.Substring(0, idx);
                }
                result[i] = item;
            }

            return result;
        }

        /// <summary>
        /// Gets all tags which contain the given commit.
        /// </summary>
        /// <param name="sha1">The sha1.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllTagsWhichContainGivenCommit(string sha1)
        {
            string info = Settings.Module.RunGitCmd("tag --contains " + sha1, Settings.SystemEncoding);


            if (info.Trim().StartsWith("fatal") || info.Trim().StartsWith("error:"))
                return new List<string>();
            return info.Split(new[] { '\r', '\n', '*', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Gets the commit info.
        /// </summary>
        /// <param name="sha1">The sha1.</param>
        /// <returns></returns>
        public static CommitInformation GetCommitInfo(string sha1)
        {
            return GetCommitInfo(Settings.Module, sha1);
        }

        /// <summary>
        /// Gets the commit info for submodule.
        /// </summary>
        /// <param name="module">Git module.</param>
        /// <param name="sha1">The sha1.</param>
        /// <returns></returns>
        public static CommitInformation GetCommitInfo(GitModule module, string sha1)
        {
            string error = "";
            CommitData data = CommitData.GetCommitData(module, sha1, ref error);
            if (data == null)
                return new CommitInformation(error, "");

            string header = data.GetHeader();
            string body = "\n\n" + HttpUtility.HtmlEncode(data.Body.Trim()) + "\n\n";

            return new CommitInformation(header, body);
        }

        /// <summary>
        /// Gets the commit info from CommitData.
        /// </summary>
        /// <returns></returns>
        public static CommitInformation GetCommitInfo(CommitData data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            string header = data.GetHeader();
            string body = "\n\n" + HttpUtility.HtmlEncode(data.Body.Trim()) + "\n\n";

            return new CommitInformation(header, body);
        }
    }
}