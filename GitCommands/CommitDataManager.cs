using System;
using System.Collections.ObjectModel;
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
        /// Creates a CommitData object from formated commit info data from git.  The string passed in should be
        /// exact output of a log or show command using --format=LogFormat.
        /// </summary>
        /// <param name="data">Formated commit data from git.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        CommitData CreateFromFormatedData(string data);

        /// <summary>
        /// Creates a CommitData object from Git revision.
        /// </summary>
        /// <param name="revision">Git commit.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        CommitData CreateFromRevision(GitRevision revision);

        /// <summary>
        /// Gets the commit info for submodule.
        /// </summary>
        CommitData GetCommitData(string sha1, ref string error);

        /// <summary>
        /// Creates a CommitData object from formated commit info data from git.  The string passed in should be
        /// exact output of a log or show command using --format=LogFormat.
        /// </summary>
        /// <param name="commitData"></param>
        /// <param name="data">Formated commit data from git.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        void UpdateBodyInCommitData(CommitData commitData, string data);

        /// <summary>
        /// Gets the commit info for submodule.
        /// </summary>
        void UpdateCommitMessage(CommitData data, string sha1, ref string error);

    }

    public sealed class CommitDataManager : ICommitDataManager
    {
        private readonly Func<IGitModule> _getModule;
        private const string LogFormat = "%H%n%T%n%P%n%aN <%aE>%n%at%n%cN <%cE>%n%ct%n%e%n%B%nNotes:%n%-N";
        private const string ShortLogFormat = "%H%n%e%n%B%nNotes:%n%-N";


        public CommitDataManager(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }


        /// <summary>
        /// Gets the commit info for submodule.
        /// </summary>
        public void UpdateCommitMessage(CommitData data, string sha1, ref string error)
        {
            var module = GetModule();
            if (sha1 == null)
                throw new ArgumentNullException("sha1");

            //Do not cache this command, since notes can be added
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

        /// <summary>
        /// Gets the commit info for submodule.
        /// </summary>
        public CommitData GetCommitData(string sha1, ref string error)
        {
            var module = GetModule();
            if (sha1 == null)
                throw new ArgumentNullException("sha1");

            //Do not cache this command, since notes can be added
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

            CommitData commitInformation = CreateFromFormatedData(info);

            return commitInformation;
        }

        /// <summary>
        /// Creates a CommitData object from formated commit info data from git.  The string passed in should be
        /// exact output of a log or show command using --format=LogFormat.
        /// </summary>
        /// <param name="data">Formated commit data from git.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        public CommitData CreateFromFormatedData(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            var module = GetModule();

            var lines = data.Split('\n');

            var guid = lines[0];

            // TODO: we can use this to add more relationship info like gitk does if wanted
            var treeGuid = lines[1];

            // TODO: we can use this to add more relationship info like gitk does if wanted
            string[] parentLines = lines[2].Split(new char[] { ' ' });
            ReadOnlyCollection<string> parentGuids = parentLines.ToList().AsReadOnly();

            var author = module.ReEncodeStringFromLossless(lines[3]);
            var authorDate = DateTimeUtils.ParseUnixTime(lines[4]);

            var committer = module.ReEncodeStringFromLossless(lines[5]);
            var commitDate = DateTimeUtils.ParseUnixTime(lines[6]);

            string commitEncoding = lines[7];

            const int startIndex = 8;
            string message = ProccessDiffNotes(startIndex, lines);

            //commit message is not reencoded by git when format is given
            var body = module.ReEncodeCommitMessage(message, commitEncoding);

            var commitInformation = new CommitData(guid, treeGuid, parentGuids, author, authorDate,
                committer, commitDate, body);

            return commitInformation;
        }

        /// <summary>
        /// Creates a CommitData object from formated commit info data from git.  The string passed in should be
        /// exact output of a log or show command using --format=LogFormat.
        /// </summary>
        /// <param name="commitData"></param>
        /// <param name="data">Formated commit data from git.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        public void UpdateBodyInCommitData(CommitData commitData, string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            var module = GetModule();

            var lines = data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var guid = lines[0];

            string commitEncoding = lines[1];

            const int startIndex = 2;
            string message = ProccessDiffNotes(startIndex, lines);

            //commit message is not reencoded by git when format is given
            Debug.Assert(commitData.Guid == guid);
            commitData.Body = module.ReEncodeCommitMessage(message, commitEncoding);
        }

        /// <summary>
        /// Creates a CommitData object from Git revision.
        /// </summary>
        /// <param name="revision">Git commit.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        public CommitData CreateFromRevision(GitRevision revision)
        {
            if (revision == null)
                throw new ArgumentNullException("revision");

            CommitData data = new CommitData(revision.Guid, revision.TreeGuid, revision.ParentGuids.ToList().AsReadOnly(),
                String.Format("{0} <{1}>", revision.Author, revision.AuthorEmail), revision.AuthorDate,
                String.Format("{0} <{1}>", revision.Committer, revision.CommitterEmail), revision.CommitDate,
                revision.Body ?? revision.Subject);
            return data;
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
                endIndex--;

            var message = new StringBuilder();
            bool bNotesStart = false;
            for (int i = startIndex; i <= endIndex; i++)
            {
                string line = lines[i];
                if (bNotesStart)
                    line = "    " + line;
                message.AppendLine(line);
                if (lines[i] == "Notes:")
                    bNotesStart = true;
            }

            return message.ToString();
        }
    }
}