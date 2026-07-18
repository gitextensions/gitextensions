using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class DeleteBranchTests
{
    private bool _originalDontConfirmDeleteUnmergedBranch;
    private StubMessageBoxHost _messageBoxes = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        _originalDontConfirmDeleteUnmergedBranch = AppSettings.DontConfirmDeleteUnmergedBranch;
        _messageBoxes = new StubMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxes;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.DontConfirmDeleteUnmergedBranch = _originalDontConfirmDeleteUnmergedBranch;
    }

    [AvaloniaTest]
    public void FormDeleteBranch_should_construct_with_branch_selector()
    {
        FormDeleteBranch form = new();

        form.FindControl<BranchComboBox>("Branches").Should().NotBeNull();
        Button delete = form.FindControl<Button>("Delete")
            ?? throw new InvalidOperationException("Delete button was not created.");
        delete.IsDefault.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormDeleteBranch_should_use_existing_translation_keys_once()
    {
        FormDeleteBranch form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormDeleteBranch), "$this", "Text", "Delete branch");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteBranch), "Delete", "Text", "&Delete");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteBranch), "labelSelectBranches", "Text", "Select &branches");
        translation.Received(1).AddTranslationItem(
            nameof(FormDeleteBranch),
            "_branchUsedByWorktreeQuestion",
            "Text",
            "The following branches are checked out in worktrees and cannot be deleted directly:\n\n{0}\n\nDo you want to delete the worktrees and branches together?");
        translation.Received(1).AddTranslationItem(
            nameof(FormDeleteBranch),
            "_cannotDeleteBranchInMainWorktree",
            "Text",
            "The branch “{0}” cannot be deleted because it is checked out in the main worktree at:\n{1}");
        translation.Received(1).AddTranslationItem(
            nameof(FormDeleteBranch),
            "_cannotDeleteCurrentBranchMessage",
            "Text",
            "Cannot delete the branch “{0}” which you are currently on.");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteBranch), "_deleteBranchCaption", "Text", "Delete Branches");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteBranch), "_deleteBranchConfirmTitle", "Text", "Delete Confirmation");
        translation.Received(1).AddTranslationItem(
            nameof(FormDeleteBranch),
            "_deleteBranchQuestion",
            "Text",
            "The selected branch(es) have not been merged into HEAD.\r\nProceed?");
        translation.Received(1).AddTranslationItem(
            nameof(FormDeleteBranch),
            "_useReflogHint",
            "Text",
            "Did you know you can use reflog to restore deleted branches?");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public void FormDeleteBranch_should_render_all_fields_at_its_minimum_size()
    {
        AppSettings.DontConfirmDeleteUnmergedBranch = true;
        (IGitUICommands commands, _) = CreateCommands("main", "feature");
        FormDeleteBranch form = new(commands, ["feature"])
        {
            Width = 420,
            Height = 130,
        };
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            BranchComboBox branches = form.FindControl<BranchComboBox>("Branches")
                ?? throw new InvalidOperationException("Branch selector was not created.");
            Button delete = form.FindControl<Button>("Delete")
                ?? throw new InvalidOperationException("Delete button was not created.");

            branches.Bounds.Width.Should().BeGreaterThan(0);
            branches.GetSelectedText().Should().Be("feature");
            delete.Bounds.Width.Should().BeGreaterThan(0);
            form.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Delete_click_should_build_forced_branch_command()
    {
        AppSettings.DontConfirmDeleteUnmergedBranch = true;
        (IGitUICommands commands, _) = CreateCommands("main", "feature");
        IGitCommand? executedCommand = null;
        commands.StartCommandLineProcessDialog(
                Arg.Any<WinFormsShims.IWin32Window>(),
                Arg.Do<IGitCommand>(command => executedCommand = command))
            .Returns(true);

        FormDeleteBranch form = new(commands, ["feature"]);
        form.Show();
        Dispatcher.UIThread.RunJobs();

        Button delete = form.FindControl<Button>("Delete")
            ?? throw new InvalidOperationException("Delete button was not created.");
        delete.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        IGitCommand command = executedCommand
            ?? throw new InvalidOperationException("The delete-branch command was not executed.");
        command.Arguments.ToString().Should().Be("branch --delete --force \"feature\"");
    }

    [AvaloniaTest]
    public void Delete_click_should_reject_the_current_branch()
    {
        AppSettings.DontConfirmDeleteUnmergedBranch = true;
        (IGitUICommands commands, _) = CreateCommands("main", "main");

        FormDeleteBranch form = new(commands, ["main"]);
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            Button delete = form.FindControl<Button>("Delete")
                ?? throw new InvalidOperationException("Delete button was not created.");
            delete.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            _messageBoxes.Messages.Should().ContainSingle()
                .Which.Should().Contain("Cannot delete the branch “main”");
            commands.DidNotReceive().StartCommandLineProcessDialog(
                Arg.Any<WinFormsShims.IWin32Window>(),
                Arg.Any<IGitCommand>());
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Delete_click_should_delete_a_branch_in_a_real_repository()
    {
        AppSettings.DontConfirmDeleteUnmergedBranch = true;
        string workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.DeleteBranch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workingDirectory);
        try
        {
            using ServiceContainer services = CreateServiceContainer();
            GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), workingDirectory);
            module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
            module.SetSetting("user.name", "Avalonia Test");
            module.SetSetting("user.email", "avalonia@example.com");
            File.WriteAllText(Path.Combine(workingDirectory, "tracked.txt"), "content");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "avalonia-delete" }).Should().BeTrue();

            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.StartCommandLineProcessDialog(
                    Arg.Any<WinFormsShims.IWin32Window>(),
                    Arg.Any<IGitCommand>())
                .Returns(call => module.GitExecutable.RunCommand(call.ArgAt<IGitCommand>(1).Arguments));

            FormDeleteBranch form = new(commands, ["avalonia-delete"]);
            form.Show();
            Dispatcher.UIThread.RunJobs();

            Button delete = form.FindControl<Button>("Delete")
                ?? throw new InvalidOperationException("Delete button was not created.");
            delete.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            module.GetRefs(RefsFilter.Heads).Select(gitRef => gitRef.Name).Should().NotContain("avalonia-delete");
        }
        finally
        {
            TestDirectory.Delete(workingDirectory);
        }
    }

    [Test]
    public void ClassifyWorktreeBranches_should_classify_main_linked_and_deleted_worktrees()
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.Worktrees-{Guid.NewGuid():N}");
        string mainPath = Path.Combine(root, "main");
        string linkedPath = Path.Combine(root, "feature");
        IGitRef main = CreateBranch("main");
        IGitRef feature = CreateBranch("feature");
        IGitRef stale = CreateBranch("stale");
        IGitRef safe = CreateBranch("safe");
        GitWorktree[] worktrees =
        [
            new(mainPath, GitWorktreeHeadType.Branch, "aaa", "main", IsDeleted: false),
            new(linkedPath, GitWorktreeHeadType.Branch, "bbb", "feature", IsDeleted: false),
            new(Path.Combine(root, "stale"), GitWorktreeHeadType.Branch, "ccc", "stale", IsDeleted: true),
        ];

        FormDeleteBranch.WorktreeBranchClassification result =
            FormDeleteBranch.ClassifyWorktreeBranches([main, feature, stale, safe], worktrees, root);

        result.HasDeletedWorktrees.Should().BeTrue();
        result.MainWorktreeBranches.Should().ContainSingle().Which.Branch.Name.Should().Be("main");
        result.LinkedWorktreeBranches.Should().ContainSingle().Which.Branch.Name.Should().Be("feature");
    }

    [Test]
    public void ClassifyWorktreeBranches_should_skip_the_current_worktree()
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.Worktrees-{Guid.NewGuid():N}");
        string linkedPath = Path.Combine(root, "feature");
        IGitRef feature = CreateBranch("feature");
        GitWorktree[] worktrees =
        [
            new(Path.Combine(root, "main"), GitWorktreeHeadType.Branch, "aaa", "main", IsDeleted: false),
            new(linkedPath, GitWorktreeHeadType.Branch, "bbb", "feature", IsDeleted: false),
        ];

        FormDeleteBranch.WorktreeBranchClassification result =
            FormDeleteBranch.ClassifyWorktreeBranches([feature], worktrees, linkedPath);

        result.MainWorktreeBranches.Should().BeEmpty();
        result.LinkedWorktreeBranches.Should().BeEmpty();
        result.HasDeletedWorktrees.Should().BeFalse();
    }

    [Test]
    public void ClassifyWorktreeBranches_should_skip_detached_and_bare_worktrees()
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.Worktrees-{Guid.NewGuid():N}");
        GitWorktree[] worktrees =
        [
            new(Path.Combine(root, "bare"), GitWorktreeHeadType.Bare, null, null, IsDeleted: false),
            new(Path.Combine(root, "detached"), GitWorktreeHeadType.Detached, "aaa", null, IsDeleted: false),
        ];

        FormDeleteBranch.WorktreeBranchClassification result =
            FormDeleteBranch.ClassifyWorktreeBranches([CreateBranch("feature")], worktrees, root);

        result.MainWorktreeBranches.Should().BeEmpty();
        result.LinkedWorktreeBranches.Should().BeEmpty();
        result.HasDeletedWorktrees.Should().BeFalse();
    }

    private static (IGitUICommands Commands, IGitModule Module) CreateCommands(string currentBranch, params string[] branchNames)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.GetSelectedBranch().Returns(currentBranch);
        module.GetRefs(RefsFilter.Heads).Returns(
            branchNames.Select(branchName => (IGitRef)new GitRef(module, ObjectId.Random(), $"refs/heads/{branchName}")).ToList());
        module.GetWorktrees().Returns([]);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return (commands, module);
    }

    private static ServiceContainer CreateServiceContainer()
    {
        ServiceContainer services = new();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(services);
        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        services.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        services.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        GitCommands.ServiceContainerRegistry.RegisterServices(services);
        return services;
    }

    private static IGitRef CreateBranch(string name)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Name.Returns(name);
        gitRef.CompleteName.Returns($"refs/heads/{name}");
        gitRef.IsHead.Returns(true);
        return gitRef;
    }

    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<string> Messages { get; } = [];

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            return WinFormsShims.DialogResult.OK;
        }
    }
}
