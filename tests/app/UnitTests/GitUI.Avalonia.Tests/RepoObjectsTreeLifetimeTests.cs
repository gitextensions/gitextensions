using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.LeftPanel;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using SkiaSharp;
using ColorHelper = GitExtUtils.GitUI.Theming.ColorHelper;

namespace GitExtensionsTests;

/// <summary>
///  Checks command-source ownership when the portable left panel replaces repository roots.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RepoObjectsTreeLifetimeTests
{
    private JoinableTaskContext? _previousContext;
    private JoinableTaskContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        _context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
        ThreadHelper.JoinableTaskContext = _context;
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            ThreadHelper.JoinableTaskContext = _previousContext!;
            _context.Dispose();
        }
    }

    /// <summary>
    ///  Retires replaced roots without changing their current-source behavior or painted state.
    /// </summary>
    [AvaloniaTest]
    [TestCase("light")]
    [TestCase("dark")]
    [TestCase("custom")]
    public void Repeated_ref_refresh_should_release_retired_tree_subscriptions_and_keep_the_actual_frame(string theme)
    {
        using TreeThemeScope themeScope = new(theme);
        bool showBranches = AppSettings.RepoObjectsTreeShowBranches;
        AppSettings.RepoObjectsTreeShowBranches = true;
        IGitModule module = Substitute.For<IGitModule>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.BrowseRepo.Returns(Substitute.For<IBrowseRepo>());
        TrackingCommandsSource source = new(commands);
        RepoObjectsTree? control = null;
        Window? window = null;
        bool rootsDisposed = false;
        const int refreshCount = 12;
        try
        {
            control = new RepoObjectsTree { UICommandsSource = source };
            ICheckRefs refsSource = Substitute.For<ICheckRefs>();
            refsSource.Contains(Arg.Any<ObjectId>()).Returns(true);
            IRevisionGridInfo gridInfo = Substitute.For<IRevisionGridInfo>();
            gridInfo.GetCurrentBranch().Returns("main");
            control.Initialize(aheadBehindDataProvider: null, _ => { }, refsSource, gridInfo);
            IGitRef[] refs =
            [
                new GitRef(module, ObjectId.Parse(new string('1', ObjectId.Sha1CharCount)), "refs/heads/main"),
                new GitRef(module, ObjectId.Parse(new string('2', ObjectId.Sha1CharCount)), "refs/remotes/origin/main", "origin"),
                new GitRef(module, ObjectId.Parse(new string('3', ObjectId.Sha1CharCount)), "refs/tags/v1"),
            ];
            control.SetRefs(refs, [], "main");
            TreeView tree = control.GetTestAccessor().TreeView;
            window = new Window { Width = 300, Height = 350, Content = control };
            window.Show();
            tree.Focus().Should().BeTrue();
            int initialSubscriptions = source.SubscriptionCount;
            Tree[] initialRoots = CurrentRoots(control);
            source.TreeSubscribers.Should().HaveCount(initialRoots.Length);
            using SKBitmap before = Capture(window, theme, "before", source, refreshCount: 0);

            for (int iteration = 0; iteration < refreshCount; iteration++)
            {
                Tree[] previous = CurrentRoots(control);
                control.SetRefs(refs, [], "main");
                Tree[] current = CurrentRoots(control);
                source.SubscriptionCount.Should().Be(initialSubscriptions,
                    "replacing roots must release their source event subscriptions rather than retaining another generation");
                source.TreeSubscribers.Should().HaveCount(current.Length);
                foreach (Tree live in current)
                {
                    source.TreeSubscribers.Should().Contain(subscriber => ReferenceEquals(subscriber, live));
                }

                foreach (Tree retired in previous.Where(old => !current.Any(live => ReferenceEquals(old, live))))
                {
                    source.TreeSubscribers.Should().NotContain(subscriber => ReferenceEquals(subscriber, retired));
                }
            }

            using SKBitmap after = Capture(window, theme, "after", source, refreshCount);
            after.Width.Should().Be(before.Width);
            after.Height.Should().Be(before.Height);
            after.Pixels.Should().Equal(before.Pixels,
                "subscription ownership is a lifetime repair, not a change to the actual same-theme selected tree presentation");
            tree.SelectedItem.Should().NotBeNull();
            Tree root = CurrentRoots(control).OfType<LocalBranchTree>().Single();
            root.SetUICommandsSource(source);
            source.SubscriptionCount.Should().Be(initialSubscriptions, "assigning the same source is idempotent");

            TrackingCommandsSource replacement = new(commands);
            root.SetUICommandsSource(replacement);
            source.SubscriptionCount.Should().Be(initialSubscriptions - 1);
            source.TreeSubscribers.Should().NotContain(subscriber => ReferenceEquals(subscriber, root));
            replacement.TreeSubscribers.Should().ContainSingle().Which.Should().BeSameAs(root);
            FieldInfo firstReload = typeof(Tree).GetField("_firstReloadNodesSinceModuleChanged", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new AssertionException("The source-shaped first-reload state must exist.");
            firstReload.SetValue(root, false);
            firstReload.GetValue(root).Should().Be(false);
            replacement.RaiseChanged();
            firstReload.GetValue(root).Should().Be(true,
                "the replacement source only reaches the root's own named module-change handler");
            root.SetUICommandsSource(source);
            replacement.SubscriptionCount.Should().Be(0);
            source.SubscriptionCount.Should().Be(initialSubscriptions);

            source.RaiseChanged();
            tree.SelectedItems.Should().NotBeNull();
            tree.SelectedItems!.Cast<object>().Should().BeEmpty("the current command source must retain the original module-change selection reset");
            foreach (Tree live in CurrentRoots(control))
            {
                live.Dispose();
            }

            rootsDisposed = true;
            source.TreeSubscribers.Should().BeEmpty("all current root owners have released their command-source subscriptions");
            source.SubscriptionCount.Should().Be(initialSubscriptions - initialRoots.Length,
                "only the unchanged owning control's module-change handler remains");
        }
        finally
        {
            window?.Close();
            if (control is not null && !rootsDisposed)
            {
                foreach (Tree root in CurrentRoots(control))
                {
                    root.Dispose();
                }
            }

            Dispatcher.UIThread.RunJobs();
            AppSettings.RepoObjectsTreeShowBranches = showBranches;
        }
    }

    private static Tree[] CurrentRoots(RepoObjectsTree owner)
    {
        FieldInfo field = typeof(RepoObjectsTree).GetField("_rootNodes", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new AssertionException("The actual repository-root collection must exist.");
        return field.GetValue(owner) is IEnumerable<Tree> roots
            ? [.. roots]
            : throw new AssertionException("The actual repository-root collection must retain its source-shaped tree owners.");
    }

    private static SKBitmap Capture(Window window, string theme, string phase, TrackingCommandsSource source, int refreshCount)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        using WriteableBitmap frame = window.CaptureRenderedFrame()
            ?? throw new AssertionException("The actual repository tree window must produce a headless frame.");
        string? evidence = Environment.GetEnvironmentVariable("GITEXT_TREE_LIFETIME_EVIDENCE");
        if (!string.IsNullOrWhiteSpace(evidence))
        {
            string directory = Path.Combine(evidence, TestContext.CurrentContext.Test.ID);
            Directory.CreateDirectory(directory);
            string name = $"{theme}-{phase}-{Guid.NewGuid():N}";
            frame.Save(Path.Combine(directory, name + ".png"), PngBitmapEncoderOptions.Default);
            File.WriteAllText(Path.Combine(directory, name + ".json"), JsonSerializer.Serialize(new
            {
                test = TestContext.CurrentContext.Test.FullName,
                theme,
                phase,
                captureMethod = "actualAvaloniaHeadlessRepoObjectsTreeWindow",
                refreshCount,
                subscriptionCount = source.SubscriptionCount,
                treeSubscriptions = source.TreeSubscribers.Count,
                frame.PixelSize,
                scope = "Same-theme before/after input and rendering invariance with bounded event subscriptions; no garbage-collection or native pixel-parity claim.",
            }, new JsonSerializerOptions { WriteIndented = true }));
        }

        using MemoryStream stream = new();
        frame.Save(stream, PngBitmapEncoderOptions.Default);
        return SKBitmap.Decode(stream.ToArray()) ?? throw new AssertionException("The actual tree frame must decode.");
    }

    private sealed class TrackingCommandsSource(IGitUICommands commands) : IGitUICommandsSource
    {
        private EventHandler<GitUICommandsChangedEventArgs>? _changed;

        public event EventHandler<GitUICommandsChangedEventArgs> UICommandsChanged
        {
            add => _changed += value;
            remove => _changed -= value;
        }

        public IGitUICommands UICommands => commands;

        public int SubscriptionCount => _changed?.GetInvocationList().Length ?? 0;

        public IReadOnlyList<Tree> TreeSubscribers => _changed?.GetInvocationList()
            .Select(handler => handler.Target).OfType<Tree>().ToArray() ?? [];

        public void RaiseChanged() => _changed?.Invoke(this, new GitUICommandsChangedEventArgs(commands));
    }

    private sealed class TreeThemeScope : IDisposable
    {
        private static readonly PropertyInfo ModuleSettings = typeof(ThemeModule).GetProperty(nameof(ThemeModule.Settings))
            ?? throw new InvalidOperationException("The source-shaped theme settings must exist.");
        private static readonly PropertyInfo HelperSettings = typeof(ColorHelper).GetProperty(nameof(ColorHelper.ThemeSettings))
            ?? throw new InvalidOperationException("The shared color settings must exist.");
        private readonly Application _application = Application.Current
            ?? throw new InvalidOperationException("The headless application must exist.");
        private readonly ThemeSettings _original = ThemeModule.Settings;
        private readonly ThemeSettings _originalHelper = HelperSettings.GetValue(null) as ThemeSettings
            ?? throw new InvalidOperationException("The shared color settings must be initialized.");
        private readonly ThemeVariant? _originalVariant;

        public TreeThemeScope(string name)
        {
            _originalVariant = _application.RequestedThemeVariant;
            Theme theme = LoadTheme(name);
            _application.RequestedThemeVariant = name == "light" ? ThemeVariant.Light : ThemeVariant.Dark;
            ThemeSettings settings = new(theme, Theme.CreateDefaultTheme(), [], useSystemVisualStyle: name == "light");
            ModuleSettings.SetValue(null, settings);
            ColorHelper.ThemeSettings = settings;
            AvaloniaThemeResources.Apply(_application, settings);
        }

        public void Dispose()
        {
            _application.RequestedThemeVariant = _originalVariant;
            ModuleSettings.SetValue(null, _original);
            ColorHelper.ThemeSettings = _originalHelper;
            AvaloniaThemeResources.Apply(_application, _original);
        }

        private static Theme LoadTheme(string name, [CallerFilePath] string source = "")
        {
            if (name == "light")
            {
                return Theme.CreateDefaultTheme();
            }

            DirectoryInfo? directory = new FileInfo(source).Directory;
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Directory.Packages.props")))
            {
                directory = directory.Parent;
            }

            string repository = directory?.FullName ?? throw new AssertionException("The tracked theme source must be available.");
            string themes = Path.Combine(repository, "src", "app", "GitUI", "Themes");
            string path = name == "custom"
                ? Path.Combine(repository, "eng", "tools", "WinFormsParityCapture", "Themes", "parity-custom.css")
                : Path.Combine(themes, "dark.css");
            IThemeCssUrlResolver resolver = Substitute.For<IThemeCssUrlResolver>();
            resolver.ResolveCssUrl(Arg.Any<string>()).Returns(call => Path.Combine(themes, call.Arg<string>()));
            return new ThemeLoader(resolver, new ThemeFileReader()).LoadTheme(path, new ThemeId(name), []);
        }
    }
}
