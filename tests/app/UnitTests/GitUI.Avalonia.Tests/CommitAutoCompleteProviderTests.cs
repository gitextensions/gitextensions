using System.ComponentModel.Design;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.AutoCompletion;

namespace GitExtensionsTests;

[TestFixture]
public sealed class CommitAutoCompleteProviderTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.AutoComplete.Tests-{Guid.NewGuid():N}");
        string relativePath = Path.GetRelativePath(FindRepositoryRoot(), _workingDirectory);
        relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal).Should().BeTrue(
            "the provider audit repository must live outside the working tree");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [Test]
    public async Task CommitAutoCompleteProvider_should_read_changed_names_renames_and_symbols_from_a_real_repository()
    {
        GitModule module = CreateRepositoryWithChangedCodeAndRename();
        CommitAutoCompleteProvider provider = new(() => module);

        IEnumerable<AutoCompleteWord> result = await provider.GetAutoCompleteWordsAsync(CancellationToken.None);

        result.Select(word => word.Word).Should().Contain(
        [
            "RenamedParser.cs",
            "RenamedParser",
            "OldParser.cs",
            "OldParser",
            "BuildRequest",
            "NewWorker.cs",
            "NewWorker",
        ]);
    }

    [Test]
    public async Task CommitAutoCompleteProvider_should_honor_an_already_cancelled_request()
    {
        GitModule module = CreateRepositoryWithChangedCodeAndRename();
        CommitAutoCompleteProvider provider = new(() => module);
        using CancellationTokenSource cancellationTokenSource = new();
        await cancellationTokenSource.CancelAsync();

        Func<Task> act = async () =>
            await provider.GetAutoCompleteWordsAsync(cancellationTokenSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private GitModule CreateRepositoryWithChangedCodeAndRename()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");

        string originalPath = Path.Combine(_workingDirectory, "OldParser.cs");
        File.WriteAllText(originalPath, "internal sealed class OldParser { }\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "OldParser.cs" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });

        module.GitExecutable.RunCommand(new GitArgumentBuilder("mv") { "--", "OldParser.cs", "RenamedParser.cs" });
        File.WriteAllText(
            Path.Combine(_workingDirectory, "RenamedParser.cs"),
            "internal sealed class RenamedParser { public void BuildRequest() { } }\n");
        File.WriteAllText(
            Path.Combine(_workingDirectory, "NewWorker.cs"),
            "internal sealed class NewWorker { }\n");
        return module;
    }

    private static string FindRepositoryRoot([System.Runtime.CompilerServices.CallerFilePath] string startPath = "")
    {
        DirectoryInfo? directory = new(Path.GetDirectoryName(startPath)!);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.Avalonia.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"Could not find the repository root from {startPath}.");
    }
}
