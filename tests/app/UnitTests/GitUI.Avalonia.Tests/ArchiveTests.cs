using System.ComponentModel.Design;
using System.IO.Compression;
using Avalonia.Headless.NUnit;
using Avalonia.Platform.Storage;
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

namespace GitExtensionsTests;

[TestFixture]
public sealed class ArchiveTests
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

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.ArchiveTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormArchive_should_construct_with_zip_and_no_file_filter_selected()
    {
        FormArchive form = new();
        FormArchive.TestAccessor accessor = form.GetTestAccessor();

        accessor.Archive.Should().NotBeNull();
        accessor.PathFilter.IsChecked.Should().BeFalse();
        accessor.Paths.IsEnabled.Should().BeFalse();
        accessor.RevisionFilter.IsChecked.Should().BeFalse();
        accessor.ChooseDiffRevision.IsEnabled.Should().BeFalse();
        accessor.Tar.IsChecked.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormArchive_should_keep_path_and_revision_filters_mutually_exclusive()
    {
        FormArchive form = new();
        FormArchive.TestAccessor accessor = form.GetTestAccessor();

        accessor.PathFilter.IsChecked = true;

        accessor.Paths.IsEnabled.Should().BeTrue();
        accessor.RevisionFilter.IsChecked.Should().BeFalse();

        accessor.RevisionFilter.IsChecked = true;

        accessor.PathFilter.IsChecked.Should().BeFalse();
        accessor.Paths.IsEnabled.Should().BeFalse();
        accessor.ChooseDiffRevision.IsEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormArchive_should_show_the_selected_comparison_revision()
    {
        GitRevision revision = new(ObjectId.Parse("1111111111111111111111111111111111111111"))
        {
            Author = "Archive Author",
            Subject = "Archive base",
        };
        FormArchive form = new();
        FormArchive.TestAccessor accessor = form.GetTestAccessor();

        form.SetDiffSelectedRevision(revision);

        accessor.RevisionFilter.IsChecked.Should().BeTrue();
        accessor.DiffHeader.Should().Be(revision.ObjectId.ToShortString());
        accessor.DiffAuthor.Should().Be(revision.Author);
        accessor.DiffMessage.Should().Be(revision.Subject);
    }

    [AvaloniaTest]
    public void FormArchive_should_build_platform_agnostic_path_arguments_and_picker_options()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDirectory);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        GitRevision revision = new(ObjectId.Parse("2222222222222222222222222222222222222222"));
        FormArchive form = new(commands) { SelectedRevision = revision };
        FormArchive.TestAccessor accessor = form.GetTestAccessor();
        form.SetPathArgument("first path.txt\r\nsecond.txt\nthird path.txt");

        string pathArgument = accessor.GetPathArgument();
        FilePickerSaveOptions zipOptions = accessor.CreateSaveFileOptions();

        pathArgument.Should().Be("\"first path.txt\" \"second.txt\" \"third path.txt\"");
        zipOptions.Title.Should().Be("Save archive as");
        zipOptions.DefaultExtension.Should().Be("zip");
        zipOptions.FileTypeChoices.Should().ContainSingle().Which.Patterns.Should().Equal("*.zip");
        accessor.Tar.IsChecked = true;
        accessor.CreateSaveFileOptions().DefaultExtension.Should().Be("tar");
        accessor.BuildArchiveArguments(Path.Combine(_workingDirectory, "archive output.tar"))
            .Should().Contain("archive --format=\"tar\"")
            .And.Contain($"--output \"{Path.Combine(_workingDirectory, "archive output.tar")}\"");
    }

    [AvaloniaTest]
    public void FormArchive_should_filter_changed_paths_and_exclude_deleted_files()
    {
        GitModule module = CreateRepositoryWithTwoCommits();
        GitRevision current = module.GetRevision(module.GetCurrentCheckout(), shortFormat: true, loadRefs: false);
        GitRevision parent = module.GetRevision(current.ParentIds!.Single(), shortFormat: true, loadRefs: false);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormArchive form = new(commands) { SelectedRevision = current };
        form.SetDiffSelectedRevision(parent);

        string pathArgument = form.GetTestAccessor().GetPathArgument();

        pathArgument.Should().Contain("\"tracked.txt\"");
        pathArgument.Should().Contain("\"added.txt\"");
        pathArgument.Should().NotContain("deleted.txt");
    }

    [AvaloniaTest]
    public void FormArchive_should_create_a_real_filtered_zip_archive()
    {
        GitModule module = CreateRepositoryWithTwoCommits();
        GitRevision current = module.GetRevision(module.GetCurrentCheckout(), shortFormat: true, loadRefs: false);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormArchive form = new(commands) { SelectedRevision = current };
        form.SetPathArgument("tracked.txt");
        string archivePath = Path.Combine(_workingDirectory, "filtered archive.zip");
        string arguments = form.GetTestAccessor().BuildArchiveArguments(archivePath);

        module.GitExecutable.RunCommand(arguments).Should().BeTrue(arguments);

        using ZipArchive archive = ZipFile.OpenRead(archivePath);
        archive.Entries.Select(entry => entry.FullName).Should().Equal("tracked.txt");
    }

    [AvaloniaTest]
    public void FormArchive_should_use_the_existing_translation_keys_once()
    {
        FormArchive form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormArchive), "$this", "Text", "Archive");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "_noRevisionSelected", "Text", "You need to choose a target revision.");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "_saveFileDialogCaption", "Text", "Save archive as");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "_saveFileDialogFilterTar", "Text", "Tar file (*.tar)");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "_saveFileDialogFilterZip", "Text", "Zip file (*.zip)");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "buttonArchiveRevision", "Text", "Save as...");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "checkBoxPathFilter", "Text", "Archive specific paths only");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "checkboxRevisionFilter", "Text", "Take the files that have changed from the revision above to this one and archive only those");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "groupBox1", "Text", "Archive format");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "groupBox2", "Text", "Filter files");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "label1", "Text", "This revision will be archived:");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "label2", "Text", "Choose another\nrevision:");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "label4", "Text", "separate each new path by new line");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "labelAuthorCaption", "Text", "Author:");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "labelDateCaption", "Text", "Commit date:");
        translation.Received(1).AddTranslationItem(nameof(FormArchive), "lblChooseDiffRevision", "Text", "Choose revision to \ncompare with first:");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    private GitModule CreateRepositoryWithTwoCommits()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "first");
        File.WriteAllText(Path.Combine(_workingDirectory, "deleted.txt"), "remove me");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt", "deleted.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second");
        File.WriteAllText(Path.Combine(_workingDirectory, "added.txt"), "added");
        File.Delete(Path.Combine(_workingDirectory, "deleted.txt"));
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--all" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "archive target".Quote() }).Should().BeTrue();
        return module;
    }
}
