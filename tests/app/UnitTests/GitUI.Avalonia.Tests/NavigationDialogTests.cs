using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using DrawingColor = System.Drawing.Color;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class NavigationDialogTests
{
    private RecordingOsShell _shell = null!;
    private RecordingMessageBoxHost _messageBoxes = null!;
    private StubClipboard _clipboard = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _clipboard = new StubClipboard();
        WinFormsShims.ShimHost.Clipboard = _clipboard;
        _shell = new RecordingOsShell();
        WinFormsShims.ShimHost.OsShell = _shell;
        _messageBoxes = new RecordingMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxes;
    }

    [AvaloniaTest]
    public void Navigation_views_should_construct_through_their_designer_boundaries()
    {
        FormGoToCommit goToCommit = new();
        FormCheckoutRevision checkoutRevision = new();
        SearchControl searchControl = new();
        SearchWindow searchWindow = new();
        SearchWindow<string> genericSearchWindow = new(_ => []);

        goToCommit.FindControl<TextBox>("textboxCommitExpression").Should().NotBeNull();
        goToCommit.FindControl<ComboBox>("comboBoxTags").Should().NotBeNull();
        checkoutRevision.FindControl<GitUI.UserControls.CommitPickerSmallControl>("commitPickerSmallControl1").Should().NotBeNull();
        checkoutRevision.FindControl<CheckBox>("Force").Should().NotBeNull();
        searchControl.FindControl<TextBox>("txtSearchBox").Should().NotBeNull();
        searchControl.FindControl<ListBox>("listBoxSearchResult").Should().NotBeNull();
        searchWindow.FindControl<Label>("lblEnterFileName")!.Content.Should().Be("Enter File Name");
        genericSearchWindow.GetLogicalDescendants().OfType<SearchControl<string>>().Should().ContainSingle();
    }

    [AvaloniaTest]
    public void Navigation_dialogs_should_render_their_complete_minimum_layouts()
    {
        (IGitUICommands commands, _) = CreateCommands();
        Window[] forms =
        [
            new FormGoToCommit(commands) { Width = 620, Height = 340 },
            new FormCheckoutRevision(commands) { Width = 460, Height = 125 },
        ];

        foreach (Window form in forms)
        {
            form.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                form.CaptureRenderedFrame().Should().NotBeNull();
                form.GetVisualDescendants().OfType<Button>().Should().Contain(button => button.Bounds.Width > 0);
            }
            finally
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public void FormGoToCommit_should_match_the_reference_client_geometry_and_translation_text()
    {
        (IGitUICommands commands, _) = CreateCommands();
        FormGoToCommit form = new(commands);
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            AssertBounds(form.FindControl<TextBox>("textboxCommitExpression")!, 155, 13, 360, 23);
            AssertBounds(form.FindControl<Button>("goButton")!, 521, 10, 75, 28);
            AssertBounds(form.FindControl<GroupBox>("groupBox1")!, 45, 43, 470, 141);
            AssertBounds(form.FindControl<TextBlock>("label1")!, 12, 17, 112, 15);
            AssertBounds(form.FindControl<TextBlock>("label2")!, 64, 65, 413, 75);
            AssertBounds(form.FindControl<HyperlinkButton>("linkGitRevParse")!, 64, 155, 126, 15);
            AssertBounds(form.FindControl<TextBlock>("label3")!, 12, 219, 59, 15);
            AssertBounds(form.FindControl<ComboBox>("comboBoxTags")!, 155, 216, 287, 23);
            AssertBounds(form.FindControl<TextBlock>("label4")!, 12, 261, 79, 15);
            AssertBounds(form.FindControl<ComboBox>("comboBoxBranches")!, 155, 258, 287, 23);
            form.FindControl<TextBlock>("label2")!.Text.Should().Be(
                "Commit expression examples:\r\n- complete commit hash: e. g.: 8eab51fcb9c4538eb74c4dcd4c31ffd693ad25c9\r\n- partial commit hash (if unique): e. g.: 8eab51fcb9c453\r\n- tag name\r\n- branch name");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormGoToCommit_rev_parse_link_should_be_keyboard_focusable_and_open_the_original_uri()
    {
        (IGitUICommands commands, _) = CreateCommands();
        FormGoToCommit form = new(commands);
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            HyperlinkButton link = form.FindControl<HyperlinkButton>("linkGitRevParse")!;
            link.Focus();

            link.IsKeyboardFocusWithin.Should().BeTrue();
            link.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            _shell.Target.Should().Be("https://git-scm.com/docs/git-rev-parse#_specifying_revisions");
            _shell.Kind.Should().Be(WinFormsShims.OsShellLaunchKind.OpenUri);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task FormGoToCommit_should_load_and_resolve_the_original_tag_and_branch_identities()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        const string tagObjectId = "1111111111111111111111111111111111111111";
        const string branchObjectId = "2222222222222222222222222222222222222222";
        IGitRef tag = CreateRef("v1.0", tagObjectId);
        IGitRef branch = CreateRef("feature", branchObjectId);
        module.GetRefs(RefsFilter.Tags).Returns([tag]);
        module.GetRefs(RefsFilter.Heads).Returns([branch]);
        ObjectId tagId = ObjectId.Parse(tagObjectId);
        ObjectId branchId = ObjectId.Parse(branchObjectId);
        module.RevParse(tagObjectId).Returns(tagId);
        module.RevParse(branchObjectId).Returns(branchId);
        FormGoToCommit form = new(commands);
        form.Show();
        try
        {
            ComboBox tags = form.FindControl<ComboBox>("comboBoxTags")!;
            ComboBox branches = form.FindControl<ComboBox>("comboBoxBranches")!;
            await WaitUntilAsync(() => tags.ItemCount == 1 && branches.ItemCount == 1);

            tags.Focus();
            tags.Text = tag.LocalName;
            Dispatcher.UIThread.RunJobs();
            form.ValidateAndGetSelectedObjectId().Should().Be(tagId);

            branches.Focus();
            branches.Text = branch.LocalName;
            Dispatcher.UIThread.RunJobs();
            form.ValidateAndGetSelectedObjectId().Should().Be(branchId);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormGoToCommit_should_restore_a_valid_clipboard_revision_and_select_it()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        const string revision = "3333333333333333333333333333333333333333";
        _clipboard.Text = revision;
        module.RevParse(revision).Returns(ObjectId.Parse(revision));
        FormGoToCommit form = new(commands);
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            TextBox expression = form.FindControl<TextBox>("textboxCommitExpression")!;

            expression.Text.Should().Be(revision);
            expression.SelectionStart.Should().Be(0);
            expression.SelectionEnd.Should().Be(revision.Length);
            form.ValidateAndGetSelectedObjectId().Should().Be(ObjectId.Parse(revision));
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormGoToCommit_should_preserve_the_original_right_anchored_resize_behavior()
    {
        (IGitUICommands commands, _) = CreateCommands();
        FormGoToCommit form = new(commands);
        form.Show();
        try
        {
            form.Width = 704;
            Dispatcher.UIThread.RunJobs();

            AssertBounds(form.FindControl<TextBox>("textboxCommitExpression")!, 155, 13, 460, 23);
            AssertBounds(form.FindControl<Button>("goButton")!, 621, 10, 75, 28);
            AssertBounds(form.FindControl<GroupBox>("groupBox1")!, 45, 43, 570, 141);
            AssertBounds(form.FindControl<ComboBox>("comboBoxTags")!, 155, 216, 387, 23);
            AssertBounds(form.FindControl<ComboBox>("comboBoxBranches")!, 155, 258, 387, 23);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormCheckoutRevision_should_match_the_reference_client_geometry_and_text()
    {
        (IGitUICommands commands, _) = CreateCommands();
        FormCheckoutRevision form = new(commands);
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            AssertBounds(form.FindControl<Grid>("tableLayoutPanel1")!, 12, 12, 457, 66);
            AssertBounds(form.FindControl<Button>("OkCheckout")!, 384, 98, 84, 25);
            GitUI.UserControls.CommitPickerSmallControl commitPicker = form.FindControl<GitUI.UserControls.CommitPickerSmallControl>("commitPickerSmallControl1")!;
            AssertBounds(commitPicker, 145, 15, 321, 26);
            AssertBounds(commitPicker.FindControl<TextBox>("textBoxCommitHash")!, 145, 17, 284, 23);
            AssertBounds(commitPicker.FindControl<Button>("buttonPickCommit")!, 432, 15, 25, 24);
            AssertBounds(form.FindControl<CheckBox>("Force")!, 145, 47, 166, 19);
            form.FindControl<Label>("label2")!.Content.Should().Be("Checkout this _revision");
            form.FindControl<CheckBox>("Force")!.Content.Should().Be("_Force (reset local changes)");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void SearchControl_should_limit_candidates_select_the_first_and_report_its_size()
    {
        Avalonia.Size reportedSize = default;
        SearchControl<string> search = new(_ => [], size => reportedSize = size);
        IEnumerable<string> candidates = Enumerable.Range(1, 25).Select(index => $"src/File{index:00}.cs");

        Invoke(search, "SearchForCandidates", candidates);

        ListBox results = search.FindControl<ListBox>("listBoxSearchResult")!;
        results.ItemCount.Should().Be(20);
        results.SelectedIndex.Should().Be(0);
        results.IsVisible.Should().BeTrue();
        reportedSize.Width.Should().BeGreaterThanOrEqualTo(300);
    }

    [AvaloniaTest]
    public void SearchWindow_should_match_the_reference_one_result_geometry()
    {
        SearchWindow<string> window = new(_ => []);
        window.Show();
        try
        {
            SearchControl<string> search = window.GetLogicalDescendants().OfType<SearchControl<string>>().Single();
            Invoke(search, "SearchForCandidates", (object)new[] { "src/App.cs" });
            Dispatcher.UIThread.RunJobs();

            window.Bounds.Width.Should().BeApproximately(300, 1);
            window.Bounds.Height.Should().BeApproximately(79, 1);
            AssertBounds(window.FindControl<Label>("lblEnterFileName")!, 0, 0, 300, 24);
            AssertBounds(search.FindControl<ListBox>("listBoxSearchResult")!, 0, 47, 300, 32);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void SearchControl_should_convert_the_reference_popup_width_from_physical_pixels()
    {
        Avalonia.Size reportedSize = default;
        SearchControl<string> search = new(_ => [], size => reportedSize = size);
        Window host = new() { Width = 325, Height = 91, Content = search };
        host.Show();
        try
        {
            host.SetRenderScaling(1.25);
            Dispatcher.UIThread.RunJobs();
            Invoke(search, "SearchForCandidates", (object)new[] { "src/App.cs" });
            Dispatcher.UIThread.RunJobs();

            reportedSize.Width.Should().BeApproximately(240, 0.01);
            TextBox searchBox = search.FindControl<TextBox>("txtSearchBox")!;
            Avalonia.Media.Typeface typeface = new(
                searchBox.FontFamily,
                searchBox.FontStyle,
                searchBox.FontWeight,
                searchBox.FontStretch);
            Avalonia.Media.FontManager.Current.TryGetGlyphTypeface(typeface, out Avalonia.Media.GlyphTypeface? glyphTypeface).Should().BeTrue();
            Avalonia.Media.FontMetrics metrics = glyphTypeface!.Metrics;
            double lineHeight = metrics.LineSpacing * searchBox.FontSize / metrics.DesignEmHeight;
            double itemHeight = Math.Max(16, Math.Ceiling(lineHeight * 1.25) / 1.25);
            reportedSize.Height.Should().BeApproximately((itemHeight * 2) + Math.Max(22, searchBox.Bounds.Height), 0.01);
            search.FindControl<ListBox>("listBoxSearchResult")!.Classes.Should().Contain("search-results");
        }
        finally
        {
            host.Close();
        }
    }

    [AvaloniaTest]
    public void SearchControl_should_commit_the_selected_item_and_raise_the_original_event()
    {
        SearchControl<string> search = new(_ => [], _ => { });
        Invoke(search, "SearchForCandidates", (object)new[] { "src/App.cs", "src/Program.cs" });
        bool entered = false;
        search.OnTextEntered += () => entered = true;

        Invoke(search, "ItemSelectedFromList");

        search.Text.Should().Be("src/App.cs");
        entered.Should().BeTrue();
        search.FindControl<ListBox>("listBoxSearchResult")!.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void SearchControl_should_raise_TextChanged_for_user_and_selected_item_text()
    {
        SearchControl<string> search = new(_ => [], _ => { });
        int changeCount = 0;
        search.TextChanged += (_, _) => changeCount++;
        Window host = new() { Content = search };
        host.Show();

        try
        {
            search.Text = "src";
            Dispatcher.UIThread.RunJobs();
            TextBox searchBox = search.FindControl<TextBox>("txtSearchBox")!;
            searchBox.IsKeyboardFocusWithin.Should().BeTrue();
            Invoke(search, "SearchForCandidates", (object)new[] { "src/App.cs" });
            Invoke(search, "ItemSelectedFromList");
            Dispatcher.UIThread.RunJobs();

            changeCount.Should().Be(2);
        }
        finally
        {
            host.Close();
        }
    }

    [AvaloniaTest]
    public void SearchControl_should_preserve_wraparound_enter_and_escape_keyboard_routes()
    {
        SearchControl<string> search = new(_ => [], _ => { });
        Invoke(search, "SearchForCandidates", (object)new[] { "first", "second", "third" });
        TextBox searchBox = search.FindControl<TextBox>("txtSearchBox")!;
        ListBox results = search.FindControl<ListBox>("listBoxSearchResult")!;

        RaiseKey(searchBox, InputElement.KeyDownEvent, Key.Up);
        results.SelectedIndex.Should().Be(2);
        RaiseKey(searchBox, InputElement.KeyDownEvent, Key.Down);
        results.SelectedIndex.Should().Be(0);

        bool entered = false;
        search.OnTextEntered += () => entered = true;
        RaiseKey(searchBox, InputElement.KeyUpEvent, Key.Enter);
        entered.Should().BeTrue();
        search.Text.Should().Be("first");
        results.IsVisible.Should().BeFalse();

        Invoke(search, "SearchForCandidates", (object)new[] { "first", "second" });
        bool cancelled = false;
        search.OnCancelled += () => cancelled = true;
        RaiseKey(searchBox, InputElement.KeyUpEvent, Key.Escape);
        cancelled.Should().BeTrue();
        results.SelectedItem.Should().BeNull();
        results.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void SearchControl_should_close_results_only_after_focus_leaves_the_search_composite()
    {
        SearchControl<string> search = new(_ => [], _ => { });
        Button outside = new() { Content = "Outside" };
        StackPanel layout = new();
        layout.Children.Add(search);
        layout.Children.Add(outside);
        Window host = new() { Content = layout };
        host.Show();
        try
        {
            Invoke(search, "SearchForCandidates", (object)new[] { "first", "second" });
            TextBox searchBox = search.FindControl<TextBox>("txtSearchBox")!;
            ListBox results = search.FindControl<ListBox>("listBoxSearchResult")!;

            searchBox.Focus();
            results.Focus();
            Dispatcher.UIThread.RunJobs();
            results.IsVisible.Should().BeTrue();

            outside.Focus();
            Dispatcher.UIThread.RunJobs();
            results.IsVisible.Should().BeFalse();
        }
        finally
        {
            host.Close();
        }
    }

    [AvaloniaTest]
    public void SearchControl_should_apply_the_original_default_and_focused_border_colors()
    {
        SearchControl<string> search = new(_ => [], _ => { })
        {
            SearchBoxBorderDefaultColor = DrawingColor.Red,
            SearchBoxBorderFocusedColor = DrawingColor.Blue,
            SearchBoxBorderStyle = WinFormsShims.BorderStyle.FixedSingle,
        };
        Button outside = new() { Content = "Outside" };
        StackPanel layout = new();
        layout.Children.Add(search);
        layout.Children.Add(outside);
        Window host = new() { Content = layout };
        host.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            TextBox searchBox = search.FindControl<TextBox>("txtSearchBox")!;
            Border border = (Border)searchBox.Parent!;

            searchBox.Focus();
            Dispatcher.UIThread.RunJobs();
            border.BorderBrush.Should().BeOfType<Avalonia.Media.SolidColorBrush>()
                .Which.Color.Should().Be(Avalonia.Media.Colors.Blue);
            border.BorderThickness.Should().Be(new Thickness(1));

            outside.Focus();
            Dispatcher.UIThread.RunJobs();
            border.BorderBrush.Should().BeOfType<Avalonia.Media.SolidColorBrush>()
                .Which.Color.Should().Be(Avalonia.Media.Colors.Red);
        }
        finally
        {
            host.Close();
        }
    }

    [AvaloniaTest]
    public void FormCheckoutRevision_should_reject_an_empty_revision_with_the_original_error()
    {
        (IGitUICommands commands, _) = CreateCommands();
        FormCheckoutRevision form = new(commands);
        form.Show();
        try
        {
            Button checkout = form.FindControl<Button>("OkCheckout")!;
            checkout.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            _messageBoxes.Messages.Should().ContainSingle()
                .Which.Should().Be("Select 1 revision to checkout.");
            _messageBoxes.Captions.Should().ContainSingle()
                .Which.Should().Be("Checkout");
            form.IsVisible.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void SearchControl_should_focus_without_selecting_existing_text()
    {
        SearchControl<string> search = new(_ => [], _ => { }) { Text = "src/App.cs" };
        Window host = new() { Content = search };
        host.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            TextBox searchBox = search.FindControl<TextBox>("txtSearchBox")!;

            searchBox.IsKeyboardFocusWithin.Should().BeTrue();
            searchBox.SelectionEnd.Should().Be(searchBox.SelectionStart);
        }
        finally
        {
            host.Close();
        }
    }

    [TestCase("app.cs", "src/App.cs", true)]
    [TestCase("APP.CS", "src/App.cs", true)]
    [TestCase("missing", "src/App.cs", false)]
    public void FindFilePredicate_should_match_portable_paths_case_insensitively(string pattern, string candidate, bool expected)
    {
        FindFilePredicateProvider provider = new();

        provider.Get(pattern, "/work/repository")(candidate).Should().Be(expected);
    }

    [Test]
    public void FindFilePredicate_should_strip_an_absolute_working_directory_prefix()
    {
        FindFilePredicateProvider provider = new();

        provider.Get("/work/repository/src", "/work/repository")("src/App.cs").Should().BeTrue();
        provider.Get("/work/repository/src", "/work/repository")("tests/AppTests.cs").Should().BeFalse();
    }

    private static void Invoke(object target, string methodName, params object[] arguments)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"{target.GetType().Name}.{methodName} was not found.");
        method.Invoke(target, arguments);
    }

    private static void RaiseKey(InputElement target, RoutedEvent<KeyEventArgs> routedEvent, Key key)
    {
        target.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = routedEvent,
            Key = key,
        });
    }

    private static void AssertBounds(Control control, double x, double y, double width, double height)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(control)
            ?? throw new InvalidOperationException($"{control.Name} is not attached to its window.");
        Point origin = control.TranslatePoint(default, topLevel)
            ?? throw new InvalidOperationException($"{control.Name} is not attached to its window.");
        origin.X.Should().BeApproximately(x, 1, control.Name);
        origin.Y.Should().BeApproximately(y, 1, control.Name);
        control.Bounds.Width.Should().BeApproximately(width, 1, control.Name);
        control.Bounds.Height.Should().BeApproximately(height, 1, control.Name);
    }

    private static (IGitUICommands Commands, IGitModule Module) CreateCommands()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.GetRefs(Arg.Any<RefsFilter>()).Returns([]);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return (commands, module);
    }

    private static IGitRef CreateRef(string localName, string objectId)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.LocalName.Returns(localName);
        gitRef.Guid.Returns(objectId);
        return gitRef;
    }

    private static async Task WaitUntilAsync(Func<bool> predicate)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!predicate() && stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        predicate().Should().BeTrue("the asynchronous navigation operation should complete within five seconds");
    }

    private sealed class StubClipboard : WinFormsShims.IClipboard
    {
        public string Text { get; set; } = string.Empty;

        public bool ContainsText() => !string.IsNullOrEmpty(Text);

        public string GetText() => Text;

        public void SetText(string value)
        {
            Text = value;
        }
    }

    private sealed class RecordingOsShell : WinFormsShims.IOsShell
    {
        public string? Target { get; private set; }

        public WinFormsShims.OsShellLaunchKind? Kind { get; private set; }

        public bool TryLaunch(string target, WinFormsShims.OsShellLaunchKind kind)
        {
            Target = target;
            Kind = kind;
            return true;
        }
    }

    private sealed class RecordingMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<string> Messages { get; } = [];

        public List<string> Captions { get; } = [];

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            Captions.Add(caption ?? string.Empty);
            return WinFormsShims.DialogResult.OK;
        }
    }
}
