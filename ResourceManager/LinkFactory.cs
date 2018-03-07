using System;
using System.Collections.Concurrent;
using System.Net;
using GitCommands;

namespace ResourceManager
{
    public interface ILinkFactory
    {
        void Clear();
        string CreateLink(string caption, string uri);
        string CreateTagLink(string tag);
        string CreateBranchLink(string noPrefixBranch);
        string CreateCommitLink(string guid, string linkText = null, bool preserveGuidInLinkText = false);
        string ParseLink(string linkText);
    }

    public sealed class LinkFactory : ILinkFactory
    {
        private readonly ConcurrentDictionary<string, string> _linksMap = new ConcurrentDictionary<string, string>();

        public void Clear()
        {
            _linksMap.Clear();
        }

        public string CreateLink(string caption, string uri)
        {
            return AddLink(caption, uri);
        }

        private string AddLink(string caption, string uri)
        {
            string htmlUri = WebUtility.HtmlEncode(uri);
            string rtfLinkText = caption + "#" + htmlUri;
            _linksMap[rtfLinkText] = htmlUri;

            string htmlLink = "<a href=" + htmlUri.Quote("'") + ">" + WebUtility.HtmlEncode(caption) + "</a>";
            return htmlLink;
        }

        public string CreateTagLink(string tag)
        {
            if (tag != "…")
            {
                return AddLink(tag, "gitext://gototag/" + tag);
            }

            return WebUtility.HtmlEncode(tag);
        }

        public string CreateBranchLink(string noPrefixBranch)
        {
            if (noPrefixBranch != "…")
            {
                return AddLink(noPrefixBranch, "gitext://gotobranch/" + noPrefixBranch);
            }

            return WebUtility.HtmlEncode(noPrefixBranch);
        }

        public string CreateCommitLink(string guid, string linkText = null, bool preserveGuidInLinkText = false)
        {
            if (linkText == null)
            {
                if (guid == GitRevision.UnstagedGuid)
                {
                    linkText = Strings.GetCurrentUnstagedChanges();
                }
                else if (guid == GitRevision.IndexGuid)
                {
                    linkText = Strings.GetCurrentIndex();
                }
                else if (preserveGuidInLinkText)
                {
                    linkText = guid;
                }
                else
                {
                    linkText = GitRevision.ToShortSha(guid);
                }
            }

            return AddLink(linkText, "gitext://gotocommit/" + guid);
        }

        public string ParseLink(string linkText)
        {
            if (_linksMap.TryGetValue(linkText, out var linkUri))
            {
                return linkUri;
            }

            string uriCandidate = linkText;
            while (uriCandidate != null)
            {
                if (Uri.TryCreate(uriCandidate, UriKind.Absolute, out var uri))
                {
                    return uri.AbsoluteUri;
                }

                uriCandidate = uriCandidate.SkipStr("#");
            }

            return linkText;
        }
    }
}
