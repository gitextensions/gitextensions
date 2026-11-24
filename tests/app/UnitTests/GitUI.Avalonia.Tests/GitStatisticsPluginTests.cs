using System.ComponentModel.Design;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Plugins.GitStatistics;
using GitExtensions.Plugins.GitStatistics.PieChart;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Compat;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using DrawingColor = System.Drawing.Color;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class GitStatisticsPluginTests
{
    private ServiceContainer _serviceContainer = null!;
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
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _workingDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.GitStatisticsTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormGitStatistics_should_construct_with_original_layout_and_translation_keys()
    {
        using FormGitStatistics form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(751);
        form.Height.Should().Be(465);
        form.MinWidth.Should().Be(350);
        form.MinHeight.Should().Be(250);
        form.FindControl<TabControl>("Tabs")!.ItemCount.Should().Be(4);
        form.FindControl<PieChartControl>("CommitCountPie").Should().NotBeNull();
        form.FindControl<PieChartControl>("LinesOfCodeExtensionPie").Should().NotBeNull();
        form.FindControl<PieChartControl>("LinesOfCodePie").Should().NotBeNull();
        form.FindControl<PieChartControl>("TestCodePie").Should().NotBeNull();

        translation.Received(1).AddTranslationItem(
            nameof(FormGitStatistics), "tabPage2", "Text", "Commits per contributor");
        translation.Received(1).AddTranslationItem(
            nameof(FormGitStatistics), "tabPage1", "Text", "Lines of code per language");
        translation.Received(1).AddTranslationItem(
            nameof(FormGitStatistics), "_commitsBy", "Text", "{0:N0} Commits by {1}");
        translation.Received(1).AddTranslationItem(
            nameof(FormGitStatistics), "_linesOfDesignerFilesP", "Text", "{0:N0} Lines in designer files ({1:P1})");
    }

    [AvaloniaTest]
    public void GitStatisticsPlugin_should_expose_its_embedded_icon()
    {
        GitStatisticsPlugin plugin = new();

        plugin.Id.Should().Be(new Guid("17D1507D-C00D-4A10-AB75-DECB2EA5FCBF"));
        PluginIconProvider.GetIcon(plugin).Should().NotBeNull();
    }

    [AvaloniaTest]
    public async Task FormGitStatistics_should_load_repository_statistics()
    {
        GitModule module = CreateRepository();
        using FormGitStatistics form = new(
            _serviceContainer.GetRequiredService<IGitExecutorProvider>(),
            module,
            "*.cs",
            countSubmodules: false)
        {
            DirectoriesToIgnore = @"\Debug;\Release;\obj;\bin",
        };

        await form.GetTestAccessor().LoadStatisticsAsync().WaitAsync(TimeSpan.FromSeconds(20));

        FormGitStatistics.TestAccessor accessor = form.GetTestAccessor();
        accessor.CommitStatisticsText.Should().Contain("Avalonia Test");
        accessor.LinesOfCodeText.Should().Contain("1");
        accessor.LinesPerLanguageText.Should().Contain(".cs");
        accessor.CommitCountPie.ToolTips.Should().ContainSingle();
        accessor.LinesOfCodeExtensionPie.ToolTips.Should().ContainSingle();
    }

    [AvaloniaTest]
    public async Task Statistics_window_should_show_real_counter_updates_before_the_final_total()
    {
        using AppSettingsTestScope settings = new();
        const int fileCount = 256;
        const int linesPerFile = 32_768;
        const int expected = (fileCount * linesPerFile) + 1;
        GitModule module = CreateRepository();
        await Task.Run(() =>
        {
            for (int index = 0; index < fileCount; index++)
            {
                using StreamWriter writer = new(Path.Combine(_workingDirectory, $"Long-{index:D3}.cs"));
                for (int line = 0; line < linesPerFile; line++)
                {
                    writer.WriteLine("public sealed class Source { }");
                }
            }
        });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--all" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "owned counter fixture".Quote() }).Should().BeTrue();
        GitUICommands commands = new(_serviceContainer, module);
        FieldInfo providerField = typeof(ManagedExtensibility).GetField("_exportProvider", BindingFlags.NonPublic | BindingFlags.Static)!;
        FieldInfo loadedField = typeof(PluginRegistry).GetField("_isLoaded", BindingFlags.NonPublic | BindingFlags.Static)!;
        PropertyInfo registeredProperty = typeof(PluginRegistry).GetProperty(nameof(PluginRegistry.PluginsRegistered))!;
        object? previousProvider = providerField.GetValue(null);
        object? previousLoaded = loadedField.GetValue(null);
        bool previousRegistered = PluginRegistry.PluginsRegistered;
        IGitPlugin[] previousPlugins = [.. PluginRegistry.Plugins];
        IRepositoryHostPlugin[] previousHosters = [.. PluginRegistry.GitHosters];
        AttributedPartDiscoveryV1 discovery = new(Resolver.DefaultInstance);
        DiscoveredParts parts = await discovery.CreatePartsAsync(typeof(GitStatisticsPlugin));
        ComposableCatalog catalog = ComposableCatalog.Create(Resolver.DefaultInstance).AddParts(parts);
        CompositionConfiguration configuration = CompositionConfiguration.Create(catalog);
        configuration.ThrowOnErrors();
        using ExportProvider provider = RuntimeComposition.CreateRuntimeComposition(configuration).CreateExportProviderFactory().CreateExportProvider();
        Task? observation = null;
        FormGitStatistics? openedForm = null;
        using IDisposable opened = Window.WindowOpenedEvent.AddClassHandler<FormGitStatistics>((form, _) =>
        {
            openedForm = form;
            observation = ObserveAsync(form);
        });
        try
        {
            PluginRegistry.Plugins.Clear();
            PluginRegistry.GitHosters.Clear();
            loadedField.SetValue(null, false);
            registeredProperty.SetValue(null, false);
            ManagedExtensibility.SetTestExportProvider(provider);
            PluginRegistry.InitializeAll();
            PluginRegistry.Register(commands);
            GitStatisticsPlugin plugin = PluginRegistry.Plugins.OfType<GitStatisticsPlugin>().Single();

            plugin.Execute(new GitUIEventArgs(ownerForm: null, commands)).Should().BeFalse();
            observation.Should().NotBeNull("the real plugin must open its statistics window");
            await observation!;
        }
        finally
        {
            openedForm?.Close();
            if (PluginRegistry.PluginsRegistered)
            {
                PluginRegistry.Unregister(commands);
            }

            providerField.SetValue(null, previousProvider);
            loadedField.SetValue(null, previousLoaded);
            registeredProperty.SetValue(null, previousRegistered);
            PluginRegistry.Plugins.Clear();
            PluginRegistry.Plugins.AddRange(previousPlugins);
            PluginRegistry.GitHosters.Clear();
            PluginRegistry.GitHosters.AddRange(previousHosters);
        }

        return;

        async Task ObserveAsync(FormGitStatistics form)
        {
            TextBlock total = form.FindControl<TextBlock>("TotalLinesOfCode")!;
            List<(int Count, bool OnUIThread)> updates = [];
            total.PropertyChanged += (_, change) =>
            {
                if (change.Property == TextBlock.TextProperty)
                {
                    updates.Add((ReadCount(), Dispatcher.UIThread.CheckAccess()));
                }
            };
            try
            {
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                while (ReadCount() != expected && stopwatch.Elapsed < TimeSpan.FromSeconds(60))
                {
                    await Task.Delay(10);
                }

                ReadCount().Should().Be(expected);
                updates.Should().Contain(update => update.Count > 1 && update.Count < expected);
                updates.Should().OnlyContain(update => update.OnUIThread);
            }
            finally
            {
                form.Close();
            }

            return;

            int ReadCount()
                => int.TryParse(new string((total.Text ?? string.Empty).Where(char.IsDigit).ToArray()), out int value) ? value : 0;
        }
    }

    [AvaloniaTest]
    public void PieChartControl_should_render_hit_test_highlight_and_select_slices()
    {
        PieChartControl control = new()
        {
            InitialAngle = -30,
            ToolTips = ["First", "Second"],
        };
        control.SetValues([3, 1]);
        control.SetColors([Colors.DodgerBlue, Colors.Coral]);
        control.SetSliceRelativeHeight(0.2f);
        control.SetEdgeColorType(EdgeColorType.DarkerThanSurface);
        control.SetShadowStyle(ShadowStyle.GradualShadow);

        Window window = new()
        {
            Width = 320,
            Height = 240,
            Content = control,
        };
        try
        {
            window.Show();
            window.CaptureRenderedFrame().Should().NotBeNull();

            PieChartControl.TestAccessor accessor = control.GetTestAccessor();
            accessor.SliceCount.Should().Be(2);
            DrawingColor originalSliceColor = DrawingColor.FromArgb(
                Colors.DodgerBlue.A,
                Colors.DodgerBlue.R,
                Colors.DodgerBlue.G,
                Colors.DodgerBlue.B);
            DrawingColor adaptedSliceColor = originalSliceColor.AdaptBackColor();
            Color expectedSliceColor = Color.FromArgb(
                adaptedSliceColor.A,
                adaptedSliceColor.R,
                adaptedSliceColor.G,
                adaptedSliceColor.B);
            accessor.GetSliceColor(0).Should().Be(expectedSliceColor);
            accessor.GetEdgeColor(0).Should().Be(CorrectLightness(expectedSliceColor, -0.3));
            Point hitPoint = accessor.GetSliceHitPoint(0);
            accessor.FindSlice(hitPoint).Should().Be(0);

            SliceSelectedArgs? selected = null;
            control.SliceSelected += (_, args) => selected = args;
            accessor.Hover(hitPoint);
            accessor.HighlightedIndex.Should().Be(0);
            accessor.Select(hitPoint);
            selected.Should().NotBeNull();
            selected!.Value.Should().Be(3);
            selected.ToolTip.Should().Be("First");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Pie_chart_sides_should_provide_the_real_pointer_tooltip_and_selection()
    {
        PieChartControl control = new() { ToolTips = ["Only slice"] };
        control.SetValues([1]);
        control.SetSliceRelativeHeight(0.5f);
        Window window = new() { Width = 200, Height = 200, Content = control };
        try
        {
            window.Show();
            window.CaptureRenderedFrame().Should().NotBeNull();
            SliceSelectedArgs? selected = null;
            int selectedCount = 0;
            control.SliceSelected += (_, args) =>
            {
                selected = args;
                selectedCount++;
            };
            Point side = control.TranslatePoint(new Point(190, 100), window)
                ?? throw new AssertionException("The rendered chart is not attached to its actual window.");
            window.MouseMove(side);

            ToolTip.GetTip(control).Should().Be("Only slice");
            control.GetTestAccessor().HighlightedIndex.Should().Be(0);
            control.Cursor?.ToString().Should().Contain(nameof(StandardCursorType.Hand));
            window.MouseDown(side, MouseButton.Left);
            window.MouseUp(side, MouseButton.Left);
            selectedCount.Should().Be(1);
            SliceSelectedArgs actual = selected ?? throw new AssertionException("The actual pointer click did not select a slice.");
            actual.Value.Should().Be(1);
            actual.ToolTip.Should().Be("Only slice");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Pie_chart_hit_tests_should_cover_the_original_periphery_cut_faces_and_bottom()
    {
        PieChart3D complete = new(0, 0, 200, 200, [1], [Colors.Red], 0.5);
        using (DrawingContext context = new DrawingGroup().Open())
        {
            complete.Draw(context);
        }

        complete.FindPieSliceUnderPoint(new Point(190, 100)).Should().Be(0, "the periphery lies outside both ellipses here");
        complete.FindPieSliceUnderPoint(new Point(100, 175)).Should().Be(0, "the lower ellipse extends below the periphery quadrilateral");
        complete.FindPieSliceUnderPoint(new Point(210, 100)).Should().Be(-1);
        PieChart3D separated = new(0, 0, 200, 200, [1, 1], [Colors.Red, Colors.Blue], 0.5);
        separated.SetSliceRelativeDisplacements([0.75, 0.75]);
        using (DrawingContext context = new DrawingGroup().Open())
        {
            separated.Draw(context);
        }

        separated.FindPieSliceUnderPoint(new Point(50, 50)).Should().Be(1, "the rear slice exposes its start cut face");
        separated.FindPieSliceUnderPoint(new Point(150, 50)).Should().Be(1, "the rear slice exposes its end cut face");
        separated.FindPieSliceUnderPoint(separated.GetSliceHitPoint(0)).Should().Be(0);
        separated.FindPieSliceUnderPoint(separated.GetSliceHitPoint(1)).Should().Be(1);
    }

    [AvaloniaTest]
    [TestCase(0.25f)]
    [TestCase(0.5f)]
    public void SetSliceRelativeHeight_should_use_the_original_chart_height_fraction(float relativeHeight)
    {
        PieChartControl control = new();
        control.SetValues([1]);
        control.SetSliceRelativeHeight(relativeHeight);
        Window window = new() { Width = 200, Height = 200, Content = control };
        try
        {
            window.Show();
            window.CaptureRenderedFrame().Should().NotBeNull();
            double expected = control.Bounds.Height * (1 - relativeHeight) / 2;
            control.GetTestAccessor().GetSliceHitPoint(0).Y.Should().BeApproximately(expected, 0.001);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Pie_chart_should_repeat_the_last_supplied_displacement_for_remaining_slices()
    {
        PieChart3D shortDisplacements = new(0, 0, 200, 200, [2, 3, 5], [Colors.Red], 0.5);
        PieChart3D explicitDisplacements = new(0, 0, 200, 200, [2, 3, 5], [Colors.Red], 0.5);
        shortDisplacements.SetSliceRelativeDisplacements([0, 0.4]);
        explicitDisplacements.SetSliceRelativeDisplacements([0, 0.4, 0.4]);
        using (DrawingContext context = new DrawingGroup().Open())
        {
            shortDisplacements.Draw(context);
            explicitDisplacements.Draw(context);
        }

        for (int index = 0; index < 3; index++)
        {
            shortDisplacements.GetSliceHitPoint(index).Should().Be(explicitDisplacements.GetSliceHitPoint(index));
        }
    }

    [AvaloniaTest]
    public void Pie_chart_should_initialize_slices_from_draw_after_public_style_changes()
    {
        InitializationTrackingChart chart = new();
        chart.Initializations.Should().Be(0);
        chart.SetInitialAngle(30);
        chart.SetSliceRelativeDisplacements([0]);
        chart.Initializations.Should().Be(0);
        using (DrawingContext context = new DrawingGroup().Open())
        {
            chart.Draw(context);
        }

        chart.Initializations.Should().Be(1);
        chart.SliceCount.Should().Be(2);
        chart.SetInitialAngle(60);
        chart.Initializations.Should().Be(1);
        using (DrawingContext context = new DrawingGroup().Open())
        {
            chart.Draw(context);
        }

        chart.Initializations.Should().Be(2);
        chart.SliceCount.Should().Be(2);
    }

    [Test]
    public void CodeFile_should_preserve_framework_neutral_line_classification()
    {
        string path = Path.Combine(_workingDirectory, "Sample.cs");
        File.WriteAllText(path, "// comment\n\npublic sealed class Sample;\n");

        CodeFile codeFile = CodeFile.Parse(new FileInfo(path));

        codeFile.TotalLineCount.Should().Be(3);
        codeFile.CommentLineCount.Should().Be(1);
        codeFile.BlankLineCount.Should().Be(1);
        codeFile.CodeLineCount.Should().Be(1);
    }

    private GitModule CreateRepository()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "trunk" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(
            Path.Combine(_workingDirectory, "Sample.cs"),
            "// comment\n\npublic sealed class Sample;\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "Sample.cs" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        return module;
    }

    private sealed class InitializationTrackingChart()
        : PieChart3D(0, 0, 200, 200, [1, 1], [Colors.Red, Colors.Blue], 0.5)
    {
        public int Initializations { get; private set; }

        protected override void InitializePieSlices()
        {
            Initializations++;
            base.InitializePieSlices();
        }
    }

    private static Color CorrectLightness(Color color, double correctionFactor)
    {
        static byte Correct(byte channel, double correctionFactor)
        {
            double value = correctionFactor < 0
                ? channel * (1 + correctionFactor)
                : ((byte.MaxValue - channel) * correctionFactor) + channel;
            return (byte)(int)value;
        }

        return Color.FromRgb(
            Correct(color.R, correctionFactor),
            Correct(color.G, correctionFactor),
            Correct(color.B, correctionFactor));
    }
}
