using CommonTestUtils;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.UserControls.RevisionGrid;

namespace GitExtensions.UITests.UserControls.RevisionGrid;

[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class RevisionGridToolTipProviderTests
{
    // Created once for each test
    private ReferenceRepository _referenceRepository = null!;
    private GitUICommands _commands = null!;

    [OneTimeSetUp]
    public void SetUpFixture()
    {
        // We don't want avatars during tests, otherwise we will be attempting to download them from gravatar.
        AppSettings.ShowAuthorAvatarColumn = false;
    }

    [SetUp]
    public void SetUp()
    {
        _referenceRepository = new ReferenceRepository();
        _referenceRepository.CreateCommit("Commit1", "Commit1");
        _referenceRepository.CreateCommit("Commit2", "Commit2");

        _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
    }

    [TearDown]
    public void TearDown()
    {
        _referenceRepository.Dispose();
    }

    [Test]
    public void Hide_should_not_shorten_or_extend_the_display_duration()
    {
        RunProviderTest(provider =>
        {
            RevisionGridToolTipProvider.TestAccessor accessor = provider.GetTestAccessor();
            int autoPopDelay = accessor.ToolTip.AutoPopDelay;

            provider.Hide();

            accessor.ToolTip.AutoPopDelay.Should().Be(autoPopDelay,
                "the display duration belongs to the tooltip, so hiding must not redefine it for all later tooltips");
        });
    }

    [Test]
    public void Clear_should_forget_the_cell_the_tooltip_was_built_for()
    {
        RunProviderTest(provider =>
        {
            provider.ShowRevisionGridTooltips = true;
            const int rowIndex = 1;
            provider.OnCellMouseMove(new DataGridViewCellMouseEventArgs(columnIndex: 1, rowIndex, localX: 0, localY: 0,
                new MouseEventArgs(MouseButtons.None, clicks: 0, x: 0, y: 0, delta: 0)), hitInfo: null);

            RevisionGridToolTipProvider.TestAccessor accessor = provider.GetTestAccessor();
            accessor.PreviousRowIndex.Should().Be(rowIndex);

            provider.Clear();

            accessor.PreviousRowIndex.Should().Be(-1,
                "a row index no longer identifies the same revision after the revisions have been reloaded");
        });
    }

    private void RunProviderTest(Action<RevisionGridToolTipProvider> testDriver)
    {
        UITest.RunForm<FormBrowse>(
            showForm: () => _commands.StartBrowseDialog(owner: null).Should().BeTrue(),
            runTestAsync: async formBrowse =>
            {
                RevisionGridControl revisionGrid = formBrowse.RevisionGridControl;
                UITest.ProcessUntil("Loading Revisions", () => revisionGrid.GetTestAccessor().IsDataLoadComplete);
                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                testDriver(revisionGrid.GetTestAccessor().ToolTipProvider);
            });
    }
}
