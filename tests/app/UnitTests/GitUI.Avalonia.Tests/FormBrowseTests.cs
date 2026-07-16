using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.UserControls.RevisionGrid;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FormBrowseTests
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
        _serviceContainer.AddService<IAppTitleGenerator>(new AppTitleGenerator(repositoryDescriptionProvider));
        _serviceContainer.AddService<ILinkFactory>(new LinkFactory());
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
    public async Task FormBrowse_should_reload_after_repository_changed_notification()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "content");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });

        GitUICommands commands = new(_serviceContainer, module);
        FormBrowse form = new(commands);
        try
        {
            form.Show();
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
                ?? throw new InvalidOperationException("Revision grid was not created.");
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")
                ?? throw new InvalidOperationException("Revision loading status was not created.");

            await WaitUntilAsync(() => loadingStatus.Text == "1 revisions");

            RevisionGridRefRenderer.RefLabelControl currentBranch =
                revisionGrid.GetVisualDescendants()
                    .OfType<RevisionGridRefRenderer.RefLabelControl>()
                    .Single();
            currentBranch.Icon.Should().Be(RefLabelIcon.Head);
            currentBranch.FontWeight.Should().Be(Avalonia.Media.FontWeight.Bold);
            TextBlock currentCommitSubject = revisionGrid.GetVisualDescendants()
                .OfType<TextBlock>()
                .Single(textBlock => textBlock.Classes.Contains("revision-subject"));
            currentCommitSubject.FontWeight.Should().Be(Avalonia.Media.FontWeight.Bold);

            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "updated");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });
            ObjectId remoteCommit = module.GetCurrentCheckout();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("reset") { "--quiet", "--hard", "HEAD~" });
            module.GitExecutable.RunCommand(new GitArgumentBuilder("update-ref") { "refs/remotes/origin/main", remoteCommit });

            bool reloadStarted = false;
            loadingStatus.PropertyChanged += (_, e) =>
                reloadStarted |= e.Property == TextBlock.TextProperty && loadingStatus.Text == "Loading…";
            commands.RepoChangedNotifier.Notify();

            await WaitUntilAsync(() => reloadStarted && loadingStatus.Text == "2 revisions");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task RevisionGrid_context_menu_should_route_the_selected_revision()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
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

            ContextMenu contextMenu = revisionGrid.FindControl<ContextMenu>("revisionContextMenu")
                ?? throw new InvalidOperationException("Revision context menu was not created.");
            MenuItem checkoutBranch = revisionGrid.FindControl<MenuItem>("checkoutBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Checkout-branch menu item was not created.");
            MenuItem createBranch = revisionGrid.FindControl<MenuItem>("createNewBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Create-branch menu item was not created.");
            ListBox revisions = revisionGrid.FindControl<ListBox>("lstRevisions")
                ?? throw new InvalidOperationException("Revision list was not created.");

            contextMenu.Open(revisions);
            Dispatcher.UIThread.RunJobs();

            TopLevel contextMenuRoot = TopLevel.GetTopLevel(contextMenu)
                ?? throw new InvalidOperationException("Revision context menu did not open in a top level.");
            WriteableBitmap? contextMenuFrame = contextMenuRoot.CaptureRenderedFrame();
            contextMenuFrame.Should().NotBeNull("the opened context menu should render headlessly");

            ObjectId selectedObjectId = revisionGrid.SelectedRevision!.ObjectId;
            checkoutBranch.IsEnabled.Should().BeTrue();
            createBranch.IsEnabled.Should().BeTrue();
            checkoutBranch.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            createBranch.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            commands.Received(1).StartCheckoutBranch(
                form,
                Arg.Is<IReadOnlyList<ObjectId>>(objectIds => objectIds.SequenceEqual(new[] { selectedObjectId })));
            commands.Received(1).StartCreateBranchDialog(form, selectedObjectId);
        }
        finally
        {
            form.Close();
        }
    }

    private GitModule CreateRepositoryWithInitialCommit()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "content");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });
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

        condition().Should().BeTrue("the repository reload should complete before the timeout");
    }
}
