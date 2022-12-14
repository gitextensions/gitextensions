using System.Diagnostics;
using System.Reflection;
using System.Text;
using ApprovalTests;
using ApprovalTests.Namers;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitExtUtils;
using GitUI;
using GitUIPluginInterfaces;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class GitModuleTests
    {
        private static readonly ObjectId Sha1 = ObjectId.Parse("3183d1e95383c44302d4b25a7c647ee169765bd8");
        private static readonly ObjectId Sha2 = ObjectId.Parse("d12782217535ef00f4f84773d5d33691bbf81d00");
        private static readonly ObjectId Sha3 = ObjectId.Parse("dd678b7160a9a5890c8725e33930947af210c765");

        private GitModule _gitModule;
        private MockExecutable _executable;

        [SetUp]
        public void SetUp()
        {
            _executable = new MockExecutable();

            _gitModule = GetGitModuleWithExecutable(executable: _executable);
        }

        [TearDown]
        public void TearDown()
        {
            _executable.Verify();
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
            GitBlame result = _gitModule.ParseGitBlame(File.ReadAllText(path), Encoding.UTF8);

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
            using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/remotebranch~0\"", null))
            {
                Assert.AreEqual(
                    "-c fetch.parallel=0 -c submodule.fetchJobs=0 fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags",
                    _gitModule.FetchCmd("remote", "remotebranch", "localbranch").Arguments);
            }

            using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/remotebranch~0\"", null))
            {
                Assert.AreEqual(
                    "-c fetch.parallel=0 -c submodule.fetchJobs=0 fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --tags",
                    _gitModule.FetchCmd("remote", "remotebranch", "localbranch", true).Arguments);
            }

            using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/remotebranch~0\"", null))
            {
                Assert.AreEqual(
                    "-c fetch.parallel=0 -c submodule.fetchJobs=0 fetch --progress \"remote\" +remotebranch:refs/heads/localbranch",
                    _gitModule.FetchCmd("remote", "remotebranch", "localbranch", null).Arguments);
            }

            using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/remotebranch~0\"", null))
            {
                Assert.AreEqual(
                    "-c fetch.parallel=0 -c submodule.fetchJobs=0 fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags --unshallow",
                    _gitModule.FetchCmd("remote", "remotebranch", "localbranch", isUnshallow: true).Arguments);
            }

            using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/remotebranch~0\"", null))
            {
                Assert.AreEqual(
                    "-c fetch.parallel=0 -c submodule.fetchJobs=0 fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags --prune --force",
                    _gitModule.FetchCmd("remote", "remotebranch", "localbranch", pruneRemoteBranches: true).Arguments);
            }

            using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/remotebranch~0\"", null))
            {
                Assert.AreEqual(
                    "-c fetch.parallel=0 -c submodule.fetchJobs=0 fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags --prune --force --prune-tags",
                    _gitModule.FetchCmd("remote", "remotebranch", "localbranch", pruneRemoteBranches: true, pruneRemoteBranchesAndTags: true).Arguments);
            }
        }

        [Test]
        public void PushAllCmd()
        {
            Assert.AreEqual(
                "push --progress --all \"remote\"",
                _gitModule.PushAllCmd("remote", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 0).Arguments);
            Assert.AreEqual(
                "push -f --progress --all \"remote\"",
                _gitModule.PushAllCmd("remote", ForcePushOptions.Force, track: false, recursiveSubmodules: 0).Arguments);
            Assert.AreEqual(
                "push --force-with-lease --progress --all \"remote\"",
                _gitModule.PushAllCmd("remote", ForcePushOptions.ForceWithLease, track: false, recursiveSubmodules: 0).Arguments);
            Assert.AreEqual(
                "push -u --progress --all \"remote\"",
                _gitModule.PushAllCmd("remote", ForcePushOptions.DoNotForce, track: true, recursiveSubmodules: 0).Arguments);
            Assert.AreEqual(
                "push --recurse-submodules=check --progress --all \"remote\"",
                _gitModule.PushAllCmd("remote", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 1).Arguments);
            Assert.AreEqual(
                "push --recurse-submodules=on-demand --progress --all \"remote\"",
                _gitModule.PushAllCmd("remote", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 2).Arguments);
        }

        [Test]
        public void PushCmd()
        {
            using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/from-branch~0\"", null))
            {
                Assert.AreEqual(
                    "push --progress \"remote\" from-branch",
                    _gitModule.PushCmd("remote", "from-branch", null, ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 0).Arguments);
            }

            using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/from-branch~0\"", null))
            {
                Assert.AreEqual(
                    "push --progress \"remote\" from-branch:refs/heads/to-branch",
                    _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 0).Arguments);
            }

            using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/from-branch~0\"", null))
            {
                Assert.AreEqual(
                    "push -f --progress \"remote\" from-branch:refs/heads/to-branch",
                    _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.Force, track: false, recursiveSubmodules: 0).Arguments);
            }

            using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/from-branch~0\"", null))
            {
                Assert.AreEqual(
                    "push --force-with-lease --progress \"remote\" from-branch:refs/heads/to-branch",
                    _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.ForceWithLease, track: false, recursiveSubmodules: 0).Arguments);
            }

            using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/from-branch~0\"", null))
            {
                Assert.AreEqual(
                    "push -u --progress \"remote\" from-branch:refs/heads/to-branch",
                    _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: true, recursiveSubmodules: 0).Arguments);
            }

            using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/from-branch~0\"", null))
            {
                Assert.AreEqual(
                    "push --recurse-submodules=check --progress \"remote\" from-branch:refs/heads/to-branch",
                    _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 1).Arguments);
            }

            using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/from-branch~0\"", null))
            {
                Assert.AreEqual(
                    "push --recurse-submodules=on-demand --progress \"remote\" from-branch:refs/heads/to-branch",
                    _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 2).Arguments);
            }
        }

        [Test]
        public void CommitCmd()
        {
            Assert.AreEqual(
                "commit -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: false).Arguments);
            Assert.AreEqual(
                "commit --amend -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: true).Arguments);
            Assert.AreEqual(
                "commit --signoff -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: false, signOff: true).Arguments);
            Assert.AreEqual(
                "commit --author=\"foo\" -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: false, author: "foo").Arguments);
            Assert.AreEqual(
                "commit",
                _gitModule.CommitCmd(amend: false, useExplicitCommitMessage: false).Arguments);
            Assert.AreEqual(
                "commit --no-verify -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: false, noVerify: true).Arguments);
            Assert.AreEqual(
                "commit -S -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: false, gpgSign: true).Arguments);
            Assert.AreEqual(
                "commit -Skey -F \"COMMITMESSAGE\"",
                _gitModule.CommitCmd(amend: false, gpgSign: true, gpgKeyId: "key").Arguments);
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

        [TestCase("branch -a --contains",
            true,
            true,
            "  aaa\n* current\n+ feature/worktree\n  feature/zzz_another\n  remotes/origin/master\n  remotes/origin/current\n  remotes/upstream/master\n",
            new string[] { "aaa", "current", "feature/worktree", "feature/zzz_another", "remotes/origin/master", "remotes/origin/current", "remotes/upstream/master" })]
        [TestCase("branch --contains",
            true,
            false,
            "  aaa\n* current\n+ feature/worktree\n  feature/zzz_another\n",
            new string[] { "aaa", "current", "feature/worktree", "feature/zzz_another" })]
        [TestCase("branch -r --contains",
            false,
            true,
            "remotes/origin/master\n  remotes/origin/current\n  remotes/upstream/master\n",
            new string[] { "remotes/origin/master", "remotes/origin/current", "remotes/upstream/master" })]
        public void GetAllBranchesWhichContainGivenCommit_wellformed(
            string cmd,
            bool getLocal,
            bool getRemote,
            string output,
            string[] expected)
        {
            using (_executable.StageOutput(cmd + " " + Sha1.ToString(), output))
            {
                var result = _gitModule.GetAllBranchesWhichContainGivenCommit(Sha1, getLocal, getRemote);
                Assert.AreEqual(result, expected);
            }
        }

        [TestCase(
            false,
            false,
            new string[] { })]
        public void GetAllBranchesWhichContainGivenCommit_empty(
            bool getLocal,
            bool getRemote,
            string[] expected)
        {
            var result = _gitModule.GetAllBranchesWhichContainGivenCommit(Sha1, getLocal, getRemote);
            Assert.AreEqual(result, expected);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void RevParse_should_return_null_if_invalid(string revisionExpression)
        {
            _gitModule.RevParse(revisionExpression).Should().BeNull();
        }

        [Test]
        public void RevParse_should_return_null_if_revisionExpression_exceeds_260_symbols()
        {
            string revisionExpression = new('a', 261);
            _gitModule.RevParse(revisionExpression).Should().BeNull();
        }

        [Test]
        public void RevParse_should_return_ObjectId_if_revisionExpression_is_valid_hash()
        {
            string revisionExpression = new('1', ObjectId.Sha1CharCount);
            _gitModule.RevParse(revisionExpression).Should().Be(ObjectId.WorkTreeId);
        }

        [Test]
        public void RevParse_should_query_git_and_return_ObjectId_if_get_valid_hash()
        {
            var revisionExpression = "11111";
            using (_executable.StageOutput($"rev-parse --quiet --verify \"{revisionExpression}~0\"", new string('1', ObjectId.Sha1CharCount), 0))
            {
                _gitModule.RevParse(revisionExpression).Should().Be(ObjectId.WorkTreeId);
            }
        }

        [Test]
        public void RevParse_should_query_git_and_return_null_if_invalid_response()
        {
            var revisionExpression = "11111";
            using (_executable.StageOutput($"rev-parse --quiet --verify \"{revisionExpression}~0\"", "foo bar", 0))
            {
                _gitModule.RevParse(revisionExpression).Should().BeNull();
            }
        }

        [Test]
        [TestCase("WorkTree1", StagedStatus.Index,
            "\r\nwarning: LF will be replaced by CRLF in CustomDictionary.xml.\r\nThe file will have its original line endings in your working directory.\r\nwarning: LF will be replaced by CRLF in FxCop.targets.\r\nThe file will have its original line endings in your working directory.\r\nM\0testfile.txt\0")]
        [TestCase("WorkTree2", StagedStatus.Index,
            "\0\r\nwarning: LF will be replaced by CRLF in CustomDictionary.xml.\r\nThe file will have its original line endings in your working directory.\r\nwarning: LF will be replaced by CRLF in FxCop.targets.\r\nThe file will have its original line endings in your working directory.\r\nM\0testfile.txt\0")]
        [TestCase("WorkTree3", StagedStatus.Index,
            "\0\nwarning: LF will be replaced by CRLF in CustomDictionary.xml.\nThe file will have its original line endings in your working directory.\nwarning: LF will be replaced by CRLF in FxCop.targets.\nThe file will have its original line endings in your working directory.\nM\0testfile.txt\0")]
        [TestCase("WorkTree4", StagedStatus.Index,
            "M\0testfile.txt\0\nwarning: LF will be replaced by CRLF in CustomDictionary.xml.\nThe file will have its original line endings in your working directory.\nwarning: LF will be replaced by CRLF in FxCop.targets.\nThe file will have its original line endings in your working directory.\n")]
        [TestCase("fatal error", StagedStatus.None,
            "M\0testfile.txt\0fatal: bad config line 1 in file F:/dev/gc/gitextensions/.git/modules/GitExtensionsDoc/config\nfatal: 'git status --porcelain=2' failed in submodule GitExtensionsDoc\nM\0testfile2.txt\0")]

        [TestCase("Ignore_unmerged_in_conflict_if_revision_is_work_tree", StagedStatus.WorkTree,
            "M\0testfile.txt\0U\0testfile.txt\0")]
        [TestCase("Include_unmerged_in_conflict_if_revision_is_index", StagedStatus.Index,
            "M\0testfile.txt\0U\0testfile2.txt\0")]
        [TestCase("Check_that_the_staged_status_is_None_if_not_IndexWorkTree1", StagedStatus.None,
            "M\0testfile.txt\0U\0testfile2.txt\0")]
        [TestCase("Check_that_the_staged_status_is_None_if_not_IndexWorkTree2", StagedStatus.None,
            "M\0testfile.txt\0U\0testfile2.txt\0")]

        [TestCase("Check that spaces are not trimmed in file names", StagedStatus.None,
            "M\0 no trim space0 \0U\0 no trim space1 \0A\0 no trim space2 \0")]
        [TestCase("Rename_with_spaces", StagedStatus.None,
            "R100\0CONTRIBUTING.md\0 CONTRIBUTI NG.md\0C70\0apa.md\0 apa.md\0A\0 co decov.yml\0D\0CODE_OF_CONDUCT.md\0")]
#if !DEBUG && false
            // This test is for documentation, but as the throw is in a called function, it will not test cleanly
                // Check that the staged status is None if not Index/WorkTree
                // Assertion in Debug, throws in Release
        [TestCase("Check_that_the_staged_status_is_None_if_not_IndexWorkTree3", StagedStatus.None,
            "M\0testfile.txt\0U\0testfile2.txt\0")]
#endif
        public void GetDiffChangedFilesFromString(string testName, StagedStatus stagedStatus, string statusString)
        {
            // TODO produce a valid working directory
            GitModule module = new(Path.GetTempPath());
            using (ApprovalResults.ForScenario(testName.Replace(' ', '_')))
            {
                // git diff --find-renames --find-copies -z --name-status
                var statuses = module.GetTestAccessor().GetDiffChangedFilesFromString(statusString, stagedStatus);
                Approvals.VerifyJson(JsonConvert.SerializeObject(statuses));
            }
        }

        [TestCase("fatal: not a git repository:")]
        [TestCase("error: something went wrong")]
        [TestCase("HEAD")]
        [TestCase("master")]
        public void GetCurrentCheckout_should_query_git_and_return_null_if_response_is_not_sha(string msg)
        {
            using (_executable.StageOutput($"rev-parse HEAD", msg, 0))
            {
                _gitModule.GetCurrentCheckout().Should().BeNull();
            }
        }

        [Test]
        public void GetCurrentCheckout_should_query_git_and_return_sha_for_HEAD()
        {
            ObjectId objectId;
            var headId = "69a7c7a40230346778e7eebed809773a6bc45268";

            using (_executable.StageOutput("rev-parse HEAD", headId))
            {
                objectId = _gitModule.GetCurrentCheckout();
            }

            Assert.AreEqual(headId, objectId.ToString());
        }

        [TestCase("ignorenopush\tgit@github.com:drewnoakes/gitextensions.git (fetch)")]
        [TestCase("ignorenopull\tgit@github.com:drewnoakes/gitextensions.git (push)")]
        public async Task GetRemotes_should_throw_if_not_pairs(string line)
        {
            using (_executable.StageOutput("remote -v", line))
            {
                await AssertEx.ThrowsAsync<System.Exception>(async () => await _gitModule.GetRemotesAsync());
            }
        }

        [Test]
        public void GetRemotes_should_parse_correctly_configured_remotes()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
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
                    "with-space\tc:\\Bare Repo (push)",

                    // A remote may have multiple push URLs, but only a single fetch URL
                    "multi\tgit@github.com:drewnoakes/gitextensions.git (fetch)",
                    "multi\tgit@github.com:drewnoakes/gitextensions.git (push)",
                    "multi\tgit@gitlab.com:drewnoakes/gitextensions.git (push)",

                    "ignoreunknown\tgit@github.com:drewnoakes/gitextensions.git (unknownType)",
                    "ignorenotab git@github.com:drewnoakes/gitextensions.git (fetch)",
                    "ignoremissingtype\tgit@gitlab.com:drewnoakes/gitextensions.git",
                    "git@gitlab.com:drewnoakes/gitextensions.git"
                };

                using (_executable.StageOutput("remote -v", string.Join("\n", lines)))
                {
                    var remotes = await _gitModule.GetRemotesAsync();

                    Assert.AreEqual(6, remotes.Count);

                    Assert.AreEqual("RussKie", remotes[0].Name);
                    Assert.AreEqual("git://github.com/RussKie/gitextensions.git", remotes[0].FetchUrl);
                    Assert.AreEqual(1, remotes[0].PushUrls.Count);
                    Assert.AreEqual("git://github.com/RussKie/gitextensions.git", remotes[0].PushUrls[0]);

                    Assert.AreEqual("origin", remotes[1].Name);
                    Assert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[1].FetchUrl);
                    Assert.AreEqual(1, remotes[1].PushUrls.Count);
                    Assert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[1].PushUrls[0]);

                    Assert.AreEqual("upstream", remotes[2].Name);
                    Assert.AreEqual("git@github.com:gitextensions/gitextensions.git", remotes[2].FetchUrl);
                    Assert.AreEqual(1, remotes[2].PushUrls.Count);
                    Assert.AreEqual("git@github.com:gitextensions/gitextensions.git", remotes[2].PushUrls[0]);

                    Assert.AreEqual("asymmetrical", remotes[3].Name);
                    Assert.AreEqual("https://github.com/gitextensions/fetch.git", remotes[3].FetchUrl);
                    Assert.AreEqual(1, remotes[3].PushUrls.Count);
                    Assert.AreEqual("https://github.com/gitextensions/push.git", remotes[3].PushUrls[0]);

                    Assert.AreEqual("with-space", remotes[4].Name);
                    Assert.AreEqual("c:/Bare Repo", remotes[4].FetchUrl);
                    Assert.AreEqual(1, remotes[4].PushUrls.Count);
                    Assert.AreEqual("c:/Bare Repo", remotes[4].PushUrls[0]);

                    Assert.AreEqual("multi", remotes[5].Name);
                    Assert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[5].FetchUrl);
                    Assert.AreEqual(2, remotes[5].PushUrls.Count);
                    Assert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[5].PushUrls[0]);
                    Assert.AreEqual("git@gitlab.com:drewnoakes/gitextensions.git", remotes[5].PushUrls[1]);
                }
            });
        }

        [Test]
        public void GetRemoteNames()
        {
            var lines = new[] { "RussKie", "origin", "upstream", "asymmetrical", "with-space" };

            using (_executable.StageOutput("remote", string.Join("\n", lines)))
            {
                var remotes = _gitModule.GetRemoteNames();

                Assert.AreEqual(lines, remotes);
            }
        }

        [Test]
        public void GetParents_calls_correct_command_and_parses_response()
        {
            GitArgumentBuilder args = new("rev-parse")
            {
                $"{Sha1}^@"
            };

            using (_executable.StageOutput(
                args.ToString(),
                $"{Sha2}\n{Sha3}\n"))
            {
                var parents = _gitModule.GetParents(Sha1);

                Assert.AreEqual(parents, new[] { Sha2, Sha3 });
            }
        }

        [TestCase(null, "reset --hard --")]
        [TestCase("file.txt", "reset --hard -- \"file.txt\"")]
        public void Reset_with_Hard_should_issue_correct_command_and_parse_response(string file, string args)
        {
            using (_executable.StageCommand(args))
            {
                _gitModule.Reset(ResetMode.Hard, file);
            }
        }

        [Test]
        public void RemoveRemote()
        {
            const string remoteName = "foo";
            const string output = "bar";

            using (_executable.StageOutput($"remote rm \"{remoteName}\"", output))
            {
                Assert.AreEqual(output, _gitModule.RemoveRemote(remoteName));
            }
        }

        [Test]
        public void RenameRemote()
        {
            const string oldName = "foo";
            const string newName = "far";
            const string output = "bar";

            using (_executable.StageOutput($"remote rename \"{oldName}\" \"{newName}\"", output))
            {
                Assert.AreEqual(output, _gitModule.RenameRemote(oldName, newName));
            }
        }

        [Test]
        public void RenameBranch()
        {
            const string oldName = "foo";
            const string newName = "far";
            const string output = "bar";

            using (_executable.StageOutput($"branch -m \"{oldName}\" \"{newName}\"", output))
            {
                Assert.AreEqual(output, _gitModule.RenameBranch(oldName, newName));
            }
        }

        [Test]
        public void AddRemote()
        {
            const string name = "foo";
            const string path = "a\\b\\c";
            const string output = "bar";

            using (_executable.StageOutput($"remote add \"{name}\" \"{path.ToPosixPath()}\"", output))
            {
                Assert.AreEqual(output, _gitModule.AddRemote(name, path));
            }

            Assert.AreEqual("Please enter a name.", _gitModule.AddRemote("", path));
            Assert.AreEqual("Please enter a name.", _gitModule.AddRemote(null, path));
        }

        private static IEnumerable<TestCaseData> StagedStatuses
        {
            get
            {
                var headObjectId = ObjectId.Random();

                yield return new TestCaseData(ObjectId.IndexId, ObjectId.WorkTreeId, ObjectId.IndexId, StagedStatus.WorkTree);

                yield return new TestCaseData(headObjectId, ObjectId.IndexId, headObjectId, StagedStatus.Index);

                yield return new TestCaseData(ObjectId.Random(), ObjectId.Random(), ObjectId.Random(), StagedStatus.None);
                yield return new TestCaseData(ObjectId.Random(), ObjectId.Random(), null, StagedStatus.None);

                // Situations where staged status is unknown
                yield return new TestCaseData(ObjectId.WorkTreeId, ObjectId.Random(), ObjectId.Random(), StagedStatus.Unknown);
                yield return new TestCaseData(ObjectId.Random(), ObjectId.WorkTreeId, ObjectId.IndexId, StagedStatus.Unknown);
                yield return new TestCaseData(ObjectId.IndexId, ObjectId.Random(), ObjectId.Random(), StagedStatus.Unknown);
                yield return new TestCaseData(ObjectId.Random(), ObjectId.IndexId, ObjectId.Random(), StagedStatus.Unknown);
                yield return new TestCaseData(ObjectId.IndexId, ObjectId.Random(), null, StagedStatus.Unknown);
                yield return new TestCaseData(ObjectId.Random(), ObjectId.IndexId, null, StagedStatus.Unknown);
                yield return new TestCaseData(ObjectId.Random(), null, ObjectId.Random(), StagedStatus.Unknown);
                yield return new TestCaseData(null, ObjectId.Random(), ObjectId.Random(), StagedStatus.Unknown);

                // Impossible combinations
                yield return new TestCaseData(ObjectId.Random(), ObjectId.WorkTreeId, ObjectId.Random(), StagedStatus.Unknown);
                yield return new TestCaseData(ObjectId.Random(), ObjectId.Random(), ObjectId.WorkTreeId, StagedStatus.None);
                yield return new TestCaseData(ObjectId.Random(), ObjectId.IndexId, ObjectId.WorkTreeId, StagedStatus.Unknown);
                yield return new TestCaseData(headObjectId, ObjectId.WorkTreeId, headObjectId, StagedStatus.Unknown);
                yield return new TestCaseData(ObjectId.Random(), ObjectId.Random(), ObjectId.IndexId, StagedStatus.None);
            }
        }

        [Test, TestCaseSource(nameof(StagedStatuses))]
        public void GetStagedStatus(ObjectId firstRevision, ObjectId secondRevision, ObjectId parentToSecond, StagedStatus status)
        {
            var stagedStatus = _gitModule.GetTestAccessor().GetStagedStatus(firstRevision, secondRevision, parentToSecond);
            Assert.AreEqual(status, stagedStatus);
        }

        [Test]
        public void GetSubmodulesLocalPaths()
        {
            List<GitModuleTestHelper> moduleTestHelpers = new();
            try
            {
                const int numModules = 4;

                for (int i = 0; i < numModules; ++i)
                {
                    moduleTestHelpers.Add(new GitModuleTestHelper($"repo{i}"));
                    Debug.WriteLine($"Repo[{i}]:{moduleTestHelpers[i].TemporaryPath}");
                }

                foreach (var helper in moduleTestHelpers)
                {
                    // Submodules require at least one commit
                    helper.Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Initial commit""");
                }

                for (int i = numModules - 1; i > 0; --i)
                {
                    var parent = moduleTestHelpers[i - 1];
                    var child = moduleTestHelpers[i];

                    // Add child as submodule of parent
                    parent.AddSubmodule(child, $"repo{i}");
                }

                // Init all modules of root
                var root = moduleTestHelpers[0];
                IEnumerable<GitConfigItem> cfgs = GitCommandHelpers.GetAllowFileConfig();

                root.Module.GitExecutable.Execute(GitCommandHelpers.SubmoduleUpdateCmd(name: null, cfgs));

                var paths = root.Module.GetSubmodulesLocalPaths(recursive: true);
                Assert.AreEqual(new string[] { "repo1", "repo1/repo2", "repo1/repo2/repo3" }, paths, $"Modules: {string.Join(" ", paths)}");

                paths = root.Module.GetSubmodulesLocalPaths(recursive: false);
                Assert.AreEqual(new string[] { "repo1" }, paths, $"Modules: {string.Join(" ", paths)}");
            }
            finally
            {
                foreach (var helper in moduleTestHelpers)
                {
                    helper.Dispose();
                }
            }
        }

        [Test]
        public void Ignore_empty_gitmodules_path()
        {
            using GitModuleTestHelper helper = new();
            helper.CreateFile(helper.Module.WorkingDir, ".gitmodules", @"[submodule ""Externals/NBug""]
    path = Externals/NBug
    url = https://github.com/gitextensions/NBug.git
[submodule ""Externals/Git.hub""]
    url = https://github.com/gitextensions/Git.hub.gitk");
            helper.Module.GetSubmodulesLocalPaths().Should().Equal(new string[] { "Externals/NBug" });
        }

        [Ignore("Flaky test - issue #7681")]
        [Test]
        public void GetSuperprojectCurrentCheckout()
        {
            // Create super and sub repo
            using GitModuleTestHelper moduleTestHelperSuper = new("super repo"),
                                                       moduleTestHelperSub = new("sub repo");

            // Add and init the submodule
            moduleTestHelperSuper.AddSubmodule(moduleTestHelperSub, "sub repo");
            var moduleSub = moduleTestHelperSuper.GetSubmodulesRecursive().ElementAt(0);

            // Commit in submodule
            moduleSub.GitExecutable.GetOutput(@"commit --allow-empty -am ""First commit""");
            string commitRef = moduleSub.GitExecutable.GetOutput("show HEAD").LazySplit('\n').First().LazySplit(' ').Skip(1).First();

            // Update ref in superproject
            moduleTestHelperSuper.Module.GitExecutable.GetOutput(@"add ""sub repo""");
            moduleTestHelperSuper.Module.GitExecutable.GetOutput(@"commit -am ""Update submodule ref""");

            // Assert
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                (char code, ObjectId commitId) = await moduleSub.GetSuperprojectCurrentCheckoutAsync();
                Assert.AreEqual(32, code);
                Assert.AreEqual(commitRef, commitId.ToString());
            });
        }

        [TestCase(false, @"stash list")]
        [TestCase(true, @"--no-optional-locks stash list")]
        public void GetStashesCmd(bool noLocks, string expected)
        {
            Assert.AreEqual(expected, _gitModule.GetStashesCmd(noLocks).ToString());
        }

        [TestCase("", "")] // empty message
        [TestCase("a\r\nb\r\n\r\nc\r\n\r\n\r\n\r\nd", "a\r\nb\r\n\r\nc\r\n\r\nd")] // various amount of new lines between text lines
        [TestCase("\r\n\r\n\r\n\r\na\r\nb\r\n\r\n\r\n\r\n\r\n\r\n", "a\r\nb")] // trimmable message
        [TestCase("a\n\n\nb\r\n\r\nc\r\rd", "a\r\n\r\nb\r\n\r\nc\r\n\r\nd")] // mix of new line types
        [TestCase("Hello, this is a single line message", "Hello, this is a single line message")] // single line message
        [TestCase("1\n2\n3\n4\n5\n6\n7\n8\n9\n10\n11\n12\n13", "1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n9\r\n10\r\n11\r\n12\r\n13")] // message with more than 10 lines (a previous limitation)
        public void GetTagMessage(string tagMessage, string expectedReturnedMessage)
        {
            // add initial tag message
            using ReferenceRepository repo = new();
            repo.CreateAnnotatedTag("test_tag", repo.CommitHash, tagMessage);

            // execute test look-up
            var actualReturnedMessage = repo.Module.GetTagMessage("test_tag");

            // compare result to expectations
            Assert.AreEqual(expectedReturnedMessage, actualReturnedMessage);
        }

        // TODO: add GetTagMessage "sad-path" tests, ones that test what happens if we try to execute it on a non-tag object.
        //       I expect we will need to do some refactoring in order to get such tests to pass (GetTagMessage is not robust at the moment)

        [TestCase(false, @"-c core.safecrlf=false update-index --add --stdin")]
        [TestCase(true, @"update-index --add --stdin")]
        public void UpdateIndexCmd_should_add_core_safecrlf(bool showErrorsWhenStagingFiles, string expected)
        {
            var accessor = _gitModule.GetTestAccessor();

            var actual = accessor.UpdateIndexCmd(showErrorsWhenStagingFiles).ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestCase(new object[] { "123", "567", "output.file", null })]
        [TestCase(new object[] { "123", "567", "output.file", 1 })]
        [TestCase(new object[] { "123", "567", "output.file", 2 })]
        public void Test_FormatPatch(string from, string to, string outputFile, int? start)
        {
            StringBuilder arguments = new();
            arguments.Append("format-patch --find-renames --find-copies --break-rewrites");
            if (start is not null)
            {
                arguments.AppendFormat(" --start-number {0}", start);
            }

            arguments.AppendFormat(" \"{0}\"..\"{1}\" -o \"{2}\"", from, to, outputFile);

            string dummyCommandOutput = "The answer is 42. Just check that the Git arguments are as expected.";

            _executable.StageOutput(arguments.ToString(), dummyCommandOutput);
            _gitModule.FormatPatch(from, to, outputFile, start).Should().Be(dummyCommandOutput);
        }

        [TestCase(new object[] { "", "567", "output.file", null })]
        [TestCase(new object[] { "", "567", "output.file", 1 })]
        [TestCase(new object[] { null, "567", "output.file", 2 })]
        public void Test_FormatPatchInRoot(string from, string to, string outputFile, int? start)
        {
            StringBuilder arguments = new();
            arguments.Append("format-patch --find-renames --find-copies --break-rewrites");
            if (start is not null)
            {
                arguments.AppendFormat(" --start-number {0}", start);
            }

            arguments.AppendFormat(" --root \"{0}\" -o \"{1}\"", to, outputFile);

            string dummyCommandOutput = "The answer is 42. Just check that the Git arguments are as expected.";

            _executable.StageOutput(arguments.ToString(), dummyCommandOutput);
            _gitModule.FormatPatch(from, to, outputFile, start).Should().Be(dummyCommandOutput);
        }

        [TestCase(null, "")]
        [TestCase(new string[] { }, "")]
        public void ResetFiles_should_handle_empty_list(string[] files, string expectedOutput)
        {
            Assert.AreEqual(expectedOutput, _gitModule.ResetFiles(files?.ToList()));
        }

        [TestCase(new string[] { "abc", "def" }, "checkout-index --index --force -- \"abc\" \"def\"")]
        public void ResetFiles_should_work_as_expected(string[] files, string args)
        {
            // Real GitModule is need to access AppSettings.GitCommand static property, avoid exception with dummy GitModule
            using GitModuleTestHelper moduleTestHelper = new();
            var gitModule = GetGitModuleWithExecutable(_executable, module: moduleTestHelper.Module);
            string dummyCommandOutput = "The answer is 42. Just check that the Git arguments are as expected.";
            _executable.StageOutput(args, dummyCommandOutput);
            var result = gitModule.ResetFiles(files.ToList());
            Assert.AreEqual(dummyCommandOutput, result);
        }

        [TestCase(new string[] { "abc", "def" }, "rm -- \"abc\" \"def\"")]
        public void RemoveFiles_shouldWorkAsExpected(string[] files, string args)
        {
            // Real GitModule is need to access AppSettings.GitCommand static property, avoid exception with dummy GitModule
            using GitModuleTestHelper moduleTestHelper = new();
            var gitModule = GetGitModuleWithExecutable(_executable, module: moduleTestHelper.Module);
            string dummyCommandOutput = "The answer is 42. Just check that the Git arguments are as expected.";
            _executable.StageOutput(args, dummyCommandOutput);
            var result = gitModule.RemoveFiles(files.ToList(), false);
            Assert.AreEqual(dummyCommandOutput, result);
        }

        [TestCase(new string[] { }, "")]
        public void RemoveFiles_should_handle_empty_list(string[] files, string expectedOutput)
        {
            Assert.AreEqual(expectedOutput, _gitModule.RemoveFiles(files.ToList(), false));
        }

        [TestCaseSource(nameof(BatchUnstageFilesTestCases))]
        public void BatchUnstageFiles_should_work_as_expected(GitItemStatus[] files, string[] args, bool expectedResult)
        {
            // Real GitModule is need to access AppSettings.GitCommand static property, avoid exception with dummy GitModule
            using GitModuleTestHelper moduleTestHelper = new();
            var gitModule = GetGitModuleWithExecutable(_executable, module: moduleTestHelper.Module);

            foreach (var arg in args)
            {
                _executable.StageCommand(arg);
            }

            var result = gitModule.BatchUnstageFiles(files);
            Assert.AreEqual(expectedResult, result);
        }

        private static IEnumerable<TestCaseData> BatchUnstageFilesTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new GitItemStatus[]
                    {
                        new GitItemStatus("abc2") { IsNew = true },
                        new GitItemStatus("abc2") { IsNew = true, IsDeleted = true },
                        new GitItemStatus("abc2") { IsNew = false },
                        new GitItemStatus("abc3") { IsNew = false, IsRenamed = true, OldName = "def" }
                    },
                    new string[]
                    {
                        "reset \"HEAD\" -- \"abc2\" \"abc3\" \"def\"",
                        "reset -- \"abc2\"",
                        "update-index --force-remove --stdin"
                    },
                    false);

                yield return new TestCaseData(
                    new GitItemStatus[]
                    {
                        new GitItemStatus("abc2") { IsNew = false },
                        new GitItemStatus("abc3") { IsNew = false, IsDeleted = true }
                    },
                    new string[]
                    {
                        "reset \"HEAD\" -- \"abc2\" \"abc3\"",
                    },
                    true);
            }
        }

        /// <summary>
        /// Create a GitModule with mockable GitExecutable
        /// </summary>
        /// <param name="path">Path to the module</param>
        /// <param name="executable">The mock executable</param>
        /// <returns>The GitModule</returns>
        private GitModule GetGitModuleWithExecutable(IExecutable executable, string path = "", GitModule module = null)
        {
            module ??= new GitModule(path);

            typeof(GitModule).GetField("_gitExecutable", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(module, executable);
            GitCommandRunner cmdRunner = new(executable, () => GitModule.SystemEncoding);
            typeof(GitModule).GetField("_gitCommandRunner", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(module, cmdRunner);

            return module;
        }
    }
}
