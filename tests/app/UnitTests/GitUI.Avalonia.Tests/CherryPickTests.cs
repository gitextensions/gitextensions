using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
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
public sealed class CherryPickTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private bool _originalAddReference;
    private bool _originalAutoCommit;

    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _originalAutoCommit = AppSettings.CommitAutomaticallyAfterCherryPick;
        _originalAddReference = AppSettings.AddCommitReferenceToCherryPick;

        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        System.IO.Abstractions.FileSystem fileSystem = new();
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(new GitDirectoryResolver(fileSystem));
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.CherryPickTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.CommitAutomaticallyAfterCherryPick = _originalAutoCommit;
        AppSettings.AddCommitReferenceToCherryPick = _originalAddReference;
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormCherryPick_should_construct_with_the_original_controls()
    {
        FormCherryPick form = new();
        FormCherryPick.TestAccessor accessor = form.GetTestAccessor();

        accessor.Pick.Should().NotBeNull();
        accessor.Abort.Should().NotBeNull();
        accessor.ParentsVisible.Should().BeFalse();
        accessor.BuildCherryPickCommand().Should().BeNull();
    }

    [AvaloniaTest]
    public void FormCherryPick_should_load_and_save_options_only_after_success()
    {
        AppSettings.CommitAutomaticallyAfterCherryPick = false;
        AppSettings.AddCommitReferenceToCherryPick = false;
        FormCherryPick form = CreateForm(new GitRevision(ObjectId.Random()), isMerge: false);
        FormCherryPick.TestAccessor accessor = form.GetTestAccessor();

        accessor.Load();
        accessor.AutoCommit.IsChecked.Should().BeFalse();
        accessor.AddReference.IsChecked.Should().BeFalse();
        accessor.AutoCommit.IsChecked = true;
        accessor.AddReference.IsChecked = true;
        form.DialogResult = WinFormsShims.DialogResult.Cancel;
        accessor.Save();
        AppSettings.CommitAutomaticallyAfterCherryPick.Should().BeFalse();
        AppSettings.AddCommitReferenceToCherryPick.Should().BeFalse();

        form.DialogResult = WinFormsShims.DialogResult.OK;
        accessor.Save();
        AppSettings.CommitAutomaticallyAfterCherryPick.Should().BeTrue();
        AppSettings.AddCommitReferenceToCherryPick.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormCherryPick_should_build_the_original_non_merge_options()
    {
        GitRevision revision = new(ObjectId.Parse("0123456789abcdef0123456789abcdef01234567"));
        FormCherryPick form = CreateForm(revision, isMerge: false);
        FormCherryPick.TestAccessor accessor = form.GetTestAccessor();
        accessor.Load();
        accessor.AutoCommit.IsChecked = false;
        accessor.AddReference.IsChecked = true;

        ArgumentString? command = accessor.BuildCherryPickCommand();

        command.Should().Be(Commands.CherryPick(revision.ObjectId, commit: false, "-x"));
        accessor.ParentsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormCherryPick_should_require_and_use_the_selected_merge_parent()
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
        FormCherryPick form = CreateForm(revision, isMerge: true, [firstParent, secondParent]);
        FormCherryPick.TestAccessor accessor = form.GetTestAccessor();
        accessor.Load();

        accessor.ParentsVisible.Should().BeTrue();
        accessor.Parents.ItemCount.Should().Be(2);
        accessor.Parents.SelectedIndex.Should().Be(0);
        accessor.Parents.SelectedIndex = 1;
        accessor.BuildCherryPickCommand().Should().Be(
            Commands.CherryPick(revision.ObjectId, commit: false, "-m 2"));

        accessor.Parents.SelectedIndex = -1;
        accessor.BuildCherryPickCommand().Should().BeNull();
    }

    [AvaloniaTest]
    public void FormCherryPick_should_copy_options_between_sequential_dialogs()
    {
        FormCherryPick source = new();
        FormCherryPick target = new();
        source.GetTestAccessor().AutoCommit.IsChecked = true;
        source.GetTestAccessor().AddReference.IsChecked = true;

        target.CopyOptions(source);

        target.GetTestAccessor().AutoCommit.IsChecked.Should().BeTrue();
        target.GetTestAccessor().AddReference.IsChecked.Should().BeTrue();
    }

    [AvaloniaTest]
    public void Revision_grid_should_return_multi_selection_in_cherry_pick_order()
    {
        RevisionGridControl control = new();
        ListBox revisions = control.FindControl<ListBox>("lstRevisions")!;
        GitRevision newest = new(ObjectId.Parse("1111111111111111111111111111111111111111"));
        GitRevision middle = new(ObjectId.Parse("2222222222222222222222222222222222222222"));
        GitRevision oldest = new(ObjectId.Parse("3333333333333333333333333333333333333333"));
        revisions.ItemsSource = new[] { newest, middle, oldest };
        revisions.SelectedItems!.Add(newest);
        revisions.SelectedItems.Add(oldest);

        revisions.SelectionMode.Should().Be(SelectionMode.Multiple);
        control.GetSelectedRevisions(SortDirection.Ascending).Should().Equal(newest, oldest);
        control.GetSelectedRevisions(SortDirection.Descending).Should().Equal(oldest, newest);
    }

    [AvaloniaTest]
    public void Revision_grid_should_reuse_the_existing_cherry_pick_translation_key()
    {
        RevisionGridControl control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(RevisionGridControl),
            "cherryPickCommitToolStripMenuItem",
            "Text",
            "Cherr&y pick this commit...");
    }

    [AvaloniaTest]
    public void FormCherryPick_should_create_a_real_commit_from_the_selected_revision()
    {
        (GitModule module, GitRevision sourceRevision) = CreateRepositoryWithCherryPickSource();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormCherryPick form = new(commands, sourceRevision);
        FormCherryPick.TestAccessor accessor = form.GetTestAccessor();
        accessor.Load();
        accessor.AutoCommit.IsChecked = true;
        ArgumentString command = accessor.BuildCherryPickCommand()!.Value;

        module.GitExecutable.RunCommand(command).Should().BeTrue(command.ToString());

        File.ReadAllText(Path.Combine(_workingDirectory, "picked.txt")).Should().Be("picked");
        module.GetSelectedBranch().Should().Be("main");
        module.GitExecutable.GetOutput(new GitArgumentBuilder("log") { "-1", "--format=%s" }).Trim()
            .Should().Be("pick source");
    }

    [AvaloniaTest]
    public void FormCherryPick_should_use_the_existing_translation_keys_once()
    {
        FormCherryPick form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormCherryPick), "$this", "Text", "Cherry pick commit");
        translation.Received(1).AddTranslationItem(nameof(FormCherryPick), "_noneParentSelectedText", "Text", "None parent is selected!");
        translation.Received(1).AddTranslationItem(nameof(FormCherryPick), "btnAbort", "Text", "A&bort");
        translation.Received(1).AddTranslationItem(nameof(FormCherryPick), "btnPick", "Text", "&Cherry pick");
        translation.Received(1).AddTranslationItem(nameof(FormCherryPick), "cbxAddReference", "Text", "A&dd commit reference to commit message");
        translation.Received(1).AddTranslationItem(nameof(FormCherryPick), "cbxAutoCommit", "Text", "&Automatically create a commit");
        translation.Received(1).AddTranslationItem(nameof(FormCherryPick), "columnHeader1", "Text", "No.");
        translation.Received(1).AddTranslationItem(nameof(FormCherryPick), "columnHeader2", "Text", "Message");
        translation.Received(1).AddTranslationItem(nameof(FormCherryPick), "columnHeader3", "Text", "Author");
        translation.Received(1).AddTranslationItem(nameof(FormCherryPick), "columnHeader4", "Text", "Date");
        translation.Received(1).AddTranslationItem(nameof(FormCherryPick), "lblAnotherRev", "Text", "C&hoose another revision:");
        translation.Received(1).AddTranslationItem(nameof(FormCherryPick), "lblBranchInfo", "Text", "Cherry pick this commit:");
        translation.Received(1).AddTranslationItem(nameof(FormCherryPick), "lblParents", "Text", "This commit is a merge, select &parent:");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    private FormCherryPick CreateForm(
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
        return new FormCherryPick(commands, revision);
    }

    private (GitModule Module, GitRevision SourceRevision) CreateRepositoryWithCherryPickSource()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "base.txt"), "base");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "base.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "-b", "pick-source" }).Should().BeTrue();
        File.WriteAllText(Path.Combine(_workingDirectory, "picked.txt"), "picked");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "picked.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "pick source".Quote() }).Should().BeTrue();
        GitRevision sourceRevision = module.GetRevision(module.GetCurrentCheckout(), shortFormat: true, loadRefs: false);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "main" }).Should().BeTrue();
        return (module, sourceRevision);
    }
}
