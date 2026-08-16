using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Category("P0_1")]
public sealed class EndToEndCaptureTests
{
    [Test]
    public async Task CaptureAsync_should_reject_repository_inside_working_tree_Async()
    {
        CaptureOptions options = new()
        {
            Command = CaptureCommand.Capture,
            PlanPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "capture-plan.json"),
            RepositoryPath = Environment.CurrentDirectory,
            OutputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
        };

        Func<Task> action = async () => await CaptureRunner.CaptureAsync(options);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*outside this working tree*");
    }

    [Test]
    public void Parse_should_require_capture_values()
    {
        CaptureOptions options = CaptureOptions.Parse(
        [
            "capture",
            "--plan", "plan.json",
            "--repository", "repo",
            "--output", "output",
            "--scales", "100,200"
        ]);

        options.Command.Should().Be(CaptureCommand.Capture);
        options.Scales.Should().BeEquivalentTo([100, 200]);
    }

    [Test]
    public void Parse_should_preserve_the_isolated_worker_state()
    {
        CaptureOptions options = CaptureOptions.Parse(
        [
            "--worker",
            "--state", "context-menu.open"
        ]);

        options.Command.Should().Be(CaptureCommand.Worker);
        options.StateId.Should().Be("context-menu.open");
    }

    [Test]
    [Apartment(ApartmentState.MTA)]
    public void Bootstrap_should_reject_a_non_sta_thread()
    {
        CaptureSettingsProfile profile = new()
        {
            UiFontFamily = "Segoe UI",
            UiFontSizePoints = 9,
            FixedFontFamily = "Consolas",
            FixedFontSizePoints = 10,
            AppSettings = new Dictionary<string, string>()
        };
        CaptureThemePlan theme = new()
        {
            Id = "light",
            Kind = "builtin",
            File = "invariant.css",
            IsBuiltin = true
        };

        Action action = () => WinFormsBootstrap.Create(Environment.CurrentDirectory, profile, theme, AppContext.BaseDirectory);

        action.Should().Throw<ThreadStateException>()
            .WithMessage("*STA*");
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void State_driver_should_find_a_dynamic_menu_item_by_name()
    {
        using Form form = new();
        using MenuStrip menu = new();
        using ToolStripMenuItem dynamicItem = new("Dynamic") { Name = "dynamicToolStripMenuItem" };
        dynamicItem.DropDownItems.Add("Child");
        menu.Items.Add(dynamicItem);
        form.Controls.Add(menu);
        form.Show();

        using ControlStateDriver driver = ControlStateDriver.Apply(
            form,
            new CaptureStatePlan
            {
                Id = "dynamic-menu.open",
                Kind = CaptureStateKind.MenuOpen,
                TargetField = dynamicItem.Name,
            });

        driver.Popups.Should().ContainSingle().Which.Should().BeSameAs(dynamicItem.DropDown);
        dynamicItem.DropDown.Visible.Should().BeTrue();
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void State_driver_should_reject_a_context_menu_that_declines_to_open()
    {
        using CancelingContextMenuForm form = new();
        form.Show();

        Action action = () => ControlStateDriver.Apply(
            form,
            new CaptureStatePlan
            {
                Id = "context-menu.open",
                Kind = CaptureStateKind.MenuOpen,
                TargetField = "_menuMain",
            });

        action.Should().Throw<CaptureStateUnsupportedException>()
            .WithMessage("*declined to open*");
    }

    [TestCase(true, false, 0, 48, 100, false, TestName = "Visible graph has not rendered")]
    [TestCase(true, true, 48, 48, 48, false, TestName = "Visible rows are still updating")]
    [TestCase(true, false, 48, 48, 100, false, TestName = "Rendered graph width has not been published")]
    [TestCase(true, false, 28, 48, 28, false, TestName = "Rendered graph width predates the complete visible range")]
    [TestCase(true, false, 48, 48, 48, true, TestName = "Rendered graph width is stable")]
    [TestCase(false, false, 0, 48, 100, true, TestName = "Hidden graph requires no render")]
    public void Revision_grid_capture_should_require_the_product_graph_render(
        bool graphVisible,
        bool updatingVisibleRows,
        int renderedWidth,
        int expectedWidth,
        int columnWidth,
        bool expected)
    {
        ComponentFactory.IsRevisionGridRenderReady(
            graphVisible,
            updatingVisibleRows,
            renderedWidth,
            expectedWidth,
            columnWidth).Should().Be(expected);
    }

    [TestCase(true, false, 28, 48, false, true, TestName = "Late graph data requests one product refresh")]
    [TestCase(true, false, 0, 48, false, true, TestName = "An initially missing graph render requests one product refresh")]
    [TestCase(true, false, 28, 48, true, false, TestName = "The same graph mismatch is not refreshed repeatedly")]
    [TestCase(true, true, 28, 48, false, false, TestName = "An active row update is not interrupted")]
    [TestCase(true, false, 48, 48, false, false, TestName = "A current graph render needs no refresh")]
    [TestCase(false, false, 28, 48, false, false, TestName = "A hidden graph needs no refresh")]
    public void Revision_grid_capture_should_refresh_a_late_graph_width_once(
        bool graphVisible,
        bool updatingVisibleRows,
        int renderedWidth,
        int expectedWidth,
        bool mismatchAlreadyRefreshed,
        bool expected)
    {
        ComponentFactory.ShouldRefreshRevisionGridRender(
            graphVisible,
            updatingVisibleRows,
            renderedWidth,
            expectedWidth,
            mismatchAlreadyRefreshed).Should().Be(expected);
    }

    [TestCase(false, true, true, true, true, true, TestName = "Stable HEAD selection is ready")]
    [TestCase(true, true, true, true, true, false, TestName = "Refresh still running")]
    [TestCase(false, false, true, true, true, false, TestName = "Data load incomplete")]
    [TestCase(false, true, false, true, true, false, TestName = "Selected revision is not HEAD")]
    [TestCase(false, true, true, false, true, false, TestName = "HEAD row is not selected")]
    [TestCase(false, true, true, true, false, false, TestName = "Latest row points elsewhere")]
    public void Revision_grid_capture_should_require_a_stable_HEAD_selection(
        bool isRefreshing,
        bool isDataLoadComplete,
        bool selectedRevisionIsHead,
        bool selectedRowIsSelected,
        bool latestRowMatches,
        bool expected)
    {
        ComponentFactory.IsRevisionGridSelectionReady(
            isRefreshing,
            isDataLoadComplete,
            selectedRevisionIsHead,
            selectedRowIsSelected,
            latestRowMatches).Should().Be(expected);
    }

    [TestCase(true, true, false, false, false, false, false, true, TestName = "Settled HEAD menu is ready")]
    [TestCase(false, true, false, false, false, false, false, false, TestName = "Rebase is hidden")]
    [TestCase(true, false, false, false, false, false, false, false, TestName = "Rebase is disabled")]
    [TestCase(true, true, true, false, false, false, false, false, TestName = "Stash action is stale")]
    [TestCase(true, true, false, false, false, true, false, false, TestName = "Artificial action is stale")]
    public void Revision_grid_capture_should_require_the_opened_HEAD_menu_state(
        bool rebaseVisible,
        bool rebaseEnabled,
        bool applyStashVisible,
        bool popStashVisible,
        bool dropStashVisible,
        bool resetChangesVisible,
        bool commitVisible,
        bool expected)
    {
        ComponentFactory.IsRevisionGridHeadContextMenuReady(
            rebaseVisible,
            rebaseEnabled,
            applyStashVisible,
            popStashVisible,
            dropStashVisible,
            resetChangesVisible,
            commitVisible).Should().Be(expected);
    }

    [Test]
    public void Revision_grid_capture_should_require_complete_copy_metadata()
    {
        string[] complete = ["&Message: subject", "&Author: User", "&Date: today"];
        string[] missingDate = ["&Message: subject", "&Author: User"];

        ComponentFactory.IsRevisionGridCopyMenuReady(
            complete,
            requiredLabels: ["Message", "Author"],
            dateLabels: ["Date", "Author date", "Commit date"]).Should().BeTrue();
        ComponentFactory.IsRevisionGridCopyMenuReady(
            missingDate,
            requiredLabels: ["Message", "Author"],
            dateLabels: ["Date", "Author date", "Commit date"]).Should().BeFalse();
        ComponentFactory.IsRevisionGridCopyMenuReady(
            ["&Message: subject", "&Date: today"],
            requiredLabels: ["Message", "Author"],
            dateLabels: ["Date", "Author date", "Commit date"]).Should().BeFalse();
    }

    private sealed class CancelingContextMenuForm : Form
    {
        private readonly ContextMenuStrip _menuMain = new() { Name = "menuMain" };

        public CancelingContextMenuForm()
        {
            _menuMain.Items.Add("Child");
            _menuMain.Opening += (_, e) => e.Cancel = true;
            ContextMenuStrip = _menuMain;
        }
    }
}
