using System.Windows.Forms;
using GitCommands.Gpg;
using GitCommands.Utils;
using GitExtUtils.GitUI;
using GitUI.Properties;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class RevisionGpgInfoControl : GitModuleControl
    {
        private readonly TranslationString _commitNotSigned = new TranslationString("Commit is not signed");
        private readonly TranslationString _tagNotSigned = new TranslationString("Tag is not signed");

        public RevisionGpgInfoControl()
        {
            InitializeComponent();
            InitializeComplete();

            DisplayGpgInfo(null);
        }

        public void DisplayGpgInfo([CanBeNull] GpgInfo info)
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

            return;

            void DisplayCommitSignatureStatus(CommitStatus commitStatus)
            {
                /* COMMIT section */
                switch (commitStatus)
                {
                    case CommitStatus.GoodSignature:
                        commitSignPicture.Image = DpiUtil.Scale(Images.CommitSignatureOk);
                        commitSignPicture.Visible = true;
                        break;
                    case CommitStatus.MissingPublicKey:
                        commitSignPicture.Image = DpiUtil.Scale(Images.CommitSignatureWarning);
                        commitSignPicture.Visible = true;
                        break;
                    case CommitStatus.SignatureError:
                        commitSignPicture.Image = DpiUtil.Scale(Images.CommitSignatureError);
                        commitSignPicture.Visible = true;
                        break;
                    case CommitStatus.NoSignature:
                    default:
                        commitSignPicture.Visible = false;
                        break;
                }
            }

            void DisplayTagSignatureStatus(TagStatus tagStatus)
            {
                /* TAG section */
                switch (tagStatus)
                {
                    case TagStatus.OneGood:
                        tagSignPicture.Image = DpiUtil.Scale(Images.TagOk);
                        tagSignPicture.Visible = true;
                        /* This shows the Tag row in ApplyLayout */
                        txtTagGpgInfo.Visible = true;
                        break;
                    case TagStatus.OneBad:
                        tagSignPicture.Image = DpiUtil.Scale(Images.TagError);
                        tagSignPicture.Visible = true;
                        /* This shows the Tag row in ApplyLayout */
                        txtTagGpgInfo.Visible = true;
                        break;
                    case TagStatus.Many:
                        tagSignPicture.Image = DpiUtil.Scale(Images.TagMany);
                        tagSignPicture.Visible = true;
                        /* This shows the Tag row in ApplyLayout */
                        txtTagGpgInfo.Visible = true;
                        break;
                    case TagStatus.NoPubKey:
                        tagSignPicture.Image = DpiUtil.Scale(Images.TagWarning);
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

            void ApplyLayout()
            {
                float heightRowCommit;
                float heightRowTag;

                if (txtTagGpgInfo.Visible)
                {
                    heightRowCommit = 50f;
                    heightRowTag = 50f;
                }
                else
                {
                    heightRowCommit = 100f;
                    heightRowTag = 0f;
                }

                tableLayoutPanel1.RowStyles[0].SizeType = SizeType.Percent;
                tableLayoutPanel1.RowStyles[1].SizeType = SizeType.Percent;

                tableLayoutPanel1.RowStyles[0].Height = heightRowCommit;
                tableLayoutPanel1.RowStyles[1].Height = heightRowTag;
            }
        }
    }
}
