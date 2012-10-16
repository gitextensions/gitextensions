using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class CommitSummaryUserControl : UserControl
    {
        public CommitSummaryUserControl()
        {
            InitializeComponent();
        }

        private GitRevision _revision;

        public GitRevision Revision
        {
            get
            {
                return _revision;
            }

            set
            {
                _revision = value;

                if (Revision != null)
                {
                    Commit.Text = string.Format(Strings.GetCommitHashText() + ": {0}", Revision.Guid);
                    Author.Text = string.Format(Strings.GetAuthorText() + ": {0}", Revision.Author);
                    Date.Text = string.Format(Strings.GetCommitDateText() + ": {0}", Revision.CommitDate);
                    Message.Text = string.Format(Strings.GetMessageText() + ": {0}", Revision.Message);
                }
                else
                {
                    Commit.Text = "No revision";
                    Author.Text = string.Format(Strings.GetAuthorText() + ": {0}", "---");
                    Date.Text = string.Format(Strings.GetCommitDateText() + ": {0}", "---");
                    Message.Text = string.Format(Strings.GetMessageText() + ": {0}", "---");
                }
            }
        }
    }
}
