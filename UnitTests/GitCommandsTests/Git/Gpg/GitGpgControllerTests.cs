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
            var guid = Guid.NewGuid().ToString("N");

            GitRevision revision = new GitRevision(guid);

            _module().RunGitCmd($"log --pretty=\"format:%G?\" -1 {revision.Guid}").Returns(gitCmdReturn);

            var actual = await _gpgController.GetRevisionCommitSignatureStatusAsync(revision);

            Assert.AreEqual(expected, actual);
        }

        [TestCase]
        public void Validate_GetRevisionCommitSignatureStatusAsync_null_revision()
        {
            ((Func<Task>)(async () => await _gpgController.GetRevisionCommitSignatureStatusAsync(null))).Should().Throw<ArgumentNullException>();
        }

        [TestCase]
        public void Validate_GetRevisionTagSignatureStatusAsync_null_revision()
        {
            ((Func<Task>)(async () => await _gpgController.GetRevisionTagSignatureStatusAsync(null))).Should().Throw<ArgumentNullException>();
        }

        [TestCase(TagStatus.NoTag, 0)]
        [TestCase(TagStatus.Many, 2)]
        public async Task Validate_GetRevisionTagSignatureStatusAsync(TagStatus tagStatus, int numberOfTags)
        {
            var guid = Guid.NewGuid().ToString("N");

            string gitRefCompleteName = "refs/tags/FirstTag^{}";

            var revision = new GitRevision("")
            {
                Refs = Enumerable.Range(0, numberOfTags)
                    .Select(_ => new GitRef(_module(), guid, gitRefCompleteName))
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
            var guid = Guid.NewGuid().ToString("N");

            var gitRef = new GitRef(_module(), guid, "refs/tags/FirstTag^{}");

            var revision = new GitRevision(guid) { Refs = new[] { gitRef } };

            _module().RunGitCmd($"verify-tag --raw {gitRef.LocalName}").Returns(gitCmdReturn);

            var actual = await _gpgController.GetRevisionTagSignatureStatusAsync(revision);

            Assert.AreEqual(tagStatus, actual);
        }

        [TestCase("return string")]
        public void Validate_GetCommitVerificationMessage(string returnString)
        {
            var guid = Guid.NewGuid().ToString("N");

            GitRevision revision = new GitRevision(guid);

            _module().RunGitCmd($"log --pretty=\"format:%GG\" -1 {guid}").Returns(returnString);

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
            var guid = Guid.NewGuid().ToString("N");
            var revision = new GitRevision(guid);

            switch (usefulTagRefNumber)
            {
                case 0:
                {
                    // Tag but not dereference
                    var gitRef = new GitRef(_module(), guid, "refs/tags/TagName");
                    revision.Refs = new[] { gitRef };

                    _module().RunGitCmd($"verify-tag  {gitRef.LocalName}").Returns("");

                    break;
                }

                case 1:
                {
                    // One tag that's also IsDereference == true
                    var gitRef = new GitRef(_module(), guid, "refs/tags/TagName^{}");
                    revision.Refs = new[] { gitRef };

                    _module().RunGitCmd($"verify-tag  {gitRef.LocalName}").Returns(gitRef.LocalName);

                    break;
                }

                case 2:
                {
                    // Two tag that's also IsDereference == true
                    var gitRef1 = new GitRef(_module(), guid, "refs/tags/FirstTag^{}");
                    revision.Refs = new[] { gitRef1 };

                    _module().RunGitCmd($"verify-tag  {gitRef1.LocalName}").Returns(gitRef1.LocalName);

                    var gitRef2 = new GitRef(_module(), guid, "refs/tags/SecondTag^{}");
                    revision.Refs = new[] { gitRef1, gitRef2 };

                    _module().RunGitCmd($"verify-tag  {gitRef2.LocalName}").Returns(gitRef2.LocalName);

                    break;
                }
            }

            var actual = _gpgController.GetTagVerifyMessage(revision);

            Assert.AreEqual(expected, actual);
        }
    }
}
