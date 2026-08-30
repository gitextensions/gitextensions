using AwesomeAssertions;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitUI.SettingControlBindings;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Category("P0_1")]
public sealed class EndToEndCaptureTests
{
    [Test]
    public void StageCapturePlan_should_replace_the_packaged_default()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.ParityPlan-{Guid.NewGuid():N}");
        string sourceDirectory = Path.Combine(directory, "source");
        string runtimeDirectory = Path.Combine(directory, "runtime");
        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(runtimeDirectory);
        string sourcePlan = Path.Combine(sourceDirectory, "capture-plan.json");
        string isolatedPlan = Path.Combine(runtimeDirectory, "capture-plan.json");
        File.WriteAllText(sourcePlan, "caller plan");
        File.WriteAllText(isolatedPlan, "packaged plan");

        try
        {
            CaptureRunner.StageCapturePlan(sourcePlan, isolatedPlan);

            File.ReadAllText(isolatedPlan).Should().Be("caller plan");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

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
    public void State_driver_should_drive_the_single_checkbox_inside_a_composite_control()
    {
        using Panel composite = new();
        using CheckBox checkBox = new();
        composite.Controls.Add(checkBox);

        using (ControlStateDriver.Apply(
                   composite,
                   new CaptureStatePlan
                   {
                       Id = "checked",
                       Kind = CaptureStateKind.Checked,
                   }))
        {
            checkBox.Checked.Should().BeTrue();
        }

        checkBox.Checked.Should().BeFalse();
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void Paired_setting_binding_surfaces_should_expose_all_original_controls_and_edge_states()
    {
        using SettingControlBindingsCaptureSurface normal = new();
        using SettingControlBindingsNullCaptureSurface edge = new();
        string[] names =
        [
            "boolControl",
            "choiceControl",
            "stringControl",
            "passwordControl",
            "numberControl",
            "numberTextControl",
            "credentialsControl",
            "pseudoControl",
        ];

        names.Should().OnlyContain(name => normal.Controls.Find(name, searchAllChildren: true).Length == 1);
        normal.Controls.Find("boolControl", searchAllChildren: true).Single().Should().BeOfType<CheckBox>()
            .Which.Checked.Should().BeTrue();
        normal.Controls.Find("choiceControl", searchAllChildren: true).Single().Should().BeOfType<ComboBox>()
            .Which.SelectedItem.Should().Be("two");
        normal.Controls.Find("numberTextControl", searchAllChildren: true).Single().Text.Should().Be("1.5");

        edge.Controls.Find("boolControl", searchAllChildren: true).Single().Should().BeOfType<CheckBox>()
            .Which.CheckState.Should().Be(CheckState.Indeterminate);
        edge.Controls.Find("numberControl", searchAllChildren: true).Single().Should().BeOfType<NumericUpDown>()
            .Which.Text.Should().BeEmpty();
        edge.Controls.Find("numberTextControl", searchAllChildren: true).Single().BackColor
            .Should().Be(GitExtUtils.GitUI.Theming.OtherColors.BrightRed);
        edge.Controls.Find("credentialsControl", searchAllChildren: true).Single().Enabled.Should().BeFalse();
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
    public async Task DeleteIsolationRoot_should_retry_a_transient_apphost_lock_Async()
    {
        string isolationRoot = Path.Combine(
            Path.GetTempPath(),
            "GitExtensions.WinFormsParityCapture",
            Guid.NewGuid().ToString("N"));
        string runtimeRoot = Path.Combine(isolationRoot, "runtime");
        Directory.CreateDirectory(runtimeRoot);
        string appHostPath = Path.Combine(runtimeRoot, "GitExtensions.exe");
        await File.WriteAllTextAsync(appHostPath, "locked apphost");
        FileStream appHostLock = new(
            appHostPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        Task releaseLock = Task.Run(async () =>
        {
            await Task.Delay(250);
            await appHostLock.DisposeAsync();
        });

        try
        {
            CaptureRunner.DeleteIsolationRoot(isolationRoot);
            await releaseLock;

            Directory.Exists(isolationRoot).Should().BeFalse();
        }
        finally
        {
            await appHostLock.DisposeAsync();
            if (Directory.Exists(isolationRoot))
            {
                Directory.Delete(isolationRoot, recursive: true);
            }
        }
    }

    [Test]
    public void Repository_host_capture_should_use_HEAD_when_the_fixture_has_no_parent_commit()
    {
        ObjectId head = ObjectId.Random();

        RepositoryHostCaptureFixture.ResolveBaseRevision(head, default).Should().Be(head);
        RepositoryHostCaptureFixture.ResolveBaseRevision(head, ObjectId.IndexId).Should().Be(ObjectId.IndexId);
    }

    [TestCase(120, 959, 678)]
    [TestCase(144, 1147, 806)]
    [TestCase(192, 1524, 1061)]
    public void CalculateDpiChangedBounds_should_scale_the_client_and_retain_the_fallback_chrome(
        int targetDpi,
        int expectedWidth,
        int expectedHeight)
    {
        Rectangle bounds = CaptureRunner.CalculateDpiChangedBounds(
            new Rectangle(16, 16, 770, 550),
            new Size(754, 511),
            currentDpi: 96,
            targetDpi);

        bounds.Should().Be(new Rectangle(16, 16, expectedWidth, expectedHeight));
    }

    [TestCase(0, 96, "currentDpi")]
    [TestCase(96, 0, "targetDpi")]
    public void CalculateDpiChangedBounds_should_reject_nonpositive_DPI(
        int currentDpi,
        int targetDpi,
        string parameterName)
    {
        Action action = () => CaptureRunner.CalculateDpiChangedBounds(
            new Rectangle(0, 0, 100, 100),
            new Size(90, 90),
            currentDpi,
            targetDpi);

        action.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be(parameterName);
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void SendDpiChanged_should_drive_the_complete_per_monitor_v2_control_tree()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        using Form form = new()
        {
            AutoScaleDimensions = new SizeF(96, 96),
            AutoScaleMode = AutoScaleMode.Dpi,
            ClientSize = new Size(320, 180),
            StartPosition = FormStartPosition.Manual,
        };
        using Panel panel = new()
        {
            Bounds = new Rectangle(10, 20, 200, 100),
        };
        using Button button = new()
        {
            Bounds = new Rectangle(8, 12, 80, 24),
        };
        List<string> messageOrder = [];
        button.DpiChangedBeforeParent += (_, _) => messageOrder.Add("button.before");
        panel.DpiChangedBeforeParent += (_, _) => messageOrder.Add("panel.before");
        form.DpiChanged += (_, _) => messageOrder.Add("form.changed");
        panel.DpiChangedAfterParent += (_, _) => messageOrder.Add("panel.after");
        button.DpiChangedAfterParent += (_, _) => messageOrder.Add("button.after");
        panel.Controls.Add(button);
        form.Controls.Add(panel);
        form.Show();
        Application.DoEvents();

        Rectangle suggestedBounds = CaptureRunner.CalculateDpiChangedBounds(
            NativeMethods.GetWindowRectangle(form.Handle),
            form.ClientSize,
            currentDpi: 96,
            targetDpi: 120);
        NativeMethods.SendDpiChanged(form.Handle, dpi: 120, suggestedBounds);
        Application.DoEvents();

        form.DeviceDpi.Should().Be(120);
        panel.DeviceDpi.Should().Be(120);
        button.DeviceDpi.Should().Be(120);
        panel.Bounds.Should().Be(new Rectangle(12, 25, 250, 125));
        button.Bounds.Should().Be(new Rectangle(10, 15, 100, 30));
        messageOrder.Should().Equal(
            "button.before",
            "panel.before",
            "form.changed",
            "panel.after",
            "button.after");
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void EnsureManagedControlDpi_should_transition_a_control_created_after_its_parent()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        using Form form = new()
        {
            AutoScaleDimensions = new SizeF(96, 96),
            AutoScaleMode = AutoScaleMode.Dpi,
            ClientSize = new Size(320, 180),
            StartPosition = FormStartPosition.Manual,
        };
        form.Show();
        Application.DoEvents();
        Rectangle suggestedBounds = CaptureRunner.CalculateDpiChangedBounds(
            NativeMethods.GetWindowRectangle(form.Handle),
            form.ClientSize,
            currentDpi: 96,
            targetDpi: 120);
        NativeMethods.SendDpiChanged(form.Handle, dpi: 120, suggestedBounds);

        using ComboBox lateControl = new() { Name = "lateControl" };
        form.Controls.Add(lateControl);
        _ = lateControl.Handle;
        lateControl.DeviceDpi.Should().Be(96);

        CaptureRunner.EnsureManagedControlDpi(form, targetDpi: 120);

        lateControl.DeviceDpi.Should().Be(120);
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void State_driver_should_capture_the_native_combo_box_popup()
    {
        using Form form = new() { ClientSize = new Size(320, 180) };
        using ComboBox comboBox = new()
        {
            Name = "cbxTarget",
            DataSource = new[] { "main", "feature/visual-parity" },
            Location = new Point(20, 20),
            Width = 220,
        };
        form.Controls.Add(comboBox);
        form.Show();

        using ControlStateDriver driver = ControlStateDriver.Apply(
            form,
            new CaptureStatePlan
            {
                Id = "combo.open",
                Kind = CaptureStateKind.MenuOpen,
                TargetField = comboBox.Name,
            });

        driver.Popups.Should().BeEmpty();
        driver.ComboBoxPopups.Should().ContainSingle();
        ComboBoxPopup popup = driver.ComboBoxPopups.Single();
        popup.Owner.Should().BeSameAs(comboBox);
        popup.Bounds.Width.Should().BeGreaterThan(0);
        popup.Bounds.Height.Should().BeGreaterThan(comboBox.Height);
        CaptureSurface surface = new ControlTreeReader(form, form.DeviceDpi)
            .ReadComboBoxPopup(
                popup,
                ordinal: 0,
                primaryScreenOrigin: ImageCapture.GetPrimaryScreenBounds(form).Location);
        surface.Root.ControlKind.Should().Be("popup");
        surface.Root.Children.Select(node => node.Text)
            .Should().Equal("main", "feature/visual-parity");
        surface.Root.Colors.Background.Should().StartWith("#");
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void State_driver_should_activate_a_hidden_tab_before_focusing_its_control()
    {
        using Form form = new();
        using TabControl tabs = new() { Dock = DockStyle.Fill };
        using TabPage first = new("First");
        using TabPage second = new("Second");
        using TextBox target = new() { Name = "txtTarget" };
        second.Controls.Add(target);
        tabs.TabPages.Add(first);
        tabs.TabPages.Add(second);
        tabs.SelectedTab = first;
        form.Controls.Add(tabs);
        form.Show();

        using (ControlStateDriver.Apply(
                   form,
                   new CaptureStatePlan
                   {
                       Id = "second.focused",
                       Kind = CaptureStateKind.Focus,
                       TargetField = target.Name,
                   }))
        {
            tabs.SelectedTab.Should().BeSameAs(second);
            target.Focused.Should().BeTrue();
        }

        tabs.SelectedTab.Should().BeSameAs(first);
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void Image_capture_should_use_a_screen_grab_only_for_a_visible_native_browser()
    {
        using Form form = new();
        using TabControl tabs = new() { Dock = DockStyle.Fill };
        using TabPage first = new("First");
        using TabPage second = new("Second");
        using WebBrowser browser = new() { Dock = DockStyle.Fill };
        second.Controls.Add(browser);
        tabs.TabPages.Add(first);
        tabs.TabPages.Add(second);
        form.Controls.Add(tabs);
        form.Show();

        ImageCapture.RequiresScreenGrab(form).Should().BeFalse();

        tabs.SelectedTab = second;
        Application.DoEvents();

        ImageCapture.RequiresScreenGrab(form).Should().BeTrue();
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

    [Test]
    [Apartment(ApartmentState.STA)]
    public void State_driver_should_open_and_restore_a_tool_strip_combo_box_popup()
    {
        using ToolStripComboBoxForm form = new();
        form.Show();
        Application.DoEvents();

        using (ControlStateDriver driver = ControlStateDriver.Apply(
                   form,
                   new CaptureStatePlan
                   {
                       Id = "combo.open",
                       Kind = CaptureStateKind.MenuOpen,
                       TargetField = "_comboBox",
                   }))
        {
            form.IsDropDownOpen.Should().BeTrue();
            driver.RequiresScreenGrab.Should().BeTrue();
        }

        form.IsDropDownOpen.Should().BeFalse();
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

    [Test]
    [Apartment(ApartmentState.STA)]
    public void Text_plan_should_remain_authoritative_after_a_load_handler_changes_the_control()
    {
        using TextSeedForm form = new();
        CaptureComponentPlan component = new()
        {
            TypeName = typeof(TextSeedForm).FullName!,
            TextValues = new Dictionary<string, string>
            {
                ["_target"] = "planned\nvalue"
            },
            States = []
        };
        form.Show();
        form.TargetText.Should().Be("value from load");

        ComponentFactory.ApplyTextValues(form, component);

        form.TargetText.Should().Be($"planned{Environment.NewLine}value");
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

    private sealed class TextSeedForm : Form
    {
        private readonly TextBox _target = new();

        public TextSeedForm()
        {
            Controls.Add(_target);
            Load += (_, _) => _target.Text = "value from load";
        }

        public string TargetText
        {
            get => _target.Text;
            set => _target.Text = value;
        }
    }

    private sealed class ToolStripComboBoxForm : Form
    {
        private readonly ToolStripComboBox _comboBox = new();

        public ToolStripComboBoxForm()
        {
            _comboBox.Items.AddRange(["Current working directory changes", "stash@{0}"]);
            _comboBox.SelectedIndex = 0;
            ToolStrip toolStrip = new();
            toolStrip.Items.Add(_comboBox);
            Controls.Add(toolStrip);
        }

        public bool IsDropDownOpen => _comboBox.DroppedDown;
    }
}
