using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public interface ICommitDataManager
    {
        /// <summary>
        /// Parses <paramref name="data"/> into a <see cref="CommitData"/> object.
        /// </summary>
        /// <param name="data">Data produced by a <c>git log</c> or <c>git show</c> command where <c>--format</c>
        /// was provided the string <see cref="CommitDataManager.LogFormat"/>.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        CommitData CreateFromFormatedData(string data);

        /// <summary>
        /// Creates a <see cref="CommitData"/> object from <paramref name="revision"/>.
        /// </summary>
        /// <param name="revision">The commit to return data for.</param>
        CommitData CreateFromRevision(GitRevision revision);

        /// <summary>
        /// Gets the commit info for submodule.
        /// </summary>
        CommitData GetCommitData(string sha1, ref string error);

        /// <summary>
        /// Creates a CommitData object from formated commit info data from git.  The string passed in should be
        /// exact output of a log or show command using --format=LogFormat.
        /// </summary>
        /// <param name="data">Formated commit data from git.</param>
        void UpdateBodyInCommitData(CommitData commitData, string data);

        /// <summary>
        /// Updates the <see cref="CommitData.Body"/> property of <paramref name="data"/>.
        /// </summary>
        void UpdateCommitMessage(CommitData data, string sha1, ref string error);
    }

    public sealed class CommitDataManager : ICommitDataManager
    {
        private const string LogFormat = "%H%n%T%n%P%n%aN <%aE>%n%at%n%cN <%cE>%n%ct%n%e%n%B%nNotes:%n%-N";
        private const string ShortLogFormat = "%H%n%e%n%B%nNotes:%n%-N";

        private readonly Func<IGitModule> _getModule;

        public CommitDataManager(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }

        /// <inheritdoc />
        public void UpdateCommitMessage(CommitData data, string sha1, ref string error)
        {
            if (sha1 == null)
            {
                throw new ArgumentNullException(nameof(sha1));
            }

            var module = GetModule();

            // Do not cache this command, since notes can be added
            string arguments = string.Format(CultureInfo.InvariantCulture,
                "log -1 --pretty=\"format:" + ShortLogFormat + "\" {0}", sha1);
            var info = module.RunGitCmd(arguments, GitModule.LosslessEncoding);

            if (info.Trim().StartsWith("fatal"))
            {
                error = "Cannot find commit " + sha1;
                return;
            }

            int index = info.IndexOf(sha1) + sha1.Length;

            if (index < 0)
            {
                error = "Cannot find commit " + sha1;
                return;
            }

            if (index >= info.Length)
            {
                error = info;
                return;
            }

            UpdateBodyInCommitData(data, info);
        }

        /// <inheritdoc />
        public CommitData GetCommitData(string sha1, ref string error)
        {
            if (sha1 == null)
            {
                throw new ArgumentNullException(nameof(sha1));
            }

            var module = GetModule();

            // Do not cache this command, since notes can be added
            string arguments = string.Format(CultureInfo.InvariantCulture,
                "log -1 --pretty=\"format:" + LogFormat + "\" {0}", sha1);
            var info = module.RunGitCmd(arguments, GitModule.LosslessEncoding);

            if (info.Trim().StartsWith("fatal"))
            {
                error = "Cannot find commit " + sha1;
                return null;
            }

            int index = info.IndexOf(sha1) + sha1.Length;

            if (index < 0)
            {
                error = "Cannot find commit " + sha1;
                return null;
            }

            if (index >= info.Length)
            {
                error = info;
                return null;
            }

            return CreateFromFormatedData(info);
        }

        /// <inheritdoc />
        public CommitData CreateFromFormatedData(string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var module = GetModule();

            var lines = data.Split('\n');

            var guid = lines[0];

            // TODO: we can use this to add more relationship info like gitk does if wanted
            var treeGuid = lines[1];

            // TODO: we can use this to add more relationship info like gitk does if wanted
            string[] parentLines = lines[2].Split(' ');
            var parentGuids = lines[2].Split(' ');
            var author = module.ReEncodeStringFromLossless(lines[3]);
            var authorDate = DateTimeUtils.ParseUnixTime(lines[4]);

            var committer = module.ReEncodeStringFromLossless(lines[5]);
            var commitDate = DateTimeUtils.ParseUnixTime(lines[6]);

            string commitEncoding = lines[7];

            var message = ProccessDiffNotes(startIndex: 8, lines);

            // commit message is not reencoded by git when format is given
            var body = module.ReEncodeCommitMessage(message, commitEncoding);

            return new CommitData(guid, treeGuid, parentGuids, author, authorDate, committer, commitDate, body);
        }

        /// <inheritdoc />
        public void UpdateBodyInCommitData(CommitData commitData, string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var module = GetModule();

            var lines = data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var guid = lines[0];

            string commitEncoding = lines[1];

            var message = ProccessDiffNotes(startIndex: 2, lines);

            // commit message is not reencoded by git when format is given
            Debug.Assert(commitData.Guid == guid, "commitData.Guid == guid");
            commitData.Body = module.ReEncodeCommitMessage(message, commitEncoding);
        }

        /// <inheritdoc />
        public CommitData CreateFromRevision(GitRevision revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            return new CommitData(revision.Guid, revision.TreeGuid, revision.ParentGuids.ToList().AsReadOnly(),
                string.Format("{0} <{1}>", revision.Author, revision.AuthorEmail), revision.AuthorDate,
                string.Format("{0} <{1}>", revision.Committer, revision.CommitterEmail), revision.CommitDate,
                revision.Body ?? revision.Subject);
        }

        private IGitModule GetModule()
        {
            var module = _getModule();
            if (module == null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
            }

            return module;
        }

        private static string ProccessDiffNotes(int startIndex, string[] lines)
        {
            int endIndex = lines.Length - 1;
            if (lines[endIndex] == "Notes:")
            {
                endIndex--;
            }

            var message = new StringBuilder();
            bool notesStart = false;

            for (int i = startIndex; i <= endIndex; i++)
            {
                string line = lines[i];

                if (notesStart)
                {
                    message.Append("    ");
                }

                message.AppendLine(line);

                if (line == "Notes:")
                {
                    notesStart = true;
                }
            }

            return message.ToString();
        }
    }
}