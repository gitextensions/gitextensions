using System.ComponentModel.Design;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
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
[NonParallelizable]
public sealed class MergeSubmoduleTests
{
    private static readonly ObjectId BaseId = ObjectId.Parse("1111111111111111111111111111111111111111");
    private static readonly ObjectId LocalId = ObjectId.Parse("2222222222222222222222222222222222222222");
    private static readonly ObjectId RemoteId = ObjectId.Parse("3333333333333333333333333333333333333333");

    private ServiceContainer _serviceContainer = null!;
    private string _testRoot = null!;

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
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _testRoot = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.MergeSubmoduleTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testRoot);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_testRoot);
    }

    [AvaloniaTest]
    public void FormMergeSubmodule_should_construct_and_use_existing_translation_keys()
    {
        FormMergeSubmodule form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormMergeSubmodule), "$this", "Text", "Submodule conflict");
        translation.Received(1).AddTranslationItem(nameof(FormMergeSubmodule), "_deleted", "Text", "deleted");
        translation.Received(1).AddTranslationItem(nameof(FormMergeSubmodule), "_stageFilename", "Text", "Stage {0}");
        translation.Received(1).AddTranslationItem(nameof(FormMergeSubmodule), "btCheckoutBranch", "Text", "Checkout Branch");
        translation.Received(1).AddTranslationItem(nameof(FormMergeSubmodule), "btOpenSubmodule", "Text", "Open submodule");
        translation.Received(1).AddTranslationItem(nameof(FormMergeSubmodule), "btStageCurrent", "Text", "Stage Current");
        translation.Received(1).AddTranslationItem(nameof(FormMergeSubmodule), "label2", "Text", "There is a conflict on the submodule:");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => call.GetArguments())
            .Where(arguments => Equals(arguments[0], nameof(FormMergeSubmodule)))
            .Select(arguments => string.Join('.', arguments.Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(
            emittedKeys.Length,
            "each FormMergeSubmodule field must use exactly one translation path");
    }

    [AvaloniaTest]
    public void FormMergeSubmodule_should_offer_the_two_conflicted_revisions_to_checkout()
    {
        string submodulePath = Path.Combine(_testRoot, "submodule");
        IGitModule module = Substitute.For<IGitModule>();
        IGitModule submodule = Substitute.For<IGitModule>();
        module.GetConflictAsync("submodule").Returns(CreateConflict());
        module.GetSubmodule("submodule").Returns(submodule);
        module.GetSubmoduleFullPath("submodule").Returns(submodulePath);
        submodule.GetCurrentCheckout().Returns(LocalId);

        IGitUICommands submoduleCommands = Substitute.For<IGitUICommands>();
        submoduleCommands.StartCheckoutBranch(Arg.Any<WinFormsShims.IWin32Window?>(), Arg.Any<IReadOnlyList<ObjectId>?>()).Returns(false);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.WithWorkingDirectory(submodulePath).Returns(submoduleCommands);

        FormMergeSubmodule form = new(commands, "submodule");
        form.Show();

        form.tbBase.Text.Should().Be(BaseId.ToString());
        form.tbLocal.Text.Should().Be(LocalId.ToString());
        form.tbRemote.Text.Should().Be(RemoteId.ToString());
        form.tbCurrent.Text.Should().Be(LocalId.ToString());
        form.btCheckoutBranch.IsEnabled.Should().BeTrue();
        form.btCheckoutBranch.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        submoduleCommands.Received(1).StartCheckoutBranch(
            form,
            Arg.Is<IReadOnlyList<ObjectId>>(ids => ids.SequenceEqual(new[] { LocalId, RemoteId })));
        form.Close();
    }

    [AvaloniaTest]
    public async Task FormMergeSubmodule_should_stage_the_current_revision_in_a_real_submodule_conflict()
    {
        (GitModule module, ObjectId baseId, ObjectId localId, ObjectId remoteId) = CreateConflictedSubmoduleRepository();
        module.InTheMiddleOfConflictedMerge().Should().BeTrue();
        module.IsSubmodule("submodule").Should().BeTrue();

        GitUICommands commands = new(_serviceContainer, module);
        FormMergeSubmodule form = new(commands, "submodule");
        form.Show();

        form.tbBase.Text.Should().Be(baseId.ToString());
        form.tbLocal.Text.Should().Be(localId.ToString());
        form.tbRemote.Text.Should().Be(remoteId.ToString());
        form.tbCurrent.Text.Should().Be(localId.ToString());
        form.CaptureRenderedFrame().Should().NotBeNull("the real submodule conflict should render headlessly");

        form.btStageCurrent.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
        form.IsVisible.Should().BeFalse();
        (await module.GetConflictsAsync("submodule")).Should().BeEmpty();
        module.GitExecutable.GetOutput(new GitArgumentBuilder("ls-files") { "--stage", "--", "submodule" })
            .Should().Contain(localId.ToString());
    }

    private (GitModule Module, ObjectId BaseId, ObjectId LocalId, ObjectId RemoteId) CreateConflictedSubmoduleRepository()
    {
        string sourcePath = Path.Combine(_testRoot, "submodule-source");
        Directory.CreateDirectory(sourcePath);
        GitModule source = CreateModule(sourcePath);
        InitializeRepository(source);

        string sourceFile = Path.Combine(sourcePath, "tracked.txt");
        File.WriteAllText(sourceFile, "base\n");
        Run(source, new GitArgumentBuilder("add") { "--", "tracked.txt" });
        Run(source, new GitArgumentBuilder("commit") { "--quiet", "-m", "base" });
        ObjectId baseId = source.GetCurrentCheckout();

        Run(source, new GitArgumentBuilder("checkout") { "--quiet", "-b", "local-side" });
        File.WriteAllText(sourceFile, "local\n");
        Run(source, new GitArgumentBuilder("commit") { "--quiet", "-am", "local" });
        ObjectId localId = source.GetCurrentCheckout();

        Run(source, new GitArgumentBuilder("checkout") { "--quiet", "main" });
        Run(source, new GitArgumentBuilder("checkout") { "--quiet", "-b", "remote-side" });
        File.WriteAllText(sourceFile, "remote\n");
        Run(source, new GitArgumentBuilder("commit") { "--quiet", "-am", "remote" });
        ObjectId remoteId = source.GetCurrentCheckout();

        string superPath = Path.Combine(_testRoot, "superproject");
        Directory.CreateDirectory(superPath);
        GitModule module = CreateModule(superPath);
        InitializeRepository(module);
        Run(module, new GitArgumentBuilder("clone") { "--quiet", sourcePath.Quote(), "submodule" });
        IGitModule submodule = module.GetSubmodule("submodule");
        Run(submodule, new GitArgumentBuilder("checkout") { "--quiet", baseId });
        File.WriteAllText(
            Path.Combine(superPath, ".gitmodules"),
            "[submodule \"submodule\"]\n\tpath = submodule\n\turl = ../submodule-source\n");
        Run(module, new GitArgumentBuilder("add") { "--", ".gitmodules", "submodule" });
        Run(module, new GitArgumentBuilder("commit") { "--quiet", "-m", "base" });

        Run(module, new GitArgumentBuilder("checkout") { "--quiet", "-b", "local-side" });
        Run(submodule, new GitArgumentBuilder("checkout") { "--quiet", localId });
        Run(module, new GitArgumentBuilder("add") { "--", "submodule" });
        Run(module, new GitArgumentBuilder("commit") { "--quiet", "-m", "local" });

        Run(module, new GitArgumentBuilder("checkout") { "--quiet", "main" });
        Run(submodule, new GitArgumentBuilder("checkout") { "--quiet", remoteId });
        Run(module, new GitArgumentBuilder("add") { "--", "submodule" });
        Run(module, new GitArgumentBuilder("commit") { "--quiet", "-m", "remote" });

        Run(module, new GitArgumentBuilder("checkout") { "--quiet", "local-side" });
        Run(submodule, new GitArgumentBuilder("checkout") { "--quiet", localId });
        ExecutionResult merge = module.GitExecutable.Execute(
            new GitArgumentBuilder("merge") { "--no-edit", "main" },
            throwOnErrorExit: false);
        merge.ExitCode.Should().NotBe(0);
        return (module, baseId, localId, remoteId);
    }

    private GitModule CreateModule(string workingDirectory)
        => new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), workingDirectory);

    private static void InitializeRepository(GitModule module)
    {
        Run(module, new GitArgumentBuilder("init") { "--quiet", "-b", "main" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        module.SetSetting("core.autocrlf", "false");
    }

    private static void Run(IGitModule module, GitArgumentBuilder arguments)
        => module.GitExecutable.RunCommand(arguments).Should().BeTrue(arguments.ToString());

    private static ConflictData CreateConflict()
        => new(
            new ConflictedFileData(BaseId, "submodule"),
            new ConflictedFileData(LocalId, "submodule"),
            new ConflictedFileData(RemoteId, "submodule"));
}
