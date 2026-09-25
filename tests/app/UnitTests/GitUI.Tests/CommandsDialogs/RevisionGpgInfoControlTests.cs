using GitCommands.Git.Gpg;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitUITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class RevisionGpgInfoControlTests
{
    [SetUp]
    public void Setup()
    {
        GitModuleForm.IsUnitTestActive = true;
    }

    [TearDown]
    public void TearDown()
    {
        GitModuleForm.IsUnitTestActive = false;
    }

    [Test]
    public void Commit_must_not_be_reported_as_unsigned_before_it_was_verified()
    {
        using RevisionGpgInfoControl control = new();

        control.GetTestAccessor().CommitGpgInfoText.Should().Be(TranslatedStrings.LoadingData);
    }

    [Test]
    public void DisplayVerificationPending_must_drop_the_result_of_the_previous_revision()
    {
        using RevisionGpgInfoControl control = new();
        control.DisplayGpgInfo(new GpgInfo(CommitStatus.GoodSignature, "Good signature", TagStatus.NoTag, TagVerificationMessage: null));

        control.DisplayVerificationPending();

        RevisionGpgInfoControl.TestAccessor accessor = control.GetTestAccessor();
        accessor.CommitGpgInfoText.Should().Be(TranslatedStrings.LoadingData);
        accessor.CommitSignPictureVisible.Should().BeFalse();
    }

    [Test]
    public void DisplayGpgInfo_must_report_an_unsigned_commit_once_it_was_verified()
    {
        using RevisionGpgInfoControl control = new();

        control.DisplayGpgInfo(null);

        control.GetTestAccessor().CommitGpgInfoText.Should().Be("Commit is not signed");
    }
}
