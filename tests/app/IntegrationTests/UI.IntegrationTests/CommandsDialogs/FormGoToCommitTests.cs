using CommonTestUtils;
using GitUI;
using GitUI.CommandsDialogs.BrowseDialog;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FormGoToCommitTests
{
    /// <summary>
    ///  The tab stops of the dialog, in tab order.
    /// </summary>
    private static readonly string[] _tabStops =
    [
        "textboxCommitExpression",
        "comboBoxTags",
        "comboBoxBranches",
        "goButton",
        "linkGitRevParse"
    ];

    // Created once for the fixture
    private ReferenceRepository _referenceRepository = null!;

    // Created once for each test
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

    [Test]
    public void Focus_should_visit_every_tab_stop_when_tabbing_forwards()
    {
        RunFormTest(form =>
            WalkFocusFromFirstTabStop(form, forward: true)
                .Should().Equal(_tabStops.Skip(1).Append(_tabStops[0])));
    }

    [Test]
    public void Focus_should_visit_every_tab_stop_when_tabbing_backwards()
    {
        RunFormTest(form =>
            WalkFocusFromFirstTabStop(form, forward: false)
                .Should().Equal(_tabStops.Skip(1).Reverse().Append(_tabStops[0])));
    }

    /// <summary>
    ///  Moves the focus around the whole dialog, starting at the first tab stop, and reports the
    ///  names of the controls which received the focus.
    /// </summary>
    private static List<string> WalkFocusFromFirstTabStop(Form form, bool forward)
    {
        form.Controls.Find(_tabStops[0], searchAllChildren: true).Single().Focus();

        List<string> focused = [];
        for (int step = 0; step < _tabStops.Length; ++step)
        {
            form.SelectNextControl(form.ActiveControl, forward, tabStopOnly: true, nested: true, wrap: true);
            focused.Add(form.ActiveControl?.Name ?? "<none>");
        }

        return focused;
    }

    private void RunFormTest(Action<FormGoToCommit> testDriver)
    {
        UITest.RunForm<FormGoToCommit>(
            () =>
            {
                using FormGoToCommit form = new(_commands);
                form.ShowDialog();
            },
            form =>
            {
                testDriver(form);
                form.Close();
                return Task.CompletedTask;
            });
    }
}
