using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using GitCommands;
using GitCommands.Git;
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
public sealed class RevertCommitTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        System.IO.Abstractions.FileSystem fileSystem = new();
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(new GitDirectoryResolver(fileSystem));
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.RevertCommitTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormRevertCommit_should_construct_with_the_original_controls()
    {
        FormRevertCommit form = new();
        FormRevertCommit.TestAccessor accessor = form.GetTestAccessor();

        accessor.Revert.Should().NotBeNull();
        accessor.Abort.Should().NotBeNull();
        accessor.ParentsVisible.Should().BeFalse();
        accessor.BuildRevertCommand().Should().BeNull();
    }

    [AvaloniaTest]
    public void FormRevertCommit_should_build_the_original_non_merge_options()
    {
        GitRevision revision = new(ObjectId.Parse("0123456789abcdef0123456789abcdef01234567"));
        FormRevertCommit form = CreateForm(revision, isMerge: false);
        FormRevertCommit.TestAccessor accessor = form.GetTestAccessor();
        accessor.Load();
        accessor.AutoCommit.IsChecked = false;

        accessor.BuildRevertCommand().Should().Be(Commands.Revert(revision.ObjectId, autoCommit: false, parentIndex: 0));
        accessor.ParentsVisible.Should().BeFalse();

        accessor.AutoCommit.IsChecked = true;
        accessor.BuildRevertCommand().Should().Be(Commands.Revert(revision.ObjectId, autoCommit: true, parentIndex: 0));
    }

    [AvaloniaTest]
    public void FormRevertCommit_should_require_and_use_the_selected_merge_parent()
    {
        GitRevision revision = new(ObjectId.Parse("89abcdef0123456789abcdef0123456789abcdef"));
        GitRevision firstParent = new(ObjectId.Parse("3333333333333333333333333333333333333333"))
        {
            Author = "First Author",
            Subject = "First parent",
        };
        GitRevision secondParent = new(ObjectId.Parse("4444444444444444444444444444444444444444"))
        {
            Author = "Second Author",
            Subject = "Second parent",
        };
        FormRevertCommit form = CreateForm(revision, isMerge: true, [firstParent, secondParent]);
        FormRevertCommit.TestAccessor accessor = form.GetTestAccessor();
        accessor.Load();

        accessor.ParentsVisible.Should().BeTrue();
        accessor.Parents.ItemCount.Should().Be(2);
        accessor.Parents.SelectedIndex.Should().Be(0);
        accessor.Parents.SelectedIndex = 1;
        accessor.BuildRevertCommand().Should().Be(Commands.Revert(revision.ObjectId, autoCommit: false, parentIndex: 2));

        accessor.Parents.SelectedIndex = -1;
        accessor.BuildRevertCommand().Should().BeNull();
    }

    [AvaloniaTest]
    public void FormRevertCommit_abort_should_return_cancel()
    {
        FormRevertCommit form = new();

        form.GetTestAccessor().Abort.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        form.DialogResult.Should().Be(WinFormsShims.DialogResult.Cancel);
    }

    [AvaloniaTest]
    public void FormRevertCommit_should_revert_a_real_commit()
    {
        (GitModule module, GitRevision revision) = CreateRepositoryWithRevertTarget();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormRevertCommit form = new(commands, revision);
        FormRevertCommit.TestAccessor accessor = form.GetTestAccessor();
        accessor.Load();
        accessor.AutoCommit.IsChecked = true;
        ArgumentString command = accessor.BuildRevertCommand()!.Value;

        module.GitExecutable.RunCommand(command).Should().BeTrue(command.ToString());

        File.ReadAllText(Path.Combine(_workingDirectory, "content.txt")).Should().Be("before");
        module.GitExecutable.GetOutput(new GitArgumentBuilder("rev-list") { "--count", "HEAD" }).Trim()
            .Should().Be("3");
    }

    [AvaloniaTest]
    public void Revision_grid_should_reuse_the_existing_revert_translation_key()
    {
        RevisionGridControl control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(RevisionGridControl),
            "revertCommitToolStripMenuItem",
            "Text",
            "Re&vert this commit...");
    }

    [AvaloniaTest]
    public void FormRevertCommit_should_use_the_existing_translation_keys_once()
    {
        FormRevertCommit form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormRevertCommit), "$this", "Text", "Revert commit");
        translation.Received(1).AddTranslationItem(nameof(FormRevertCommit), "AutoCommit", "Text", "&Automatically create a commit");
        translation.Received(1).AddTranslationItem(nameof(FormRevertCommit), "BranchInfo", "Text", "Revert this commit:");
        translation.Received(1).AddTranslationItem(nameof(FormRevertCommit), "ParentsLabel", "Text", "This commit is a merge, select &parent:");
        translation.Received(1).AddTranslationItem(nameof(FormRevertCommit), "Revert", "Text", "&Revert this commit");
        translation.Received(1).AddTranslationItem(nameof(FormRevertCommit), "_noneParentSelectedText", "Text", "None parent is selected!");
        translation.Received(1).AddTranslationItem(nameof(FormRevertCommit), "btnAbort", "Text", "A&bort");
        translation.Received(1).AddTranslationItem(nameof(FormRevertCommit), "columnHeader1", "Text", "No.");
        translation.Received(1).AddTranslationItem(nameof(FormRevertCommit), "columnHeader2", "Text", "Message");
        translation.Received(1).AddTranslationItem(nameof(FormRevertCommit), "columnHeader3", "Text", "Author");
        translation.Received(1).AddTranslationItem(nameof(FormRevertCommit), "columnHeader4", "Text", "Date");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    private FormRevertCommit CreateForm(
        GitRevision revision,
        bool isMerge,
        IReadOnlyList<GitRevision>? parents = null)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDirectory);
        module.IsMerge(revision.ObjectId).Returns(isMerge);
        module.GetParentRevisions(revision.ObjectId).Returns(parents ?? []);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return new FormRevertCommit(commands, revision);
    }

    private (GitModule Module, GitRevision Revision) CreateRepositoryWithRevertTarget()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        string filePath = Path.Combine(_workingDirectory, "content.txt");
        File.WriteAllText(filePath, "before");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "content.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        File.WriteAllText(filePath, "after");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "change" }).Should().BeTrue();
        GitRevision revision = module.GetRevision(module.GetCurrentCheckout(), shortFormat: true, loadRefs: false);
        return (module, revision);
    }
}
