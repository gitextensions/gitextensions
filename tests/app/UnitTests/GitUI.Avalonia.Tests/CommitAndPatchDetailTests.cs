using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Editor;
using GitUI.HelperDialogs;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class CommitAndPatchDetailTests
{
    private const string SamplePatch =
        "diff --git a/second.txt b/second.txt\n" +
        "new file mode 100644\n" +
        "index 0000000..3e75765\n" +
        "--- /dev/null\n" +
        "+++ b/second.txt\n" +
        "@@ -0,0 +1 @@\n" +
        "+second\n" +
        "diff --git a/tracked.txt b/tracked.txt\n" +
        "index 5dfcbbf..466b0cc 100644\n" +
        "--- a/tracked.txt\n" +
        "+++ b/tracked.txt\n" +
        "@@ -1 +1 @@\n" +
        "-before\n" +
        "+after\n";

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

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.CommitPatch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void Detail_surfaces_should_construct_with_the_original_control_names()
    {
        FormCommitDiff commitForm = new();
        FormViewPatch patchForm = new();

        commitForm.FindControl<CommitDiff>("CommitDiff").Should().NotBeNull();
        patchForm.FindControl<ListBox>("GridChangedFiles").Should().NotBeNull();
        patchForm.FindControl<FileViewer>("ChangesList").Should().NotBeNull();
        patchForm.FindControl<TextBox>("PatchFileNameEdit").Should().NotBeNull();
        patchForm.FindControl<Button>("BrowsePatch").Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormViewPatch_should_use_existing_translation_keys_once()
    {
        FormViewPatch form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormViewPatch), "$this", "Text", "View patch file");
        translation.Received(1).AddTranslationItem(nameof(FormViewPatch), "BrowsePatch", "Text", "Browse");
        translation.Received(1).AddTranslationItem(nameof(FormViewPatch), "File", "HeaderText", "Type");
        translation.Received(1).AddTranslationItem(nameof(FormViewPatch), "FileNameA", "HeaderText", "Filename");
        translation.Received(1).AddTranslationItem(nameof(FormViewPatch), "_patchFileFilterString", "Text", "Patch file (*.Patch)");
        translation.Received(1).AddTranslationItem(nameof(FormViewPatch), "_patchFileFilterTitle", "Text", "Select patch file");
        translation.Received(1).AddTranslationItem(nameof(FormViewPatch), "labelPatch", "Text", "Patch");
        translation.Received(1).AddTranslationItem(nameof(FormViewPatch), "typeDataGridViewTextBoxColumn", "HeaderText", "Change");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => call.GetArguments())
            .Where(arguments => Equals(arguments[0], nameof(FormViewPatch)))
            .Select(arguments => string.Join('.', arguments.Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public void FormCommitDiff_should_use_the_existing_title_translation_key_once()
    {
        FormCommitDiff form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormCommitDiff), "$this", "Text", "Diff");
    }

    [AvaloniaTest]
    public void FormViewPatch_should_parse_files_and_show_the_selected_fixed_patch()
    {
        GitModule module = CreateRepository(out _);
        GitUICommands commands = new(_serviceContainer, module);
        FormViewPatch form = new(commands);

        form.GetTestAccessor().LoadPatchText(SamplePatch);

        form.GetTestAccessor().Patches.Should().HaveCount(2);
        form.GridChangedFiles.SelectedItem.Should().Be(form.GetTestAccessor().Patches[0]);
        form.ChangesList.GetTestAccessor().ViewMode.Should().Be(ViewMode.FixedDiff);
        form.ChangesList.TextEditor.Text.Should().Contain("+second");
    }

    [AvaloniaTest]
    public async Task FormCommitDiff_should_show_real_commit_files_details_and_title()
    {
        GitModule module = CreateRepository(out ObjectId headId);
        GitUICommands commands = new(_serviceContainer, module);
        FormCommitDiff form = new(commands, headId);
        form.Show();
        try
        {
            CommitDiff commitDiff = form.CommitDiff;

            await WaitUntilAsync(() => !string.IsNullOrEmpty(commitDiff.FileViewer.TextEditor.Text));

            commitDiff.FileStatusList.AllItems.Should().ContainSingle();
            commitDiff.FileStatusList.SelectedItem!.Name.Should().Be("tracked.txt");
            commitDiff.CommitInformation.Revision!.ObjectId.Should().Be(headId);
            form.Text.Should().StartWith("Diff - " + headId.ToShortString());
            commitDiff.FileViewer.TextEditor.Text.Should().Contain("+after");
            form.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void PatchGrid_double_click_should_route_commits_and_patch_files_to_the_shared_commands()
    {
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        IGitModule module = Substitute.For<IGitModule>();
        commands.Module.Returns(module);
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(commands);
        PatchGrid patchGrid = new() { UICommandsSource = source };

        PatchFile commit = new() { ObjectId = ObjectId.Random() };
        patchGrid.Patches.ItemsSource = new[] { commit };
        patchGrid.Patches.SelectedItem = commit;
        patchGrid.GetTestAccessor().OpenSelectedPatch();

        commands.Received(1).StartFormCommitDiff(commit.ObjectId);

        PatchFile patch = new() { FullName = "0001-change.patch" };
        patchGrid.Patches.ItemsSource = new[] { patch };
        patchGrid.Patches.SelectedItem = patch;
        patchGrid.GetTestAccessor().OpenSelectedPatch();

        commands.Received(1).StartViewPatchDialog(patch.FullName);
    }

    private GitModule CreateRepository(out ObjectId headId)
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");

        string trackedFile = Path.Combine(_workingDirectory, "tracked.txt");
        File.WriteAllText(trackedFile, "before\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "before" }).Should().BeTrue();

        File.WriteAllText(trackedFile, "after\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "after" }).Should().BeTrue();
        headId = module.GetCurrentCheckout();
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

        condition().Should().BeTrue("the condition should be met within the timeout");
    }
}
