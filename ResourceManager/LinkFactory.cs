using System;
using System.Linq;
using System.Net;
using GitCommands;

namespace ResourceManager
{
    public class LinkFactory
    {
        public static string CreateLink(string caption, string uri)
        {
            return "<a href="+WebUtility.HtmlEncode(uri).Quote()+">" + WebUtility.HtmlEncode(caption) + "</a>";
        }

        public static string CreateTagLink(string tag)
        {
            if (tag != "…")
                return "<a href='gitext://gototag/" + tag + "'>" + WebUtility.HtmlEncode(tag) + "</a>";
            return WebUtility.HtmlEncode(tag);
        }

        public static string CreateBranchLink(string noPrefixBranch)
        {
            if (noPrefixBranch != "…")
                return "<a href='gitext://gotobranch/" + noPrefixBranch + "'>" + WebUtility.HtmlEncode(noPrefixBranch) + "</a>";
            return WebUtility.HtmlEncode(noPrefixBranch);
        }

        public static string CreateCommitLink(string guid, string linkText = null, bool preserveGuidInLinkText = false)
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
            return "<a href='gitext://gotocommit/" + guid + "'>" + linkText + "</a>";
        }

        public static string ParseLink(string aLinkText)
        {
            string uriCandidate = aLinkText;
            while(uriCandidate != null)
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
