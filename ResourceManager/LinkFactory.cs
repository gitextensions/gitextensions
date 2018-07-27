using System;
using System.Collections.Concurrent;
using System.Net;
using GitUIPluginInterfaces;

namespace ResourceManager
{
    public interface ILinkFactory
    {
        void Clear();
        string CreateLink(string caption, string uri);
        string CreateTagLink(string tag);
        string CreateBranchLink(string noPrefixBranch);
        string CreateCommitLink(ObjectId objectId, string linkText = null, bool preserveGuidInLinkText = false);
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

        public string CreateCommitLink(ObjectId objectId, string linkText = null, bool preserveGuidInLinkText = false)
        {
            if (linkText == null)
            {
                if (objectId == ObjectId.WorkTreeId)
                {
                    linkText = Strings.Workspace;
                }
                else if (objectId == ObjectId.IndexId)
                {
                    linkText = Strings.Index;
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

            return AddLink(linkText, "gitext://gotocommit/" + objectId);
        }

        public string ParseLink(string linkText)
        {
            if (_linksMap.TryGetValue(linkText, out var linkUri))
            {
                return linkUri;
            }

            var uriCandidate = linkText;

            while (true)
            {
                if (Uri.TryCreate(uriCandidate, UriKind.Absolute, out var uri))
                {
                    return uri.AbsoluteUri;
                }

                var idx = uriCandidate.IndexOf('#');

                if (idx == -1)
                {
                    break;
                }

                uriCandidate = uriCandidate.Substring(idx + 1);
            }

            return linkText;
        }
    }
}
