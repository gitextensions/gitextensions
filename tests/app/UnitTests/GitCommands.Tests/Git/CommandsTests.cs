using GitCommands;
using GitCommands.Git;
using GitCommands.Utils;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitCommandsTests_Git;
public partial class CommandsTests
{
    [Test]
    public void AbortMergeCmd()
    {
        Commands.AbortMerge().Arguments.Should().Be("merge --abort");
    }

    private static IEnumerable<TestCaseData> AddSubmoduleTestCases()
    {
        yield return new TestCaseData("", null);
        yield return new TestCaseData("-c protocol.file.allow=always ", Commands.GetAllowFileConfig());
    }

    [Test, TestCaseSource(nameof(AddSubmoduleTestCases))]
    public void AddSubmoduleCmd(string config, IEnumerable<GitConfigItem> configs)
    {
        Commands.AddSubmodule("remotepath", "localpath", "branch", force: false, configs).Arguments.Should().Be($"{config}submodule add -b \"branch\" \"remotepath\" \"localpath\"");

        Commands.AddSubmodule("remotepath", "localpath", branch: null!, force: false, configs).Arguments.Should().Be($"{config}submodule add \"remotepath\" \"localpath\"");

        Commands.AddSubmodule("remotepath", "localpath", "branch", force: true, configs).Arguments.Should().Be($"{config}submodule add -f -b \"branch\" \"remotepath\" \"localpath\"");

        Commands.AddSubmodule("remote\\path", "local\\path", "branch", force: true, configs).Arguments.Should().Be($"{config}submodule add -f -b \"branch\" \"remote/path\" \"local/path\"");
    }

    [Test]
    public void ApplyDiffPatchCmd()
    {
        Commands.ApplyDiffPatch(false, "hello\\world.patch", PathUtil.ToPosixPath).Arguments.Should().Be("apply \"hello/world.patch\"");
        Commands.ApplyDiffPatch(true, "hello\\world.patch", PathUtil.ToPosixPath).Arguments.Should().Be("apply --ignore-whitespace \"hello/world.patch\"");
    }

    [TestCase(false, false, "hello\\world.patch", "am --3way \"hello/world.patch\"")]
    [TestCase(false, true, "hello\\world.patch", "am --3way --ignore-whitespace \"hello/world.patch\"")]
    [TestCase(true, false, "hello\\world.patch", "am --3way --signoff \"hello/world.patch\"")]
    [TestCase(true, true, "hello\\world.patch", "am --3way --signoff --ignore-whitespace \"hello/world.patch\"")]
    [TestCase(true, true, null, "am --3way --signoff --ignore-whitespace")]
    public void ApplyMailboxPatchCmd(bool signOff, bool ignoreWhitespace, string? patchFile, string expected)
    {
        Commands.ApplyMailboxPatch(signOff, ignoreWhitespace, patchFile, PathUtil.ToPosixPath).Arguments.Should().Be(expected);
    }

    [Test]
    public void BranchCmd()
    {
        // TODO split this into BranchCmd and CheckoutCmd

        Commands.Branch("branch", "revision", checkout: true).Arguments.Should().Be("checkout -b \"branch\" \"revision\"");
        Commands.Branch("branch", "revision", checkout: false).Arguments.Should().Be("branch \"branch\" \"revision\"");
        Commands.Branch("branch", null!, checkout: true).Arguments.Should().Be("checkout -b \"branch\"");
        Commands.Branch("branch", "", checkout: true).Arguments.Should().Be("checkout -b \"branch\"");
        Commands.Branch("branch", "  ", checkout: true).Arguments.Should().Be("checkout -b \"branch\"");
    }

    [Test]
    public void CheckoutCmd()
    {
        Commands.Checkout("branch", LocalChangesAction.DontChange).Arguments.Should().Be("checkout \"branch\"");
        Commands.Checkout("branch", LocalChangesAction.Merge).Arguments.Should().Be("checkout --merge \"branch\"");
        Commands.Checkout("branch", LocalChangesAction.Reset).Arguments.Should().Be("checkout --force \"branch\"");
        Commands.Checkout("branch", LocalChangesAction.Stash).Arguments.Should().Be("checkout \"branch\"");
    }

    [TestCase(CleanMode.OnlyNonIgnored, true, false, null, null, "clean --dry-run")]
    [TestCase(CleanMode.OnlyNonIgnored, false, false, null, null, "clean -f")]
    [TestCase(CleanMode.OnlyNonIgnored, false, true, null, null, "clean -d -f")]
    [TestCase(CleanMode.OnlyNonIgnored, false, false, "\"path1\"", null, "clean -f \"path1\"")]
    [TestCase(CleanMode.OnlyNonIgnored, false, false, "\"path1\"", "--exclude=excludes", "clean -f \"path1\" --exclude=excludes")]
    [TestCase(CleanMode.OnlyNonIgnored, false, false, null, "excludes", "clean -f excludes")]
    [TestCase(CleanMode.OnlyNonIgnored, false, false, "\"path1\" \"path2\"", null, "clean -f \"path1\" \"path2\"")]
    [TestCase(CleanMode.OnlyNonIgnored, false, false, "\"path1\" \"path2\"", "--exclude=exclude1 --exclude=exclude2", "clean -f \"path1\" \"path2\" --exclude=exclude1 --exclude=exclude2")]
    [TestCase(CleanMode.OnlyNonIgnored, false, false, null, "--exclude=exclude1 --exclude=exclude2", "clean -f --exclude=exclude1 --exclude=exclude2")]
    [TestCase(CleanMode.OnlyIgnored, false, false, null, null, "clean -X -f")]
    [TestCase(CleanMode.All, false, false, null, null, "clean -x -f")]
    public void CleanCmd(CleanMode mode, bool dryRun, bool directories, string? paths, string? excludes, string expected)
    {
        Commands.Clean(mode, dryRun, directories, paths, excludes).Arguments.Should().Be(expected);
    }

    [TestCase(CleanMode.OnlyNonIgnored, true, false, null, "clean --dry-run")]
    [TestCase(CleanMode.OnlyNonIgnored, false, false, null, "clean -f")]
    [TestCase(CleanMode.OnlyNonIgnored, false, true, null, "clean -d -f")]
    [TestCase(CleanMode.OnlyNonIgnored, false, false, "paths", "clean -f paths")]
    [TestCase(CleanMode.OnlyIgnored, false, false, null, "clean -X -f")]
    [TestCase(CleanMode.All, false, false, null, "clean -x -f")]
    public void CleanupSubmoduleCommand(CleanMode mode, bool dryRun, bool directories, string? paths, string expected)
    {
        string subExpected = "submodule foreach --recursive git " + expected;
        Commands.CleanSubmodules(mode, dryRun, directories, paths).Arguments.Should().Be(subExpected);
    }

    [Test]
    public void CloneCmd()
    {
        Commands.Clone("from", "to", PathUtil.ToPosixPath).Arguments.Should().Be("clone -v --progress \"from\" \"to\"");
        Commands.Clone("from/path", "to/path", PathUtil.ToPosixPath).Arguments.Should().Be("clone -v --progress \"from/path\" \"to/path\"");
        Commands.Clone("from", "to", PathUtil.ToPosixPath, central: true).Arguments.Should().Be("clone -v --bare --progress \"from\" \"to\"");
        Commands.Clone("from", "to", PathUtil.ToPosixPath, initSubmodules: true).Arguments.Should().Be("clone -v --recurse-submodules --progress \"from\" \"to\"");
        Commands.Clone("from", "to", PathUtil.ToPosixPath, initSubmodules: true).Arguments.Should().Be("clone -v --recurse-submodules --progress \"from\" \"to\"");
        Commands.Clone("from", "to", PathUtil.ToPosixPath, depth: 2).Arguments.Should().Be("clone -v --depth 2 --progress \"from\" \"to\"");
        Commands.Clone("from", "to", PathUtil.ToPosixPath, isSingleBranch: true).Arguments.Should().Be("clone -v --single-branch --progress \"from\" \"to\"");
        Commands.Clone("from", "to", PathUtil.ToPosixPath, isSingleBranch: false).Arguments.Should().Be("clone -v --no-single-branch --progress \"from\" \"to\"");
        Commands.Clone("from", "to", PathUtil.ToPosixPath, branch: "branch").Arguments.Should().Be("clone -v --progress --branch branch \"from\" \"to\"");
        Commands.Clone("from", "to", PathUtil.ToPosixPath, branch: null).Arguments.Should().Be("clone -v --progress --no-checkout \"from\" \"to\"");
    }

    [Test]
    public void CommitCmd()
    {
        Commands.Commit(amend: false, signOff: false, author: "", useExplicitCommitMessage: true, commitMessageFile: "COMMITMESSAGE", PathUtil.ToPosixPath, gpgSign: null).Arguments.Should().Be("commit -F \"COMMITMESSAGE\"");
        Commands.Commit(amend: false, signOff: false, author: "", useExplicitCommitMessage: true, commitMessageFile: "COMMITMESSAGE", path => "adapted_commit_message_path", gpgSign: null).Arguments.Should().Be("commit -F \"adapted_commit_message_path\"");
        Commands.Commit(amend: true, signOff: false, author: "", useExplicitCommitMessage: true, commitMessageFile: "COMMITMESSAGE", PathUtil.ToPosixPath, gpgSign: null).Arguments.Should().Be("commit --amend -F \"COMMITMESSAGE\"");
        Commands.Commit(amend: false, signOff: true, author: "", useExplicitCommitMessage: true, commitMessageFile: "COMMITMESSAGE", PathUtil.ToPosixPath, gpgSign: null).Arguments.Should().Be("commit --signoff -F \"COMMITMESSAGE\"");
        Commands.Commit(amend: false, signOff: false, author: "foo", useExplicitCommitMessage: true, commitMessageFile: "COMMITMESSAGE", PathUtil.ToPosixPath, gpgSign: null).Arguments.Should().Be("commit --author=\"foo\" -F \"COMMITMESSAGE\"");
        Commands.Commit(amend: false, signOff: false, author: "", useExplicitCommitMessage: false, commitMessageFile: null, PathUtil.ToPosixPath, gpgSign: null).Arguments.Should().Be("commit");
        Commands.Commit(amend: false, signOff: false, author: "", useExplicitCommitMessage: true, commitMessageFile: "COMMITMESSAGE", PathUtil.ToPosixPath, noVerify: true, gpgSign: null).Arguments.Should().Be("commit --no-verify -F \"COMMITMESSAGE\"");
        Commands.Commit(amend: false, signOff: false, author: "", useExplicitCommitMessage: true, commitMessageFile: "COMMITMESSAGE", PathUtil.ToPosixPath, gpgSign: false).Arguments.Should().Be("commit --no-gpg-sign -F \"COMMITMESSAGE\"");
        Commands.Commit(amend: false, signOff: false, author: "", useExplicitCommitMessage: true, commitMessageFile: "COMMITMESSAGE", PathUtil.ToPosixPath, gpgSign: true).Arguments.Should().Be("commit --gpg-sign -F \"COMMITMESSAGE\"");
        Commands.Commit(amend: false, signOff: false, author: "", useExplicitCommitMessage: true, commitMessageFile: "COMMITMESSAGE", PathUtil.ToPosixPath, gpgSign: true, gpgKeyId: "key").Arguments.Should().Be("commit --gpg-sign=key -F \"COMMITMESSAGE\"");
    }

    [TestCase(@"  ""author <author@mail.com>""  ", @"commit --author=""author <author@mail.com>"" -F ""COMMITMESSAGE""")]
    [TestCase(@"""author <author@mail.com>""", @"commit --author=""author <author@mail.com>"" -F ""COMMITMESSAGE""")]
    [TestCase(@"author <author@mail.com>", @"commit --author=""author <author@mail.com>"" -F ""COMMITMESSAGE""")]
    public void CommitCmdShouldTrimAuthor(string input, string expected)
    {
        ArgumentString actual = Commands.Commit(amend: false, signOff: false, author: input, useExplicitCommitMessage: true, commitMessageFile: "COMMITMESSAGE", PathUtil.ToPosixPath);
        actual.Arguments.Should().Be(expected);
    }

    [TestCase(false, false, @"", false, false, null, @"", @"commit")]
    [TestCase(false, false, @"", false, false, false, @"", @"commit --no-gpg-sign")]
    [TestCase(true, false, @"", false, false, false, @"", @"commit --amend --no-gpg-sign")]
    [TestCase(false, true, @"", false, false, false, @"", @"commit --signoff --no-gpg-sign")]
    [TestCase(false, false, @"", true, false, false, @"", @"commit --no-gpg-sign -F ""COMMITMESSAGE""")]
    [TestCase(false, false, @"", false, true, false, @"", @"commit --no-verify --no-gpg-sign")]
    [TestCase(false, false, @"", false, false, false, @"12345678", @"commit --no-gpg-sign")]
    [TestCase(false, false, @"", false, false, true, @"", @"commit --gpg-sign")]
    [TestCase(false, false, @"", false, false, true, @"      ", @"commit --gpg-sign")]
    [TestCase(false, false, @"", false, false, true, null, @"commit --gpg-sign")]
    [TestCase(false, false, @"", false, false, true, @"12345678", @"commit --gpg-sign=12345678")]
    [TestCase(true, true, @"", true, true, true, @"12345678", @"commit --amend --no-verify --signoff --gpg-sign=12345678 -F ""COMMITMESSAGE""")]
    public void CommitCmdTests(bool amend, bool signOff, string author, bool useExplicitCommitMessage, bool noVerify, bool? gpgSign, string? gpgKeyId, string expected)
    {
        ArgumentString actual = Commands.Commit(amend, signOff, author, useExplicitCommitMessage, "COMMITMESSAGE", PathUtil.ToPosixPath, noVerify, gpgSign, gpgKeyId!);
        actual.Arguments.Should().Be(expected);
    }

    [Test]
    public void ContinueBisectCmd()
    {
        ObjectId id1 = ObjectId.Random();
        ObjectId id2 = ObjectId.Random();

        Commands.ContinueBisect(GitBisectOption.Good).Arguments.Should().Be("bisect good");
        Commands.ContinueBisect(GitBisectOption.Bad).Arguments.Should().Be("bisect bad");
        Commands.ContinueBisect(GitBisectOption.Skip).Arguments.Should().Be("bisect skip");
        Commands.ContinueBisect(GitBisectOption.Good, id1, id2).Arguments.Should().Be($"bisect good {id1} {id2}");
    }

    [Test]
    public void ContinueMergeCmd()
    {
        Commands.ContinueMerge().Arguments.Should().Be("merge --continue");
    }

    [Test]
    public void GetAllChangedFilesCmd()
    {
        Commands.GetAllChangedFiles(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Default).Arguments.Should().Be("-c diff.ignoresubmodules=none status --porcelain=2 -z --untracked-files --ignore-submodules");
        Commands.GetAllChangedFiles(excludeIgnoredFiles: false, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Default).Arguments.Should().Be("-c diff.ignoresubmodules=none status --porcelain=2 -z --untracked-files --ignored --ignore-submodules");
        Commands.GetAllChangedFiles(excludeIgnoredFiles: true, UntrackedFilesMode.No, IgnoreSubmodulesMode.Default).Arguments.Should().Be("-c diff.ignoresubmodules=none status --porcelain=2 -z --untracked-files=no --ignore-submodules");
        Commands.GetAllChangedFiles(excludeIgnoredFiles: true, UntrackedFilesMode.Normal, IgnoreSubmodulesMode.Default).Arguments.Should().Be("-c diff.ignoresubmodules=none status --porcelain=2 -z --untracked-files=normal --ignore-submodules");
        Commands.GetAllChangedFiles(excludeIgnoredFiles: true, UntrackedFilesMode.All, IgnoreSubmodulesMode.Default).Arguments.Should().Be("-c diff.ignoresubmodules=none status --porcelain=2 -z --untracked-files=all --ignore-submodules");
        Commands.GetAllChangedFiles(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.None).Arguments.Should().Be("-c diff.ignoresubmodules=none status --porcelain=2 -z --untracked-files");
        Commands.GetAllChangedFiles(excludeIgnoredFiles: true, UntrackedFilesMode.Default).Arguments.Should().Be("-c diff.ignoresubmodules=none status --porcelain=2 -z --untracked-files");
        Commands.GetAllChangedFiles(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Untracked).Arguments.Should().Be("-c diff.ignoresubmodules=none status --porcelain=2 -z --untracked-files --ignore-submodules=untracked");
        Commands.GetAllChangedFiles(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Dirty).Arguments.Should().Be("-c diff.ignoresubmodules=none status --porcelain=2 -z --untracked-files --ignore-submodules=dirty");
        Commands.GetAllChangedFiles(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.All).Arguments.Should().Be("-c diff.ignoresubmodules=none status --porcelain=2 -z --untracked-files --ignore-submodules=all");
        Commands.GetAllChangedFiles(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Default, noLocks: true).Arguments.Should().Be("--no-optional-locks -c diff.ignoresubmodules=none status --porcelain=2 -z --untracked-files --ignore-submodules");
    }

    [TestCase(@"-c color.ui=never -c diff.submodule=short -c diff.noprefix=false -c diff.mnemonicprefix=false -c diff.ignoresubmodules=none -c core.safecrlf=false diff --no-ext-diff --find-renames --find-copies extra --cached -- ""new"" ""old""", "new", "old", true, "extra", false)]
    [TestCase(@"-c color.ui=never -c diff.submodule=short -c diff.noprefix=false -c diff.mnemonicprefix=false -c diff.ignoresubmodules=none -c core.safecrlf=false diff --no-ext-diff --find-renames --find-copies extra -- ""new""", "new", "old", false, "extra", false)]
    [TestCase(@"--no-optional-locks -c color.ui=never -c diff.submodule=short -c diff.noprefix=false -c diff.mnemonicprefix=false -c diff.ignoresubmodules=none -c core.safecrlf=false diff --no-ext-diff --find-renames --find-copies extra --cached -- ""new"" ""old""", "new", "old", true, "extra", true)]
    public void GetCurrentChangesCmd(string expected, string fileName, string oldFileName, bool staged, string extraDiffArguments, bool noLocks)
    {
        Commands.GetCurrentChanges(fileName, oldFileName, staged,
            extraDiffArguments, noLocks).ToString().Should().Be(expected);
    }

    private static IEnumerable<TestCaseData> GetRefsTestData
    {
        get
        {
            foreach (GitRefsSortBy sortBy in EnumHelper.GetValues<GitRefsSortBy>())
            {
                foreach (GitRefsSortOrder sortOrder in EnumHelper.GetValues<GitRefsSortOrder>())
                {
                    string sortCondition;
                    string sortConditionRef;
                    string format = @" --format=""%(if)%(authordate)%(then)%(objectname) %(refname)%(else)%(*objectname) %(*refname)%(end)""";
                    string formatNoTag = @" --format=""%(objectname) %(refname)""";
                    if (sortBy == GitRefsSortBy.Default)
                    {
                        sortCondition = sortConditionRef = string.Empty;
                    }
                    else
                    {
                        const string gitRefsSortByVersion = "version:refname";
                        string sortKey = sortBy == GitRefsSortBy.versionRefname ? gitRefsSortByVersion : sortBy.ToString();
                        string derefSortKey = (sortBy == GitRefsSortBy.versionRefname ? GitRefsSortBy.refname : sortBy).ToString();

                        string order = sortOrder == GitRefsSortOrder.Ascending ? string.Empty : "-";
                        sortCondition = $@" --sort=""{order}{sortKey}""";
                        sortConditionRef = $@" --sort=""{order}*{derefSortKey}""";
                    }

                    yield return new TestCaseData(RefsFilter.Tags | RefsFilter.Heads | RefsFilter.Remotes, /* noLocks */ false, sortBy, sortOrder, 0,
                        /* expected */ $@"for-each-ref{sortConditionRef}{sortCondition}{format} refs/heads/ refs/remotes/ refs/tags/");
                    yield return new TestCaseData(RefsFilter.Tags, /* noLocks */ false, sortBy, sortOrder, 0,
                        /* expected */ $@"for-each-ref{sortConditionRef}{sortCondition}{format} refs/tags/");
                    yield return new TestCaseData(RefsFilter.Heads, /* noLocks */ false, sortBy, sortOrder, 0,
                        /* expected */ $@"for-each-ref{sortCondition}{formatNoTag} refs/heads/");
                    yield return new TestCaseData(RefsFilter.Heads, /* noLocks */ false, sortBy, sortOrder, 100,
                        /* expected */ $@"for-each-ref{sortCondition}{formatNoTag} --count=100 refs/heads/");
                    yield return new TestCaseData(RefsFilter.Heads, /* noLocks */ true, sortBy, sortOrder, 0,
                        /* expected */ $@"--no-optional-locks for-each-ref{sortCondition}{formatNoTag} refs/heads/");
                    yield return new TestCaseData(RefsFilter.Remotes, /* noLocks */ false, sortBy, sortOrder, 0,
                        /* expected */ $@"for-each-ref{sortCondition}{formatNoTag} refs/remotes/");

                    yield return new TestCaseData(RefsFilter.NoFilter, /* noLocks */ true, sortBy, sortOrder, 0,
                        /* expected */ $@"--no-optional-locks for-each-ref{sortConditionRef}{sortCondition}{format}");
                    yield return new TestCaseData(RefsFilter.Tags | RefsFilter.Heads | RefsFilter.Remotes | RefsFilter.NoFilter, /* noLocks */ true, sortBy, sortOrder, 0,
                        /* expected */ $@"--no-optional-locks for-each-ref{sortConditionRef}{sortCondition}{format} refs/heads/ refs/remotes/ refs/tags/");
                }
            }
        }
    }

    [TestCaseSource(nameof(GetRefsTestData))]
    public void GetRefsCmd(RefsFilter getRefs, bool noLocks, GitRefsSortBy sortBy, GitRefsSortOrder sortOrder, int count, string expected)
    {
        Commands.GetRefs(getRefs, noLocks, sortBy, sortOrder, count).ToString().Should().Be(expected);
    }

    // Don't care about permutations because the args aren't correlated
    [TestCase(false, false, false, null, false, null, null, "merge --no-ff branch")]
    [TestCase(true, true, true, null, true, null, null, "merge --squash --no-commit --allow-unrelated-histories branch")]

    // mergeCommitFilePath parameter
    [TestCase(false, true, false, null, false, "", null, "merge --no-ff --squash branch")]
    [TestCase(false, true, false, null, false, "   ", null, "merge --no-ff --squash branch")]
    [TestCase(false, true, false, null, false, "\t", null, "merge --no-ff --squash branch")]
    [TestCase(false, true, false, null, false, "\n", null, "merge --no-ff --squash branch")]
    [TestCase(false, true, false, null, false, "foo", null, "merge --no-ff --squash -F \"foo\" branch")]
    [TestCase(false, true, false, null, false, "D:\\myrepo\\.git\\file", null, "merge --no-ff --squash -F \"D:/myrepo/.git/file\" branch")]

    // log parameter
    [TestCase(true, true, false, null, false, null, -1, "merge --squash branch")]
    [TestCase(true, true, false, null, false, null, 0, "merge --squash branch")]
    [TestCase(true, true, false, null, false, null, 5, "merge --squash --log=5 branch")]
    public void MergeBranchCmd(bool allowFastForward, bool squash, bool noCommit, string? strategy, bool allowUnrelatedHistories, string? mergeCommitFilePath, int? log, string expected)
    {
        Commands.MergeBranch("branch", allowFastForward, squash, noCommit, strategy!, allowUnrelatedHistories, mergeCommitFilePath, PathUtil.ToPosixPath, log).Arguments.Should().Be(expected);
    }

    [Test]
    public void MergedBranchesCmd([Values(true, false)] bool includeRemote, [Values(true, false)] bool fullRefname,
         [Values(null, "", "HEAD", "1234567890")] string? commit)
    {
        string formatArg = fullRefname ? @" --format=""%(refname)""" : string.Empty;
        string remoteArg = includeRemote ? " -a" : string.Empty;
        string commitArg = string.IsNullOrWhiteSpace(commit) ? string.Empty : $" {commit.Quote()}";
        string expected = $"branch{formatArg}{remoteArg} --merged{commitArg}";

        Commands.MergedBranches(includeRemote, fullRefname, commit).Arguments.Should().Be(expected);
    }

    [Test]
    public void PushAllCmd()
    {
        Commands.PushAll("remote", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 0).Arguments.Should().Be("push --progress --all \"remote\"");
        Commands.PushAll("remote", ForcePushOptions.Force, track: false, recursiveSubmodules: 0).Arguments.Should().Be("push -f --progress --all \"remote\"");
        Commands.PushAll("remote", ForcePushOptions.ForceWithLease, track: false, recursiveSubmodules: 0).Arguments.Should().Be("push --force-with-lease --progress --all \"remote\"");
        Commands.PushAll("remote", ForcePushOptions.DoNotForce, track: true, recursiveSubmodules: 0).Arguments.Should().Be("push -u --progress --all \"remote\"");
        Commands.PushAll("remote", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 1).Arguments.Should().Be("push --recurse-submodules=check --progress --all \"remote\"");
        Commands.PushAll("remote", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 2).Arguments.Should().Be("push --recurse-submodules=on-demand --progress --all \"remote\"");
    }

    [Test]
    public void PushCmd()
    {
        Commands.Push("remote", "from-branch", null, ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 0).Arguments.Should().Be("push --progress \"remote\" from-branch");

        Commands.Push("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 0).Arguments.Should().Be("push --progress \"remote\" from-branch:refs/heads/to-branch");

        Commands.Push("remote", "from-branch", "to-branch", ForcePushOptions.Force, track: false, recursiveSubmodules: 0).Arguments.Should().Be("push -f --progress \"remote\" from-branch:refs/heads/to-branch");

        Commands.Push("remote", "from-branch", "to-branch", ForcePushOptions.ForceWithLease, track: false, recursiveSubmodules: 0).Arguments.Should().Be("push --force-with-lease --progress \"remote\" from-branch:refs/heads/to-branch");

        Commands.Push("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: true, recursiveSubmodules: 0).Arguments.Should().Be("push -u --progress \"remote\" from-branch:refs/heads/to-branch");

        Commands.Push("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 1).Arguments.Should().Be("push --recurse-submodules=check --progress \"remote\" from-branch:refs/heads/to-branch");

        Commands.Push("remote", "from-branch", "to-branch", ForcePushOptions.DoNotForce, track: false, recursiveSubmodules: 2).Arguments.Should().Be("push --recurse-submodules=on-demand --progress \"remote\" from-branch:refs/heads/to-branch");
    }

    [TestCase("mybranch", ".", false, false, ExpectedResult = @"push ""file://."" ""1111111111111111111111111111111111111111:mybranch""")]
    [TestCase("branch2", "/my/path", true, false, ExpectedResult = @"push ""file:///my/path"" ""1111111111111111111111111111111111111111:branch2"" --force")]
    [TestCase("branchx", @"c:/my/path", true, true, ExpectedResult = @"push ""file://c:/my/path"" ""1111111111111111111111111111111111111111:branchx"" --force --dry-run")]
    public string PushLocalCmd(string gitRef, string repoDir, bool force, bool dryRun)
    {
        return Commands.PushLocal(gitRef, ObjectId.WorkTreeId, repoDir, PathUtil.ToPosixPath, force, dryRun).Arguments!;
    }

    [Test]
    public void PushTagCmd()
    {
        Commands.PushTag("path", "tag", all: false).Arguments.Should().Be("push --progress \"path\" tag tag");
        Commands.PushTag("path", " tag ", all: false).Arguments.Should().Be("push --progress \"path\" tag tag");
        Commands.PushTag("path\\path", " tag ", all: false).Arguments.Should().Be("push --progress \"path/path\" tag tag");
        Commands.PushTag("path", "tag", all: true).Arguments.Should().Be("push --progress \"path\" --tags");
        Commands.PushTag("path", "tag", all: true, force: ForcePushOptions.Force).Arguments.Should().Be("push -f --progress \"path\" --tags");
        Commands.PushTag("path", "tag", all: true, force: ForcePushOptions.ForceWithLease).Arguments.Should().Be("push --force-with-lease --progress \"path\" --tags");

        // TODO this should probably throw rather than return an empty string
        Commands.PushTag("path", "", all: false).Arguments.Should().Be("");
    }

    [TestCase(null, "onto")]
    [TestCase("from", null)]
    public void RebaseCmd_throws_ArgumentException_if_from_or_onto_null(string? from, string? onto)
    {
        Commands.RebaseOptions rebaseOptions = new()
        {
            BranchName = "branch",
            From = from,
            OnTo = onto
        };

        ((Action)(() => Commands.Rebase(rebaseOptions))).Should().Throw<ArgumentException>();
    }

    [TestCase(false, false, false, false, false, false, true, null, "-c rebase.autosquash=false rebase \"branch\"")]
    [TestCase(true, false, false, false, false, false, true, null, "-c rebase.autosquash=false rebase -i --no-autosquash \"branch\"")]
    [TestCase(false, true, false, false, false, false, true, null, "-c rebase.autosquash=false rebase --rebase-merges \"branch\"")]
    [TestCase(false, false, true, false, false, false, true, null, "-c rebase.autosquash=false rebase \"branch\"")]
    [TestCase(false, false, false, true, false, false, true, null, "-c rebase.autosquash=false rebase --autostash \"branch\"")]
    [TestCase(true, false, true, false, false, false, true, null, "-c rebase.autosquash=false rebase -i --autosquash \"branch\"")]
    [TestCase(false, false, false, false, true, false, true, null, "-c rebase.autosquash=false rebase --ignore-date \"branch\"")]
    [TestCase(false, false, false, false, false, true, true, null, "-c rebase.autosquash=false rebase --committer-date-is-author-date \"branch\"")]
    [TestCase(false, false, false, true, true, false, true, null, "-c rebase.autosquash=false rebase --ignore-date --autostash \"branch\"")]
    [TestCase(false, false, false, true, false, true, true, null, "-c rebase.autosquash=false rebase --committer-date-is-author-date --autostash \"branch\"")]
    [TestCase(true, true, true, true, true, false, true, null, "-c rebase.autosquash=false rebase --ignore-date --autostash \"branch\"")]
    [TestCase(true, true, true, true, false, true, true, null, "-c rebase.autosquash=false rebase --committer-date-is-author-date --autostash \"branch\"")]
    [TestCase(true, true, true, true, false, false, true, null, "-c rebase.autosquash=false rebase -i --autosquash --rebase-merges --autostash \"branch\"")]
    [TestCase(false, false, false, false, false, false, true, false, "-c rebase.autosquash=false rebase --no-update-refs \"branch\"")]
    [TestCase(false, false, false, false, false, false, true, true, "-c rebase.autosquash=false rebase --update-refs \"branch\"")]
    public void RebaseCmd(bool interactive, bool preserveMerges, bool autosquash, bool autoStash, bool ignoreDate, bool committerDateIsAuthorDate, bool supportRebaseMerges, bool? updateRefs, string expected)
    {
        Commands.RebaseOptions rebaseOptions = new()
        {
            BranchName = "branch",
            Interactive = interactive,
            PreserveMerges = preserveMerges,
            AutoSquash = autosquash,
            AutoStash = autoStash,
            IgnoreDate = ignoreDate,
            CommitterDateIsAuthorDate = committerDateIsAuthorDate,
            SupportRebaseMerges = supportRebaseMerges,
            UpdateRefs = updateRefs
        };

        Commands.Rebase(rebaseOptions).Arguments.Should().Be(expected);
    }

    [TestCase(false, false, false, false, false, false, null, "from", "onto", "-c rebase.autosquash=false rebase --onto onto \"from\" \"branch\"")]
    [TestCase(false, false, false, false, true, false, null, "from", "onto", "-c rebase.autosquash=false rebase --ignore-date --onto onto \"from\" \"branch\"")]
    public void RebaseCmd_specific_range(bool interactive, bool preserveMerges, bool autoSquash, bool autoStash, bool ignoreDate, bool committerDateIsAuthorDate, bool? updateRefs, string? from, string? onto, string expected)
    {
        Commands.RebaseOptions rebaseOptions = new()
        {
            BranchName = "branch",
            Interactive = interactive,
            PreserveMerges = preserveMerges,
            AutoSquash = autoSquash,
            AutoStash = autoStash,
            IgnoreDate = ignoreDate,
            CommitterDateIsAuthorDate = committerDateIsAuthorDate,
            UpdateRefs = updateRefs,
            From = from,
            OnTo = onto
        };

        Commands.Rebase(rebaseOptions).Arguments.Should().Be(expected);
    }

    [Test]
    public void RemoveCmd()
    {
        // TODO file names should be quoted

        Commands.Remove().Arguments.Should().Be("rm --force -r .");
        Commands.Remove(force: false).Arguments.Should().Be("rm -r .");
        Commands.Remove(isRecursive: false).Arguments.Should().Be("rm --force .");
        Commands.Remove(files: ["a", "b", "c"]).Arguments.Should().Be("rm --force -r a b c");
    }

    [Test]
    public void RenameBranch()
    {
        const string oldName = "foo";
        const string newName = "far";

        Commands.RenameBranch(oldName, newName).Arguments.Should().Be($"branch -m \"{oldName}\" \"{newName}\"");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("\t")]
    public void ResetCmd_should_throw_if_ResetIndex_and_hash_is_null_or_empty(string? hash)
    {
        ((Action)(() => Commands.Reset(ResetMode.ResetIndex, commit: hash, file: "file.txt"))).Should().Throw<ArgumentException>();
    }

    [TestCase(ResetMode.ResetIndex, "tree-ish", null, @"reset --quiet ""tree-ish"" --")]
    [TestCase(ResetMode.ResetIndex, "tree-ish", "file.txt", @"reset --quiet ""tree-ish"" -- ""file.txt""")]
    [TestCase(ResetMode.Soft, null, null, @"reset --soft --quiet --")]
    [TestCase(ResetMode.Mixed, null, null, @"reset --mixed --quiet --")]
    [TestCase(ResetMode.Hard, null, null, @"reset --hard --quiet --")]
    [TestCase(ResetMode.Merge, null, null, @"reset --merge --quiet --")]
    [TestCase(ResetMode.Keep, null, null, @"reset --keep --quiet --")]
    [TestCase(ResetMode.Soft, "tree-ish", null, @"reset --soft --quiet ""tree-ish"" --")]
    [TestCase(ResetMode.Mixed, "tree-ish", null, @"reset --mixed --quiet ""tree-ish"" --")]
    [TestCase(ResetMode.Hard, "tree-ish", null, @"reset --hard --quiet ""tree-ish"" --")]
    [TestCase(ResetMode.Merge, "tree-ish", null, @"reset --merge --quiet ""tree-ish"" --")]
    [TestCase(ResetMode.Keep, "tree-ish", null, @"reset --keep --quiet ""tree-ish"" --")]
    [TestCase(ResetMode.Soft, null, "file.txt", @"reset --soft --quiet -- ""file.txt""")]
    [TestCase(ResetMode.Mixed, null, "file.txt", @"reset --mixed --quiet -- ""file.txt""")]
    [TestCase(ResetMode.Hard, null, "file.txt", @"reset --hard --quiet -- ""file.txt""")]
    [TestCase(ResetMode.Merge, null, "file.txt", @"reset --merge --quiet -- ""file.txt""")]
    [TestCase(ResetMode.Keep, null, "file.txt", @"reset --keep --quiet -- ""file.txt""")]
    [TestCase(ResetMode.Soft, "tree-ish", "file.txt", @"reset --soft --quiet ""tree-ish"" -- ""file.txt""")]
    [TestCase(ResetMode.Mixed, "tree-ish", "file.txt", @"reset --mixed --quiet ""tree-ish"" -- ""file.txt""")]
    [TestCase(ResetMode.Hard, "tree-ish", "file.txt", @"reset --hard --quiet ""tree-ish"" -- ""file.txt""")]
    [TestCase(ResetMode.Merge, "tree-ish", "file.txt", @"reset --merge --quiet ""tree-ish"" -- ""file.txt""")]
    [TestCase(ResetMode.Keep, "tree-ish", "file.txt", @"reset --keep --quiet ""tree-ish"" -- ""file.txt""")]
    public void ResetCmd(ResetMode mode, string? commit, string? file, string expected)
    {
        Commands.Reset(mode, commit, file).Arguments.Should().Be(expected);
    }

    [Test]
    public void RevertCmd()
    {
        ObjectId commitId = ObjectId.Random();

        Commands.Revert(commitId, autoCommit: true, parentIndex: 0).Arguments.Should().Be($"revert {commitId}");

        Commands.Revert(commitId, autoCommit: false, parentIndex: 0).Arguments.Should().Be($"revert --no-commit {commitId}");

        Commands.Revert(commitId, autoCommit: true, parentIndex: 1).Arguments.Should().Be($"revert -m 1 {commitId}");
    }

    [Test]
    public void StashSaveCmd()
    {
        // TODO test case where message string contains quotes
        // TODO test case where message string contains newlines
        // TODO test case where selectedFiles contains whitespaces (not currently quoted)

        Commands.StashSave(untracked: false, keepIndex: false, null!, []).Arguments.Should().Be("stash save");

        Commands.StashSave(untracked: false, keepIndex: false, null!, null).Arguments.Should().Be("stash save");

        Commands.StashSave(untracked: true, keepIndex: false, null!, null).Arguments.Should().Be("stash save -u");

        Commands.StashSave(untracked: false, keepIndex: true, null!, null).Arguments.Should().Be("stash save --keep-index");

        Commands.StashSave(untracked: false, keepIndex: true, null!, null).Arguments.Should().Be("stash save --keep-index");

        Commands.StashSave(untracked: false, keepIndex: false, "message", null).Arguments.Should().Be("stash save \"message\"");

        Commands.StashSave(untracked: false, keepIndex: false, null!, new[] { "a", "b" }).Arguments.Should().Be("stash push -- \"a\" \"b\"");
    }

    [Test]
    public void StashSave_should_add_message_if_provided_partial_stash()
    {
        Commands.StashSave(untracked: false, keepIndex: false, "test message", new[] { "a", "b" }).Arguments.Should().Be("stash push -m \"test message\" -- \"a\" \"b\"");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t")]
    public void StashSave_should_not_add_empty_message_partial_stash(string? theMessage)
    {
        Commands.StashSave(untracked: false, keepIndex: false, theMessage!, new[] { "a", "b" }).Arguments.Should().Be("stash push -- \"a\" \"b\"");
    }

    [Test]
    public void StashSave_should_not_add_null_or_empty_filenames()
    {
        Commands.StashSave(untracked: false, keepIndex: false, null!, new[] { null!, "", "a" }).Arguments.Should().Be("stash push -- \"a\"");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t")]
    public void StashSaveCmd_should_not_add_empty_message_full_stash(string? theMessage)
    {
        Commands.StashSave(untracked: false, keepIndex: false, theMessage!, []).Arguments.Should().Be("stash save");
    }

    [Test]
    public void StashSaveCmd_should_add_message_if_provided_full_stash()
    {
        Commands.StashSave(untracked: false, keepIndex: false, "test message", []).Arguments.Should().Be("stash save \"test message\"");
    }

    [Test]
    public void SubmoduleSyncCmd()
    {
        string config = "";
        Commands.SubmoduleSync("foo").Arguments.Should().Be($"{config}submodule sync \"foo\"");
        Commands.SubmoduleSync("").Arguments.Should().Be($"{config}submodule sync");
        Commands.SubmoduleSync(null).Arguments.Should().Be($"{config}submodule sync");
    }

    [TestCase("mybranch", "2111111111111111111111111111111111111111", ExpectedResult = @"update-ref ""mybranch"" 2111111111111111111111111111111111111111")]
    public string UpdateRef(string gitRef, string hash)
    {
        return Commands.UpdateRef(gitRef, ObjectId.Parse(hash)).Arguments!;
    }
}
