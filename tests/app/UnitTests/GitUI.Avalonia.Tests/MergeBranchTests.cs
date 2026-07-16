using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Help;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class MergeBranchTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private bool _alwaysShowAdvanced;
    private bool _closeProcessDialog;
    private bool _dontCommitMerge;
    private bool _dontShowHelpImages;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        _alwaysShowAdvanced = AppSettings.AlwaysShowAdvOpt;
        _closeProcessDialog = AppSettings.CloseProcessDialog;
        _dontCommitMerge = AppSettings.DontCommitMerge;
        _dontShowHelpImages = AppSettings.DontShowHelpImages;
        AppSettings.AlwaysShowAdvOpt = false;
        AppSettings.DontCommitMerge = false;
        AppSettings.DontShowHelpImages = false;

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

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.MergeBranchTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.AlwaysShowAdvOpt = _alwaysShowAdvanced;
        AppSettings.CloseProcessDialog = _closeProcessDialog;
        AppSettings.DontCommitMerge = _dontCommitMerge;
        AppSettings.DontShowHelpImages = _dontShowHelpImages;
        _serviceContainer.Dispose();
        Directory.Delete(_workingDirectory, recursive: true);
    }

    [AvaloniaTest]
    public void FormMergeBranch_should_construct_and_use_existing_translation_keys()
    {
        FormMergeBranch form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "$this", "Text", "Merge branches");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "Currentbranch", "Text", "Into current branch");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "NonDefaultMergeStrategy", "Text", "Use non-default merge strategy");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "Ok", "Text", "&Merge");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "_formMergeBranchHoverShowImageLabelText", "Text", "Hover to see scenario when fast forward is possible.");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "addLogMessages", "Text", "Add log messages");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "addMergeMessage", "Text", "Specify merge message");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "advanced", "Text", "Show advanced options");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "allowUnrelatedHistories", "Text", "Allow unrelated histories");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "fastForward", "Text", "Keep a single branch line if possible (fast forward)");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "groupBox1", "Text", "Merge");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "label2", "Text", "Merge branch");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "noCommit", "Text", "Do not commit");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "noFastForward", "Text", "Always create a new merge commit");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "squash", "Text", "Squash commits");
        translation.Received(1).AddTranslationItem(nameof(FormMergeBranch), "strategyHelp", "Text", "Help");
        form.helpImageDisplayUserControl1.Image1.Should().NotBeNull();
        form.helpImageDisplayUserControl1.Image2.Should().NotBeNull();

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => call.GetArguments())
            .Where(arguments => Equals(arguments[0], nameof(FormMergeBranch)))
            .Select(arguments => string.Join('.', arguments.Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(
            emittedKeys.Length,
            "each FormMergeBranch field must be routed through exactly one translation path");
    }

    [AvaloniaTest]
    public void Merge_branch_reusable_controls_should_use_existing_translation_keys()
    {
        BranchComboBox branchComboBox = new();
        ITranslation branchTranslation = Substitute.For<ITranslation>();
        branchComboBox.AddTranslationItems(branchTranslation);
        branchComboBox.TranslateItems(branchTranslation);
        branchTranslation.Received(1).AddTranslationItem(
            nameof(BranchComboBox),
            "_branchCheckoutError",
            "Text",
            "Branch '{0}' is not selectable, this branch has been removed from the selection.");

        FormSelectMultipleBranches selectBranches = new();
        ITranslation selectionTranslation = Substitute.For<ITranslation>();
        selectBranches.AddTranslationItems(selectionTranslation);
        selectBranches.TranslateItems(selectionTranslation);
        selectionTranslation.Received(1).AddTranslationItem(
            nameof(FormSelectMultipleBranches),
            "$this",
            "Text",
            "Select multiple branches");
        selectionTranslation.Received(1).AddTranslationItem(
            nameof(FormSelectMultipleBranches),
            "okButton",
            "Text",
            "OK");
        selectionTranslation.Received(1).AddTranslationItem(
            nameof(FormSelectMultipleBranches),
            "selectBranchesLabel",
            "Text",
            "Select branches");

        HelpImageDisplayUserControl help = new();
        ITranslation helpTranslation = Substitute.For<ITranslation>();
        help.AddTranslationItems(helpTranslation);
        help.TranslateItems(helpTranslation);
        helpTranslation.Received(1).AddTranslationItem(
            nameof(HelpImageDisplayUserControl),
            "linkLabelHide",
            "Text",
            "Hide help");
        helpTranslation.Received(1).AddTranslationItem(
            nameof(HelpImageDisplayUserControl),
            "linkLabelShowHelp",
            "Text",
            $"Show{Environment.NewLine}help");
    }

    [AvaloniaTest]
    public void FormMergeBranch_should_load_refs_and_honor_the_default_branch()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands("main", "feature", "origin/main");
        module.GetSelectedBranch().Returns("main");

        FormMergeBranch form = new(commands, "feature");
        form.Show();

        form.currentBranchLabel.Text.Should().Be("main");
        form.Branches.GetSelectedText().Should().Be("feature");
        form.Branches.BranchesToSelect.Should().HaveCount(3);
        module.Received(1).GetRefs(RefsFilter.Heads | RefsFilter.Remotes | RefsFilter.Tags);
        form.Close();
    }

    [AvaloniaTest]
    public void FormMergeBranch_should_apply_advanced_option_dependencies()
    {
        (IGitUICommands commands, _) = CreateCommands("main", "feature");
        FormMergeBranch form = new(commands, "feature");

        form.advancedPanel.IsVisible.Should().BeFalse();
        form.advanced.IsChecked = true;
        form.advancedPanel.IsVisible.Should().BeTrue();

        form.NonDefaultMergeStrategy.IsChecked = true;
        form._NO_TRANSLATE_mergeStrategy.IsVisible.Should().BeTrue();
        form.strategyHelp.IsVisible.Should().BeTrue();

        form.squash.IsChecked = true;
        form.noFastForward.IsChecked = true;
        form.squash.IsEnabled.Should().BeFalse();
        form.squash.IsChecked.Should().BeFalse();

        form.addMergeMessage.IsChecked = true;
        form.mergeMessage.IsEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormSelectMultipleBranches_should_return_all_selected_refs()
    {
        IReadOnlyList<IGitRef> refs =
        [
            CreateRef("one"),
            CreateRef("two"),
            CreateRef("three"),
        ];
        FormSelectMultipleBranches form = new(refs);

        form.SelectBranch("one");
        form.SelectBranch("three");

        form.GetSelectedBranches().Select(branch => branch.Name)
            .Should().BeEquivalentTo(["one", "three"]);
    }

    [AvaloniaTest]
    public async Task FormMergeBranch_should_fast_forward_a_real_repository()
    {
        AppSettings.CloseProcessDialog = true;
        GitModule module = CreateRepositoryWithFeatureBranch(out ObjectId featureId);
        GitUICommands commands = new(_serviceContainer, module);
        FormMergeBranch form = new(commands, "feature");
        try
        {
            form.Show();
            await WaitUntilAsync(() => form.Branches.GetSelectedText() == "feature");

            form.CaptureRenderedFrame().Should().NotBeNull("the ready-to-merge dialog should render headlessly");
            form.Ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            await WaitUntilAsync(() => !form.IsVisible && module.GetCurrentCheckout() == featureId);

            File.ReadAllText(Path.Combine(_workingDirectory, "tracked.txt"))
                .Should().Contain("feature line");
            module.InTheMiddleOfConflictedMerge().Should().BeFalse();
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
        IConfigValueStore configValueStore = Substitute.For<IConfigValueStore>();
        SettingsSource settingsSource = new SettingsSource<IConfigValueStore>(configValueStore);
        IReadOnlyList<IGitRef> refs = refNames.Select(CreateRef).ToList();

        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDirectory);
        module.WorkingDirGitDir.Returns(Path.Combine(_workingDirectory, ".git"));
        module.CommitEncoding.Returns(Encoding.UTF8);
        module.GetEffectiveSettings().Returns(settingsSource);
        module.GetRefs(Arg.Any<RefsFilter>()).Returns(refs);
        module.GetRemoteBranch(Arg.Any<string>()).Returns(string.Empty);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
        return (commands, module);
    }

    private GitModule CreateRepositoryWithFeatureBranch(out ObjectId featureId)
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");

        string trackedFile = Path.Combine(_workingDirectory, "tracked.txt");
        File.WriteAllText(trackedFile, "initial line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "-M", "main" });

        module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "-b", "feature" });
        File.AppendAllText(trackedFile, "feature line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "feature" });
        featureId = module.GetCurrentCheckout();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "main" });
        return module;
    }

    private static IGitRef CreateRef(string name)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Name.Returns(name);
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

        condition().Should().BeTrue("the merge dialog operation should complete before the timeout");
    }
}
