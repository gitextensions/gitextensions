using System.ComponentModel.Design;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommitInfo;
using GitUI.Compat;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class CommitInfoLifetimeTests
{
    private static readonly FieldInfo TaskManagerField = typeof(ThreadHelper).GetField("_taskManager", BindingFlags.Static | BindingFlags.NonPublic)!;
    private ServiceContainer _services = null!;
    private AppSettingsTestScope _settings = null!;
    private string _directory = null!;
    private SynchronizationContext? _previousContext;
    private object? _previousTaskManager;
    private JoinableTaskContext _context = null!;
    private GitModule _module = null!;
    private GitRevision _first = null!;
    private GitRevision _second = null!;

    [SetUp]
    public void SetUp()
    {
        _previousContext = SynchronizationContext.Current;
        AvaloniaSynchronizationContext.InstallIfNeeded();
        _previousTaskManager = TaskManagerField.GetValue(null);
        _context = new JoinableTaskContext();
        ThreadHelper.JoinableTaskContext = _context;
        _settings = new AppSettingsTestScope();
        AppSettings.ShowAuthorAvatarInCommitInfo = false;
        AppSettings.CommitInfoShowContainedInBranchesLocal = false;
        AppSettings.CommitInfoShowContainedInBranchesRemote = false;
        AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal = false;
        AppSettings.CommitInfoShowContainedInTags = true;
        AppSettings.ShowAnnotatedTagsMessages = false;
        AppSettings.ShowGitNotes = false;
        AppSettings.CommitInfoShowTagThisCommitDerivesFrom = false;
        _services = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_services);
        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver resolver = new(fileSystem);
        RepositoryDescriptionProvider descriptions = new(resolver);
        _services.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _services.AddService<IGitDirectoryResolver>(resolver);
        _services.AddService<IRepositoryDescriptionProvider>(descriptions);
        _services.AddService<IAppTitleGenerator>(new AppTitleGenerator(descriptions));
        _services.AddService<ILinkFactory>(new LinkFactory());
        GitCommands.ServiceContainerRegistry.RegisterServices(_services);
        GitUI.ServiceContainerRegistry.RegisterServices(_services);
        _directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.CommitInfo.Lifetime-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);
        _module = new GitModule(_services.GetRequiredService<IGitExecutorProvider>(), _directory);
        _module.GitExecutable.RunCommand("init --quiet -b lifetime").Should().BeTrue();
        _module.SetSetting("user.name", "Lifetime Test");
        _module.SetSetting("user.email", "lifetime@example.com");
        _first = AddRevision("First revision\n\n" + string.Join(' ', Enumerable.Repeat("First revision message with real wrapping", 12)), "first");
        _second = AddRevision("Second revision\n\n" + string.Join('\n', Enumerable.Repeat("A longer real commit body", 12)), "second");
    }

    [TearDown]
    public void TearDown()
    {
        _services.Dispose();
        _settings.Dispose();
        TaskManagerField.SetValue(null, _previousTaskManager);
        _context.Dispose();
        SynchronizationContext.SetSynchronizationContext(_previousContext);
        TestDirectory.Delete(_directory);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CommitInfo_should_dispose_owned_resources_on_final_close_or_explicit_dispose(bool closeOwner)
    {
        CommitInfo control = new();
        using CommitInfoOwner owner = CreateOwner(control);
        owner.Show();
        try
        {
            control.Revision = _first;
            await WaitUntilAsync(owner, () => IsSettled(control, "First revision message"));
            if (closeOwner)
            {
                owner.Close();
            }
            else
            {
                ((IDisposable)control).Dispose();
            }

            // Rx posts event removal to the UI context captured when the stream was created.
            Dispatcher.UIThread.RunJobs();
            using (new AwesomeAssertions.Execution.AssertionScope())
            {
                GetContentsHandlers(control.GetTestAccessor().CommitMessage).Should().BeNull();
                GetContentsHandlers(control.GetTestAccessor().RevisionInfo).Should().BeNull();
                CancellationTokenSequence sequence = (CancellationTokenSequence)typeof(CommitInfo)
                    .GetField("_asyncLoadCancellation", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(control)!;
                Action next = () => sequence.Next();
                next.Should().Throw<OperationCanceledException>();
                Delegate? changed = (Delegate?)typeof(GitModuleForm)
                    .GetField("UICommandsChanged", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(owner);
                (changed?.GetInvocationList() ?? []).Should().NotContain(handler => ReferenceEquals(handler.Target, control));
            }
        }
        finally
        {
            owner.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void CommitInfo_should_dispose_an_unused_child_when_its_owner_closes(bool commandOwner)
    {
        CommitInfo control = new();
        Window owner = commandOwner ? CreateOwner(control) : new Window { Width = 600, Height = 500, Content = control };
        owner.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            control.TryGetUICommandsDirect(out _).Should().BeFalse();
            GetContentsHandlers(control.GetTestAccessor().CommitMessage).Should().NotBeNull();
            GetContentsHandlers(control.GetTestAccessor().RevisionInfo).Should().NotBeNull();

            owner.Close();
            Dispatcher.UIThread.RunJobs();

            using (new AwesomeAssertions.Execution.AssertionScope())
            {
                GetContentsHandlers(control.GetTestAccessor().CommitMessage).Should().BeNull();
                GetContentsHandlers(control.GetTestAccessor().RevisionInfo).Should().BeNull();
                CancellationTokenSequence sequence = (CancellationTokenSequence)typeof(CommitInfo)
                    .GetField("_asyncLoadCancellation", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(control)!;
                Action next = () => sequence.Next();
                next.Should().Throw<OperationCanceledException>();
            }
        }
        finally
        {
            owner.Close();
            ((IDisposable)control).Dispose();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void CommitInfo_should_dispose_an_unused_child_when_its_owner_closes_before_Show(bool commandOwner)
    {
        CommitInfo control = new();
        Window owner = commandOwner ? CreateOwner(control) : new Window { Content = control };
        try
        {
            Dispatcher.UIThread.RunJobs();
            GetContentsHandlers(control.GetTestAccessor().CommitMessage).Should().NotBeNull();
            GetContentsHandlers(control.GetTestAccessor().RevisionInfo).Should().NotBeNull();

            owner.Close();
            Dispatcher.UIThread.RunJobs();

            GetContentsHandlers(control.GetTestAccessor().CommitMessage).Should().BeNull();
            GetContentsHandlers(control.GetTestAccessor().RevisionInfo).Should().BeNull();
        }
        finally
        {
            owner.Close();
            ((IDisposable)control).Dispose();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTest]
    public void CommitInfo_should_follow_its_current_owner_when_a_layout_moves_it_to_another_window()
    {
        CommitInfo control = new();
        Window first = new() { Width = 600, Height = 500, Content = control };
        Window second = new() { Width = 600, Height = 500 };
        first.Show();
        second.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            first.Content = null;
            second.Content = control;
            Dispatcher.UIThread.RunJobs();

            first.Close();
            Dispatcher.UIThread.RunJobs();

            GetContentsHandlers(control.GetTestAccessor().CommitMessage).Should().NotBeNull();
            GetContentsHandlers(control.GetTestAccessor().RevisionInfo).Should().NotBeNull();

            second.Close();
            Dispatcher.UIThread.RunJobs();

            GetContentsHandlers(control.GetTestAccessor().CommitMessage).Should().BeNull();
            GetContentsHandlers(control.GetTestAccessor().RevisionInfo).Should().BeNull();
        }
        finally
        {
            first.Close();
            second.Close();
            ((IDisposable)control).Dispose();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTest]
    public void CommitInfo_should_keep_its_resources_when_the_owner_cancels_closing()
    {
        CommitInfo control = new();
        Window owner = new() { Width = 600, Height = 500, Content = control };
        void CancelClose(object? sender, WindowClosingEventArgs e) => e.Cancel = true;
        owner.Closing += CancelClose;
        owner.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            owner.Close();
            Dispatcher.UIThread.RunJobs();

            owner.IsVisible.Should().BeTrue();
            GetContentsHandlers(control.GetTestAccessor().CommitMessage).Should().NotBeNull();
            GetContentsHandlers(control.GetTestAccessor().RevisionInfo).Should().NotBeNull();

            owner.Closing -= CancelClose;
            owner.Close();
            Dispatcher.UIThread.RunJobs();

            GetContentsHandlers(control.GetTestAccessor().CommitMessage).Should().BeNull();
            GetContentsHandlers(control.GetTestAccessor().RevisionInfo).Should().BeNull();
        }
        finally
        {
            owner.Closing -= CancelClose;
            owner.Close();
            ((IDisposable)control).Dispose();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTest]
    public void CommitInfo_should_remain_reusable_when_unparented_before_its_former_owner_closes()
    {
        CommitInfo control = new();
        Window first = new() { Width = 600, Height = 500, Content = control };
        Window second = new() { Width = 600, Height = 500 };
        first.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            first.Content = null;

            first.Close();
            Dispatcher.UIThread.RunJobs();

            GetContentsHandlers(control.GetTestAccessor().CommitMessage).Should().NotBeNull();
            GetContentsHandlers(control.GetTestAccessor().RevisionInfo).Should().NotBeNull();

            second.Content = control;
            second.Show();
            Dispatcher.UIThread.RunJobs();
            second.Close();
            Dispatcher.UIThread.RunJobs();

            GetContentsHandlers(control.GetTestAccessor().CommitMessage).Should().BeNull();
            GetContentsHandlers(control.GetTestAccessor().RevisionInfo).Should().BeNull();
        }
        finally
        {
            first.Close();
            second.Close();
            ((IDisposable)control).Dispose();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTest]
    public async Task CommitInfo_should_defer_resize_notifications_and_remain_usable_after_reattachment()
    {
        CommitInfo control = new();
        using CommitInfoOwner owner = CreateOwner(control);
        owner.Show();
        try
        {
            control.Revision = _first;
            await WaitUntilAsync(owner, () => IsSettled(control, "First revision message"));
            CommitInfoTestSupport.Accessor accessor = control.GetNativeTestSupport();
            int previousHeight = accessor.CommitMessageHeight;
            int previousRefsHeight = accessor.RevisionInfoHeight;
            owner.Content = null;
            owner.Content = control;
            control.Revision = _second;
            owner.Width = 1000;
            owner.UpdateLayout();
            using (new AwesomeAssertions.Execution.AssertionScope())
            {
                accessor.CommitMessageHeight.Should().Be(previousHeight, "the original coalesces contents changes before updating parent rows");
                accessor.RevisionInfoHeight.Should().Be(previousRefsHeight, "the original defers both contents notification streams");
            }

            await WaitUntilAsync(owner, () => IsSettled(control, "A longer real commit body") && accessor.CommitMessageHeight > previousHeight);
            Dispatcher.UIThread.CheckAccess().Should().BeTrue();
            control.Revision.Should().BeSameAs(_second);
            accessor.TableLayout.IsVisible.Should().BeTrue();
        }
        finally
        {
            owner.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTest]
    public async Task CommitInfo_should_report_resize_handler_errors_and_continue_observing_contents()
    {
        CommitInfo control = new();
        using CommitInfoOwner owner = CreateOwner(control);
        owner.Show();
        List<Exception> errors = [];
        List<Exception> dispatcherErrors = [];
        IOException expected = new("Resize consumer failed");
        bool throwOnce = true;
        void Report(object? sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Dispatcher.UIThread.CheckAccess().Should().BeTrue();
            errors.Add(e.Exception);
        }

        void ReportUnhandled(object? sender, DispatcherUnhandledExceptionEventArgs e)
        {
            dispatcherErrors.Add(e.Exception);
        }

        void RejectHeight(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == RowDefinition.HeightProperty && throwOnce)
            {
                throwOnce = false;
                throw expected;
            }
        }

        try
        {
            control.Revision = _first;
            await WaitUntilAsync(owner, () => IsSettled(control, "First revision message"));
            RowDefinition row = control.GetNativeTestSupport().TableLayout.RowDefinitions[1];
            row.PropertyChanged += RejectHeight;
            WinFormsShims.Application.ThreadException += Report;
            Dispatcher.UIThread.UnhandledException += ReportUnhandled;
            try
            {
                control.Revision = _second;
                await WaitUntilAsync(owner, () => errors.Count > 0);
                errors.Should().ContainSingle().Which.Should().BeSameAs(expected);
                control.Revision = _first;
                await WaitUntilAsync(owner, () => IsSettled(control, "First revision message"));
                errors.Should().ContainSingle();
                dispatcherErrors.Should().BeEmpty("resize handlers must report errors before they escape to the dispatcher");
            }
            finally
            {
                row.PropertyChanged -= RejectHeight;
                WinFormsShims.Application.ThreadException -= Report;
                Dispatcher.UIThread.UnhandledException -= ReportUnhandled;
            }
        }
        finally
        {
            owner.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private CommitInfoOwner CreateOwner(CommitInfo control)
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        if (!_context.IsOnMainThread)
        {
            _context.Dispose();
            _context = new JoinableTaskContext();
            ThreadHelper.JoinableTaskContext = _context;
        }

        return new CommitInfoOwner(new GitUICommands(_services, _module)) { Width = 600, Height = 500, Content = control };
    }

    private GitRevision AddRevision(string body, string tag)
    {
        File.AppendAllText(Path.Combine(_directory, "tracked.txt"), tag);
        _module.GitExecutable.RunCommand("add -- tracked.txt").Should().BeTrue();
        _module.GitExecutable.RunCommand("commit --quiet -F -", input: Encoding.UTF8.GetBytes(body)).Should().BeTrue();
        _module.GitExecutable.RunCommand(new GitArgumentBuilder("tag") { tag }).Should().BeTrue();
        return _module.GetRevision(_module.GetCurrentCheckout());
    }

    private static bool IsSettled(CommitInfo control, string expectedBody)
    {
        CommitInfoTestSupport.Accessor accessor = control.GetNativeTestSupport();
        return accessor.CommitMessage.GetPlainText().Contains(expectedBody, StringComparison.Ordinal)
            && accessor.RevisionInfo.GetPlainText().Contains("second", StringComparison.Ordinal)
            && accessor.CommitMessage.ContentsHeight > 0
            && accessor.RevisionInfo.ContentsHeight > 0
            && accessor.CommitMessageHeight == accessor.CommitMessage.ContentsHeight
            && accessor.RevisionInfoHeight == accessor.RevisionInfo.ContentsHeight;
    }

    private static Delegate? GetContentsHandlers(XhtmlTextBlock block)
        => (Delegate?)typeof(XhtmlTextBlock).GetField("ContentsResized", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(block);

    private static async Task WaitUntilAsync(Window owner, Func<bool> condition)
    {
        Stopwatch watch = Stopwatch.StartNew();
        while (!condition() && watch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            owner.UpdateLayout();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the real revision rendering and throttled layout should settle");
    }

    // parity-scaffolding: host CommitInfo under a real command-owning form without starting Browse's unrelated readers.
    private sealed class CommitInfoOwner(IGitUICommands commands) : GitModuleForm(commands, enablePositionRestore: false);
}
