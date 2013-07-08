using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace Gerrit
{
    internal static class GerritUtil
    {
        public static string RunGerritCommand([NotNull] IWin32Window owner, [NotNull] IGitModule aModule, [NotNull] string command, [NotNull] string remote, byte[] stdIn)
        {
            var fetchUrl = GetFetchUrl(aModule, remote);

            return RunGerritCommand(owner, aModule, command, fetchUrl, remote, stdIn);
        }

        public static Uri GetFetchUrl(IGitModule aModule, string remote)
        {
            string remotes = aModule.RunGitCmd("remote show -n \"" + remote + "\"");

            string fetchUrlLine = remotes.Split('\n').Select(p => p.Trim()).First(p => p.StartsWith("Push"));

            return new Uri(fetchUrlLine.Split(new[] { ':' }, 2)[1].Trim());
        }

        public static string RunGerritCommand([NotNull] IWin32Window owner, [NotNull] IGitModule aModule, [NotNull] string command, [NotNull] Uri fetchUrl, [NotNull] string remote, byte[] stdIn)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (aModule == null)
                throw new ArgumentNullException("aModule");
            if (command == null)
                throw new ArgumentNullException("command");
            if (fetchUrl == null)
                throw new ArgumentNullException("fetchUrl");
            if (remote == null)
                throw new ArgumentNullException("remote");

            StartAgent(owner, aModule, remote);

            var sshCmd = GitCommandHelpers.GetSsh();
            if (GitCommandHelpers.Plink())
            {
                sshCmd = AppSettings.Plink;
            }
            if (string.IsNullOrEmpty(sshCmd))
            {
                sshCmd = "ssh.exe";
            }

            string hostname = fetchUrl.Host;
            string username = fetchUrl.UserInfo;
            string portFlag = GitCommandHelpers.Plink() ? " -P " : " -p ";
            int port = fetchUrl.Port;

            if (port == -1 && fetchUrl.Scheme == "ssh")
                port = 22;

            var sb = new StringBuilder();

            sb.Append('"');

            if (!string.IsNullOrEmpty(username))
            {
                sb.Append(username);
                sb.Append('@');
            }

            sb.Append(hostname);
            sb.Append('"');
            sb.Append(portFlag);
            sb.Append(port);

            sb.Append(" \"");
            sb.Append(command);
            sb.Append("\"");

            return aModule.RunCmd(
                sshCmd,
                sb.ToString(),
                null,
                stdIn
            );
        }

        public static void StartAgent([NotNull] IWin32Window owner, [NotNull] IGitModule aModule, [NotNull] string remote)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (aModule == null)
                throw new ArgumentNullException("aModule");
            if (remote == null)
                throw new ArgumentNullException("remote");

            if (GitCommandHelpers.Plink())
            {
                if (!File.Exists(AppSettings.Pageant))
                    MessageBoxes.PAgentNotFound(owner);
                else
                    aModule.StartPageantForRemote(remote);
            }
        }
    }
}
