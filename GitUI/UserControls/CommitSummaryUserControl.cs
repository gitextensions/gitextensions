using System;
using System.Drawing;
using System.Linq;
using GitCommands;
using ResourceManager;
using ResourceManager.CommitDataRenders;

namespace GitUI.UserControls
{
    /// <summary>
    /// TODO: replace with some better looking RTF control (similar to Commit Tab in main window)
    ///       Tried RichTextBox: strangely it does not show any formatting, just plain text.
    /// </summary>
    public partial class CommitSummaryUserControl : GitExtensionsControl
    {
        private const int MaxBranchTagLength = 75;
        private readonly TranslationString _noRevision = new TranslationString("No revision");
        private readonly TranslationString _notAvailable = new TranslationString("n/a");
        private readonly IDateFormatter _dateFormatter = new DateFormatter();
        private readonly string _tagsCaption;
        private readonly string _branchesCaption;
        private readonly Color _tagsBackColor = Color.LightSteelBlue;
        private readonly Color _branchesBackColor = Color.LightSalmon;
        private GitRevision _revision;

        private readonly int _messageY;
        private readonly int _messageHeight;

        public CommitSummaryUserControl()
        {
            InitializeComponent();
            InitializeComplete();
            _tagsCaption = labelTagsCaption.Text;
            _branchesCaption = labelBranchesCaption.Text;

            _messageY = labelMessage.Location.Y;
            _messageHeight = labelMessage.Height;
            labelMessage.AutoSize = true;

            labelMessage.Font = new Font(labelMessage.Font, FontStyle.Bold);
            labelAuthor.Font = new Font(labelAuthor.Font, FontStyle.Bold);
        }

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
                labelDateCaption.Text = Strings.CommitDate + ":";
                labelTagsCaption.Text = _tagsCaption;
                labelBranchesCaption.Text = _branchesCaption;

                if (Revision != null)
                {
                    groupBox1.Text = Revision.ObjectId.ToShortString();
                    labelAuthor.Text = Revision.Author;
                    labelDate.Text = _dateFormatter.FormatDateAsRelativeLocal(Revision.CommitDate);
                    labelMessage.Text = Revision.Subject;

                    var tagList = Revision.Refs.Where(r => r.IsTag).ToList();
                    if (tagList.Any())
                    {
                        labelTags.BackColor = _tagsBackColor;
                        labelTags.ForeColor = ColorHelper.GetForeColorForBackColor(_tagsBackColor);
                        labelTags.Font = new Font(labelTags.Font, FontStyle.Bold);
                        string tagListStr = string.Join(", ", tagList.Select(h => h.LocalName)).ShortenTo(MaxBranchTagLength);
                        labelTags.Text = tagListStr;
                    }
                    else
                    {
                        labelTags.Text = _notAvailable.Text;
                    }

                    var branchesList = Revision.Refs.Where(r => r.IsHead).ToList();
                    if (branchesList.Any())
                    {
                        labelBranches.BackColor = _branchesBackColor;
                        labelBranches.ForeColor = ColorHelper.GetForeColorForBackColor(_branchesBackColor);
                        labelBranches.Font = new Font(labelBranches.Font, FontStyle.Bold);
                        string branchesListStr = string.Join(", ", branchesList.Select(h => h.LocalName)).ShortenTo(MaxBranchTagLength);
                        labelBranches.Text = branchesListStr;
                    }
                    else
                    {
                        labelBranches.Text = _notAvailable.Text;
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
