using System.IO;
using System.Text;

namespace GitCommands
{
    /// <summary>
    /// git svn commands:
    /// svn clone
    /// svn fetch
    /// svn rebase
    /// svn dcommit
    /// </summary>
    public static class GitSvnCommandHelpers
    {
        private const string SvnPrefix = "svn";

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
        public static bool CheckRefsRemoteSvn(GitModule aModule)
        {
            string svnremote = GetConfigSvnRemoteFetch(aModule);
            return svnremote != null && svnremote.Trim().Contains(":refs/remote");
        }

        public static string GetConfigSvnRemoteFetch(GitModule aModule)
        {
            return aModule.RunCmd(Settings.GitCommand, "config svn-remote.svn.fetch");
        }

        public static string RebaseCmd()
        {
            return SvnPrefix + " rebase";
        }

        public static string DcommitCmd()
        {
            return SvnPrefix + " dcommit";
        }

        public static string FetchCmd()
        {
            return SvnPrefix + " fetch";
        }

        public static bool ValidSvnWorkingDir(GitModule aModule)
        {
            return ValidSvnWorkingDir(aModule.WorkingDir);
        }

        public static bool ValidSvnWorkingDir(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return false;

            string path = dir + Settings.PathSeparator.ToString() + ".git" + Settings.PathSeparator.ToString() + "svn";
            if (Directory.Exists(path) || File.Exists(path))
                return true;

            return !dir.Contains(".git") &&
                   Directory.Exists(dir + Settings.PathSeparator.ToString() + "svn");
        }
    }
}
