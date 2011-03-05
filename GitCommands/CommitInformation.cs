using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
        private CommitInformation (string header, string body)
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
            string info = GitCommandHelpers.RunCmd(Settings.GitCommand, "branch "+args);
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
            string info = GitCommandHelpers.RunCmd(Settings.GitCommand, "tag --contains " + sha1);


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
            //Do not cache this command, since notes can be added
            string info = GitCommandHelpers.RunCmd(
                Settings.GitCommand,
                string.Format(
                    "show -s --pretty=raw --show-notes=* {0}", sha1));

            if (info.Trim().StartsWith("fatal"))
                return new CommitInformation("Cannot find commit" + sha1, "");

            info = RemoveRedundancies(info);

            int index = info.IndexOf(sha1) + sha1.Length;

            if (index < 0)
                return new CommitInformation("Cannot find commit" + sha1, "");
            if (index >= info.Length)
                return new CommitInformation(info, "");

            CommitInformation commitInformation = CreateFromRawData(info);

            return commitInformation;
        }

        /// <summary>
        /// Creates a CommitInformation object from raw commit info data from git.  The string passed in should be
        /// exact output of a log or show command using --format=raw.
        /// </summary>
        /// <param name="rawData">Raw commit data from git.</param>
        /// <returns>CommitInformation object populated with parsed info from git string.</returns>
        public static CommitInformation CreateFromRawData(string rawData)
        {
            var lines = new List<string>(rawData.Split('\n'));

            var commit = lines.Single(l => l.StartsWith(COMMIT_LABEL));
            var guid = commit.Substring(COMMIT_LABEL.Length);
            lines.Remove(commit);

            // TODO: we can use this to add more relationship info like gitk does if wanted
            var tree = lines.Single(l => l.StartsWith(TREE_LABEL));
            var treeGuid = tree.Substring(TREE_LABEL.Length);
            lines.Remove(tree);

            // TODO: we can use this to add more relationship info like gitk does if wanted
            List<string> parentLines = lines.FindAll(l => l.StartsWith(PARENT_LABEL));
            var parentGuids = parentLines.Select(parent => parent.Substring(PARENT_LABEL.Length)).ToArray();
            lines.RemoveAll(parentLines.Contains);

            var authorInfo = lines.Single(l => l.StartsWith(AUTHOR_LABEL));
            var author = GetPersonFromAuthorInfoLine(authorInfo, AUTHOR_LABEL.Length);
            var authorDate = GetTimeFromAuthorInfoLine(authorInfo);
            lines.Remove(authorInfo);

            var committerInfo = lines.Single(l => l.StartsWith(COMMITTER_LABEL));
            var committer = GetPersonFromAuthorInfoLine(committerInfo, COMMITTER_LABEL.Length);
            var commitDate = GetTimeFromAuthorInfoLine(committerInfo);
            lines.Remove(committerInfo);

            var message = new StringBuilder();
            foreach (var line in lines)
                message.AppendFormat("{0}\n", line);

            var body = "\n\n" + message.ToString().TrimStart().TrimEnd() + "\n\n";

            //We need to recode the commit message because of a bug in Git.
            //We cannot let git recode the message to Settings.Encoding which is
            //needed to allow the "git log" to print the filename in Settings.Encoding
            Encoding logoutputEncoding = GitCommandHelpers.GetLogoutputEncoding();
            if (logoutputEncoding != Settings.Encoding)
                body = logoutputEncoding.GetString(Settings.Encoding.GetBytes(body));

            var header = FillToLenght(Strings.GetAuthorText() + ":", COMMITHEADER_STRING_LENGTH) + author + "\n" +
                         FillToLenght(Strings.GetAuthorDateText() + ":", COMMITHEADER_STRING_LENGTH) + GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, authorDate.UtcDateTime) + " (" + authorDate.LocalDateTime.ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")\n" +
                         FillToLenght(Strings.GetCommitterText() + ":", COMMITHEADER_STRING_LENGTH) + committer + "\n" +
                         FillToLenght(Strings.GetCommitterDateText() + ":", COMMITHEADER_STRING_LENGTH) + GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, commitDate.UtcDateTime) + " (" + commitDate.LocalDateTime.ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")\n" +
                         FillToLenght(Strings.GetCommitHashText() + ":", COMMITHEADER_STRING_LENGTH) + guid;

            header = RemoveRedundancies(header);

            var commitInformation = new CommitInformation(header, body);

            return commitInformation;
        }

        private static string FillToLenght(string input, int length)
        {
            const int tabsize = 8;
            if (input.Length < length)
                return input + new string('\t', ((length - input.Length) / tabsize) + (((length - input.Length) % tabsize) == 0 ? 0 : 1));

            return input;
        }

        private static string GetPersonFromAuthorInfoLine(string authorInfo, int labelLength)
        {
            int offsetIndex = authorInfo.LastIndexOf(' ');
            int timeIndex = authorInfo.LastIndexOf(' ', offsetIndex - 1);

            return authorInfo.Substring(labelLength, timeIndex - labelLength);
        }

        private static DateTimeOffset GetTimeFromAuthorInfoLine(string authorInfo)
        {
            var offsetIndex = authorInfo.LastIndexOf(' ');
            var timeIndex = authorInfo.LastIndexOf(' ', offsetIndex - 1);
            
            var unixTime = long.Parse(authorInfo.Substring(timeIndex + 1, offsetIndex - (timeIndex + 1)));
            var time = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(unixTime);

            return new DateTimeOffset(time, new TimeSpan(0));
        }

        private static string RemoveRedundancies(string info)
        {
            string author = GetField(info, Strings.GetAuthorText() + ":");
            string committer = GetField(info, Strings.GetCommitterText() + ":");

            if (String.Equals(author, committer, StringComparison.CurrentCulture))
            {
                info = RemoveField(info, Strings.GetCommitterText() + ":");
            }

            string authorDate = GetField(info, Strings.GetAuthorDateText() + ":");
            string commitDate = GetField(info, Strings.GetCommitterDateText() + ":");

            if (String.Equals(authorDate, commitDate, StringComparison.CurrentCulture))
            {
                info =
                    RemoveField(info, Strings.GetCommitterDateText() + ":").Replace(
                        FillToLenght(Strings.GetAuthorDateText() + ":", COMMITHEADER_STRING_LENGTH), FillToLenght(Strings.GetDateText() + ":", COMMITHEADER_STRING_LENGTH));
            }

            return info;
        }

        private static string RemoveField(string data, string header)
        {
            int headerIndex = data.IndexOf(header);

            if (headerIndex == -1)
                return data;

            int endIndex = data.IndexOf('\n', headerIndex);

            if (endIndex == -1)
                endIndex = data.Length - 1;

            int length = endIndex - headerIndex + 1;

            return data.Remove(headerIndex, length);
        }

        private static string GetField(string data, string header)
        {
            int valueIndex = IndexOfValue(data, header);

            if (valueIndex == -1)
                return null;

            int length = LengthOfValue(data, valueIndex);
            return data.Substring(valueIndex, length);
        }

        private static int LengthOfValue(string data, int valueIndex)
        {
            if (valueIndex == -1)
                return 0;

            int endIndex = data.IndexOf('\n', valueIndex);

            if (endIndex == -1)
                endIndex = data.Length - 1;

            return endIndex - valueIndex;
        }

        private static int IndexOfValue(string data, string header)
        {
            int headerIndex = data.IndexOf(header);

            if (headerIndex == -1)
                return -1;

            int valueIndex = headerIndex + header.Length;

            while (data[valueIndex] == '\t')
            {
                valueIndex++;

                if (valueIndex == data.Length)
                    return -1;
            }

            return valueIndex;
        }
    }
}