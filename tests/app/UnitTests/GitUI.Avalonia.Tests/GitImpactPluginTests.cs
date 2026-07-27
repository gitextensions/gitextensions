using System.ComponentModel.Design;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Plugins.GitImpact;
using GitExtUtils;
using GitUI;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class GitImpactPluginTests
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
            $"GitExtensions.Avalonia.GitImpactTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormImpact_should_construct_with_original_layout_and_translation_keys()
    {
        using FormImpact form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(863);
        form.Height.Should().Be(484);
        form.FindControl<ImpactControl>("Impact").Should().NotBeNull();
        form.FindControl<Border>("pnlAuthorColor")!.Width.Should().Be(20);
        form.FindControl<CheckBox>("cbIncludingSubmodules")!.Content.Should().Be("Including submodules");
        form.GetTestAccessor().IsAuthorVisible.Should().BeFalse();

        translation.Received(1).AddTranslationItem(
            nameof(FormImpact), "_authorCommits", "Text", "{0} ({1} Commits, {2} Changed Lines)");
        translation.Received(1).AddTranslationItem(
            nameof(FormImpact), "cbIncludingSubmodules", "Text", "Including submodules");
        translation.Received(1).AddTranslationItem(
            nameof(FormImpact), "lblAuthor", "Text", "Author");
    }

    [AvaloniaTest]
    public void GitImpactPlugin_should_expose_its_embedded_icon()
    {
        GitImpactPlugin plugin = new();

        plugin.Id.Should().Be(new Guid("F1ACFE42-6A5E-4C30-AC10-9A7C4BB8B480"));
        PluginIconProvider.GetIcon(plugin).Should().NotBeNull();
    }

    [AvaloniaTest]
    public async Task ImpactLoader_should_read_repository_commit_impact()
    {
        GitModule module = CreateRepository();
        using ImpactLoader loader = new(module)
        {
            RespectMailmap = true,
        };
        TaskCompletionSource<IList<ImpactLoader.Commit>> loaded = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource exited = new(TaskCreationOptions.RunContinuationsAsynchronously);
        loader.CommitLoaded += commits => loaded.TrySetResult(commits);
        loader.Exited += (_, _) => exited.TrySetResult();

        loader.Execute();
        IList<ImpactLoader.Commit> commits = await loaded.Task.WaitAsync(TimeSpan.FromSeconds(15));
        await exited.Task.WaitAsync(TimeSpan.FromSeconds(15));

        ImpactLoader.Commit commit = commits.Should().ContainSingle().Which;
        commit.Author.Should().Be("Avalonia Test");
        commit.Data.Commits.Should().Be(1);
        commit.Data.AddedLines.Should().Be(2);
        commit.Data.DeletedLines.Should().Be(0);
    }

    [AvaloniaTest]
    public void ImpactControl_should_aggregate_draw_and_select_authors()
    {
        using ImpactControl control = new();
        Window window = new()
        {
            Width = 400,
            Height = 240,
            Content = control,
        };
        try
        {
            window.Show();
            ImpactControl.TestAccessor accessor = control.GetTestAccessor();
            DateOnly firstWeek = new(2026, 7, 6);
            int invalidated = 0;
            control.Invalidated += (_, _) => invalidated++;

            accessor.AddCommits(
            [
                new ImpactLoader.Commit(firstWeek, "Alice", new ImpactLoader.DataPoint(1, 8, 2)),
                new ImpactLoader.Commit(firstWeek.AddDays(7), "Alice", new ImpactLoader.DataPoint(1, 4, 1)),
            ]);

            accessor.PathCount.Should().Be(1);
            accessor.GraphWidth.Should().Be(170);
            control.Authors.Should().Equal("Alice");
            control.GetAuthorInfo("Alice").Should().BeEquivalentTo(new ImpactLoader.DataPoint(2, 12, 3));
            invalidated.Should().Be(1);
            window.CaptureRenderedFrame().Should().NotBeNull();

            Point hitPoint = accessor.GetAuthorHitPoint("Alice");
            control.TrySetAuthorByScreenPosition((int)hitPoint.X, (int)hitPoint.Y).Should().BeTrue();
            control.SelectedAuthor.Should().Be("Alice");
            control.TrySetAuthorByScreenPosition((int)hitPoint.X, (int)hitPoint.Y).Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Test]
    public void AddIntermediateEmptyWeeks_should_bridge_each_author_span()
    {
        DateOnly firstWeek = new(2026, 7, 6);
        SortedDictionary<DateOnly, Dictionary<string, ImpactLoader.DataPoint>> impact = new()
        {
            [firstWeek] = new() { ["Alice"] = new ImpactLoader.DataPoint(1, 2, 0) },
            [firstWeek.AddDays(7)] = new() { ["Bob"] = new ImpactLoader.DataPoint(1, 1, 0) },
            [firstWeek.AddDays(14)] = new() { ["Alice"] = new ImpactLoader.DataPoint(1, 3, 0) },
        };

        ImpactLoader.AddIntermediateEmptyWeeks(ref impact, ["Alice", "Bob"]);

        ImpactLoader.DataPoint bridge = impact[firstWeek.AddDays(7)]["Alice"];
        bridge.Commits.Should().Be(0);
        bridge.ChangedLines.Should().Be(0);
        impact[firstWeek].Should().NotContainKey("Bob");
        impact[firstWeek.AddDays(14)].Should().NotContainKey("Bob");
    }

    private GitModule CreateRepository()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "trunk" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "impact.txt"), "one\ntwo\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "impact.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        return module;
    }
}
