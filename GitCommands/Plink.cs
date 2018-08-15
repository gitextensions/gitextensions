using System;
using System.Text;
using System.Threading.Tasks;
using GitUI;

namespace GitCommands
{
    public sealed class Plink
    {
        /* Documentation at: https://www.ssh.com/ssh/putty/putty-manuals/0.68/Chapter7.html
         *
         * Plink: command-line connection utility
         * Release 0.70
         * Usage: plink [options] [user@]host [command]
         *        ("host" can also be a PuTTY saved session name)
         * Options:
         *   -V        print version information and exit
         *   -pgpfp    print PGP key fingerprints and exit
         *   -v        show verbose messages
         *   -load sessname  Load settings from saved session
         *   -ssh -telnet -rlogin -raw -serial
         *             force use of a particular protocol
         *   -P port   connect to specified port
         *   -l user   connect with specified username
         *   -batch    disable all interactive prompts
         *   -proxycmd command
         *             use 'command' as local proxy
         *   -sercfg configuration-string (e.g. 19200,8,n,1,X)
         *             Specify the serial configuration (serial only)
         * The following options only apply to SSH connections:
         *   -pw passw login with specified password
         *   -D [listen-IP:]listen-port
         *             Dynamic SOCKS-based port forwarding
         *   -L [listen-IP:]listen-port:host:port
         *             Forward local port to remote address
         *   -R [listen-IP:]listen-port:host:port
         *             Forward remote port to local address
         *   -X -x     enable / disable X11 forwarding
         *   -A -a     enable / disable agent forwarding
         *   -t -T     enable / disable pty allocation
         *   -1 -2     force use of particular protocol version
         *   -4 -6     force use of IPv4 or IPv6
         *   -C        enable compression
         *   -i key    private key file for user authentication
         *   -noagent  disable use of Pageant
         *   -agent    enable use of Pageant
         *   -hostkey aa:bb:cc:...
         *             manually specify a host key (may be repeated)
         *   -m file   read remote command(s) from file
         *   -s        remote command is an SSH subsystem (SSH-2 only)
         *   -N        don't start a shell/command (SSH-2 only)
         *   -nc host:port
         *             open tunnel in place of session (SSH-2 only)
         *   -sshlog file
         *   -sshrawlog file
         *             log protocol details to a file
         *   -shareexists
         *             test whether a connection-sharing upstream exists
         */
        private readonly Executable _executable;

        public Plink(Executable executable = null)
        {
            _executable = executable ?? new Executable("cmd.exe");
        }

        public bool Connect(string host)
        {
            return ThreadHelper.JoinableTaskFactory.Run(
                () => ConnectAsync(host));
        }

        public async Task<bool> ConnectAsync(string host)
        {
            host = GetPlinkCompatibleUrl(host);

            var args = $"/k \"\"{AppSettings.Plink}\" -T {host}\"";

            using (var process = _executable.Start(args, createWindow: true, redirectInput: false, redirectOutput: false, outputEncoding: null))
            {
                return await process.WaitForExitAsync() == 0;
            }
        }

        /// <summary>
        /// Transforms the given input Url to make it compatible with Plink, if necessary
        /// </summary>
        internal static string GetPlinkCompatibleUrl(string inputUrl)
        {
            // We don't need putty for http:// links and git@... urls are already usable.
            // But ssh:// urls can cause problems
            if (!inputUrl.StartsWith("ssh") || !Uri.IsWellFormedUriString(inputUrl, UriKind.Absolute))
            {
                return inputUrl.Quote();
            }

            // Turn ssh://user@host/path into user@host:path, which works better
            var uri = new Uri(inputUrl, UriKind.Absolute);

            var fixedUrl = new StringBuilder();

            if (!uri.IsDefaultPort)
            {
                fixedUrl.Append("-P ").Append(uri.Port).Append(' ');
            }

            fixedUrl.Append('"');

            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                fixedUrl.Append(uri.UserInfo).Append('@');
            }

            fixedUrl.Append(uri.Host).Append(':').Append(uri.LocalPath, 1, uri.LocalPath.Length - 1)
                .Append('"');

            return fixedUrl.ToString();
        }
    }
}