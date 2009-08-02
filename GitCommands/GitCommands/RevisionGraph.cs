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

            gitGetGraphCommand = new GitCommands();
            gitGetGraphCommand.CollectOutput = false;
            gitGetGraphCommand.CmdStartProcess(Settings.GitDir + "git.cmd", "log -n " + LimitRevisions + " --pretty=format:\"Commit %H %nTree:   %T%nAuthor: %aN %nDate:   " + dateFormat + " %nParents:%P %n%s\" " + LogParam);

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

            //First line found!
            if (e.Data.IndexOf("Commit ") > 0 && e.Data.IndexOf("*") >= 0 || (e.Data.IndexOf("Commit ") == 0))
            {
                revision = new GitRevision();
                Revisions.Add(revision);

                graphIndex = e.Data.IndexOf("Commit ");
                if (e.Data.Length > graphIndex)
                    revision.GraphLines.Add(e.Data.Substring(0, graphIndex));
                revision.Name = revision.Guid = e.Data.Substring(graphIndex + 7).Trim();

                List<GitHead> foundHeads = new List<GitHead>();

                foreach (GitHead h in heads)
                {
                    if (h.Guid == revision.Guid)
                    {
                        revision.Heads.Add(h);
                    }
                }

            }
            else
            if (e.Data.IndexOf("Tree:   ", graphIndex) >= 0)
            {
                revision.TreeGuid = e.Data.Substring(e.Data.IndexOf("Tree:   ", graphIndex) + 8).Trim();
                if (e.Data.Length > graphIndex)
                    revision.GraphLines.Add(e.Data.Substring(0, graphIndex));
            }
            else
            if (e.Data.IndexOf("Merge: ", graphIndex) >= 0)
            {
                //ignore
                if (e.Data.Length > graphIndex)
                    revision.GraphLines.Add(e.Data.Substring(0, graphIndex).Trim());
            }
            else
            if (e.Data.IndexOf("Author: ", graphIndex) >= 0)
            {
                revision.Author = e.Data.Substring(e.Data.IndexOf("Author: ", graphIndex) + 8).Trim();
                if (e.Data.Length > graphIndex)
                    revision.GraphLines.Add(e.Data.Substring(0, graphIndex));
            }
            else
            if (e.Data.IndexOf("Date:   ", graphIndex) >= 0)
            {
                revision.Date = e.Data.Substring(e.Data.IndexOf("Date:   ", graphIndex) + 8).Trim();
                if (e.Data.Length > graphIndex)
                    revision.GraphLines.Add(e.Data.Substring(0, graphIndex));
            }
            else
            if (e.Data.IndexOf("Parents:", graphIndex) >= 0)
            {
                List<string> parentGuids = new List<string>();
                foreach (string s in e.Data.Substring(e.Data.IndexOf("Parents:", graphIndex) + 8).Split(' '))
                {
                    parentGuids.Add(s.Trim());
                }

                revision.ParentGuids = parentGuids;
                if (e.Data.Length > graphIndex)
                    revision.GraphLines.Add(e.Data.Substring(0, graphIndex));
            }
            else
            if (revision != null)
            {
                if (e.Data.Length > graphIndex)
                {
                    if (string.IsNullOrEmpty(revision.Message))
                        revision.Message = e.Data.Substring(graphIndex).Trim() + Environment.NewLine;

                    revision.GraphLines.Add(e.Data.Substring(0, graphIndex));
                }
            }
        
        
        }

    }
}
