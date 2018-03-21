using System.IO;
using System.Linq;
using System.Text;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class GitModuleTest
    {
        private GitModule _gitModule;

        [SetUp]
        public void SetUp()
        {
            _gitModule = new GitModule(null);
        }

        [TestCase(@"  ""author <author@mail.com>""  ", @"commit --author=""author <author@mail.com>"" -F ""COMMITMESSAGE""")]
        [TestCase(@"""author <author@mail.com>""", @"commit --author=""author <author@mail.com>"" -F ""COMMITMESSAGE""")]
        [TestCase(@"author <author@mail.com>", @"commit --author=""author <author@mail.com>"" -F ""COMMITMESSAGE""")]
        public void CommitCmdShouldTrimAuthor(string input, string expected)
        {
            var actual = _gitModule.CommitCmd(false, author: input);
            StringAssert.AreEqualIgnoringCase(expected, actual);
        }

        [TestCase(false, false, @"", false, false, false, @"", @"commit")]
        [TestCase(true, false, @"", false, false, false, @"", @"commit --amend")]
        [TestCase(false, true, @"", false, false, false, @"", @"commit --signoff")]
        [TestCase(false, false, @"", true, false, false, @"", @"commit -F ""COMMITMESSAGE""")]
        [TestCase(false, false, @"", false, true, false, @"", @"commit --no-verify")]
        [TestCase(false, false, @"", false, false, false, @"12345678", @"commit")]
        [TestCase(false, false, @"", false, false, true, @"", @"commit -S")]
        [TestCase(false, false, @"", false, false, true, @"      ", @"commit -S")]
        [TestCase(false, false, @"", false, false, true, null, @"commit -S")]
        [TestCase(false, false, @"", false, false, true, @"12345678", @"commit -S12345678")]
        [TestCase(true, true, @"", true, true, true, @"12345678", @"commit --amend --no-verify --signoff -S12345678 -F ""COMMITMESSAGE""")]
        public void CommitCmdTests(bool amend, bool signOff, string author, bool useExplicitCommitMessage, bool noVerify, bool gpgSign, string gpgKeyId, string expected)
        {
            var actual = _gitModule.CommitCmd(amend, signOff, author, useExplicitCommitMessage, noVerify, gpgSign, gpgKeyId);
            StringAssert.AreEqualIgnoringCase(expected, actual);
        }

        [Test]
        public void ParseGitBlame()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData/README.blame");

            var result = _gitModule.ParseGitBlame(File.ReadAllText(path), Encoding.UTF8);

            Assert.AreEqual(80, result.Lines.Count);

            Assert.AreEqual("957ff3ce9193fec3bd2578378e71676841804935", result.Lines[0].Commit.ObjectId);
            Assert.AreEqual("# Git Extensions", result.Lines[0].Text);

            Assert.AreEqual(1, result.Lines[0].OriginLineNumber);
            Assert.AreEqual(1, result.Lines[0].FinalLineNumber);

            Assert.AreSame(result.Lines[0].Commit, result.Lines[1].Commit);
            Assert.AreSame(result.Lines[0].Commit, result.Lines[6].Commit);

            Assert.AreEqual("e3268019c66da7534414e9562ececdee5d455b1b", result.Lines.Last().Commit.ObjectId);
            Assert.AreEqual("", result.Lines.Last().Text);
        }

        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase(" ", " ")]
        [TestCase("Hello, World!", "Hello, World!")]
        [TestCase(@"\353\221\220\353\213\244.txt", "두다.txt")] // escaped octal code points (Korean Hangul in this case)
        [TestCase(@"Invalid byte \777.txt", @"Invalid byte \777.txt")] // 777 is an invalid byte, which is omitted from the output
        [TestCase(@"\353\221\220\353\213\244 \777.txt", @"두다 \777.txt")] // valid and invalid in the same string
        [TestCase(@"\353\221\220\353\213\244\777.txt", @"\353\221\220\353\213\244\777.txt")] // valid and invalid in the same string
        public void UnescapeOctalCodePoints_handles_octal_codes(string input, string expected)
        {
            Assert.AreEqual(expected, GitModule.UnescapeOctalCodePoints(input));
        }

        [Test]
        public void UnescapeOctalCodePoints_returns_same_string_if_nothing_to_escape()
        {
            // If nothing was escaped in the original string, the same string instance is returned.
            const string s = "Hello, World!";

            Assert.AreSame(s, GitModule.UnescapeOctalCodePoints(s));
        }

        [Test]
        public void FetchCmd()
        {
            // TODO test case where this is false
            Assert.IsTrue(GitCommandHelpers.VersionInUse.FetchCanAskForProgress);

            Assert.AreEqual(
                "fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags",
                _gitModule.FetchCmd("remote", "remotebranch", "localbranch"));
            Assert.AreEqual(
                "fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --tags",
                _gitModule.FetchCmd("remote", "remotebranch", "localbranch", true));
            Assert.AreEqual(
                "fetch --progress \"remote\" +remotebranch:refs/heads/localbranch",
                _gitModule.FetchCmd("remote", "remotebranch", "localbranch", null));
            Assert.AreEqual(
                "fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags --unshallow",
                _gitModule.FetchCmd("remote", "remotebranch", "localbranch", isUnshallow: true));
            Assert.AreEqual(
                "fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags --prune",
                _gitModule.FetchCmd("remote", "remotebranch", "localbranch", prune: true));
        }

        [Test]
        public void PushAllCmd()
        {
            // TODO test case where this is false
            Assert.IsTrue(GitCommandHelpers.VersionInUse.PushCanAskForProgress);

            Assert.AreEqual(
                "push --progress --all \"remote\"",
                _gitModule.PushAllCmd("remote", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 0));
            Assert.AreEqual(
                "push -f --progress --all \"remote\"",
                _gitModule.PushAllCmd("remote", ForcePushOptions.Force, track: false, recursiveSubmodules: 0));
            Assert.AreEqual(
                "push --force-with-lease --progress --all \"remote\"",
                _gitModule.PushAllCmd("remote", ForcePushOptions.ForceWithLease, track: false, recursiveSubmodules: 0));
            Assert.AreEqual(
                "push -u --progress --all \"remote\"",
                _gitModule.PushAllCmd("remote", ForcePushOptions.DoNotForce, track: true, recursiveSubmodules: 0));
            Assert.AreEqual(
                "push --recurse-submodules=check --progress --all \"remote\"",
                _gitModule.PushAllCmd("remote", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 1));
            Assert.AreEqual(
                "push --recurse-submodules=on-demand --progress --all \"remote\"",
                _gitModule.PushAllCmd("remote", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 2));
        }

        [Test]
        public void PushCmd()
        {
            Assert.AreEqual(
                "push --progress \"remote\" from-branch:refs/heads/to-branch",
                _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 0));
            Assert.AreEqual(
                "push -f --progress \"remote\" from-branch:refs/heads/to-branch",
                _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.Force, track: false, recursiveSubmodules: 0));
            Assert.AreEqual(
                "push --force-with-lease --progress \"remote\" from-branch:refs/heads/to-branch",
                _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.ForceWithLease, track: false, recursiveSubmodules: 0));
            Assert.AreEqual(
                "push -u --progress \"remote\" from-branch:refs/heads/to-branch",
                _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: true, recursiveSubmodules: 0));
            Assert.AreEqual(
                "push --recurse-submodules=check --progress \"remote\" from-branch:refs/heads/to-branch",
                _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 1));
            Assert.AreEqual(
                "push --recurse-submodules=on-demand --progress \"remote\" from-branch:refs/heads/to-branch",
                _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 2));
        }

        [Test]
        public void CommitCmd()
        {
            Assert.AreEqual(
                "commit -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: false));
            Assert.AreEqual(
                "commit --amend -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: true));
            Assert.AreEqual(
                "commit --signoff -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: false, signOff: true));
            Assert.AreEqual(
                "commit --author=\"foo\" -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: false, author: "foo"));
            Assert.AreEqual(
                "commit",
                _gitModule.CommitCmd(amend: false, useExplicitCommitMessage: false));
            Assert.AreEqual(
                "commit --no-verify -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: false, noVerify: true));
            Assert.AreEqual(
                "commit -S -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: false, gpgSign: true));
            Assert.AreEqual(
                "commit -Skey -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: false, gpgSign: true, gpgKeyId: "key"));
        }
    }
}
