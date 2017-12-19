using System;
using System.Linq;
using System.Net;
using System.Text;
using GitCommands;

namespace ResourceManager
{
    /// <summary>
    /// Provides the ability to render commit information.
    /// </summary>
    public interface ICommitDataHeaderRenderer
    {
        /// <summary>
        /// Generate header.
        /// </summary>
        /// <returns></returns>
        string Render(CommitData commitData, bool showRevisionsAsLinks);

        /// <summary>
        /// Generate header.
        /// </summary>
        /// <param name="commitData"></param>
        /// <returns></returns>
        string RenderPlain(CommitData commitData);
    }

    /// <summary>
    /// Renders commit information in a tabular format with data columns aligned with spaces.
    /// </summary>
    public sealed class CommitDataHeaderRenderer : ICommitDataHeaderRenderer
    {
        private readonly ILinkFactory _linkFactory;


        public CommitDataHeaderRenderer(ILinkFactory linkFactory)
        {
            _linkFactory = linkFactory;
        }


        private static string GetEmail(string author)
        {
            if (string.IsNullOrEmpty(author))
                return "";
            var ind = author.IndexOf("<", StringComparison.Ordinal);
            if (ind == -1)
                return "";
            ++ind;
            return author.Substring(ind, author.LastIndexOf(">", StringComparison.Ordinal) - ind);
        }

        private static int? _headerPadding;

        private static int GetHeaderPadding()
        {
            if (_headerPadding != null)
            {
                return _headerPadding.GetValueOrDefault();
            }

            var strings = new[]
            {
                Strings.GetAuthorText(),
                Strings.GetAuthorDateText(),
                Strings.GetCommitterText(),
                Strings.GetCommitDateText(),
                Strings.GetCommitHashText(),
                Strings.GetChildrenText(),
                Strings.GetParentsText()
            };

            var maxLegnth = strings.Select(s => s.Length).Max();

            _headerPadding = maxLegnth + 2;
            return _headerPadding.GetValueOrDefault();
        }

        /// <inheritdoc />
        /// <summary>
        /// Generate header.
        /// </summary>
        /// <param name="commitData"></param>
        /// <returns></returns>
        public string RenderPlain(CommitData commitData)
        {
            if (commitData == null)
            {
                throw new ArgumentNullException(nameof(commitData));
            }

            StringBuilder header = new StringBuilder();
            bool authorIsCommiter = string.Equals(commitData.Author, commitData.Committer, StringComparison.CurrentCulture);
            bool datesEqual = commitData.AuthorDate.EqualsExact(commitData.CommitDate);

            header.AppendLine((Strings.GetAuthorText() + ":").PadRight(GetHeaderPadding()) +
                              commitData.Author);

            header.AppendLine((datesEqual ? Strings.GetDateText() : Strings.GetAuthorDateText() + ":").PadRight(GetHeaderPadding()) +
                              LocalizationHelpers.GetRelativeDateString(DateTime.UtcNow, commitData.AuthorDate.UtcDateTime) +
                              " (" + LocalizationHelpers.GetFullDateString(commitData.AuthorDate) + ")");
            if (!authorIsCommiter)
            {
                header.AppendLine((Strings.GetCommitterText() + ":").PadRight(GetHeaderPadding()) +
                                  commitData.Committer);
            }
            if (!datesEqual)
            {
                header.AppendLine((Strings.GetCommitDateText() + ":").PadRight(GetHeaderPadding()) +
                                  LocalizationHelpers.GetRelativeDateString(DateTime.UtcNow, commitData.CommitDate.UtcDateTime) +
                                  " (" + LocalizationHelpers.GetFullDateString(commitData.CommitDate) + ")");

                header.Append((Strings.GetCommitHashText() + ":").PadRight(GetHeaderPadding()) +
                              commitData.Guid);
            }

            header.Append(
                (WebUtility.HtmlEncode(Strings.GetCommitHashText()) + ":").PadRight(GetHeaderPadding()) +
                WebUtility.HtmlEncode(commitData.Guid));

            return header.ToString();
        }

        /// <inheritdoc />
        /// <summary>
        /// Generate header.
        /// </summary>
        /// <returns></returns>
        public string Render(CommitData commitData, bool showRevisionsAsLinks)
        {
            if (commitData == null)
            {
                throw new ArgumentNullException(nameof(commitData));
            }

            StringBuilder header = new StringBuilder();
            bool authorIsCommiter = string.Equals(commitData.Author, commitData.Committer, StringComparison.CurrentCulture);
            bool datesEqual = commitData.AuthorDate.EqualsExact(commitData.CommitDate);

            string authorEmail = GetEmail(commitData.Author);
            header.AppendLine(
                (WebUtility.HtmlEncode(Strings.GetAuthorText()) + ":").PadRight(GetHeaderPadding()) +
                _linkFactory.CreateLink(commitData.Author, "mailto:" + authorEmail));
            header.AppendLine(
                (WebUtility.HtmlEncode(datesEqual ? Strings.GetDateText() :
                                       Strings.GetAuthorDateText()) + ":").PadRight(GetHeaderPadding()) +
                WebUtility.HtmlEncode(
                    LocalizationHelpers.GetRelativeDateString(DateTime.UtcNow, commitData.AuthorDate.UtcDateTime) + " (" +
                    LocalizationHelpers.GetFullDateString(commitData.AuthorDate) + ")"));

            if (!authorIsCommiter)
            {
                string committerEmail = GetEmail(commitData.Committer);
                header.AppendLine(
                    (WebUtility.HtmlEncode(Strings.GetCommitterText()) + ":").PadRight(GetHeaderPadding()) +
                    "<a href='mailto:" + WebUtility.HtmlEncode(committerEmail) + "'>" +
                    WebUtility.HtmlEncode(commitData.Committer) + "</a>");
            }

            if (!datesEqual)
            {
                header.AppendLine(
                    (WebUtility.HtmlEncode(Strings.GetCommitDateText()) + ":").PadRight(GetHeaderPadding()) +
                    WebUtility.HtmlEncode(
                        LocalizationHelpers.GetRelativeDateString(DateTime.UtcNow, commitData.CommitDate.UtcDateTime) + " (" +
                        LocalizationHelpers.GetFullDateString(commitData.CommitDate) + ")"));
            }

            header.Append(
                (WebUtility.HtmlEncode(Strings.GetCommitHashText()) + ":").PadRight(GetHeaderPadding()) +
                WebUtility.HtmlEncode(commitData.Guid));

            if (commitData.ChildrenGuids != null && commitData.ChildrenGuids.Count != 0)
            {
                header.AppendLine();
                string commitsString;
                if (showRevisionsAsLinks)
                    commitsString = commitData.ChildrenGuids.Select(g => _linkFactory.CreateCommitLink(g)).Join(" ");
                else
                    commitsString = commitData.ChildrenGuids.Select(guid => guid.Substring(0, 10)).Join(" ");
                header.Append((WebUtility.HtmlEncode(Strings.GetChildrenText()) + ":").PadRight(GetHeaderPadding()) +
                              commitsString);
            }

            var parentGuids = commitData.ParentGuids.Where(s => !string.IsNullOrEmpty(s)).ToList();
            if (parentGuids.Any())
            {
                header.AppendLine();
                string commitsString;
                if (showRevisionsAsLinks)
                    commitsString = parentGuids.Select(g => _linkFactory.CreateCommitLink(g)).Join(" ");
                else
                    commitsString = parentGuids.Select(guid => guid.Substring(0, 10)).Join(" ");
                header.Append((WebUtility.HtmlEncode(Strings.GetParentsText()) + ":").PadRight(GetHeaderPadding())
                              + commitsString);
            }

            return header.ToString();
        }

    }
}