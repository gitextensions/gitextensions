using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;

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
        public bool ShaOnly { get; set; }

        private readonly char[] splitChars = " \t\n".ToCharArray();

        private readonly char[] hexChars = "0123456789ABCDEFabcdef".ToCharArray();

        private readonly string COMMIT_BEGIN = "<(__BEGIN_COMMIT__)>"; // Something unlikely to show up in a comment

        private List<GitHead> heads;

        private GitCommands gitGetGraphCommand;

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
        public string BranchFilter = String.Empty;

        public void Execute()
        {
            if (BackgroundThread)
            {
                if (backgroundThread != null)
                {
                    backgroundThread.Abort();
                }
                backgroundThread = new Thread(new ThreadStart(execute));
                backgroundThread.IsBackground = true;
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
                /* Parents        */ "%P%n";
            if (!ShaOnly)
            {
                formatString +=
                    /* Tree           */ "%T%n" +
                    /* Author Name    */ "%aN%n" +
                    /* Author Date    */ "%ai%n" +
                    /* Committer Name */ "%cN%n" +
                    /* Committer Date */ "%ci%n" +
                    /* Commit Message */ "%s";
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

            gitGetGraphCommand = new GitCommands();
            gitGetGraphCommand.StreamOutput = true;
            gitGetGraphCommand.CollectOutput = false;
            Process p = gitGetGraphCommand.CmdStartProcess(Settings.GitCommand, arguments);

            string line;
            do
            {
                line = p.StandardOutput.ReadLine();

                if (line != null)
                {
                    foreach (string entry in line.Split('\0'))
                    {
                        dataReceived(entry);
                    }
                }
            } while (line != null);
            finishRevision();

            Exited(this, new EventArgs());
        }

        void finishRevision()
        {
            lock (revisions)
            {
                if (revision == null || revision.Guid.Trim(hexChars).Length == 0)
                {
                    revisions.Add(revision);
                    Updated(this, new RevisionGraphUpdatedEvent(revision));
                }
                nextStep = ReadStep.Commit;
            }
        }

        void dataReceived(string line)
        {
            if (line == null)
            {
                return;
            }

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
                    for (int i = heads.Count-1; i >=0; i--)
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

                case ReadStep.AuthorDate:
                    {
                        DateTime dateTime;
                        DateTime.TryParse(line, out dateTime);
                        revision.AuthorDate = dateTime;
                    }
                    break;

                case ReadStep.CommitterName:
                    revision.Committer = line;
                    break;

                case ReadStep.CommitterDate:
                    {
                        DateTime dateTime;
                        DateTime.TryParse(line, out dateTime);
                        revision.CommitDate = dateTime;
                    }
                    break;

                case ReadStep.CommitMessage:
                    //We need to recode the commit message because of a bug in Git.
                    //We cannot let git recode the message to Windows-1252 which is
                    //needed to allow the "git log" to print the filename in Windows-1252.
                    revision.Message = Encoding.UTF8.GetString(Settings.Encoding.GetBytes(line));
                    //revision.Message = line;
                    break;

                case ReadStep.FileName:
                    revision.Name = line;
                    break;
            }

            nextStep++;
        }
    }
}