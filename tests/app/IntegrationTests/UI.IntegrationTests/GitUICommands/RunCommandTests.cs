using CommonTestUtils;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.UITests;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitUITests.GitUICommandsTests;

[Apartment(ApartmentState.STA)]
public sealed class RunCommandTests
{
    private const string CompareCommand = "compare";
    private const string FirstFullShaPlaceholder = "<first-full-sha>";
    private const string FirstShortShaPlaceholder = "<first-short-sha>";
    private const string SecondFullShaPlaceholder = "<second-full-sha>";
    private const string NonexistentFullSha = "0123456789abcdef0123456789abcdef01234567";
    private const int ShortShaLength = 7;
    private const string CompareBranchName = "compare-base";
    private const string CompareTagName = "compare-tag";

    // Created once for the fixture
    private ReferenceRepository _referenceRepository = null!;

    // Created once for each test
    private GitUICommands _commands = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        AppSettings.SetDocumentationBaseUrl("33.33.33");
    }

    [SetUp]
    public void SetUp()
    {
        _referenceRepository = new ReferenceRepository();

        string cmdPath = (Environment.GetEnvironmentVariable("COMSPEC") ?? "C:/WINDOWS/system32/cmd.exe").ToPosixPath().QuoteNE();
        _referenceRepository.Module.GitExecutable.RunCommand($"config --local difftool.cmd.path {cmdPath}").Should().BeTrue();
        _referenceRepository.Module.GitExecutable.RunCommand($"config --local mergetool.cmd.path {cmdPath}").Should().BeTrue();
        _referenceRepository.Module.GitExecutable.RunCommand("config --local diff.guitool cmd").Should().BeTrue();
        _referenceRepository.Module.GitExecutable.RunCommand("config --local merge.guitool cmd").Should().BeTrue();

        AppSettings.UseConsoleEmulatorForCommands.Value = false;
        AppSettings.CloseProcessDialog = true;
        AppSettings.UseBrowseForFileHistory.Value = false;

        _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
    }

    [TearDown]
    public void TearDown()
    {
        _referenceRepository.Dispose();
    }

    [Test]
    public void RunCommandBasedOnArgument_should_throw_on_null_args()
    {
        ((Action)(() => _commands.GetTestAccessor().RunCommandBasedOnArgument(null!)))
            .Should().Throw<NullReferenceException>();
    }

    [Test]
    public void RunCommandBasedOnArgument_should_throw_on_empty_args()
    {
        ((Action)(() => _commands.GetTestAccessor().RunCommandBasedOnArgument([])))
            .Should().Throw<ArgumentOutOfRangeException>();

        ((Action)(() => _commands.GetTestAccessor().RunCommandBasedOnArgument(["ge.exe"])))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void RunCommandBasedOnArgument_about()
    {
        UserEnvironmentInformation.Initialise("1234567", isDirty: true);
        RunCommandBasedOnArgument<FormAbout>(["ge.exe", "about"]);
    }

    [TestCase("add")]
    [TestCase("addfiles")]
    public void RunCommandBasedOnArgument_add(string command)
        => RunCommandBasedOnArgument<FormAddFiles>(["ge.exe", command]);

    [TestCase("apply")]
    [TestCase("applypatch")]
    public void RunCommandBasedOnArgument_apply(string command)
        => RunCommandBasedOnArgument<FormApplyPatch>(["ge.exe", command]);

    [Test]
    public void RunCommandBasedOnArgument_blame_throws()
    {
        ((Action)(() => _commands.GetTestAccessor().RunCommandBasedOnArgument(["ge.exe", "blame"])))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void RunCommandBasedOnArgument_blame()
        => RunCommandBasedOnArgument<FormBlame>(["ge.exe", "blame", "filename"]);

    [Test]
    public void RunCommandBasedOnArgument_blame_line()
        => RunCommandBasedOnArgument<FormBlame>(["ge.exe", "blame", "filename", "42"]);

    [Test]
    public void RunCommandBasedOnArgument_branch()
    {
        string branchName = _referenceRepository.Module.GetCurrentBranchName();

        string newBranchName = $"{branchName}-subbranch";

        _referenceRepository.CheckoutBranch(branchName);
        RunCommandBasedOnArgument<FormCreateBranch>(["ge.exe", "branch"], runTest: form =>
        {
            SetText(form, "BranchNameTextBox", newBranchName);
            ClickButton(form, "cmdOk");
        });
    }

    [Test]
    public void RunCommandBasedOnArgument_browse()
    {
        ObjectId selected = ObjectId.Random();
        ObjectId first = ObjectId.Random();
        ObjectId otherIgnored = ObjectId.Random();
        RunCommandBasedOnArgument<FormBrowse>(["ge.exe", "browse", $"-commit={selected},{first},,{otherIgnored}"]);
    }

    [TestCase("checkout")]
    [TestCase("checkoutbranch")]
    public void RunCommandBasedOnArgument_checkout(string command)
        => RunCommandBasedOnArgument<FormCheckoutBranch>(["ge.exe", command], expectedResult: false);

    [Test]
    public void RunCommandBasedOnArgument_checkoutrevision()
        => RunCommandBasedOnArgument<FormCheckoutRevision>(["ge.exe", "checkoutrevision"], expectedResult: false);

    [Test]
    public void RunCommandBasedOnArgument_cherry()
        => RunCommandBasedOnArgument<FormCherryPick>(["ge.exe", "cherry"], expectedResult: false);

    [Test]
    public void RunCommandBasedOnArgument_cleanup()
        => RunCommandBasedOnArgument<FormCleanupRepository>(["ge.exe", "cleanup"]);

    [Test]
    public void RunCommandBasedOnArgument_clone()
        => RunCommandBasedOnArgument<FormClone>(["ge.exe", "clone"]);

    [Test]
    public void RunCommandBasedOnArgument_commit()
        => RunCommandBasedOnArgument<FormCommit>(["ge.exe", "commit"]);

    [TestCase(FirstFullShaPlaceholder, SecondFullShaPlaceholder)]
    [TestCase(FirstShortShaPlaceholder, "HEAD")]
    [TestCase(CompareBranchName, "HEAD")]
    [TestCase(CompareTagName, "HEAD")]
    [TestCase("HEAD~1", SecondFullShaPlaceholder)]
    public void RunCommandBasedOnArgument_compare_should_open_FormDiff(string firstArg, string secondArg)
    {
        string firstCommit = _referenceRepository.CommitHash ?? throw new InvalidOperationException("The reference repository has no commit.");
        string secondCommit = _referenceRepository.CreateCommit("second commit", "second content");
        _referenceRepository.CreateBranch(CompareBranchName, firstCommit);
        _referenceRepository.CreateAnnotatedTag(CompareTagName, firstCommit, "annotated tag");

        firstArg = ReplacePlaceholders(firstArg);
        secondArg = ReplacePlaceholders(secondArg);

        RunCommandBasedOnArgument<FormDiff>(["ge.exe", CompareCommand, firstArg, secondArg], runTest: form =>
        {
            FormDiff.TestAccessor accessor = form.GetTestAccessor();
            accessor.FirstId.Should().Be(ObjectId.Parse(firstCommit));
            accessor.SecondId.Should().Be(ObjectId.Parse(secondCommit));
            GetText(form, "lblFirstCommit").Should().Be(ExpectedDisplayString(firstArg, firstCommit));
            GetText(form, "lblSecondCommit").Should().Be(ExpectedDisplayString(secondArg, secondCommit));
        });

        return;

        static string ExpectedDisplayString(string arg, string commit)
            => commit.StartsWith(arg, StringComparison.OrdinalIgnoreCase) ? ObjectId.Parse(commit).ToShortString() : arg;

        string ReplacePlaceholders(string arg)
            => arg switch
            {
                FirstFullShaPlaceholder => firstCommit,
                FirstShortShaPlaceholder => firstCommit[..ShortShaLength],
                SecondFullShaPlaceholder => secondCommit,
                _ => arg
            };
    }

    [TestCase("no-such-revision", "HEAD")]
    [TestCase("HEAD", "no-such-revision")]
    [TestCase(NonexistentFullSha, "HEAD")]
    [TestCase("HEAD", NonexistentFullSha)]
    public void RunCommandBasedOnArgument_compare_should_return_false_on_unknown_revision(string firstArg, string secondArg)
    {
        _commands.GetTestAccessor().RunCommandBasedOnArgument(["ge.exe", CompareCommand, firstArg, secondArg])
            .Should().BeFalse();
        Application.OpenForms.OfType<FormDiff>().Should().BeEmpty();
    }

    [Test]
    public void RunCommandBasedOnArgument_compare_should_return_false_on_full_sha_of_non_commit()
    {
        string treeSha = _referenceRepository.Module.GitExecutable.GetOutput("rev-parse HEAD^{tree}").Trim();
        ObjectId.TryParse(treeSha, out _).Should().BeTrue();

        _commands.GetTestAccessor().RunCommandBasedOnArgument(["ge.exe", CompareCommand, treeSha, "HEAD"])
            .Should().BeFalse();
        Application.OpenForms.OfType<FormDiff>().Should().BeEmpty();
    }

    [Test]
    public void RunCommandBasedOnArgument_compare_should_return_false_without_revisions()
    {
        _commands.GetTestAccessor().RunCommandBasedOnArgument(["ge.exe", CompareCommand])
            .Should().BeFalse();
        Application.OpenForms.OfType<FormDiff>().Should().BeEmpty();
    }

    [TestCase("HEAD")]
    [TestCase("HEAD", "HEAD", "HEAD")]
    public void RunCommandBasedOnArgument_compare_should_return_false_on_wrong_number_of_revisions(params string[] revisions)
    {
        _commands.GetTestAccessor().RunCommandBasedOnArgument(["ge.exe", CompareCommand, .. revisions])
            .Should().BeFalse();
        Application.OpenForms.OfType<FormDiff>().Should().BeEmpty();
    }

    [Test]
    public void RunCommandBasedOnArgument_difftool_returns_false_on_missing_argument()
    {
        _commands.GetTestAccessor().RunCommandBasedOnArgument(["ge.exe", "difftool"])
            .Should().BeFalse();
    }

    [Test]
    public void RunCommandBasedOnArgument_difftool()
        => _commands.GetTestAccessor().RunCommandBasedOnArgument(["ge.exe", "difftool", "filename"]).Should().BeTrue();

    [Test]
    public void RunCommandBasedOnArgument_history_throws(
        [Values("blamehistory", "filehistory")] string command)
    {
        ((Action)(() => _commands.GetTestAccessor().RunCommandBasedOnArgument(["ge.exe", command])))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void RunCommandBasedOnArgument_history(
        [Values("blamehistory", "filehistory")] string command,
        [Values(false, true)] bool commit,
        [Values(false, true)] bool filter)
    {
        string expectedTab = command.Contains("blame") ? "Blame" : "Diff";

        List<string> args = ["ge.exe", command, "filename"];
        if (commit)
        {
            args.Add(_referenceRepository.CommitHash!);
            if (filter)
            {
                args.Add("--filter-by-revision");
            }
        }

        RunCommandBasedOnArgument<FormFileHistory>([.. args],
            runTest: form => form.FindDescendantOfType<FullBleedTabControl>(_ => true)!.SelectedTab!.Text.Should().Be(expectedTab));
    }

    [Test]
    public void RunCommandBasedOnArgument_history_returns_false(
        [Values("blamehistory", "filehistory")] string command,
        [Values(0, 1, 2, 3)] int invalidVariant)
    {
        const bool ignored = false;
        (bool commitValid, bool filter, bool filterValid)[] invalidVariants =
        [
            (false, false, ignored),
            (false, true, false),
            (false, true, true),
            (true, true, false)
        ];
        (bool commitValid, bool filter, bool filterValid) = invalidVariants[invalidVariant];

        List<string> args = ["ge.exe", command, "filename", commitValid ? _referenceRepository.CommitHash! : "no-commit"];
        if (filter)
        {
            args.Add(filterValid ? "--filter-by-revision" : "invalid");
        }

        _commands.GetTestAccessor().RunCommandBasedOnArgument([.. args]).Should().BeFalse();
    }

    [Test]
    public void RunCommandBasedOnArgument_fileeditor_throws()
    {
        ((Action)(() => _commands.GetTestAccessor().RunCommandBasedOnArgument(["ge.exe", "fileeditor"])))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void RunCommandBasedOnArgument_fileeditor()
        => RunCommandBasedOnArgument<FormEditor>(["ge.exe", "fileeditor", "filename"]);

    [Test]
    public void RunCommandBasedOnArgument_formatpatch()
        => RunCommandBasedOnArgument<FormFormatPatch>(["ge.exe", "formatpatch"]);

    [Test]
    public void RunCommandBasedOnArgument_gitignore()
        => RunCommandBasedOnArgument<FormGitIgnore>(["ge.exe", "gitignore"]);

    [Test]
    public void RunCommandBasedOnArgument_init()
        => RunCommandBasedOnArgument<FormInit>(["ge.exe", "init"]);

    [Test]
    public void RunCommandBasedOnArgument_merge()
        => RunCommandBasedOnArgument<FormMergeBranch>(["ge.exe", "merge"]);

    [TestCase("mergeconflicts")]
    [TestCase("mergetool")]
    public void RunCommandBasedOnArgument_mergeconflicts(string command)
        => RunCommandBasedOnArgument<FormResolveConflicts>(["ge.exe", command]);

    [Test]
    public void RunCommandBasedOnArgument_openrepo()
      => RunCommandBasedOnArgument<FormBrowse>(["ge.exe", "openrepo"]);

    [Test]
    public void RunCommandBasedOnArgument_pull()
        => RunCommandBasedOnArgument<FormPull>(["ge.exe", "pull"], expectedResult: false);

    [Test]
    public void RunCommandBasedOnArgument_push()
        => RunCommandBasedOnArgument<FormPush>(["ge.exe", "push"], expectedResult: false);

    [Test]
    public void RunCommandBasedOnArgument_rebase()
        => RunCommandBasedOnArgument<FormRebase>(["ge.exe", "rebase"]);

    [Test]
    public void RunCommandBasedOnArgument_remotes()
        => RunCommandBasedOnArgument<FormRemotes>(["ge.exe", "remotes"]);

    [TestCase("revert")]
    [TestCase("reset")]
    public void RunCommandBasedOnArgument_reset(string command)
        => RunCommandBasedOnArgument<FormResetChanges>(["ge.exe", command], expectedResult: false);

    [Test]
    public void RunCommandBasedOnArgument_searchfile()
        => RunCommandBasedOnArgument<SearchWindow<string>>(["ge.exe", "searchfile"], expectedResult: false);

    [Test]
    public void RunCommandBasedOnArgument_settings()
        => RunCommandBasedOnArgument<FormSettings>(["ge.exe", "settings"], expectedResult: false /* because dialog is not closed using OK button */);

    [Test]
    public void RunCommandBasedOnArgument_stash()
        => RunCommandBasedOnArgument<FormStash>(["ge.exe", "stash"]);

    [Test]
    public void RunCommandBasedOnArgument_synchronize()
    {
        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            while (true)
            {
                await UITest.WaitForIdleAsync();
                FormPull? formPull = Application.OpenForms.OfType<FormPull>().FirstOrDefault();
                if (formPull is not null)
                {
                    formPull.Close();
                    break;
                }
            }

            while (true)
            {
                await UITest.WaitForIdleAsync();
                FormPush? formPush = Application.OpenForms.OfType<FormPush>().FirstOrDefault();
                if (formPush is not null)
                {
                    formPush.Close();
                    break;
                }
            }
        });

        RunCommandBasedOnArgument<FormCommit>(["ge.exe", "synchronize"], expectedResult: false);
    }

    [Test]
    public void RunCommandBasedOnArgument_tag()
        => RunCommandBasedOnArgument<FormCreateTag>(["ge.exe", "tag"], expectedResult: false);

    [Test]
    public void RunCommandBasedOnArgument_viewdiff()
        => RunCommandBasedOnArgument<FormLog>(["ge.exe", "viewdiff"], expectedResult: false);

    [TestCase("git://")]
    [TestCase("http://")]
    [TestCase("https://")]
    [TestCase("github-windows://openRepo/")]
    [TestCase("github-mac://openRepo/")]
    public void RunCommandBasedOnArgument_Url(string url)
        => RunCommandBasedOnArgument<FormClone>(["ge.exe", url]);

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("help")]
    [TestCase("nonsense")]
    public void RunCommandBasedOnArgument_unsupported(string command)
        => RunCommandBasedOnArgument<FormCommandlineHelp>(["ge.exe", command]);

    private void RunCommandBasedOnArgument<TForm>(string[] args, bool expectedResult = true, Action<TForm>? runTest = null) where TForm : Form
    {
        UITest.RunForm<TForm>(
            showForm: () => _commands.GetTestAccessor().RunCommandBasedOnArgument(args).Should().Be(expectedResult),
            runTestAsync: form =>
            {
                runTest?.Invoke(form);

                return Task.CompletedTask;
            });
    }

    private static void ClickButton(Form form, string buttonName)
        => form.FindDescendantOfType<Button>(button => button.Name == buttonName)!.PerformClick();

    private static string GetText(Form form, string controlName)
        => (form.FindDescendantOfType<Control>(control => control.Name == controlName)
            ?? throw new InvalidOperationException($"Control '{controlName}' not found.")).Text;

    private static void SetText(Form form, string controlName, string value)
        => form.FindDescendantOfType<Control>(control => control.Name == controlName)!.Text = value;
}
