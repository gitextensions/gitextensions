using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

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
            StringBuilder header = new StringBuilder();
            string authorEmail = GetEmail(Author);
            header.AppendLine(FillToLength(HttpUtility.HtmlEncode(Strings.GetAuthorText()) + ":", COMMITHEADER_STRING_LENGTH) +
                "<a href='mailto:" + authorEmail  + "'>" + HttpUtility.HtmlEncode(Author) + "</a>");
            header.AppendLine(FillToLength(HttpUtility.HtmlEncode(Strings.GetAuthorDateText()) + ":", COMMITHEADER_STRING_LENGTH) +
                HttpUtility.HtmlEncode(GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, AuthorDate.UtcDateTime) + " (" + AuthorDate.LocalDateTime.ToString("ddd MMM dd HH':'mm':'ss yyyy")) + ")");
            string committerEmail = GetEmail(Committer);
            header.AppendLine(FillToLength(HttpUtility.HtmlEncode(Strings.GetCommitterText()) + ":", COMMITHEADER_STRING_LENGTH) +
                "<a href='mailto:" + committerEmail + "'>" + HttpUtility.HtmlEncode(Committer) + "</a>");
            header.AppendLine(FillToLength(HttpUtility.HtmlEncode(Strings.GetCommitDateText()) + ":", COMMITHEADER_STRING_LENGTH) + 
                HttpUtility.HtmlEncode(GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, CommitDate.UtcDateTime) + " (" + CommitDate.LocalDateTime.ToString("ddd MMM dd HH':'mm':'ss yyyy")) + ")");
            header.Append(FillToLength(HttpUtility.HtmlEncode(Strings.GetCommitHashText()) + ":", COMMITHEADER_STRING_LENGTH) + 
                HttpUtility.HtmlEncode(Guid));

            return RemoveRedundancies(header.ToString());
        }

        private string GetEmail(string author)
        {
            if (String.IsNullOrEmpty(Author))
                return "";
            int ind = Author.IndexOf("<") + 1;
            if (ind == -1)
                return "";
            return Author.Substring(ind, Author.LastIndexOf(">") - ind);
        }

        /// <summary>
        /// Gets the commit info.
        /// </summary>
        public static CommitData GetCommitData(string sha1, ref string error)
        {
            return GetCommitData(Settings.Module, sha1, ref error);
        }

        /// <summary>
        /// Gets the commit info for submodule.
        /// </summary>
        public static CommitData GetCommitData(GitModule module, string sha1, ref string error)
        {
            if (module == null)
                throw new ArgumentNullException("module");
            if (sha1 == null)
                throw new ArgumentNullException("sha1");

            //Do not cache this command, since notes can be added
            string arguments = string.Format(CultureInfo.InvariantCulture,
                    "log -1 --pretty=\"format:"+LogFormat+"\" {0}", sha1);
            var info =
                module.RunCmd(
                    Settings.GitCommand,
                    arguments,
                    Settings.LosslessEncoding
                    );

            if (info.Trim().StartsWith("fatal"))
            {
                error = "Cannot find commit" + sha1;
                return null;
            }

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

            CommitData commitInformation = CreateFromFormatedData(info);

            return commitInformation;
        }

        public static readonly string LogFormat = "%H%n%T%n%P%n%aN <%aE>%n%at%n%cN <%cE>%n%ct%n%e%n%B%nNotes:%n%-N";

        /// <summary>
        /// Creates a CommitData object from formated commit info data from git.  The string passed in should be
        /// exact output of a log or show command using --format=LogFormat.
        /// </summary>
        /// <param name="data">Formated commit data from git.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        public static CommitData CreateFromFormatedData(string data)
        {
            if (data == null)
                throw new ArgumentNullException("Data");

            var lines = data.Split('\n');
            
            var guid = lines[0];

            // TODO: we can use this to add more relationship info like gitk does if wanted
            var treeGuid = lines[1];

            // TODO: we can use this to add more relationship info like gitk does if wanted
            string[] parentLines = lines[2].Split(new char[]{' '});
            ReadOnlyCollection<string> parentGuids = parentLines.ToList().AsReadOnly();

            var author = GitCommandHelpers.ReEncodeStringFromLossless(lines[3]);
            var authorDate = GetTimeFromUtcTimeLine(lines[4]);

            var committer = GitCommandHelpers.ReEncodeStringFromLossless(lines[5]);
            var commitDate = GetTimeFromUtcTimeLine(lines[6]);

            string commitEncoding = lines[7];

            int startIndex = 8;
            int endIndex = lines.Length - 1;
            if (lines[endIndex] == "Notes:")
                endIndex--;

            var message = new StringBuilder();
            bool bNotesStart = false;
            for (int i = startIndex; i <= endIndex; i++)
            {
                string line = lines[i];
                if (bNotesStart)
                    line = "    " + line;
                message.AppendLine(line);
                if (lines[i] == "Notes:")
                    bNotesStart = true;
            }

            //commit message is not reencoded by git when format is given
            var body = GitCommandHelpers.ReEncodeCommitMessage(message.ToString(), commitEncoding);

            var commitInformation = new CommitData(guid, treeGuid, parentGuids, author, authorDate,
                committer, commitDate, body);

            return commitInformation;
        }

        private static string FillToLength(string input, int length)
        {
            return FillToLength(input, length, 0);
        }

        private static string FillToLength(string input, int length, int skip)
        {
            // length
            const int tabsize = 8;
            if ((input.Length - skip) < length)
            {
                int l = length - (input.Length - skip);
                return input + new string('\t', (l / tabsize) + ((l % tabsize) == 0 ? 0 : 1));
            }

            return input;
        }

        private static DateTimeOffset GetTimeFromUtcTimeLine(string data)
        {
            var unixTime = long.Parse(data);
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
            string commitDate = GetField(info, Strings.GetCommitDateText() + ":");

            if (String.Equals(authorDate, commitDate, StringComparison.CurrentCulture))
            {
                info =
                    RemoveField(info, Strings.GetCommitDateText() + ":").Replace(
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