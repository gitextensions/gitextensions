using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GitCommands
{
    public abstract class RevisionGraphInMemFilter
    {
        public abstract bool PassThru(GitRevision rev);
    }

    public sealed class RevisionGraph : IDisposable
    {
        public event EventHandler Exited;
        public event EventHandler Error;
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

        public bool BackgroundThread { get; set; }
        public bool ShaOnly { get; set; }

        private readonly char[] splitChars = " \t\n".ToCharArray();

        private readonly char[] hexChars = "0123456789ABCDEFabcdef".ToCharArray();

        private const string COMMIT_BEGIN = "<(__BEGIN_COMMIT__)>"; // Something unlikely to show up in a comment

        private List<GitHead> heads;

        private GitCommandsInstance gitGetGraphCommand;

        private Thread backgroundThread;

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
            CommitterDate,
            CommitMessageEncoding,
            CommitMessage,
            FileName,
            Done,
        }

        private ReadStep nextStep = ReadStep.Commit;

        private GitRevision revision;

        public RevisionGraph()
        {
            BackgroundThread = true;
        }

        ~RevisionGraph()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (backgroundThread != null)
            {
                backgroundThread.Abort();
                backgroundThread = null;
            }
            if (gitGetGraphCommand != null)
            {
                gitGetGraphCommand.Kill();
                gitGetGraphCommand = null;
            }
        }

        public string LogParam = "HEAD --all";//--branches --remotes --tags";
        public string BranchFilter = String.Empty;
        public RevisionGraphInMemFilter InMemFilter;
        private string selectedBranchName;

        public void Execute()
        {
            if (BackgroundThread)
            {
                if (backgroundThread != null)
                {
                    backgroundThread.Abort();
                }
                backgroundThread = new Thread(execute) { IsBackground = true };
                backgroundThread.Start();
            }
            else
            {
                execute();
            }
        }

        private void execute()
        {
            try
            {
                RevisionCount = 0;
                heads = GetHeads();

                string formatString =
                    /* <COMMIT>       */ COMMIT_BEGIN + "%n" +
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
                        /* Committer Date          */ "%ct%n" +
                        /* Commit message encoding */ "%e%n" + //there is a bug: git does not recode commit message when format is given
                        /* Commit Message          */ "%s";
                }

                // NOTE:
                // when called from FileHistory and FollowRenamesInFileHistory is enabled the "--name-only" argument is set.
                // the filename is the next line after the commit-format defined above.

                if (Settings.OrderRevisionByDate)
                {
                    LogParam = " --date-order " + LogParam;
                }
                else
                {
                    LogParam = " --topo-order " + LogParam;
                }

                string arguments = String.Format(CultureInfo.InvariantCulture,
                    "log -z {2} --pretty=format:\"{1}\" {0}",
                    LogParam,
                    formatString,
                    BranchFilter);

                gitGetGraphCommand = new GitCommandsInstance();
                gitGetGraphCommand.StreamOutput = true;
                gitGetGraphCommand.CollectOutput = false;                
                Encoding LogOutputEncoding = Settings.LogOutputEncoding;
                gitGetGraphCommand.SetupStartInfoCallback = startInfo =>
                {
                    startInfo.StandardOutputEncoding = Settings.LosslessEncoding;
                    startInfo.StandardErrorEncoding = Settings.LosslessEncoding;
                }; 

                Process p = gitGetGraphCommand.CmdStartProcess(Settings.GitCommand, arguments);

                previousFileName = null;
                if (BeginUpdate != null)
                    BeginUpdate(this, EventArgs.Empty);

                string line;
                do
                {
                    line = p.StandardOutput.ReadLine();
                    //commit message is not encoded by git
                    if (nextStep != ReadStep.CommitMessage)
                        line = GitCommandHelpers.ReEncodeString(line, Settings.LosslessEncoding, LogOutputEncoding);

                    if (line != null)
                    {
                        foreach (string entry in line.Split('\0'))
                        {
                            dataReceived(entry);
                        }
                    }
                } while (line != null);
                finishRevision();
                previousFileName = null;
            }
            catch (ThreadAbortException)
            {
                //Silently ignore this exception...
            }
            catch (Exception ex)
            {
                if (Error != null)
                    Error(this, EventArgs.Empty);
                ExceptionUtils.ShowException(ex, "Cannot load commit log.");
                previousFileName = null;
                return;
            }

            if (Exited != null)
                Exited(this, EventArgs.Empty);
        }

        private List<GitHead> GetHeads()
        {
            var result = Settings.Module.GetHeads(true);
            bool validWorkingDir = Settings.Module.ValidWorkingDir();
            selectedBranchName = validWorkingDir ? Settings.Module.GetSelectedBranch() : string.Empty;
            GitHead selectedHead = result.Find(head => head.Name == selectedBranchName);

            if (selectedHead != null)
            {
                selectedHead.Selected = true;

                GitHead selectedHeadMergeSource =
                    result.Find(head => head.IsRemote
                                        && selectedHead.TrackingRemote == head.Remote
                                        && selectedHead.MergeWith == head.LocalName);

                if (selectedHeadMergeSource != null)
                    selectedHeadMergeSource.SelectedHeadMergeSource = true;
            }

            return result;
        }

        private string previousFileName = null;

        void finishRevision()
        {
            if (revision != null)
            {
                if (revision.Name == null)                
                    revision.Name = previousFileName;
                else
                    previousFileName = revision.Name;
            }
            if (revision == null || revision.Guid.Trim(hexChars).Length == 0)
            {
                if ((revision == null) || (InMemFilter == null) || InMemFilter.PassThru(revision))
                {
                    if (revision != null)
                        RevisionCount++;
                    if (Updated != null)
                        Updated(this, new RevisionGraphUpdatedEventArgs(revision));
                }
            }
            nextStep = ReadStep.Commit;
        }

        void dataReceived(string line)
        {
            if (line == null)
                return;

            if (line == COMMIT_BEGIN)
            {
                // a new commit finalizes the last revision
                finishRevision();

                nextStep = ReadStep.Commit;
            }

            switch (nextStep)
            {
                case ReadStep.Commit:
                    // Sanity check
                    if (line == COMMIT_BEGIN)
                    {
                        revision = new GitRevision(null);
                    }
                    else
                    {
                        // Bail out until we see what we expect
                        return;
                    }
                    break;

                case ReadStep.Hash:
                    revision.Guid = line;
                    for (int i = heads.Count - 1; i >= 0; i--)
                    {
                        if (heads[i].Guid == revision.Guid)
                        {
                            revision.Heads.Add(heads[i]);

                            //Only search for a head once, remove it from list
                            heads.Remove(heads[i]);
                        }
                    }
                    break;

                case ReadStep.Parents:
                    revision.ParentGuids = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    break;

                case ReadStep.Tree:
                    revision.TreeGuid = line;
                    break;

                case ReadStep.AuthorName:
                    revision.Author = line;
                    break;

                case ReadStep.AuthorEmail:
                    revision.AuthorEmail = line;
                    break;

                case ReadStep.AuthorDate:
                    {
                        DateTime dateTime;
                        if (DateTimeUtils.TryParseUnixTime(line, out dateTime))
                            revision.AuthorDate = dateTime;
                    }
                    break;

                case ReadStep.CommitterName:
                    revision.Committer = line;
                    break;

                case ReadStep.CommitterDate:
                    {
                        DateTime dateTime;
                        if (DateTimeUtils.TryParseUnixTime(line, out dateTime))
                            revision.CommitDate = dateTime;
                    }
                    break;

                case ReadStep.CommitMessageEncoding:
                    revision.MessageEncoding = line;
                    break;
                
                case ReadStep.CommitMessage:
                    revision.Message = GitCommandHelpers.ReEncodeCommitMessage(line, revision.MessageEncoding);

                    break;

                case ReadStep.FileName:
                    revision.Name = line;
                    break;
            }

            nextStep++;
        }
    }
}
