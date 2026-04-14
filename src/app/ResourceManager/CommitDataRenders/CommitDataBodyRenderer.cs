using System.Net;
using GitExtensions.Extensibility.Extensions;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace ResourceManager.CommitDataRenders;

/// <summary>
/// Provides the ability to render the body of a commit message.
/// </summary>
public interface ICommitDataBodyRenderer
{
    /// <summary>
    /// Render the body of a commit message.
    /// </summary>
    /// <param name="commitData"> The commit data to render.</param>
    /// <param name="showRevisionsAsLinks"> Whether to linkify commit hashes.</param>
    /// <param name="markdownConverter">
    ///  An optional function that converts Markdown text to XHTML. When provided,
    ///  the raw body is passed through this function instead of being HTML-encoded.
    /// </param>
    string Render(CommitData commitData, bool showRevisionsAsLinks, Func<string, string>? markdownConverter = null);
}

/// <summary>
/// Renders the body of a commit message.
/// </summary>
public sealed class CommitDataBodyRenderer : ICommitDataBodyRenderer
{
    private readonly Func<IGitModule> _getModule;
    private readonly ILinkFactory _linkFactory;

    public CommitDataBodyRenderer(Func<IGitModule> getModule, ILinkFactory linkFactory)
    {
        _getModule = getModule;
        _linkFactory = linkFactory;
    }

    /// <summary>
    /// Render the body of a commit message.
    /// </summary>
    public string Render(CommitData commitData, bool showRevisionsAsLinks, Func<string, string>? markdownConverter = null)
    {
        ArgumentNullException.ThrowIfNull(commitData);

        string rawBody = (UIExtensions.FormatBodyAndNotes(commitData.Body, commitData.Notes) ?? "").Trim();

        string body = markdownConverter is not null
            ? markdownConverter(rawBody)
            : WebUtility.HtmlEncode(rawBody);

        if (showRevisionsAsLinks)
        {
            body = GitRevision.Sha1HashShortRegex.Replace(body, match => ProcessHashCandidate(match.Value));
        }

        return body;
    }

    private string ProcessHashCandidate(string hash)
    {
        IGitModule module = _getModule();

        if (module is null)
        {
            return hash;
        }

        if (!module.TryResolvePartialCommitId(hash, out ObjectId? fullHash))
        {
            return hash;
        }

        return _linkFactory.CreateCommitLink(fullHash, hash, true);
    }
}
