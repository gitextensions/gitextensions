using System;
using System.Linq;
using System.Net;
using GitCommands;
using System.Collections.Concurrent;

namespace ResourceManager
{
    public interface ILinkFactory
    {
        void Clear();
        string CreateLink(string caption, string uri);
        string CreateTagLink(string tag);
        string CreateBranchLink(string noPrefixBranch);
        string CreateCommitLink(string guid, string linkText = null, bool preserveGuidInLinkText = false);
        string ParseLink(string aLinkText);
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
                return AddLink(tag, "gitext://gototag/" + tag);
            return WebUtility.HtmlEncode(tag);
        }

        public string CreateBranchLink(string noPrefixBranch)
        {
            if (noPrefixBranch != "…")
                return AddLink(noPrefixBranch, "gitext://gotobranch/" + noPrefixBranch);
            return WebUtility.HtmlEncode(noPrefixBranch);
        }

        public string CreateCommitLink(string guid, string linkText = null, bool preserveGuidInLinkText = false)
        {
            if (linkText == null)
            {
                if (GitRevision.UnstagedGuid == guid)
                    linkText = Strings.GetCurrentUnstagedChanges();
                else if (GitRevision.IndexGuid == guid)
                    linkText = Strings.GetCurrentIndex();
                else
                {
                    linkText = preserveGuidInLinkText || guid.Length < 10
                        ? guid
                        : guid.Substring(0, 10);
                }
            }
            return AddLink(linkText, "gitext://gotocommit/" + guid);
        }

        public string ParseLink(string aLinkText)
        {
            string linkUri;
            if (_linksMap.TryGetValue(aLinkText, out linkUri))
            {
                return linkUri;
            }

            string uriCandidate = aLinkText;
            while (uriCandidate != null)
            {
                Uri uri;
                if (Uri.TryCreate(uriCandidate, UriKind.Absolute, out uri))
                    return uri.AbsoluteUri;
                uriCandidate = uriCandidate.SkipStr("#");
            }

            return aLinkText;
        }
    }
}
