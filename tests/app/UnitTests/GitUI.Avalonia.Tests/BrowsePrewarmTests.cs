using System.ComponentModel.Design;
using System.Text;
using Avalonia.Controls;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class BrowsePrewarmTests
{
    [Test]
    public async Task Browse_startup_should_prewarm_all_branches_before_history_is_available()
    {
        await DedicatedHeadlessTestSession.RunAsync(async () =>
        {
            Thread.CurrentThread.IsThreadPoolThread.Should().BeFalse();
            Dispatcher.UIThread.CheckAccess().Should().BeTrue();
            using JoinableTaskContextTestScope context = new();
            using AppSettingsTestScope settings = new();
            using ServiceContainer services = new();
            GitExtensions.ServiceContainerRegistry.RegisterServices(services);
            AppSettings.CheckForUpdates = false;
            string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.BrowsePrewarm-{Guid.NewGuid():N}");
            Directory.CreateDirectory(directory);
            try
            {
                AppSettings.ShowAheadBehindData = true;
                AppSettings.RevisionGraphShowArtificialCommits = false;
                GitModule repository = new(services.GetRequiredService<IGitExecutorProvider>(), directory);
                repository.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
                repository.SetSetting("user.name", "Avalonia Test");
                repository.SetSetting("user.email", "avalonia@example.com");
                File.WriteAllText(Path.Combine(directory, "tracked.txt"), "content");
                repository.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
                repository.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
                IGitExecutor original = services.GetRequiredService<IGitExecutorProvider>().GetExecutor(directory);
                TaskCompletionSource<int> allBranchesStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
                TaskCompletionSource logStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
                using ManualResetEventSlim releaseLog = new();
                using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(15));
                IExecutable executable = Substitute.For<IExecutable>();
                executable.WorkingDir.Returns(original.GitExecutable.WorkingDir);
                executable.Command.Returns(original.GitExecutable.Command);
                executable.PrefixArguments.Returns(original.GitExecutable.PrefixArguments);
                executable.Start(Arg.Any<ArgumentString>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(),
                        Arg.Any<Encoding?>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                    .Returns(call =>
                    {
                        ArgumentString arguments = call.ArgAt<ArgumentString>(0);
                        string command = arguments.ToString();
                        if (command.Contains("%(push:track,nobracket)", StringComparison.Ordinal)
                            && command.EndsWith("refs/heads/", StringComparison.Ordinal))
                        {
                            TestContext.Out.WriteLine($"All-branch query: thread={Environment.CurrentManagedThreadId}, pool={Thread.CurrentThread.IsThreadPoolThread}, UI={Dispatcher.UIThread.CheckAccess()}, context={SynchronizationContext.Current?.GetType().Name}");
                            allBranchesStarted.TrySetResult(Environment.CurrentManagedThreadId);
                        }

                        return original.GitExecutable.Start(arguments, call.ArgAt<bool>(1), call.ArgAt<bool>(2),
                            call.ArgAt<bool>(3), call.ArgAt<Encoding?>(4), call.ArgAt<bool>(5), call.ArgAt<bool>(6),
                            call.ArgAt<CancellationToken>(7));
                    });
                IGitCommandRunner runner = Substitute.For<IGitCommandRunner>();
                runner.RunDetached(Arg.Any<CancellationToken>(), Arg.Any<ArgumentString>(), Arg.Any<bool>(), Arg.Any<bool>(),
                        Arg.Any<bool>(), Arg.Any<Encoding?>(), Arg.Any<bool>())
                    .Returns(call =>
                    {
                        ArgumentString arguments = call.ArgAt<ArgumentString>(1);
                        string command = arguments.ToString();
                        if (command.StartsWith("log ", StringComparison.Ordinal)
                            || command.Contains(" log ", StringComparison.Ordinal))
                        {
                            logStarted.TrySetResult();
                            releaseLog.Wait(deadline.Token);
                        }

                        return original.GitCommandRunner.RunDetached(call.ArgAt<CancellationToken>(0), arguments,
                            call.ArgAt<bool>(2), call.ArgAt<bool>(3), call.ArgAt<bool>(4), call.ArgAt<Encoding?>(5), call.ArgAt<bool>(6));
                    });
                IGitExecutor executor = Substitute.For<IGitExecutor>();
                executor.WorkingDir.Returns(original.WorkingDir);
                executor.GetGitDirectory().Returns(original.GetGitDirectory());
                executor.GitExecutable.Returns(executable);
                executor.GitCommandRunner.Returns(runner);
                executor.GitWindowsExecutable.Returns(original.GitWindowsExecutable);
                executor.GitWindowsCommandRunner.Returns(original.GitWindowsCommandRunner);
                executor.WslDistro.Returns(original.WslDistro);
                IGitExecutorProvider provider = Substitute.For<IGitExecutorProvider>();
                provider.GetExecutor(Arg.Any<string>()).Returns(executor);
                GitModule module = new(provider, directory);
                int uiThread = Environment.CurrentManagedThreadId;
                using FormBrowse form = new(new GitUICommands(services, module), new BrowseArguments { IsFileHistoryMode = true });
                try
                {
                    TaskCompletionSource loaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    form.RevisionGrid.RevisionsLoaded += (_, _) => loaded.TrySetResult();
                    form.Show();
                    await logStarted.Task.WaitAsync(deadline.Token);
                    int prewarmThread = await allBranchesStarted.Task.WaitAsync(deadline.Token);
                    prewarmThread.Should().NotBe(uiThread);
                    form.FindControl<Control>("leftPanel")!.IsVisible.Should().BeFalse();
                    form.RevisionGrid._gridView.Items.Should().BeEmpty();
                    form.RevisionGrid.GetSelectedRevisions().Should().BeEmpty();
                    releaseLog.Set();
                    await loaded.Task.WaitAsync(deadline.Token);
                    form.RevisionGrid.SelectedRevision?.ObjectId.Should().Be(repository.GetCurrentCheckout());
                    form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!.Text.Should().Be("1 revisions");
                }
                finally
                {
                    releaseLog.Set();
                    form.Close();
                }
            }
            finally
            {
                TestDirectory.Delete(directory);
            }
        });
    }
}
