namespace GitCommands
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

        public static string CloneCmd(string fromSvn, string toPath, string authorsFile)
        {
            toPath = GitCommandHelpers.FixPath(toPath);
            StringBuilder sb = new StringBuilder();
            sb.Append(SvnPrefix);
            sb.Append(" clone ");
            sb.AppendFormat("\"{0}\"", fromSvn.Trim());
            sb.Append(' ');
            sb.AppendFormat("\"{0}\"", toPath.Trim());
            if (authorsFile != null && authorsFile.Trim().Length!=0)
            {
                sb.Append(" --authors-file ");
                sb.AppendFormat("\"{0}\"", authorsFile.Trim());
            }
            return sb.ToString();
        }
    }
}
