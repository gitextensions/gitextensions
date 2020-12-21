using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using GitUIPluginInterfaces;

namespace ResourceManager
{
    public interface ILinkFactory
    {
        void Clear();

        string CreateLink(string? caption, string uri);

        string CreateTagLink(string tag);

        string CreateBranchLink(string noPrefixBranch);

        string CreateCommitLink(ObjectId objectId, string? linkText = null, bool preserveGuidInLinkText = false);

        string CreateShowAllLink(string what);

        void ExecuteLink(string linkText, Action<CommandEventArgs>? handleInternalLink = null, Action<string?>? showAll = null);

        bool ParseInternalScheme(Uri uri, [NotNullWhen(returnValue: true)] out CommandEventArgs? commandEventArgs);

        bool ParseLink(string linkText, [NotNullWhen(returnValue: true)] out Uri? uri);
    }

    public sealed class LinkFactory : ILinkFactory
    {
        private const string InternalScheme = "gitext";
        private const string ShowAll = "showall";

        private readonly ConcurrentDictionary<string, string> _linksMap = new ConcurrentDictionary<string, string>();

        public void Clear()
        {
            _linksMap.Clear();
        }

        public string CreateLink(string? caption, string uri)
        {
            return AddLink(caption, uri);
        }

        private string AddLink(string? caption, string uri)
        {
            string htmlUri = WebUtility.HtmlEncode(uri);
            string rtfLinkText = caption;
            _linksMap[rtfLinkText] = htmlUri;

            string htmlLink = "<a href=" + htmlUri.Quote("'") + ">" + WebUtility.HtmlEncode(caption) + "</a>";
            return htmlLink;
        }

        public string CreateTagLink(string tag)
        {
            if (tag != "…")
            {
                return AddLink(tag, $"{InternalScheme}://gototag/" + tag);
            }

            return WebUtility.HtmlEncode(tag);
        }

        public string CreateBranchLink(string noPrefixBranch)
        {
            if (noPrefixBranch != "…")
            {
                return AddLink(noPrefixBranch, $"{InternalScheme}://gotobranch/" + noPrefixBranch);
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

            return AddLink(linkText, $"{InternalScheme}://gotocommit/" + objectId);
        }

        public string CreateShowAllLink(string what)
            => AddLink($"[ {TranslatedStrings.ShowAll} ]", $"{InternalScheme}://{ShowAll}/{what}");

        public void ExecuteLink(string linkText, Action<CommandEventArgs>? handleInternalLink = null, Action<string?>? showAll = null)
        {
            if (!ParseLink(linkText, out var uri))
            {
                return;
            }

            if (ParseInternalScheme(uri, out var commandEventArgs))
            {
                if (commandEventArgs.Command == ShowAll)
                {
                    if (showAll is null)
                    {
                        throw new InvalidOperationException($"unexpected internal link: {linkText}");
                    }

                    showAll(commandEventArgs.Data);
                    return;
                }

                if (handleInternalLink is null)
                {
                    throw new InvalidOperationException($"unexpected internal link: {linkText}");
                }

                handleInternalLink(commandEventArgs);
                return;
            }

            using var process = new Process
            {
                EnableRaisingEvents = false,
                StartInfo = { FileName = uri.AbsoluteUri, UseShellExecute = true },
            };
            process.Start();
        }

        public bool ParseInternalScheme(Uri? uri, [NotNullWhen(returnValue: true)] out CommandEventArgs? commandEventArgs)
        {
            if (uri?.Scheme == InternalScheme)
            {
                commandEventArgs = new CommandEventArgs(uri.Host, uri.AbsolutePath.TrimStart('/'));
                return true;
            }

            commandEventArgs = null;
            return false;
        }

        public bool ParseLink(string linkText, [NotNullWhen(returnValue: true)] out Uri? uri)
        {
            if (linkText is null)
            {
                uri = null;
                return false;
            }

            if (_linksMap.TryGetValue(linkText, out var linkUri))
            {
                return Uri.TryCreate(linkUri, UriKind.Absolute, out uri);
            }

            var uriCandidate = linkText;

            while (true)
            {
                if (Uri.TryCreate(uriCandidate, UriKind.Absolute, out uri))
                {
                    return true;
                }

                var idx = uriCandidate.IndexOf('#');

                if (idx == -1)
                {
                    break;
                }

                uriCandidate = uriCandidate.Substring(idx + 1);
            }

            return false;
        }
    }
}
