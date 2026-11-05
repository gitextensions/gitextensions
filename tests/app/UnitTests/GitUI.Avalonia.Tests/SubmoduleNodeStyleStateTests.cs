using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Settings;
using GitCommands.Submodules;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Compat;
using GitUI.LeftPanel;
using GitUI.Theming;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

/// <summary>
///  Retains source NodeFont and logical selection when a submodule's native image header is refreshed.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class SubmoduleNodeStyleStateTests
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

    [AvaloniaTest]
    [TestCase("light", false)]
    [TestCase("light", true)]
    [TestCase("dark", false)]
    [TestCase("dark", true)]
    [TestCase("custom", false)]
    [TestCase("custom", true)]
    public void ApplyStyle_and_RefreshDetails_should_retain_node_font_selection_and_current_state(string theme, bool current)
    {
        WinFormsShims.Font previousFont = AppSettings.Font;
        try
        {
            WithIsolatedSettings(() =>
            {
                using ThemeScope themeScope = new(theme);
                AppSettings.RepoObjectsTreeShowSubmodules = true;
                AppSettings.Font = new WinFormsShims.Font("Consolas", 13);
                AvaloniaFontSettings.ApplyAppSettings();
                string topPath = Path.Combine(Path.GetTempPath(), "GitExtensions-submodule-style-model");
                string submodulePath = Path.Combine(topPath, "externals", "foo");
                IGitModule module = Substitute.For<IGitModule>();
                module.WorkingDir.Returns(topPath);
                module.GetTopModule().Returns(module);
                module.SuperprojectModule.Returns((IGitModule?)null);
                SubmoduleInfoResult result = new()
                {
                    Module = module,
                    TopProject = new SubmoduleInfo("repository (main)", topPath, bold: true),
                };
                result.AllSubmodules.Add(new SubmoduleInfo("externals/foo (main)", submodulePath, bold: false));
                RepoObjectsTree control = new();
                Window window = new() { Width = 560, Height = 420, Content = control };
                try
                {
                    control.SetRefs([], []);
                    RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
                    accessor.SetSubmodules(result);
                    TreeViewItem root = accessor.TreeView.Items.Cast<TreeViewItem>().Single(item => item.Tag is SubmoduleTree);
                    SubmoduleNode node = ((SubmoduleTree)root.Tag!).DescendantsAndSelf().OfType<SubmoduleNode>()
                        .Single(item => item.IsCurrent == current);
                    TreeViewItem item = node.TreeViewNode;
                    SubmoduleInfo originalInfo = node.Info;
                    window.Show();
                    RunLayout(window);
                    accessor.TreeView.Focus().Should().BeTrue();
                    accessor.TreeView.SelectedItem = item;
                    node.Select(true);
                    AssertStyle(window, accessor, node, item, selected: true, "Consolas", 13);
                    Record(window, theme, "selected", node);

                    object? previousHeader = item.Header;
                    node.Info = new SubmoduleInfo(originalInfo.Text, originalInfo.Path, originalInfo.Bold)
                    {
                        Detailed = new DetailedSubmoduleInfo { IsDirty = true },
                    };
                    node.RefreshDetails();
                    item.Header.Should().NotBeSameAs(previousHeader, "the status update must exercise the framework's new image header");
                    (node.Info.Detailed?.IsDirty).Should().BeTrue();
                    AssertStyle(window, accessor, node, item, selected: true, "Consolas", 13);
                    Record(window, theme, "status-refreshed", node);

                    AppSettings.Font = new WinFormsShims.Font("Arial", 15);
                    AvaloniaFontSettings.ApplyAppSettings();
                    node.ApplyStyle();
                    AssertStyle(window, accessor, node, item, selected: true, "Arial", 15);
                    Record(window, theme, "font-reapplied", node);

                    node.Select(false);
                    AssertStyle(window, accessor, node, item, selected: false, "Arial", 15);
                    accessor.TreeView.SelectedItem.Should().BeSameAs(item, "logical deselection does not move the highlighted caret");
                    Record(window, theme, "deselected", node);
                }
                finally
                {
                    window.Close();
                    DisposeRoots(control);
                }
            });
        }
        finally
        {
            AvaloniaFontSettings.ApplyAppSettings();
            AppSettings.Font.Name.Should().Be(previousFont.Name);
            AppSettings.Font.Size.Should().Be(previousFont.Size);
        }
    }

    private static void AssertStyle(Window window, RepoObjectsTree.TestAccessor accessor, SubmoduleNode node,
        TreeViewItem originalItem, bool selected, string family, int points)
    {
        RunLayout(window);
        node.TreeViewNode.Should().BeSameAs(originalItem);
        node.TreeViewNode.Tag.Should().BeSameAs(node);
        node.IsSelected.Should().Be(selected);
        accessor.LogicalSelection.Contains(originalItem).Should().Be(selected);
        NativeTreeTextBlock label = HeaderLabel(node);
        label.FontFamily.Name.Should().Be(family);
        label.FontSize.Should().Be(AvaloniaFontSettings.ToDeviceIndependentPixels(points));
        label.FontWeight.Should().Be(node.IsCurrent ? FontWeight.Bold : FontWeight.Normal);
        label.FontStyle.Should().Be(FontStyle.Normal);
        HasUnderline(label).Should().Be(selected);
        label.UsesAmbientFont.Should().Be(!selected && !node.IsCurrent);
        label.Text.Should().Be(node.SubmoduleName + node.BranchText + node.Info.Detailed?.AddedAndRemovedText);
        ToolTip.GetTip(node.TreeViewNode).Should().Be(label.Text);
        ((StackPanel)node.TreeViewNode.Header!).Children.OfType<Image>().Single().Source.Should().NotBeNull();
        label.Bounds.Width.Should().BeGreaterThan(0);
        label.Bounds.Height.Should().BeGreaterThan(0);
    }

    private static void DisposeRoots(RepoObjectsTree control)
    {
        FieldInfo field = typeof(RepoObjectsTree).GetField("_rootNodes", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new AssertionException("The actual tree owners must exist.");
        foreach (Tree root in field.GetValue(control) as IEnumerable<Tree> ?? [])
        {
            root.Dispose();
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static bool HasUnderline(TextBlock label)
        => label.TextDecorations == TextDecorations.Underline;

    private static NativeTreeTextBlock HeaderLabel(SubmoduleNode node)
        => ((StackPanel)node.TreeViewNode.Header!).Children.OfType<NativeTreeTextBlock>().Single();

    private static void Record(Window window, string theme, string state, SubmoduleNode node)
    {
        string? evidence = Environment.GetEnvironmentVariable("GITEXT_ACTION_STATE_EVIDENCE");
        if (string.IsNullOrWhiteSpace(evidence))
        {
            return;
        }

        RunLayout(window);
        using WriteableBitmap captured = window.CaptureRenderedFrame()
            ?? throw new AssertionException("The actual attached submodule tree must render a frame.");
        string directory = Path.Combine(evidence, TestContext.CurrentContext.Test.ID);
        Directory.CreateDirectory(directory);
        string imagePath = Path.Combine(directory, state + ".png");
        string observationPath = Path.Combine(directory, state + ".json");
        File.Exists(imagePath).Should().BeFalse("evidence must not overwrite an earlier capture");
        File.Exists(observationPath).Should().BeFalse("observations must not overwrite an earlier capture");
        captured.Save(imagePath, PngBitmapEncoderOptions.Default);
        NativeTreeTextBlock label = HeaderLabel(node);
        File.WriteAllText(observationPath, JsonSerializer.Serialize(new
        {
            test = TestContext.CurrentContext.Test.FullName,
            testId = TestContext.CurrentContext.Test.ID,
            theme,
            state,
            captureMethod = "actualAvaloniaHeadlessSubmoduleTreeWindow",
            pixelSize = new { captured.PixelSize.Width, captured.PixelSize.Height },
            renderScaling = window.RenderScaling,
            observations = new
            {
                node.IsCurrent,
                node.IsSelected,
                sourceText = node.Info.Text,
                displayText = label.Text,
                fontFamily = label.FontFamily.Name,
                label.FontSize,
                fontWeight = label.FontWeight.ToString(),
                fontStyle = label.FontStyle.ToString(),
                foregroundArgb = label.Foreground is ISolidColorBrush foreground
                    ? foreground.Color.ToUInt32().ToString("X8")
                    : throw new AssertionException("The actual node foreground must resolve to an ARGB color."),
                underline = HasUnderline(label),
                label.UsesAmbientFont,
                labelBounds = new { label.Bounds.X, label.Bounds.Y, label.Bounds.Width, label.Bounds.Height },
                tooltip = ToolTip.GetTip(node.TreeViewNode),
                dirty = node.Info.Detailed?.IsDirty,
            },
            scope = "Actual source-action state regression; not a paired WinForms image or native font/full-parity comparison.",
        }, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RunLayout(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    private static void WithIsolatedSettings(Action action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "GitExtensions.SubmoduleStyleState", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using GitExtSettingsCache cache = new(Path.Combine(directory, "capture.settings"), autoSave: false);
            DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
            AppSettings.UsingContainer(settings, action);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private sealed class ThemeScope : IDisposable
    {
        private static readonly PropertyInfo ModuleSettings = typeof(ThemeModule).GetProperty(nameof(ThemeModule.Settings))
            ?? throw new InvalidOperationException("The source-shaped theme settings must exist.");
        private static readonly PropertyInfo HelperSettings = typeof(ColorHelper).GetProperty(nameof(ColorHelper.ThemeSettings))
            ?? throw new InvalidOperationException("The shared color settings must exist.");
        private readonly Application _application = Application.Current ?? throw new InvalidOperationException("The headless application must exist.");
        private readonly ThemeSettings _original = ThemeModule.Settings;
        private readonly ThemeSettings _originalHelper = HelperSettings.GetValue(null) as ThemeSettings
            ?? throw new InvalidOperationException("The shared color settings must be initialized.");
        private readonly ThemeVariant? _variant;

        public ThemeScope(string theme)
        {
            _variant = _application.RequestedThemeVariant;
            ThemeSettings settings = new(LoadTheme(theme), LoadTheme("invariant"), [], useSystemVisualStyle: theme == "light");
            _application.RequestedThemeVariant = theme == "light" ? ThemeVariant.Light : ThemeVariant.Dark;
            ModuleSettings.SetValue(null, settings);
            ColorHelper.ThemeSettings = settings;
            AvaloniaThemeResources.Apply(_application, settings);
        }

        public void Dispose()
        {
            _application.RequestedThemeVariant = _variant;
            ModuleSettings.SetValue(null, _original);
            ColorHelper.ThemeSettings = _originalHelper;
            AvaloniaThemeResources.Apply(_application, _original);
        }

        private static Theme LoadTheme(string theme, [CallerFilePath] string source = "")
        {
            if (theme == "light")
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
            string path = theme == "custom"
                ? Path.Combine(repository, "eng", "tools", "WinFormsParityCapture", "Themes", "parity-custom.css")
                : Path.Combine(themes, theme == "invariant" ? "invariant.css" : "dark.css");
            IThemeCssUrlResolver resolver = Substitute.For<IThemeCssUrlResolver>();
            resolver.ResolveCssUrl(Arg.Any<string>()).Returns(call => Path.Combine(themes, call.Arg<string>()));
            return new ThemeLoader(resolver, new ThemeFileReader()).LoadTheme(path,
                theme == "invariant" ? ThemeId.DefaultLight : new ThemeId(theme), []);
        }
    }
}
