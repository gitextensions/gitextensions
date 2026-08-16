using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.Editor;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FileViewerSearchTests
{
    [AvaloniaTest]
    public void FileViewer_should_create_the_find_window_only_when_requested()
    {
        FileViewer viewer = new();
        FileViewer.TestAccessor accessor = viewer.GetTestAccessor();

        accessor.IsFindAndReplaceFormCreated.Should().BeFalse();

        FindAndReplaceForm first = accessor.FindAndReplaceForm;

        accessor.IsFindAndReplaceFormCreated.Should().BeTrue();
        first.Close();
        Dispatcher.UIThread.RunJobs();
        accessor.IsFindAndReplaceFormCreated.Should().BeFalse();

        FindAndReplaceForm second = accessor.FindAndReplaceForm;
        second.Should().NotBeSameAs(first);
        second.Close();
    }

    [AvaloniaTest]
    public void SetFileLoader_should_not_create_the_find_window_and_should_forward_it_on_first_use()
    {
        FileViewer viewer = new();
        GetNextFileFnc fileLoader = (bool seekBackward, bool loop, out FileStatusItem? item, out Task load) =>
        {
            item = null;
            load = Task.CompletedTask;
            return false;
        };

        viewer.SetFileLoader(fileLoader);

        FileViewer.TestAccessor accessor = viewer.GetTestAccessor();
        accessor.IsFindAndReplaceFormCreated.Should().BeFalse();
        FindAndReplaceForm form = accessor.FindAndReplaceForm;
        form.GetTestAccessor().FileLoader.Should().BeSameAs(fileLoader);
        form.Close();
    }

    [AvaloniaTest]
    public void FileViewer_should_close_the_find_window_when_detached()
    {
        FileViewer viewer = new();
        Window owner = new() { Content = viewer };
        owner.Show();
        try
        {
            viewer.Find(replace: false);
            FindAndReplaceForm form = viewer.GetTestAccessor().FindAndReplaceForm;
            form.IsVisible.Should().BeTrue();

            owner.Content = null;
            Dispatcher.UIThread.RunJobs();

            viewer.GetTestAccessor().IsFindAndReplaceFormCreated.Should().BeFalse();
            form.IsVisible.Should().BeFalse();
        }
        finally
        {
            owner.Close();
        }
    }

    [Test]
    public void TextEditorSearcher_should_find_case_sensitive_and_whole_word_matches()
    {
        TextEditorSearcher searcher = new()
        {
            Document = new TextDocument("alpha ALPHA alphabet alpha"),
            LookFor = "alpha",
            MatchCase = true,
            MatchWholeWordOnly = true,
        };

        TextRange? first = searcher.FindNext(0, searchBackward: false, out bool firstLooped);
        TextRange? second = searcher.FindNext(first!.EndOffset, searchBackward: false, out bool secondLooped);

        first.Offset.Should().Be(0);
        firstLooped.Should().BeFalse();
        second.Should().NotBeNull();
        second!.Offset.Should().Be(21);
        secondLooped.Should().BeFalse();
    }

    [Test]
    public void TextEditorSearcher_should_search_backwards_and_loop_inside_scan_region()
    {
        TextEditorSearcher searcher = new()
        {
            Document = new TextDocument("outside one two one outside"),
            LookFor = "one",
        };
        searcher.SetScanRegion(offset: 8, length: 11);

        TextRange? previous = searcher.FindNext(searcher.EndOffset, searchBackward: true, out bool firstLooped);
        TextRange? looped = searcher.FindNext(searcher.BeginOffset, searchBackward: true, out bool secondLooped);

        previous.Should().NotBeNull();
        previous!.Offset.Should().Be(16);
        firstLooped.Should().BeFalse();
        looped.Should().NotBeNull();
        looped!.Offset.Should().Be(16);
        secondLooped.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task FindAndReplaceForm_should_select_forward_and_backward_matches_in_FileViewer()
    {
        FileViewer viewer = new();
        await viewer.ViewTextAsync("sample.txt", "one two one");
        FindAndReplaceForm form = viewer.GetTestAccessor().FindAndReplaceForm;
        FindAndReplaceForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SetEditor(viewer.TextEditor);
        accessor.TxtLookFor.Text = "one";

        TextRange? first = await form.FindNextAsync(viaF3: false, searchBackward: false, messageIfNotFound: null);
        TextRange? second = await form.FindNextAsync(viaF3: false, searchBackward: false, messageIfNotFound: null);
        TextRange? previous = await form.FindNextAsync(viaF3: false, searchBackward: true, messageIfNotFound: null);

        first!.Offset.Should().Be(0);
        second!.Offset.Should().Be(8);
        previous!.Offset.Should().Be(0);
        viewer.TextEditor.SelectedText.Should().Be("one");

        form.Close();
    }

    [AvaloniaTest]
    public async Task FindAndReplaceForm_should_replace_the_selection_and_find_the_next_match()
    {
        FileViewer viewer = new();
        viewer.TextEditor.IsReadOnly = false;
        await viewer.ViewTextAsync("sample.txt", "cat cat");
        FindAndReplaceForm form = viewer.GetTestAccessor().FindAndReplaceForm;
        FindAndReplaceForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SetEditor(viewer.TextEditor);
        accessor.TxtLookFor.Text = "cat";
        accessor.TxtReplaceWith.Text = "dog";
        accessor.Search.SetScanRegion(offset: 0, length: viewer.TextEditor.Text.Length);

        await form.FindNextAsync(viaF3: false, searchBackward: false, messageIfNotFound: null);
        await accessor.ReplaceAsync();

        viewer.TextEditor.Text.Should().Be("dog cat");
        viewer.TextEditor.SelectedText.Should().Be("cat");
        viewer.TextEditor.SelectionStart.Should().Be(4);
        accessor.Search.HasScanRegion.Should().BeTrue();
        accessor.Search.EndOffset.Should().Be(viewer.TextEditor.Text.Length);

        form.Close();
    }

    [AvaloniaTest]
    public async Task FileViewerInternal_should_highlight_and_navigate_selected_text_occurrences()
    {
        FileViewer viewer = new();
        await viewer.ViewTextAsync("sample.txt", "one two one");
        FileViewerInternal internalViewer = viewer.FindControl<FileViewerInternal>("internalFileViewer")!;

        viewer.TextEditor.Select(0, 3);

        internalViewer.GetTestAccessor().SelectionOccurrences.Should().HaveCount(2);

        viewer.TextEditor.TextArea.Caret.Offset = 0;
        viewer.GoToNextOccurrence();
        viewer.TextEditor.TextArea.Caret.Offset.Should().Be(8);

        viewer.GoToPreviousOccurrence();
        viewer.TextEditor.TextArea.Caret.Offset.Should().Be(0);
    }

    [AvaloniaTest]
    public async Task FindAndReplaceForm_should_show_the_file_name_and_render_the_scan_region()
    {
        FileViewer viewer = new();
        await viewer.ViewTextAsync("folder/sample.txt", "first\nselected text\nlast");
        FindAndReplaceForm form = viewer.GetTestAccessor().FindAndReplaceForm;
        FindAndReplaceForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SetEditor(viewer.TextEditor);
        form.ReplaceMode = false;

        accessor.Title.Should().Be("Find - sample.txt");

        accessor.Search.SetScanRegion(offset: 6, length: 13);

        accessor.Title.Should().Be("Find - sample.txt (selection only)");
        accessor.HasScanRegionRenderer.Should().BeTrue();

        accessor.Search.ClearScanRegion();
        accessor.HasScanRegionRenderer.Should().BeFalse();
        form.Close();
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void FileViewer_should_restore_diff_appearance_text_and_continuous_scroll_options()
    {
        DiffDisplayAppearance originalAppearance = AppSettings.DiffDisplayAppearance.Value;
        bool originalContinuousScroll = AppSettings.AutomaticContinuousScroll;
        string? originalColor = Environment.GetEnvironmentVariable("DFT_COLOR");
        string? originalBackground = Environment.GetEnvironmentVariable("DFT_BACKGROUND");
        string? originalSyntax = Environment.GetEnvironmentVariable("DFT_SYNTAX_HIGHLIGHT");
        string? originalContext = Environment.GetEnvironmentVariable("DFT_CONTEXT");
        string? originalStripCr = Environment.GetEnvironmentVariable("DFT_STRIP_CR");
        string? originalWidth = Environment.GetEnvironmentVariable("DFT_WIDTH");
        string? originalWslEnv = Environment.GetEnvironmentVariable("WSLENV");
        try
        {
            AppSettings.DiffDisplayAppearance.Value = DiffDisplayAppearance.Patch;
            AppSettings.AutomaticContinuousScroll = false;
            FileViewer viewer = new();
            FileViewer.TestAccessor accessor = viewer.GetTestAccessor();
            int reloadRequests = 0;
            viewer.ExtraDiffArgumentsChanged += (_, _) => reloadRequests++;

            accessor.ShowGitWordColoringMenuItem.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

            AppSettings.DiffDisplayAppearance.Value.Should().Be(DiffDisplayAppearance.GitWordDiff);
            viewer.GetExtraDiffArguments().ToString().Should().Contain("--word-diff=color");

            accessor.TreatAllFilesAsTextMenuItem.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

            viewer.GetExtraDiffArguments().ToString().Should().Contain("--text");
            viewer.GetExtraGrepArguments().ToString().Should().Contain("--text");

            (GitExtensions.Extensibility.ArgumentString args, string extraCacheKey) = viewer.GetDifftasticArguments();
            args.ToString().Should().Contain("--tool=difftastic").And.Contain("--text");
            extraCacheKey.Should().Contain(";DFT_COLOR=always").And.Contain(";DFT_BACKGROUND=light");
            Environment.GetEnvironmentVariable("DFT_WIDTH").Should().Be("88");

            accessor.AutomaticContinuousScrollMenuItem.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

            AppSettings.AutomaticContinuousScroll.Should().BeTrue();
            reloadRequests.Should().Be(2);
            viewer.FindControl<MenuItem>("showPatchToolStripMenuItem").Should().NotBeNull();
            viewer.FindControl<MenuItem>("showDifftasticToolStripMenuItem").Should().NotBeNull();
            viewer.FindControl<Button>("settingsButton").Should().NotBeNull();
        }
        finally
        {
            AppSettings.DiffDisplayAppearance.Value = originalAppearance;
            AppSettings.AutomaticContinuousScroll = originalContinuousScroll;
            Environment.SetEnvironmentVariable("DFT_COLOR", originalColor);
            Environment.SetEnvironmentVariable("DFT_BACKGROUND", originalBackground);
            Environment.SetEnvironmentVariable("DFT_SYNTAX_HIGHLIGHT", originalSyntax);
            Environment.SetEnvironmentVariable("DFT_CONTEXT", originalContext);
            Environment.SetEnvironmentVariable("DFT_STRIP_CR", originalStripCr);
            Environment.SetEnvironmentVariable("DFT_WIDTH", originalWidth);
            Environment.SetEnvironmentVariable("WSLENV", originalWslEnv);
        }
    }

    [AvaloniaTest]
    public async Task FileViewer_should_dispatch_configured_find_next_hotkey()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        IHotkeySettingsLoader loader = Substitute.For<IHotkeySettingsLoader>();
        loader.LoadHotkeys(FileViewer.HotkeySettingsName).Returns(
        [
            new HotkeyCommand((int)FileViewer.Command.FindNextOrOpenWithDifftool, nameof(FileViewer.Command.FindNextOrOpenWithDifftool))
            {
                KeyData = WinFormsShims.Keys.F3,
            },
        ]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(loader);
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(commands);

        FileViewer viewer = new() { UICommandsSource = source };
        await viewer.ViewTextAsync("sample.txt", "first target last");
        FindAndReplaceForm form = viewer.GetTestAccessor().FindAndReplaceForm;
        FindAndReplaceForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SetEditor(viewer.TextEditor);
        accessor.TxtLookFor.Text = "target";

        viewer.ProcessHotkey(WinFormsShims.Keys.F3).Should().BeTrue();
        Dispatcher.UIThread.RunJobs();
        await ThreadHelper.JoinPendingOperationsAsync(CancellationToken.None);

        viewer.TextEditor.SelectedText.Should().Be("target");
        form.Close();
    }

    [AvaloniaTest]
    public void FindAndReplaceForm_should_preserve_original_layout_and_translation_keys()
    {
        FindAndReplaceForm form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.ReplaceMode = false;

        form.Width.Should().Be(419);
        form.Height.Should().Be(169);
        form.FindControl<TextBox>("txtLookFor").Should().NotBeNull();
        form.FindControl<Button>("btnFindNext")!.Width.Should().Be(100);
        form.FindControl<Button>("btnHighlightAll")!.IsVisible.Should().BeTrue();
        form.FindControl<Button>("btnReplace")!.IsVisible.Should().BeFalse();
        translation.Received(1).AddTranslationItem(nameof(FindAndReplaceForm), "label1", "Text", "Fi&nd what:");
        translation.Received(1).AddTranslationItem(nameof(FindAndReplaceForm), "btnFindNext", "Text", "&Find next");
        translation.Received(1).AddTranslationItem(nameof(FindAndReplaceForm), "_textNotFoundString", "Text", "Text not found");

        form.Close();
    }

    [AvaloniaTest]
    public void FormGoToLine_should_apply_bounds_and_preserve_translation_keys()
    {
        FormGoToLine form = new();
        ITranslation translation = Substitute.For<ITranslation>();
        form.AddTranslationItems(translation);

        form.SetMaxLineNumber(42);
        NumericUpDown input = form.FindControl<NumericUpDown>("_NO_TRANSLATE_LineNumberUpDown")!;
        input.Value = 17;

        form.GetLineNumber().Should().Be(17);
        input.Maximum.Should().Be(42);
        form.FindControl<TextBlock>("lineLabel")!.Text.Should().Be("Line number (1 - 42):");
        translation.Received(1).AddTranslationItem(nameof(FormGoToLine), "$this", "Text", "Go to line");
        translation.Received(1).AddTranslationItem(nameof(FormGoToLine), "lineLabel", "Text", "Line number");
        translation.Received(1).AddTranslationItem(nameof(FormGoToLine), "okBtn", "Text", "OK");

        form.Close();
    }

    [AvaloniaTest]
    public void FileViewer_should_expose_the_functional_context_actions()
    {
        FileViewer viewer = new();
        ITranslation translation = Substitute.For<ITranslation>();

        viewer.AddTranslationItems(translation);

        viewer.FindControl<MenuItem>("copyToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("stageSelectedLinesToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("unstageSelectedLinesToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("resetSelectedLinesToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("copyPatchToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("copyNewVersionToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("copyOldVersionToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("increaseNumberOfLinesToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("decreaseNumberOfLinesToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("showEntireFileToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("ignoreWhitespaceAtEolToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("ignoreWhitespaceChangesToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("ignoreAllWhitespaceChangesToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("findToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("goToLineToolStripMenuItem").Should().NotBeNull();
        viewer.FindControl<MenuItem>("replaceToolStripMenuItem")!.IsVisible.Should().BeFalse();
        translation.Received(1).AddTranslationItem(nameof(FileViewer), "copyToolStripMenuItem", "Text", "&Copy");
        translation.Received(1).AddTranslationItem(nameof(FileViewer), "findToolStripMenuItem", "Text", "&Find...");
        translation.Received(1).AddTranslationItem(nameof(FileViewer), "goToLineToolStripMenuItem", "Text", "&Go to line");
    }
}
