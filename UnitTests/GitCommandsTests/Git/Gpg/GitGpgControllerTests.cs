using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GitCommands;
using GitCommands.Gpg;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests.Git.Gpg
{
    [TestFixture]
    public class GitGpgControllerTests
    {
        private Func<IGitModule> _module;
        private GitGpgController _gpgController;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<Func<IGitModule>>();
            _gpgController = new GitGpgController(_module);
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
            var objectId = ObjectId.Random();

            var revision = new GitRevision(objectId);
            var args = new GitArgumentBuilder("log")
            {
                "--pretty=\"format:%G?\"",
                "-1",
                revision.Guid
            };
            _module().GitExecutable.GetOutput(Arg.Is<ArgumentString>(arg => arg.ToString().Equals(args.ToString())))
                .Returns(x => gitCmdReturn);

            var actual = await _gpgController.GetRevisionCommitSignatureStatusAsync(revision);

            Assert.AreEqual(expected, actual);
        }

        [TestCase]
        public void Validate_GetRevisionCommitSignatureStatusAsync_null_revision()
        {
            ((Func<Task>)(() => _gpgController.GetRevisionCommitSignatureStatusAsync(null))).Should().Throw<ArgumentNullException>();
        }

        [TestCase]
        public void Validate_GetRevisionTagSignatureStatusAsync_null_revision()
        {
            ((Func<Task>)(() => _gpgController.GetRevisionTagSignatureStatusAsync(null))).Should().Throw<ArgumentNullException>();
        }

        [TestCase(TagStatus.NoTag, 0)]
        [TestCase(TagStatus.Many, 2)]
        public async Task Validate_GetRevisionTagSignatureStatusAsync(TagStatus tagStatus, int numberOfTags)
        {
            var objectId = ObjectId.Random();

            string gitRefCompleteName = "refs/tags/FirstTag^{}";

            var revision = new GitRevision(objectId)
            {
                Refs = Enumerable.Range(0, numberOfTags)
                    .Select(_ => new GitRef(_module(), objectId, gitRefCompleteName))
                    .ToList()
            };

            var actual = await _gpgController.GetRevisionTagSignatureStatusAsync(revision);

            Assert.AreEqual(tagStatus, actual);
        }

        [TestCase(TagStatus.OneGood, "GOODSIG ... VALIDSIG ...")]
        [TestCase(TagStatus.TagNotSigned, "error: no signature found")]
        [TestCase(TagStatus.NoPubKey, "NO_PUBKEY ...")]
        public async Task Validate_GetRevisionTagSignatureStatusAsync_one_tag(TagStatus tagStatus, string gitCmdReturn)
        {
            var objectId = ObjectId.Random();

            var gitRef = new GitRef(_module(), objectId, "refs/tags/FirstTag^{}");

            var revision = new GitRevision(objectId) { Refs = new[] { gitRef } };
            var args = new GitArgumentBuilder("verify-tag")
            {
                "--raw",
                gitRef.LocalName
            };
            _module().GitExecutable.GetOutput(args).Returns(gitCmdReturn);

            var actual = await _gpgController.GetRevisionTagSignatureStatusAsync(revision);

            Assert.AreEqual(tagStatus, actual);
        }

        [TestCase("return string")]
        public void Validate_GetCommitVerificationMessage(string returnString)
        {
            var objectId = ObjectId.Random();
            var revision = new GitRevision(objectId);
            var args = new GitArgumentBuilder("log")
            {
                "--pretty=\"format:%GG\"",
                "-1",
                revision.Guid
            };
            _module().GitExecutable.GetOutput(Arg.Is<ArgumentString>(arg => arg.Arguments.Equals(args.ToString())))
                .Returns(x => returnString);

            var actual = _gpgController.GetCommitVerificationMessage(revision);

            Assert.AreEqual(returnString, actual);
        }

        [TestCase]
        public void Validate_GetCommitVerificationMessage_null_revision()
        {
            Assert.Throws<ArgumentNullException>(() => _gpgController.GetCommitVerificationMessage(null));
        }

        [TestCase]
        public void Validate_GetTagVerifyMessage_null_revision()
        {
            Assert.Throws<ArgumentNullException>(() => _gpgController.GetTagVerifyMessage(null));
        }

        [TestCase(0, "")]
        [TestCase(1, "TagName")]
        [TestCase(2, "FirstTag\r\nFirstTag\r\n\r\nSecondTag\r\nSecondTag\r\n\r\n")]
        public void Validate_GetTagVerifyMessage(int usefulTagRefNumber, string expected)
        {
            var objectId = ObjectId.Random();
            var revision = new GitRevision(objectId);

            switch (usefulTagRefNumber)
            {
                case 0:
                    {
                        // Tag but not dereference
                        var gitRef = new GitRef(_module(), objectId, "refs/tags/TagName");
                        revision.Refs = new[] { gitRef };

                        var args = new GitArgumentBuilder("verify-tag") { gitRef.LocalName };
                        _module().GitExecutable.GetOutput(args).Returns("");

                        break;
                    }

                case 1:
                    {
                        // One tag that's also IsDereference == true
                        var gitRef = new GitRef(_module(), objectId, "refs/tags/TagName^{}");
                        revision.Refs = new[] { gitRef };

                        var args = new GitArgumentBuilder("verify-tag") { gitRef.LocalName };
                        _module().GitExecutable.GetOutput(args).Returns(gitRef.LocalName);

                        break;
                    }

                case 2:
                    {
                        // Two tag that's also IsDereference == true
                        var gitRef1 = new GitRef(_module(), objectId, "refs/tags/FirstTag^{}");

                        var args = new GitArgumentBuilder("verify-tag") { gitRef1.LocalName };
                        _module().GitExecutable.GetOutput(args).Returns(gitRef1.LocalName);

                        var gitRef2 = new GitRef(_module(), objectId, "refs/tags/SecondTag^{}");
                        revision.Refs = new[] { gitRef1, gitRef2 };

                        args = new GitArgumentBuilder("verify-tag") { gitRef2.LocalName };
                        _module().GitExecutable.GetOutput(args).Returns(gitRef2.LocalName);

                        break;
                    }
            }

            var actual = _gpgController.GetTagVerifyMessage(revision);

            Assert.AreEqual(expected, actual);
        }
    }
}
