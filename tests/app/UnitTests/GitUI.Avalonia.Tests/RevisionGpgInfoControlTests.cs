using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitCommands.Git.Gpg;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitExtensionsTests;

[TestFixture]
public sealed class RevisionGpgInfoControlTests
{
    [AvaloniaTest]
    public void Commit_must_not_be_reported_as_unsigned_before_it_was_verified()
    {
        RevisionGpgInfoControl control = new();

        control.FindControl<TextBox>("txtCommitGpgInfo")!.Text.Should().Be(TranslatedStrings.LoadingData);
        control.FindControl<Image>("commitSignPicture")!.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void DisplayVerificationPending_must_drop_the_result_of_the_previous_revision()
    {
        RevisionGpgInfoControl control = new();
        control.DisplayGpgInfo(new GpgInfo(CommitStatus.GoodSignature, "Good signature", TagStatus.OneGood, "Good tag signature"));
        control.FindControl<Image>("commitSignPicture")!.IsVisible.Should().BeTrue();
        control.FindControl<Image>("tagSignPicture")!.IsVisible.Should().BeTrue();

        control.DisplayVerificationPending();

        control.FindControl<TextBox>("txtCommitGpgInfo")!.Text.Should().Be(TranslatedStrings.LoadingData);
        control.FindControl<Image>("commitSignPicture")!.IsVisible.Should().BeFalse();
        control.FindControl<Image>("tagSignPicture")!.IsVisible.Should().BeFalse();
        control.FindControl<TextBox>("txtTagGpgInfo")!.IsVisible.Should().BeFalse();
        control.FindControl<Grid>("tableLayoutPanel1")!.RowDefinitions[1].Height.Value.Should().Be(0);
    }

    [AvaloniaTest]
    public void DisplayGpgInfo_must_report_an_unsigned_commit_once_it_was_verified()
    {
        RevisionGpgInfoControl control = new();

        control.DisplayGpgInfo(null);

        control.FindControl<TextBox>("txtCommitGpgInfo")!.Text.Should().Be("Commit is not signed");
        control.FindControl<Image>("commitSignPicture")!.IsVisible.Should().BeFalse();
    }
}
