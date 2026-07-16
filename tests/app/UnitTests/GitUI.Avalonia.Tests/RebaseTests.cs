using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RebaseTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private bool _alwaysShowAdvanced;
    private bool _closeProcessDialog;
    private bool _dontShowHelpImages;
    private bool _rebaseAutoStash;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        _alwaysShowAdvanced = AppSettings.AlwaysShowAdvOpt;
        _closeProcessDialog = AppSettings.CloseProcessDialog;
        _dontShowHelpImages = AppSettings.DontShowHelpImages;
        _rebaseAutoStash = AppSettings.RebaseAutoStash;
        AppSettings.AlwaysShowAdvOpt = false;
        AppSettings.DontShowHelpImages = false;
        AppSettings.RebaseAutoStash = false;

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

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.RebaseTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.AlwaysShowAdvOpt = _alwaysShowAdvanced;
        AppSettings.CloseProcessDialog = _closeProcessDialog;
        AppSettings.DontShowHelpImages = _dontShowHelpImages;
        AppSettings.RebaseAutoStash = _rebaseAutoStash;
        _serviceContainer.Dispose();
        Directory.Delete(_workingDirectory, recursive: true);
    }

    [AvaloniaTest]
    public void FormRebase_should_construct_and_use_existing_translation_keys()
    {
        FormRebase form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormRebase), "$this", "Text", "Rebase");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "_continueRebaseText", "Text", "&Continue rebase");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "_solveConflictsText", "Text", "&Solve conflicts");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "_noBranchSelectedText", "Text", "Please select a branch");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "btnAbort", "Text", "A&bort");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "btnAddFiles", "Text", "&Add files");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "btnContinueRebase", "Text", "&Continue rebase");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "btnEditTodo", "Text", "&Edit todo...");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "btnRebase", "Text", "Rebase");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "btnSkip", "Text", "S&kip currently applying commit");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "btnSolveConflicts", "Text", "&Solve conflicts");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "checkBoxUpdateRefs", "Text", "Update dependent r&efs");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "chkAutosquash", "Text", "Autos&quash");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "chkCommitterDateIsAuthorDate", "Text", "Co&mmitter date is author date");
        translation.Received(1).AddTranslationItem(
            nameof(FormRebase),
            "chkCommitterDateIsAuthorDate",
            "toolTip1",
            $"Sets the commit date to the original author date{Environment.NewLine}(instead of the current date).");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "chkIgnoreDate", "Text", "Ignore &date");
        translation.Received(1).AddTranslationItem(
            nameof(FormRebase),
            "chkIgnoreDate",
            "toolTip1",
            $"Sets the author date to the current date (same as{Environment.NewLine}commit date), ignoring the original author date.");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "chkPreserveMerges", "Text", "&Preserve Merges");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "chkInteractive", "Text", "&Interactive Rebase");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "chkSpecificRange", "Text", "Specific ra&nge");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "chkStash", "Text", "A&uto stash");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "label2", "Text", "&Rebase on");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "lblCurrent", "Text", "Current branch:");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "lblRebase", "Text", "Rebase current branch on top of another branch");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "llblShowOptions", "Text", "Show options");
        form.PanelLeftImage.Image1.Should().NotBeNull();

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => call.GetArguments())
            .Where(arguments => Equals(arguments[0], nameof(FormRebase)))
            .Select(arguments => string.Join('.', arguments.Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(
            emittedKeys.Length,
            "each FormRebase field must be routed through exactly one translation path");
    }

    [AvaloniaTest]
    public void FormRebase_should_load_refs_and_offer_interactive_rebase()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands("main", "feature", "origin/main");
        module.GetSelectedBranch().Returns("feature");

        FormRebase form = new(commands, "main");
        form.Show();

        form.Currentbranch.Text.Should().Be("feature");
        form.cboBranches.Text.Should().Be("main");
        form.cboBranches.ItemCount.Should().Be(3);
        form.chkInteractive.IsVisible.Should().BeTrue();
        form.chkAutosquash.IsVisible.Should().BeTrue();
        form.chkAutosquash.IsEnabled.Should().BeFalse();
        form.chkInteractive.IsChecked = true;
        form.chkAutosquash.IsEnabled.Should().BeTrue();
        form.btnEditTodo.IsVisible.Should().BeFalse();
        form.PatchGrid.IsVisible.Should().BeFalse();
        form.Close();
    }

    [AvaloniaTest]
    public void FormRebase_should_apply_plain_advanced_option_dependencies()
    {
        (IGitUICommands commands, _) = CreateCommands("main", "feature");
        FormRebase form = new(commands, "main");
        form.Show();

        form.flpnlOptionsPanelTop.IsVisible.Should().BeTrue("normal dialog mode opens options like upstream");
        form.chkSpecificRange.IsChecked = true;
        form.txtFrom.IsEnabled.Should().BeTrue();
        form.cboTo.IsEnabled.Should().BeTrue();

        form.chkIgnoreDate.IsChecked = true;
        form.chkCommitterDateIsAuthorDate.IsEnabled.Should().BeFalse();
        form.chkPreserveMerges.IsEnabled.Should().BeFalse();

        form.chkIgnoreDate.IsChecked = false;
        form.chkCommitterDateIsAuthorDate.IsChecked = true;
        form.chkIgnoreDate.IsEnabled.Should().BeFalse();
        form.Close();
    }

    [AvaloniaTest]
    public void FormRebase_should_show_the_conflict_controller_during_a_rebase()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands("main", "feature");
        module.InTheMiddleOfRebase().Returns(true);
        module.InTheMiddleOfConflictedMerge().Returns(true);

        FormRebase form = new(commands, defaultBranch: null);
        form.Show();

        form.rebasePanel.IsVisible.Should().BeFalse();
        form.btnRebase.IsVisible.Should().BeFalse();
        form.btnSolveConflicts.IsVisible.Should().BeTrue();
        form.btnSolveMergeconflicts.IsVisible.Should().BeTrue();
        form.btnSkip.IsVisible.Should().BeTrue();
        form.btnAbort.IsVisible.Should().BeTrue();
        form.btnAddFiles.IsVisible.Should().BeTrue();
        form.btnEditTodo.IsVisible.Should().BeTrue();
        form.PatchGrid.IsVisible.Should().BeTrue();
        form.btnContinueRebase.IsVisible.Should().BeFalse();
        form.Close();
    }

    [AvaloniaTest]
    public void FormRebase_should_preselect_interactive_mode()
    {
        (IGitUICommands commands, _) = CreateCommands("main", "feature");

        FormRebase form = new(
            commands,
            from: string.Empty,
            to: null,
            defaultBranch: "main",
            interactive: true,
            startRebaseImmediately: false);
        form.Show();

        form.chkInteractive.IsChecked.Should().BeTrue();
        form.chkAutosquash.IsEnabled.Should().BeTrue();
        form.Close();
    }

    [AvaloniaTest]
    public async Task FormRebase_should_rebase_a_real_repository()
    {
        AppSettings.CloseProcessDialog = true;
        GitModule module = CreateDivergedRepository(out ObjectId mainId, out ObjectId oldFeatureId);
        GitUICommands commands = new(_serviceContainer, module);
        FormRebase form = new(commands, "main");
        try
        {
            form.Show();
            await WaitUntilAsync(() => form.cboBranches.Text == "main");

            form.CaptureRenderedFrame().Should().NotBeNull("the ready-to-rebase dialog should render headlessly");
            form.btnRebase.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            await WaitUntilAsync(() => !form.IsVisible && module.GetCurrentCheckout() != oldFeatureId);

            GitRevision rebasedFeature = module.GetRevision(module.GetCurrentCheckout(), shortFormat: true, loadRefs: false);
            rebasedFeature.FirstParentId.Should().Be(mainId);
            File.ReadAllText(Path.Combine(_workingDirectory, "feature.txt")).Should().Contain("feature line");
            File.ReadAllText(Path.Combine(_workingDirectory, "main.txt")).Should().Contain("main line");
            module.InTheMiddleOfRebase().Should().BeFalse();
        }
        finally
        {
            if (form.IsVisible)
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public async Task FormRebase_should_run_a_real_interactive_rebase()
    {
        AppSettings.CloseProcessDialog = true;
        GitModule module = CreateDivergedRepository(out ObjectId mainId, out ObjectId oldFeatureId);
        module.SetSetting("sequence.editor", "true");
        GitUICommands commands = new(_serviceContainer, module);
        FormRebase form = new(
            commands,
            from: string.Empty,
            to: null,
            defaultBranch: "main",
            interactive: true,
            startRebaseImmediately: false);
        try
        {
            form.Show();
            await WaitUntilAsync(() => form.cboBranches.Text == "main");

            form.chkAutosquash.IsChecked = true;
            form.btnRebase.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            await WaitUntilAsync(() => !form.IsVisible && module.GetCurrentCheckout() != oldFeatureId);

            GitRevision rebasedFeature = module.GetRevision(module.GetCurrentCheckout(), shortFormat: true, loadRefs: false);
            rebasedFeature.FirstParentId.Should().Be(mainId);
            module.InTheMiddleOfRebase().Should().BeFalse();
        }
        finally
        {
            if (form.IsVisible)
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public void PatchGrid_should_use_existing_header_translation_keys()
    {
        PatchGrid patchGrid = new();
        ITranslation translation = Substitute.For<ITranslation>();

        patchGrid.AddTranslationItems(translation);
        patchGrid.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(PatchGrid), "Action", "HeaderText", "Action");
        translation.Received(1).AddTranslationItem(nameof(PatchGrid), "CommitHash", "HeaderText", "Commit hash");
        translation.Received(1).AddTranslationItem(nameof(PatchGrid), "FileName", "HeaderText", "Name");
        translation.Received(1).AddTranslationItem(nameof(PatchGrid), "Status", "HeaderText", "Status");
        translation.Received(1).AddTranslationItem(nameof(PatchGrid), "authorDataGridViewTextBoxColumn", "HeaderText", "Author");
        translation.Received(1).AddTranslationItem(nameof(PatchGrid), "dateDataGridViewTextBoxColumn", "HeaderText", "Date");
        translation.Received(1).AddTranslationItem(nameof(PatchGrid), "subjectDataGridViewTextBoxColumn", "HeaderText", "Subject");
        translation.Received(1).AddTranslationItem(
            nameof(PatchGrid),
            "_unableToShowPatchDetails",
            "Text",
            "Unable to show details of patch file.");
    }

    [AvaloniaTest]
    public void PatchGrid_should_parse_interactive_rebase_state()
    {
        GitModule module = CreateLinearRepository(out ObjectId firstId, out ObjectId applyingId, out ObjectId todoId);
        string rebaseDirectory = Path.Combine(_workingDirectory, ".git", "rebase-merge");
        Directory.CreateDirectory(rebaseDirectory);
        File.WriteAllText(
            Path.Combine(rebaseDirectory, "done"),
            $"pick {firstId} first{Environment.NewLine}pick {applyingId} applying{Environment.NewLine}");
        File.WriteAllText(Path.Combine(rebaseDirectory, "stopped-sha"), applyingId.ToShortString());
        File.WriteAllText(
            Path.Combine(rebaseDirectory, "git-rebase-todo"),
            $"pick {todoId} todo{Environment.NewLine}");

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(commands);

        PatchGrid patchGrid = new()
        {
            UICommandsSource = source,
            IsManagingRebase = true,
        };
        patchGrid.Initialize();

        patchGrid.PatchFiles.Should().HaveCount(3);
        patchGrid.PatchFiles![0].IsApplied.Should().BeTrue();
        patchGrid.PatchFiles[1].IsNext.Should().BeTrue();
        patchGrid.PatchFiles[2].IsApplied.Should().BeFalse();
        patchGrid.Patches.SelectedItem.Should().BeSameAs(patchGrid.PatchFiles[1]);
    }

    [AvaloniaTest]
    public async Task FormRebase_should_display_a_real_interactive_conflict()
    {
        GitModule module = CreateConflictingRepository();
        ExecutionResult result = module.GitExecutable.Execute("rebase -i main", throwOnErrorExit: false);
        result.ExitCode.Should().NotBe(0);
        module.InTheMiddleOfRebase().Should().BeTrue();

        GitUICommands commands = new(_serviceContainer, module);
        FormRebase form = new(commands, defaultBranch: null);
        try
        {
            form.Show();
            await WaitUntilAsync(() => form.PatchGrid.PatchFiles?.Count == 2);

            form.PatchGrid.IsVisible.Should().BeTrue();
            form.PatchGrid.PatchFiles!.Should().ContainSingle(patchFile => patchFile.IsNext);
            form.PatchGrid.PatchFiles.Should().OnlyContain(patchFile =>
                !string.IsNullOrWhiteSpace(patchFile.Action)
                && !string.IsNullOrWhiteSpace(patchFile.Subject));
            Dispatcher.UIThread.RunJobs();
            string[] renderedTexts = form.PatchGrid.Patches.GetVisualDescendants()
                .OfType<TextBlock>()
                .Select(textBlock => textBlock.Text ?? string.Empty)
                .ToArray();
            renderedTexts.Should().Contain("pick");
            renderedTexts.Should().Contain("feature conflict");
            renderedTexts.Should().Contain("feature after");
            form.CaptureRenderedFrame().Should().NotBeNull(
                "the interactive conflict controller and patch list should render headlessly");
        }
        finally
        {
            form.Close();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("rebase") { "--abort" });
        }
    }

    [AvaloniaTest]
    public async Task FormChooseCommit_should_preselect_a_real_revision()
    {
        GitModule module = CreateLinearRepository(out _, out ObjectId selectedId, out _);
        GitUICommands commands = new(_serviceContainer, module);
        FormChooseCommit form = new(commands, selectedId.ToString(), showCurrentBranchOnly: true);
        try
        {
            form.Show();
            await WaitUntilAsync(() => form.SelectedRevision?.ObjectId == selectedId);

            form.SelectedRevision!.ObjectId.Should().Be(selectedId);
            form.flowLayoutPanelParents.IsVisible.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormChooseCommit_should_construct_and_use_existing_translation_keys()
    {
        FormChooseCommit form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormChooseCommit), "$this", "Text", "Choose Commit");
        translation.Received(1).AddTranslationItem(nameof(FormChooseCommit), "btnOK", "Text", "OK");
        translation.Received(1).AddTranslationItem(nameof(FormChooseCommit), "buttonGotoCommit", "Text", "Go to commit...");
        translation.Received(1).AddTranslationItem(nameof(FormChooseCommit), "label1", "Text", "Find specific commit:");
        translation.Received(1).AddTranslationItem(nameof(FormChooseCommit), "labelParents", "Text", "Parent(s):");
        translation.Received(1).AddTranslationItem(nameof(FormChooseCommit), "linkLabelParent", "Text", "sha parent 1");
        translation.Received(1).AddTranslationItem(nameof(FormChooseCommit), "linkLabelParent2", "Text", "sha parent 2");
    }

    [AvaloniaTest]
    public void FormAddFiles_should_construct_and_use_existing_translation_keys()
    {
        FormAddFiles form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormAddFiles), "$this", "Text", "Add files");
        translation.Received(1).AddTranslationItem(nameof(FormAddFiles), "AddFiles", "Text", "Add files");
        translation.Received(1).AddTranslationItem(nameof(FormAddFiles), "ShowFiles", "Text", "Show files");
        translation.Received(1).AddTranslationItem(nameof(FormAddFiles), "force", "Text", "Force");
        translation.Received(1).AddTranslationItem(nameof(FormAddFiles), "label1", "Text", "Filter");
        form.Filter.Text.Should().Be(".");
    }

    private (IGitUICommands Commands, IGitModule Module) CreateCommands(params string[] refNames)
    {
        IReadOnlyList<IGitRef> refs = refNames.Select(name => CreateRef(name, isHead: !name.Contains('/'))).ToList();
        IGitVersion gitVersion = Substitute.For<IGitVersion>();
        gitVersion.SupportRebaseMerges.Returns(true);
        gitVersion.SupportUpdateRefs.Returns(true);

        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDirectory);
        module.GitVersion.Returns(gitVersion);
        module.GetRefs(Arg.Any<RefsFilter>()).Returns(refs);
        module.GetSelectedBranch().Returns("feature");
        module.GetEffectiveSetting<bool>(Arg.Any<string>()).Returns((bool?)false);
        module.GetRebaseDir().Returns(Path.Combine(_workingDirectory, ".git", "rebase-merge") + Path.DirectorySeparatorChar);
        module.IsDirtyDir().Returns(false);
        module.InTheMiddleOfRebase().Returns(false);
        module.InTheMiddleOfConflictedMerge().Returns(false);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
        return (commands, module);
    }

    private GitModule CreateDivergedRepository(out ObjectId mainId, out ObjectId featureId)
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");

        File.WriteAllText(Path.Combine(_workingDirectory, "base.txt"), "base line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "base.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "base" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "-M", "main" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "feature" });

        File.WriteAllText(Path.Combine(_workingDirectory, "main.txt"), "main line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "main.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "main" });
        mainId = module.GetCurrentCheckout();

        module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "feature" });
        File.WriteAllText(Path.Combine(_workingDirectory, "feature.txt"), "feature line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "feature.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "feature" });
        featureId = module.GetCurrentCheckout();
        return module;
    }

    private GitModule CreateLinearRepository(
        out ObjectId firstId,
        out ObjectId secondId,
        out ObjectId thirdId)
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");

        firstId = Commit("first");
        secondId = Commit("second");
        thirdId = Commit("third");
        return module;

        ObjectId Commit(string message)
        {
            File.AppendAllText(Path.Combine(_workingDirectory, "history.txt"), $"{message}{Environment.NewLine}");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "history.txt" });
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", message.Quote() });
            return module.GetCurrentCheckout();
        }
    }

    private GitModule CreateConflictingRepository()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        module.SetSetting("sequence.editor", "true");

        File.WriteAllText(Path.Combine(_workingDirectory, "conflict.txt"), "base\n");
        Commit("base", "conflict.txt");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "-M", "main" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "feature" });

        File.WriteAllText(Path.Combine(_workingDirectory, "conflict.txt"), "main\n");
        Commit("main", "conflict.txt");

        module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "feature" });
        File.WriteAllText(Path.Combine(_workingDirectory, "conflict.txt"), "feature\n");
        Commit("feature conflict", "conflict.txt");
        File.WriteAllText(Path.Combine(_workingDirectory, "after.txt"), "after\n");
        Commit("feature after", "after.txt");
        return module;

        void Commit(string message, string file)
        {
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", file });
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", message.Quote() });
        }
    }

    private static IGitRef CreateRef(string name, bool isHead)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Name.Returns(name);
        gitRef.IsHead.Returns(isHead);
        return gitRef;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the rebase dialog operation should complete before the timeout");
    }
}
