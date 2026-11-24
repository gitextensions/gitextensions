using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Text;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.Avatars;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommitInfo;
using GitUI.Compat;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class BrowseCommitInfoSelectionTests
{
    private const int RevisionCount = 18_000;
    private const int SelectedDepth = 1500;

    [Test]
    public async Task Browse_should_size_the_new_commit_message_before_containment_finishes()
    {
        await DedicatedHeadlessTestSession.RunAsync(async () =>
        {
            using JoinableTaskContextTestScope context = new();
            using AppSettingsTestScope settings = new();
            using ServiceContainer services = new();
            GitExtensions.ServiceContainerRegistry.RegisterServices(services);
            AppSettings.CheckForUpdates = false;
            AppSettings.ShowAheadBehindData = false;
            AppSettings.ShowSplitViewLayout = true;
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.RevisionGraphShowArtificialCommits = false;
            AppSettings.MaxRevisionGraphCommits = RevisionCount;
            AppSettings.CommitInfoShowContainedInBranchesLocal = true;
            AppSettings.CommitInfoShowContainedInBranchesRemote = false;
            AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal = false;
            AppSettings.CommitInfoShowContainedInTags = true;
            AppSettings.ShowAnnotatedTagsMessages = false;
            AppSettings.CommitInfoShowTagThisCommitDerivesFrom = false;
            AppSettings.ShowAuthorAvatarInCommitInfo = false;
            AppSettings.ShowAuthorAvatarColumn = false;
            AppSettings.AvatarProvider = AvatarProvider.None;
            AppSettings.AvatarFallbackType = AvatarFallbackType.AuthorInitials;
            AvatarService.UpdateAvatarProvider();

            string directory = Directory.CreateTempSubdirectory("GitExtensions.CommitSelection-").FullName;
            using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(75));
            using ManualResetEventSlim releaseContainment = new();
            try
            {
                IGitExecutorProvider executors = services.GetRequiredService<IGitExecutorProvider>();
                GitModule repository = new(executors, directory);
                ImportHistory(repository);
                ObjectId previous = repository.RevParse($"HEAD~{SelectedDepth - 1}");
                ObjectId target = repository.RevParse($"HEAD~{SelectedDepth}");
                ConcurrentQueue<(string Arguments, bool OnUiThread)> calls = new();
                IGitExecutor original = executors.GetExecutor(directory);
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
                        bool onUi = Dispatcher.UIThread.CheckAccess();
                        calls.Enqueue((command, onUi));
                        if (!onUi && IsContainment(command) && command.Contains(target.ToString(), StringComparison.Ordinal))
                        {
                            releaseContainment.Wait(deadline.Token);
                        }

                        return original.GitExecutable.Start(arguments, call.ArgAt<bool>(1), call.ArgAt<bool>(2),
                            call.ArgAt<bool>(3), call.ArgAt<Encoding?>(4), call.ArgAt<bool>(5), call.ArgAt<bool>(6),
                            call.ArgAt<CancellationToken>(7));
                    });

                IGitCommandRunner runner = Substitute.For<IGitCommandRunner>();
                runner.RunDetached(Arg.Any<CancellationToken>(), Arg.Any<ArgumentString>(), Arg.Any<bool>(),
                        Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<Encoding?>(), Arg.Any<bool>())
                    .Returns(call =>
                    {
                        ArgumentString arguments = call.ArgAt<ArgumentString>(1);
                        calls.Enqueue((arguments.ToString(), Dispatcher.UIThread.CheckAccess()));
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
                using FormBrowse form = new(new GitUICommands(services, module), new BrowseArguments { SelectedId = previous }) { Width = 1100, Height = 780 };
                CommitInfo info = form.FindControl<CommitInfo>("RevisionInfo")!;
                XhtmlTextBlock message = info.GetTestAccessor().CommitMessage;
                XhtmlTextBlock refs = info.GetTestAccessor().RevisionInfo;
                TaskCompletionSource graphLoaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
                form.RevisionGrid.RevisionsLoaded += (_, _) => graphLoaded.TrySetResult();
                try
                {
                    form.Show();
                    form.CommitInfoTabControl.SelectedItem = form.CommitInfoTabPage;
                    await graphLoaded.Task.WaitAsync(deadline.Token);
                    await form.JoinLoadOperationsForTestAsync(deadline.Token);
                    await WaitForAsync(() => info.Revision?.ObjectId == previous
                        && message.GetPlainText().Contains("Fixture baseline", StringComparison.Ordinal)
                        && refs.GetPlainText().Contains("fixture-001", StringComparison.Ordinal));
                    await ThreadHelper.JoinPendingOperationsAsync(deadline.Token);
                    await WaitForAsync(HeightsArePublished);
                    form.RevisionGrid._gridView.Focus().Should().BeTrue();
                    form.UpdateLayout();
                    calls.Clear();

                    AppSettings.ShowAuthorAvatarInCommitInfo = true;
                    form.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, keySymbol: null);
                    form.KeyRelease(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, keySymbol: null);
                    await WaitForAsync(() => message.GetPlainText().Contains("Fixture selection", StringComparison.Ordinal));
                    form.UpdateLayout();
                    double firstBounds = message.Bounds.Height;
                    int contentsHeight = message.ContentsHeight;

                    releaseContainment.Set();
                    await WaitForAsync(() => refs.GetPlainText().Contains("fixture-001", StringComparison.Ordinal)
                        && refs.GetPlainText().Contains("release-001", StringComparison.Ordinal));
                    await ThreadHelper.JoinPendingOperationsAsync(deadline.Token);
                    await WaitForAsync(HeightsArePublished);
                    form.UpdateLayout();

                    (string Arguments, bool OnUiThread)[] selectionCalls = calls.ToArray();
                    await TestContext.Out.WriteLineAsync($"Git total={selectionCalls.Length}, UI={selectionCalls.Count(call => call.OnUiThread)}");

                    await TestContext.Out.WriteLineAsync($"Message first arranged height {firstBounds}; measured contents {contentsHeight}");
                    IEnumerable<(string Arguments, bool OnUiThread)> containment = selectionCalls
                        .Where(call => IsContainment(call.Arguments) && call.Arguments.Contains(target.ToString(), StringComparison.Ordinal));
                    containment.Should().HaveCount(2).And.OnlyContain(call => !call.OnUiThread);
                    selectionCalls.Should().NotContain(call => call.OnUiThread);
                    firstBounds.Should().BeGreaterThanOrEqualTo(contentsHeight);
                    info.Revision?.ObjectId.Should().Be(target);
                    form.RevisionGrid.GetRevisionChildren(target).Should().Equal(previous);
                    form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(form.CommitInfoTabPage);
                }
                finally
                {
                    releaseContainment.Set();
                    form.Close();
                    await ThreadHelper.JoinPendingOperationsAsync(deadline.Token);
                }

                bool HeightsArePublished()
                    => info.GetNativeTestSupport().CommitMessageHeight == message.ContentsHeight
                        && info.GetNativeTestSupport().RevisionInfoHeight == refs.ContentsHeight;

                async Task WaitForAsync(Func<bool> ready)
                {
                    while (!ready())
                    {
                        await Task.Delay(10, deadline.Token);
                    }
                }
            }
            finally
            {
                releaseContainment.Set();
                TestDirectory.Delete(directory);
            }
        });
    }

    private static bool IsContainment(string command)
        => (command.StartsWith("branch ", StringComparison.Ordinal) || command.StartsWith("tag ", StringComparison.Ordinal))
            && command.Contains(" --contains ", StringComparison.Ordinal);

    private static void ImportHistory(GitModule module)
    {
        const int refPairs = 120;
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        StringBuilder input = new();
        for (int mark = 1; mark <= RevisionCount; mark++)
        {
            string message = mark switch
            {
                RevisionCount - SelectedDepth + 1 => "Fixture baseline",
                RevisionCount - SelectedDepth => "Fixture selection\n\n" + string.Join('\n', Enumerable.Repeat("The selected commit has a multi-line message.", 16)),
                _ => $"Fixture revision {mark}",
            };
            input.Append($"commit refs/heads/main\nmark :{mark}\ncommitter Fixture Author <gitextensions-selection@example.invalid> {1_600_000_000 + mark} +0000\ndata {message.Length}\n{message}\n\n");
        }

        for (int index = 0; index < refPairs; index++)
        {
            int mark = RevisionCount - (index * 16);
            input.Append($"reset refs/heads/fixture-{index:000}\nfrom :{mark}\n\nreset refs/tags/release-{index:000}\nfrom :{mark}\n\n");
        }

        input.Append("done\n");
        module.GitExecutable.Execute(new GitArgumentBuilder("fast-import") { "--quiet" },
            writeInput: writer => writer.Write(input.ToString())).ExitCode.Should().Be(0);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("reset") { "--hard", "--quiet" }).Should().BeTrue();
    }
}
