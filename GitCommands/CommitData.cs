using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace GitCommands
{
    public class CommitData
    {
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
        public List<string> ChildrenGuids { get; set; }
        public string Author { get; private set; }
        public DateTimeOffset AuthorDate { get; private set; }
        public string Committer { get; private set; }
        public DateTimeOffset CommitDate { get; private set; }

        public string Body { get; private set; }

        /// <summary>
        /// Generate header.
        /// </summary>
        /// <returns></returns>
        public string GetHeader(bool showRevisionsAsLinks)
        {
            StringBuilder header = new StringBuilder();
            string authorEmail = GetEmail(Author);
            header.AppendLine(FillToLength(WebUtility.HtmlEncode(Strings.GetAuthorText()) + ":", COMMITHEADER_STRING_LENGTH) +
                "<a href='mailto:" + WebUtility.HtmlEncode(authorEmail) + "'>" + WebUtility.HtmlEncode(Author) + "</a>");
            header.AppendLine(FillToLength(WebUtility.HtmlEncode(Strings.GetAuthorDateText()) + ":", COMMITHEADER_STRING_LENGTH) +
                WebUtility.HtmlEncode(GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, AuthorDate.UtcDateTime) + " (" + GitCommandHelpers.GetFullDateString(AuthorDate) + ")"));
            string committerEmail = GetEmail(Committer);
            header.AppendLine(FillToLength(WebUtility.HtmlEncode(Strings.GetCommitterText()) + ":", COMMITHEADER_STRING_LENGTH) +
                "<a href='mailto:" + WebUtility.HtmlEncode(committerEmail) + "'>" + WebUtility.HtmlEncode(Committer) + "</a>");
            header.AppendLine(FillToLength(WebUtility.HtmlEncode(Strings.GetCommitDateText()) + ":", COMMITHEADER_STRING_LENGTH) +
                WebUtility.HtmlEncode(GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, CommitDate.UtcDateTime) + " (" + GitCommandHelpers.GetFullDateString(CommitDate) + ")"));
            header.Append(FillToLength(WebUtility.HtmlEncode(Strings.GetCommitHashText()) + ":", COMMITHEADER_STRING_LENGTH) +
                WebUtility.HtmlEncode(Guid));

            if (ChildrenGuids != null && ChildrenGuids.Count != 0)
            {
                header.AppendLine();
                string commitsString;
                if (showRevisionsAsLinks)
                    commitsString  = ChildrenGuids.Select(LinkFactory.CreateCommitLink).Join(" ");
                else
                    commitsString = ChildrenGuids.Select(guid => guid.Substring(0, 10)).Join(" ");
                header.Append(FillToLength(WebUtility.HtmlEncode(Strings.GetChildrenText()) + ":",
                                           COMMITHEADER_STRING_LENGTH) + commitsString);
            }

            var parentGuids = ParentGuids.Where(s => !string.IsNullOrEmpty(s));
            if (parentGuids.Any())
            {
                header.AppendLine();
                string commitsString;
                if (showRevisionsAsLinks)
                    commitsString = parentGuids.Select(LinkFactory.CreateCommitLink).Join(" ");
                else
                    commitsString = parentGuids.Select(guid => guid.Substring(0, 10)).Join(" ");
                header.Append(FillToLength(WebUtility.HtmlEncode(Strings.GetParentsText()) + ":",
                                           COMMITHEADER_STRING_LENGTH) + commitsString);
            }

            return RemoveRedundancies(header.ToString());
        }

        /// <summary>
        /// Returns an array of strings containg titles of fields field returned by GetHeader.
        /// Used to calculate layout in advance
        /// </summary>
        /// <returns></returns>
        public static string[] GetPossibleHeaders()
        {
            return new string[] {Strings.GetAuthorText(), Strings.GetAuthorDateText(), Strings.GetCommitterText(),
                                 Strings.GetCommitDateText(), Strings.GetCommitHashText(), Strings.GetChildrenText(),
                                 Strings.GetParentsText()};
        }

        /// <summary>
        /// Generate header.
        /// </summary>
        /// <returns></returns>
        public string GetHeaderPlain()
        {
            StringBuilder header = new StringBuilder();
            header.AppendLine(FillToLength(Strings.GetAuthorText() + ":", COMMITHEADER_STRING_LENGTH) + Author);
            header.AppendLine(FillToLength(Strings.GetAuthorDateText() + ":", COMMITHEADER_STRING_LENGTH) +
                GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, AuthorDate.UtcDateTime) + " (" + GitCommandHelpers.GetFullDateString(AuthorDate) + ")");
            header.AppendLine(FillToLength(Strings.GetCommitterText() + ":", COMMITHEADER_STRING_LENGTH) +
                Committer);
            header.AppendLine(FillToLength(Strings.GetCommitDateText() + ":", COMMITHEADER_STRING_LENGTH) +
                GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, CommitDate.UtcDateTime) + " (" + GitCommandHelpers.GetFullDateString(CommitDate) + ")");
            header.Append(FillToLength(Strings.GetCommitHashText() + ":", COMMITHEADER_STRING_LENGTH) +
                Guid);

            return RemoveRedundancies(header.ToString());
        }

        private static string GetEmail(string author)
        {
            if (String.IsNullOrEmpty(author))
                return "";
            int ind = author.IndexOf("<") + 1;
            if (ind == -1)
                return "";
            return author.Substring(ind, author.LastIndexOf(">") - ind);
        }

        /// <summary>
        /// Gets the commit info for submodule.
        /// </summary>
        public static void UpdateCommitMessage(CommitData data, GitModule module, string sha1, ref string error)
        {
            if (module == null)
                throw new ArgumentNullException("module");
            if (sha1 == null)
                throw new ArgumentNullException("sha1");

            //Do not cache this command, since notes can be added
            string arguments = string.Format(CultureInfo.InvariantCulture,
                    "log -1 --pretty=\"format:" + ShortLogFormat + "\" {0}", sha1);
            var info = module.RunGitCmd(arguments, GitModule.LosslessEncoding);

            if (info.Trim().StartsWith("fatal"))
            {
                error = "Cannot find commit " + sha1;
                return;
            }

            int index = info.IndexOf(sha1) + sha1.Length;

            if (index < 0)
            {
                error = "Cannot find commit " + sha1;
                return;
            }
            if (index >= info.Length)
            {
                error = info;
                return;
            }

            UpdateBodyInCommitData(data, info, module);
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
            var info = module.RunGitCmd(arguments, GitModule.LosslessEncoding);

            if (info.Trim().StartsWith("fatal"))
            {
                error = "Cannot find commit " + sha1;
                return null;
            }

            int index = info.IndexOf(sha1) + sha1.Length;

            if (index < 0)
            {
                error = "Cannot find commit " + sha1;
                return null;
            }
            if (index >= info.Length)
            {
                error = info;
                return null;
            }

            CommitData commitInformation = CreateFromFormatedData(info, module);

            return commitInformation;
        }

        public const string LogFormat = "%H%n%T%n%P%n%aN <%aE>%n%at%n%cN <%cE>%n%ct%n%e%n%B%nNotes:%n%-N";

        /// <summary>
        /// Creates a CommitData object from formated commit info data from git.  The string passed in should be
        /// exact output of a log or show command using --format=LogFormat.
        /// </summary>
        /// <param name="data">Formated commit data from git.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        public static CommitData CreateFromFormatedData(string data, GitModule aModule)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var lines = data.Split('\n');
            
            var guid = lines[0];

            // TODO: we can use this to add more relationship info like gitk does if wanted
            var treeGuid = lines[1];

            // TODO: we can use this to add more relationship info like gitk does if wanted
            string[] parentLines = lines[2].Split(new char[]{' '});
            ReadOnlyCollection<string> parentGuids = parentLines.ToList().AsReadOnly();

            var author = aModule.ReEncodeStringFromLossless(lines[3]);
            var authorDate = DateTimeUtils.ParseUnixTime(lines[4]);

            var committer = aModule.ReEncodeStringFromLossless(lines[5]);
            var commitDate = DateTimeUtils.ParseUnixTime(lines[6]);

            string commitEncoding = lines[7];

            const int startIndex = 8;
            string message = ProccessDiffNotes(startIndex, lines);

            //commit message is not reencoded by git when format is given
            var body = aModule.ReEncodeCommitMessage(message, commitEncoding);

            var commitInformation = new CommitData(guid, treeGuid, parentGuids, author, authorDate,
                committer, commitDate, body);

            return commitInformation;
        }

        public const string ShortLogFormat = "%H%n%e%n%B%nNotes:%n%-N";

        /// <summary>
        /// Creates a CommitData object from formated commit info data from git.  The string passed in should be
        /// exact output of a log or show command using --format=LogFormat.
        /// </summary>
        /// <param name="data">Formated commit data from git.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        public static void UpdateBodyInCommitData(CommitData commitData, string data, GitModule aModule)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var lines = data.Split('\n');

            var guid = lines[0];

            string commitEncoding = lines[1];

            const int startIndex = 2;
            string message = ProccessDiffNotes(startIndex, lines);

            //commit message is not reencoded by git when format is given
            Debug.Assert(commitData.Guid == guid);
            commitData.Body = aModule.ReEncodeCommitMessage(message, commitEncoding);
        }

        private static string ProccessDiffNotes(int startIndex, string[] lines)
        {
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

            return message.ToString();
        }

        /// <summary>
        /// Creates a CommitData object from Git revision.
        /// </summary>
        /// <param name="revision">Git commit.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        public static CommitData CreateFromRevision(GitRevision revision)
        {
            if (revision == null)
                throw new ArgumentNullException("revision");

            CommitData data = new CommitData(revision.Guid, revision.TreeGuid, revision.ParentGuids.ToList().AsReadOnly(),
                String.Format("{0} <{1}>", revision.Author, revision.AuthorEmail), revision.AuthorDate,
                String.Format("{0} <{1}>", revision.Committer, revision.CommitterEmail), revision.CommitDate,
                revision.Body ?? revision.Message);
            return data;
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
