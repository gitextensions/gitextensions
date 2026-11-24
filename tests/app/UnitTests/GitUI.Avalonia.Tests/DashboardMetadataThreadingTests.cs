using System.ComponentModel.Design;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommonTestUtils;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class DashboardMetadataThreadingTests
{
    [Test]
    public async Task Dashboard_should_show_the_tile_before_its_repository_metadata_finishes()
        => await DedicatedHeadlessTestSession.RunAsync(VerifyMetadataAsync);

    private static async Task VerifyMetadataAsync()
    {
        using JoinableTaskContextTestScope context = new();
        using AppSettingsTestScope settings = new(useSharedCache: true);
        using ServiceContainer services = new();
        using GitModuleTestHelper historyRepo = new();
        using ManualResetEventSlim releaseMetadata = new();
        TaskCompletionSource<bool> metadataStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        FormBrowse? form = null;
        try
        {
            AppSettings.ShowRepoCurrentBranch = true;
            historyRepo.Module.GitExecutable.GetOutput("symbolic-ref HEAD refs/heads/fixture-branch");
            historyRepo.Module.GitExecutable.GetOutput("commit --allow-empty -m initial");
            await RepositoryHistoryManager.Locals.SaveRecentHistoryAsync([new Repository(historyRepo.Module.WorkingDir)]);
            await RepositoryHistoryManager.Locals.SaveFavouriteHistoryAsync([]);

            GitExtensions.ServiceContainerRegistry.RegisterServices(services);
            services.RemoveService(typeof(IUpdateCheckService));
            services.AddService<IUpdateCheckService>(Substitute.For<IUpdateCheckService>());
            IRepositoryCurrentBranchNameCache prewarmCache = services.GetRequiredService<IRepositoryCurrentBranchNameCache>();
            services.RemoveService(typeof(IRepositoryCurrentBranchNameCache));
            IRepositoryCurrentBranchNameCache cache = Substitute.For<IRepositoryCurrentBranchNameCache>();
            cache.GetCurrentBranchName(Arg.Any<string>()).Returns(call =>
            {
                bool onUiThread = Dispatcher.UIThread.CheckAccess();
                metadataStarted.TrySetResult(onUiThread);
                if (!onUiThread && !releaseMetadata.Wait(TimeSpan.FromSeconds(10)))
                {
                    throw new TimeoutException("The test did not release the repository metadata lookup.");
                }

                return historyRepo.Module.GetSelectedBranch();
            });
            services.AddService<IRepositoryCurrentBranchNameCache>(cache);
            IGitExecutorProvider executors = services.GetRequiredService<IGitExecutorProvider>();
            string dashboardPath = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(settings.WindowPositionPath)!, "dashboard")).FullName;
            form = new FormBrowse(new GitUICommands(services, new GitModule(executors, dashboardPath)), new BrowseArguments());
            form.Show();
            await form.JoinLoadOperationsForTestAsync().WaitAsync(TimeSpan.FromSeconds(20));

            (await metadataStarted.Task.WaitAsync(TimeSpan.FromSeconds(10))).Should().BeFalse(
                "the original publishes tiles before background validity and branch reads");
            UserRepositoriesList list = form.GetLogicalDescendants().OfType<Dashboard>().Single().userRepositoriesList;
            form.UpdateLayout();
            ListBoxItem tile = list.GetVisualDescendants().OfType<ListBoxItem>()
                .Single(item => item.Content is UserRepositoriesList.RepositoryListItem);
            object? beforeMetadata = tile.Content;
            HasBranchText().Should().BeFalse();

            releaseMetadata.Set();
            using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(10));
            while (!HasBranchText())
            {
                await Task.Delay(10, deadline.Token);
            }

            tile.Content.Should().BeSameAs(beforeMetadata);
            list.listView1.Items.OfType<UserRepositoriesList.RepositoryListItem>().Should().ContainSingle()
                .Which.Should().BeSameAs(beforeMetadata);

            IRepositoryHistoryUIService history = form.UICommands.GetRequiredService<IRepositoryHistoryUIService>();
            cache.ClearReceivedCalls();
            prewarmCache.InvalidateAll();
            history.TriggerBranchNameCacheUpdate();
            FieldInfo taskField = typeof(RepositoryHistoryUIService)
                .GetField("_branchCacheUpdateTask", BindingFlags.Instance | BindingFlags.NonPublic)!;
            JoinableTask prewarm = (JoinableTask)taskField.GetValue(history)!;
            await prewarm.Task.WaitAsync(deadline.Token);
            Dispatcher.UIThread.RunJobs();

            prewarmCache.GetCachedBranchName(historyRepo.Module.WorkingDir).Should().Be("fixture-branch");
            cache.DidNotReceive().InvalidateAll();
            list.listView1.Items.OfType<UserRepositoriesList.RepositoryListItem>().Should().ContainSingle()
                .Which.Should().BeSameAs(beforeMetadata);

            bool HasBranchText()
                => tile.GetVisualDescendants().OfType<TextBlock>()
                    .Any(text => text.IsVisible && text.Text == "fixture-branch");
        }
        finally
        {
            releaseMetadata.Set();
            form?.Close();
            ((IDisposable?)form)?.Dispose();
            using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(10));
            await ThreadHelper.JoinPendingOperationsAsync(deadline.Token);
        }
    }
}
