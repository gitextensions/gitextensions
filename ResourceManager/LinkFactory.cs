using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using GitCommands.Git;
using GitUIPluginInterfaces;

namespace ResourceManager
{
    public interface ILinkFactory
    {
        string CreateLink(string? caption, string uri);

        string CreateTagLink(string tag);

        string CreateBranchLink(string noPrefixBranch);

        string CreateCommitLink(ObjectId objectId, string? linkText = null, bool preserveGuidInLinkText = false);

        string CreateShowAllLink(string what);

        void ExecuteLink(string? linkUri, Action<CommandEventArgs>? handleInternalLink = null, Action<string?>? showAll = null);
    }

    [Export(typeof(ILinkFactory))]
    public sealed class LinkFactory : ILinkFactory
    {
        private const string InternalScheme = "gitext";
        private const string ShowAll = "showall";

        public string CreateLink(string? caption, string uri)
        {
            string htmlUri = WebUtility.HtmlEncode(uri);

            string htmlLink = "<a href=" + htmlUri.Quote("'") + ">" + WebUtility.HtmlEncode(caption) + "</a>";
            return htmlLink;
        }

        public string CreateTagLink(string tag)
        {
            if (tag != "…")
            {
                return CreateLink(tag, $"{InternalScheme}://gototag/" + tag);
            }

            return WebUtility.HtmlEncode(tag);
        }

        public string CreateBranchLink(string noPrefixBranch)
        {
            if (noPrefixBranch != "…")
            {
                string linkTarget = DetachedHeadParser.IsDetachedHead(noPrefixBranch) ? "HEAD" : noPrefixBranch;
                return CreateLink(noPrefixBranch, $"{InternalScheme}://gotobranch/{linkTarget}");
            }

            return WebUtility.HtmlEncode(noPrefixBranch);
        }

        public string CreateCommitLink(ObjectId objectId, string? linkText = null, bool preserveGuidInLinkText = false)
        {
            if (linkText is null)
            {
                if (objectId == ObjectId.WorkTreeId)
                {
                    linkText = TranslatedStrings.Workspace;
                }
                else if (objectId == ObjectId.IndexId)
                {
                    linkText = TranslatedStrings.Index;
                }
                else if (preserveGuidInLinkText)
                {
                    linkText = objectId.ToString();
                }
                else
                {
                    linkText = objectId.ToShortString();
                }
            }

            return CreateLink(linkText, $"{InternalScheme}://gotocommit/" + objectId);
        }

        public string CreateShowAllLink(string what)
            => CreateLink($"[ {TranslatedStrings.ShowAll} ]", $"{InternalScheme}://{ShowAll}/{what}");

        public void ExecuteLink(string? linkUri, Action<CommandEventArgs>? handleInternalLink = null, Action<string?>? showAll = null)
        {
            if (!TryParseLink(linkUri, out Uri? uri))
            {
                return;
            }

            if (ParseInternalScheme(uri, out var commandEventArgs))
            {
                if (commandEventArgs.Command == ShowAll)
                {
                    if (showAll is null)
                    {
                        throw new InvalidOperationException($"unexpected internal link: {linkUri}");
                    }

                    showAll(commandEventArgs.Data);
                    return;
                }

                if (handleInternalLink is null)
                {
                    throw new InvalidOperationException($"unexpected internal link: {linkUri}");
                }

                handleInternalLink(commandEventArgs);
                return;
            }

            using Process process = new()
            {
                EnableRaisingEvents = false,
                StartInfo = { FileName = uri.AbsoluteUri, UseShellExecute = true },
            };
            process.Start();
        }

        private static bool ParseInternalScheme(Uri? uri, [NotNullWhen(returnValue: true)] out CommandEventArgs? commandEventArgs)
        {
            if (uri?.Scheme == InternalScheme)
            {
                commandEventArgs = new CommandEventArgs(uri.Host, uri.AbsolutePath.TrimStart('/'));
                return true;
            }

            commandEventArgs = null;
            return false;
        }

        private static bool TryParseLink(string? linkUri, [NotNullWhen(returnValue: true)] out Uri? uri)
        {
            if (string.IsNullOrWhiteSpace(linkUri))
            {
                uri = null;
                return false;
            }

            string uriCandidate = linkUri;
            while (true)
            {
                if (Uri.TryCreate(uriCandidate, UriKind.Absolute, out uri))
                {
                    return true;
                }

                int idx = uriCandidate.IndexOf('#');
                if (idx == -1)
                {
                    break;
                }

                uriCandidate = uriCandidate.Substring(idx + 1);
            }

            return false;
        }

        internal TestAccessor GetTestAccessor() => new(this);

        internal struct TestAccessor
        {
            private readonly LinkFactory _linkFactory;

            public TestAccessor(LinkFactory linkFactory)
            {
                _linkFactory = linkFactory;
            }

            public bool ParseInternalScheme(Uri uri, [NotNullWhen(returnValue: true)] out CommandEventArgs? commandEventArgs)
                => LinkFactory.ParseInternalScheme(uri, out commandEventArgs);

            public bool TryParseLink(string? linkUri, [NotNullWhen(returnValue: true)] out Uri? uri)
                => LinkFactory.TryParseLink(linkUri, out uri);
        }
    }
}
