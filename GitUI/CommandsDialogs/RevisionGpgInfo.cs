using System.Windows.Forms;
using GitCommands.Gpg;
using GitCommands.Utils;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class RevisionGpgInfo : GitModuleControl
    {
        private readonly TranslationString _commitNotSigned = new TranslationString("Commit is not signed");
        private readonly TranslationString _tagNotSigned = new TranslationString("Tag is not signed");

        public RevisionGpgInfo()
        {
            InitializeComponent();
            Translate();

            DisplayGpgInfo(null);
            this.AdjustForDpiScaling();
        }

        public void DisplayGpgInfo(GpgInfo info)
        {
            // No Commit Signature and No Tag Signature
            if (info == null)
            {
                commitSignPicture.Visible = false;
                txtCommitGpgInfo.Text = _commitNotSigned.Text;
                tagSignPicture.Visible = false;

                /* This hides the Tag row in ApplyLayout */
                txtTagGpgInfo.Visible = false;
            }
            else
            {
                DisplayCommitSignatureStatus(info.CommitStatus);
                var message = EnvUtils.ReplaceLinuxNewLinesDependingOnPlatform(info.CommitVerificationMessage);
                txtCommitGpgInfo.Text = info.CommitStatus != CommitStatus.NoSignature ? message : _commitNotSigned.Text;

                DisplayTagSignatureStatus(info.TagStatus);
                message = EnvUtils.ReplaceLinuxNewLinesDependingOnPlatform(info.TagVerificationMessage);

                // if there is a not signed tag - show 'not signed' text
                // NoTag case is hidden by ApplyLayout
                txtTagGpgInfo.Text = info.TagStatus != TagStatus.TagNotSigned ? message : _tagNotSigned.Text;
            }

            ApplyLayout();
        }

        private void ApplyLayout()
        {
            float heightRowCommit;
            var heightRowTag = 0f;

            if (txtTagGpgInfo.Visible)
            {
                heightRowCommit = 50f;
                heightRowTag = 50f;
            }
            else
            {
                heightRowCommit = 100f;
            }

            tableLayoutPanel1.RowStyles[0].SizeType = SizeType.Percent;
            tableLayoutPanel1.RowStyles[1].SizeType = SizeType.Percent;

            tableLayoutPanel1.RowStyles[0].Height = heightRowCommit;
            tableLayoutPanel1.RowStyles[1].Height = heightRowTag;
        }

        private void DisplayCommitSignatureStatus(CommitStatus commitStatus)
        {
            /* COMMIT section */
            switch (commitStatus)
            {
                case CommitStatus.GoodSignature:
                    commitSignPicture.Image = Properties.Resources.commit_ok;
                    commitSignPicture.Visible = true;
                    break;
                case CommitStatus.MissingPublicKey:
                    commitSignPicture.Image = Properties.Resources.commit_warning;
                    commitSignPicture.Visible = true;
                    break;
                case CommitStatus.SignatureError:
                    commitSignPicture.Image = Properties.Resources.commit_error;
                    commitSignPicture.Visible = true;
                    break;
                case CommitStatus.NoSignature:
                default:
                    commitSignPicture.Visible = false;
                    break;
            }
        }

        private void DisplayTagSignatureStatus(TagStatus tagStatus)
        {
            /* TAG section */
            switch (tagStatus)
            {
                case TagStatus.OneGood:
                    tagSignPicture.Image = Properties.Resources.tag_ok;
                    tagSignPicture.Visible = true;
                    /* This shows the Tag row in ApplyLayout */
                    txtTagGpgInfo.Visible = true;
                    break;
                case TagStatus.OneBad:
                    tagSignPicture.Image = Properties.Resources.tag_error;
                    tagSignPicture.Visible = true;
                    /* This shows the Tag row in ApplyLayout */
                    txtTagGpgInfo.Visible = true;
                    break;
                case TagStatus.Many:
                    tagSignPicture.Image = Properties.Resources.tag_many;
                    tagSignPicture.Visible = true;
                    /* This shows the Tag row in ApplyLayout */
                    txtTagGpgInfo.Visible = true;
                    break;
                case TagStatus.NoPubKey:
                    tagSignPicture.Image = Properties.Resources.tag_warning;
                    tagSignPicture.Visible = true;
                    /* This shows the Tag row in ApplyLayout */
                    txtTagGpgInfo.Visible = true;
                    break;
                case TagStatus.TagNotSigned:
                    tagSignPicture.Visible = false;
                    /* This shows the Tag row in ApplyLayout */
                    txtTagGpgInfo.Visible = true;
                    break;
                case TagStatus.NoTag:
                default:
                    tagSignPicture.Visible = false;
                    txtTagGpgInfo.Visible = false;
                    break;
            }
        }
    }
}
