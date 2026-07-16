using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
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
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "btnContinueRebase", "Text", "&Continue rebase");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "btnRebase", "Text", "Rebase");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "btnSkip", "Text", "S&kip currently applying commit");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "btnSolveConflicts", "Text", "&Solve conflicts");
        translation.Received(1).AddTranslationItem(nameof(FormRebase), "checkBoxUpdateRefs", "Text", "Update dependent r&efs");
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
    public void FormRebase_should_load_refs_and_keep_interactive_controls_absent()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands("main", "feature", "origin/main");
        module.GetSelectedBranch().Returns("feature");

        FormRebase form = new(commands, "main");
        form.Show();

        form.Currentbranch.Text.Should().Be("feature");
        form.cboBranches.Text.Should().Be("main");
        form.cboBranches.ItemCount.Should().Be(3);
        form.chkInteractive.IsVisible.Should().BeFalse();
        form.chkAutosquash.IsVisible.Should().BeFalse();
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
        form.btnContinueRebase.IsVisible.Should().BeFalse();
        form.Close();
    }

    [Test]
    public void StartInteractiveRebase_should_remain_explicitly_unavailable()
    {
        IGitModule module = Substitute.For<IGitModule>();
        GitUICommands commands = new(Substitute.For<IServiceProvider>(), module);

        Action action = () => commands.StartInteractiveRebase(owner: null, onto: "main");

        action.Should().Throw<NotImplementedException>();
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
