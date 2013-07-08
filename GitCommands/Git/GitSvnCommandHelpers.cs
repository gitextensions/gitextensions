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

        public static string CloneCmd(string fromSvn, string toPath, string username, 
            string authorsFile, int fromRevision, 
            string trunk, string tags, string branches)
        {
            toPath = GitCommandHelpers.FixPath(toPath);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} clone \"{1}\" \"{2}\"", SvnPrefix, fromSvn, toPath);
            if (!string.IsNullOrEmpty(username))
            {
                sb.AppendFormat(" --username=\"{0}\"", username);
            }
            if (!string.IsNullOrEmpty(authorsFile))
            {
                sb.AppendFormat(" --authors-file=\"{0}\"", authorsFile);
            }
            if (fromRevision != 0)
            {
                sb.AppendFormat(" -r \"{0}\"", fromRevision);
            }
            if (!string.IsNullOrEmpty(trunk))
            {
                sb.AppendFormat(" --trunk=\"{0}\"", trunk);
            }
            if (!string.IsNullOrEmpty(tags))
            {
                sb.AppendFormat(" --tags=\"{0}\"", tags);
            }
            if (!string.IsNullOrEmpty(branches))
            {
                sb.AppendFormat(" --branches=\"{0}\"", branches);
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
            return aModule.RunGitCmd("config svn-remote.svn.fetch");
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

            string path = dir + AppSettings.PathSeparator.ToString() + ".git" + AppSettings.PathSeparator.ToString() + "svn";
            if (Directory.Exists(path) || File.Exists(path))
                return true;

            return !dir.Contains(".git") &&
                   Directory.Exists(dir + AppSettings.PathSeparator.ToString() + "svn");
        }
    }
}
