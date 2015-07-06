using System;
using System.Net;
using GitCommands;

namespace ResourceManager
{
    public class LinkFactory
    {
        public static string CreateLink(string caption, string uri)
        {
            return "<a href="+uri.Quote()+">" + WebUtility.HtmlEncode(caption) + "</a>";
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

        public static string CreateCommitLink(string guid)
        {
            if (GitRevision.UnstagedGuid == guid)
                return "<a href='gitext://gotocommit/" + guid + "'>" + Strings.GetCurrentUnstagedChanges() + "</a>";
            else if (GitRevision.IndexGuid == guid)
                return "<a href='gitext://gotocommit/" + guid + "'>" + Strings.GetCurrentIndex() + "</a>";
            else
                return "<a href='gitext://gotocommit/" + guid + "'>" + guid.Substring(0, 10) + "</a>";
        }
    }
}