using System.Runtime.CompilerServices;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtensions.ParityCapture;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Compat;
using GitUI.HelperDialogs;
using WinFormsInputParityToAvalonia;
using WinFormsKeys = GitExtensions.Shims.WinForms.Keys;

namespace GitExtensionsTests;

[TestFixture]
[Category("P1_5")]
public sealed class InputAccessibilityTests
{
    [Test]
    public void Generated_input_metadata_should_match_the_WinForms_Designers()
    {
        string repositoryRoot = FindRepositoryRoot();
        string generatedPath = Path.Combine(
            repositoryRoot,
            "src",
            "app",
            "GitUI.Avalonia",
            "Compat",
            "WinFormsInputMetadata.g.cs");

        File.ReadAllText(generatedPath).ReplaceLineEndings("\n").Should().Be(InputMetadataGenerator.Generate(
            Path.Combine(repositoryRoot, "src", "app", "GitUI"),
            Path.Combine(repositoryRoot, "src", "app", "GitUI.Avalonia")));
        WinFormsInputMetadata.ByType.Should().HaveCount(103);
        WinFormsInputMetadata.ByType.Values.Sum(controls => controls.Count).Should().Be(1161);
    }

    [AvaloniaTest]
    public void Original_tab_order_and_TabStop_should_be_applied_to_same_named_controls()
    {
        FormClone clone = new();
        FormCommit commit = new();

        KeyboardNavigation.GetTabIndex(clone.FindControl<ComboBox>("_NO_TRANSLATE_From")!).Should().Be(1);
        KeyboardNavigation.GetTabIndex(clone.FindControl<Button>("FromBrowse")!).Should().Be(2);
        KeyboardNavigation.GetTabIndex(clone.FindControl<ComboBox>("_NO_TRANSLATE_To")!).Should().Be(4);
        KeyboardNavigation.GetTabIndex(commit.FindControl<Button>("Commit")!).Should().Be(101);
        KeyboardNavigation.GetTabIndex(commit.FindControl<CheckBox>("Amend")!).Should().Be(104);
        KeyboardNavigation.GetIsTabStop(commit.FindControl<GitUI.Editor.FileViewer>("SelectedDiff")!).Should().BeFalse();

        clone.Close();
        commit.Close();
    }

    [AvaloniaTest]
    public void Original_accessible_names_and_generated_automation_identifiers_should_be_applied()
    {
        GitConfigSettingsPage settings = new();
        Button browse = settings.FindControl<Button>("btnCommitTemplateBrowse")!;

        AutomationProperties.GetName(browse).Should().Be("Browse Path to commit template");
        AutomationProperties.GetAutomationId(browse).Should().Be("btnCommitTemplateBrowse");

        FormClone clone = new();
        Control[] actionable = clone.GetLogicalDescendants()
            .OfType<Control>()
            .Where(InputAccessibility.IsActionable)
            .ToArray();
        actionable.Should().NotBeEmpty();
        actionable.Should().OnlyContain(control => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(control)));

        ((IDisposable)settings).Dispose();
        clone.Close();
    }

    [AvaloniaTest]
    public void Native_labels_should_route_mnemonics_to_the_original_input_targets()
    {
        FormClone form = new();
        Label repositoryLabel = form.FindControl<Label>("repositoryLabel")!;
        ComboBox source = form.FindControl<ComboBox>("_NO_TRANSLATE_From")!;

        repositoryLabel.Content.Should().Be("Repository to _clone:");
        repositoryLabel.Target.Should().BeSameAs(source);
        form.Close();

        ComboBox target = new() { ItemsSource = new[] { "one", "two" } };
        Label label = new() { Content = "_Choice:", Target = target };
        Button other = new() { Content = "Other" };
        StackPanel content = new();
        content.Children.Add(label);
        content.Children.Add(target);
        content.Children.Add(other);
        Window window = new() { Width = 240, Height = 140, Content = content };
        window.Show();
        try
        {
            other.Focus(NavigationMethod.Tab).Should().BeTrue();

            window.KeyPress(Key.C, RawInputModifiers.Alt, PhysicalKey.C, "c");
            Dispatcher.UIThread.RunJobs();

            target.IsKeyboardFocusWithin.Should().BeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Multiline_radio_button_mnemonics_should_remain_native_AccessText()
    {
        FormResetCurrentBranch form = new();
        RadioButton keep = form.FindControl<RadioButton>("Keep")!;
        RadioButton merge = form.FindControl<RadioButton>("Merge")!;
        RadioButton hard = form.FindControl<RadioButton>("Hard")!;

        keep.Content.Should().BeOfType<AccessText>().Which.Text.Should().StartWith("_Keep:");
        merge.Content.Should().BeOfType<AccessText>().Which.Text.Should().StartWith("_Merge:");
        hard.Content.Should().BeOfType<AccessText>().Which.Text.Should().StartWith("_Hard:");

        form.Close();
    }

    [AvaloniaTest]
    [TestCase(Key.Apps, RawInputModifiers.None)]
    [TestCase(Key.F10, RawInputModifiers.Shift)]
    public void Context_menu_keys_should_open_the_focused_controls_menu(Key key, RawInputModifiers modifiers)
    {
        ListBox list = new() { Name = "lstItems", ItemsSource = new[] { "one", "two" }, SelectedIndex = 0 };
        ContextMenu menu = new() { ItemsSource = new[] { new MenuItem { Header = "Action" } } };
        list.ContextMenu = menu;
        Window window = new() { Width = 240, Height = 120, Content = list };
        InputAccessibility.Apply(window);
        window.Show();
        try
        {
            ListBoxItem item = (ListBoxItem)list.ContainerFromIndex(0)!;
            item.Focus(NavigationMethod.Tab).Should().BeTrue();

            window.KeyPress(key, modifiers, key == Key.Apps ? PhysicalKey.ContextMenu : PhysicalKey.F10, keySymbol: null);
            Dispatcher.UIThread.RunJobs();

            menu.IsOpen.Should().BeTrue();
        }
        finally
        {
            menu.Close();
            window.Close();
        }
    }

    [AvaloniaTest]
    public void List_and_tree_arrow_keys_should_move_the_selection()
    {
        ListBox list = new() { ItemsSource = new[] { "one", "two", "three" }, SelectedIndex = 0 };
        TreeViewItem[] treeItems =
        [
            new TreeViewItem { Header = "one" },
            new TreeViewItem { Header = "two" },
            new TreeViewItem { Header = "three" },
        ];
        TreeView tree = new()
        {
            ItemsSource = treeItems,
            SelectedItem = treeItems[0],
        };
        Grid content = new() { RowDefinitions = new RowDefinitions("*,*") };
        content.Children.Add(list);
        Grid.SetRow(tree, 1);
        content.Children.Add(tree);
        Window window = new() { Width = 300, Height = 240, Content = content };
        window.Show();
        try
        {
            ListBoxItem listItem = (ListBoxItem)list.ContainerFromIndex(0)!;
            listItem.Focus(NavigationMethod.Tab).Should().BeTrue();
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, keySymbol: null);
            list.SelectedIndex.Should().Be(1);

            TreeViewItem treeItem = (TreeViewItem)tree.ContainerFromItem(treeItems[0])!;
            treeItem.Focus(NavigationMethod.Tab).Should().BeTrue();
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, keySymbol: null);
            tree.SelectedItem.Should().BeSameAs(treeItems[1]);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Keyboard_focus_should_use_the_WinForms_dotted_focus_adorner()
    {
        Button button = new() { Width = 100, Height = 30, Content = "Action" };
        Window window = new() { Width = 160, Height = 80, Content = button };
        window.Show();
        try
        {
            button.Focus(NavigationMethod.Tab).Should().BeTrue();
            Dispatcher.UIThread.RunJobs();

            WinFormsFocusAdorner adorner = window.GetVisualDescendants()
                .OfType<WinFormsFocusAdorner>()
                .Single();
            adorner.Bounds.Width.Should().Be(button.Bounds.Width);
            adorner.Bounds.Height.Should().Be(button.Bounds.Height);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Capture_focus_should_delegate_from_a_semantic_container_to_its_focusable_item()
    {
        TabControl tabs = new()
        {
            ItemsSource = new[]
            {
                new TabItem { Header = "First", Content = "First content" },
                new TabItem { Header = "Second", Content = "Second content" },
            },
        };
        Window window = new() { Width = 260, Height = 140, Content = tabs };
        window.Show();
        try
        {
            CaptureStatePlan state = new() { Id = "focused", Kind = CaptureStateKind.Focus };

            using AvaloniaControlStateDriver driver = AvaloniaControlStateDriver.Apply(tabs, state);

            tabs.IsKeyboardFocusWithin.Should().BeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Test]
    public void Persisted_Control_hotkeys_should_project_to_macOS_Meta()
    {
        KeyGesture gesture = KeysMapper.ToKeyGesture(WinFormsKeys.Control | WinFormsKeys.K, useMetaForControl: true)!;

        gesture.Key.Should().Be(Key.K);
        gesture.KeyModifiers.Should().Be(KeyModifiers.Meta);
    }

    private static string FindRepositoryRoot([CallerFilePath] string startPath = "")
    {
        DirectoryInfo? directory = new(Path.GetDirectoryName(startPath)!);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.Avalonia.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"Could not find the repository root from {startPath}.");
    }
}
