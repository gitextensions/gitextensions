using Avalonia;
using Avalonia.Controls;
using GitCommands.Git.Gpg;
using GitCommands.Utils;
using GitUI.Properties;

using ResourceManager;

namespace GitUI.CommandsDialogs;

public partial class RevisionGpgInfoControl : GitModuleControl
{
    private readonly TranslationString _commitNotSigned = new("Commit is not signed");
    private readonly TranslationString _tagNotSigned = new("Tag is not signed");

    public RevisionGpgInfoControl()
    {
        InitializeComponent();
        InitializeComplete();

        DisplayVerificationPending();
    }

    public void DisplayGpgInfo(GpgInfo? info)
    {
        // No Commit Signature and No Tag Signature
        if (info is null)
        {
            commitSignPicture.IsVisible = false;
            txtCommitGpgInfo.Text = _commitNotSigned.Text;
            /* TAG section */
            tagSignPicture.IsVisible = false;
            txtTagGpgInfo.IsVisible = false;
        }
        else
        {
            DisplayCommitSignatureStatus(info.CommitStatus);
            string? message = EnvUtils.ReplaceLinuxNewLinesDependingOnPlatform(info.CommitVerificationMessage);
            txtCommitGpgInfo.Text = info.CommitStatus != CommitStatus.NoSignature ? message : _commitNotSigned.Text;

            DisplayTagSignatureStatus(info.TagStatus);
            message = EnvUtils.ReplaceLinuxNewLinesDependingOnPlatform(info.TagVerificationMessage);

            // if there is a not signed tag - show 'not signed' text
            // NoTag case is hidden by ApplyLayout
            txtTagGpgInfo.Text = info.TagStatus != TagStatus.TagNotSigned ? message : _tagNotSigned.Text;
        }

        ApplyLayout();
    }

    private void DisplayCommitSignatureStatus(CommitStatus commitStatus)
    {
        commitSignPicture.Source = commitStatus switch
        {
            CommitStatus.GoodSignature => Images.CommitSignatureOk,
            CommitStatus.MissingPublicKey => Images.CommitSignatureWarning,
            CommitStatus.SignatureError => Images.CommitSignatureError,
            _ => null,
        };
        commitSignPicture.IsVisible = commitSignPicture.Source is not null;
    }

    private void DisplayTagSignatureStatus(TagStatus tagStatus)
    {
        tagSignPicture.Source = tagStatus switch
        {
            TagStatus.OneGood => Images.TagOk,
            TagStatus.OneBad => Images.TagError,
            TagStatus.Many => Images.TagMany,
            TagStatus.NoPubKey => Images.TagWarning,
            _ => null,
        };
        tagSignPicture.IsVisible = tagSignPicture.Source is not null;
        txtTagGpgInfo.IsVisible = tagStatus != TagStatus.NoTag;
    }

    /// <summary>
    ///  Indicates that the signature is still being verified, so that neither the result for the
    ///  previously displayed revision nor the default "not signed" is presented as verified.
    /// </summary>
    public void DisplayVerificationPending()
    {
        commitSignPicture.IsVisible = false;
        txtCommitGpgInfo.Text = TranslatedStrings.LoadingData;
        tagSignPicture.IsVisible = false;

        /* This hides the Tag row in ApplyLayout */
        txtTagGpgInfo.IsVisible = false;

        ApplyLayout();
    }

    private void ApplyLayout()
    {
        tableLayoutPanel1.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
        tableLayoutPanel1.RowDefinitions[1].Height = txtTagGpgInfo.IsVisible
            ? new GridLength(1, GridUnitType.Star)
            : new GridLength(0);
    }
}
