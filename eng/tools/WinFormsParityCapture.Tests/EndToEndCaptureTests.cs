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
