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

        public static string CreateCommitLink(string parentGuid)
        {
            return "<a href='gitex://gotocommit/" + parentGuid + "'>" + parentGuid.Substring(0, 10) + "</a>";
        }
    }
}