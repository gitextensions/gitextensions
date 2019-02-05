using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class GitModuleTest
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

            _gitModule = new GitModule("", _executable);
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
            GitBlame result;

            using (_executable.StageOutput("rev-parse --git-common-dir", ".git"))
            {
                var path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData/README.blame");

                result = _gitModule.ParseGitBlame(File.ReadAllText(path), Encoding.UTF8);
            }

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
            Assert.IsTrue(GitVersion.Current.FetchCanAskForProgress);

            using (_executable.StageOutput("rev-parse \"refs/heads/remotebranch~0\"", null))
            {
                Assert.AreEqual(
                    "fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags",
                    _gitModule.FetchCmd("remote", "remotebranch", "localbranch").Arguments);
            }

            using (_executable.StageOutput("rev-parse \"refs/heads/remotebranch~0\"", null))
            {
                Assert.AreEqual(
                    "fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --tags",
                    _gitModule.FetchCmd("remote", "remotebranch", "localbranch", true).Arguments);
            }

            using (_executable.StageOutput("rev-parse \"refs/heads/remotebranch~0\"", null))
            {
                Assert.AreEqual(
                    "fetch --progress \"remote\" +remotebranch:refs/heads/localbranch",
                    _gitModule.FetchCmd("remote", "remotebranch", "localbranch", null).Arguments);
            }

            using (_executable.StageOutput("rev-parse \"refs/heads/remotebranch~0\"", null))
            {
                Assert.AreEqual(
                    "fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags --unshallow",
                    _gitModule.FetchCmd("remote", "remotebranch", "localbranch", isUnshallow: true).Arguments);
            }

            using (_executable.StageOutput("rev-parse \"refs/heads/remotebranch~0\"", null))
            {
                Assert.AreEqual(
                    "fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags --prune",
                    _gitModule.FetchCmd("remote", "remotebranch", "localbranch", prune: true).Arguments);
            }
        }

        [Test]
        public void PushAllCmd()
        {
            // TODO test case where this is false
            Assert.IsTrue(GitVersion.Current.PushCanAskForProgress);

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
            using (_executable.StageOutput("rev-parse \"refs/heads/from-branch~0\"", null))
            {
                Assert.AreEqual(
                    "push --progress \"remote\" from-branch",
                    _gitModule.PushCmd("remote", "from-branch", null, ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 0).Arguments);
            }

            using (_executable.StageOutput("rev-parse \"refs/heads/from-branch~0\"", null))
            {
                Assert.AreEqual(
                    "push --progress \"remote\" from-branch:refs/heads/to-branch",
                    _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 0).Arguments);
            }

            using (_executable.StageOutput("rev-parse \"refs/heads/from-branch~0\"", null))
            {
                Assert.AreEqual(
                    "push -f --progress \"remote\" from-branch:refs/heads/to-branch",
                    _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.Force, track: false, recursiveSubmodules: 0).Arguments);
            }

            using (_executable.StageOutput("rev-parse \"refs/heads/from-branch~0\"", null))
            {
                Assert.AreEqual(
                    "push --force-with-lease --progress \"remote\" from-branch:refs/heads/to-branch",
                    _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.ForceWithLease, track: false, recursiveSubmodules: 0).Arguments);
            }

            using (_executable.StageOutput("rev-parse \"refs/heads/from-branch~0\"", null))
            {
                Assert.AreEqual(
                    "push -u --progress \"remote\" from-branch:refs/heads/to-branch",
                    _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: true, recursiveSubmodules: 0).Arguments);
            }

            using (_executable.StageOutput("rev-parse \"refs/heads/from-branch~0\"", null))
            {
                Assert.AreEqual(
                    "push --recurse-submodules=check --progress \"remote\" from-branch:refs/heads/to-branch",
                    _gitModule.PushCmd("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 1).Arguments);
            }

            using (_executable.StageOutput("rev-parse \"refs/heads/from-branch~0\"", null))
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

        [Test]
        public void GetRemotes()
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

            using (_executable.StageOutput("remote -v", string.Join("\n", lines)))
            {
                var remotes = _gitModule.GetRemotes();

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
            var args = new GitArgumentBuilder("log")
            {
                "-n 1",
                "--format=format:%P",
                Sha1
            };

            using (_executable.StageOutput(
                args.ToString(),
                $"{Sha2} {Sha3}"))
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

        [Test]
        public void GetSubmodulesLocalPaths()
        {
            var moduleTestHelpers = new List<CommonTestUtils.GitModuleTestHelper>();
            try
            {
                const int numModules = 4;

                for (int i = 0; i < numModules; ++i)
                {
                    moduleTestHelpers.Add(new CommonTestUtils.GitModuleTestHelper($"repo{i}"));
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
                    parent.Module.GitExecutable.GetOutput(GitCommandHelpers.AddSubmoduleCmd(child.Module.WorkingDir.ToPosixPath(), $"repo{i}", null, true));
                    parent.Module.GitExecutable.GetOutput(@"commit -am ""Add submodule""");
                }

                // Init all modules of root
                var root = moduleTestHelpers[0];
                root.Module.GitExecutable.GetOutput(@"submodule update --init --recursive");

                var paths = root.Module.GetSubmodulesLocalPaths(recursive: true);
                Assert.AreEqual(3, paths.Count);
                Assert.AreEqual(new string[] { "repo1", "repo1/repo2", "repo1/repo2/repo3" }, paths);

                paths = root.Module.GetSubmodulesLocalPaths(recursive: false);
                Assert.AreEqual(1, paths.Count);
                Assert.AreEqual(new string[] { "repo1" }, paths);
            }
            finally
            {
                foreach (var helper in moduleTestHelpers)
                {
                    helper.Dispose();
                }
            }
        }

        [TestCase(false, @"stash list")]
        [TestCase(true, @"--no-optional-locks stash list")]
        public void GetStashesCmd(bool noLocks, string expected)
        {
            Assert.AreEqual(expected, _gitModule.GetStashesCmd(noLocks).ToString());
        }

        [TestCase(@"-c diff.submodule=short -c diff.noprefix=false diff --no-color -M -C --cached extra -- ""new"" ""old""", "new", "old", true, "extra", null, false)]
        [TestCase(@"-c diff.submodule=short -c diff.noprefix=false diff --no-color extra -- ""new""", "new", "old", false, "extra", null, false)]
        [TestCase(@"--no-optional-locks -c diff.submodule=short -c diff.noprefix=false diff --no-color -M -C --cached extra -- ""new"" ""old""", "new", "old", true, "extra", null, true)]
        public void GetCurrentChangesCmd(string expected, string fileName, [CanBeNull] string oldFileName, bool staged,
            string extraDiffArguments, Encoding encoding, bool noLocks)
        {
            Assert.AreEqual(expected, _gitModule.GetCurrentChangesCmd(fileName, oldFileName, staged,
                extraDiffArguments, encoding, noLocks).ToString());
        }

        [TestCase(@"for-each-ref --sort=-committerdate --format=""%(objectname) %(refname)"" refs/heads/", false)]
        [TestCase(@"--no-optional-locks for-each-ref --sort=-committerdate --format=""%(objectname) %(refname)"" refs/heads/", true)]
        public void GetSortedRefsCommand(string expected, bool noLocks)
        {
            Assert.AreEqual(expected, _gitModule.GetSortedRefsCommand(noLocks).ToString());
        }

        [TestCase(@"show-ref --dereference", true, true, false)]
        [TestCase(@"show-ref --tags", true, false, false)]
        [TestCase(@"for-each-ref --sort=-committerdate refs/heads/ --format=""%(objectname) %(refname)""", false, true, false)]
        [TestCase(@"--no-optional-locks for-each-ref --sort=-committerdate refs/heads/ --format=""%(objectname) %(refname)""", false, true, true)]
        public void GetRefsCmd(string expected, bool tags, bool branches, bool noLocks)
        {
            Assert.AreEqual(expected, _gitModule.GetRefsCmd(tags, branches, noLocks).ToString());
        }
    }
}
