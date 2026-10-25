using CommonTestUtils;
using GitUI;
using GitUI.UserControls.RevisionGrid;

namespace GitExtensions.UITests.UserControls.RevisionGrid;

[Apartment(ApartmentState.STA)]
public class FormRevisionFilterTests
{
    private ReferenceRepository _referenceRepository = null!;
    private GitUICommands _commands = null!;

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

    [TestCase(true)]
    [TestCase(false)]
    public void Simplifying_the_merges_must_be_offered_only_along_with_the_full_history(bool fullHistory)
    {
        using FormRevisionFilter form = new(_commands, new FilterInfo());
        FormRevisionFilter.TestAccessor accessor = form.GetTestAccessor();

        accessor.FullHistoryCheck.Checked = fullHistory;
        accessor.UpdateFilters();

        accessor.SimplifyMergesCheck.Enabled.Should().Be(fullHistory,
            "git ignores --simplify-merges unless --full-history is given as well");
    }
}
