﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using GitCommands;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class GitModuleTest
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

            Assert.AreEqual(ObjectId.Parse("957ff3ce9193fec3bd2578378e71676841804935"), result.Lines[0].Commit.ObjectId);
            Assert.AreEqual("# Git Extensions", result.Lines[0].Text);

            Assert.AreEqual(1, result.Lines[0].OriginLineNumber);
            Assert.AreEqual(1, result.Lines[0].FinalLineNumber);

            Assert.AreSame(result.Lines[0].Commit, result.Lines[1].Commit);
            Assert.AreSame(result.Lines[0].Commit, result.Lines[6].Commit);

            Assert.AreEqual(ObjectId.Parse("e3268019c66da7534414e9562ececdee5d455b1b"), result.Lines.Last().Commit.ObjectId);
            Assert.AreEqual("", result.Lines.Last().Text);
        }

        [TestCase(null, null)]
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
                "push --progress \"remote\" from-branch",
                _gitModule.PushCmd("remote", "from-branch", null, ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 0));
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

        [Test]
        public void ParseRefs()
        {
            Assert.IsEmpty(_gitModule.ParseRefs(""));
            Assert.IsEmpty(_gitModule.ParseRefs("Foo"));

            const string refList =
                "69a7c7a40230346778e7eebed809773a6bc45268 refs/heads/master\n" +
                "69a7c7a40230346778e7eebed809773a6bc45268 refs/remotes/origin/master\n" +
                "5303e7114f1896c639dea0231fac522752cc44a2\trefs/remotes/upstream/mono\n" +
                "366dfba1abf6cb98d2934455713f3d190df2ba34\trefs/tags/2.51\n";

            var refs = _gitModule.ParseRefs(refList);

            Assert.AreEqual(4, refs.Count);

            Assert.AreEqual("69a7c7a40230346778e7eebed809773a6bc45268", refs[0].Guid);
            Assert.AreEqual("refs/heads/master", refs[0].CompleteName);
            Assert.AreEqual("master", refs[0].LocalName);
            Assert.AreEqual("", refs[0].Remote);
            Assert.IsTrue(refs[0].IsHead);
            Assert.IsFalse(refs[0].IsRemote);
            Assert.IsFalse(refs[0].IsTag);
            Assert.AreSame(_gitModule, refs[0].Module);

            Assert.AreEqual("69a7c7a40230346778e7eebed809773a6bc45268", refs[1].Guid);
            Assert.AreEqual("refs/remotes/origin/master", refs[1].CompleteName);
            Assert.AreEqual("master", refs[1].LocalName);
            Assert.AreEqual("origin", refs[1].Remote);
            Assert.IsFalse(refs[1].IsHead);
            Assert.IsTrue(refs[1].IsRemote);
            Assert.IsFalse(refs[1].IsTag);
            Assert.AreSame(_gitModule, refs[1].Module);

            Assert.AreEqual("5303e7114f1896c639dea0231fac522752cc44a2", refs[2].Guid);
            Assert.AreEqual("refs/remotes/upstream/mono", refs[2].CompleteName);
            Assert.AreEqual("mono", refs[2].LocalName);
            Assert.AreEqual("upstream", refs[2].Remote);
            Assert.IsFalse(refs[2].IsHead);
            Assert.IsTrue(refs[2].IsRemote);
            Assert.IsFalse(refs[2].IsTag);
            Assert.AreSame(_gitModule, refs[2].Module);

            Assert.AreEqual("366dfba1abf6cb98d2934455713f3d190df2ba34", refs[3].Guid);
            Assert.AreEqual("refs/tags/2.51", refs[3].CompleteName);
            Assert.AreEqual("2.51", refs[3].LocalName);
            Assert.AreEqual("", refs[3].Remote);
            Assert.IsFalse(refs[3].IsHead);
            Assert.IsFalse(refs[3].IsRemote);
            Assert.IsTrue(refs[3].IsTag);
            Assert.AreSame(_gitModule, refs[3].Module);
        }

        [Test]
        public void ParseRemotes()
        {
            var lines = new[]
            {
                "RussKie\tgit://github.com/RussKie/gitextensions.git (fetch)",
                "RussKie\tgit://github.com/RussKie/gitextensions.git (push)",
                "origin\tgit@github.com:drewnoakes/gitextensions.git (fetch)",
                "origin\tgit@github.com:drewnoakes/gitextensions.git (push)",
                "upstream\tgit@github.com:gitextensions/gitextensions.git (fetch)",
                "upstream\tgit@github.com:gitextensions/gitextensions.git (push)",
                "asymmetrical\thttps://github.com/gitextensions/fetch.git (fetch)",
                "asymmetrical\thttps://github.com/gitextensions/push.git (push)",
                "with-space\tc:\\Bare Repo (fetch)",
                "with-space\tc:\\Bare Repo (push)"
            };

            var remotes = GitModule.ParseRemotesInternal(lines);

            Assert.AreEqual(5, remotes.Count);

            Assert.AreEqual("RussKie", remotes[0].Name);
            Assert.AreEqual("git://github.com/RussKie/gitextensions.git", remotes[0].FetchUrl);
            Assert.AreEqual("git://github.com/RussKie/gitextensions.git", remotes[0].PushUrl);

            Assert.AreEqual("origin", remotes[1].Name);
            Assert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[1].FetchUrl);
            Assert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[1].PushUrl);

            Assert.AreEqual("upstream", remotes[2].Name);
            Assert.AreEqual("git@github.com:gitextensions/gitextensions.git", remotes[2].FetchUrl);
            Assert.AreEqual("git@github.com:gitextensions/gitextensions.git", remotes[2].PushUrl);

            Assert.AreEqual("asymmetrical", remotes[3].Name);
            Assert.AreEqual("https://github.com/gitextensions/fetch.git", remotes[3].FetchUrl);
            Assert.AreEqual("https://github.com/gitextensions/push.git", remotes[3].PushUrl);

            Assert.AreEqual("with-space", remotes[4].Name);
            Assert.AreEqual("c:\\Bare Repo", remotes[4].FetchUrl);
            Assert.AreEqual("c:\\Bare Repo", remotes[4].PushUrl);
        }
    }
}
