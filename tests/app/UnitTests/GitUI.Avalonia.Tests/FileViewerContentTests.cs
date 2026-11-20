using System.ComponentModel.Design;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.Blame;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.Editor;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FileViewerContentTests
{
    private GitUICommands _commands = null!;
    private GitModule _module = null!;
    private ServiceContainer _serviceContainer = null!;
    private IGitUICommandsSource _source = null!;
    private string _workingDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        _serviceContainer.AddService<IAppTitleGenerator>(new AppTitleGenerator(repositoryDescriptionProvider));
        _serviceContainer.AddService<ILinkFactory>(new LinkFactory());
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.FileViewer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
        _module = new GitModule(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        _module.SetSetting("user.name", "Avalonia Test");
        _module.SetSetting("user.email", "avalonia@example.com");

        _commands = new GitUICommands(_serviceContainer, _module);
        _source = Substitute.For<IGitUICommandsSource>();
        _source.UICommands.Returns(_commands);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FileViewer_should_resume_post_settings_after_reattachment_and_unsubscribe_on_dispose()
    {
        using AppSettingsTestScope settings = new();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(_module);
        commands.GetService(Arg.Any<Type>()).Returns(call => _commands.GetService(call.Arg<Type>()));
        using FormEditor editor = new(commands, fileName: null, showWarning: false);
        using FileViewer viewer = editor.FindControl<FileViewer>("fileViewer")!;
        Grid layout = editor.FindControl<Grid>("tableLayoutPanel1")!;
        try
        {
            editor.Show();
            PublishRuler(89);
            viewer.VRulerPosition.Should().Be(89);

            layout.Children.Remove(viewer);
            Dispatcher.UIThread.RunJobs();
            PublishRuler(90);
            viewer.VRulerPosition.Should().Be(89);

            layout.Children.Add(viewer);
            Dispatcher.UIThread.RunJobs();
            PublishRuler(103);
            viewer.VRulerPosition.Should().Be(103);

            ((IDisposable)viewer).Dispose();
            PublishRuler(105);
            viewer.VRulerPosition.Should().Be(103);
        }
        finally
        {
            editor.Close();
        }

        void PublishRuler(int position)
        {
            AppSettings.DiffVerticalRulerPosition = position;
            commands.PostSettings += Raise.Event<EventHandler<GitUIPostActionEventArgs>>(commands, new GitUIPostActionEventArgs(null, commands, actionDone: true));
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTest]
    public void FileViewer_should_populate_encoding_choices_and_size_the_dropdown_on_runtime_load()
    {
        using FormEditor editor = new(_commands, fileName: null, showWarning: false);
        using FileViewer viewer = editor.FindControl<FileViewer>("fileViewer")!;
        ComboBox encodings = viewer.FindControl<ComboBox>("encodingToolStripComboBox")!;
        encodings.Items.Should().BeEmpty("the original adds the choices in OnRuntimeLoad");

        try
        {
            editor.Show();
            Dispatcher.UIThread.RunJobs();

            encodings.Items.Should().Contain(Encoding.Unicode.EncodingName);
            encodings.SelectedItem.Should().BeNull();
            encodings.Should().BeOfType<ToolbarComboBox>().Which.DropDownWidth.Should().BeInRange(50, 250);
        }
        finally
        {
            editor.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task FileViewer_should_publish_copy_availability_before_its_context_menu_opens(bool selectText)
    {
        using FormEditor editor = new(_commands, fileName: null, showWarning: false);
        using FileViewer viewer = editor.FindControl<FileViewer>("fileViewer")!;
        ContextMenu menu = viewer.FindControl<ContextMenu>("contextMenu")!;
        MenuItem copy = viewer.FindControl<MenuItem>("copyToolStripMenuItem")!;
        object? openingSender = null;
        bool? copyEnabledOnOpening = null;
        viewer.ContextMenuOpening += (sender, args) =>
        {
            openingSender = sender;
            copyEnabledOnOpening = copy.IsEnabled;
            args.Cancel = true;
        };

        try
        {
            editor.Show();
            await viewer.ViewTextAsync("selection.txt", "some selected text");
            viewer.TextEditor.Select(0, selectText ? 4 : 0);
            Dispatcher.UIThread.RunJobs();
            viewer.TextEditor.ContextMenu.Should().BeSameAs(menu);

            Point point = viewer.TextEditor.TextArea.TextView.TranslatePoint(new Point(20, 8), editor)!.Value;
            editor.MouseMove(point, RawInputModifiers.None);
            editor.MouseDown(point, MouseButton.Right, RawInputModifiers.None);
            editor.MouseUp(point, MouseButton.Right, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            copyEnabledOnOpening.Should().Be(selectText);
            openingSender.Should().BeSameAs(menu);
            menu.IsOpen.Should().BeFalse();
        }
        finally
        {
            editor.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task FileViewer_should_publish_only_the_scroll_axis_that_changes(bool horizontal, bool useWheel)
    {
        using FormEditor editor = new(_commands, fileName: null, showWarning: false);
        using FileViewer viewer = editor.FindControl<FileViewer>("fileViewer")!;
        string text = string.Join('\n', Enumerable.Repeat(new string('w', 300), 100));
        try
        {
            editor.Show();
            await viewer.ViewTextAsync("scroll.txt", text);
            editor.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            viewer.TextEditor.Text.Should().Be(text);
            viewer.HScrollPosition.Should().Be(0);
            viewer.VScrollPosition.Should().Be(0);
            int horizontalChanges = 0;
            int verticalChanges = 0;
            viewer.HScrollPositionChanged += (_, _) => horizontalChanges++;
            viewer.VScrollPositionChanged += (_, _) => verticalChanges++;

            if (useWheel)
            {
                Point point = viewer.TextEditor.TextArea.TextView.TranslatePoint(new Point(20, 8), editor)!.Value;
                editor.MouseWheel(point, horizontal ? new Vector(-1, 0) : new Vector(0, -1), RawInputModifiers.None);
            }
            else if (horizontal)
            {
                viewer.HScrollPosition = 48;
            }
            else
            {
                viewer.VScrollPosition = 48;
            }

            editor.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            int changedOffset = horizontal ? viewer.HScrollPosition : viewer.VScrollPosition;
            if (useWheel)
            {
                changedOffset.Should().BePositive();
            }
            else
            {
                changedOffset.Should().Be(48);
            }

            (horizontal ? viewer.VScrollPosition : viewer.HScrollPosition).Should().Be(0);
            horizontalChanges.Should().Be(horizontal ? 1 : 0);
            verticalChanges.Should().Be(horizontal ? 0 : 1);
        }
        finally
        {
            editor.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(true)]
    [TestCase(false)]
    public async Task FileViewer_should_land_at_the_source_offset_when_scrolling_to_the_previous_file(bool longPatch)
    {
        using AppSettingsTestScope settings = new();
        AppSettings.AutomaticContinuousScroll = false;
        AppSettings.DiffDisplayAppearance.Value = DiffDisplayAppearance.Patch;
        File.WriteAllText(Path.Combine(_workingDirectory, "first.txt"),
            string.Concat(Enumerable.Range(1, 100).Select(line => $"first file line {line}\n")));
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "first.txt" }).Should().BeTrue();
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "first" }).Should().BeTrue();
        ObjectId first = _module.GetCurrentCheckout();

        int addedLines = longPatch ? 100 : 1;
        string lastLine = $"+first file line {100 + addedLines}";
        File.AppendAllText(Path.Combine(_workingDirectory, "first.txt"),
            string.Concat(Enumerable.Range(101, addedLines).Select(line => $"first file line {line}\n")));
        File.WriteAllText(Path.Combine(_workingDirectory, "second.txt"), "second file\n");
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "." }).Should().BeTrue();
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "second" }).Should().BeTrue();

        using FormDiff form = new(_commands, first, _module.GetCurrentCheckout(), "Base", "Compare");
        FileStatusList files = form.FindControl<FileStatusList>("DiffFiles")!;
        FileViewer viewer = form.FindControl<FileViewer>("DiffText")!;
        try
        {
            form.Show();
            await WaitUntilAsync(() => viewer.GetText().Contains(lastLine, StringComparison.Ordinal));
            files.SelectNextVisibleItem();
            await WaitUntilAsync(() => viewer.GetText().Contains("second file", StringComparison.Ordinal));
            form.UpdateLayout();
            viewer.VScrollPosition.Should().Be(0);

            TextView textView = viewer.TextEditor.TextArea.TextView;
            Point point = textView.TranslatePoint(new Point(20, 8), form)!.Value;
            form.MouseWheel(point, new Vector(0, 1), RawInputModifiers.Alt);
            await WaitUntilAsync(() => viewer.GetText().Contains(lastLine, StringComparison.Ordinal));
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            files.SelectedFileStatusItem!.Item.Name.Should().Be("first.txt");
            ScrollViewer scrollViewer = textView.FindAncestorOfType<ScrollViewer>()!;
            if (longPatch)
            {
                double viewport = scrollViewer.Viewport.Height;
                double lineHeight = textView.DefaultLineHeight;
                int extraLines = (1 + (int)(viewport / lineHeight)) * 2 / 3;
                double expectedOffset = textView.DocumentHeight + (extraLines * lineHeight) - viewport - 300;
                expectedOffset.Should().BePositive();
                scrollViewer.Offset.Y.Should().BeApproximately(expectedOffset, 1);
                textView.VisualLines.Should().Contain(line => line.FirstDocumentLine.LineNumber == textView.Document.LineCount - 1);
            }
            else
            {
                scrollViewer.ScrollBarMaximum.Y.Should().Be(0);
                scrollViewer.Offset.Y.Should().Be(0);
            }
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task BlameControl_should_refresh_the_author_tip_under_a_stationary_pointer(bool scrollAuthorPane)
    {
        using AppSettingsTestScope settings = new();
        AppSettings.BlameShowAuthorAvatar = false;
        string path = Path.Combine(_workingDirectory, "blame.txt");
        File.WriteAllText(path, string.Concat(Enumerable.Range(1, 4).Select(line => $"original line {line}\n")));
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "blame.txt" }).Should().BeTrue();
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "original-author-change" }).Should().BeTrue();
        File.AppendAllText(path, string.Concat(Enumerable.Range(5, 96).Select(line => $"new line {line}\n")));
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "blame.txt" }).Should().BeTrue();
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "later-author-change" }).Should().BeTrue();

        using FormBlame form = new(_commands, "blame.txt", _module.GetRevision(), initialLine: 1);
        BlameControl control = form.FindControl<BlameControl>("blameControl1")!;
        FileViewer author = control.BlameAuthor;
        FileViewer file = control.BlameFile;
        try
        {
            form.Show();
            await WaitUntilAsync(() => file.GetText().Contains("new line 100", StringComparison.Ordinal));
            form.UpdateLayout();
            TextView textView = author.TextEditor.TextArea.TextView;
            Point textPoint = new(2, textView.DefaultLineHeight * 3.5);
            Point point = textView.TranslatePoint(textPoint, form)!.Value;
            int authorY = (int)textView.TranslatePoint(textPoint, author)!.Value.Y;
            form.MouseMove(point, RawInputModifiers.None);
            author.IsPointerOver.Should().BeTrue();
            author.GetLineFromVisualPosY(authorY).Should().Be(3);
            ToolTip.GetTip(author).Should().BeOfType<string>().Which.Should().Contain("original-author-change");

            if (scrollAuthorPane)
            {
                form.MouseWheel(point, new Vector(0, -4), RawInputModifiers.None);
            }
            else
            {
                file.VScrollPosition = (int)Math.Ceiling(textView.Bounds.Height + textView.DefaultLineHeight);
            }

            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            author.IsPointerOver.Should().BeTrue();
            author.VScrollPosition.Should().BePositive();
            file.VScrollPosition.Should().Be(author.VScrollPosition);
            author.GetLineFromVisualPosY(authorY).Should().BeInRange(4, 99);
            ToolTip.GetTip(author).Should().BeOfType<string>().Which.Should().Contain("later-author-change");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task FileViewer_should_clear_a_selection_before_raising_EscapePressed()
    {
        using FormEditor editor = new(_commands, fileName: null, showWarning: false);
        using FileViewer viewer = editor.FindControl<FileViewer>("fileViewer")!;
        try
        {
            editor.Show();
            await viewer.ViewTextAsync("selection.txt", "first\nsecond");
            editor.UpdateLayout();
            Point point = viewer.TextEditor.TextArea.TextView.TranslatePoint(new Point(8, 6), editor)!.Value;
            editor.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            editor.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            viewer.TextEditor.TextArea.IsFocused.Should().BeTrue();
            viewer.TextEditor.Select(0, 5);
            viewer.TextEditor.SelectedText.Should().Be("first");
            int escapeCount = 0;
            viewer.EscapePressed += () => escapeCount++;

            editor.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
            editor.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
            escapeCount.Should().Be(0);
            viewer.TextEditor.SelectionLength.Should().Be(0);

            editor.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
            editor.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
            escapeCount.Should().Be(1);
        }
        finally
        {
            editor.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(12, 6)]
    [TestCase(30, 6)]
    [TestCase(200, 1)]
    public void FileViewer_should_navigate_to_the_first_right_side_diff_line_at_or_after_the_requested_line(int requestedLine, int expectedDocumentLine)
    {
        using FormEditor editor = new(_commands, fileName: null, showWarning: false);
        using FileViewer viewer = editor.FindControl<FileViewer>("fileViewer")!;
        try
        {
            editor.Show();
            viewer.ViewPatch("diff --git a/a.txt b/a.txt\n--- a/a.txt\n+++ b/a.txt\n@@ -30,2 +30,2 @@\n-before\n+after\n context\n");

            viewer.GoToLine(requestedLine);

            viewer.TextEditor.TextArea.Caret.Line.Should().Be(expectedDocumentLine);
            viewer.TextEditor.TextArea.Caret.Column.Should().Be(1);
        }
        finally
        {
            editor.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task FileViewer_should_use_F3_to_search_when_a_query_exists(bool hasQuery)
    {
        using AppSettingsTestScope settings = new();
        using FormEditor editor = new(_commands, fileName: null, showWarning: false);
        using FileViewer viewer = editor.FindControl<FileViewer>("fileViewer")!;
        try
        {
            editor.Show();
            int difftoolRequests = 0;
            await viewer.ViewTextAsync("search.txt", "first target last", openWithDifftool: () => difftoolRequests++);

            if (hasQuery)
            {
                viewer.Find(replace: false);
                FindAndReplaceForm find = viewer.GetTestAccessor().FindAndReplaceForm;
                find.FindControl<TextBox>("txtLookFor")!.Text = "target";
                find.FindControl<Button>("btnCancel")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                find.IsVisible.Should().BeFalse();
            }

            editor.UpdateLayout();
            Point point = viewer.TextEditor.TextArea.TextView.TranslatePoint(new Point(5, 8), editor)!.Value;
            editor.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            editor.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            viewer.TextEditor.TextArea.IsFocused.Should().BeTrue();
            editor.KeyPress(Key.F3, RawInputModifiers.None, PhysicalKey.F3, keySymbol: null);
            editor.KeyRelease(Key.F3, RawInputModifiers.None, PhysicalKey.F3, keySymbol: null);
            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
            await ThreadHelper.JoinPendingOperationsAsync(timeout.Token);

            difftoolRequests.Should().Be(hasQuery ? 0 : 1);
            viewer.TextEditor.SelectedText.Should().Be(hasQuery ? "target" : string.Empty);
        }
        finally
        {
            editor.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, false, true)]
    [TestCase(true, true, true)]
    [TestCase(true, false, false)]
    public async Task FileViewer_should_handle_change_hotkeys_only_when_the_action_is_available(bool showPatch, bool backward, bool actionEnabled)
    {
        using AppSettingsTestScope settings = new();
        using FormEditor editor = new(_commands, fileName: null, showWarning: false);
        using FileViewer viewer = editor.FindControl<FileViewer>("fileViewer")!;
        try
        {
            editor.Show();
            if (showPatch)
            {
                viewer.ViewPatch("diff --git a/a.txt b/a.txt\n--- a/a.txt\n+++ b/a.txt\n@@ -1,2 +1,2 @@\n-old\n+new\n context\n@@ -8,2 +8,2 @@\n-before\n+after\n context\n");
            }
            else
            {
                await viewer.ViewTextAsync("file.txt", "first\nsecond");
            }

            editor.UpdateLayout();
            Point point = viewer.TextEditor.TextArea.TextView.TranslatePoint(new Point(5, 8), editor)!.Value;
            editor.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            editor.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            viewer.TextEditor.TextArea.IsFocused.Should().BeTrue();
            Key key = backward ? Key.Up : Key.Down;
            PhysicalKey physicalKey = backward ? PhysicalKey.ArrowUp : PhysicalKey.ArrowDown;
            Button changeButton = viewer.FindControl<Button>(backward ? "previousChangeButton" : "nextChangeButton")!;
            changeButton.IsEnabled = actionEnabled;
            int bubbledKeys = 0;
            editor.KeyDown += (_, e) =>
            {
                if (e.Key == key)
                {
                    bubbledKeys++;
                }
            };
            int initialLine = backward ? 9 : 5;
            if (showPatch)
            {
                viewer.TextEditor.TextArea.Caret.Position = new TextViewPosition(initialLine, 1);
            }

            editor.KeyPress(key, RawInputModifiers.Alt, physicalKey, keySymbol: null);
            editor.KeyRelease(key, RawInputModifiers.Alt, physicalKey, keySymbol: null);

            bool actionAvailable = showPatch && actionEnabled;
            bubbledKeys.Should().Be(actionAvailable ? 0 : 1);
            if (showPatch)
            {
                int finalLine = viewer.TextEditor.TextArea.Caret.Line;
                if (actionAvailable)
                {
                    (backward ? finalLine < initialLine : finalLine > initialLine).Should().BeTrue();
                }
                else
                {
                    finalLine.Should().Be(initialLine);
                }
            }
        }
        finally
        {
            editor.Close();
        }
    }

    [AvaloniaTest]
    public async Task FileViewer_should_publish_the_clicked_blame_line_without_requiring_a_caret_move()
    {
        using FormEditor editor = new(_commands, fileName: null, showWarning: false);
        using FileViewer viewer = editor.FindControl<FileViewer>("fileViewer")!;
        try
        {
            editor.Show();
            await viewer.ViewTextAsync("selection.txt", "first\nsecond\nthird");
            Dispatcher.UIThread.RunJobs();

            List<int> selected = [];
            viewer.SelectedLineChanged += (_, args) => selected.Add(args.SelectedLine);
            viewer.GoToLine(2);
            viewer.TextEditor.TextArea.Caret.Line.Should().Be(2);
            selected.Should().BeEmpty("the source reports a mouse selection, not a caret move");

            TextView view = viewer.TextEditor.TextArea.TextView;
            VisualLine line = view.VisualLines.Single(line => line.FirstDocumentLine.LineNumber == 2);
            Point point = view.TranslatePoint(new Point(8, line.VisualTop + (line.Height / 2)), editor)!.Value;
            editor.MouseMove(point, RawInputModifiers.None);
            editor.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            editor.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            selected.Should().Equal(1);
        }
        finally
        {
            editor.Close();
        }
    }

    [AvaloniaTest]
    public async Task ViewTextAsync_should_select_fixed_diff_and_bounded_binary_hex_modes()
    {
        FileViewer viewer = CreateViewer();
        FileViewer.TestAccessor accessor = viewer.GetTestAccessor();
        int loaded = 0;
        viewer.TextLoaded += (_, _) => loaded++;

        await viewer.ViewTextAsync("change.patch", "@@ -1 +1 @@\n-old\n+new");

        accessor.ViewMode.Should().Be(ViewMode.FixedDiff);
        viewer.TextEditor.Text.Should().Contain("+new");

        await viewer.ViewTextAsync("payload.bin", "prefix\0\0\0\0\0\0suffix");

        accessor.ViewMode.Should().Be(ViewMode.Text);
        viewer.TextEditor.Text.Should().Contain("Binary file: payload.bin");
        viewer.TextEditor.Text.Should().Contain("0000");
        viewer.TextEditor.Text.Should().Contain("70 72 65 66 69 78 00 00");
        loaded.Should().Be(2);
    }

    [AvaloniaTest]
    public async Task ViewChangesAsync_should_marshal_rendering_from_a_worker_thread()
    {
        string filePath = Path.Combine(_workingDirectory, "worker-thread.txt");
        File.WriteAllText(filePath, "before\n");
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "worker-thread.txt" });
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });
        ObjectId head = _module.GetCurrentCheckout();
        File.WriteAllText(filePath, "after\n");

        FileStatusItem item = new(
            new GitRevision(head),
            new GitRevision(ObjectId.WorkTreeId) { ParentIds = [head] },
            new GitItemStatus("worker-thread.txt") { IsTracked = true, Staged = StagedStatus.WorkTree });
        FileViewer viewer = CreateViewer();

        await Task.Run(() => viewer.ViewChangesAsync(item, CancellationToken.None));

        viewer.GetTestAccessor().ViewMode.Should().Be(ViewMode.Diff);
        viewer.TextEditor.Text.Should().Contain("+after");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public async Task ViewChangesAsync_should_render_status_only_failures_without_running_a_diff()
    {
        GitItemStatus status = new("parse-error")
        {
            IsStatusOnly = true,
            ErrorMessage = "Unable to parse the diff output.",
        };
        FileStatusItem item = new(null, new GitRevision(ObjectId.WorkTreeId), status);
        FileViewer viewer = CreateViewer();

        await viewer.ViewChangesAsync(item, CancellationToken.None);

        viewer.GetTestAccessor().ViewMode.Should().Be(ViewMode.Text);
        viewer.TextEditor.Text.Should().Be("Unable to parse the diff output.");
    }

    [AvaloniaTest]
    public void FileViewer_should_preserve_blob_control_and_translation_identities()
    {
        FileViewer viewer = CreateViewer();
        ITranslation translation = Substitute.For<ITranslation>();

        viewer.AddTranslationItems(translation);

        viewer.FindControl<Border>("PictureBox").Should().NotBeNull();
        viewer.FindControl<HyperlinkButton>("_NO_TRANSLATE_lblShowPreview").Should().NotBeNull();
        viewer.FindControl<ComboBox>("encodingToolStripComboBox").Should().NotBeNull();
        viewer.FindControl<MenuItem>("showNonprintableCharactersToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("showSyntaxHighlightingToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<Button>("showNonPrintChars").Should().NotBeNull();
        viewer.FindControl<Button>("showSyntaxHighlighting").Should().NotBeNull();
        translation.Received(1).AddTranslationItem(nameof(FileViewer), "_largeFileSizeWarning", "Text", "This file is {0:N1} MB. Showing large files can be slow. Click to show anyway.");
        translation.Received(1).AddTranslationItem(nameof(FileViewer), "_cannotViewImage", "Text", "Cannot view image {0}");
        translation.Received(1).AddTranslationItem(nameof(FileViewer), "_binaryFile", "Text", "Binary file: {0}");
        translation.Received(1).AddTranslationItem(nameof(FileViewer), "showNonprintableCharactersToolStripMenuItem", "Text", "S&how nonprinting characters");
        translation.Received(1).AddTranslationItem(nameof(FileViewer), "showSyntaxHighlightingToolStripMenuItem", "Text", "Show synta&x highlighting");
        translation.Received(1).AddTranslationItem(nameof(FileViewer), "showNonPrintChars", "ToolTipText", "Show nonprinting characters");
        translation.Received(1).AddTranslationItem(nameof(FileViewer), "showSyntaxHighlighting", "ToolTipText", "Show syntax highlighting");
    }

    [AvaloniaTest]
    [TestCase("COMMIT_EDITMSG", typeof(CommitMessageHighlightingStrategy))]
    [TestCase("git-rebase-todo", typeof(RebaseTodoHighlightingStrategy))]
    public async Task FileViewer_should_use_its_repository_for_git_specific_syntax(string name, Type expected)
    {
        using FileViewer viewer = CreateViewer();

        await viewer.ViewTextAsync(name, "pick fb42c9e example\n");

        viewer.TextEditor.TextArea.TextView.LineTransformers.OfType<GitHighlightingStrategyBase>()
            .Should().ContainSingle().Which.Should().BeOfType(expected);
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task Display_options_should_apply_file_syntax_settings_and_independent_diff_rendering()
    {
        bool originalRememberNonPrinting = AppSettings.RememberShowNonPrintingCharsPreference;
        bool originalShowNonPrinting = AppSettings.ShowNonPrintingChars.Value;
        bool originalRememberSyntax = AppSettings.RememberShowSyntaxHighlightingInDiff;
        bool originalShowSyntax = AppSettings.ShowSyntaxHighlightingInDiff.Value;
        bool originalEolGlyph = AppSettings.ShowEolMarkerAsGlyph;
        int originalRuler = AppSettings.DiffVerticalRulerPosition;
        try
        {
            AppSettings.RememberShowNonPrintingCharsPreference = true;
            AppSettings.ShowNonPrintingChars.Value = false;
            AppSettings.RememberShowSyntaxHighlightingInDiff = true;
            AppSettings.ShowSyntaxHighlightingInDiff.Value = true;
            AppSettings.ShowEolMarkerAsGlyph = false;
            AppSettings.DiffVerticalRulerPosition = 88;

            string filePath = Path.Combine(_workingDirectory, "Sample.cs");
            File.WriteAllText(filePath, "internal class Sample { }\n");
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "Sample.cs" });
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "first" });
            ObjectId firstRevisionId = _module.GetCurrentCheckout();
            File.WriteAllText(filePath, "internal class Sample\n{\n    // visible whitespace\n}\n");
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "Sample.cs" });
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "second" });
            ObjectId secondRevisionId = _module.GetCurrentCheckout();

            GitRevision firstRevision = new(firstRevisionId);
            GitRevision secondRevision = new(secondRevisionId) { ParentIds = [firstRevisionId] };
            GitItemStatus status = new("Sample.cs") { IsTracked = true };
            FileStatusItem item = new(firstRevision, secondRevision, status);
            FileViewer viewer = CreateViewer();
            FileViewer.TestAccessor accessor = viewer.GetTestAccessor();
            FileViewerInternal editor = viewer.FindControl<FileViewerInternal>("internalFileViewer")!;
            MenuItem showSyntaxHighlightingMenuItem = viewer.FindControl<MenuItem>("showSyntaxHighlightingToolStripMenuItem")!;
            Button showSyntaxHighlighting = viewer.FindControl<Button>("showSyntaxHighlighting")!;
            MenuItem showNonprintableCharactersMenuItem = viewer.FindControl<MenuItem>("showNonprintableCharactersToolStripMenuItem")!;
            Button showNonPrintChars = viewer.FindControl<Button>("showNonPrintChars")!;
            int reloadRequests = 0;
            viewer.ExtraDiffArgumentsChanged += (_, _) => reloadRequests++;

            await viewer.ViewChangesAsync(item, CancellationToken.None);

            accessor.ViewMode.Should().Be(ViewMode.Diff);
            viewer.TextEditor.SyntaxHighlighting.Should().NotBeNull();
            viewer.TextEditor.SyntaxHighlighting!.Name.Should().Be("C#");
            (editor.DiffHighlightService is not null).Should().BeTrue();
            viewer.TextEditor.ShowLineNumbers.Should().BeFalse();
            showSyntaxHighlightingMenuItem.IsVisible.Should().BeTrue();
            showSyntaxHighlightingMenuItem.IsChecked.Should().BeTrue();
            showSyntaxHighlighting.Classes.Should().Contain("checked");
            viewer.VRulerPosition.Should().Be(88);
            viewer.TextEditor.Options.ShowColumnRulers.Should().BeTrue();

            showNonprintableCharactersMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            viewer.TextEditor.Options.ShowSpaces.Should().BeTrue();
            viewer.TextEditor.Options.ShowTabs.Should().BeTrue();
            viewer.TextEditor.Options.ShowEndOfLine.Should().BeTrue();
            viewer.TextEditor.Options.EndOfLineCRLFGlyph.Should().Be("\\r\\n");
            showNonPrintChars.Classes.Should().Contain("checked");
            AppSettings.ShowNonPrintingChars.Value.Should().BeTrue();

            showSyntaxHighlightingMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            viewer.TextEditor.SyntaxHighlighting.Should().BeNull();
            (editor.DiffHighlightService is not null).Should().BeTrue();
            AppSettings.ShowSyntaxHighlightingInDiff.Value.Should().BeFalse();
            reloadRequests.Should().Be(1);

            showSyntaxHighlightingMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            viewer.TextEditor.SyntaxHighlighting.Should().NotBeNull();
            (editor.DiffHighlightService is not null).Should().BeTrue();
            reloadRequests.Should().Be(2);
            viewer.FindControl<Border>("fileviewerToolbar")!.IsVisible = true;
            await CaptureModeIfRequestedAsync(viewer, "DisplayOptions");

            viewer.ShowLineNumbers = false;
            await viewer.ViewTextAsync("Sample.cs", "line one\nline two\n");
            viewer.TextEditor.ShowLineNumbers.Should().BeFalse();
            viewer.ShowLineNumbers = null;
            viewer.TextEditor.ShowLineNumbers.Should().BeTrue();
            showSyntaxHighlighting.IsVisible.Should().BeFalse();
        }
        finally
        {
            AppSettings.RememberShowNonPrintingCharsPreference = originalRememberNonPrinting;
            AppSettings.ShowNonPrintingChars.Value = originalShowNonPrinting;
            AppSettings.RememberShowSyntaxHighlightingInDiff = originalRememberSyntax;
            AppSettings.ShowSyntaxHighlightingInDiff.Value = originalShowSyntax;
            AppSettings.ShowEolMarkerAsGlyph = originalEolGlyph;
            AppSettings.DiffVerticalRulerPosition = originalRuler;
        }
    }

    [AvaloniaTest]
    public async Task ViewGitItemAsync_should_render_an_image_blob_and_preserve_view_state()
    {
        string imagePath = Path.Combine(_workingDirectory, "tiny.png");
        using (RenderTargetBitmap bitmap = new(new PixelSize(8, 8), new Vector(96, 96)))
        {
            using DrawingContext drawingContext = bitmap.CreateDrawingContext();
            drawingContext.FillRectangle(Brushes.Crimson, new Rect(0, 0, 8, 8));
            using FileStream imageStream = File.Create(imagePath);
            bitmap.Save(imageStream, PngBitmapEncoderOptions.Default);
        }

        _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tiny.png" });
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "image" });
        ObjectId revision = _module.GetCurrentCheckout();
        ObjectId blob = _module.GetFileBlobHash("tiny.png", revision);
        GitItemStatus item = new("tiny.png") { TreeId = blob };
        FileViewer viewer = CreateViewer();
        FileViewer.TestAccessor accessor = viewer.GetTestAccessor();
        Border pictureBox = viewer.FindControl<Border>("PictureBox")!;
        Image imagePreview = viewer.FindControl<Image>("ImagePreview")!;

        await viewer.ViewGitItemAsync(item, revision);

        accessor.ViewMode.Should().Be(ViewMode.Image);
        pictureBox.IsVisible.Should().BeTrue();
        viewer.TextEditor.IsVisible.Should().BeFalse();
        imagePreview.Source.Should().NotBeNull();

        await CaptureModeIfRequestedAsync(viewer, "Image");

        string iconPath = Path.Combine(GetRepositoryRoot(), "src", "app", "GitUI", "Resources", "Icons", "difftastic.ico");
        await viewer.ViewFileAsync(iconPath);

        accessor.ViewMode.Should().Be(ViewMode.Image);
        imagePreview.Source.Should().NotBeNull();
    }

    [AvaloniaTest]
    public async Task ViewGitItemAsync_should_show_a_large_committed_blob_without_deferring_after_preview_is_requested()
    {
        const string fileName = "large.bin";
        byte[] content = new byte[(5 * 1024 * 1024) + 1];
        content[0] = 0xFF;
        File.WriteAllBytes(Path.Combine(_workingDirectory, fileName), content);
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", fileName });
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "large blob".Quote() });
        ObjectId revision = _module.GetCurrentCheckout();
        GitItemStatus item = new(fileName) { TreeId = _module.GetFileBlobHash(fileName, revision) };
        using FileViewer viewer = CreateViewer();
        HyperlinkButton previewLink = viewer.FindControl<HyperlinkButton>("_NO_TRANSLATE_lblShowPreview")!;

        await viewer.ViewFileAsync(fileName);
        previewLink.IsVisible.Should().BeTrue();

        await viewer.ViewGitItemAsync(item, revision);

        previewLink.IsVisible.Should().BeTrue();
        viewer.TextEditor.Text.Should().BeEmpty();
        previewLink.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        await WaitUntilAsync(() => viewer.TextEditor.Text.Contains(fileName, StringComparison.Ordinal));

        previewLink.IsVisible.Should().BeFalse();
        viewer.TextEditor.Text.Should().Contain(fileName);
        viewer.FilePreamble.Should().BeEmpty();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task Viewing_an_image_should_clear_the_previous_text_preamble(bool useBlob)
    {
        const string imageName = "sample.ico";
        const string textName = "previous.txt";
        string imagePath = Path.Combine(GetRepositoryRoot(), "src", "app", "GitUI", "Resources", "Icons", "difftastic.ico");
        File.Copy(imagePath, Path.Combine(_workingDirectory, imageName));
        File.WriteAllText(Path.Combine(_workingDirectory, textName), "previous text", Encoding.Unicode);
        using FileViewer viewer = CreateViewer();
        await viewer.ViewFileAsync(textName);
        viewer.FilePreamble.Should().Equal(Encoding.Unicode.GetPreamble());

        if (useBlob)
        {
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", imageName });
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "image blob".Quote() });
            ObjectId revision = _module.GetCurrentCheckout();
            GitItemStatus item = new(imageName) { TreeId = _module.GetFileBlobHash(imageName, revision) };
            await viewer.ViewGitItemAsync(item, revision);
        }
        else
        {
            await viewer.ViewFileAsync(imageName);
        }

        viewer.GetTestAccessor().ViewMode.Should().Be(ViewMode.Image);
        viewer.FilePreamble.Should().BeNull();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task ViewFileAsync_should_show_path_errors_without_diff_actions(bool isDirectory)
    {
        const string path = "unavailable";
        string fullPath = Path.Combine(_workingDirectory, path);
        if (isDirectory)
        {
            Directory.CreateDirectory(fullPath);
        }

        GitItemStatus status = new(path) { IsNew = true, Staged = StagedStatus.WorkTree };
        FileStatusItem item = new(new GitRevision(ObjectId.IndexId), new GitRevision(ObjectId.WorkTreeId), status);
        using FileViewer viewer = CreateViewer();
        FileViewerInternal editor = viewer.FindControl<FileViewerInternal>("internalFileViewer")!;

        await viewer.ViewFileAsync(path, item: item, line: 10, openWithDifftool: () => { });

        viewer.TextEditor.Text.Should().Be(isDirectory ? $"Directory: {path}" : $"File {fullPath} does not exist");
        viewer.SupportLinePatching.Should().BeFalse();
        editor.OpenWithDifftool.Should().BeNull();
    }

    [AvaloniaTest]
    public async Task ViewPatchAsync_should_publish_text_after_applying_the_requested_line()
    {
        using AppSettingsTestScope settings = new();
        AppSettings.DiffDisplayAppearance.Value = DiffDisplayAppearance.Patch;
        using FormEditor editor = new(_commands, fileName: null, showWarning: false);
        using FileViewer viewer = editor.FindControl<FileViewer>("fileViewer")!;
        GitRevision first = new(ObjectId.Random());
        GitRevision second = new(ObjectId.Random());
        FileStatusItem item = new(first, second, new GitItemStatus("sample.txt") { IsChanged = true, IsTracked = true });
        const string patch = "diff --git a/sample.txt b/sample.txt\n--- a/sample.txt\n+++ b/sample.txt\n"
            + "@@ -1 +1 @@\n-old\n+first\n@@ -30 +30 @@\n-before\n+target\n";
        try
        {
            editor.Show();
            List<int> publishedLines = [];
            viewer.TextLoaded += (_, _) => publishedLines.Add(viewer.TextEditor.TextArea.Caret.Line);

            await viewer.ViewPatchAsync(item, patch, line: 30, openWithDifftool: null);

            viewer.TextEditor.TextArea.Caret.Line.Should().Be(9);
            publishedLines.Should().Equal(9);
        }
        finally
        {
            editor.Close();
        }
    }

    [AvaloniaTest]
    public async Task ViewFixedPatchAsync_should_defer_an_oversized_patch()
    {
        using FileViewer viewer = CreateViewer();
        const int lineLength = 1024;
        const int addedLines = (5 * 1024 * 1024 / lineLength) + 1;
        string addedLine = "+" + new string('x', lineLength - 2) + "\n";
        string patch = $"diff --git a/sample.txt b/sample.txt\n--- /dev/null\n+++ b/sample.txt\n@@ -0,0 +1,{addedLines} @@\n"
            + string.Concat(Enumerable.Repeat(addedLine, addedLines));

        await viewer.ViewFixedPatchAsync("sample.patch", patch);

        viewer.FindControl<HyperlinkButton>("_NO_TRANSLATE_lblShowPreview")!.IsVisible.Should().BeTrue();
        viewer.GetText().Should().BeEmpty();

        const string smallerPatch = "@@ -1 +1 @@\n-before\n+after\n";
        await viewer.ViewFixedPatchAsync("sample.patch", smallerPatch);

        viewer.FindControl<HyperlinkButton>("_NO_TRANSLATE_lblShowPreview")!.IsVisible.Should().BeFalse();
        viewer.GetText().Should().Be(smallerPatch);
    }

    [AvaloniaTest]
    [TestCase(ViewMode.Difftastic)]
    [TestCase(ViewMode.RangeDiff)]
    [TestCase(ViewMode.Grep)]
    public async Task Special_diff_loads_should_publish_on_the_owner_ui_thread(ViewMode viewMode)
    {
        using FormEditor editor = new(_commands, fileName: null, showWarning: false);
        using FileViewer viewer = editor.FindControl<FileViewer>("fileViewer")!;
        GitRevision revision = new(ObjectId.Random());
        FileStatusItem item = new(null, revision, new GitItemStatus("sample.txt") { IsTracked = true });
        try
        {
            editor.Show();
            List<bool> publishThreadAccess = [];
            viewer.TextLoaded += (_, _) => publishThreadAccess.Add(Dispatcher.UIThread.CheckAccess());

            await Task.Run(() =>
            {
                Dispatcher.UIThread.CheckAccess().Should().BeFalse();
                return viewMode switch
                {
                    ViewMode.Difftastic => viewer.ViewDifftasticAsync("sample.txt", "sample.txt --- Text\n1 worker content\n"),
                    ViewMode.RangeDiff => viewer.ViewRangeDiffAsync("sample.txt", "1: abc1234 = 1: abc1234 worker content\n"),
                    ViewMode.Grep => viewer.ViewGrepAsync(item, "12:worker content\n"),
                    _ => throw new ArgumentOutOfRangeException(nameof(viewMode)),
                };
            });

            viewer.GetText().Should().Contain("worker content");
            publishThreadAccess.Should().Equal(true);
        }
        finally
        {
            editor.Close();
        }
    }

    [AvaloniaTest]
    public async Task ViewTextAsync_should_defer_large_content_until_the_preview_link_is_clicked()
    {
        FileViewer viewer = CreateViewer();
        FileViewer.TestAccessor accessor = viewer.GetTestAccessor();
        string largeText = new('x', (5 * 1024 * 1024) + 1);

        await viewer.ViewTextAsync("large.txt", largeText);

        accessor.ShowPreviewLink.IsVisible.Should().BeTrue();
        accessor.ShowPreviewLink.Content.Should().Be("This file is 5.0 MB. Showing large files can be slow. Click to show anyway.");
        viewer.TextEditor.Text.Should().BeEmpty();
        await CaptureModeIfRequestedAsync(viewer, "LargePreview");

        // Closing the opt-in capture host deliberately cancels detached viewer work, just as
        // closing a real parent view does. Restore the preview request before exercising it.
        await viewer.ViewTextAsync("large.txt", largeText);

        accessor.ShowPreviewLink.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        await WaitUntilAsync(() => viewer.TextEditor.Text.Length == largeText.Length);

        accessor.ShowPreviewLink.IsVisible.Should().BeFalse();
        viewer.TextEditor.Text.Should().Be(largeText);
    }

    [AvaloniaTest]
    public void FileViewer_should_show_the_current_encoding_when_the_toolbar_becomes_visible()
    {
        using FileViewer viewer = CreateViewer();
        Window window = new() { Width = 640, Height = 360, Content = viewer };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Border toolbar = viewer.FindControl<Border>("fileviewerToolbar")!;
            ComboBox encodings = viewer.FindControl<ComboBox>("encodingToolStripComboBox")!;

            toolbar.IsVisible.Should().BeFalse();
            encodings.SelectedItem.Should().BeNull();

            toolbar.IsVisible = true;

            encodings.SelectedItem.Should().Be(_module.FilesEncoding.EncodingName);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public async Task Encoding_selector_should_use_the_shared_available_encodings_and_request_a_reload()
    {
        using FileViewer viewer = CreateViewer();
        Window window = new() { Width = 640, Height = 360, Content = viewer };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ComboBox encodings = viewer.FindControl<ComboBox>("encodingToolStripComboBox")!;
            Encoding initial = Encoding.UTF8;
            Encoding replacement = Encoding.Unicode;
            int reloadRequests = 0;
            viewer.ExtraDiffArgumentsChanged += (_, _) => reloadRequests++;

            viewer.Encoding = initial;
            encodings.SelectedItem.Should().Be(initial.EncodingName);
            encodings.SelectedItem = replacement.EncodingName;

            viewer.Encoding.Should().Be(replacement);
            reloadRequests.Should().Be(1);

            viewer.FindControl<Border>("fileviewerToolbar")!.IsVisible = true;
            await CaptureModeIfRequestedAsync(viewer, "EncodingToolbar", window);

            File.WriteAllBytes(
                Path.Combine(_workingDirectory, "legacy.txt"),
                Encoding.Latin1.GetBytes("café!"));
            viewer.Encoding = Encoding.Latin1;
            await viewer.ViewFileAsync("legacy.txt");

            viewer.TextEditor.Text.Should().Be("café!");
            viewer.FilePreamble.Should().BeEmpty();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public async Task Added_file_line_patching_should_include_copied_files_and_respect_revision_and_path_state(
        [Values] bool copied,
        [Values(StagedStatus.WorkTree, StagedStatus.Index, StagedStatus.Unknown)] StagedStatus staged,
        [Values] bool exists)
    {
        const string fileName = "added-file.txt";
        if (exists)
        {
            File.WriteAllText(Path.Combine(_workingDirectory, fileName), "added file contents\n");
        }

        GitItemStatus status = new(fileName)
        {
            IsNew = !copied,
            IsCopied = copied,
            IsTracked = true,
            Staged = staged,
        };
        FileStatusItem item = new(firstRev: null, secondRev: new GitRevision(ObjectId.Random()), status);
        FileViewer viewer = CreateViewer();

        await viewer.ViewTextAsync(fileName, "added file contents\n", item);

        viewer.SupportLinePatching.Should().Be(staged is StagedStatus.WorkTree or StagedStatus.Index || !exists);
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task Reset_selected_lines_should_decline_Escape_and_window_close_without_changing_the_file([Values] bool byEscape)
    {
        DiffDisplayAppearance originalAppearance = AppSettings.DiffDisplayAppearance.Value;
        Window owner = new();
        try
        {
            AppSettings.DiffDisplayAppearance.Value = DiffDisplayAppearance.Patch;
            const string fileName = "reset-selected-lines.txt";
            string path = Path.Combine(_workingDirectory, fileName);
            File.WriteAllText(path, "one\ntwo\nthree\n");
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", fileName }).Should().BeTrue();
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "reset selected lines".Quote() }).Should().BeTrue();
            ObjectId head = _module.GetCurrentCheckout();
            const string changed = "ONE\ntwo\nthree\n";
            File.WriteAllText(path, changed);
            FileStatusItem item = new(
                new GitRevision(ObjectId.IndexId) { ParentIds = [head] },
                new GitRevision(ObjectId.WorkTreeId) { ParentIds = [ObjectId.IndexId] },
                new GitItemStatus(fileName) { IsTracked = true, IsChanged = true, Staged = StagedStatus.WorkTree });
            FileViewer viewer = CreateViewer();
            owner.Content = viewer;
            owner.Show();
            await viewer.ViewChangesAsync(item, CancellationToken.None);
            viewer.SupportLinePatching.Should().BeTrue();
            int selectedLine = viewer.GetText().IndexOf("+ONE", StringComparison.Ordinal);
            selectedLine.Should().BeGreaterThanOrEqualTo(0);
            viewer.TextEditor.Select(selectedLine, "+ONE".Length);
            int applied = 0;
            bool cancelButtonWasAbsent = false;
            viewer.PatchApplied += (_, _) => applied++;
            using UpstreamTaskDialogObserver opened = new(dialog =>
            {
                cancelButtonWasAbsent = !dialog.GetLogicalDescendants().OfType<Button>().Any(button => button.IsCancel);
                if (byEscape)
                {
                    dialog.RaiseEvent(new Avalonia.Input.KeyEventArgs
                    {
                        RoutedEvent = Avalonia.Input.InputElement.KeyDownEvent,
                        Key = Avalonia.Input.Key.Escape,
                        KeyModifiers = Avalonia.Input.KeyModifiers.None,
                    });
                }
                else
                {
                    dialog.Close();
                }
            });

            viewer.ResetNoncommittedSelectedLines();

            opened.Count.Should().Be(1);
            opened.Failure.Should().BeNull();
            cancelButtonWasAbsent.Should().BeTrue("AllowCancel makes Escape cancel rather than selecting No");
            applied.Should().Be(0);
            File.ReadAllText(path).Should().Be(changed);
            _module.GitExecutable.GetOutput(new GitArgumentBuilder("diff") { "--cached" }).Should().BeEmpty();
        }
        finally
        {
            owner.Close();
            AppSettings.DiffDisplayAppearance.Value = originalAppearance;
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task Selected_lines_should_stage_and_unstage_through_the_shared_patch_manager()
    {
        DiffDisplayAppearance originalAppearance = AppSettings.DiffDisplayAppearance.Value;
        try
        {
            AppSettings.DiffDisplayAppearance.Value = DiffDisplayAppearance.Patch;
            string filePath = Path.Combine(_workingDirectory, "selected-lines.txt");
            File.WriteAllText(filePath, "one\ntwo\nthree\n");
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "selected-lines.txt" }).Should().BeTrue();
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "selected lines".Quote() }).Should().BeTrue();
            ObjectId headId = _module.GetCurrentCheckout();
            File.WriteAllText(filePath, "ONE\ntwo\nTHREE\n");

            GitRevision indexRevision = new(ObjectId.IndexId) { ParentIds = [headId] };
            GitRevision workTreeRevision = new(ObjectId.WorkTreeId) { ParentIds = [ObjectId.IndexId] };
            FileStatusItem workTreeItem = new(
                indexRevision,
                workTreeRevision,
                new GitItemStatus("selected-lines.txt") { IsTracked = true, Staged = StagedStatus.WorkTree });
            FileViewer viewer = CreateViewer();
            int patchApplied = 0;
            viewer.PatchApplied += (_, _) => patchApplied++;

            await viewer.ViewChangesAsync(workTreeItem, CancellationToken.None);

            viewer.SupportLinePatching.Should().BeTrue();
            int selectedLine = viewer.GetText().IndexOf("+ONE", StringComparison.Ordinal);
            selectedLine.Should().BeGreaterThanOrEqualTo(0);
            viewer.TextEditor.Select(selectedLine, "+ONE".Length);
            viewer.StageSelectedLines(stage: true);

            string stagedDiff = _module.GitExecutable.GetOutput(new GitArgumentBuilder("diff") { "--cached" });
            string workTreeDiff = _module.GitExecutable.GetOutput(new GitArgumentBuilder("diff"));
            stagedDiff.Should().Contain("+ONE").And.NotContain("+THREE");
            workTreeDiff.Should().Contain("+THREE").And.NotContain("+ONE");
            patchApplied.Should().Be(1);

            FileStatusItem indexItem = new(
                new GitRevision(headId),
                new GitRevision(ObjectId.IndexId) { ParentIds = [headId] },
                new GitItemStatus("selected-lines.txt") { IsTracked = true, Staged = StagedStatus.Index });
            await viewer.ViewChangesAsync(indexItem, CancellationToken.None);
            int stagedLine = viewer.GetText().IndexOf("+ONE", StringComparison.Ordinal);
            viewer.TextEditor.Select(stagedLine, "+ONE".Length);
            viewer.StageSelectedLines(stage: false);

            _module.GitExecutable.GetOutput(new GitArgumentBuilder("diff") { "--cached" }).Should().BeEmpty();
            _module.GitExecutable.GetOutput(new GitArgumentBuilder("diff")).Should().Contain("+ONE").And.Contain("+THREE");
            patchApplied.Should().Be(2);
        }
        finally
        {
            AppSettings.DiffDisplayAppearance.Value = originalAppearance;
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task Selected_line_patch_failures_should_remove_file_mode_warnings_only_on_Windows()
    {
        using AppSettingsTestScope settings = new();
        DiffDisplayAppearance originalAppearance = AppSettings.DiffDisplayAppearance.Value;
        AppSettings.TestAccessor settingsAccessor = AppSettings.GetTestAccessor();
        string originalExecutablePath = settingsAccessor.ApplicationExecutablePath;
        FormCommit? form = null;
        Dictionary<FieldInfo, object?> originalServices = typeof(GitExtensions.Shims.WinForms.ShimHost)
            .GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(field => !field.IsInitOnly && !field.IsLiteral)
            .ToDictionary(field => field, field => field.GetValue(null));
        try
        {
            settingsAccessor.ApplicationExecutablePath = Path.Combine(AppContext.BaseDirectory, "GitExtensions.Avalonia.exe");
            ShimServices.Install(Substitute.For<IClassicDesktopStyleApplicationLifetime>());
            AppSettings.DiffDisplayAppearance.Value = DiffDisplayAppearance.Patch;
            _module.SetSetting("core.autocrlf", "false");
            _module.SetSetting("core.filemode", "false");
            const string fileName = "mode-warning.txt";
            string path = Path.Combine(_workingDirectory, fileName);
            File.WriteAllText(path, "one\ntwo\nthree\n");
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", fileName }).Should().BeTrue();
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("update-index") { "--chmod=+x", "--", fileName }).Should().BeTrue();
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
            File.WriteAllText(path, "ONE\ntwo\nthree\n");
            form = new FormCommit(_commands);
            form.Show();
            FileViewer viewer = form.FindControl<FileViewer>("SelectedDiff")
                ?? throw new AssertionException("The actual commit form did not create its diff viewer.");
            await WaitUntilAsync(() => viewer.TextEditor.Text.Contains("+ONE", StringComparison.Ordinal));
            viewer.SupportLinePatching.Should().BeTrue();
            int selectedLine = viewer.GetText().IndexOf("+ONE", StringComparison.Ordinal);
            selectedLine.Should().BeGreaterThanOrEqualTo(0);
            viewer.TextEditor.Select(selectedLine, "+ONE".Length);

            File.WriteAllText(path, "different\ntwo\nthree\n");
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", fileName }).Should().BeTrue();
            _module.GitExecutable.RunCommand(new GitArgumentBuilder("update-index") { "--chmod=-x", "--", fileName }).Should().BeTrue();
            string? message = null;
            int dialogs = 0;
            using IDisposable opened = Window.WindowOpenedEvent.AddClassHandler<Window>((dialog, _) =>
            {
                if (dialog.Classes.Contains("gitextensions-message-box"))
                {
                    dialogs++;
                    Dispatcher.UIThread.Post(() =>
                    {
                        message = string.Join('\n', dialog.GetLogicalDescendants().OfType<TextBlock>().Select(text => text.Text));
                        dialog.Close();
                    });
                }
            });
            viewer.StageSelectedLines(stage: true);

            dialogs.Should().Be(1);
            message.Should().NotBeNull().And.Contain("error:").And.Contain("patch does not apply").And.Contain("+ONE");
            if (OperatingSystem.IsWindows())
            {
                message.Should().NotContain("has type 100644, expected 100755");
            }
            else
            {
                message.Should().Contain("warning: mode-warning.txt has type 100644, expected 100755");
            }

            await WaitUntilAsync(() => viewer.GetText().Contains("+different", StringComparison.Ordinal));
        }
        finally
        {
            try
            {
                if (form is not null)
                {
                    form.Close();
                    await form.GetTestAccessor().ClosePersistenceTask;
                }
            }
            finally
            {
                foreach ((FieldInfo field, object? originalValue) in originalServices)
                {
                    field.SetValue(null, originalValue);
                }

                settingsAccessor.ApplicationExecutablePath = originalExecutablePath;
                AppSettings.DiffDisplayAppearance.Value = originalAppearance;
            }
        }
    }

    [AvaloniaTest]
    public void FileViewer_should_expose_portable_contract_and_navigate_change_blocks()
    {
        FileViewer viewer = new();
        FileViewerInternal internalViewer = viewer.FindControl<FileViewerInternal>("internalFileViewer")!;
        IFileViewer contract = internalViewer;
        viewer.ViewPatch("diff --git a/a.txt b/a.txt\n--- a/a.txt\n+++ b/a.txt\n@@ -1,4 +1,4 @@\n-old\n+new\n context\n@@ -8,2 +8,2 @@\n-before\n+after\n");

        contract.GetText().Should().Contain("+after");
        internalViewer.GoToFirstChange(contextLines: 3);
        int firstBlock = viewer.TextEditor.TextArea.Caret.Line;
        internalViewer.GoToNextChange(contextLines: 3);

        firstBlock.Should().Be(5);
        viewer.TextEditor.TextArea.Caret.Line.Should().Be(9);
        internalViewer.GoToPreviousChange(contextLines: 3);
        viewer.TextEditor.TextArea.Caret.Line.Should().Be(5);
        contract.TotalNumberOfLines.Should().BeGreaterThan(9);
    }

    [AvaloniaTest]
    public void FileViewer_should_scroll_to_a_first_change_below_the_measured_viewport()
    {
        FileViewer viewer = new();
        Window owner = new() { Width = 337, Height = 260, Content = viewer };
        try
        {
            owner.Show();
            Dispatcher.UIThread.RunJobs();
            string context = string.Concat(Enumerable.Range(1, 35).Select(line => $" context {line}\n"));
            viewer.ViewPatch("diff --git a/a.txt b/a.txt\n--- a/a.txt\n+++ b/a.txt\n@@ -1,36 +1,36 @@\n"
                + context + "-before\n+after\n");
            Dispatcher.UIThread.RunJobs();

            viewer.TextEditor.TextArea.TextView.ScrollOffset.Y.Should().BeGreaterThan(0,
                "a first change below the viewport must still be brought into view");
            viewer.TextEditor.Options.AllowScrollBelowDocument.Should().BeTrue(
                "long files retain the source editor's below-document scroll travel");
        }
        finally
        {
            owner.Close();
        }
    }

    [AvaloniaTest]
    public void FileViewerInternal_should_restore_the_first_visible_line_when_the_caret_is_outside_the_view()
    {
        using FileViewerInternal viewer = new();
        Window window = new() { Width = 480, Height = 250, Content = viewer };
        try
        {
            window.Show();
            string text = string.Join('\n', Enumerable.Range(1, 90).Select(line => $"line {line:00}"));
            viewer.Editor.ShowLineNumbers = true;
            viewer.SetText(text, openWithDifftool: null, ViewMode.Text, useGitColoring: false, contentIdentification: "same.txt");
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            viewer.Editor.TextArea.Caret.Position = new TextViewPosition(11, 3);
            TextView textView = viewer.Editor.TextArea.TextView;
            ScrollViewer scrollViewer = viewer.Editor.TextArea.FindAncestorOfType<ScrollViewer>()!;
            scrollViewer.Offset = new Vector(0, textView.GetVisualTopByDocumentLine(37) + 1);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            viewer.GetLineFromVisualPosY(0).Should().Be(36);

            string refreshed = text.Replace("line 50", "line fifty", StringComparison.Ordinal);
            bool restored = viewer.SetText(
                refreshed, openWithDifftool: null, ViewMode.Text, useGitColoring: false, contentIdentification: "same.txt");
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            restored.Should().BeTrue();
            viewer.Editor.TextArea.Caret.Line.Should().Be(11);
            viewer.Editor.TextArea.Caret.Column.Should().Be(3);
            viewer.GetLineFromVisualPosY(0).Should().Be(
                36,
                "the scroll position {0:R} must reach the document line starting at {1:R} and the first arranged line is {2}",
                textView.ScrollOffset.Y,
                textView.GetVisualTopByDocumentLine(37),
                textView.VisualLines[0].FirstDocumentLine.LineNumber);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void FileViewerInternal_should_restore_the_caret_for_the_same_content_identity()
    {
        FileViewerInternal viewer = new();
        string first = string.Join('\n', Enumerable.Range(1, 60).Select(line => $"line {line}"));
        string refreshed = first.Replace("line 40", "line forty", StringComparison.Ordinal);

        viewer.SetText(first, openWithDifftool: null, ViewMode.Text, useGitColoring: false, contentIdentification: "same.txt");
        viewer.Editor.TextArea.Caret.Position = new TextViewPosition(40, 3);

        bool restored = viewer.SetText(refreshed, openWithDifftool: null, ViewMode.Text, useGitColoring: false, contentIdentification: "same.txt");

        restored.Should().BeTrue();
        viewer.Editor.TextArea.Caret.Line.Should().Be(40);
        viewer.Editor.TextArea.Caret.Column.Should().Be(3);
    }

    [AvaloniaTest]
    [NonParallelizable]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public async Task Ordinary_content_should_restore_the_preferred_ruler_after_difftastic(bool sideBySide, bool patch)
    {
        using AppSettingsTestScope settings = new();
        int originalRuler = AppSettings.DiffVerticalRulerPosition;
        string? originalWidth = Environment.GetEnvironmentVariable("DFT_WIDTH");
        Window? window = null;
        try
        {
            AppSettings.DiffVerticalRulerPosition = 88;
            Environment.SetEnvironmentVariable("DFT_WIDTH", "80");
            FileViewer viewer = CreateViewer();
            window = new Window { Width = 700, Height = 400, Content = viewer };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            await WaitUntilAsync(() => viewer.VRulerPosition == 88);
            string text = "sample.cs --- Text\n" + (sideBySide ? "1 before".PadRight(40) + "1 after" : "1 before") + "\n";
            await viewer.ViewDifftasticAsync("sample.cs", text);
            viewer.VRulerPosition.Should().Be(sideBySide ? 38 : 0);
            viewer.TextEditor.Options.ShowColumnRulers.Should().Be(sideBySide);

            if (patch)
            {
                viewer.ViewPatch("diff --git a/sample.cs b/sample.cs\n--- a/sample.cs\n+++ b/sample.cs\n@@ -1 +1 @@\n-before\n+after\n");
            }
            else
            {
                await viewer.ViewTextAsync("plain.cs", "ordinary text", CancellationToken.None);
            }

            viewer.VRulerPosition.Should().Be(88);
            viewer.TextEditor.Options.ShowColumnRulers.Should().BeTrue();
            await viewer.ViewDifftasticAsync("sample.cs", text);
            viewer.VRulerPosition.Should().Be(sideBySide ? 38 : 0);
        }
        finally
        {
            window?.Close();
            AppSettings.DiffVerticalRulerPosition = originalRuler;
            Environment.SetEnvironmentVariable("DFT_WIDTH", originalWidth);
        }
    }

    private FileViewer CreateViewer()
        => new() { UICommandsSource = _source };

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        DateTime timeout = DateTime.UtcNow.AddSeconds(10);
        while (!condition())
        {
            if (DateTime.UtcNow >= timeout)
            {
                throw new TimeoutException("The FileViewer operation did not complete.");
            }

            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }
    }

    private static async Task CaptureModeIfRequestedAsync(FileViewer viewer, string mode, Window? owner = null)
    {
        if (Environment.GetEnvironmentVariable("GITEXT_CAPTURE_FILE_VIEWER_MODES") != "1")
        {
            return;
        }

        string outputDirectory = Path.Combine(GetRepositoryRoot(), "eng", "avalonia", "parity-shots", "FileViewerModes");
        Directory.CreateDirectory(outputDirectory);
        Window window = owner ?? new Window
        {
            Width = 640,
            Height = 360,
            Content = viewer,
        };
        if (owner is null)
        {
            window.Show();
        }

        try
        {
            foreach ((string themeName, ThemeVariant theme) in new[]
                     {
                         ("Light", ThemeVariant.Light),
                         ("Dark", ThemeVariant.Dark),
                     })
            {
                foreach ((string scaleName, double scale) in new[]
                         {
                             ("100", 1d),
                             ("200", 2d),
                         })
                {
                    window.RequestedThemeVariant = theme;
                    window.SetRenderScaling(scale);
                    Dispatcher.UIThread.RunJobs();
                    using WriteableBitmap? frame = window.CaptureRenderedFrame();
                    frame.Should().NotBeNull();
                    using FileStream stream = File.Create(Path.Combine(outputDirectory, $"{mode}-{themeName}-{scaleName}.png"));
                    frame!.Save(stream, PngBitmapEncoderOptions.Default);
                }
            }
        }
        finally
        {
            if (owner is null)
            {
                window.Close();
            }
        }

        await Task.CompletedTask;
    }

    private static string GetRepositoryRoot()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
