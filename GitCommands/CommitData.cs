using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace GitCommands
{
    public class CommitData
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
        private CommitData(string guid,
            string treeGuid, ReadOnlyCollection<string> parentGuids, 
            string author, DateTimeOffset authorDate,
            string committer, DateTimeOffset commitDate,
            string body)
        {
            Guid = guid;
            TreeGuid = treeGuid;
            ParentGuids = parentGuids;
            Author = author;
            AuthorDate = authorDate;
            Committer = committer;
            CommitDate = commitDate;

            Body = body;
        }

        public string Guid { get; private set; }
        public string TreeGuid { get; private set; }
        public ReadOnlyCollection<string> ParentGuids { get; private set; }
        public string Author { get; private set; }
        public DateTimeOffset AuthorDate { get; private set; }
        public string Committer { get; private set; }
        public DateTimeOffset CommitDate { get; private set; }

        public string Body{get; private set;}

        /// <summary>
        /// Generate header.
        /// </summary>
        /// <returns></returns>
        public string GetHeader()
        {
            string header = FillToLength(Strings.GetAuthorText() + ":", COMMITHEADER_STRING_LENGTH) + Author + "\n" +
                 FillToLength(Strings.GetAuthorDateText() + ":", COMMITHEADER_STRING_LENGTH) + GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, AuthorDate.UtcDateTime) + " (" + AuthorDate.LocalDateTime.ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")\n" +
                 FillToLength(Strings.GetCommitterText() + ":", COMMITHEADER_STRING_LENGTH) + Committer + "\n" +
                 FillToLength(Strings.GetCommitterDateText() + ":", COMMITHEADER_STRING_LENGTH) + GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, CommitDate.UtcDateTime) + " (" + CommitDate.LocalDateTime.ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")\n" +
                 FillToLength(Strings.GetCommitHashText() + ":", COMMITHEADER_STRING_LENGTH) + Guid;

            return RemoveRedundancies(header);
        }

        /// <summary>
        /// Gets the commit info.
        /// </summary>
        /// <param name="sha1">The sha1.</param>
        /// <returns></returns>
        public static CommitData GetCommitData(string sha1, ref string error)
        {
            return GetCommitData(Settings.Module, sha1, ref error);
        }

        /// <summary>
        /// Gets the commit info for submodule.
        /// </summary>
        /// <param name="sha1">The sha1.</param>
        /// <returns></returns>
        public static CommitData GetCommitData(GitModule module, string sha1, ref string error)
        {
            if (module == null)
                throw new ArgumentNullException("module");
            if (sha1 == null)
                throw new ArgumentNullException("sha1");

            //Do not cache this command, since notes can be added
            string info = module.RunGitCmd(
                string.Format(
                    "log -1 --pretty=raw --show-notes=* {0}", sha1));

            if (info.Trim().StartsWith("fatal"))
            {
                error = "Cannot find commit" + sha1;
                return null;
            }

            info = RemoveRedundancies(info);

            int index = info.IndexOf(sha1) + sha1.Length;

            if (index < 0)
            {
                error = "Cannot find commit" + sha1;
                return null;
            }
            if (index >= info.Length)
            {
                error = info;
                return null;
            }

            CommitData commitInformation = CreateFromRawData(info);

            return commitInformation;
        }

        /// <summary>
        /// Creates a CommitData object from raw commit info data from git.  The string passed in should be
        /// exact output of a log or show command using --format=raw.
        /// </summary>
        /// <param name="rawData">Raw commit data from git.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        public static CommitData CreateFromRawData(string rawData)
        {
            if (rawData == null)
                throw new ArgumentNullException("rawData");

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
            ReadOnlyCollection<string> parentGuids = parentLines.Select(parent => parent.Substring(PARENT_LABEL.Length)).ToList().AsReadOnly();
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
                message.AppendLine(line);

            var body = message.ToString();

            //We need to recode the commit message because of a bug in Git.
            //We cannot let git recode the message to Settings.Encoding which is
            //needed to allow the "git log" to print the filename in Settings.Encoding
            Encoding logoutputEncoding = Settings.Module.GetLogoutputEncoding();
            if (logoutputEncoding != Settings.Encoding)
                body = logoutputEncoding.GetString(Settings.Encoding.GetBytes(body));

            var commitInformation = new CommitData(guid, treeGuid, parentGuids, author, authorDate,
                committer, commitDate, body);

            return commitInformation;
        }

        private static string FillToLength(string input, int length)
        {
            // length
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
                        FillToLength(Strings.GetAuthorDateText() + ":", COMMITHEADER_STRING_LENGTH), FillToLength(Strings.GetDateText() + ":", COMMITHEADER_STRING_LENGTH));
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