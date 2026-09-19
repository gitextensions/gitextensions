using CommonTestUtils;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class FormCommitSubmoduleTests
{
    private const string _submodulePath = "sub";

    // Created once for each test
    private GitModuleTestHelper _parentRepo = null!;
    private GitModuleTestHelper _submoduleRepo = null!;
    private GitUICommands _commands = null!;

    // Track the original setting value
    private bool _showAvailableDiffTools;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _showAvailableDiffTools = AppSettings.ShowAvailableDiffTools;

        // Stop loading custom diff tools
        AppSettings.ShowAvailableDiffTools = false;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        AppSettings.ShowAvailableDiffTools = _showAvailableDiffTools;
    }

    [SetUp]
    public void SetUp()
    {
        _parentRepo = new GitModuleTestHelper("parent");
        _submoduleRepo = new GitModuleTestHelper("submodule");

        _parentRepo.Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Initial commit""");

        // AddSubmodule commits the submodule; undo that commit to get the state of a freshly added
        // submodule, which is staged. Unstaging it then leaves an untracked path, which is what
        // git-status reports it as.
        _parentRepo.AddSubmodule(_submoduleRepo, _submodulePath);
        _parentRepo.Module.GitExecutable.GetOutput("reset --soft HEAD~1");

        _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _parentRepo.Module);
    }

    [TearDown]
    public void TearDown()
    {
        _parentRepo.Dispose();
        _submoduleRepo.Dispose();
    }

    [Test]
    public void Unstaging_a_new_submodule_should_leave_the_status_a_rescan_reports()
    {
        RunFormTest(form =>
        {
            FormCommit.TestAccessor ta = form.GetTestAccessor();

            GitItemStatus staged = GetSubmoduleItem(ta.StagedList);
            staged.IsSubmodule.Should().BeTrue("`git submodule add` stages the submodule");

            ta.StagedList.SelectedGitItems = [staged];
            ta.Unstage();

            bool afterUnstaging = GetSubmoduleItem(ta.UnstagedList).IsSubmodule;

            ta.RescanChanges();
            AsyncTestHelper.JoinPendingOperations();

            GetSubmoduleItem(ta.UnstagedList).IsSubmodule
                .Should().Be(afterUnstaging, "unstaging must not leave a status which the next rescan changes");

            return Task.CompletedTask;
        });

        return;

        static GitItemStatus GetSubmoduleItem(FileStatusList list)
            => list.AllItems.Items().Single(item => item.Name.TrimEnd('/') == _submodulePath);
    }

    private void RunFormTest(Func<FormCommit, Task> testDriverAsync)
    {
        UITest.RunForm(
            showForm: () =>
            {
                _commands.StartCommitDialog(owner: null).Should().BeTrue();

                // Await updated FileViewer
                AsyncTestHelper.JoinPendingOperations();
            },
            testDriverAsync);
    }
}
