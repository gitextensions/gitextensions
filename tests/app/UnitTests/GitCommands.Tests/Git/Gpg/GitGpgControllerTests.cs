using CommonTestUtils;
using GitCommands;
using GitCommands.Git.Gpg;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUIPluginInterfaces;
using NSubstitute;

#pragma warning disable SA1312 // Variable names should begin with lower-case letter (doesn't understand discards)

namespace GitCommandsTests.Git.Gpg;
public class GitGpgControllerTests
{
    private IGitModule _module = null!;
    private GitGpgController _gpgController = null!;
    private MockExecutable _executable = null!;

    [SetUp]
    public void Setup()
    {
        _executable = new MockExecutable();
        _module = Substitute.For<IGitModule>();
        _module.GitExecutable.Returns(_executable);
        _gpgController = new GitGpgController(() => _module);
    }

    [TestCase(CommitStatus.GoodSignature, "G")]
    [TestCase(CommitStatus.SignatureError, "B")]
    [TestCase(CommitStatus.SignatureError, "U")]
    [TestCase(CommitStatus.SignatureError, "X")]
    [TestCase(CommitStatus.SignatureError, "Y")]
    [TestCase(CommitStatus.SignatureError, "R")]
    [TestCase(CommitStatus.MissingPublicKey, "E")]
    [TestCase(CommitStatus.NoSignature, "N")]
    public async Task Validate_GetRevisionCommitSignatureStatusAsync(CommitStatus expected, string gitCmdReturn)
    {
        ObjectId objectId = ObjectId.Random();

        GitRevision revision = new(objectId);
        GitArgumentBuilder args = new("log")
        {
            "--pretty=\"format:%G?\"",
            "-1",
            revision.Guid
        };

        using IDisposable _ = _executable.StageOutput(args.ToString(), gitCmdReturn);

        CommitStatus actual = await _gpgController.GetRevisionCommitSignatureStatusAsync(revision);

        actual.Should().Be(expected);
    }

    [TestCase]
    public void Validate_GetRevisionCommitSignatureStatusAsync_null_revision()
    {
        ((Func<Task>)(() => _gpgController.GetRevisionCommitSignatureStatusAsync(null!))).Should().ThrowAsync<ArgumentNullException>();
    }

    [TestCase]
    public void Validate_GetRevisionTagSignatureStatusAsync_null_revision()
    {
        ((Func<Task>)(() => _gpgController.GetRevisionTagSignatureStatusAsync(null!))).Should().ThrowAsync<ArgumentNullException>();
    }

    [TestCase(TagStatus.NoTag, 0)]
    [TestCase(TagStatus.Many, 2)]
    public async Task Validate_GetRevisionTagSignatureStatusAsync(TagStatus tagStatus, int numberOfTags)
    {
        ObjectId objectId = ObjectId.Random();

        string gitRefCompleteName = "refs/tags/FirstTag^{}";

        GitRevision revision = new(objectId)
        {
            Refs = Enumerable.Range(0, numberOfTags)
                .Select(_ => new GitRef(_module, objectId, gitRefCompleteName))
                .ToList()
        };

        TagStatus actual = await _gpgController.GetRevisionTagSignatureStatusAsync(revision);

        actual.Should().Be(tagStatus);
    }

    [TestCase(TagStatus.OneGood, "GOODSIG ... VALIDSIG ...")]
    [TestCase(TagStatus.TagNotSigned, "error: no signature found")]
    [TestCase(TagStatus.NoPubKey, "NO_PUBKEY ...")]
    public async Task Validate_GetRevisionTagSignatureStatusAsync_one_tag(TagStatus tagStatus, string gitCmdReturn)
    {
        ObjectId objectId = ObjectId.Random();

        GitRef gitRef = new(_module, objectId, "refs/tags/FirstTag^{}");

        GitRevision revision = new(objectId) { Refs = new[] { gitRef } };
        GitArgumentBuilder args = new("verify-tag")
        {
            "--raw",
            gitRef.LocalName
        };

        using IDisposable _ = _executable.StageOutput(args.ToString(), output: "", error: gitCmdReturn);

        TagStatus actual = await _gpgController.GetRevisionTagSignatureStatusAsync(revision);

        actual.Should().Be(tagStatus);
    }

    [TestCase("return string")]
    public void Validate_GetCommitVerificationMessage(string returnString)
    {
        ObjectId objectId = ObjectId.Random();
        GitRevision revision = new(objectId);
        GitArgumentBuilder args = new("log")
        {
            "--pretty=\"format:%GG\"",
            "-1",
            revision.Guid
        };

        using IDisposable _ = _executable.StageOutput(args.ToString(), returnString);

        string actual = _gpgController.GetCommitVerificationMessage(revision);

        actual.Should().Be(returnString);
    }

    [TestCase]
    public void Validate_GetCommitVerificationMessage_null_revision()
    {
        Action act = () => _gpgController.GetCommitVerificationMessage(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [TestCase]
    public void Validate_GetTagVerifyMessage_null_revision()
    {
        Action act = () => _gpgController.GetTagVerifyMessage(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [TestCase(0, "")]
    [TestCase(1, "TagName")]
    [TestCase(2, "FirstTag\r\nFirstTag\r\n\r\nSecondTag\r\nSecondTag\r\n\r\n")]
    public void Validate_GetTagVerifyMessage(int usefulTagRefNumber, string expected)
    {
        ObjectId objectId = ObjectId.Random();
        GitRevision revision = new(objectId);

        IDisposable? validate = null;

        switch (usefulTagRefNumber)
        {
            case 0:
                {
                    // Tag but not dereference
                    GitRef gitRef = new(_module, objectId, "refs/tags/TagName");
                    revision.Refs = new[] { gitRef };

                    break;
                }

            case 1:
                {
                    // One tag that's also IsDereference == true
                    GitRef gitRef = new(_module, objectId, "refs/tags/TagName^{}");
                    revision.Refs = new[] { gitRef };

                    GitArgumentBuilder args = new("verify-tag") { gitRef.LocalName };
                    validate = _executable.StageOutput(args.ToString(), output: "", error: gitRef.LocalName);

                    break;
                }

            case 2:
                {
                    // Two tag that's also IsDereference == true
                    GitRef gitRef1 = new(_module, objectId, "refs/tags/FirstTag^{}");

                    GitArgumentBuilder args = new("verify-tag") { gitRef1.LocalName };
                    _executable.StageOutput(args.ToString(), output: "", error: gitRef1.LocalName);

                    GitRef gitRef2 = new(_module, objectId, "refs/tags/SecondTag^{}");
                    revision.Refs = new[] { gitRef1, gitRef2 };

                    args = new GitArgumentBuilder("verify-tag") { gitRef2.LocalName };
                    validate = _executable.StageOutput(args.ToString(), output: "", error: gitRef2.LocalName);

                    break;
                }
        }

        string? actual = _gpgController.GetTagVerifyMessage(revision);

        actual.Should().Be(expected);

        validate?.Dispose();
    }
}
