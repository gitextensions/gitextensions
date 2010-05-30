using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GitCommands
{
    public class RevisionGraph
    {
        public event EventHandler Exited;
        public List<GitRevision> Revisions;

        public int LimitRevisions { get; set; }

        private List<GitHead> heads;
        private GitRevision revision;
        private GitCommands gitGetGraphCommand;

        public RevisionGraph()
        {
            LimitRevisions = 200;
        }

        ~RevisionGraph()
        {
            Kill();
        }

        public void Kill()
        {
            if (gitGetGraphCommand != null)
                gitGetGraphCommand.Kill();
        }

        public string LogParam = "HEAD --all";

        public void Execute()
        {
            Revisions = new List<GitRevision>();

            heads = GitCommands.GetHeads(true);
            
            string limitRevisionsArgument;
            if (LogParam.Contains("--follow"))
                limitRevisionsArgument = "";
            else
                limitRevisionsArgument = " -n " + LimitRevisions;

            string arguments = String.Format(CultureInfo.InvariantCulture,
                "log{0} --pretty=format:\"Commit %H %nTree: %T %nAuthor: %aN %nAuthorDate: %ai %nCommitter: %cN %nCommitDate: %ci %nParents: %P %n%s\" {1}",
                limitRevisionsArgument,
                LogParam);

            gitGetGraphCommand = new GitCommands();
            gitGetGraphCommand.CollectOutput = false;
            gitGetGraphCommand.CmdStartProcess(Settings.GitCommand, arguments);

            gitGetGraphCommand.DataReceived += new System.Diagnostics.DataReceivedEventHandler(gitGetGraphCommand_DataReceived);
            gitGetGraphCommand.Exited += new EventHandler(gitGetGraphCommand_Exited);
        }

        void gitGetGraphCommand_Exited(object sender, EventArgs e)
        {
            if (Exited != null)
                Exited(this, e);
        }

        void gitGetGraphCommand_DataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            if (TryParseFields(e.Data))
            {
            }
            else if (revision != null)
            {
                if (!string.IsNullOrEmpty(revision.Message) && e.Data.LastIndexOfAny(new char[] { '|', '*' }) < 0 && !e.Data.StartsWith("..."))
                {
                    revision.Name = e.Data;
                }
                else if (e.Data.Length > 0 && !e.Data.StartsWith("..."))
                {
                    if (string.IsNullOrEmpty(revision.Message))
                        revision.Message = e.Data.Trim() + Environment.NewLine;

                }
            }
        }

        private bool TryParseFields(string data)
        {
            //First line found!
            int commitIndex = data.IndexOf("Commit ");
            if (commitIndex > 0 && data.IndexOf("*") >= 0 || (commitIndex == 0))
            {
                revision = new GitRevision();
                Revisions.Add(revision);

                /*revision.Name = */
                revision.Guid = data.Substring(7).Trim();

                foreach (GitHead h in heads)
                {
                    if (h.Guid == revision.Guid)
                    {
                        revision.Heads.Add(h);
                    }
                }

                return true;
            }

            string treeGuid = GetField(data, "Tree:");
            if (treeGuid != null)
            {
                revision.TreeGuid = treeGuid;
                return true;
            }

            if (GetField(data, "Merge:") != null)
            {
                //ignore
                return true;
            }

            string author = GetField(data, "Author:");
            if (author != null)
            {
                revision.Author = author;
                return true;
            }

            string authorDate = GetField(data, "AuthorDate:");
            if (authorDate != null)
            {
                revision.AuthorDate = authorDate;
                return true;
            }

            string committer = GetField(data, "Committer:");
            if (committer != null)
            {
                revision.Committer = committer;
                return true;
            }

            string commitDate = GetField(data, "CommitDate:");
            if (commitDate != null)
            {
                revision.CommitDate = commitDate;
                return true;
            }

            string parents = GetField(data, "Parents:");
            if (parents != null)
            {
                List<string> parentGuids = new List<string>();
                foreach (string s in parents.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    parentGuids.Add(s.Trim());
                }

                revision.ParentGuids = parentGuids;
                return true;
            }

            return false;
        }

        private string GetField(string data, string header)
        {
            int index = data.IndexOf(header, 0);

            if (index >= 0)
            {
                return data.Substring(index + header.Length).Trim();
            }

            return null;
        }
    }
}