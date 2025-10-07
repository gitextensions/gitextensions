using System.Diagnostics;
using System.Reflection;
using System.Text;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;

namespace GitCommandsTests;

[TestFixture]
public sealed partial class GitModuleTests
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

    [Test]
    public void ParseGitBlame()
    {
        using IDisposable gitVersion = _executable.StageOutput("--version", "git version 2.46.0");
        using IDisposable configList = _executable.StageOutput("config list --includes --null", null);
        GitVersion.ResetVersion();

        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData/README.blame");
        GitBlame result = _gitModule.ParseGitBlame(File.ReadAllText(path), Encoding.UTF8);

        ClassicAssert.AreEqual(80, result.Lines.Count);

        ClassicAssert.AreEqual(ObjectId.Parse("957ff3ce9193fec3bd2578378e71676841804935"), result.Lines[0].Commit.ObjectId);
        ClassicAssert.AreEqual("# Git Extensions", result.Lines[0].Text);

        ClassicAssert.AreEqual(1, result.Lines[0].OriginLineNumber);
        ClassicAssert.AreEqual(1, result.Lines[0].FinalLineNumber);

        ClassicAssert.AreSame(result.Lines[0].Commit, result.Lines[1].Commit);
        ClassicAssert.AreSame(result.Lines[0].Commit, result.Lines[6].Commit);

        ClassicAssert.AreEqual(ObjectId.Parse("e3268019c66da7534414e9562ececdee5d455b1b"), result.Lines[^1].Commit.ObjectId);
        ClassicAssert.AreEqual("", result.Lines[^1].Text);
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
        ClassicAssert.AreEqual(expected, GitModule.UnescapeOctalCodePoints(input));
    }

    [Test]
    public void UnescapeOctalCodePoints_returns_same_string_if_nothing_to_escape()
    {
        // If nothing was escaped in the original string, the same string instance is returned.
        const string s = "Hello, World!";

        ClassicAssert.AreSame(s, GitModule.UnescapeOctalCodePoints(s));
    }

    [Test]
    public void FetchCmd()
    {
        using IDisposable gitVersion = _executable.StageOutput("--version", "git version 2.46.0");
        using IDisposable configList = _executable.StageOutput("config list --includes --null", null);
        GitVersion.ResetVersion();

        using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/remotebranch~0\"", null))
        {
            ClassicAssert.AreEqual(
                "-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags",
                _gitModule.FetchCmd("remote", "remotebranch", "localbranch").Arguments);
        }

        using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/remotebranch~0\"", null))
        {
            ClassicAssert.AreEqual(
                "-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --tags",
                _gitModule.FetchCmd("remote", "remotebranch", "localbranch", true).Arguments);
        }

        using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/remotebranch~0\"", null))
        {
            ClassicAssert.AreEqual(
                "-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"remote\" +remotebranch:refs/heads/localbranch",
                _gitModule.FetchCmd("remote", "remotebranch", "localbranch", null).Arguments);
        }

        using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/remotebranch~0\"", null))
        {
            ClassicAssert.AreEqual(
                "-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags --unshallow",
                _gitModule.FetchCmd("remote", "remotebranch", "localbranch", isUnshallow: true).Arguments);
        }

        using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/remotebranch~0\"", null))
        {
            ClassicAssert.AreEqual(
                "-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags --prune --force",
                _gitModule.FetchCmd("remote", "remotebranch", "localbranch", pruneRemoteBranches: true).Arguments);
        }

        using (_executable.StageOutput("rev-parse --quiet --verify \"refs/heads/remotebranch~0\"", null))
        {
            ClassicAssert.AreEqual(
                "-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"remote\" +remotebranch:refs/heads/localbranch --no-tags --prune --force --prune-tags",
                _gitModule.FetchCmd("remote", "remotebranch", "localbranch", pruneRemoteBranches: true, pruneRemoteBranchesAndTags: true).Arguments);
        }
    }

    [Test]
    public void ParseRefs()
    {
        ClassicAssert.IsEmpty(_gitModule.ParseRefs(""));
        ClassicAssert.IsEmpty(_gitModule.ParseRefs("Foo"));

        const string refList =
            "69a7c7a40230346778e7eebed809773a6bc45268 refs/heads/master\n" +
            "69a7c7a40230346778e7eebed809773a6bc45268 refs/remotes/origin/master\n" +
            "5303e7114f1896c639dea0231fac522752cc44a2\trefs/remotes/upstream/mono\n" +
            "366dfba1abf6cb98d2934455713f3d190df2ba34\trefs/tags/2.51\n";

        IReadOnlyList<IGitRef> refs = _gitModule.ParseRefs(refList);

        ClassicAssert.AreEqual(4, refs.Count);

        ClassicAssert.AreEqual("69a7c7a40230346778e7eebed809773a6bc45268", refs[0].Guid);
        ClassicAssert.AreEqual("refs/heads/master", refs[0].CompleteName);
        ClassicAssert.AreEqual("master", refs[0].LocalName);
        ClassicAssert.AreEqual("", refs[0].Remote);
        ClassicAssert.IsTrue(refs[0].IsHead);
        ClassicAssert.IsFalse(refs[0].IsRemote);
        ClassicAssert.IsFalse(refs[0].IsTag);
        ClassicAssert.AreSame(_gitModule, refs[0].Module);

        ClassicAssert.AreEqual("69a7c7a40230346778e7eebed809773a6bc45268", refs[1].Guid);
        ClassicAssert.AreEqual("refs/remotes/origin/master", refs[1].CompleteName);
        ClassicAssert.AreEqual("master", refs[1].LocalName);
        ClassicAssert.AreEqual("origin", refs[1].Remote);
        ClassicAssert.IsFalse(refs[1].IsHead);
        ClassicAssert.IsTrue(refs[1].IsRemote);
        ClassicAssert.IsFalse(refs[1].IsTag);
        ClassicAssert.AreSame(_gitModule, refs[1].Module);

        ClassicAssert.AreEqual("5303e7114f1896c639dea0231fac522752cc44a2", refs[2].Guid);
        ClassicAssert.AreEqual("refs/remotes/upstream/mono", refs[2].CompleteName);
        ClassicAssert.AreEqual("mono", refs[2].LocalName);
        ClassicAssert.AreEqual("upstream", refs[2].Remote);
        ClassicAssert.IsFalse(refs[2].IsHead);
        ClassicAssert.IsTrue(refs[2].IsRemote);
        ClassicAssert.IsFalse(refs[2].IsTag);
        ClassicAssert.AreSame(_gitModule, refs[2].Module);

        ClassicAssert.AreEqual("366dfba1abf6cb98d2934455713f3d190df2ba34", refs[3].Guid);
        ClassicAssert.AreEqual("refs/tags/2.51", refs[3].CompleteName);
        ClassicAssert.AreEqual("2.51", refs[3].LocalName);
        ClassicAssert.AreEqual("", refs[3].Remote);
        ClassicAssert.IsFalse(refs[3].IsHead);
        ClassicAssert.IsFalse(refs[3].IsRemote);
        ClassicAssert.IsTrue(refs[3].IsTag);
        ClassicAssert.AreSame(_gitModule, refs[3].Module);
    }

    [TestCase("branch -a --contains",
        true,
        true,
        "  aaa\n* current\n+ feature/worktree\n  feature/zzz_another\n  remotes/origin/master\n  remotes/origin/current\n  remotes/upstream/master\n  a+b",
        new string[] { "aaa", "current", "feature/worktree", "feature/zzz_another", "remotes/origin/master", "remotes/origin/current", "remotes/upstream/master", "a+b" })]
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
            IReadOnlyList<string> result = _gitModule.GetAllBranchesWhichContainGivenCommit(Sha1, getLocal, getRemote, cancellationToken: default);
            ClassicAssert.AreEqual(result, expected);
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
        IReadOnlyList<string> result = _gitModule.GetAllBranchesWhichContainGivenCommit(Sha1, getLocal, getRemote, cancellationToken: default);
        ClassicAssert.AreEqual(result, expected);
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
        string revisionExpression = "11111";
        using (_executable.StageOutput($"rev-parse --quiet --verify \"{revisionExpression}~0\"", new string('1', ObjectId.Sha1CharCount), 0))
        {
            _gitModule.RevParse(revisionExpression).Should().Be(ObjectId.WorkTreeId);
        }
    }

    [Test]
    public void RevParse_should_query_git_and_return_null_if_invalid_response()
    {
        string revisionExpression = "11111";
        using (_executable.StageOutput($"rev-parse --quiet --verify \"{revisionExpression}~0\"", "foo bar", 0))
        {
            _gitModule.RevParse(revisionExpression).Should().BeNull();
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
        string headId = "69a7c7a40230346778e7eebed809773a6bc45268";

        using (_executable.StageOutput("rev-parse HEAD", headId))
        {
            objectId = _gitModule.GetCurrentCheckout();
        }

        ClassicAssert.AreEqual(headId, objectId.ToString());
    }

    [Test]
    public void GetParents_calls_correct_command_and_parses_response()
    {
        GitArgumentBuilder args = new("rev-parse")
        {
            $"{Sha1}^@".Quote()
        };

        using (_executable.StageOutput(
            args.ToString(),
            $"{Sha2}\n{Sha3}\n"))
        {
            IReadOnlyList<ObjectId> parents = _gitModule.GetParents(Sha1);

            ClassicAssert.AreEqual(parents, new[] { Sha2, Sha3 });
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

    private static IEnumerable<TestCaseData> StagedStatuses
    {
        get
        {
            ObjectId headObjectId = ObjectId.Random();

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
        StagedStatus stagedStatus = _gitModule.GetTestAccessor().GetStagedStatus(firstRevision, secondRevision, parentToSecond);
        ClassicAssert.AreEqual(status, stagedStatus);
    }

    [Test]
    public void GetSubmodulesLocalPaths()
    {
        List<GitModuleTestHelper> moduleTestHelpers = [];
        try
        {
            const int numModules = 4;

            for (int i = 0; i < numModules; ++i)
            {
                moduleTestHelpers.Add(new GitModuleTestHelper($"repo{i}"));
                Debug.WriteLine($"Repo[{i}]:{moduleTestHelpers[i].TemporaryPath}");
            }

            foreach (GitModuleTestHelper helper in moduleTestHelpers)
            {
                // Submodules require at least one commit
                helper.Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Initial commit""");
            }

            for (int i = numModules - 1; i > 0; --i)
            {
                GitModuleTestHelper parent = moduleTestHelpers[i - 1];
                GitModuleTestHelper child = moduleTestHelpers[i];

                // Add child as submodule of parent
                parent.AddSubmodule(child, $"repo{i}");
            }

            // Init all modules of root
            GitModuleTestHelper root = moduleTestHelpers[0];
            IEnumerable<GitConfigItem> cfgs = Commands.GetAllowFileConfig();

            root.Module.GitExecutable.Execute(Commands.SubmoduleUpdate(name: null, cfgs));

            IReadOnlyList<string> paths = root.Module.GetSubmodulesLocalPaths(recursive: true);
            ClassicAssert.AreEqual(new string[] { "repo1", "repo1/repo2", "repo1/repo2/repo3" }, paths, $"Modules: {string.Join(" ", paths)}");

            paths = root.Module.GetSubmodulesLocalPaths(recursive: false);
            ClassicAssert.AreEqual(new string[] { "repo1" }, paths, $"Modules: {string.Join(" ", paths)}");
        }
        finally
        {
            foreach (GitModuleTestHelper helper in moduleTestHelpers)
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

    [Test]
    public void GetSuperprojectCurrentCheckout()
    {
        // Create super and sub repo
        using GitModuleTestHelper moduleTestHelperSuper = new("super repo"),
                                                   moduleTestHelperSub = new("sub repo");

        // Add and init the submodule
        moduleTestHelperSuper.AddSubmodule(moduleTestHelperSub, "sub repo");
        IGitModule moduleSub = moduleTestHelperSuper.GetSubmodulesRecursive().ElementAt(0);

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
            ClassicAssert.AreEqual(32, code);
            ClassicAssert.AreEqual(commitRef, commitId.ToString());
        });
    }

    [TestCase(false, @"stash list")]
    [TestCase(true, @"--no-optional-locks stash list")]
    public void GetStashesCmd(bool noLocks, string expected)
    {
        ClassicAssert.AreEqual(expected, _gitModule.GetStashesCmd(noLocks).ToString());
    }

    [TestCase("", "")] // empty message
    [TestCase("a\r\nb\r\n\r\nc\r\n\r\n\r\n\r\nd", "a\r\nb\r\n\r\nc\r\n\r\nd")] // various amount of new lines between text lines
    [TestCase("\r\n\r\n\r\n\r\na\r\nb\r\n\r\n\r\n\r\n\r\n\r\n", "a\r\nb")] // trimmable message
    [TestCase("a\n\n\nb\r\n\r\nc\n\nd", "a\r\n\r\nb\r\n\r\nc\r\n\r\nd")] // mix of new line types
    [TestCase("Hello, this is a single line message", "Hello, this is a single line message")] // single line message
    [TestCase("1\n2\n3\n4\n5\n6\n7\n8\n9\n10\n11\n12\n13", "1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n9\r\n10\r\n11\r\n12\r\n13")] // message with more than 10 lines (a previous limitation)
    public void GetTagMessage(string tagMessage, string expectedReturnedMessage)
    {
        // add initial tag message
        using ReferenceRepository repo = new();
        repo.CreateAnnotatedTag("test_tag", repo.CommitHash, tagMessage);

        // execute test look-up
        string actualReturnedMessage = repo.Module.GetTagMessage("test_tag", cancellationToken: default);

        // compare result to expectations
        ClassicAssert.AreEqual(expectedReturnedMessage, actualReturnedMessage);
    }

    // TODO: add GetTagMessage "sad-path" tests, ones that test what happens if we try to execute it on a non-tag object.
    //       I expect we will need to do some refactoring in order to get such tests to pass (GetTagMessage is not robust at the moment)

    [TestCase(false, @"-c core.safecrlf=false update-index --add --stdin")]
    [TestCase(true, @"update-index --add --stdin")]
    public void UpdateIndexCmd_should_add_core_safecrlf(bool showErrorsWhenStagingFiles, string expected)
    {
        GitModule.TestAccessor accessor = _gitModule.GetTestAccessor();

        string actual = accessor.UpdateIndexCmd(showErrorsWhenStagingFiles).ToString();
        ClassicAssert.AreEqual(expected, actual);
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
        ClassicAssert.AreEqual(expectedOutput, _gitModule.CheckoutIndexFiles(files?.ToList()));
    }

    [TestCase(new string[] { "abc", "def" }, "checkout-index --index --force -- \"abc\" \"def\"")]
    public void ResetFiles_should_work_as_expected(string[] files, string args)
    {
        // Real GitModule is need to access AppSettings.GitCommand static property, avoid exception with dummy GitModule
        using GitModuleTestHelper moduleTestHelper = new();
        GitModule gitModule = GetGitModuleWithExecutable(_executable, module: moduleTestHelper.Module);
        string dummyCommandOutput = "The answer is 42. Just check that the Git arguments are as expected.";
        _executable.StageOutput(args, dummyCommandOutput);
        string result = gitModule.CheckoutIndexFiles(files.ToList());
        ClassicAssert.AreEqual(dummyCommandOutput, result);
    }

    [TestCase(new string[] { "abc", "def" }, "rm -- \"abc\" \"def\"")]
    public void RemoveFiles_shouldWorkAsExpected(string[] files, string args)
    {
        // Real GitModule is need to access AppSettings.GitCommand static property, avoid exception with dummy GitModule
        using GitModuleTestHelper moduleTestHelper = new();
        GitModule gitModule = GetGitModuleWithExecutable(_executable, module: moduleTestHelper.Module);
        string dummyCommandOutput = "The answer is 42. Just check that the Git arguments are as expected.";
        _executable.StageOutput(args, dummyCommandOutput);
        string result = gitModule.RemoveFiles(files.ToList(), false);
        ClassicAssert.AreEqual(dummyCommandOutput, result);
    }

    [TestCase(new string[] { }, "")]
    public void RemoveFiles_should_handle_empty_list(string[] files, string expectedOutput)
    {
        ClassicAssert.AreEqual(expectedOutput, _gitModule.RemoveFiles(files.ToList(), false));
    }

    [TestCaseSource(nameof(BatchUnstageFilesTestCases))]
    public void BatchUnstageFiles_should_work_as_expected(GitItemStatus[] files, string[] args, bool expectedResult)
    {
        // Real GitModule is need to access AppSettings.GitCommand static property, avoid exception with dummy GitModule
        using GitModuleTestHelper moduleTestHelper = new();
        GitModule gitModule = GetGitModuleWithExecutable(_executable, module: moduleTestHelper.Module);

        foreach (string arg in args)
        {
            _executable.StageCommand(arg);
        }

        bool result = gitModule.BatchUnstageFiles(files);
        ClassicAssert.AreEqual(expectedResult, result);
    }

    private static IEnumerable<TestCaseData> BatchUnstageFilesTestCases
    {
        get
        {
            yield return new TestCaseData(
                new GitItemStatus[]
                {
                    new("abc2") { IsNew = true },
                    new("abc2") { IsNew = true, IsDeleted = true },
                    new("abc2") { IsNew = false },
                    new("abc3") { IsNew = false, IsRenamed = true, OldName = "def" }
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
                    new("abc2") { IsNew = false },
                    new("abc3") { IsNew = false, IsDeleted = true }
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
    private static GitModule GetGitModuleWithExecutable(IExecutable executable, string path = "", GitModule module = null)
    {
        module ??= new GitModule(path);

        typeof(GitModule).GetField("_gitExecutable", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(module, executable);
        typeof(GitModule).GetField("_gitWindowsExecutable", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(module, executable);
        GitCommandRunner cmdRunner = new(executable, () => GitModule.SystemEncoding);
        typeof(GitModule).GetField("_gitCommandRunner", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(module, cmdRunner);

        return module;
    }
}
