using System.ComponentModel.Design;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Plugins.GitStatistics;
using GitExtensions.Plugins.GitStatistics.PieChart;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Compat;
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
