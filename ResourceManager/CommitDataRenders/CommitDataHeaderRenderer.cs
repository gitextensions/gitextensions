using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitUIPluginInterfaces;
using Microsoft;

namespace ResourceManager.CommitDataRenders
{
    /// <summary>
    /// Provides the ability to render commit information.
    /// </summary>
    public interface ICommitDataHeaderRenderer
    {
        /// <summary>
        /// Gets the plain text for the clipboard - without tabs, relative date, children and parents.
        /// </summary>
        string GetPlainText(string header);

        Font GetFont(Graphics g);

        IEnumerable<int> GetTabStops();

        /// <summary>
        /// Generate header.
        /// </summary>
        string Render(CommitData commitData, bool showRevisionsAsLinks);

        /// <summary>
        /// Generate header.
        /// </summary>
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
        private readonly ILinkFactory? _linkFactory;

        public CommitDataHeaderRenderer(IHeaderLabelFormatter labelFormatter, IDateFormatter dateFormatter, IHeaderRenderStyleProvider headerRendererStyleProvider, ILinkFactory? linkFactory)
        {
            _labelFormatter = labelFormatter;
            _dateFormatter = dateFormatter;
            _headerRendererStyleProvider = headerRendererStyleProvider;
            _linkFactory = linkFactory;
        }

        public string GetPlainText(string header)
        {
            string children = $"({TranslatedStrings.GetChildren(1)})|({TranslatedStrings.GetChildren(2)})|({TranslatedStrings.GetChildren(10)})";
            string parents = $"({TranslatedStrings.GetParents(1)})|({TranslatedStrings.GetParents(2)})|({TranslatedStrings.GetParents(10)})";
            header = Regex.Replace(header, @"[ \t]+", " ");
            header = Regex.Replace(header, @"(\n[^:]+: ).* ago \(([^)]+)\)", "$1$2");
            header = Regex.Replace(header, @$"\n({children}|{parents})[^\n]*", "");
            return header;
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
        public string Render(CommitData commitData, bool showRevisionsAsLinks)
        {
            if (commitData is null)
            {
                throw new ArgumentNullException(nameof(commitData));
            }

            bool isArtificial = commitData.ObjectId.IsArtificial;
            bool authorIsCommitter = string.Equals(commitData.Author, commitData.Committer, StringComparison.CurrentCulture);
            bool datesEqual = commitData.AuthorDate.EqualsExact(commitData.CommitDate);
            var padding = _headerRendererStyleProvider.GetMaxWidth();
            string authorEmail = GetEmail(commitData.Author);

            Validates.NotNull(_linkFactory);

            StringBuilder header = new();
            header.AppendLine(_labelFormatter.FormatLabel(TranslatedStrings.Author, padding) + _linkFactory.CreateLink(commitData.Author, "mailto:" + authorEmail));

            if (!isArtificial)
            {
                header.AppendLine(_labelFormatter.FormatLabel(datesEqual ? TranslatedStrings.Date : TranslatedStrings.AuthorDate, padding) + WebUtility.HtmlEncode(_dateFormatter.FormatDateAsRelativeLocal(commitData.AuthorDate)));
            }

            if (!authorIsCommitter)
            {
                string committerEmail = GetEmail(commitData.Committer);
                header.AppendLine(_labelFormatter.FormatLabel(TranslatedStrings.Committer, padding) + _linkFactory.CreateLink(commitData.Committer, "mailto:" + committerEmail));
            }

            if (!isArtificial)
            {
                if (!datesEqual)
                {
                    header.AppendLine(_labelFormatter.FormatLabel(TranslatedStrings.CommitDate, padding) + WebUtility.HtmlEncode(_dateFormatter.FormatDateAsRelativeLocal(commitData.CommitDate)));
                }

                header.AppendLine(_labelFormatter.FormatLabel(TranslatedStrings.CommitHash, padding) + WebUtility.HtmlEncode(commitData.ObjectId.ToString()));
            }

            if (commitData.ChildIds is not null && commitData.ChildIds.Count != 0)
            {
                header.AppendLine(_labelFormatter.FormatLabel(TranslatedStrings.GetChildren(commitData.ChildIds.Count), padding) + RenderObjectIds(commitData.ChildIds, showRevisionsAsLinks));
            }

            var parentIds = commitData.ParentIds;
            if (parentIds?.Count > 0)
            {
                header.AppendLine(_labelFormatter.FormatLabel(TranslatedStrings.GetParents(parentIds.Count), padding) + RenderObjectIds(parentIds, showRevisionsAsLinks));
            }

            // remove the trailing newline character
            header.Length = header.Length - Environment.NewLine.Length;

            return header.ToString();
        }

        /// <summary>
        /// Generate header.
        /// </summary>
        public string RenderPlain(CommitData commitData)
        {
            if (commitData is null)
            {
                throw new ArgumentNullException(nameof(commitData));
            }

            bool authorIsCommitter = string.Equals(commitData.Author, commitData.Committer, StringComparison.CurrentCulture);
            bool datesEqual = commitData.AuthorDate.EqualsExact(commitData.CommitDate);
            var padding = _headerRendererStyleProvider.GetMaxWidth();

            StringBuilder header = new();
            header.AppendLine(_labelFormatter.FormatLabel(TranslatedStrings.Author, padding) + commitData.Author);
            header.AppendLine(_labelFormatter.FormatLabel(datesEqual ? TranslatedStrings.Date : TranslatedStrings.AuthorDate, padding) + _dateFormatter.FormatDateAsRelativeLocal(commitData.AuthorDate));
            if (!authorIsCommitter)
            {
                header.AppendLine(_labelFormatter.FormatLabel(TranslatedStrings.Committer, padding) + commitData.Committer);
            }

            if (!datesEqual)
            {
                header.AppendLine(_labelFormatter.FormatLabel(TranslatedStrings.CommitDate, padding) + _dateFormatter.FormatDateAsRelativeLocal(commitData.CommitDate));
            }

            header.Append(_labelFormatter.FormatLabel(TranslatedStrings.CommitHash, padding) + commitData.ObjectId);

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

        private string RenderObjectIds(IEnumerable<ObjectId> objectIds, bool showRevisionsAsLinks)
        {
            Validates.NotNull(_linkFactory);
            return showRevisionsAsLinks
                ? objectIds.Select(id => _linkFactory.CreateCommitLink(id)).Join(" ")
                : objectIds.Select(id => id.ToShortString()).Join(" ");
        }
    }
}
