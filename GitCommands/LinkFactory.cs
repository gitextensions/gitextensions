using System.Web;

namespace GitCommands
{
    public class LinkFactory
    {
         public static string CreateTagLink(string tag)
         {
             return "<a href='gitex://gototag/" + tag + "'>" + HttpUtility.HtmlEncode(tag) + "</a>";
         }

        public static string CreateBranchLink(string noPrefixBranch)
        {
            return "<a href='gitex://gotobranch/" + noPrefixBranch + "'>" + HttpUtility.HtmlEncode(noPrefixBranch) + "</a>";
        }

        public static string CreateCommitLink(string guid)
        {
            if (GitRevision.UncommittedWorkingDirGuid == guid)
                return "<a href='gitex://gotocommit/" + guid + "'>" + Strings.GetCurrentWorkingDirChanges() + "</a>";
            else if (GitRevision.IndexGuid == guid)
                return "<a href='gitex://gotocommit/" + guid + "'>" + Strings.GetCurrentIndex() + "</a>";
            else
                return "<a href='gitex://gotocommit/" + guid + "'>" + guid.Substring(0, 10) + "</a>";
        }
    }
}