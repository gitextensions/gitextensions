using System;
using System.Drawing;
using System.Linq;
using GitCommands;
using ResourceManager;

namespace GitUI.UserControls
{
    /// <summary>
    /// TODO: replace with some better looking RTF control (similar to Commit Tab in main window)
    ///       Tried RichTextBox: strangely it does not show any formatting, just plain text.
    /// </summary>
    public partial class CommitSummaryUserControl : GitExtensionsControl
    {
        private readonly TranslationString _noRevision = new TranslationString("No revision");
        private readonly TranslationString _notAvailable = new TranslationString("n/a");
        private readonly string _tagsCaption;
        private readonly string _branchesCaption;
        private readonly Color _tagsBackColor = Color.LightSteelBlue;
        private readonly Color _branchesBackColor = Color.LightSalmon;

        public CommitSummaryUserControl()
        {
            InitializeComponent();
            InitializeComplete();
            _tagsCaption = labelTagsCaption.Text;
            _branchesCaption = labelBranchesCaption.Text;

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

                labelAuthorCaption.Text = Strings.Author + ":";
                labelTagsCaption.Text = _tagsCaption;
                labelBranchesCaption.Text = _branchesCaption;

                if (Revision != null)
                {
                    groupBox1.Text = Revision.ObjectId.ToShortString();
                    labelAuthor.Text = Revision.Author;
                    labelDate.Text = string.Format(Strings.CommitDate + ": {0}", Revision.CommitDate);
                    labelMessage.Text = Revision.Subject;

                    var tagList = Revision.Refs.Where(r => r.IsTag).ToList();
                    string tagListStr = string.Join(", ", tagList.Select(h => h.LocalName).ToArray());
                    labelTags.Text = tagListStr.IsNullOrEmpty() ? _notAvailable.Text : tagListStr;
                    if (tagList.Any())
                    {
                        labelTags.BackColor = _tagsBackColor;
                        labelTags.ForeColor = ColorHelper.GetForeColorForBackColor(_tagsBackColor);
                    }
                    else
                    {
                        labelTags.Font = new Font(labelTags.Font, FontStyle.Regular);
                    }

                    var branchesList = Revision.Refs.Where(r => r.IsHead).ToList();
                    string branchesListStr = string.Join(", ", branchesList.Select(h => h.LocalName).ToArray());
                    labelBranches.Text = branchesListStr.IsNullOrEmpty() ? _notAvailable.Text : branchesListStr;
                    if (branchesList.Any())
                    {
                        labelBranches.BackColor = _branchesBackColor;
                        labelBranches.ForeColor = ColorHelper.GetForeColorForBackColor(_branchesBackColor);
                    }
                    else
                    {
                        labelBranches.Font = new Font(labelBranches.Font, FontStyle.Regular);
                    }
                }
                else
                {
                    groupBox1.Text = _noRevision.Text;
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
                (int)(_messageY + (_messageHeight / 2.0) - (labelMessage.Height / 2.0)));
        }

        private void groupBox1_Resize(object sender, EventArgs e)
        {
            labelMessage.MaximumSize = new Size(groupBox1.Width - 15, labelMessage.MaximumSize.Height);
        }
    }
}
