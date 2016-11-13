using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GitUIPluginInterfaces;

namespace GitCommands
{
    [Flags]
    public enum RefsFiltringOptions
    {
        Branches = 1,               // --branches
        Remotes = 2,                // --remotes
        Tags = 4,                   // --tags
        Stashes = 8,                //
        All = 15,                   // --all
        Boundary = 16,              // --boundary
        ShowGitNotes = 32,          // --not --glob=notes --not
        NoMerges = 64,              // --no-merges
        FirstParent = 128,          // --first-parent
        SimplifyByDecoration = 256  // --simplify-by-decoration
    }

    public abstract class RevisionGraphInMemFilter
    {
        public abstract bool PassThru(GitRevision rev);
    }

    public sealed class RevisionGraph : IDisposable
    {
        public event EventHandler Exited;
        public event EventHandler<AsyncErrorEventArgs> Error
        {
            add
            {
                _backgroundLoader.LoadingError += value;
            }

            remove
            {
                _backgroundLoader.LoadingError -= value;
            }
        }
        public event EventHandler Updated;
        public event EventHandler BeginUpdate;
        public int RevisionCount { get; set; }

        public class RevisionGraphUpdatedEventArgs : EventArgs
        {
            public RevisionGraphUpdatedEventArgs(GitRevision revision)
            {
                Revision = revision;
            }

            public readonly GitRevision Revision;
        }

        public bool ShaOnly { get; set; }

        private readonly char[] _hexChars = "0123456789ABCDEFabcdef".ToCharArray();

        private const string CommitBegin = "<(__BEGIN_COMMIT__)>"; // Something unlikely to show up in a comment

        private Dictionary<string, List<IGitRef>> _refs;

        private enum ReadStep
        {
            Commit,
            CommitSubject,
            CommitBody,
            FileName,
        }

        private ReadStep _nextStep = ReadStep.Commit;

        private GitRevision _revision;

        private readonly AsyncLoader _backgroundLoader = new AsyncLoader();

        private readonly GitModule _module;

        public RevisionGraph(GitModule module)
        {
            _module = module;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _backgroundLoader.Cancel();
                _backgroundLoader.Dispose();
            }
        }

        public RefsFiltringOptions RefsOptions = RefsFiltringOptions.All | RefsFiltringOptions.Boundary;
        public string RevisionFilter = String.Empty;
        public string PathFilter = String.Empty;
        public string BranchFilter = String.Empty;
        public RevisionGraphInMemFilter InMemFilter;
        private string _selectedBranchName;
        static char[] ShellGlobCharacters = new[] { '?', '*', '[' };

        public void Execute()
        {
            _backgroundLoader.Load(ProccessGitLog, ProccessGitLogExecuted);
        }

        private void ProccessGitLog(CancellationToken taskState)
        {
            RevisionCount = 0;
            if (Updated != null)
                Updated(this, new RevisionGraphUpdatedEventArgs(null));
            _refs = GetRefs().ToDictionaryOfList(head => head.Guid);

            string formatString =
                /* <COMMIT>       */ CommitBegin + "%n" +
                /* Hash           */ "%H%n" +
                /* Parents        */ "%P%n";
            if (!ShaOnly)
            {
                formatString +=
                    /* Tree                    */ "%T%n" +
                    /* Author Name             */ "%aN%n" +
                    /* Author Email            */ "%aE%n" +
                    /* Author Date             */ "%at%n" +
                    /* Committer Name          */ "%cN%n" +
                    /* Committer Email         */ "%cE%n" +
                    /* Committer Date          */ "%ct%n" +
                    /* Commit message encoding */ "%e%x00" + //there is a bug: git does not recode commit message when format is given
                    /* Commit Subject          */ "%s%x00" +
                    /* Commit Body             */ "%B%x00";
            }

            // NOTE:
            // when called from FileHistory and FollowRenamesInFileHistory is enabled the "--name-only" argument is set.
            // the filename is the next line after the commit-format defined above.

            string logParam;
            if (AppSettings.OrderRevisionByDate)
            {
                logParam = " --date-order";
            }
            else
            {
                logParam = " --topo-order";
            }

            if ((RefsOptions & RefsFiltringOptions.All) == RefsFiltringOptions.All)
                logParam += " --all";
            else
            {
                if ((RefsOptions & RefsFiltringOptions.Branches) == RefsFiltringOptions.Branches)
                    logParam = " --branches";
                if ((RefsOptions & RefsFiltringOptions.Remotes) == RefsFiltringOptions.Remotes)
                    logParam += " --remotes";
                if ((RefsOptions & RefsFiltringOptions.Tags) == RefsFiltringOptions.Tags)
                    logParam += " --tags";
            }
            if ((RefsOptions & RefsFiltringOptions.Boundary) == RefsFiltringOptions.Boundary)
                logParam += " --boundary";
            if ((RefsOptions & RefsFiltringOptions.ShowGitNotes) == RefsFiltringOptions.ShowGitNotes)
                logParam += " --not --glob=notes --not";

            if ((RefsOptions & RefsFiltringOptions.NoMerges) == RefsFiltringOptions.NoMerges)
                logParam += " --no-merges";

            if ((RefsOptions & RefsFiltringOptions.FirstParent) == RefsFiltringOptions.FirstParent)
                logParam += " --first-parent";

            if ((RefsOptions & RefsFiltringOptions.SimplifyByDecoration) == RefsFiltringOptions.SimplifyByDecoration)
                logParam += " --simplify-by-decoration";

            string branchFilter = BranchFilter;
            if ((!string.IsNullOrWhiteSpace(BranchFilter)) &&
                (BranchFilter.IndexOfAny(ShellGlobCharacters) >= 0))
                branchFilter = "--branches=" + BranchFilter;

            string arguments = String.Format(CultureInfo.InvariantCulture,
                "log -z {2} --pretty=format:\"{1}\" {0} {3} -- {4}",
                logParam,
                formatString,
                branchFilter,
                RevisionFilter,
                PathFilter);

            Process p = _module.RunGitCmdDetached(arguments, GitModule.LosslessEncoding);

            if (taskState.IsCancellationRequested)
                return;

            _previousFileName = null;
            if (BeginUpdate != null)
                BeginUpdate(this, EventArgs.Empty);

            _nextStep = ReadStep.Commit;
            foreach (string data in ReadDataBlocks(p.StandardOutput))
            {
                if (taskState.IsCancellationRequested)
                    break;

                DataReceived(data);
            }
        }

        private static IEnumerable<string> ReadDataBlocks(StreamReader reader)
        {
            int bufferSize = 4 * 1024;
            char[] buffer = new char[bufferSize];

            StringBuilder incompleteBlock = new StringBuilder();
            while (true)
            {
                int bytesRead = reader.ReadBlock(buffer, 0, bufferSize);
                if (bytesRead == 0)
                    break;

                string bufferString = new string(buffer, 0, bytesRead);
                string[] dataBlocks = bufferString.Split(new char[] { '\0' });

                if (dataBlocks.Length > 1)
                {
                    // There are at least two blocks, so we can return the first one
                    incompleteBlock.Append(dataBlocks[0]);
                    yield return incompleteBlock.ToString();
                    incompleteBlock.Clear();
                }

                int lastDataBlockIndex = dataBlocks.Length - 1;

                // Return all the blocks until the last one 
                for (int i = 1; i < lastDataBlockIndex; i++)
                {
                    yield return dataBlocks[i];
                }

                // Append the beginning of the last block
                incompleteBlock.Append(dataBlocks[lastDataBlockIndex]);
            }

            if (incompleteBlock.Length > 0)
            {
                yield return incompleteBlock.ToString();
            }
        }

        private void ProccessGitLogExecuted()
        {
            FinishRevision();
            _previousFileName = null;

            if (Exited != null)
                Exited(this, EventArgs.Empty);
        }

        private IList<IGitRef> GetRefs()
        {
            var result = _module.GetRefs(true);
            bool validWorkingDir = _module.IsValidGitWorkingDir();
            _selectedBranchName = validWorkingDir ? _module.GetSelectedBranch() : string.Empty;
            var selectedRef = result.FirstOrDefault(head => head.Name == _selectedBranchName);

            if (selectedRef != null)
            {
                selectedRef.Selected = true;

                var localConfigFile = _module.LocalConfigFile;

                var selectedHeadMergeSource =
                    result.FirstOrDefault(head => head.IsRemote
                                        && selectedRef.GetTrackingRemote(localConfigFile) == head.Remote
                                        && selectedRef.GetMergeWith(localConfigFile) == head.LocalName);

                if (selectedHeadMergeSource != null)
                    selectedHeadMergeSource.SelectedHeadMergeSource = true;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Refs loaded while the latest processing of git log</returns>
        public IEnumerable<IGitRef> LatestRefs()
        {
            if (_refs == null)
            {
                return Enumerable.Empty<IGitRef>();
            }
            else
            {
                return _refs.Values.Unwrap();
            }
        }

        private string _previousFileName;

        void FinishRevision()
        {
            if (_revision != null && _revision.Guid == null)
                _revision = null;
            if (_revision != null)
            {
                if (_revision.Name == null)
                    _revision.Name = _previousFileName;
                else
                    _previousFileName = _revision.Name;

                if (_revision.Guid.Trim(_hexChars).Length == 0 &&
                    (InMemFilter == null || InMemFilter.PassThru(_revision)))
                {
                    // Remove full commit message to reduce memory consumption (28% for a repo with 69K commits)
                    // Full commit message is used in InMemFilter but later it's not needed
                    _revision.Body = null;

                    RevisionCount++;
                    if (Updated != null)
                        Updated(this, new RevisionGraphUpdatedEventArgs(_revision));
                }
            }
        }

        void DataReceived(string data)
        {
            if (data.StartsWith(CommitBegin))
            {
                // a new commit finalizes the last revision
                FinishRevision();
                _nextStep = ReadStep.Commit;
            }

            switch (_nextStep)
            {
                case ReadStep.Commit:
                    data = GitModule.ReEncodeString(data, GitModule.LosslessEncoding, _module.LogOutputEncoding);

                    string[] lines = data.Split(new char[] { '\n' });
                    Debug.Assert(lines.Length == 11);
                    Debug.Assert(lines[0] == CommitBegin);

                    _revision = new GitRevision(_module, null);

                    _revision.Guid = lines[1];
                    {
                        List<IGitRef> gitRefs;
                        if (_refs.TryGetValue(_revision.Guid, out gitRefs))
                            _revision.Refs.AddRange(gitRefs);
                    }

                    // RemoveEmptyEntries is required for root commits. They should have empty list of parents.
                    _revision.ParentGuids = lines[2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    _revision.TreeGuid = lines[3];

                    _revision.Author = lines[4];
                    _revision.AuthorEmail = lines[5];
                    {
                        DateTime dateTime;
                        if (DateTimeUtils.TryParseUnixTime(lines[6], out dateTime))
                            _revision.AuthorDate = dateTime;
                    }

                    _revision.Committer = lines[7];
                    _revision.CommitterEmail = lines[8];
                    {
                        DateTime dateTime;
                        if (DateTimeUtils.TryParseUnixTime(lines[9], out dateTime))
                            _revision.CommitDate = dateTime;
                    }

                    _revision.MessageEncoding = lines[10];
                    break;

                case ReadStep.CommitSubject:
                    _revision.Subject = _module.ReEncodeCommitMessage(data, _revision.MessageEncoding);
                    break;

                case ReadStep.CommitBody:
                    _revision.Body = _module.ReEncodeCommitMessage(data, _revision.MessageEncoding);
                    break;

                case ReadStep.FileName:
                    if (!string.IsNullOrEmpty(data))
                    {
                        // Git adds \n between the format string (ends with \0 in our case) 
                        // and the first file name. So, we need to remove it from the file name.
                        _revision.Name = data.TrimStart(new char[] { '\n' });
                    }
                    break;
            }

            _nextStep++;
        }
    }
}
