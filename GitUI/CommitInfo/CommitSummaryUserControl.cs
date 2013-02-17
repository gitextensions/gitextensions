using System;
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
    public partial class CommitSummaryUserControl : UserControl
    {
        private readonly TranslationString tagsCaption = new TranslationString("Tags");
        private readonly TranslationString branchesCaption = new TranslationString("Branches");
        private readonly TranslationString noRevision = new TranslationString("No revision");
        private readonly TranslationString notAvailable = new TranslationString("n/a");
        private readonly Color tagsBackColor = Color.LightSteelBlue;
        private readonly Color branchesBackColor = Color.LightSalmon;

        public CommitSummaryUserControl()
        {
            InitializeComponent();

            messageY = labelMessage.Location.Y;
            messageHeight = labelMessage.Height;
            labelMessage.AutoSize = true;
        }

        private GitRevision _revision;

        private int messageY;
        private int messageHeight;

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
                labelTagsCaption.Text = tagsCaption.Text + ":";
                labelBranchesCaption.Text = branchesCaption.Text + ":";

                if (Revision != null)
                {
                    groupBox1.Text += string.Format("{0}", Revision.Guid);
                    labelAuthor.Text = string.Format("{0}", Revision.Author);
                    labelDate.Text = string.Format(Strings.GetCommitDateText() + ": {0}", Revision.CommitDate);
                    labelMessage.Text = string.Format("{0}", Revision.Message);
                    
                    var tagList = Revision.Heads.Where(r => r.IsTag);
                    string tagListStr = string.Join(", ", tagList.Select(h => h.LocalName).ToArray());
                    labelTags.Text = string.Format("{0}", tagListStr.IsNullOrEmpty() ? notAvailable.Text : tagListStr);
                    labelTags.BackColor = tagList.Any() ? tagsBackColor : Control.DefaultBackColor;

                    var branchesList = Revision.Heads.Where(r => r.IsHead);
                    string branchesListStr = string.Join(", ", branchesList.Select(h => h.LocalName).ToArray());
                    labelBranches.Text = string.Format("{0}", branchesListStr.IsNullOrEmpty() ? notAvailable.Text : branchesListStr);
                    labelBranches.BackColor = branchesList.Any() ? branchesBackColor : Control.DefaultBackColor;
                }
                else
                {
                    groupBox1.Text += noRevision.Text;
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

        private void labelMessage_SizeChanged(object sender, EventArgs e)
        {
            labelMessage.Location = new Point(
                labelMessage.Location.X,
                (int)(messageY + messageHeight / 2.0 - labelMessage.Height / 2.0));
        }

        private void groupBox1_Resize(object sender, EventArgs e)
        {
            labelMessage.MaximumSize = new Size(groupBox1.Width - 15, labelMessage.MaximumSize.Height);
        }

    }
}
