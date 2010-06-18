using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
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
                return new List<GitRevision>( revisions );
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

        private readonly char[] hexChars = "0123456789ABCDEFabcdef".ToCharArray();
        private readonly string COMMIT_BEGIN = "<(__BEGIN_COMMIT__)>"; // Something unlikely to show up in a comment
        private List<GitHead> heads;
        private GitCommands gitGetGraphCommand;
        private uint revisionOrder = 0;

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
            // Don't need to sync this since the other thread that uses it
            // was aborted above.
            if (gitGetGraphCommand != null)
            {
                gitGetGraphCommand.Kill();
            }
        }

        public string LogParam = "HEAD --all";

        public void Execute()
        {
            revisions.Clear();

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

            string arguments = String.Format(CultureInfo.InvariantCulture,
                "log --pretty=format:\"{1}\" {0}",
                LogParam,
                formatString);

            gitGetGraphCommand = new GitCommands();
            gitGetGraphCommand.CollectOutput = false;
            gitGetGraphCommand.CmdStartProcess(Settings.GitCommand, arguments);

            gitGetGraphCommand.DataReceived += new System.Diagnostics.DataReceivedEventHandler(gitGetGraphCommand_DataReceived);
            gitGetGraphCommand.Exited += new EventHandler(gitGetGraphCommand_Exited);
        }

        void gitGetGraphCommand_Exited(object sender, EventArgs e)
        {
            if (Exited != null)
            {
                Exited(this, e);
            }
        }

        void gitGetGraphCommand_DataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }
            switch (nextStep)
            {
                case ReadStep.Commit:
                    // Sanity check
                    if (e.Data == COMMIT_BEGIN)
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
                    revision.Guid = e.Data;
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
                    parentGuids.AddRange(e.Data.Split(" \t\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                    revision.ParentGuids = parentGuids;
                    break;

                case ReadStep.Tree:
                    revision.TreeGuid = e.Data;
                    break;

                case ReadStep.AuthorName:
                    revision.Author = e.Data;
                    break;

                case ReadStep.AuthorDate:
                    revision.AuthorDate = DateTime.Parse( e.Data );
                    break;

                case ReadStep.CommitterName:
                    revision.Committer = e.Data;
                    break;

                case ReadStep.CommitterDate:
                    revision.CommitDate = DateTime.Parse( e.Data );
                    break;

                case ReadStep.CommitMessage:
                    revision.Message += e.Data;
                    break;
            }

            nextStep++;

            if (nextStep == ReadStep.Done)
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