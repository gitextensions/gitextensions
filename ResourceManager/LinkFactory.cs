using System;
using System.Collections.Concurrent;
using System.Net;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace ResourceManager
{
    public interface ILinkFactory
    {
        void Clear();

        string CreateLink(string caption, string uri);

        string CreateTagLink(string tag);

        string CreateBranchLink(string noPrefixBranch);

        string CreateCommitLink(ObjectId objectId, string linkText = null, bool preserveGuidInLinkText = false);

        string CreateShowAllLink(string what);

        [ContractAnnotation("=>false,uri:null")]
        [ContractAnnotation("=>true,uri:notnull")]
        bool ParseLink(string linkText, out Uri uri);
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
                    linkText = ResourceManager.Strings.Workspace;
                }
                else if (objectId == ObjectId.IndexId)
                {
                    linkText = ResourceManager.Strings.Index;
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

        public string CreateShowAllLink(string what)
            => AddLink($"[ {Strings.ShowAll} ]", $"gitext://showall/{what}");

        public bool ParseLink(string linkText, out Uri uri)
        {
            if (linkText == null)
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
