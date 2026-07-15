using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using AvaloniaEdit;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Editor;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FormCommitTests
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

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.Tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        Directory.Delete(_workingDirectory, recursive: true);
    }

    [AvaloniaTest]
    public void FormCommit_should_construct_and_use_existing_translation_keys()
    {
        FormCommit form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormCommit), "$this", "Text", "Commit");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "toolStageItem", "Text", "&Stage");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "toolUnstageItem", "Text", "&Unstage");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "commitStagedCountLabel", "Text", "Staged");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "Commit", "Text", "&Commit");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(
            emittedKeys.Length,
            "each field must be routed through exactly one translation path");
    }

    [AvaloniaTest]
    public async Task FormCommit_should_show_staged_and_unstaged_changes_with_a_diff_preview()
    {
        GitModule module = CreateRepositoryWithStagedAndUnstagedChanges();
        GitUICommands commands = new(_serviceContainer, module);
        FormCommit form = new(commands);
        try
        {
            form.Show();
            FileStatusList unstaged = form.FindControl<FileStatusList>("Unstaged")
                ?? throw new InvalidOperationException("Unstaged file list was not created.");
            FileStatusList staged = form.FindControl<FileStatusList>("Staged")
                ?? throw new InvalidOperationException("Staged file list was not created.");
            FileViewer selectedDiff = form.FindControl<FileViewer>("SelectedDiff")
                ?? throw new InvalidOperationException("Diff viewer was not created.");
            TextEditor diffEditor = selectedDiff.FindControl<TextEditor>("TextEditor")
                ?? throw new InvalidOperationException("Diff editor was not created.");

            await WaitUntilAsync(() =>
                unstaged.GitItemStatuses.Count == 1
                && staged.GitItemStatuses.Count == 1
                && diffEditor.Document?.Text.Contains("unstaged line", StringComparison.Ordinal) == true);

            form.CaptureRenderedFrame().Should().NotBeNull("the dirty-repository staging view should render headlessly");
            unstaged.SelectedItem.Should().NotBeNull();
            staged.SelectedItem.Should().BeNull("the unstaged list is selected first, matching the upstream dialog");

            ListBox stagedFiles = staged.FindControl<ListBox>("lstFiles")
                ?? throw new InvalidOperationException("Staged file list box was not created.");
            stagedFiles.SelectedIndex = 0;
            await WaitUntilAsync(() =>
                diffEditor.Document?.Text.Contains("staged line", StringComparison.Ordinal) == true
                && diffEditor.Document.Text.Contains("unstaged line", StringComparison.Ordinal) == false);
            staged.SelectedItem.Should().NotBeNull();
            unstaged.SelectedItem.Should().BeNull();
        }
        finally
        {
            form.Close();
        }
    }

    private GitModule CreateRepositoryWithStagedAndUnstagedChanges()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");

        string fileName = Path.Combine(_workingDirectory, "tracked.txt");
        File.WriteAllText(fileName, "initial line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });

        File.AppendAllText(fileName, "staged line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        File.AppendAllText(fileName, "unstaged line\n");
        return module;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the changed files and diff should load before the timeout");
    }
}
