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
    /// <summary>
    /// TODO: replace with some better looking RTF control (similar to Commit Tab in main window)
    ///       Tried RichTextBox: strangely it does not show any formatting, just plain text.
    /// </summary>
    public partial class CommitSummaryUserControl : UserControl
    {
        private const string tagsCaption = "Tags";
        private const string branchesCaption = "Branches";
        private readonly Color tagsBackColor = Color.LightSteelBlue;
        private readonly Color branchesBackColor = Color.LightSalmon;

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

                labelCommitCaption.Text = Strings.GetCommitHashText() + ":";
                labelAuthorCaption.Text = Strings.GetAuthorText() + ":";
                labelMessageCaption.Text = Strings.GetMessageText() + ":";
                labelTagsCaption.Text = tagsCaption + ":";
                labelBranchesCaption.Text = branchesCaption + ":";

                if (Revision != null)
                {
                    labelCommit.Text = string.Format("{0}", Revision.Guid);
                    labelAuthor.Text = string.Format("{0}", Revision.Author);
                    labelDate.Text = string.Format(Strings.GetCommitDateText() + ": {0}", Revision.CommitDate);
                    labelMessage.Text = string.Format("{0}", Revision.Message);
                    
                    var tagList = Revision.Heads.Where(r => r.IsTag);
                    string tagListStr = string.Join(", ", tagList.Select(h => h.LocalName).ToArray());
                    labelTags.Text = string.Format("{0}", tagListStr.IsNullOrEmpty() ? "n/a" : tagListStr);
                    labelTags.BackColor = tagList.Any() ? tagsBackColor : Control.DefaultBackColor;

                    var branchesList = Revision.Heads.Where(r => r.IsHead);
                    string branchesListStr = string.Join(", ", branchesList.Select(h => h.LocalName).ToArray());
                    labelBranches.Text = string.Format("{0}", branchesListStr.IsNullOrEmpty() ? "n/a" : branchesListStr);
                    labelBranches.BackColor = branchesList.Any() ? branchesBackColor : Control.DefaultBackColor;
                }
                else
                {
                    labelCommit.Text = "No revision";
                    labelAuthor.Text = "---";
                    labelDate.Text = "---";
                    labelMessage.Text = "---";
                    labelTags.Text = "---";
                    labelTags.BackColor = Control.DefaultBackColor;
                    labelBranches.Text = "---";
                    labelBranches.BackColor = Control.DefaultBackColor;
                }
            }
        }
    }
}
