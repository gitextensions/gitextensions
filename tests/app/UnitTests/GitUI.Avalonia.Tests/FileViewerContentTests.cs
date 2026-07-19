using System.ComponentModel.Design;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
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
            int reloadRequests = 0;
            viewer.ExtraDiffArgumentsChanged += (_, _) => reloadRequests++;

            await viewer.ViewChangesAsync(item, CancellationToken.None);

            accessor.ViewMode.Should().Be(ViewMode.Diff);
            viewer.TextEditor.SyntaxHighlighting.Should().NotBeNull();
            viewer.TextEditor.SyntaxHighlighting!.Name.Should().Be("C#");
            accessor.HasDiffHighlighting.Should().BeTrue();
            viewer.TextEditor.ShowLineNumbers.Should().BeFalse();
            accessor.ShowSyntaxHighlightingMenuItem.IsVisible.Should().BeTrue();
            accessor.ShowSyntaxHighlightingMenuItem.IsChecked.Should().BeTrue();
            accessor.ShowSyntaxHighlightingButton.Classes.Should().Contain("checked");
            accessor.VRulerPosition.Should().Be(88);
            viewer.TextEditor.Options.ShowColumnRulers.Should().BeTrue();

            accessor.ShowNonprintingCharactersMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            viewer.TextEditor.Options.ShowSpaces.Should().BeTrue();
            viewer.TextEditor.Options.ShowTabs.Should().BeTrue();
            viewer.TextEditor.Options.ShowEndOfLine.Should().BeTrue();
            viewer.TextEditor.Options.EndOfLineCRLFGlyph.Should().Be("\\r\\n");
            accessor.ShowNonprintingCharactersButton.Classes.Should().Contain("checked");
            AppSettings.ShowNonPrintingChars.Value.Should().BeTrue();

            accessor.ShowSyntaxHighlightingMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            viewer.TextEditor.SyntaxHighlighting.Should().BeNull();
            accessor.HasDiffHighlighting.Should().BeTrue();
            AppSettings.ShowSyntaxHighlightingInDiff.Value.Should().BeFalse();
            reloadRequests.Should().Be(1);

            accessor.ShowSyntaxHighlightingMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            viewer.TextEditor.SyntaxHighlighting.Should().NotBeNull();
            accessor.HasDiffHighlighting.Should().BeTrue();
            reloadRequests.Should().Be(2);
            accessor.FileViewerToolbar.IsVisible = true;
            await CaptureModeIfRequestedAsync(viewer, "DisplayOptions");

            viewer.ShowLineNumbers = false;
            await viewer.ViewTextAsync("Sample.cs", "line one\nline two\n");
            viewer.TextEditor.ShowLineNumbers.Should().BeFalse();
            viewer.ShowLineNumbers = null;
            viewer.TextEditor.ShowLineNumbers.Should().BeTrue();
            accessor.ShowSyntaxHighlightingButton.IsVisible.Should().BeFalse();
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

        await viewer.ViewGitItemAsync(item, revision);

        accessor.ViewMode.Should().Be(ViewMode.Image);
        accessor.PictureBox.IsVisible.Should().BeTrue();
        viewer.TextEditor.IsVisible.Should().BeFalse();
        accessor.ImagePreview.Source.Should().NotBeNull();

        await CaptureModeIfRequestedAsync(viewer, "Image");

        string iconPath = Path.Combine(GetRepositoryRoot(), "src", "app", "GitUI", "Resources", "Icons", "difftastic.ico");
        await viewer.ViewFileAsync(iconPath);

        accessor.ViewMode.Should().Be(ViewMode.Image);
        accessor.ImagePreview.Source.Should().NotBeNull();
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
    public async Task Encoding_selector_should_use_the_shared_available_encodings_and_request_a_reload()
    {
        FileViewer viewer = CreateViewer();
        FileViewer.TestAccessor accessor = viewer.GetTestAccessor();
        Encoding initial = Encoding.UTF8;
        Encoding replacement = Encoding.Unicode;
        int reloadRequests = 0;
        viewer.ExtraDiffArgumentsChanged += (_, _) => reloadRequests++;

        viewer.Encoding = initial;
        accessor.EncodingToolStripComboBox.SelectedItem.Should().Be(initial.EncodingName);
        accessor.EncodingToolStripComboBox.SelectedItem = replacement.EncodingName;

        viewer.Encoding.Should().Be(replacement);
        reloadRequests.Should().Be(1);

        accessor.FileViewerToolbar.IsVisible = true;
        await CaptureModeIfRequestedAsync(viewer, "EncodingToolbar");

        File.WriteAllBytes(
            Path.Combine(_workingDirectory, "legacy.txt"),
            Encoding.Latin1.GetBytes("café!"));
        viewer.Encoding = Encoding.Latin1;
        await viewer.ViewFileAsync("legacy.txt");

        viewer.TextEditor.Text.Should().Be("café!");
        viewer.FilePreamble.Should().BeEmpty();
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
    public void FileViewer_should_expose_portable_contract_and_navigate_change_blocks()
    {
        FileViewer viewer = new();
        IFileViewer contract = viewer;
        viewer.ViewPatch("diff --git a/a.txt b/a.txt\n--- a/a.txt\n+++ b/a.txt\n@@ -1,4 +1,4 @@\n-old\n+new\n context\n@@ -8,2 +8,2 @@\n-before\n+after\n");

        contract.GetText().Should().Contain("+after");
        viewer.GoToFirstChange();
        int firstBlock = viewer.TextEditor.TextArea.Caret.Line;
        viewer.GoToNextChange();

        firstBlock.Should().Be(5);
        viewer.TextEditor.TextArea.Caret.Line.Should().Be(9);
        viewer.GoToPreviousChange();
        viewer.TextEditor.TextArea.Caret.Line.Should().Be(5);
        contract.TotalNumberOfLines.Should().BeGreaterThan(9);
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

    private static async Task CaptureModeIfRequestedAsync(FileViewer viewer, string mode)
    {
        if (Environment.GetEnvironmentVariable("GITEXT_CAPTURE_FILE_VIEWER_MODES") != "1")
        {
            return;
        }

        string outputDirectory = Path.Combine(GetRepositoryRoot(), "eng", "avalonia", "parity-shots", "FileViewerModes");
        Directory.CreateDirectory(outputDirectory);
        Window window = new()
        {
            Width = 640,
            Height = 360,
            Content = viewer,
        };
        window.Show();
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
            window.Close();
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
