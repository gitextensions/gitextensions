using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using GitCommands;
using GitCommands.Git.Extensions;

namespace ResourceManager.CommitDataRenders
{
    /// <summary>
    /// Provides the ability to render commit information.
    /// </summary>
    public interface ICommitDataHeaderRenderer
    {
        Font GetFont(Graphics g);

        IEnumerable<int> GetTabStops();

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
        private readonly IHeaderLabelFormatter _labelFormatter;
        private readonly IDateFormatter _dateFormatter;
        private readonly IHeaderRenderStyleProvider _headerRendererStyleProvider;
        private readonly ILinkFactory _linkFactory;


        public CommitDataHeaderRenderer(IHeaderLabelFormatter labelFormatter, IDateFormatter dateFormatter, IHeaderRenderStyleProvider headerRendererStyleProvider, ILinkFactory linkFactory)
        {
            _labelFormatter = labelFormatter;
            _dateFormatter = dateFormatter;
            _headerRendererStyleProvider = headerRendererStyleProvider;
            _linkFactory = linkFactory;
        }


        public Font GetFont(Graphics g)
        {
            return _headerRendererStyleProvider.GetFont(g);
        }

        public IEnumerable<int> GetTabStops()
        {
            return _headerRendererStyleProvider.GetTabStops();
        }

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

            bool isArtificial = commitData.Guid.IsArtificial();
            bool authorIsCommiter = string.Equals(commitData.Author, commitData.Committer, StringComparison.CurrentCulture);
            bool datesEqual = commitData.AuthorDate.EqualsExact(commitData.CommitDate);
            var padding = _headerRendererStyleProvider.GetMaxWidth();
            string authorEmail = GetEmail(commitData.Author);

            StringBuilder header = new StringBuilder();
            header.AppendLine(_labelFormatter.FormatLabel(Strings.GetAuthorText(), padding) + _linkFactory.CreateLink(commitData.Author, "mailto:" + authorEmail));

            if (!isArtificial)
            {
                header.AppendLine(_labelFormatter.FormatLabel(datesEqual ? Strings.GetDateText() : Strings.GetAuthorDateText(), padding) + WebUtility.HtmlEncode(_dateFormatter.FormatDateAsRelativeLocal(commitData.AuthorDate)));
            }

            if (!authorIsCommiter)
            {
                string committerEmail = GetEmail(commitData.Committer);
                header.AppendLine(_labelFormatter.FormatLabel(Strings.GetCommitterText(), padding) + _linkFactory.CreateLink(commitData.Committer, "mailto:" + committerEmail));
            }

            if (!isArtificial)
            {
                if (!datesEqual)
                {
                    header.AppendLine(_labelFormatter.FormatLabel(Strings.GetCommitDateText(), padding) + WebUtility.HtmlEncode(_dateFormatter.FormatDateAsRelativeLocal(commitData.CommitDate)));
                }

                header.AppendLine(_labelFormatter.FormatLabel(Strings.GetCommitHashText(), padding) + WebUtility.HtmlEncode(commitData.Guid));
            }

            if (commitData.ChildrenGuids != null && commitData.ChildrenGuids.Count != 0)
            {
                header.AppendLine(_labelFormatter.FormatLabel(Strings.GetChildrenText(), padding) + RenderHashCollection(commitData.ChildrenGuids, showRevisionsAsLinks));
            }

            var parentGuids = commitData.ParentGuids.Where(s => !string.IsNullOrEmpty(s)).ToList();
            if (parentGuids.Any())
            {
                header.AppendLine(_labelFormatter.FormatLabel(Strings.GetParentsText(), padding) + RenderHashCollection(parentGuids, showRevisionsAsLinks));
            }

            // remove the trailing newline character
            header.Length = header.Length - Environment.NewLine.Length;

            return header.ToString();
        }

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

            bool authorIsCommiter = string.Equals(commitData.Author, commitData.Committer, StringComparison.CurrentCulture);
            bool datesEqual = commitData.AuthorDate.EqualsExact(commitData.CommitDate);
            var padding = _headerRendererStyleProvider.GetMaxWidth();

            StringBuilder header = new StringBuilder();
            header.AppendLine(_labelFormatter.FormatLabel(Strings.GetAuthorText(), padding) + commitData.Author);
            header.AppendLine(_labelFormatter.FormatLabel(datesEqual ? Strings.GetDateText() : Strings.GetAuthorDateText(), padding) + _dateFormatter.FormatDateAsRelativeLocal(commitData.AuthorDate));
            if (!authorIsCommiter)
            {
                header.AppendLine(_labelFormatter.FormatLabel(Strings.GetCommitterText(), padding) + commitData.Committer);
            }
            if (!datesEqual)
            {
                header.AppendLine(_labelFormatter.FormatLabel(Strings.GetCommitDateText(), padding) + _dateFormatter.FormatDateAsRelativeLocal(commitData.CommitDate));
            }
            header.Append(_labelFormatter.FormatLabel(Strings.GetCommitHashText(), padding) + commitData.Guid);

            return header.ToString();
        }



        private static string GetEmail(string author)
        {
            if (string.IsNullOrEmpty(author))
            {
                return "";
            }
            var ind = author.IndexOf("<", StringComparison.Ordinal);
            if (ind == -1)
            {
                return "";
            }
            ++ind;
            return author.Substring(ind, author.LastIndexOf(">", StringComparison.Ordinal) - ind);
        }

        private string RenderHashCollection(IEnumerable<string> hashes, bool showRevisionsAsLinks)
        {
            string commitsString;
            if (showRevisionsAsLinks)
            {
                commitsString = hashes.Select(g => _linkFactory.CreateCommitLink(g)).Join(" ");
            }
            else
            {
                commitsString = hashes.Select(guid => GitRevision.ToShortSha(guid)).Join(" ");
            }
            return commitsString;
        }
    }
}