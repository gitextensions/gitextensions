using System.Diagnostics;
using System.Text;
using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public interface ISystemEncodingReader
    {
        /// <summary>
        /// Checks whether Git Extensions works with standard msysgit or msysgit-unicode.
        /// </summary>
        /// <returns>System encoding.</returns>
        Encoding Read();
    }

    public sealed class SystemEncodingReader : ISystemEncodingReader
    {
        private readonly IGitModule _module;

        public SystemEncodingReader(IGitModule? module)
        {
            _module = module ?? new GitModule("");
        }

        public SystemEncodingReader()
            : this(null)
        {
        }

        /// <inheritdoc />
        public Encoding Read()
        {
            try
            {
                Encoding systemEncoding;

                // invoke a git command that returns an invalid argument in its response, and
                // check if a unicode-only character is reported back. If so assume msysgit-unicode

                // git config --get with a malformed key (no section) returns:
                // "error: key does not contain a section: <key>"
                const string controlStr = "ą"; // "a caudata"
                GitArgumentBuilder arguments = new("config")
                {
                    "--get",
                    controlStr
                };

                ExecutionResult result = _module.GitExecutable.Execute(arguments, outputEncoding: Encoding.UTF8, throwOnErrorExit: false);
                string? s = result.StandardError;
                systemEncoding = s is not null && s.IndexOf(controlStr) != -1
                    ? new UTF8Encoding(false)
                    : Encoding.Default;

                Debug.WriteLine("System encoding: " + systemEncoding.EncodingName);

                return systemEncoding;
            }
            catch (Exception)
            {
                // Ignore exception. If the git location itself is not configured correctly yet, we could never execute it.
                return Encoding.Default;
            }
        }
    }
}
