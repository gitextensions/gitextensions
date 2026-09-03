using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class VerifyTests
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

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.VerifyTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormVerify_should_construct_with_the_original_filters_and_context_actions()
    {
        FormVerify form = new();
        FormVerify.TestAccessor accessor = form.GetTestAccessor();
        accessor.SetPreviewRows();

        accessor.ShowCommits.IsChecked.Should().BeTrue();
        accessor.ShowOther.IsChecked.Should().BeFalse();
        accessor.NoReflogs.IsChecked.Should().BeTrue();
        accessor.Warnings.ItemCount.Should().Be(2);
        accessor.CreateTag.IsEnabled.Should().BeTrue();
        accessor.CreateBranch.IsEnabled.Should().BeTrue();
        accessor.SaveAs.IsEnabled.Should().BeFalse();
        accessor.Close.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormVerify_should_filter_rows_and_keep_at_least_one_object_kind_visible()
    {
        FormVerify form = new();
        FormVerify.TestAccessor accessor = form.GetTestAccessor();
        accessor.SetPreviewRows();

        accessor.ShowOther.IsChecked = true;
        accessor.Warnings.ItemCount.Should().Be(3);

        accessor.ShowCommits.IsChecked = false;
        accessor.Warnings.ItemCount.Should().Be(1);
        accessor.RawTypes.Should().ContainSingle().Which.Should().StartWith("dangling blob");
        accessor.SaveAs.IsEnabled.Should().BeTrue();

        accessor.ShowOther.IsChecked = false;
        accessor.ShowCommits.IsChecked.Should().BeTrue();
        accessor.Warnings.ItemCount.Should().Be(2);
    }

    [AvaloniaTest]
    public void FormVerify_should_build_fsck_options_sort_rows_and_select_the_filtered_set()
    {
        FormVerify form = new();
        FormVerify.TestAccessor accessor = form.GetTestAccessor();
        accessor.SetPreviewRows();
        accessor.FullCheck.IsChecked = true;
        accessor.Unreachable.IsChecked = true;

        accessor.Options.Should().Be(" --unreachable --full --no-reflogs");

        accessor.SortByType();
        accessor.RawTypes.Should().BeInAscendingOrder(StringComparer.Ordinal);

        accessor.SelectAll(selected: true);
        accessor.SelectedCount.Should().Be(2);
    }

    [AvaloniaTest]
    public void FormVerify_should_open_the_selected_object_in_the_read_only_FormEdit_twin()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.ShowObject(Arg.Any<ObjectId>(), Arg.Any<bool>()).Returns("recovered object content");
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormVerify form = new(commands);
        FormVerify.TestAccessor accessor = form.GetTestAccessor();
        accessor.SetPreviewRows();

        FormEdit? view = accessor.CreateCurrentItemView();
        view.Should().NotBeNull();
        using FormEdit actualView = view!;

        actualView.IsReadOnly.Should().BeTrue();
        actualView.GetTestAccessor().Viewer.GetText().Should().Be("recovered object content");
    }

    [AvaloniaTest]
    public void FormVerify_should_parse_unreachable_objects_from_a_real_repository()
    {
        GitModule module = CreateRepositoryWithUnreachableCommit();
        ObjectId unreachableCommit = module.RevParse("HEAD@{1}");
        string output = module.GitExecutable.GetOutput("fsck-objects --unreachable --no-reflogs");

        IReadOnlyList<(string RawType, string ObjectId, string? Subject)> objects = FormVerify.TestAccessor.Parse(module, output);

        objects.Should().Contain(item => item.RawType == "unreachable commit"
            && item.ObjectId == unreachableCommit.ToString()
            && item.Subject == "lost change");
        objects.Should().Contain(item => item.RawType == "unreachable blob");
        objects.Should().Contain(item => item.RawType == "unreachable tree");
    }

    [AvaloniaTest]
    public void FormVerify_should_use_the_existing_translation_keys_once()
    {
        FormVerify form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormVerify), "$this", "Text", "Verify database");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "btnCloseDialog", "Text", "Cancel");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "btnRestoreSelectedObjects", "Text", "Recover selected objects");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "DeleteAllLostAndFoundTags", "Text", "Delete all LOST_AND_FOUND tags");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "Remove", "Text", "Remove all dangling objects");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "SaveObjects", "Text", "Save objects to .git/lost-found");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "ShowCommitsAndTags", "Text", "Show commits and annotated tags");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "ShowOtherObjects", "Text", "Show blobs and trees");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "ShowOtherObjects", "toolTip", "To recover contents of files once staged but mistakenly deleted");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "columnDate", "HeaderText", "Date");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "columnType", "HeaderText", "Type");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "columnSubject", "HeaderText", "Subject");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "columnAuthor", "HeaderText", "Author");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "columnHash", "HeaderText", "Hash");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "columnParent", "HeaderText", "Parent(s) hashs");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "mnuLostObjectView", "Text", "View");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "mnuLostObjectsCreateTag", "Text", "Create tag");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "mnuLostObjectsCreateBranch", "Text", "Create branch");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "copyHashToolStripMenuItem", "Text", "Copy object hash");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "copyParentHashToolStripMenuItem", "Text", "Copy parent hash");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "saveAsToolStripMenuItem", "Text", "Save as...");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "_removeDanglingObjectsCaption", "Text", "Remove");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "_selectLostObjectsToRestoreCaption", "Text", "Restore lost objects");
        translation.Received(1).AddTranslationItem(nameof(FormVerify), "_seemingly", "Text", "seemingly");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    private GitModule CreateRepositoryWithUnreachableCommit()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        string filePath = Path.Combine(_workingDirectory, "content.txt");
        File.WriteAllText(filePath, "reachable");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "content.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        File.WriteAllText(filePath, "unreachable");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "lost change".Quote() }).Should().BeTrue();
        module.GitExecutable.RunCommand("reset --hard HEAD~1").Should().BeTrue();
        return module;
    }
}
