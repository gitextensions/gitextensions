namespace GitCommands.Git
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// git svn commands:
    /// svn clone
    /// svn fetch
    /// svn rebase
    /// svn dcommit
    /// </summary>
    public static class GitSvnCommandHelpers
    {
        private static string SvnPrefix = "svn";

        public static string CloneCmd(string fromSvn, string toPath)
        {
            toPath = GitCommandHelpers.FixPath(toPath);
            StringBuilder sb = new StringBuilder();
            sb.Append(SvnPrefix);
            sb.Append(" clone ");
            sb.AppendFormat("\"{0}\"", fromSvn.Trim());
            sb.AppendFormat("\"{0}\"", toPath.Trim());
            return sb.ToString();
        }
    }
}
