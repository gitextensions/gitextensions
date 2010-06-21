using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace GitCommands
{
    public class RevisionGraph
    {
        public event EventHandler Exited;
        public event EventHandler Updated;
        public List<GitRevision> Revisions
        {
            get
            {
                lock (revisions)
                {
                    return new List<GitRevision>(revisions);
                }
            }
        }

        public class RevisionGraphUpdatedEvent : EventArgs
        {
            public RevisionGraphUpdatedEvent(GitRevision revision)
            {
                Revision = revision;
            }

            public GitRevision Revision;
        }

        public bool BackgroundThread { get; set; }

        private readonly char[] hexChars = "0123456789ABCDEFabcdef".ToCharArray();
        private readonly string COMMIT_BEGIN = "<(__BEGIN_COMMIT__)>"; // Something unlikely to show up in a comment
        private List<GitHead> heads;
        private GitCommands gitGetGraphCommand;
        private uint revisionOrder = 0;

        private Thread backgroundThread = null;
        private List<GitRevision> revisions = new List<GitRevision>();

        private enum ReadStep
        {
            Commit,
            Hash,
            Parents,
            Tree,
            AuthorName,
            AuthorDate,
            CommitterName,
            CommitterDate,
            CommitMessage,
            Done,
        }
        private ReadStep nextStep = ReadStep.Commit;
        private GitRevision revision;

        public RevisionGraph()
        {
        }

        ~RevisionGraph()
        {
            Kill();
        }

        public void Kill()
        {
            if (backgroundThread != null)
            {
                backgroundThread.Abort();
            }
            if (gitGetGraphCommand != null)
            {
                gitGetGraphCommand.Kill();
            }
        }

        public string LogParam = "HEAD --all";

        public void Execute()
        {
            if (BackgroundThread)
            {
                if (backgroundThread != null)
                {
                    backgroundThread.Abort();
                }
                backgroundThread = new Thread(new ThreadStart(execute));
                backgroundThread.Start();
            }
            else
            {
                execute();
            }
        }

        private void execute()
        {
            lock (revisions)
            {
                revisions.Clear();
            }

            heads = GitCommands.GetHeads(true);

            string formatString =
                /* <COMMIT>       */ COMMIT_BEGIN + "%n" +
                /* Hash           */ "%H%n" +
                /* Parents        */ "%P%n" +
                /* Tree           */ "%T%n" +
                /* Author Name    */ "%aN%n" +
                /* Author Date    */ "%ai%n" +
                /* Committer Name */ "%cN%n" +
                /* Committer Date */ "%ci%n" +
                /* Commit Message */ "%s";

            if (Settings.OrderRevisionByDate)
            {
                LogParam = " --date-order " + LogParam;
            }
            else
            {
                LogParam = " --topo-order " + LogParam;
            }

            string arguments = String.Format(CultureInfo.InvariantCulture,
                "log --pretty=format:\"{1}\" {0}",
                LogParam,
                formatString);

            gitGetGraphCommand = new GitCommands();
            gitGetGraphCommand.StreamOutput = true;
            gitGetGraphCommand.CollectOutput = false;
            Process p = gitGetGraphCommand.CmdStartProcess(Settings.GitCommand, arguments);

            string line;
            do
            {
                line = p.StandardOutput.ReadLine();
                dataReceived(line);
            } while (line != null);

            Exited(this, new EventArgs());
        }

        void dataReceived(string line)
        {
            if (line == null)
            {
                return;
            }
            switch (nextStep)
            {
                case ReadStep.Commit:
                    // Sanity check
                    if (line == COMMIT_BEGIN)
                    {
                        revision = new GitRevision();
                    }
                    else
                    {
                        // Bail out until we see what we expect
                        return;
                    }
                    break;

                case ReadStep.Hash:
                    revision.Guid = line;
                    foreach (GitHead h in heads)
                    {
                        if (h.Guid == revision.Guid)
                        {
                            revision.Heads.Add(h);
                        }
                    }
                    break;

                case ReadStep.Parents:
                    List<string> parentGuids = new List<string>();
                    parentGuids.AddRange(line.Split(" \t\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                    revision.ParentGuids = parentGuids;
                    break;

                case ReadStep.Tree:
                    revision.TreeGuid = line;
                    break;

                case ReadStep.AuthorName:
                    revision.Author = line;
                    break;

                case ReadStep.AuthorDate:
                    revision.AuthorDate = DateTime.Parse(line);
                    break;

                case ReadStep.CommitterName:
                    revision.Committer = line;
                    break;

                case ReadStep.CommitterDate:
                    revision.CommitDate = DateTime.Parse(line);
                    break;

                case ReadStep.CommitMessage:
                    revision.Message += line;
                    break;
            }

            nextStep++;

            if (nextStep == ReadStep.Done)
            {
                lock (revisions)
                {
                    if (revision == null || revision.Guid.Trim(hexChars).Length == 0)
                    {
                        revision.Order = revisionOrder++;
                        revisions.Add(revision);
                        Updated(this, new RevisionGraphUpdatedEvent(revision));
                    }
                    nextStep = ReadStep.Commit;
                }
            }
        }
    }
}