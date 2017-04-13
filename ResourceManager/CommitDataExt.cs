using System;
using System.Linq;
using System.Net;
using System.Text;
using GitCommands;
using GitCommands.Git;

namespace ResourceManager
{
    public static class CommitDataExt
    {
        private const int COMMITHEADER_STRING_LENGTH = 16;

        private static string GetEmail(string author)
        {
            if (String.IsNullOrEmpty(author))
                return "";
            int ind = author.IndexOf("<");
            if (ind == -1)
                return "";
            ++ind;
            return author.Substring(ind, author.LastIndexOf(">") - ind);
        }

        /// <summary>
        /// Generate header.
        /// </summary>
        /// <param name="commitData"></param>
        /// <returns></returns>
        public static string GetHeaderPlain(this CommitData commitData)
        {
            StringBuilder header = new StringBuilder();
            header.AppendLine(FillToLength(Strings.GetAuthorText() + ":", COMMITHEADER_STRING_LENGTH) +
                              commitData.Author);
            header.AppendLine(FillToLength(Strings.GetAuthorDateText() + ":", COMMITHEADER_STRING_LENGTH) +
                              LocalizationHelpers.GetRelativeDateString(DateTime.UtcNow, commitData.AuthorDate.UtcDateTime) +
                              " (" + LocalizationHelpers.GetFullDateString(commitData.AuthorDate) + ")");
            header.AppendLine(FillToLength(Strings.GetCommitterText() + ":", COMMITHEADER_STRING_LENGTH) +
                              commitData.Committer);
            header.AppendLine(FillToLength(Strings.GetCommitDateText() + ":", COMMITHEADER_STRING_LENGTH) +
                              LocalizationHelpers.GetRelativeDateString(DateTime.UtcNow, commitData.CommitDate.UtcDateTime) +
                              " (" + LocalizationHelpers.GetFullDateString(commitData.CommitDate) + ")");
            header.Append(FillToLength(Strings.GetCommitHashText() + ":", COMMITHEADER_STRING_LENGTH) +
                          commitData.Guid);

            return RemoveRedundancies(header.ToString());
        }

        /// <summary>
        /// Generate header.
        /// </summary>
        /// <returns></returns>
        public static string GetHeader(this CommitData commitData, IGitRevisionProvider module, bool showRevisionsAsLinks)
        {
            StringBuilder header = new StringBuilder();
            string authorEmail = GetEmail(commitData.Author);
            header.AppendLine(
                FillToLength(WebUtility.HtmlEncode(Strings.GetAuthorText()) + ":", COMMITHEADER_STRING_LENGTH) +
                "<a href='mailto:" + WebUtility.HtmlEncode(authorEmail) + "'>" +
                WebUtility.HtmlEncode(commitData.Author) + "</a>");
            header.AppendLine(
                FillToLength(WebUtility.HtmlEncode(Strings.GetAuthorDateText()) + ":",
                    COMMITHEADER_STRING_LENGTH) +
                WebUtility.HtmlEncode(
                    LocalizationHelpers.GetRelativeDateString(DateTime.UtcNow, commitData.AuthorDate.UtcDateTime) + " (" +
                    LocalizationHelpers.GetFullDateString(commitData.AuthorDate) + ")"));
            string committerEmail = GetEmail(commitData.Committer);
            header.AppendLine(
                FillToLength(WebUtility.HtmlEncode(Strings.GetCommitterText()) + ":",
                    COMMITHEADER_STRING_LENGTH) +
                "<a href='mailto:" + WebUtility.HtmlEncode(committerEmail) + "'>" +
                WebUtility.HtmlEncode(commitData.Committer) + "</a>");
            header.AppendLine(
                FillToLength(WebUtility.HtmlEncode(Strings.GetCommitDateText()) + ":",
                    COMMITHEADER_STRING_LENGTH) +
                WebUtility.HtmlEncode(
                    LocalizationHelpers.GetRelativeDateString(DateTime.UtcNow, commitData.CommitDate.UtcDateTime) + " (" +
                    LocalizationHelpers.GetFullDateString(commitData.CommitDate) + ")"));
            header.Append(
                FillToLength(WebUtility.HtmlEncode(Strings.GetCommitHashText()) + ":",
                    COMMITHEADER_STRING_LENGTH) +
                WebUtility.HtmlEncode(commitData.Guid));
            var indent = FillToLength(" ", COMMITHEADER_STRING_LENGTH);
            if (commitData.ChildrenGuids != null && commitData.ChildrenGuids.Count != 0)
            {
                header.AppendLine();
                string commitsString;
                if (showRevisionsAsLinks)
                {
                    commitsString = commitData.ChildrenGuids
                        .Select(c => LinkFactory.CreateCommitLink(c) + " " + GetCommitSubject(c, module))
                        .Join(Environment.NewLine + indent);
                }
                else
                {
                    commitsString = commitData.ChildrenGuids
                        .Select(guid => guid.Substring(0, 10) + " " + GetCommitSubject(guid, module))
                        .Join(Environment.NewLine + indent);
                }
                header.Append(FillToLength(WebUtility.HtmlEncode(Strings.GetChildrenText()) + ":",
                    COMMITHEADER_STRING_LENGTH) + commitsString);
            }

            var parentGuids = commitData.ParentGuids.Where(s => !String.IsNullOrEmpty(s)).ToList();
            if (parentGuids.Any())
            {
                header.AppendLine();

                string commitsString;
                if (showRevisionsAsLinks)
                {
                    commitsString = parentGuids
                        .Select(c => LinkFactory.CreateCommitLink(c) + " " + GetCommitSubject(c, module))
                        .Join(Environment.NewLine + indent);
                }
                else
                {
                    commitsString = parentGuids
                        .Select(guid => indent + guid.Substring(0, 10) + " " + GetCommitSubject(guid, module))
                        .Join(Environment.NewLine + indent);
                }
                header.Append(FillToLength(WebUtility.HtmlEncode(Strings.GetParentsText()) + ":",
                    COMMITHEADER_STRING_LENGTH) + commitsString);
            }

            return RemoveRedundancies(header.ToString());
        }

        private static string GetCommitSubject(string sha, IGitRevisionProvider module)
        {
            return GitRevision.IsArtificial(sha)
                ? ""
                : module.GetRevision(sha, shortFormat: true).Subject;
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
                        (string) FillToLength(Strings.GetAuthorDateText() + ":", COMMITHEADER_STRING_LENGTH),
                        FillToLength(Strings.GetDateText() + ":", COMMITHEADER_STRING_LENGTH));
            }

            return info;
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