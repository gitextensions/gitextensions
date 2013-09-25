using System;
using System.Net;

namespace GitCommands
{
    public class LinkFactory
    {
        public static string CreateLink(string caption, string uri)
        {
            return "<a href="+uri.Quote()+">" + WebUtility.HtmlEncode(caption) + "</a>";
        }

        public static string CreateTagLink(string tag)
        {
            return "<a href='gitext://gototag/" + tag + "'>" + WebUtility.HtmlEncode(tag) + "</a>";
        }

        public static string CreateBranchLink(string noPrefixBranch)
        {
            return "<a href='gitext://gotobranch/" + noPrefixBranch + "'>" + WebUtility.HtmlEncode(noPrefixBranch) + "</a>";
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