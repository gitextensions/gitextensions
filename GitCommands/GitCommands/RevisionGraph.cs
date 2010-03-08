using System;
using System.Collections.Generic;
using System.Text;

namespace GitCommands
{
    public class RevisionGraph
    {
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

        private GitCommands gitGetGraphCommand;

        public List<GitRevision> Revisions;
        private char[] graphChars = new char[] { '*', '|', '*', '\\', '/' };
        public int LimitRevisions { get; set; }

        public string LogParam = "HEAD --all";

        public void Execute()
        {
            Revisions = new List<GitRevision>();

            heads = GitCommands.GetHeads(true);

            if (!LogParam.Contains("=") && Settings.ShowRevisionGraph)
                LogParam = " --graph " + LogParam;

            if (Settings.OrderRevisionByDate)
                LogParam = " --date-order " + LogParam;


            string dateFormat;
            if (Settings.RelativeDate)
                dateFormat = "%cr";
            else
                dateFormat = "%cd";

            string limitRevisionsArgument;
            if (LogParam.Contains("--follow"))
                limitRevisionsArgument = "";
            else
                limitRevisionsArgument = " -n " + LimitRevisions;

            gitGetGraphCommand = new GitCommands();
            gitGetGraphCommand.CollectOutput = false;
            gitGetGraphCommand.CmdStartProcess(Settings.GitCommand, "log" + limitRevisionsArgument + " --pretty=format:\"Commit %H %nTree:   %T%nAuthor: %aN %nDate:   " + dateFormat + " %nParents:%P %n%s\" " + LogParam);

            gitGetGraphCommand.DataReceived += new System.Diagnostics.DataReceivedEventHandler(gitGetGraphCommand_DataReceived);
            gitGetGraphCommand.Exited += new EventHandler(gitGetGraphCommand_Exited);

        }

        public event EventHandler Exited;

        void gitGetGraphCommand_Exited(object sender, EventArgs e)
        {
            if (Exited != null)
                Exited(this, e);
        }

        private List<GitHead> heads;
        private GitRevision revision;
        private int graphIndex;
        void gitGetGraphCommand_DataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            if (e.Data.Length <= graphIndex)
                return;

            if (TryParseFields(e.Data))
            {
                AddGraphLine(e.Data);
            }
            else if (revision != null)
            {
                if (!string.IsNullOrEmpty(revision.Message) && e.Data.LastIndexOfAny(new char[] { '|', '*' }) < 0 && !e.Data.StartsWith("..."))
                {
                    revision.Name = e.Data;
                }
                else if (e.Data.Length > graphIndex && !e.Data.StartsWith("..."))
                {
                    if (string.IsNullOrEmpty(revision.Message))
                        revision.Message = e.Data.Substring(graphIndex).Trim() + Environment.NewLine;

                    revision.GraphLines.Add(e.Data.Substring(0, graphIndex));

                }
                else if (e.Data.Length == graphIndex && !e.Data.StartsWith("..."))
                {
                    revision.GraphLines.Add(e.Data.Substring(0, graphIndex));
                }
            }
        }

        private void AddGraphLine(string data)
        {
            if (data.Length > graphIndex)
                revision.GraphLines.Add(data.Substring(0, graphIndex));
        }

        private bool TryParseFields(string data)
        {
            //First line found!
            int commitIndex = data.IndexOf("Commit ");
            if (commitIndex > 0 && data.IndexOf("*") >= 0 || (commitIndex == 0))
            {
                revision = new GitRevision();
                Revisions.Add(revision);

                graphIndex = commitIndex;

                /*revision.Name = */
                revision.Guid = data.Substring(graphIndex + 7).Trim();

                foreach (GitHead h in heads)
                {
                    if (h.Guid == revision.Guid)
                    {
                        revision.Heads.Add(h);
                    }
                }

                return true;
            }

            int treeIndex = data.IndexOf("Tree:   ", graphIndex);
            if (treeIndex >= 0)
            {
                revision.TreeGuid = data.Substring(treeIndex + 8).Trim();
                return true;
            }

            if (data.IndexOf("Merge: ", graphIndex) >= 0)
            {
                //ignore
                return true;
            }

            int authorIndex = data.IndexOf("Author: ", graphIndex);
            if (authorIndex >= 0)
            {
                revision.Author = data.Substring(authorIndex + 8).Trim();
                return true;
            }

            int dateIndex = data.IndexOf("Date:   ", graphIndex);
            if (dateIndex >= 0)
            {
                revision.CommitDate = data.Substring(dateIndex + 8).Trim();
                return true;
            }

            int parentsIndex = data.IndexOf("Parents:", graphIndex);
            if (parentsIndex >= 0)
            {
                List<string> parentGuids = new List<string>();
                foreach (string s in data.Substring(parentsIndex + 8).Split(' '))
                {
                    parentGuids.Add(s.Trim());
                }

                revision.ParentGuids = parentGuids;
                return true;
            }

            return false;
        }
    }
}