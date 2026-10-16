using CommonTestUtils;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FormResolveConflictsTests
{
    private const string _fileName = "conflicted.txt";
    private const string _baseContent = "base\n";
    private const string _modifiedContent = "modified\n";

    private ReferenceRepository _referenceRepository = null!;
    private GitUICommands _commands = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Required to open the form; can only be set once, but other fixtures set it too
        AppSettings.GetTestAccessor().ResetDocumentationBaseUrl();
        AppSettings.SetDocumentationBaseUrl("33.33.33");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        AppSettings.GetTestAccessor().ResetDocumentationBaseUrl();
    }

    [SetUp]
    public void SetUp()
    {
        _referenceRepository = new ReferenceRepository();
        _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
    }

    [TearDown]
    public void TearDown()
    {
        _referenceRepository.Dispose();
    }

    [TestCase(true, TestName = "Solve_dialog_offers_a_non_closing_diff_of_base_with_modified_when_deleted_locally")]
    [TestCase(false, TestName = "Solve_dialog_offers_a_non_closing_diff_of_base_with_modified_when_deleted_remotely")]
    public void Solve_dialog_offers_a_non_closing_diff_of_base_with_modified(bool deletedLocally)
    {
        string gitDir = Path.Combine(_referenceRepository.Module.WorkingDir, ".git");
        string diffedBase = Path.Combine(gitDir, "diffed-base.txt");
        string diffedModified = Path.Combine(gitDir, "diffed-modified.txt");
        UseDifftoolCopyingItsInputsTo(gitDir, diffedBase, diffedModified);
        CreateModifyDeleteConflict(deletedLocally);
        ConflictData item = ThreadHelper.JoinableTaskFactory.Run(() => _referenceRepository.Module.GetConflictAsync(_fileName));

        RunFormTest(async form =>
        {
            FormResolveConflicts.TestAccessor accessor = form.GetTestAccessor();

            TaskDialogCommandLinkButton? diffButton = null;
            bool? dialogStayedOpenAfterDiff = null;
            accessor.SolveMergeConflictDialogPageCreated = page => page.Created += (_, _) =>
            {
                // Act on the dialog from its own message loop, once it is up.
                System.Windows.Forms.Timer timer = new() { Interval = 1 };
                timer.Tick += (_, _) =>
                {
                    timer.Dispose();
                    try
                    {
                        diffButton = page.Buttons.OfType<TaskDialogCommandLinkButton>()
                            .SingleOrDefault(button => button.Text?.StartsWith("Diff base with modified") is true);
                        if (diffButton is not null)
                        {
                            diffButton.PerformClick();
                            dialogStayedOpenAfterDiff = page.BoundDialog is not null;
                        }
                    }
                    finally
                    {
                        page.BoundDialog?.Close();
                    }
                };
                timer.Start();
            };

            bool hasRevision = deletedLocally ? accessor.CheckForLocalRevision(item) : accessor.CheckForRemoteRevision(item);

            hasRevision.Should().BeFalse();
            diffButton.Should().NotBeNull();
            diffButton!.AllowCloseDialog.Should().BeFalse("a diff is not a resolution, so it must neither close the dialog nor be applied to all files");
            dialogStayedOpenAfterDiff.Should().BeTrue();

            // The difftool was started with the two versions which exist, and the conflict is still there.
            for (int i = 0; i < 100 && !(File.Exists(diffedBase) && File.Exists(diffedModified)); ++i)
            {
                await Task.Delay(100);
            }

            File.ReadAllText(diffedBase).Should().Be(_baseContent);
            File.ReadAllText(diffedModified).Should().Be(_modifiedContent);
            (await _referenceRepository.Module.GetConflictAsync(_fileName)).Filename.Should().Be(_fileName);
        });
    }

    private void UseDifftoolCopyingItsInputsTo(string gitDir, string localCopy, string remoteCopy)
    {
        // The form insists on a mergetool with an existing executable, which is not run by the test.
        string cmdPath = (Environment.GetEnvironmentVariable("COMSPEC") ?? "C:/WINDOWS/system32/cmd.exe").ToPosixPath();
        File.AppendAllText(Path.Combine(gitDir, "config"),
            $"""
            [diff]
            guitool = copy
            [difftool "copy"]
            cmd = cp "\"$LOCAL\"" "\"{localCopy.ToPosixPath()}\"" && cp "\"$REMOTE\"" "\"{remoteCopy.ToPosixPath()}\""
            [merge]
            tool = cmd
            guitool = cmd
            [mergetool "cmd"]
            path = "{cmdPath}"

            """);
        _referenceRepository.Module.InvalidateGitSettings();
    }

    private void CreateModifyDeleteConflict(bool deletedLocally)
    {
        _referenceRepository.CreateCommit("base", _baseContent, _fileName);
        RunGit("checkout -q -b other");
        ModifyOrDelete(delete: !deletedLocally);
        RunGit("checkout -q -");
        ModifyOrDelete(delete: deletedLocally);

        // Fails with the modify/delete conflict
        _referenceRepository.Module.GitExecutable.RunCommand("merge other", throwOnErrorExit: false).Should().BeFalse();

        void ModifyOrDelete(bool delete)
        {
            if (delete)
            {
                RunGit($"rm -q {_fileName}");
                RunGit("-c user.name=test -c user.email=test@test commit -q -m deleted");
            }
            else
            {
                _referenceRepository.CreateCommit("modified", _modifiedContent, _fileName);
            }
        }

        void RunGit(string arguments)
            => _referenceRepository.Module.GitExecutable.RunCommand(arguments).Should().BeTrue(arguments);
    }

    private void RunFormTest(Func<FormResolveConflicts, Task> testDriverAsync)
    {
        UITest.RunForm(
            () =>
            {
                _commands.StartResolveConflictsDialog(owner: null, offerCommit: false);
            },
            testDriverAsync);
    }
}
