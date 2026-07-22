using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
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
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class ReflogTests
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
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(new GitDirectoryResolver(fileSystem));
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.ReflogTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormReflog_should_construct_with_the_original_grid_and_available_actions()
    {
        FormReflog form = new();
        FormReflog.TestAccessor accessor = form.GetTestAccessor();

        accessor.Branches.Should().NotBeNull();
        accessor.Reflog.Should().NotBeNull();
        accessor.Copy.IsEnabled.Should().BeFalse();
        accessor.CreateBranch.IsEnabled.Should().BeFalse();
        accessor.Reset.IsVisible.Should().BeTrue();
        accessor.Reset.IsEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormReflog_should_parse_only_complete_native_reflog_rows()
    {
        ObjectId first = ObjectId.Parse("1111111111111111111111111111111111111111");
        ObjectId second = ObjectId.Parse("2222222222222222222222222222222222222222");
        string output = $"{first} HEAD@{{0}}: commit: second\ninvalid row\n{second} HEAD@{{1}}: checkout: moving from feature to main\n";

        IReadOnlyList<RefLine> rows = FormReflog.TestAccessor.Parse(output);

        rows.Should().HaveCount(2);
        rows[0].Sha.Should().Be(first);
        rows[0].Ref.Should().Be("HEAD@{0}");
        rows[0].Action.Should().Be("commit: second");
        rows[1].Sha.Should().Be(second);
        rows[1].Action.Should().Be("checkout: moving from feature to main");
    }

    [AvaloniaTest]
    public void FormReflog_should_route_create_branch_for_the_selected_entry()
    {
        ObjectId first = ObjectId.Parse("1111111111111111111111111111111111111111");
        ObjectId second = ObjectId.Parse("2222222222222222222222222222222222222222");
        IGitModule module = Substitute.For<IGitModule>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormReflog form = new(commands);
        FormReflog.TestAccessor accessor = form.GetTestAccessor();
        RefLine[] rows =
        [
            new(first, "HEAD@{0}", "commit: second"),
            new(second, "HEAD@{1}", "commit: initial"),
        ];
        accessor.SetRefLines(rows, rows[1]);

        accessor.CreateBranch.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        accessor.Copy.IsEnabled.Should().BeTrue();
        accessor.CreateBranch.IsEnabled.Should().BeTrue();
        commands.Received(1).StartCreateBranchDialog(form, second, null);
    }

    [AvaloniaTest]
    public void FormReflog_should_warn_before_resetting_a_dirty_checked_out_branch()
    {
        StubMessageBoxHost messageBoxes = new() { Result = WinFormsShims.DialogResult.No };
        WinFormsShims.ShimHost.MessageBoxHost = messageBoxes;
        ObjectId objectId = ObjectId.Parse("1111111111111111111111111111111111111111");
        IGitModule module = Substitute.For<IGitModule>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormReflog form = new(commands);
        FormReflog.TestAccessor accessor = form.GetTestAccessor();
        RefLine refLine = new(objectId, "HEAD@{0}", "commit: second");
        accessor.SetRefLines([refLine], refLine);
        accessor.SetRepositoryState(isDirtyDir: true, "main");

        accessor.Reset.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        accessor.DirtyWorkingDirectory.IsVisible.Should().BeTrue();
        accessor.Reset.IsEnabled.Should().BeTrue();
        accessor.ResetType.Should().Be(FormResetCurrentBranch.ResetType.Soft);
        messageBoxes.Messages.Should().ContainSingle().Which.Should().Be(
            "You have changes in your working directory that could be lost.\n\nDo you want to continue?");
        commands.DidNotReceive().DoActionOnRepo(Arg.Any<Func<bool>>());
    }

    [AvaloniaTest]
    public void FormReflog_should_default_a_clean_reset_to_hard_and_disable_detached_head()
    {
        FormReflog form = new();
        FormReflog.TestAccessor accessor = form.GetTestAccessor();
        RefLine refLine = new(ObjectId.Random(), "HEAD@{0}", "commit: second");
        accessor.SetRefLines([refLine], refLine);

        accessor.SetRepositoryState(isDirtyDir: false, "main");

        accessor.DirtyWorkingDirectory.IsVisible.Should().BeFalse();
        accessor.Reset.IsEnabled.Should().BeTrue();
        accessor.ResetType.Should().Be(FormResetCurrentBranch.ResetType.Hard);

        accessor.SetRepositoryState(isDirtyDir: false, DetachedHeadParser.DetachedBranch);
        accessor.Reset.IsEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormReflog_should_sort_headers_in_both_directions_and_keep_selection()
    {
        RefLine first = new(ObjectId.Parse("1111111111111111111111111111111111111111"), "HEAD@{2}", "third");
        RefLine second = new(ObjectId.Parse("2222222222222222222222222222222222222222"), "HEAD@{0}", "first");
        RefLine third = new(ObjectId.Parse("3333333333333333333333333333333333333333"), "HEAD@{1}", "second");
        FormReflog form = new();
        FormReflog.TestAccessor accessor = form.GetTestAccessor();
        accessor.SetRefLines([first, second, third], third);

        accessor.SortByRef();

        accessor.RefLines.Select(refLine => refLine.Ref).Should().Equal("HEAD@{0}", "HEAD@{1}", "HEAD@{2}");
        accessor.Reflog.SelectedItem.Should().BeSameAs(third);

        accessor.SortByRef();

        accessor.RefLines.Select(refLine => refLine.Ref).Should().Equal("HEAD@{2}", "HEAD@{1}", "HEAD@{0}");
        accessor.Reflog.SelectedItem.Should().BeSameAs(third);
    }

    [AvaloniaTest]
    public void FormReflog_should_use_the_existing_translation_keys_once()
    {
        FormReflog form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormReflog), "$this", "Text", "Reflog");
        translation.Received(1).AddTranslationItem(nameof(FormReflog), "Action", "HeaderText", "Action");
        translation.Received(1).AddTranslationItem(nameof(FormReflog), "Ref", "HeaderText", "Ref");
        translation.Received(1).AddTranslationItem(nameof(FormReflog), "Sha", "HeaderText", "SHA-1");
        translation.Received(1).AddTranslationItem(nameof(FormReflog), "_continueResetCurrentBranchCaptionText", "Text", "Changes not committed...");
        translation.Received(1).AddTranslationItem(
            nameof(FormReflog),
            "_continueResetCurrentBranchEvenWithChangesText",
            "Text",
            "You have changes in your working directory that could be lost.\n\nDo you want to continue?");
        translation.Received(1).AddTranslationItem(nameof(FormReflog), "copySha1ToolStripMenuItem", "Text", "Copy SHA-1");
        translation.Received(1).AddTranslationItem(nameof(FormReflog), "createABranchOnThisCommitToolStripMenuItem", "Text", "Create a branch on this commit...");
        translation.Received(1).AddTranslationItem(nameof(FormReflog), "label1", "Text", "Display reflog for:");
        translation.Received(1).AddTranslationItem(nameof(FormReflog), "label2", "Text", "Reference:");
        translation.Received(1).AddTranslationItem(
            nameof(FormReflog),
            "lblDirtyWorkingDirectory",
            "Text",
            "Warning: you've got changes in your working directory that could be lost if you want to reset the current branch to another commit.\nStash them before if you don't want to lose them.");
        translation.Received(1).AddTranslationItem(nameof(FormReflog), "linkCurrentBranch", "Text", "(Current branch name)");
        translation.Received(1).AddTranslationItem(nameof(FormReflog), "linkHead", "Text", "HEAD");
        translation.Received(1).AddTranslationItem(nameof(FormReflog), "resetCurrentBranchOnThisCommitToolStripMenuItem", "Text", "Reset current branch to this commit...");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public async Task FormReflog_should_load_head_and_named_references_from_a_real_repository()
    {
        GitModule module = CreateRepositoryWithTwoCommits();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormReflog form = new(commands);
        try
        {
            form.Show();
            FormReflog.TestAccessor accessor = form.GetTestAccessor();

            await WaitUntilAsync(() => accessor.RefLines.Count >= 2);

            accessor.Branches.Items.Cast<string>().Should().Contain(["HEAD", module.GetSelectedBranch()]);
            accessor.RefLines[0].Sha.Should().Be(module.GetCurrentCheckout());
            accessor.RefLines.Should().OnlyContain(refLine => !string.IsNullOrWhiteSpace(refLine.Action));
        }
        finally
        {
            form.Close();
        }
    }

    private GitModule CreateRepositoryWithTwoCommits()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        string fileName = Path.Combine(_workingDirectory, "tracked.txt");
        File.WriteAllText(fileName, "first");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });
        File.AppendAllText(fileName, "second");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });
        return module;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }

            await Task.Delay(20);
        }

        Assert.Fail("Timed out waiting for the reflog dialog to load.");
    }

    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<string> Messages { get; } = [];
        public WinFormsShims.DialogResult Result { get; set; }

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            return Result;
        }
    }
}
