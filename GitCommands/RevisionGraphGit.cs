using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace GitCommands
{
    public sealed class RevisionGraphGit : RevisionGraph
    {
        public RevisionGraphGit(GitModule module)
            : base(module)
        {
        }

        private readonly char[] _splitChars = " \t\n".ToCharArray();

        private readonly char[] _hexChars = "0123456789ABCDEFabcdef".ToCharArray();

        private const string CommitBegin = "<(__BEGIN_COMMIT__)>"; // Something unlikely to show up in a comment

        private enum ReadStep
        {
            Commit,
            Hash,
            Parents,
            Tree,
            AuthorName,
            AuthorEmail,
            AuthorDate,
            CommitterName,
            CommitterEmail,
            CommitterDate,
            CommitMessageEncoding,
            CommitMessage,
            FileName,
            Done,
        }

        private ReadStep _nextStep = ReadStep.Commit;

        static char[] ShellGlobCharacters = new[] { '?', '*', '[' };

        private string _previousFileName;

        protected override void ProccessGitLog(CancellationToken taskState)
        {
            RevisionCount = 0;
            _refs = GetRefs().ToDictionaryOfList(head => head.Guid);

            string formatString =
                /* <COMMIT>                */ CommitBegin + "%n" +
                /* Hash                    */ "%H%n" +
                /* Parents                 */ "%P%n" +
                /* Tree                    */ "%T%n" +
                /* Author Name             */ "%aN%n" +
                /* Author Email            */ "%aE%n" +
                /* Author Date             */ "%at%n" +
                /* Committer Name          */ "%cN%n" +
                /* Committer Email         */ "%cE%n" +
                /* Committer Date          */ "%ct%n" +
                /* Commit message encoding */ "%e%n" + //there is a bug: git does not recode commit message when format is given
                /* Commit Message          */ "%s";

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

            string branchFilter = BranchFilter;
            if ((!string.IsNullOrWhiteSpace(BranchFilter)) &&
                (BranchFilter.IndexOfAny(ShellGlobCharacters) >= 0))
                branchFilter = "--branches=" + BranchFilter;

            string arguments = String.Format(CultureInfo.InvariantCulture,
                "log -z {2} --pretty=format:\"{1}\" {0} {3}",
                logParam,
                formatString,
                branchFilter,
                Filter);

            Encoding logOutputEncoding = _module.LogOutputEncoding;

            Process p = _module.RunGitCmdDetached(arguments, GitModule.LosslessEncoding);

            if (taskState.IsCancellationRequested)
                return;

            _previousFileName = null;

            string line;
            do
            {
                line = p.StandardOutput.ReadLine();
                //commit message is not encoded by git
                if (_nextStep != ReadStep.CommitMessage)
                    line = GitModule.ReEncodeString(line, GitModule.LosslessEncoding, logOutputEncoding);

                if (line != null)
                {
                    foreach (string entry in line.Split('\0'))
                    {
                        DataReceived(entry);
                    }
                }
            } while (line != null && !taskState.IsCancellationRequested);
        }

        protected override void OnExited()
        {
            FinishRevision();
            _previousFileName = null;

            base.OnExited();
        }

        private GitRevision _revision;

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
            }
            if (_revision == null || _revision.Guid.Trim(_hexChars).Length == 0)
            {
                if (_revision == null || InMemFilter == null || InMemFilter.PassThru(_revision))
                {
                    if (_revision != null)
                        RevisionCount++;
                    OnUpdated(_revision);
                }
            }
            _nextStep = ReadStep.Commit;
        }

        protected Dictionary<string, List<GitRef>> _refs;

        void DataReceived(string line)
        {
            if (line == null)
                return;

            if (line == CommitBegin)
            {
                // a new commit finalizes the last revision
                FinishRevision();

                _nextStep = ReadStep.Commit;
            }

            switch (_nextStep)
            {
                case ReadStep.Commit:
                    // Sanity check
                    if (line == CommitBegin)
                    {
                        _revision = new GitRevision(_module, null);
                    }
                    else
                    {
                        // Bail out until we see what we expect
                        return;
                    }
                    break;

                case ReadStep.Hash:
                    _revision.Guid = line;

                    List<GitRef> gitRefs;
                    if (_refs.TryGetValue(_revision.Guid, out gitRefs))
                        _revision.Refs.AddRange(gitRefs);

                    break;

                case ReadStep.Parents:
                    _revision.ParentGuids = line.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries);
                    break;

                case ReadStep.Tree:
                    _revision.TreeGuid = line;
                    break;

                case ReadStep.AuthorName:
                    _revision.Author = line;
                    break;

                case ReadStep.AuthorEmail:
                    _revision.AuthorEmail = line;
                    break;

                case ReadStep.AuthorDate:
                    {
                        DateTime dateTime;
                        if (DateTimeUtils.TryParseUnixTime(line, out dateTime))
                            _revision.AuthorDate = dateTime;
                    }
                    break;

                case ReadStep.CommitterName:
                    _revision.Committer = line;
                    break;

                case ReadStep.CommitterEmail:
                    _revision.CommitterEmail = line;
                    break;

                case ReadStep.CommitterDate:
                    {
                        DateTime dateTime;
                        if (DateTimeUtils.TryParseUnixTime(line, out dateTime))
                            _revision.CommitDate = dateTime;
                    }
                    break;

                case ReadStep.CommitMessageEncoding:
                    _revision.MessageEncoding = line;
                    break;

                case ReadStep.CommitMessage:
                    _revision.Message = _module.ReEncodeCommitMessage(line, _revision.MessageEncoding);

                    break;

                case ReadStep.FileName:
                    _revision.Name = line;
                    break;
            }

            _nextStep++;
        }
    }
}
