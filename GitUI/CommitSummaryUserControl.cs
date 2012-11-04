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
                    labelCommit.Text = string.Format(Strings.GetCommitHashText() + ": {0}", Revision.Guid);
                    labelAuthor.Text = string.Format(Strings.GetAuthorText() + ": {0}", Revision.Author);
                    labelDate.Text = string.Format(Strings.GetCommitDateText() + ": {0}", Revision.CommitDate);
                    labelMessage.Text = string.Format(Strings.GetMessageText() + ": {0}", Revision.Message);
                    var tagList = Revision.Heads.Where(r => r.IsTag);
                    string tagListStr = string.Join(", ", tagList.Select(h => h.LocalName).ToArray());
                    labelTags.Text = string.Format("Tags" + ": {0}", tagListStr.IsNullOrEmpty() ? "n/a" : tagListStr);
                }
                else
                {
                    labelCommit.Text = "No revision";
                    labelAuthor.Text = string.Format(Strings.GetAuthorText() + ": {0}", "---");
                    labelDate.Text = string.Format(Strings.GetCommitDateText() + ": {0}", "---");
                    labelMessage.Text = string.Format(Strings.GetMessageText() + ": {0}", "---");
                    labelTags.Text = string.Format("Tags" + ": {0}", "---");
                }
            }
        }
    }
}
