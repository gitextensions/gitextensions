using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
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
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class RenameBranchTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private bool _closeProcessDialog;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        _closeProcessDialog = AppSettings.CloseProcessDialog;

        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        _serviceContainer.AddService<IAppTitleGenerator>(new AppTitleGenerator(repositoryDescriptionProvider));
        _serviceContainer.AddService<ILinkFactory>(new LinkFactory());
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.RenameBranchTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.CloseProcessDialog = _closeProcessDialog;
        _serviceContainer.Dispose();
        Directory.Delete(_workingDirectory, recursive: true);
    }

    [AvaloniaTest]
    public void FormRenameBranch_should_construct_and_render_at_its_minimum_size()
    {
        FormRenameBranch form = new()
        {
            Width = 400,
            Height = 80,
        };
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            form.FindControl<TextBox>("BranchNameTextBox").Should().NotBeNull();
            Button rename = form.FindControl<Button>("Ok")
                ?? throw new InvalidOperationException("Rename button was not created.");
            rename.IsDefault.Should().BeTrue();
            form.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormRenameBranch_should_use_existing_translation_keys_once()
    {
        FormRenameBranch form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormRenameBranch), "$this", "Text", "Rename branch");
        translation.Received(1).AddTranslationItem(nameof(FormRenameBranch), "Ok", "Text", "Rename");
        translation.Received(1).AddTranslationItem(nameof(FormRenameBranch), "label1", "Text", "New name");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public void Rename_click_should_cancel_when_the_name_is_unchanged()
    {
        (IGitUICommands commands, _) = CreateCommands();

        FormRenameBranch form = new(commands, "feature");
        Button rename = form.FindControl<Button>("Ok")
            ?? throw new InvalidOperationException("Rename button was not created.");

        rename.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        form.DialogResult.Should().Be(WinFormsShims.DialogResult.Cancel, "an unchanged name is a no-op");
    }

    [AvaloniaTest]
    public void Losing_focus_should_normalise_the_branch_name()
    {
        (IGitUICommands commands, _) = CreateCommands();

        bool originalAutoNormalise = AppSettings.AutoNormaliseBranchName;
        try
        {
            AppSettings.AutoNormaliseBranchName = true;

            FormRenameBranch form = new(commands, "feature");
            form.Show();
            try
            {
                TextBox branchName = form.FindControl<TextBox>("BranchNameTextBox")
                    ?? throw new InvalidOperationException("Branch name box was not created.");
                Button rename = form.FindControl<Button>("Ok")
                    ?? throw new InvalidOperationException("Rename button was not created.");

                branchName.Focus();
                branchName.Text = "my new branch";
                rename.Focus();

                branchName.Text.Should().Be("my_new_branch");
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.AutoNormaliseBranchName = originalAutoNormalise;
        }
    }

    [AvaloniaTest]
    public void Rename_click_should_rename_a_branch_in_a_real_repository()
    {
        AppSettings.CloseProcessDialog = true;

        GitModule module = CreateRepositoryWithInitialCommit(branchName: "old-name");
        GitUICommands commands = new(_serviceContainer, module);
        FormRenameBranch form = new(commands, "old-name");
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            TextBox branchName = form.FindControl<TextBox>("BranchNameTextBox")
                ?? throw new InvalidOperationException("Branch name box was not created.");
            Button rename = form.FindControl<Button>("Ok")
                ?? throw new InvalidOperationException("Rename button was not created.");

            branchName.Text = "new-name";
            rename.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            string[] branches = [.. module.GetRefs(RefsFilter.Heads).Select(gitRef => gitRef.LocalName)];
            branches.Should().Contain("new-name");
            branches.Should().NotContain("old-name");
            form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
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
    public async Task Grid_context_menu_should_offer_renaming_each_local_branch_on_the_commit()
    {
        GitModule module = CreateRepositoryWithInitialCommit(branchName: "feature_one");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "main" }).Should().BeTrue();
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>()).Returns(call => _serviceContainer.GetService(call.Arg<Type>()));

        FormBrowse form = new(commands);
        try
        {
            form.Show();
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
                ?? throw new InvalidOperationException("Revision grid was not created.");
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")
                ?? throw new InvalidOperationException("Revision loading status was not created.");
            await WaitUntilAsync(() => loadingStatus.Text == "1 revisions" && revisionGrid.SelectedRevision is not null);

            MenuItem rename = revisionGrid.FindControl<MenuItem>("renameBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Rename-branch menu item was not created.");

            rename.IsEnabled.Should().BeTrue();
            rename.Items.OfType<MenuItem>().Select(item => item.Header).Should().BeEquivalentTo(
                ["feature__one", "main"],
                "one entry per local branch on the commit, with underscores escaped for display");

            MenuItem featureItem = rename.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "feature__one"));
            featureItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            commands.Received(1).StartRenameDialog(form, "feature_one");
        }
        finally
        {
            form.Close();
        }
    }

    private (IGitUICommands Commands, IGitModule Module) CreateCommands()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDirectory);
        module.IsValidGitWorkingDir().Returns(true);
        module.IsBareRepository().Returns(false);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(typeof(IGitBranchNameNormaliser)).Returns(_serviceContainer.GetService(typeof(IGitBranchNameNormaliser)));
        return (commands, module);
    }

    private GitModule CreateRepositoryWithInitialCommit(string branchName)
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "content");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "-M", branchName }).Should().BeTrue();
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
