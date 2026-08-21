using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.ParityCapture;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Compat;
using GitUI.HelperDialogs;
using GitUI.SpellChecker;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using WinFormsFont = GitExtensions.Shims.WinForms.Font;

namespace GitExtensionsTests;

public sealed partial class ParityScreenshotTests
{
    private const string CapturePlanEnvironmentVariable = "GITEXT_CAPTURE_PARITY_PLAN";
    // parity-scaffolding: Isolate renderer-hostile capture matrix cells in separate processes.
    private const string CaptureScaleEnvironmentVariable = "GITEXT_CAPTURE_PARITY_SCALE";
    private const string CaptureThemeEnvironmentVariable = "GITEXT_CAPTURE_PARITY_THEME";
    private const string P02Category = "P0_2";

    [Test]
    [Category(P02Category)]
    public void Generic_search_capture_hosts_should_use_the_declared_shell_sizes()
    {
        GetCaptureSize(typeof(SearchControl<string>)).Should().Be((64, 23));
        GetCaptureSize(typeof(SearchWindow<string>)).Should().Be((325, 213));
    }

    [Test]
    [Category(P02Category)]
    public void Diff_patch_capture_hosts_should_use_96_dpi_designer_dimensions()
    {
        GetCaptureSize(typeof(BranchSelector)).Should().Be((325, 54));
        GetCaptureSize(typeof(FormDiff)).Should().Be((1042, 685));
        // WinForms AutoSize contracts the 110-pixel Designer client to 106 pixels with the runtime font.
        GetCaptureSize(typeof(FormCompareToBranch)).Should().Be((434, 106));
        GetCaptureSize(typeof(FormFormatPatch)).Should().Be((824, 532));
    }

    [Test]
    [Category(P02Category)]
    public void Blame_log_capture_hosts_should_use_96_dpi_designer_dimensions()
    {
        GetCaptureSize(typeof(FormBlame)).Should().Be((784, 762));
        GetCaptureSize(typeof(FormLog)).Should().Be((750, 529));
        GetCaptureSize(typeof(FormGitCommandLog)).Should().Be((659, 470));
    }

    [Test]
    [Category(P02Category)]
    public void Ignore_editor_capture_hosts_should_use_96_dpi_designer_dimensions()
    {
        GetCaptureSize(typeof(FormAddToGitIgnore)).Should().Be((599, 341));
        GetCaptureSize(typeof(FormGitIgnore)).Should().Be((634, 623));
        GetCaptureSize(typeof(FormGitAttributes)).Should().Be((634, 474));
        GetCaptureSize(typeof(FormMailMap)).Should().Be((634, 474));
    }

    [Test]
    [Category(P02Category)]
    public void Repository_maintenance_capture_hosts_should_use_native_96_dpi_client_dimensions()
    {
        GetCaptureSize(typeof(FormCleanupRepository)).Should().Be((434, 582));
        GetCaptureSize(typeof(FormBisect)).Should().Be((248, 169));
        GetCaptureSize(typeof(FormSparseWorkingCopy)).Should().Be((784, 561));
    }

    [Test]
    [Category(P02Category)]
    public void Branch_operation_capture_hosts_should_use_native_96_dpi_runtime_dimensions()
    {
        GetCaptureSize(typeof(FormDeleteRemoteBranch)).Should().Be((403, 167));
        GetCaptureSize(typeof(FormResetAnotherBranch)).Should().Be((545, 347));
    }

    [AvaloniaTest]
    [Category(P02Category)]
    public void Avalonia_tree_reader_should_measure_named_controls_from_their_nearest_semantic_owner()
    {
        Window window = new() { Width = 320, Height = 160 };
        Canvas layout = new() { Margin = new Thickness(32, 24, 0, 0) };
        Button command = new() { Name = "btnCommand", Width = 75, Height = 25 };
        layout.Children.Add(command);
        window.Content = layout;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        CaptureSurface surface = new AvaloniaControlTreeReader(window, renderScale: 1)
            .ReadPrimary(window, new PixelSize(320, 160));
        CaptureNode commandNode = Flatten(surface.Root).Single(node => node.FieldName == command.Name);

        commandNode.BoundsDip.Should().Be(
            new CaptureRectangleF { X = 32, Y = 24, Width = 75, Height = 25 });
        window.Close();
    }

    [AvaloniaTest]
    [Category(P02Category)]
    public void Avalonia_tree_reader_should_emit_label_and_group_text_without_template_children()
    {
        Label label = new() { Name = "lblValue", Content = "_Value" };
        StackPanel content = new() { Name = "contentPanel", Children = { label } };
        GroupBox group = new() { Name = "valueGroup", Header = "Group", Content = content };
        Window window = new() { Width = 320, Height = 160, Content = group };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        CaptureSurface surface = new AvaloniaControlTreeReader(window, renderScale: 1)
            .ReadPrimary(window, new PixelSize(320, 160));
        CaptureNode groupNode = Flatten(surface.Root).Single(node => node.FieldName == group.Name);
        CaptureNode labelNode = Flatten(surface.Root).Single(node => node.FieldName == label.Name);

        groupNode.Text.Should().Be("Group");
        groupNode.Children.Should().ContainSingle(node => node.FieldName == content.Name);
        labelNode.Text.Should().Be("&Value");
        labelNode.Children.Should().BeEmpty();
        window.Close();
    }

    [Test]
    [Category(P02Category)]
    public void Capture_plan_should_define_the_shared_acceptance_matrix()
    {
        CapturePlan plan = CapturePlan.Load(GetCapturePlanPath());

        plan.Scales.Should().Equal(100, 125, 150, 200);
        plan.Themes.Select(theme => theme.Id).Should().Equal("light", "dark", "parity-custom");
        plan.Components.Select(component => component.TypeName).Should().Equal(
            "GitUI.CommandsDialogs.FormDiff",
            "GitUI.CommandsDialogs.FormCompareToBranch",
            "GitUI.CommandsDialogs.FormFormatPatch");
        plan.Components.Should().OnlyContain(component => component.States.Any(state => state.Kind == CaptureStateKind.Normal));
        plan.Components.Single(component => component.TypeName.EndsWith("FormDiff", StringComparison.Ordinal))
            .States.Select(state => state.Kind).Should().Contain(CaptureStateKind.Checked);
        plan.Components.Single(component => component.TypeName.EndsWith("FormCompareToBranch", StringComparison.Ordinal))
            .TextValues["Branches"].Should().Be("feature/visual-parity");
        plan.Components.Single(component => component.TypeName.EndsWith("FormFormatPatch", StringComparison.Ordinal))
            .TextValues["OutputPath"].Should().Be("patch-output");
    }

    [AvaloniaTest]
    [Category(P02Category)]
    public void Capture_text_seeding_should_preserve_composite_editors()
    {
        EditNetSpell editor = new() { Name = "bodyEditor" };
        CaptureComponentPlan component = new()
        {
            TypeName = typeof(EditNetSpell).FullName!,
            TextValues = new Dictionary<string, string> { [editor.Name] = "Parity body" },
            States = [new CaptureStatePlan { Id = "normal", Kind = CaptureStateKind.Normal }],
        };
        TextBox textBox = editor.GetTestAccessor().TextBox;
        object? content = editor.Content;

        ApplyTextValues(editor, component);

        editor.Text.Should().Be("Parity body");
        editor.GetTestAccessor().TextBox.Should().BeSameAs(textBox);
        editor.Content.Should().BeSameAs(content);
    }

    [AvaloniaTest]
    [Category(P02Category)]
    public void Capture_text_seeding_should_close_an_editable_combo_popup()
    {
        ComboBox comboBox = new() { Name = "Branches", IsEditable = true, IsDropDownOpen = true };
        CaptureComponentPlan component = new()
        {
            TypeName = typeof(ComboBox).FullName!,
            TextValues = new Dictionary<string, string> { [comboBox.Name] = "feature/reset-target" },
            States = [new CaptureStatePlan { Id = "normal", Kind = CaptureStateKind.Normal }],
        };

        ApplyTextValues(comboBox, component);

        comboBox.Text.Should().Be("feature/reset-target");
        comboBox.IsDropDownOpen.Should().BeFalse();
    }

    [AvaloniaTest]
    [Category(P02Category)]
    public void Avalonia_tree_reader_should_emit_canonical_shared_schema()
    {
        Window window = new() { Width = 320, Height = 120 };
        Grid content = new();
        TextBox txtValue = new() { Name = "txtValue", Text = "Parity" };
        content.Children.Add(txtValue);
        window.Content = content;
        window.Show();
        window.SetRenderScaling(1.25);
        Dispatcher.UIThread.RunJobs();

        WriteableBitmap frame = window.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("The headless window did not render.");
        AvaloniaControlTreeReader reader = new(window, 1.25);
        CaptureDocument document = CreateDocument(
            window,
            reader.ReadPrimary(window, frame.PixelSize),
            frame.PixelSize,
            new CaptureThemePlan { Id = "light", Kind = "builtin", File = "invariant.css", IsBuiltin = true },
            sourceSha256: new string('A', 64),
            scalePercent: 125,
            state: "normal");

        string json = CaptureJson.Serialize(document);
        CaptureDocument roundTripped = CaptureJson.Deserialize(json);

        CaptureJson.Serialize(roundTripped).Should().Be(json);
        roundTripped.Capture.DpiMode.Should().Be(CaptureDpiMode.HeadlessRenderScale);
        roundTripped.Image.CaptureMethod.Should().Be(CaptureMethod.HeadlessSkia);
        Flatten(roundTripped.Surfaces[0].Root)
            .Should().Contain(node => node.FieldName == "txtValue" && node.Text == "Parity");
        window.Close();
    }

    [AvaloniaTest]
    [Category(P02Category)]
    public void Avalonia_tree_reader_should_emit_framework_neutral_selection_and_separator_state()
    {
        Window window = new() { Width = 320, Height = 160 };
        ListBox list = new() { ItemsSource = new[] { "Selected" }, SelectedIndex = 0 };
        Separator separator = new();
        window.Content = new StackPanel { Children = { list, separator } };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        CaptureSurface surface = new AvaloniaControlTreeReader(window, renderScale: 1)
            .ReadPrimary(window, new PixelSize(320, 160));
        CaptureNode listNode = Flatten(surface.Root).Single(node => node.Type == typeof(ListBox).FullName);
        CaptureNode separatorNode = Flatten(surface.Root).Single(node => node.Type == typeof(Separator).FullName);

        listNode.Selected.Should().BeNull("selection belongs to realized items, as it does in the WinForms grid tree");
        separatorNode.Enabled.Should().BeFalse();
        separatorNode.ControlKind.Should().Be("menuItem");
        separatorNode.Selected.Should().BeNull();
        separatorNode.CheckState.Should().BeNull();
        window.Close();
    }

    [AvaloniaTest]
    [Category(P02Category)]
    public void Avalonia_tree_reader_should_emit_WinForms_shaped_mnemonic_text()
    {
        Window window = new() { Width = 320, Height = 160 };
        MenuItem command = new() { Name = "command", Header = "_Commit & __literal" };
        ContextMenu menu = new() { ItemsSource = new[] { command } };
        Button owner = new() { Name = "owner", Content = "_Open & __literal", ContextMenu = menu };
        Label literal = new() { Name = "literal", Content = new TextBlock { Text = "literal_value" } };
        TranslationCompat.SetConvertMnemonics(literal, false);
        window.Content = new StackPanel { Children = { owner, literal } };
        window.Show();
        menu.Open(owner);
        Dispatcher.UIThread.RunJobs();

        AvaloniaControlTreeReader reader = new(window, renderScale: 1);
        CaptureSurface primary = reader.ReadPrimary(window, new PixelSize(320, 160));
        Control popupRoot = menu.GetLogicalDescendants().OfType<Control>().First();
        CaptureSurface popup = reader.ReadSurface(popupRoot, "popup:0", new PixelRect(0, 0, 320, 160));

        Flatten(primary.Root).Single(node => node.FieldName == "owner").Text.Should().Be("&Open && _literal");
        Flatten(primary.Root).Single(node => node.FieldName == "literal").Text.Should().Be("literal_value");
        Flatten(popup.Root).Single(node => node.Name == "command").Text.Should().Be("&Commit && _literal");
        primary.Root.Text.Should().BeEmpty("WinForms records controls without a text property as empty text");
        Control overlayHost = window.GetVisualDescendants().OfType<Control>()
            .Single(control => control.GetType().Name == "OverlayPopupHost");
        CaptureSurface overlaySurface = reader.ReadSurface(
                overlayHost,
                "popup:host",
                new PixelRect(0, 0, 320, 160));
        overlaySurface.Root.Text.Should().BeNull("the semantic ContextMenu maps to WinForms' textless ContextMenuStrip surface");
        overlaySurface.Root.Type.Should().Be(typeof(ContextMenu).FullName);
        overlaySurface.Root.ControlKind.Should().Be("popup");
        menu.Close();
        window.Close();
    }

    [AvaloniaTest]
    [Category(P02Category)]
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public void Avalonia_tree_reader_should_emit_semantic_menu_items_without_template_artifacts()
    {
        Window window = new() { Width = 320, Height = 160 };
        MenuItem child = new()
        {
            Name = "dynamicCommand",
            Header = "_Child",
            ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = true,
        };
        MenuItem command = new() { Name = "mnuCommand", Header = "_Command", ItemsSource = new[] { child } };
        ContextMenu menu = new() { Name = "menu", ItemsSource = new[] { command } };
        Button owner = new() { Name = "owner", Content = "Owner", ContextMenu = menu };
        window.Content = owner;
        window.Show();
        menu.Open(owner);
        Dispatcher.UIThread.RunJobs();

        Control overlayHost = window.GetVisualDescendants().OfType<Control>()
            .Single(control => control.GetType().Name == "OverlayPopupHost");
        CaptureSurface surface = new AvaloniaControlTreeReader(window, renderScale: 1)
            .ReadSurface(overlayHost, "popup:0", new PixelRect(17, 23, 320, 160));
        CaptureNode[] nodes = Flatten(surface.Root).ToArray();

        surface.Root.BoundsPx.Should().Be(new CaptureRectangle { X = 17, Y = 23, Width = 320, Height = 160 });
        surface.Root.BoundsDip.Should().Be(new CaptureRectangleF { X = 17, Y = 23, Width = 320, Height = 160 });
        surface.Root.Type.Should().Be(typeof(ContextMenu).FullName);
        surface.Root.AutoSize.Should().BeTrue();
        surface.Root.Alignment.Should().BeNull();
        surface.Root.TabIndex.Should().BeNull();
        surface.Root.TabStop.Should().BeNull();
        surface.Root.Focused.Should().BeFalse();
        surface.Root.Expanded.Should().BeTrue();
        surface.Root.BorderWidthDip.Should().BeNull();
        nodes.Should().ContainSingle(node => node.Name == "mnuCommand");
        nodes.Should().ContainSingle(node => node.Name == "dynamicCommand" && node.FieldName == null);
        nodes.Single(node => node.Name == "mnuCommand").CheckState.Should().Be("Unchecked");
        nodes.Single(node => node.Name == "dynamicCommand").CheckState.Should().Be("Checked");
        nodes.Should().NotContain(node => node.Type != null && node.Type.Contains("AccessText", StringComparison.Ordinal));
        CaptureNode commandNode = nodes.Single(node => node.Name == "mnuCommand");
        commandNode.AutoSize.Should().BeTrue();
        commandNode.Alignment.Should().Be("MiddleCenter");
        commandNode.TabIndex.Should().BeNull();
        commandNode.TabStop.Should().BeNull();
        commandNode.BorderWidthDip.Should().BeNull();
        commandNode.CornerRadiusDip.Should().BeNull();
        commandNode.Colors.SelectionBackground.Should().NotBeNull();
        commandNode.Colors.DisabledForeground.Should().NotBeNull();
        command.IsSubMenuOpen = true;
        Dispatcher.UIThread.RunJobs();
        CaptureSurface submenuSurface = window.GetVisualDescendants().OfType<Control>()
            .Where(control => control.GetType().Name == "OverlayPopupHost")
            .Select(control => new AvaloniaControlTreeReader(window, renderScale: 1)
                .ReadSurface(control, "popup:submenu", new PixelRect(0, 0, 320, 160)))
            .Single(surface => surface.Root.Type.Contains("OverlayPopupHost", StringComparison.Ordinal));
        submenuSurface.Root.Font!.Family.Should().Be(command.FontFamily.Name);
        submenuSurface.Root.Font.SizePoints.Should().Be(9);
        submenuSurface.Root.Colors.Background.Should().NotBeNull();
        submenuSurface.Root.Colors.DisabledForeground.Should().NotBeNull();
        Flatten(submenuSurface.Root).Should().ContainSingle(node => node.Name == "dynamicCommand");
        command.IsSubMenuOpen = false;
        menu.Close();
        window.Close();
    }

    [AvaloniaTest]
    [Category(P02Category)]
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public void Avalonia_tree_reader_should_emit_closed_submenu_separator_as_not_visible()
    {
        Window window = new() { Width = 320, Height = 160 };
        Separator separator = new() { Name = "sepChild" };
        MenuItem parent = new()
        {
            Name = "mnuParent",
            Header = "Parent",
            ItemsSource = new Control[]
            {
                new MenuItem { Name = "mnuChild", Header = "Child" },
                separator
            }
        };
        ContextMenu menu = new() { ItemsSource = new[] { parent } };
        Button owner = new() { Content = "Owner", ContextMenu = menu };
        window.Content = owner;
        window.Show();
        menu.Open(owner);
        Dispatcher.UIThread.RunJobs();

        Control overlayHost = window.GetVisualDescendants().OfType<Control>()
            .Single(control => control.GetType().Name == "OverlayPopupHost");
        CaptureNode[] nodes = Flatten(new AvaloniaControlTreeReader(window, renderScale: 1)
                .ReadSurface(overlayHost, "popup:0", new PixelRect(0, 0, 320, 160))
                .Root)
            .ToArray();

        nodes.Single(node => node.Name == "mnuParent").Visible.Should().BeTrue();
        nodes.Single(node => node.Name == "mnuChild").Visible.Should().BeFalse();
        nodes.Single(node => node.Name == "sepChild").Visible.Should().BeFalse();
        menu.Close();
        window.Close();
    }

    [AvaloniaTest]
    [Category(P02Category)]
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public void Avalonia_tree_reader_should_emit_revision_grid_layout_and_effective_state_semantics()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        RevisionGridControl revisionGrid = new();
        revisionGrid.FindControl<ListBox>("_gridView")!.ItemsSource =
            new[] { new GitRevision(ObjectId.Random()) { Subject = "Captured revision" } };
        Window window = new() { Width = 682, Height = 235, Content = revisionGrid };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        ListBox gridView = revisionGrid.FindControl<ListBox>("_gridView")
            ?? throw new InvalidOperationException("The revision grid list was not created.");
        using (AvaloniaControlStateDriver.Apply(
                   revisionGrid,
                   new CaptureStatePlan
                   {
                       Id = "revision-grid.focus",
                       Kind = CaptureStateKind.Focus,
                       TargetField = "_gridView"
                   }))
        {
            CaptureSurface enabledSurface = new AvaloniaControlTreeReader(revisionGrid, renderScale: 1)
                .ReadPrimary(revisionGrid, new PixelSize(682, 235));
            CaptureNode rootNode = enabledSurface.Root;
            CaptureNode gridNode = rootNode.Children.Single(node => node.FieldName == "_gridView");

            rootNode.BorderStyle.Should().Be("None");
            rootNode.Anchor.Should().Equal("Top", "Left");
            rootNode.Dock.Should().Be("None");
            rootNode.AutoSize.Should().BeFalse();
            rootNode.Alignment.Should().BeNull();
            rootNode.TabIndex.Should().Be(0);
            rootNode.TabStop.Should().BeTrue();
            rootNode.Expanded.Should().BeFalse();
            gridNode.BorderStyle.Should().Be("None");
            gridNode.Anchor.Should().Equal("Top", "Left");
            gridNode.Dock.Should().Be("Fill");
            gridNode.AutoSize.Should().BeFalse();
            gridNode.Alignment.Should().BeNull();
            gridNode.TabIndex.Should().Be(0);
            gridNode.TabStop.Should().BeTrue();
            gridNode.Focused.Should().BeTrue();
            gridNode.Expanded.Should().BeFalse();
            decimal resolvedRowHeight = decimal.Round((decimal)RevisionGridControl.GetRowHeight(revisionGrid), 4);
            gridNode.ItemHeightDip.Should().Be(resolvedRowHeight);

            ContextMenu contextMenu = new() { ItemsSource = new[] { new MenuItem { Header = "Command" } } };
            gridView.ContextMenu = contextMenu;
            contextMenu.Open(gridView);
            Dispatcher.UIThread.RunJobs();
            CaptureNode popupOwnerNode = new AvaloniaControlTreeReader(revisionGrid, renderScale: 1)
                .ReadPrimary(revisionGrid, new PixelSize(682, 235))
                .Root.Children.Single(node => node.FieldName == "_gridView");
            popupOwnerNode.Focused.Should().BeTrue("an owned menu retains WinForms-equivalent grid focus");
            contextMenu.Close();
        }

        revisionGrid.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();
        CaptureNode disabledGridNode = new AvaloniaControlTreeReader(revisionGrid, renderScale: 1)
            .ReadPrimary(revisionGrid, new PixelSize(682, 235))
            .Root.Children.Single(node => node.FieldName == "_gridView");
        disabledGridNode.Enabled.Should().BeFalse("child state must include inherited Avalonia disablement");
        window.Close();
    }

    [AvaloniaTest]
    [Category(P02Category)]
    public void Avalonia_state_driver_should_apply_supported_states_and_reject_wrong_targets()
    {
        foreach (CaptureStateKind kind in new[]
                 {
                     CaptureStateKind.Focus,
                     CaptureStateKind.Disabled,
                     CaptureStateKind.Hover,
                     CaptureStateKind.Pressed
                 })
        {
            Window window = new() { Width = 240, Height = 100 };
            Button target = new() { Name = "btnTarget", Content = "Target" };
            window.Content = target;
            window.Show();
            Dispatcher.UIThread.RunJobs();

            CaptureStatePlan state = new() { Id = kind.ToString(), Kind = kind, TargetField = "btnTarget" };
            using (AvaloniaControlStateDriver.Apply(window, state))
            {
                switch (kind)
                {
                    case CaptureStateKind.Focus:
                        target.IsFocused.Should().BeTrue();
                        break;
                    case CaptureStateKind.Disabled:
                        target.IsEnabled.Should().BeFalse();
                        break;
                    case CaptureStateKind.Hover:
                        target.IsPointerOver.Should().BeTrue();
                        break;
                    case CaptureStateKind.Pressed:
                        target.IsPressed.Should().BeTrue();
                        break;
                }

                window.CaptureRenderedFrame().Should().NotBeNull();
            }

            window.Close();
        }

        Window menuWindow = new() { Width = 240, Height = 100 };
        MenuItem childMenu = new() { Header = "Child" };
        MenuItem parentMenu = new()
        {
            Name = "mnuParent",
            Header = "Parent",
            ItemsSource = new[] { childMenu }
        };
        menuWindow.Content = new Menu { ItemsSource = new[] { parentMenu } };
        menuWindow.Show();
        Dispatcher.UIThread.RunJobs();
        CaptureStatePlan menuState = new()
        {
            Id = "menu.open",
            Kind = CaptureStateKind.MenuOpen,
            TargetField = "mnuParent"
        };
        using (AvaloniaControlStateDriver menuDriver = AvaloniaControlStateDriver.Apply(menuWindow, menuState))
        {
            parentMenu.IsSubMenuOpen.Should().BeTrue();
            menuDriver.PopupSurfaceRoots.Should().ContainSingle(
                "headless Skia renders the real overlay popup host into the owning frame");
            menuDriver.RequiresExternalSurfaceCapture.Should().BeFalse(
                "an overlay popup host is not a separate top-level capture");
            childMenu.IsPointerOver.Should().BeFalse();
            childMenu.IsSelected.Should().BeFalse();
        }

        menuWindow.Close();

        Window longMenuWindow = new() { Width = 240, Height = 100 };
        Button longMenuOwner = new() { Name = "btnLongMenu", Content = "Long menu" };
        longMenuOwner.ContextMenu = new ContextMenu
        {
            ItemsSource = Enumerable.Range(1, 25).Select(index => new MenuItem { Header = $"Item {index}" }).ToArray()
        };
        longMenuWindow.Content = longMenuOwner;
        longMenuWindow.Show();
        Dispatcher.UIThread.RunJobs();
        using (AvaloniaControlStateDriver longMenuDriver = AvaloniaControlStateDriver.Apply(
                   longMenuWindow,
                   new CaptureStatePlan
                   {
                       Id = "long-menu.open",
                       Kind = CaptureStateKind.MenuOpen,
                       TargetField = "btnLongMenu"
                   }))
        {
            longMenuWindow.Height.Should().Be(900);
            longMenuDriver.PopupSurfaceRoots.Should().ContainSingle()
                .Which.Bounds.Height.Should().BeGreaterThan(100);
        }

        longMenuWindow.Close();

        Window flyoutWindow = new() { Width = 240, Height = 100 };
        Button flyoutButton = new()
        {
            Name = "btnFlyout",
            Content = "Flyout",
            Flyout = new MenuFlyout
            {
                ItemsSource = new[] { new MenuItem { Header = "Choice" } }
            }
        };
        flyoutWindow.Content = flyoutButton;
        flyoutWindow.Show();
        Dispatcher.UIThread.RunJobs();
        CaptureStatePlan flyoutState = new()
        {
            Id = "flyout.open",
            Kind = CaptureStateKind.MenuOpen,
            TargetField = "btnFlyout"
        };
        using (AvaloniaControlStateDriver flyoutDriver = AvaloniaControlStateDriver.Apply(flyoutWindow, flyoutState))
        {
            flyoutButton.Flyout!.IsOpen.Should().BeTrue();
            flyoutDriver.PopupSurfaceRoots.Should().ContainSingle();
            flyoutDriver.RequiresExternalSurfaceCapture.Should().BeFalse();
        }

        flyoutWindow.Close();

        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        string originalDictionary = AppSettings.Dictionary;
        AppSettings.Dictionary = "None";
        EditNetSpell hostedEditor = new() { Name = "editHosted" };
        Window hostedEditorWindow = new() { Width = 240, Height = 100, Content = hostedEditor };
        hostedEditorWindow.Show();
        Dispatcher.UIThread.RunJobs();
        CaptureStatePlan hostedFocusState = new()
        {
            Id = "hosted.focused",
            Kind = CaptureStateKind.Focus,
            TargetField = "editHosted"
        };
        using (AvaloniaControlStateDriver.Apply(hostedEditorWindow, hostedFocusState))
        {
            hostedEditor.GetTestAccessor().TextBox.IsFocused.Should().BeTrue();
        }

        CaptureStatePlan hostedTextBoxFocusState = new()
        {
            Id = "hosted-textbox.focused",
            Kind = CaptureStateKind.Focus,
            TargetField = "TextBox"
        };
        using (AvaloniaControlStateDriver.Apply(hostedEditorWindow, hostedTextBoxFocusState))
        {
            hostedEditor.GetTestAccessor().TextBox.IsFocused.Should().BeTrue();
        }

        hostedEditorWindow.Close();
        AppSettings.Dictionary = originalDictionary;

        Button secondTabButton = new() { Name = "btnSecondTab", Content = "Second" };
        TabItem firstTab = new() { Header = "First", Content = "First content" };
        TabItem secondTab = new() { Header = "Second", Content = secondTabButton };
        TabControl tabs = new() { ItemsSource = new[] { firstTab, secondTab }, SelectedItem = firstTab };
        Window tabWindow = new() { Width = 240, Height = 100, Content = tabs };
        tabWindow.Show();
        Dispatcher.UIThread.RunJobs();
        CaptureStatePlan hiddenTabFocusState = new()
        {
            Id = "second.focused",
            Kind = CaptureStateKind.Focus,
            TargetField = "btnSecondTab"
        };
        using (AvaloniaControlStateDriver.Apply(tabWindow, hiddenTabFocusState))
        {
            tabs.SelectedItem.Should().BeSameAs(secondTab);
            secondTabButton.IsFocused.Should().BeTrue();
        }

        tabs.SelectedItem.Should().BeSameAs(firstTab);
        tabWindow.Close();

        Window unsupportedWindow = new() { Width = 240, Height = 100 };
        unsupportedWindow.Content = new Button { Name = "btnTarget", Content = "Target" };
        unsupportedWindow.Show();
        CaptureStatePlan unsupportedState = new()
        {
            Id = "checked",
            Kind = CaptureStateKind.Checked,
            TargetField = "btnTarget"
        };

        Action applyUnsupported = () => AvaloniaControlStateDriver.Apply(unsupportedWindow, unsupportedState);

        applyUnsupported.Should().Throw<AvaloniaCaptureStateUnsupportedException>()
            .WithMessage("*ToggleButton*");
        unsupportedWindow.Close();
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2b.2b.2b.4")]
    public void Avalonia_state_driver_should_preserve_selection_when_focus_is_already_within_the_target()
    {
        Window window = new() { Width = 240, Height = 120 };
        ListBox target = new()
        {
            Name = "target",
            ItemsSource = new[] { "first", "selected", "last" },
            SelectedIndex = 1
        };
        window.Content = target;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        Control selectedContainer = target.ContainerFromIndex(1)
            ?? throw new InvalidOperationException("The selected item container was not materialized.");
        selectedContainer.Focus();
        Dispatcher.UIThread.RunJobs();

        using (AvaloniaControlStateDriver.Apply(
                   window,
                   new CaptureStatePlan
                   {
                       Id = "focused",
                       Kind = CaptureStateKind.Focus,
                       TargetField = "target"
                   }))
        {
            target.SelectedIndex.Should().Be(1);
        }

        window.Close();
    }

    [Test]
    [Category(P02Category)]
    public void Avalonia_unsupported_manifest_entries_should_not_claim_capture_artifacts()
    {
        CaptureManifestEntry entry = Unsupported(
            "GitUI.FormBrowse",
            "light",
            scalePercent: 125,
            "main-menu.open",
            "External popup capture is unavailable.");

        entry.Status.Should().Be(CaptureStateStatus.Unsupported);
        entry.DpiMode.Should().BeNull();
        entry.CaptureMethod.Should().Be(CaptureMethod.Unsupported);
        entry.ImageFile.Should().BeNull();
        entry.TreeFile.Should().BeNull();
    }

    private static async Task CaptureParityPlanAsync()
    {
        await Task.Run(RunCaptureWithIsolatedSettings);
    }

    private static void RunCaptureWithIsolatedSettings()
    {
        string settingsDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.ParitySettings-{Guid.NewGuid():N}");
        Directory.CreateDirectory(settingsDirectory);
        AppSettings.TestAccessor accessor = AppSettings.GetTestAccessor();
        Lazy<string?> originalApplicationDataPath = accessor.ApplicationDataPath;
        string userSettingsPath = AppSettings.SettingsFilePath;
        SettingsFileSnapshot userSettingsSnapshot = SettingsFileSnapshot.Take(userSettingsPath);
        try
        {
            accessor.ApplicationDataPath = new Lazy<string?>(() => settingsDirectory);
            string settingsPath = AppSettings.SettingsFilePath;
            IsPathContained(settingsPath, settingsDirectory).Should().BeTrue(
                "the capture process must use a temporary settings path");
            using GitExtSettingsCache settingsCache = GitExtSettingsCache.Create(settingsPath);
            DistributedSettings captureSettings = new(
                lowerPriority: null,
                settingsCache,
                SettingLevel.Unknown);
            AppSettings.UsingContainer(captureSettings, () =>
            {
                JoinableTaskContext joinableTaskContext = new();
                joinableTaskContext.Factory.Run(async () =>
                {
                    await Dispatcher.UIThread.InvokeAsync(CaptureParityPlanCoreAsync);
                });
            });
        }
        finally
        {
            accessor.ApplicationDataPath = originalApplicationDataPath;
            userSettingsSnapshot.AssertUnchanged(userSettingsPath);
            TestDirectory.Delete(settingsDirectory);
        }
    }

    private static async Task CaptureParityPlanCoreAsync()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        GitExtensions.Shims.WinForms.ShimHost.MessageBoxHost = new StubMessageBoxHost();
        GitExtensions.Shims.WinForms.ShimHost.Clipboard = new CaptureClipboard();

        CapturePlan plan = CapturePlan.Load(GetCapturePlanPath());
        string? viewFilter = Environment.GetEnvironmentVariable(CaptureViewEnvironmentVariable);
        IReadOnlyList<CaptureComponentPlan> components = string.IsNullOrWhiteSpace(viewFilter)
            ? plan.Components
            : plan.Components
                .Where(component => component.TypeName.Contains(viewFilter, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        components.Should().NotBeEmpty($"{CaptureViewEnvironmentVariable} should match at least one capture-plan component");
        string? themeFilter = Environment.GetEnvironmentVariable(CaptureThemeEnvironmentVariable);
        IReadOnlyList<CaptureThemePlan> themes = string.IsNullOrWhiteSpace(themeFilter)
            ? plan.Themes
            : plan.Themes.Where(theme => theme.Id.Equals(themeFilter, StringComparison.OrdinalIgnoreCase)).ToArray();
        themes.Should().NotBeEmpty($"{CaptureThemeEnvironmentVariable} should match one capture-plan theme");
        string? scaleFilter = Environment.GetEnvironmentVariable(CaptureScaleEnvironmentVariable);
        IReadOnlyList<int> scales = string.IsNullOrWhiteSpace(scaleFilter)
            ? plan.Scales
            : plan.Scales.Where(scale => scale.ToString(System.Globalization.CultureInfo.InvariantCulture) == scaleFilter).ToArray();
        scales.Should().NotBeEmpty($"{CaptureScaleEnvironmentVariable} should match one capture-plan scale");

        Dictionary<string, ViewDescriptor> descriptors = GetViewDescriptors()
            .ToDictionary(descriptor => descriptor.ClassName, StringComparer.Ordinal);
        foreach (CaptureComponentPlan component in components)
        {
            descriptors.Should().ContainKey(component.TypeName);
        }

        string outputDirectory = Path.Combine(GetOutputDirectory(), "avalonia");
        if (Directory.Exists(outputDirectory))
        {
            TestDirectory.Delete(outputDirectory);
        }

        Directory.CreateDirectory(outputDirectory);
        List<CaptureManifestEntry> entries = [];
        using CaptureContext context = new();
        string repositoryRoot = FindRepositoryRoot(Path.Combine(AppContext.BaseDirectory, "GitUI.Avalonia.Tests.dll"));
        IsPathContained(context.WorkingDirectory, repositoryRoot).Should().BeFalse(
            "capture repositories must be throwaway directories outside the working tree");
        CaptureSettingsProfile profile = LoadCaptureSettingsProfile(plan);
        CaptureSettingsSnapshot settingsSnapshot = CaptureSettingsSnapshot.Take(profile);
        try
        {
            ApplyCaptureProfile(profile);
            foreach (CaptureThemePlan theme in themes)
            {
                ApplyCaptureTheme(theme);
                foreach (int scalePercent in scales)
                {
                    foreach (CaptureComponentPlan component in components)
                    {
                        ViewDescriptor descriptor = descriptors[component.TypeName];
                        foreach (CaptureStatePlan state in component.States)
                        {
                            entries.Add(await CapturePlannedStateAsync(
                                context,
                                descriptor,
                                component,
                                theme,
                                scalePercent,
                                state,
                                outputDirectory));
                        }
                    }
                }
            }
        }
        finally
        {
            settingsSnapshot.Restore();
        }

        CaptureSetManifest manifest = new()
        {
            SchemaVersion = CaptureDocument.CurrentSchemaVersion,
            CreatedAtUtc = DateTime.UtcNow,
            ToolVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0",
            Repository = context.WorkingDirectory,
            Captures = entries
        };
        string manifestPath = Path.Combine(outputDirectory, "manifest.json");
        File.WriteAllText(manifestPath, SerializeManifest(manifest));

        int expectedCount = components.Sum(component => component.States.Count) * themes.Count * scales.Count;
        entries.Should().HaveCount(expectedCount);
        entries.Should().NotContain(entry => entry.Status == CaptureStateStatus.Failed);
        entries.Where(entry => entry.Status == CaptureStateStatus.Captured).Should().OnlyContain(
            entry => File.Exists(Path.Combine(outputDirectory, entry.ImageFile!))
                     && File.Exists(Path.Combine(outputDirectory, entry.TreeFile!)));
        await TestContext.Progress.WriteLineAsync(
            $"Captured {entries.Count(entry => entry.Status == CaptureStateStatus.Captured)} Avalonia states "
            + $"and recorded {entries.Count(entry => entry.Status == CaptureStateStatus.Unsupported)} unsupported states in {outputDirectory}");
    }

    private static async Task<CaptureManifestEntry> CapturePlannedStateAsync(
        CaptureContext context,
        ViewDescriptor descriptor,
        CaptureComponentPlan component,
        CaptureThemePlan theme,
        int scalePercent,
        CaptureStatePlan state,
        string outputRoot)
    {
        Control view = CreateView(context, descriptor.ViewType);
        Control captureHost = view;
        bool cropToComponent = false;
        (double width, double height) = GetCaptureSize(captureHost.GetType());
        if (descriptor.ViewType == typeof(WatermarkComboBox)
            || descriptor.ViewType == typeof(CaseSensitiveComboBox))
        {
            // parity-scaffolding: Code-only WinForms ComboBoxes receive this standalone capture size directly.
            view.Width = width;
            view.Height = height;
        }

        double renderScale = scalePercent / 100d;
        if (descriptor.ViewType == typeof(EditNetSpell)
            || descriptor.ViewType == typeof(FileStatusList)
            || descriptor.ViewType == typeof(RevisionGridControl)
            || descriptor.ViewType == typeof(BlameViewerSettingsPage))
        {
            // parity-scaffolding: The WinForms standalone host keeps this Designer-sized control in physical pixels.
            width = (width - 0.75) / renderScale;
            height = (height - 0.75) / renderScale;
        }

        bool isWindow = captureHost is Window;
        Window window = captureHost as Window
            ?? new Window
            {
                Title = descriptor.ClassName,
                Content = captureHost,
            };
        window.Width = width;
        window.Height = height;
        window.SizeToContent = SizeToContent.Manual;
        window.RequestedThemeVariant = Application.Current?.RequestedThemeVariant;
        if (descriptor.ViewType == typeof(RevisionGridControl)
            && state.Kind == CaptureStateKind.MenuOpen
            && state.TargetField == "viewToolStripMenuItem")
        {
            // parity-scaffolding: Size the headless screen before Show; its overlay cannot grow
            // after realization, while the real desktop submenu is taller than the component.
            view.Width = width;
            view.Height = height;
            view.HorizontalAlignment = HorizontalAlignment.Left;
            view.VerticalAlignment = VerticalAlignment.Top;
            window.Height = 900;
        }

        try
        {
            PrepareView(captureHost, context);
            window.Show();
            // parity-scaffolding: A form may restore its persisted bounds during OnOpened;
            // the paired plan's declared size remains authoritative for every state.
            window.Width = width;
            window.Height = height;
            window.SetRenderScaling(renderScale);
            if (!isWindow)
            {
                await SeedStandaloneControlAsync(captureHost, context);
            }

            if (cropToComponent)
            {
                await SeedStandaloneControlAsync(view, context);
            }

            ApplyTextValues(view, component);

            await WaitForAsyncViewsAsync(captureHost, context);
            // parity-scaffolding: Async loaders may replace seeded text; the capture plan remains authoritative.
            ApplyTextValues(view, component);
            if (view is RevisionGridControl revisionGrid)
            {
                using (AvaloniaControlStateDriver.Apply(
                           view,
                           new CaptureStatePlan
                           {
                               Id = "revision-grid.initial-focus",
                               Kind = CaptureStateKind.Focus,
                               TargetField = "_gridView",
                           }))
                {
                }

                // parity-scaffolding: Both capture frameworks must drive menus and row states from HEAD, not the row used to establish focus.
                revisionGrid.SetSelectedRevision(context.HeadRevision.ObjectId).Should().BeTrue();
            }

            Dispatcher.UIThread.RunJobs();
            if (view is FileStatusList fileStatusList)
            {
                if (OperatingSystem.IsWindows())
                {
                    string vswhere = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Microsoft Visual Studio\Installer\vswhere.exe";
                    if (File.Exists(vswhere))
                    {
                        // parity-scaffolding: Settle the original asynchronous Visual Studio discovery before the first menu state.
                        VisualStudioIntegration.Init();
                        for (int attempt = 0; attempt < 200 && !VisualStudioIntegration.IsVisualStudioInstalled; attempt++)
                        {
                            await Task.Delay(10);
                        }

                        VisualStudioIntegration.IsVisualStudioInstalled.Should().BeTrue();
                    }
                }

                // parity-scaffolding: Apply the real opening contract after the paired selection has settled.
                fileStatusList.GetTestAccessor().UpdateContextMenu().Should().BeFalse();
            }

            // parity-scaffolding: Each planned state owns a fresh headless window; activate it
            // before driving focus so a previously closed capture cannot retain the input root.
            window.Activate();
            Dispatcher.UIThread.RunJobs();
            using AvaloniaControlStateDriver driver = AvaloniaControlStateDriver.Apply(view, state);
            using WriteableBitmap primaryFrame = CaptureRenderedFrame(window);
            PixelRect primarySurfaceBounds = cropToComponent
                ? GetScreenBounds(view, window, renderScale)
                : ReferenceEquals(view, window)
                    ? GetScreenBounds(window, primaryFrame.PixelSize)
                    : GetScreenBounds(view, window, renderScale);
            List<WriteableBitmap> externalFrames = [];
            List<CapturedTopLevelFrame> capturedFrames =
            [
                new CapturedTopLevelFrame(
                    view,
                    primaryFrame,
                    GetScreenBounds(window, primaryFrame.PixelSize))
            ];
            try
            {
                foreach (TopLevel externalTopLevel in driver.ExternalTopLevels)
                {
                    WriteableBitmap externalFrame = CaptureRenderedFrame(
                        externalTopLevel,
                        "Headless Skia did not render an opened popup top level.");
                    externalFrames.Add(externalFrame);
                    capturedFrames.Add(new CapturedTopLevelFrame(
                        externalTopLevel,
                        externalFrame,
                        GetScreenBounds(externalTopLevel, externalFrame.PixelSize)));
                }

                PixelRect[] popupSurfaceBounds = driver.PopupSurfaceRoots
                    .Select(popupRoot => GetScreenBounds(
                        popupRoot,
                        TopLevel.GetTopLevel(popupRoot) ?? window,
                        renderScale))
                    .ToArray();
                PixelRect imageBounds = popupSurfaceBounds.Length > 0
                    ? UnionBounds([primarySurfaceBounds, .. popupSurfaceBounds])
                    : cropToComponent && capturedFrames.Count == 1
                        ? primarySurfaceBounds
                        : UnionBounds(capturedFrames.Select(frame => frame.ScreenBounds));
                CaptureMethod captureMethod = capturedFrames.Count == 1
                    ? CaptureMethod.HeadlessSkia
                    : CaptureMethod.HeadlessSkiaComposite;
                using RenderTargetBitmap? composite = capturedFrames.Count == 1
                    ? null
                    : ComposeTopLevels(capturedFrames, imageBounds, renderScale);
                using WriteableBitmap? componentCrop = cropToComponent
                                                         && capturedFrames.Count == 1
                                                         && popupSurfaceBounds.Length == 0
                    ? CropToComponent(primaryFrame, view, window, imageBounds.Size, renderScale)
                    : null;
                using WriteableBitmap? overlayCrop = popupSurfaceBounds.Length > 0 && capturedFrames.Count == 1
                    ? CropToScreenBounds(primaryFrame, window, imageBounds)
                    : null;
                if (componentCrop is not null)
                {
                    EnsureRenderedContent(
                        componentCrop,
                        "The owning consumer rendered a blank component region.");
                }

                Bitmap image = composite is not null
                    ? composite
                    : componentCrop is not null
                        ? componentCrop
                        : overlayCrop is not null
                            ? overlayCrop
                        : primaryFrame;

                string relativeDirectory = Path.Combine(
                    Sanitize(component.TypeName),
                    Sanitize(theme.Id),
                    scalePercent.ToString(System.Globalization.CultureInfo.InvariantCulture));
                string absoluteDirectory = Path.Combine(outputRoot, relativeDirectory);
                Directory.CreateDirectory(absoluteDirectory);
                string imagePath = Path.Combine(absoluteDirectory, $"{Sanitize(state.Id)}.png");
                string treePath = Path.Combine(absoluteDirectory, $"{Sanitize(state.Id)}.tree.json");
                using (FileStream stream = File.Create(imagePath))
                {
                    image.Save(stream, PngBitmapEncoderOptions.Default);
                }

                AvaloniaControlTreeReader reader = new(view, renderScale, primarySurfaceBounds.Position);
                List<CaptureSurface> surfaces = capturedFrames
                    .Select((capturedFrame, index) => reader.ReadSurface(
                        capturedFrame.TreeRoot,
                        index == 0 ? "primary" : $"popup:{index - 1}",
                        index == 0 ? primarySurfaceBounds : capturedFrame.ScreenBounds))
                    .ToList();
                surfaces.AddRange(driver.PopupSurfaceRoots.Select((popupRoot, index) => reader.ReadSurface(
                    popupRoot,
                    $"popup:{capturedFrames.Count - 1 + index}",
                    GetScreenBounds(
                        popupRoot,
                        TopLevel.GetTopLevel(popupRoot) ?? window,
                        renderScale))));
                CaptureDocument document = CreateDocument(
                    view,
                    surfaces,
                    imageBounds.Size,
                    captureMethod,
                    theme,
                    GetThemeSourceSha256(theme),
                    scalePercent,
                    state.Id,
                    component.TypeName);
                string treeJson = CaptureJson.Serialize(document);
                CaptureJson.Serialize(CaptureJson.Deserialize(treeJson)).Should().Be(treeJson);
                File.WriteAllText(treePath, treeJson);

                return new CaptureManifestEntry
                {
                    ComponentType = component.TypeName,
                    ThemeId = theme.Id,
                    ScalePercent = scalePercent,
                    State = state.Id,
                    Status = CaptureStateStatus.Captured,
                    Note = null,
                    DpiMode = CaptureDpiMode.HeadlessRenderScale,
                    CaptureMethod = captureMethod,
                    ImageFile = Path.GetRelativePath(outputRoot, imagePath).Replace('\\', '/'),
                    TreeFile = Path.GetRelativePath(outputRoot, treePath).Replace('\\', '/')
                };
            }
            finally
            {
                foreach (WriteableBitmap externalFrame in externalFrames)
                {
                    externalFrame.Dispose();
                }
            }
        }
        catch (AvaloniaCaptureStateUnsupportedException exception)
        {
            return Unsupported(component.TypeName, theme.Id, scalePercent, state.Id, exception.Message);
        }
        catch (Exception exception)
        {
            return new CaptureManifestEntry
            {
                ComponentType = component.TypeName,
                ThemeId = theme.Id,
                ScalePercent = scalePercent,
                State = state.Id,
                Status = CaptureStateStatus.Failed,
                Note = exception.ToString(),
                DpiMode = CaptureDpiMode.HeadlessRenderScale,
                CaptureMethod = CaptureMethod.Unsupported,
                ImageFile = null,
                TreeFile = null
            };
        }
        finally
        {
            window.Close();
            if (!ReferenceEquals(window, captureHost) && captureHost is IDisposable disposableHost)
            {
                disposableHost.Dispose();
            }

            Dispatcher.UIThread.RunJobs();
        }
    }

    private static CaptureDocument CreateDocument(
        Control root,
        CaptureSurface surface,
        PixelSize imageSize,
        CaptureThemePlan theme,
        string sourceSha256,
        int scalePercent,
        string state,
        string? componentType = null)
        => CreateDocument(
            root,
            [surface],
            imageSize,
            CaptureMethod.HeadlessSkia,
            theme,
            sourceSha256,
            scalePercent,
            state,
            componentType);

    private static CaptureDocument CreateDocument(
        Control root,
        IReadOnlyList<CaptureSurface> surfaces,
        PixelSize imageSize,
        CaptureMethod captureMethod,
        CaptureThemePlan theme,
        string sourceSha256,
        int scalePercent,
        string state,
        string? componentType = null)
    {
        int dpi = checked(scalePercent * 96 / 100);
        return new CaptureDocument
        {
            SchemaVersion = CaptureDocument.CurrentSchemaVersion,
            Component = new CaptureComponent
            {
                TypeName = componentType ?? root.GetType().FullName ?? root.GetType().Name,
                AssemblyName = root.GetType().Assembly.GetName().Name ?? "GitUI.Avalonia"
            },
            Capture = new CaptureMetadata
            {
                Framework = "avalonia",
                Theme = new CaptureTheme
                {
                    Id = theme.Id,
                    Kind = theme.Kind,
                    SourceSha256 = sourceSha256
                },
                ScalePercent = scalePercent,
                Dpi = new CaptureDpi { X = dpi, Y = dpi },
                DpiMode = CaptureDpiMode.HeadlessRenderScale,
                State = state,
                StateStatus = CaptureStateStatus.Captured,
                StateNote = null
            },
            Image = new CaptureImage
            {
                WidthPx = imageSize.Width,
                HeightPx = imageSize.Height,
                CaptureMethod = captureMethod
            },
            Surfaces = surfaces
        };
    }

    private static PixelRect GetScreenBounds(TopLevel topLevel, PixelSize frameSize)
    {
        PixelPoint origin = topLevel.PointToScreen(default);
        return new PixelRect(origin.X, origin.Y, frameSize.Width, frameSize.Height);
    }

    private static PixelRect GetScreenBounds(Control control, TopLevel topLevel, double renderScale)
    {
        Point relativeOrigin = control.TranslatePoint(default, topLevel)
            ?? throw new AvaloniaCaptureStateUnsupportedException("The popup host could not be translated into owning-window coordinates.");
        PixelPoint topLevelOrigin = topLevel.PointToScreen(default);
        return new PixelRect(
            checked(topLevelOrigin.X + ToPixel(relativeOrigin.X, renderScale)),
            checked(topLevelOrigin.Y + ToPixel(relativeOrigin.Y, renderScale)),
            Math.Max(1, ToPixel(control.Bounds.Width, renderScale)),
            Math.Max(1, ToPixel(control.Bounds.Height, renderScale)));

        static int ToPixel(double value, double scale) =>
            checked((int)Math.Round(value * scale, MidpointRounding.AwayFromZero));
    }

    private static PixelRect UnionBounds(IEnumerable<PixelRect> bounds)
    {
        PixelRect[] values = bounds.ToArray();
        if (values.Length == 0)
        {
            throw new AvaloniaCaptureStateUnsupportedException("No rendered top-level surfaces were available for capture.");
        }

        int left = values.Min(value => value.X);
        int top = values.Min(value => value.Y);
        int right = values.Max(value => value.Right);
        int bottom = values.Max(value => value.Bottom);
        return new PixelRect(left, top, checked(right - left), checked(bottom - top));
    }

    // parity-scaffolding: Exact-height combo boxes are cropped from a real owning consumer,
    // preserving the product template and layout instead of enlarging the standalone control.
    private static WriteableBitmap CropToComponent(
        WriteableBitmap source,
        Control component,
        TopLevel topLevel,
        PixelSize targetSize,
        double renderScale)
    {
        Point relativeOrigin = component.TranslatePoint(default, topLevel)
            ?? throw new AvaloniaCaptureStateUnsupportedException("The component could not be translated into owning-window coordinates.");
        int sourceX = checked((int)Math.Round(relativeOrigin.X * renderScale, MidpointRounding.AwayFromZero));
        int sourceY = checked((int)Math.Round(relativeOrigin.Y * renderScale, MidpointRounding.AwayFromZero));
        if (sourceX < 0
            || sourceY < 0
            || sourceX + targetSize.Width > source.PixelSize.Width
            || sourceY + targetSize.Height > source.PixelSize.Height)
        {
            throw new AvaloniaCaptureStateUnsupportedException("The owning consumer did not contain the requested component crop.");
        }

        using WriteableBitmap normalizedSource = new(
            source.PixelSize,
            source.Dpi,
            PixelFormat.Bgra8888,
            AlphaFormat.Unpremul);
        using (ILockedFramebuffer normalizedFramebuffer = normalizedSource.Lock())
        {
            source.CopyPixels(normalizedFramebuffer);
        }

        WriteableBitmap crop = new(targetSize, source.Dpi, PixelFormat.Bgra8888, AlphaFormat.Unpremul);
        using ILockedFramebuffer sourceFramebuffer = normalizedSource.Lock();
        using ILockedFramebuffer targetFramebuffer = crop.Lock();
        byte[] row = new byte[targetSize.Width * 4];
        for (int y = 0; y < targetSize.Height; y++)
        {
            Marshal.Copy(
                IntPtr.Add(sourceFramebuffer.Address, ((sourceY + y) * sourceFramebuffer.RowBytes) + (sourceX * 4)),
                row,
                0,
                row.Length);
            Marshal.Copy(row, 0, IntPtr.Add(targetFramebuffer.Address, y * targetFramebuffer.RowBytes), row.Length);
        }

        return crop;
    }

    private static WriteableBitmap CropToScreenBounds(
        WriteableBitmap source,
        TopLevel topLevel,
        PixelRect targetBounds)
    {
        PixelPoint sourceOrigin = topLevel.PointToScreen(default);
        int sourceX = targetBounds.X - sourceOrigin.X;
        int sourceY = targetBounds.Y - sourceOrigin.Y;
        if (sourceX < 0
            || sourceY < 0
            || sourceX + targetBounds.Width > source.PixelSize.Width
            || sourceY + targetBounds.Height > source.PixelSize.Height)
        {
            throw new AvaloniaCaptureStateUnsupportedException("The real overlay popup exceeds the enlarged capture viewport.");
        }

        using WriteableBitmap normalizedSource = new(
            source.PixelSize,
            source.Dpi,
            PixelFormat.Bgra8888,
            AlphaFormat.Unpremul);
        using (ILockedFramebuffer normalizedFramebuffer = normalizedSource.Lock())
        {
            source.CopyPixels(normalizedFramebuffer);
        }

        WriteableBitmap crop = new(targetBounds.Size, source.Dpi, PixelFormat.Bgra8888, AlphaFormat.Unpremul);
        using ILockedFramebuffer sourceFramebuffer = normalizedSource.Lock();
        using ILockedFramebuffer targetFramebuffer = crop.Lock();
        byte[] row = new byte[targetBounds.Width * 4];
        for (int y = 0; y < targetBounds.Height; y++)
        {
            Marshal.Copy(
                IntPtr.Add(sourceFramebuffer.Address, ((sourceY + y) * sourceFramebuffer.RowBytes) + (sourceX * 4)),
                row,
                0,
                row.Length);
            Marshal.Copy(row, 0, IntPtr.Add(targetFramebuffer.Address, y * targetFramebuffer.RowBytes), row.Length);
        }

        return crop;
    }

    // parity-scaffolding: Headless Skia renders each real Avalonia top level independently;
    // preserve their reported screen placement when producing the one image shared with ParityDiff.
    private static RenderTargetBitmap ComposeTopLevels(
        IReadOnlyList<CapturedTopLevelFrame> frames,
        PixelRect imageBounds,
        double renderScale)
    {
        RenderTargetBitmap composite = new(
            imageBounds.Size,
            new Vector(96 * renderScale, 96 * renderScale));
        using DrawingContext context = composite.CreateDrawingContext();
        foreach (CapturedTopLevelFrame frame in frames)
        {
            Rect destination = new(
                (frame.ScreenBounds.X - imageBounds.X) / renderScale,
                (frame.ScreenBounds.Y - imageBounds.Y) / renderScale,
                frame.Frame.PixelSize.Width / renderScale,
                frame.Frame.PixelSize.Height / renderScale);
            context.DrawImage(frame.Frame, new Rect(frame.Frame.Size), destination);
        }

        return composite;
    }

    private sealed record CapturedTopLevelFrame(
        Control TreeRoot,
        WriteableBitmap Frame,
        PixelRect ScreenBounds);

    private static void ApplyTextValues(Control root, CaptureComponentPlan component)
    {
        foreach ((string fieldName, string text) in component.TextValues)
        {
            Control? target = FindNamedControl(root, fieldName);
            if (target is null)
            {
                throw new InvalidDataException($"Text seed field '{fieldName}' was not found on {component.TypeName}.");
            }

            switch (target)
            {
                // parity-scaffolding: Seeds editable Avalonia combo boxes from the shared capture plan.
                case ComboBox comboBox when comboBox.IsEditable:
                    comboBox.Text = text;
                    comboBox.IsDropDownOpen = false;
                    break;
                // parity-scaffolding: Preserve the composite editor's visual tree while seeding
                // its product text boundary; assigning Content would replace its native TextBox.
                case EditNetSpell editNetSpell:
                    editNetSpell.Text = text;
                    break;
                case TextBox textBox:
                    textBox.Text = text;
                    break;
                case ContentControl contentControl:
                    contentControl.Content = text;
                    break;

                // parity-scaffolding: Seeds native AvaloniaEdit surfaces from the shared capture plan.
                case TextEditor textEditor:
                    textEditor.Text = text;
                    break;
                default:
                    throw new InvalidDataException($"Text seed field '{fieldName}' does not expose a supported text boundary.");
            }
        }
    }

    private static Control? FindNamedControl(Control root, string fieldName)
    {
        if (root.Name == fieldName)
        {
            return root;
        }

        foreach (Control child in root.GetLogicalChildren().OfType<Control>())
        {
            if (FindNamedControl(child, fieldName) is Control match)
            {
                return match;
            }
        }

        return null;
    }

    private static IEnumerable<CaptureNode> Flatten(CaptureNode root)
    {
        yield return root;
        foreach (CaptureNode child in root.Children)
        {
            foreach (CaptureNode descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }

    private static CaptureManifestEntry Unsupported(
        string componentType,
        string themeId,
        int scalePercent,
        string state,
        string note) =>
        new()
        {
            ComponentType = componentType,
            ThemeId = themeId,
            ScalePercent = scalePercent,
            State = state,
            Status = CaptureStateStatus.Unsupported,
            Note = note,
            DpiMode = null,
            CaptureMethod = CaptureMethod.Unsupported,
            ImageFile = null,
            TreeFile = null
        };

    private static string SerializeManifest(CaptureSetManifest manifest)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return JsonSerializer.Serialize(manifest, options) + Environment.NewLine;
    }

    private static void EnsureRenderedContent(
        WriteableBitmap bitmap,
        string unsupportedMessage = "Headless Skia returned a blank image.")
    {
        if (HasRenderedContent(bitmap))
        {
            return;
        }

        throw new AvaloniaCaptureStateUnsupportedException(unsupportedMessage);
    }

    private static WriteableBitmap CaptureRenderedFrame(
        TopLevel topLevel,
        string unsupportedMessage = "Headless Skia returned a blank image.")
    {
        for (int attempt = 0; attempt < 4; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            WriteableBitmap? frame = topLevel.CaptureRenderedFrame();
            if (frame is not null && HasRenderedContent(frame))
            {
                return frame;
            }

            frame?.Dispose();
            topLevel.InvalidateVisual();
        }

        throw new AvaloniaCaptureStateUnsupportedException(unsupportedMessage);
    }

    private static bool HasRenderedContent(WriteableBitmap bitmap)
    {
        using ILockedFramebuffer framebuffer = bitmap.Lock();
        int bytesPerPixel = framebuffer.Format.BitsPerPixel / 8;
        int firstPixel = Marshal.ReadInt32(framebuffer.Address);
        int stepX = Math.Max(1, framebuffer.Size.Width / 64);
        int stepY = Math.Max(1, framebuffer.Size.Height / 64);
        for (int y = 0; y < framebuffer.Size.Height; y += stepY)
        {
            for (int x = 0; x < framebuffer.Size.Width; x += stepX)
            {
                IntPtr address = IntPtr.Add(framebuffer.Address, (y * framebuffer.RowBytes) + (x * bytesPerPixel));
                if (Marshal.ReadInt32(address) != firstPixel)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string GetCapturePlanPath()
    {
        string? configuredPath = Environment.GetEnvironmentVariable(CapturePlanEnvironmentVariable);
        return string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(
                FindRepositoryRoot(Path.Combine(AppContext.BaseDirectory, "GitUI.Avalonia.Tests.dll")),
                "eng",
                "avalonia",
                "p0.2-capture-plan.json")
            : Path.GetFullPath(configuredPath);
    }

    private static CaptureSettingsProfile LoadCaptureSettingsProfile(CapturePlan plan)
    {
        string path = Path.Combine(AppContext.BaseDirectory, plan.SettingsProfile);
        CaptureSettingsProfile? profile = JsonSerializer.Deserialize<CaptureSettingsProfile>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return profile ?? throw new InvalidDataException("The capture settings profile is empty.");
    }

    private static void ApplyCaptureProfile(CaptureSettingsProfile profile)
    {
        AppSettings.Font = new WinFormsFont(profile.UiFontFamily, profile.UiFontSizePoints);
        AppSettings.FixedWidthFont = new WinFormsFont(profile.FixedFontFamily, profile.FixedFontSizePoints);
        AppSettings.CommitFont = new WinFormsFont(profile.UiFontFamily, profile.UiFontSizePoints);
        AppSettings.MonospaceFont = new WinFormsFont(profile.FixedFontFamily, profile.FixedFontSizePoints);
        AppSettings.Dictionary = "en-US";
        AppSettings.MarkIllFormedLinesInCommitMsg = true;
        AppSettings.TelemetryEnabled = false;
        AppSettings.CheckForUpdates = false;
        AppSettings.ShowAvailableDiffTools = false;
        foreach ((string key, string value) in profile.AppSettings)
        {
            AppSettings.SetString(key, value);
        }

        AvaloniaFontSettings.ApplyAppSettings();
    }

    private static void ApplyCaptureTheme(CaptureThemePlan theme)
    {
        AppSettings.ThemeId = new ThemeId(theme.Id, theme.IsBuiltin);
        AppSettings.ThemeVariations = [];
        AppSettings.UseSystemVisualStyle = theme.Id.Equals("light", StringComparison.OrdinalIgnoreCase);
        AvaloniaThemeSettings.ApplyAppSettings();
    }

    private static string GetThemeSourceSha256(CaptureThemePlan theme)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Themes", theme.File);
        IsPathContained(path, AppContext.BaseDirectory).Should().BeTrue(
            "capture themes must come from the isolated test runtime, not the user themes directory");
        return Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    }

    private static bool IsPathContained(string path, string root)
    {
        string fullPath = Path.GetFullPath(path);
        string fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return fullPath.StartsWith(fullRoot, comparison);
    }

    private static string Sanitize(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(character => invalid.Contains(character) || character is '.' ? '_' : character).ToArray());
    }

    private sealed record CaptureSettingsProfile
    {
        public required string UiFontFamily { get; init; }

        public required float UiFontSizePoints { get; init; }

        public required string FixedFontFamily { get; init; }

        public required float FixedFontSizePoints { get; init; }

        public required IReadOnlyDictionary<string, string> AppSettings { get; init; }
    }

    private sealed record CaptureSettingsSnapshot(
        WinFormsFont Font,
        WinFormsFont FixedWidthFont,
        WinFormsFont CommitFont,
        WinFormsFont MonospaceFont,
        ThemeId ThemeId,
        string[] ThemeVariations,
        bool UseSystemVisualStyle,
        string Dictionary,
        bool MarkIllFormedLines,
        bool? TelemetryEnabled,
        bool CheckForUpdates,
        bool ShowAvailableDiffTools,
        IReadOnlyDictionary<string, string?> ProfileSettings)
    {
        public static CaptureSettingsSnapshot Take(CaptureSettingsProfile profile)
        {
            return new CaptureSettingsSnapshot(
                AppSettings.Font,
                AppSettings.FixedWidthFont,
                AppSettings.CommitFont,
                AppSettings.MonospaceFont,
                AppSettings.ThemeId,
                AppSettings.ThemeVariations,
                AppSettings.UseSystemVisualStyle,
                AppSettings.Dictionary,
                AppSettings.MarkIllFormedLinesInCommitMsg,
                AppSettings.TelemetryEnabled,
                AppSettings.CheckForUpdates,
                AppSettings.ShowAvailableDiffTools,
                profile.AppSettings.Keys.ToDictionary(
                    key => key,
                    key => AppSettings.GetString(key, defaultValue: null),
                    StringComparer.Ordinal));
        }

        public void Restore()
        {
            AppSettings.Font = Font;
            AppSettings.FixedWidthFont = FixedWidthFont;
            AppSettings.CommitFont = CommitFont;
            AppSettings.MonospaceFont = MonospaceFont;
            AppSettings.ThemeId = ThemeId;
            AppSettings.ThemeVariations = ThemeVariations;
            AppSettings.UseSystemVisualStyle = UseSystemVisualStyle;
            AppSettings.Dictionary = Dictionary;
            AppSettings.MarkIllFormedLinesInCommitMsg = MarkIllFormedLines;
            AppSettings.TelemetryEnabled = TelemetryEnabled;
            AppSettings.CheckForUpdates = CheckForUpdates;
            AppSettings.ShowAvailableDiffTools = ShowAvailableDiffTools;
            foreach ((string key, string? value) in ProfileSettings)
            {
                AppSettings.SetString(key, value ?? string.Empty);
            }

            AvaloniaFontSettings.ApplyAppSettings();
            AvaloniaThemeSettings.ApplyAppSettings();
        }
    }

    private sealed record SettingsFileSnapshot(bool Exists, byte[] Content)
    {
        public static SettingsFileSnapshot Take(string path) =>
            File.Exists(path)
                ? new SettingsFileSnapshot(Exists: true, File.ReadAllBytes(path))
                : new SettingsFileSnapshot(Exists: false, []);

        public void AssertUnchanged(string path)
        {
            File.Exists(path).Should().Be(Exists, "capture must not create or remove the real settings file");
            if (Exists)
            {
                File.ReadAllBytes(path).Should().Equal(Content, "capture must not modify the real settings file");
            }
        }
    }
}
