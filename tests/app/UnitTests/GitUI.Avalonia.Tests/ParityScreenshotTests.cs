using System.ComponentModel.Design;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaEdit;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Gpg;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.Blame;
using GitUI.CommandsDialogs;
using GitUI.CommitInfo;
using GitUI.Editor;
using GitUI.Help;
using GitUI.HelperDialogs;
using GitUI.LeftPanel;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

/// <summary>
///  Discovers every ported AXAML view and, on request, captures it with headless Skia in
///  both supported theme variants. The PNG output is a local review artifact rather than
///  a golden test input, so normal test runs validate the inventory without rewriting it.
/// </summary>
[TestFixture]
public sealed partial class ParityScreenshotTests
{
    private const string CaptureCategory = "VisualParityCapture";
    private const string CaptureEnvironmentVariable = "GITEXT_CAPTURE_PARITY_SHOTS";
    private const string AxamlExtension = ".axaml";
    private const string AppSourcePath = "src/App.cs";
    private const string FeatureBranchName = "feature/visual-parity";
    private const string MainBranchName = "main";
    private const string RemoteName = "origin";
    private const string InitialCommitSubject = "Add representative repository content";
    private const string HeadCommitSubject = "Establish the Avalonia application shell";

    [GeneratedRegex("x:Class=\"(?<className>[^\"]+)\"")]
    private static partial Regex ClassNameRegex { get; }

    [Test]
    public void Ported_view_inventory_should_resolve_every_AXAML_class()
    {
        IReadOnlyList<ViewDescriptor> views = GetViewDescriptors();

        views.Should().NotBeEmpty();
        views.Select(view => view.ViewType).Should().OnlyHaveUniqueItems();
        views.Should().OnlyContain(view => typeof(Control).IsAssignableFrom(view.ViewType));
        views.Where(view => view.ViewType.GetConstructor(Type.EmptyTypes) is null).Should().BeEmpty();
    }

    [AvaloniaTest]
    [Category(CaptureCategory)]
    public async Task Capture_every_ported_view_in_light_and_dark()
    {
        if (Environment.GetEnvironmentVariable(CaptureEnvironmentVariable) != "1")
        {
            return;
        }

        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        WinFormsShims.ShimHost.MessageBoxHost = new StubMessageBoxHost();

        string outputDirectory = GetOutputDirectory();
        foreach (string themeName in new[] { "Light", "Dark" })
        {
            string themeDirectory = Path.Combine(outputDirectory, themeName);
            if (Directory.Exists(themeDirectory))
            {
                TestDirectory.Delete(themeDirectory);
            }
        }

        Directory.CreateDirectory(outputDirectory);
        File.Delete(Path.Combine(outputDirectory, "manifest.json"));
        IReadOnlyList<ViewDescriptor> views = GetViewDescriptors();
        List<ManifestEntry> manifest = [];

        using CaptureContext context = new();
        (string Name, ThemeVariant Variant)[] themes =
        [
            ("Light", ThemeVariant.Light),
            ("Dark", ThemeVariant.Dark),
        ];
        (string Name, double Factor)[] scales =
        [
            ("100", 1),
            ("125", 1.25),
            ("150", 1.5),
            ("200", 2),
        ];

        foreach ((string themeName, ThemeVariant themeVariant) in themes)
        {
            foreach ((string scaleName, double scaleFactor) in scales)
            {
                foreach (ViewDescriptor view in views)
                {
                    ManifestEntry entry = await CaptureViewAsync(
                        context,
                        view,
                        themeName,
                        themeVariant,
                        scaleName,
                        scaleFactor,
                        outputDirectory);
                    manifest.Add(entry);
                }
            }
        }

        string manifestPath = Path.Combine(outputDirectory, "manifest.json");
        File.WriteAllText(
            manifestPath,
            JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));

        manifest.Should().HaveCount(views.Count * themes.Length * scales.Length);
        manifest.Should().OnlyContain(entry => File.Exists(Path.Combine(outputDirectory, entry.File)));
        await TestContext.Progress.WriteLineAsync($"Captured {manifest.Count} parity screenshots in {outputDirectory}");
    }

    private static async Task<ManifestEntry> CaptureViewAsync(
        CaptureContext context,
        ViewDescriptor descriptor,
        string themeName,
        ThemeVariant themeVariant,
        string scaleName,
        double scaleFactor,
        string outputDirectory)
    {
        Control view = CreateView(context, descriptor.ViewType);
        (double width, double height) = GetCaptureSize(descriptor.ViewType);
        Window window;
        bool isWindow = view is Window;
        if (view is Window viewWindow)
        {
            window = viewWindow;
            window.Width = width;
            window.Height = height;
            window.SizeToContent = SizeToContent.Manual;
        }
        else
        {
            window = new Window
            {
                Title = descriptor.ClassName,
                Width = width,
                Height = height,
                SizeToContent = SizeToContent.Manual,
                Content = view,
            };
        }

        window.RequestedThemeVariant = themeVariant;
        try
        {
            PrepareView(view, context);
            window.Show();
            window.SetRenderScaling(scaleFactor);
            if (!isWindow)
            {
                await SeedStandaloneControlAsync(view, context);
            }

            await WaitForAsyncViewsAsync(view);
            Dispatcher.UIThread.RunJobs();
            if (view is FormBrowse formBrowse)
            {
                formBrowse.commandsToolStripMenuItem.IsSubMenuOpen = true;
                Dispatcher.UIThread.RunJobs();
            }
            else if (view is FileStatusList fileStatusList)
            {
                FileStatusList.TestAccessor accessor = fileStatusList.GetTestAccessor();
                accessor.UpdateContextMenu();
                accessor.ContextMenu.Open(accessor.List);
                Dispatcher.UIThread.RunJobs();
            }

            WriteableBitmap? frame = window.CaptureRenderedFrame();
            frame.Should().NotBeNull($"{descriptor.ClassName} should render with headless Skia");

            string relativeFile = Path.Combine(themeName, scaleName, descriptor.RelativePathWithoutExtension + ".png");
            string outputPath = Path.Combine(outputDirectory, relativeFile);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            using FileStream stream = File.Create(outputPath);
            frame!.Save(stream, PngBitmapEncoderOptions.Default);

            return new ManifestEntry(
                descriptor.RelativePath,
                descriptor.ClassName,
                themeName,
                scaleName,
                isWindow ? "Window" : "UserControl",
                (int)frame.PixelSize.Width,
                (int)frame.PixelSize.Height,
                relativeFile.Replace(Path.DirectorySeparatorChar, '/'));
        }
        finally
        {
            window.Close();
            if (!ReferenceEquals(window, view) && view is IDisposable disposableView)
            {
                disposableView.Dispose();
            }

            if (window is IDisposable disposableWindow)
            {
                disposableWindow.Dispose();
            }
        }
    }

    private static Control CreateView(CaptureContext context, Type viewType)
    {
        if (viewType == typeof(FormBrowse))
        {
            return new FormBrowse(context.Commands);
        }

        if (viewType == typeof(FormAddFiles))
        {
            return new FormAddFiles(context.Commands);
        }

        if (viewType == typeof(FormCheckoutBranch))
        {
            return new FormCheckoutBranch(context.Commands, FeatureBranchName, remote: false);
        }

        if (viewType == typeof(FormClone))
        {
            return new FormClone(
                context.Commands,
                "https://github.com/gitextensions/gitextensions.git",
                openedFromProtocolHandler: false,
                gitModuleChanged: null);
        }

        if (viewType == typeof(FormCommit))
        {
            return new FormCommit(context.Commands, commitMessage: "Describe the representative Avalonia changes");
        }

        if (viewType == typeof(FormCreateBranch))
        {
            return new FormCreateBranch(context.Commands, context.HeadRevision.ObjectId, "feature/");
        }

        if (viewType == typeof(FormCreateTag))
        {
            return new FormCreateTag(context.Commands, context.HeadRevision.ObjectId);
        }

        if (viewType == typeof(FormDeleteBranch))
        {
            return new FormDeleteBranch(context.Commands, [FeatureBranchName]);
        }

        if (viewType == typeof(FormDeleteTag))
        {
            return new FormDeleteTag(context.Commands, "v1.0");
        }

        if (viewType == typeof(FormFileHistory))
        {
            return new FormFileHistory(context.Commands, AppSourcePath, context.HeadRevision);
        }

        if (viewType == typeof(FormCommitDiff))
        {
            return new FormCommitDiff(context.Commands, context.HeadRevision.ObjectId);
        }

        if (viewType == typeof(FormViewPatch))
        {
            FormViewPatch form = new(context.Commands);
            form.GetTestAccessor().LoadPatchText(context.SamplePatch);
            return form;
        }

        if (viewType == typeof(FormInit))
        {
            return new FormInit(context.Commands, context.WorkingDirectory, gitModuleChanged: null);
        }

        if (viewType == typeof(FormMergeBranch))
        {
            return new FormMergeBranch(context.Commands, FeatureBranchName);
        }

        if (viewType == typeof(FormPull))
        {
            return new FormPull(context.Commands, MainBranchName, RemoteName, GitPullAction.Default);
        }

        if (viewType == typeof(FormPush))
        {
            return new FormPush(context.Commands, MainBranchName);
        }

        if (viewType == typeof(FormRebase))
        {
            return new FormRebase(context.Commands, FeatureBranchName);
        }

        if (viewType == typeof(FormRemotes))
        {
            return new FormRemotes(context.Commands) { PreselectRemoteOnLoad = RemoteName };
        }

        if (viewType == typeof(FormRenameBranch))
        {
            return new FormRenameBranch(context.Commands, FeatureBranchName);
        }

        if (viewType == typeof(FormResolveConflicts))
        {
            return new FormResolveConflicts(context.Commands);
        }

        if (viewType == typeof(FormStash))
        {
            return new FormStash(context.Commands);
        }

        if (viewType == typeof(FormChooseCommit))
        {
            return new FormChooseCommit(context.Commands, context.HeadRevision.ObjectId.ToString());
        }

        if (viewType == typeof(FormSelectMultipleBranches))
        {
            return new FormSelectMultipleBranches(context.Refs.Where(gitRef => gitRef.IsHead).ToArray());
        }

        if (viewType == typeof(FormResetChanges))
        {
            return new FormResetChanges(
                hasExistingFiles: true,
                hasNewFiles: true,
                "Reset the representative staged and unstaged changes?");
        }

        if (viewType == typeof(FindAndReplaceForm))
        {
            FindAndReplaceForm form = new();
            TextEditor editor = new() { Text = "Find representative text in the diff viewer." };
            FindAndReplaceForm.TestAccessor accessor = form.GetTestAccessor();
            accessor.SetEditor(editor);
            accessor.TxtLookFor.Text = "representative";
            form.ReplaceMode = false;
            return form;
        }

        if (viewType == typeof(FormGoToLine))
        {
            FormGoToLine form = new();
            form.SetMaxLineNumber(120);
            return form;
        }

        return (Control)(Activator.CreateInstance(viewType)
            ?? throw new InvalidOperationException($"Could not construct {viewType.FullName}."));
    }

    private static void PrepareView(Control root, CaptureContext context)
    {
        if (root is FormStatus formStatus)
        {
            FieldInfo processCallback = typeof(FormStatus)
                .GetField("ProcessCallback", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("FormStatus.ProcessCallback could not be found.");
            processCallback.SetValue(formStatus, (Action<FormStatus>)(_ => { }));
        }

        if (root is Window)
        {
            return;
        }

        foreach (GitModuleControl moduleControl in new[] { root }
                     .Concat(root.GetLogicalDescendants().OfType<Control>())
                     .OfType<GitModuleControl>())
        {
            moduleControl.UICommandsSource = context;
        }
    }

    private static async Task SeedStandaloneControlAsync(Control root, CaptureContext context)
    {
        if (root is RevisionGpgInfoControl revisionGpgInfo)
        {
            revisionGpgInfo.DisplayGpgInfo(new GpgInfo(
                CommitStatus.GoodSignature,
                "Good signature from Visual Parity <visual@example.com>\nPrimary key fingerprint: 0123 4567 89AB CDEF",
                TagStatus.OneGood,
                "Good signature on tag v1.0.0\nTagger: Visual Parity <visual@example.com>"));
            return;
        }

        if (root is RevisionDiffControl revisionFileTree)
        {
            IReadOnlyList<GitItemStatus> treeItems = context.Module.GetTreeFiles(context.HeadRevision.ObjectId, full: true);
            revisionFileTree.FileStatusList.SetDiffs(
                [new FileStatusWithDescription(null, context.HeadRevision, "File tree", treeItems)],
                isFileTreeMode: true);
            revisionFileTree.FileStatusList.SelectFileOrFolder(RelativePath.From(AppSourcePath));
            FileStatusItem fileStatusItem = revisionFileTree.FileStatusList.SelectedFileStatusItem!;
            await revisionFileTree.FileViewer.ViewGitItemAsync(fileStatusItem);
            return;
        }

        if (root is CommitDiff commitDiff)
        {
            commitDiff.SetRevision(context.HeadRevision.ObjectId, AppSourcePath);
            await WaitForCommitDiffAsync(commitDiff);
            return;
        }

        Control[] controls =
        [
            root,
            .. root.GetLogicalDescendants().OfType<Control>(),
        ];

        foreach (Control control in controls)
        {
            switch (control)
            {
                case RepoObjectsTree repoObjectsTree:
                    repoObjectsTree.SetRefs(context.Refs);
                    break;

                case CommitInfo commitInfo:
                    commitInfo.Revision = context.HeadRevision;
                    break;

                case CommitSummaryUserControl summary:
                    summary.Revision = context.HeadRevision;
                    break;

                case BranchComboBox branchComboBox:
                    branchComboBox.BranchesToSelect = context.Refs.Where(gitRef => gitRef.IsHead).ToArray();
                    branchComboBox.SetSelectedText(MainBranchName);
                    break;

                case FileViewer fileViewer:
                    fileViewer.ViewPatch(context.SamplePatch);
                    break;

                case FileStatusList fileStatusList:
                    fileStatusList.SetDiffs(context.ChangedFiles);
                    if (ReferenceEquals(fileStatusList, root))
                    {
                        fileStatusList.SetFilter("src|CHANGELOG");
                    }

                    break;

                case RevisionGridControl revisionGrid:
                    revisionGrid.ReloadRevisions(context.Module);
                    break;

                case PatchGrid patchGrid:
                    SeedPatchGrid(patchGrid, context);
                    break;

                case CommitPickerSmallControl commitPicker:
                    SeedCommitPicker(commitPicker, context);
                    break;

                case PasswordInput passwordInput:
                    GetRequiredControl<TextBox>(passwordInput, "Password").Text = "representative-secret";
                    break;

                case RemotesComboboxControl remotes:
                    ComboBox comboBox = GetRequiredControl<ComboBox>(remotes, "comboBoxRemotes");
                    comboBox.ItemsSource = new[] { RemoteName, "upstream" };
                    comboBox.SelectedIndex = 0;
                    break;

                case BlameControl blame:
                    FileViewer? blameFile = blame.FindControl<FileViewer>("BlameFile");
                    blameFile?.ViewPatch(context.SampleBlame);
                    break;
            }
        }
    }

    private static void SeedPatchGrid(PatchGrid patchGrid, CaptureContext context)
    {
        patchGrid.IsManagingRebase = true;
        PatchFile[] patches =
        [
            new PatchFile
            {
                Action = "pick",
                Name = "0001",
                ObjectId = context.HeadRevision.ObjectId,
                Subject = HeadCommitSubject,
                Author = "Avalonia Contributor",
                Date = "2026-07-17",
                IsNext = true,
            },
            new PatchFile
            {
                Action = "reword",
                Name = "0002",
                ObjectId = context.ParentRevision.ObjectId,
                Subject = InitialCommitSubject,
                Author = "Git Extensions Team",
                Date = "2026-07-16",
            },
        ];
        GetRequiredControl<ListBox>(patchGrid, "Patches").ItemsSource = patches;
    }

    private static void SeedCommitPicker(CommitPickerSmallControl commitPicker, CaptureContext context)
    {
        GetRequiredControl<TextBox>(commitPicker, "textBoxCommitHash").Text = context.HeadRevision.ObjectId.ToShortString();
        GetRequiredControl<TextBlock>(commitPicker, "lbCommits").Text = MainBranchName;
    }

    private static T GetRequiredControl<T>(Control root, string name)
        where T : Control
        => root.FindControl<T>(name)
            ?? throw new InvalidOperationException($"{root.GetType().Name}.{name} could not be found.");

    private static async Task WaitForAsyncViewsAsync(Control root)
    {
        CommitDiff? commitDiff = root as CommitDiff ?? root.GetLogicalDescendants().OfType<CommitDiff>().FirstOrDefault();
        if (commitDiff is not null)
        {
            await WaitForCommitDiffAsync(commitDiff);
        }

        TextBlock[] loadingStatuses =
        [
            .. new[] { root }
                .Concat(root.GetLogicalDescendants().OfType<Control>())
                .OfType<RevisionGridControl>()
                .Select(revisionGrid => revisionGrid.FindControl<TextBlock>("lblLoadingStatus"))
                .OfType<TextBlock>(),
        ];
        if (loadingStatuses.Length == 0)
        {
            Dispatcher.UIThread.RunJobs();
            return;
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        while (loadingStatuses.Any(status => !IsLoadingComplete(status.Text))
               && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        loadingStatuses.Should().OnlyContain(status => IsLoadingComplete(status.Text));
    }

    private static async Task WaitForCommitDiffAsync(CommitDiff commitDiff)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (string.IsNullOrEmpty(commitDiff.FileViewer.TextEditor.Text)
               && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        commitDiff.FileStatusList.AllItems.Should().NotBeEmpty();
        commitDiff.FileViewer.TextEditor.Text.Should().NotBeEmpty();
    }

    private static bool IsLoadingComplete(string? status)
        => status?.EndsWith(" revisions", StringComparison.Ordinal) == true;

    private static (double Width, double Height) GetCaptureSize(Type viewType)
    {
        if (viewType == typeof(FormBrowse))
        {
            return (1400, 850);
        }

        if (viewType == typeof(RepoObjectsTree))
        {
            return (360, 560);
        }

        if (viewType == typeof(CommitInfo) || viewType == typeof(CommitSummaryUserControl))
        {
            return (620, 320);
        }

        if (viewType == typeof(BranchComboBox)
            || viewType == typeof(CommitPickerSmallControl)
            || viewType == typeof(FolderBrowserButton)
            || viewType == typeof(GotoUserManualControl)
            || viewType == typeof(PasswordInput)
            || viewType == typeof(RemotesComboboxControl))
        {
            return (620, 100);
        }

        if (viewType == typeof(HelpImageDisplayUserControl))
        {
            return (420, 360);
        }

        if (viewType == typeof(BlameControl))
        {
            return (1000, 650);
        }

        if (viewType == typeof(CommitDiff))
        {
            return (885, 578);
        }

        if (viewType == typeof(FormCommitDiff))
        {
            return (717, 529);
        }

        if (viewType == typeof(FormViewPatch))
        {
            return (689, 501);
        }

        if (viewType == typeof(FindAndReplaceForm))
        {
            return (419, 169);
        }

        if (viewType == typeof(FormGoToLine))
        {
            return (237, 100);
        }

        if (typeof(Window).IsAssignableFrom(viewType))
        {
            return (900, 620);
        }

        return (900, 560);
    }

    private static IReadOnlyList<ViewDescriptor> GetViewDescriptors([CallerFilePath] string thisFilePath = "")
    {
        string repositoryRoot = FindRepositoryRoot(thisFilePath);
        string viewRoot = Path.Combine(repositoryRoot, "src", "app", "GitUI.Avalonia");
        Assembly viewAssembly = typeof(FormBrowse).Assembly;

        return Directory.EnumerateFiles(viewRoot, "*" + AxamlExtension, SearchOption.AllDirectories)
            .Where(path => !Path.GetRelativePath(viewRoot, path).StartsWith("Styles" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .Select(path => CreateDescriptor(viewRoot, path, viewAssembly))
            .ToArray();
    }

    private static ViewDescriptor CreateDescriptor(string viewRoot, string path, Assembly viewAssembly)
    {
        string axaml = File.ReadAllText(path);
        Match classMatch = ClassNameRegex.Match(axaml);
        classMatch.Success.Should().BeTrue($"{path} should declare x:Class");
        string className = classMatch.Groups["className"].Value;
        Type viewType = viewAssembly.GetType(className)
            ?? throw new InvalidOperationException($"The AXAML class {className} could not be resolved.");
        string relativePath = Path.GetRelativePath(viewRoot, path).Replace(Path.DirectorySeparatorChar, '/');
        return new ViewDescriptor(
            relativePath,
            relativePath[..^AxamlExtension.Length],
            className,
            viewType);
    }

    private static string GetOutputDirectory([CallerFilePath] string thisFilePath = "")
        => Path.Combine(FindRepositoryRoot(thisFilePath), "eng", "avalonia", "parity-shots");

    private static string FindRepositoryRoot(string startPath)
    {
        DirectoryInfo? directory = new(Path.GetDirectoryName(startPath)!);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.Avalonia.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"Could not find the repository root from {startPath}.");
    }

    private sealed record ViewDescriptor(
        string RelativePath,
        string RelativePathWithoutExtension,
        string ClassName,
        Type ViewType);

    private sealed record ManifestEntry(
        string View,
        string ClassName,
        string Theme,
        string ScalePercent,
        string Kind,
        int Width,
        int Height,
        string File);

    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
            => buttons switch
            {
                WinFormsShims.MessageBoxButtons.YesNo or WinFormsShims.MessageBoxButtons.YesNoCancel => WinFormsShims.DialogResult.Yes,
                _ => WinFormsShims.DialogResult.OK,
            };
    }

    private sealed class CaptureContext : IDisposable, IGitUICommandsSource
    {
        private readonly ServiceContainer _serviceContainer;
        private readonly string _workingDirectory;

        public CaptureContext()
        {
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

            _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Parity-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_workingDirectory);
            Module = new GitModule(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
            CreateRepository();
            Commands = new GitUICommands(_serviceContainer, Module);

            Refs = Module.GetRefs(RefsFilter.NoFilter);
            ObjectId headId = Module.GetCurrentCheckout();
            ObjectId parentId = Module.RevParse("HEAD~1");
            long headTime = new DateTimeOffset(2026, 7, 17, 10, 30, 0, TimeSpan.Zero).ToUnixTimeSeconds();
            long parentTime = new DateTimeOffset(2026, 7, 16, 16, 45, 0, TimeSpan.Zero).ToUnixTimeSeconds();
            ParentRevision = CreateRevision(parentId, InitialCommitSubject, parentTime, []);
            HeadRevision = CreateRevision(headId, HeadCommitSubject, headTime, [parentId]);
            ChangedFiles = Module.GetAllChangedFilesWithSubmodulesStatus();
        }

        public event EventHandler<GitUICommandsChangedEventArgs>? UICommandsChanged
        {
            add { }
            remove { }
        }

        public GitModule Module { get; }

        public string WorkingDirectory => _workingDirectory;

        public GitUICommands Commands { get; }

        IGitUICommands IGitUICommandsSource.UICommands => Commands;

        public IReadOnlyList<IGitRef> Refs { get; }

        public GitRevision HeadRevision { get; }

        public GitRevision ParentRevision { get; }

        public IReadOnlyList<GitItemStatus> ChangedFiles { get; }

        public string SamplePatch =>
            """
            diff --git a/src/App.cs b/src/App.cs
            index 8c1d2ab..b437e55 100644
            --- a/src/App.cs
            +++ b/src/App.cs
            @@ -1,3 +1,4 @@
             using Avalonia;
            +using GitUI;
             namespace GitExtensions;
            -// Windows-only application shell
            +// Cross-platform application shell

            """;

        public string SampleBlame =>
            """
            1  Avalonia Contributor  public partial class App
            2  Git Extensions Team  {
            3  Avalonia Contributor      public void Initialize()
            4  Git Extensions Team      {
            5  Avalonia Contributor          AvaloniaXamlLoader.Load(this);
            6  Git Extensions Team      }
            7  Avalonia Contributor  }

            """;

        public void Dispose()
        {
            _serviceContainer.Dispose();
            TestDirectory.Delete(_workingDirectory);
        }

        private void CreateRepository()
        {
            Module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", MainBranchName });
            Module.SetSetting("user.name", "Avalonia Contributor");
            Module.SetSetting("user.email", "avalonia@example.com");

            string sourceDirectory = Path.Combine(_workingDirectory, "src");
            Directory.CreateDirectory(sourceDirectory);
            File.WriteAllText(Path.Combine(_workingDirectory, "README.md"), "# Visual parity sample\n");
            Module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "README.md" });
            Module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", '"' + InitialCommitSubject + '"' });

            File.WriteAllText(Path.Combine(sourceDirectory, "App.cs"), "namespace GitExtensions;\n\npublic partial class App;\n");
            Module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", AppSourcePath });
            Module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", '"' + HeadCommitSubject + '"' });
            Module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { FeatureBranchName, "HEAD~1" });
            Module.GitExecutable.RunCommand(new GitArgumentBuilder("tag") { "v1.0", "HEAD~1" });
            Module.GitExecutable.RunCommand(new GitArgumentBuilder("remote") { "add", RemoteName, "https://example.com/gitextensions/parity.git" });
            Module.GitExecutable.RunCommand(new GitArgumentBuilder("update-ref") { $"refs/remotes/{RemoteName}/{MainBranchName}", "HEAD" });

            File.AppendAllText(Path.Combine(sourceDirectory, "App.cs"), "// Unstaged visual parity adjustment\n");
            File.WriteAllText(Path.Combine(_workingDirectory, "CHANGELOG.md"), "Avalonia visual parity harness\n");
            Module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "CHANGELOG.md" });
            File.WriteAllText(Path.Combine(_workingDirectory, "notes.txt"), "untracked review notes\n");
        }

        private GitRevision CreateRevision(ObjectId objectId, string subject, long unixTime, IReadOnlyList<ObjectId> parentIds)
            => new(objectId)
            {
                Author = "Avalonia Contributor",
                AuthorEmail = "avalonia@example.com",
                AuthorUnixTime = unixTime,
                Committer = "Git Extensions Team",
                CommitterEmail = "team@gitextensions.org",
                CommitUnixTime = unixTime,
                Subject = subject,
                Body = subject + "\n\nRepresentative content used by the visual parity screenshot harness.",
                ParentIds = parentIds,
                Refs = Refs.Where(gitRef => gitRef.ObjectId == objectId).ToArray(),
            };
    }
}
