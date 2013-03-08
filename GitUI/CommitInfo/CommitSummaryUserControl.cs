﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    /// <summary>
    /// TODO: replace with some better looking RTF control (similar to Commit Tab in main window)
    ///       Tried RichTextBox: strangely it does not show any formatting, just plain text.
    /// </summary>
    public partial class CommitSummaryUserControl : GitExtensionsControl
    {
        private readonly TranslationString noRevision = new TranslationString("No revision");
        private readonly TranslationString notAvailable = new TranslationString("n/a");
        private readonly string tagsCaption;
        private readonly string branchesCaption;
        private readonly Color tagsBackColor = Color.LightSteelBlue;
        private readonly Color branchesBackColor = Color.LightSalmon;

        public CommitSummaryUserControl()
        {
            InitializeComponent();
            Translate();
            tagsCaption = labelTagsCaption.Text;
            branchesCaption = labelBranchesCaption.Text;

            _messageY = labelMessage.Location.Y;
            _messageHeight = labelMessage.Height;
            labelMessage.AutoSize = true;
        }

        private GitRevision _revision;

        private readonly int _messageY;
        private readonly int _messageHeight;

        public GitRevision Revision
        {
            get
            {
                return _revision;
            }

            set
            {
                _revision = value;

                groupBox1.Text = Strings.GetCommitHashText() + ": ";
                labelAuthorCaption.Text = Strings.GetAuthorText() + ":";
                labelTagsCaption.Text = tagsCaption;
                labelBranchesCaption.Text = branchesCaption;

                if (Revision != null)
                {
                    groupBox1.Text += string.Format("{0}", Revision.Guid);
                    labelAuthor.Text = string.Format("{0}", Revision.Author);
                    labelDate.Text = string.Format(Strings.GetCommitDateText() + ": {0}", Revision.CommitDate);
                    labelMessage.Text = string.Format("{0}", Revision.Message);
                    
                    var tagList = Revision.Heads.Where(r => r.IsTag);
                    string tagListStr = string.Join(", ", tagList.Select(h => h.LocalName).ToArray());
                    labelTags.Text = string.Format("{0}", tagListStr.IsNullOrEmpty() ? notAvailable.Text : tagListStr);
                    labelTags.BackColor = tagList.Any() ? tagsBackColor : DefaultBackColor;

                    var branchesList = Revision.Heads.Where(r => r.IsHead);
                    string branchesListStr = string.Join(", ", branchesList.Select(h => h.LocalName).ToArray());
                    labelBranches.Text = string.Format("{0}", branchesListStr.IsNullOrEmpty() ? notAvailable.Text : branchesListStr);
                    labelBranches.BackColor = branchesList.Any() ? branchesBackColor : DefaultBackColor;
                }
                else
                {
                    groupBox1.Text += noRevision.Text;
                    labelAuthor.Text = "---";
                    labelDate.Text = "---";
                    labelMessage.Text = "---";
                    labelTags.Text = "---";
                    labelTags.BackColor = DefaultBackColor;
                    labelBranches.Text = "---";
                    labelBranches.BackColor = DefaultBackColor;
                }
            }
        }

        private void labelMessage_SizeChanged(object sender, EventArgs e)
        {
            labelMessage.Location = new Point(
                labelMessage.Location.X,
                (int)(_messageY + _messageHeight / 2.0 - labelMessage.Height / 2.0));
        }

        private void groupBox1_Resize(object sender, EventArgs e)
        {
            labelMessage.MaximumSize = new Size(groupBox1.Width - 15, labelMessage.MaximumSize.Height);
        }

    }
}
