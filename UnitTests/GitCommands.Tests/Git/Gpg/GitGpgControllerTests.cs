using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git.Gpg;
using GitCommands.Gpg;
using GitExtUtils;
using GitUIPluginInterfaces;
using NSubstitute;

#pragma warning disable SA1312 // Variable names should begin with lower-case letter (doesn't understand discards)

namespace GitCommandsTests.Git.Gpg
{
    [TestFixture]
    public class GitGpgControllerTests
    {
        private IGitModule _module;
        private GitGpgController _gpgController;
        private GpgSecretKeysParser _gpgSecretKeysParser;
        private MockExecutable _executable;
        private GpgKeyInfo[] _expectedKeys;

        [SetUp]
        public void Setup()
        {
            _executable = new MockExecutable();
            _module = Substitute.For<IGitModule>();
            _module.GitExecutable.Returns(_executable);
            _module.GpgExecutable.Returns(_executable);

            _gpgSecretKeysParser = new GpgSecretKeysParser();
            _gpgController = new GitGpgController(() => _module, _gpgSecretKeysParser);

            _expectedKeys = new GpgKeyInfo[]
            {
                new GpgKeyInfo("076D94226A5AF99D5911E6131C100BD531ED45B4",
                    "1C100BD531ED45B4",
                    "Test User Key <123@123.com>",
                    DateTimeOffset.FromUnixTimeSeconds(1728403200),
                    Capabilities.Sign | Capabilities.Encrypt | Capabilities.Certify,
                    KeyValidity.Valid,
                    false),
                new GpgKeyInfo("076D94226A5AF99D5911E6131C100BD531ED45B4",
                    "1C100BD531ED45B4",
                    "Test User Key <123@123.com>",
                    DateTimeOffset.FromUnixTimeSeconds(1728403200),
                    Capabilities.Sign | Capabilities.Encrypt | Capabilities.Certify,
                    KeyValidity.Valid,
                    false,
                    true)
            };
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

            GitRevision revision = new(objectId);
            GitArgumentBuilder args = new("log")
            {
                "--pretty=\"format:%G?\"",
                "-1",
                revision.Guid
            };

            using var _ = _executable.StageOutput(args.ToString(), gitCmdReturn);

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

            GitRevision revision = new(objectId)
            {
                Refs = Enumerable.Range(0, numberOfTags)
                    .Select(_ => new GitRef(_module, objectId, gitRefCompleteName))
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

            GitRef gitRef = new(_module, objectId, "refs/tags/FirstTag^{}");

            GitRevision revision = new(objectId) { Refs = new[] { gitRef } };
            GitArgumentBuilder args = new("verify-tag")
            {
                "--raw",
                gitRef.LocalName
            };

            using var _ = _executable.StageOutput(args.ToString(), output: "", error: gitCmdReturn);

            var actual = await _gpgController.GetRevisionTagSignatureStatusAsync(revision);

            Assert.AreEqual(tagStatus, actual);
        }

        [TestCase("return string")]
        public void Validate_GetCommitVerificationMessage(string returnString)
        {
            var objectId = ObjectId.Random();
            GitRevision revision = new(objectId);
            GitArgumentBuilder args = new("log")
            {
                "--pretty=\"format:%GG\"",
                "-1",
                revision.Guid
            };

            using var _ = _executable.StageOutput(args.ToString(), returnString);

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
            GitRevision revision = new(objectId);

            IDisposable validate = null;

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

            var actual = _gpgController.GetTagVerifyMessage(revision);

            Assert.AreEqual(expected, actual);

            validate?.Dispose();
        }

        [TestCase(@"sec:u:255:22:1C100BD531ED45B4:1665235438:1728403200::u:::scESC:::+::ed25519:::0:
fpr:::::::::076D94226A5AF99D5911E6131C100BD531ED45B4:
grp:::::::::397ECB64B371A9CE8DB742CACF8A66165399234D:
uid:u::::1665235438::BFF780DF9E4541BED0ECBE237D0B6A86D73FD1F2::Test User Key <123@123.com>::::::::::0:
ssb:u:255:18:54C01383C638A805:1665235438:1728403200:::::e:::+::cv25519::
fpr:::::::::EE2785B28CCB0C768B25C29054C01383C638A805:
grp:::::::::E7CCC28181C1BE739B7BA856C99F0DAB4E6637B6:
", 1, "")]
        [TestCase(@"sec:u:255:22:1C100BD531ED45B4:1665235438:1728403200::u:::scESC:::+::ed25519:::0:
fpr:::::::::076D94226A5AF99D5911E6131C100BD531ED45B4:
grp:::::::::397ECB64B371A9CE8DB742CACF8A66165399234D:
uid:u::::1665235438::BFF780DF9E4541BED0ECBE237D0B6A86D73FD1F2::Test User Key <123@123.com>::::::::::0:
ssb:u:255:18:54C01383C638A805:1665235438:1728403200:::::e:::+::cv25519::
fpr:::::::::EE2785B28CCB0C768B25C29054C01383C638A805:
grp:::::::::E7CCC28181C1BE739B7BA856C99F0DAB4E6637B6:
", 1, "1C100BD531ED45B4")]
        [TestCase(@"sec:u:255:22:1C100BD531ED45B4:1665235438:1728403200::u:::scESC:::+::ed25519:::0:
fpr:076D94226A5AF99D5911E6131C100BD531ED45B4:
grp:::::::397ECB64B371A9CE8DB742CACF8A66165399234D:
uid:u::1665235438::BFF780DF9E4541BED0ECBE237D0B6A86D73FD1F2::Test User Key <123@123.com>::::::::0:
ssb:u:255:18:54C01383C638A805:1665235438:1728403200::e::+::cv25519::
fpr:::::::EE2785B28CCB0C768B25C29054C01383C638A805:
grp:::::::E7CCC28181C1BE739B7BA856C99F0DAB4E6637B6:
", 0, "1C100BD531ED45B4")] // invalid but looks like gpg output

        [TestCase("this is not valid gpg output", 0, "")]
        public void Validate_ParseOutput(string output, int expectedCount, string defaultKey)
        {
            var infos = _gpgSecretKeysParser.ParseKeysOutput(output, defaultKey);

            Assert.IsNotNull(infos);
            Assert.That(infos.Count() == expectedCount);
            if (expectedCount > 0)
            {
                AssertEx.SequenceEqual(_expectedKeys.Where(e => e.IsDefault == !string.IsNullOrEmpty(defaultKey)), infos);
            }
        }

        [TestCase(@"sec:u:255:22:1C100BD531ED45B4:1665235438:1728403200::u:::scESC:::+::ed25519:::0:
fpr:::::::::076D94226A5AF99D5911E6131C100BD531ED45B4:
grp:::::::::397ECB64B371A9CE8DB742CACF8A66165399234D:
uid:u::::1665235438::BFF780DF9E4541BED0ECBE237D0B6A86D73FD1F2::Test User Key <123@123.com>::::::::::0:
ssb:u:255:18:54C01383C638A805:1665235438:1728403200:::::e:::+::cv25519::
fpr:::::::::EE2785B28CCB0C768B25C29054C01383C638A805:
grp:::::::::E7CCC28181C1BE739B7BA856C99F0DAB4E6637B6:
", 1, "1C100BD531ED45B4")]
        [TestCase(@"sec:u:255:22:1C100BD531ED45B4:1665235438:1728403200::u:::scESC:::+::ed25519:::0:
fpr:::::::::076D94226A5AF99D5911E6131C100BD531ED45B4:
grp:::::::::397ECB64B371A9CE8DB742CACF8A66165399234D:
uid:u::::1665235438::BFF780DF9E4541BED0ECBE237D0B6A86D73FD1F2::Test User Key <123@123.com>::::::::::0:
ssb:u:255:18:54C01383C638A805:1665235438:1728403200:::::e:::+::cv25519::
fpr:::::::::EE2785B28CCB0C768B25C29054C01383C638A805:
grp:::::::::E7CCC28181C1BE739B7BA856C99F0DAB4E6637B6:
", 1, "")]
        [TestCase(@"sec:u:255:22:1C100BD531ED45B4:1665235438:1728403200::u:::scESC:::+::ed25519:::0:
fpr:076D94226A5AF99D5911E6131C100BD531ED45B4:
grp:::::::397ECB64B371A9CE8DB742CACF8A66165399234D:
uid:u::1665235438::BFF780DF9E4541BED0ECBE237D0B6A86D73FD1F2::Test User Key <123@123.com>::::::::0:
ssb:u:255:18:54C01383C638A805:1665235438:1728403200::e::+::cv25519::
fpr:::::::EE2785B28CCB0C768B25C29054C01383C638A805:
grp:::::::E7CCC28181C1BE739B7BA856C99F0DAB4E6637B6:
", 0, "1C100BD531ED45B4")] // invalid but looks like gpg output
        [TestCase(@"sec:u:255:22:1C100BD531ED45B4:1665235438:1728403200::u:::scESC:::+::ed25519:::0:
fpr:076D94226A5AF99D5911E6131C100BD531ED45B4:
grp:::::::397ECB64B371A9CE8DB742CACF8A66165399234D:
uid:u::1665235438::BFF780DF9E4541BED0ECBE237D0B6A86D73FD1F2::Test User Key <123@123.com>::::::::0:
ssb:u:255:18:54C01383C638A805:1665235438:1728403200::e::+::cv25519::
fpr:::::::EE2785B28CCB0C768B25C29054C01383C638A805:
grp:::::::E7CCC28181C1BE739B7BA856C99F0DAB4E6637B6:
", 0, "")] // invalid but looks like gpg output

        [TestCase("this is not valid gpg output", 0, "")]
        [TestCase("this is not valid gpg output", 0, "1C100BD531ED45B4")]
        public async Task Validate_GettingSecretKeys(string output, int expectedCount, string defaultKey)
        {
            var args = new ArgumentBuilder()
            {
                "-K", // Secret Keys
                "--with-colons",
                "--keyid-format LONG"
            };

            using var _ = _executable.StageOutput(args.ToString(), output);
            _module.GetEffectiveGitSetting(SettingKeyString.UserSigningKey, false).Value.Returns(defaultKey);

            var infos = await _gpgController.GetGpgSecretKeysAsync();

            Assert.IsNotNull(infos);
            Assert.That(infos.Count() == expectedCount);
            if (expectedCount > 0)
            {
                AssertEx.SequenceEqual(_expectedKeys.Where(e => e.IsDefault == !string.IsNullOrEmpty(defaultKey)), infos);
            }
        }
    }
}
