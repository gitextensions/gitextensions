using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Net;
using LibGit2Sharp;

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
        public string GetHeader()
        {
            StringBuilder header = new StringBuilder();
            string authorEmail = GetEmail(Author);
            header.AppendLine(FillToLength(WebUtility.HtmlEncode(Strings.GetAuthorText()) + ":", COMMITHEADER_STRING_LENGTH) +
                "<a href='mailto:" + WebUtility.HtmlEncode(authorEmail) + "'>" + WebUtility.HtmlEncode(Author) + "</a>");
            header.AppendLine(FillToLength(WebUtility.HtmlEncode(Strings.GetAuthorDateText()) + ":", COMMITHEADER_STRING_LENGTH) +
                WebUtility.HtmlEncode(GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, AuthorDate.UtcDateTime) + " (" + AuthorDate.LocalDateTime.ToString("ddd MMM dd HH':'mm':'ss yyyy")) + ")");
            string committerEmail = GetEmail(Committer);
            header.AppendLine(FillToLength(WebUtility.HtmlEncode(Strings.GetCommitterText()) + ":", COMMITHEADER_STRING_LENGTH) +
                "<a href='mailto:" + WebUtility.HtmlEncode(committerEmail) + "'>" + WebUtility.HtmlEncode(Committer) + "</a>");
            header.AppendLine(FillToLength(WebUtility.HtmlEncode(Strings.GetCommitDateText()) + ":", COMMITHEADER_STRING_LENGTH) +
                WebUtility.HtmlEncode(GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, CommitDate.UtcDateTime) + " (" + CommitDate.LocalDateTime.ToString("ddd MMM dd HH':'mm':'ss yyyy")) + ")");
            header.Append(FillToLength(WebUtility.HtmlEncode(Strings.GetCommitHashText()) + ":", COMMITHEADER_STRING_LENGTH) +
                WebUtility.HtmlEncode(Guid));

            if (ChildrenGuids != null && ChildrenGuids.Count != 0)
            {
                header.AppendLine();
                var commitsString = ChildrenGuids.Select(LinkFactory.CreateCommitLink).Join(" ");
                header.Append(FillToLength(WebUtility.HtmlEncode(Strings.GetChildrenText()) + ":",
                                           COMMITHEADER_STRING_LENGTH) + commitsString);
            }

            var parentGuids = ParentGuids.Where(s => !string.IsNullOrEmpty(s));
            if (parentGuids.Any())
            {
                header.AppendLine();
                var commitsString = parentGuids.Select(LinkFactory.CreateCommitLink).Join(" ");
                header.Append(FillToLength(WebUtility.HtmlEncode(Strings.GetParentsText()) + ":",
                                           COMMITHEADER_STRING_LENGTH) + commitsString);
            }

            return RemoveRedundancies(header.ToString());
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
                GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, AuthorDate.UtcDateTime) + " (" + AuthorDate.LocalDateTime.ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")");
            header.AppendLine(FillToLength(Strings.GetCommitterText() + ":", COMMITHEADER_STRING_LENGTH) +
                Committer);
            header.AppendLine(FillToLength(Strings.GetCommitDateText() + ":", COMMITHEADER_STRING_LENGTH) +
                GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, CommitDate.UtcDateTime) + " (" + CommitDate.LocalDateTime.ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")");
            header.Append(FillToLength(Strings.GetCommitHashText() + ":", COMMITHEADER_STRING_LENGTH) +
                Guid);

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
        /// Gets the commit info for submodule.
        /// </summary>
        public static CommitData GetCommitData(GitModule module, string sha1, ref string error)
        {
            error = "";
            if (module == null)
                throw new ArgumentNullException("module");
            if (sha1 == null)
                throw new ArgumentNullException("sha1");

            var commit = module.Repository.Lookup<LibGit2Sharp.Commit>(sha1);
            if (commit == null)
            {
                error = "Cannot find commit " + sha1;
                return null;
            }
            return CreateFromCommit(commit);
        }

        /// <summary>
        /// Creates a CommitData object from formated commit info data from git.
        /// </summary>
        /// <param name="commit">Commit object from libgit2sharp.</param>
        /// <returns>CommitData object populated with parsed info from commit object.</returns>
        public static CommitData CreateFromCommit(LibGit2Sharp.Commit commit)
        {
            if (commit == null)
                throw new ArgumentNullException("commit");

            var author = string.Format("{0} <{1}>", commit.Author.Name, commit.Author.Email);
            var authorDate = commit.Author.When;

            var committer = string.Format("{0} <{1}>", commit.Committer.Name, commit.Committer.Email); ;
            var commitDate = commit.Committer.When;

            var body = commit.Message;

            var parents = commit.Parents.Select(p => p.Sha).ToList().AsReadOnly();
            var commitInformation = new CommitData(commit.Sha, commit.Tree.Sha, parents, author, authorDate, committer, commitDate, body);

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
