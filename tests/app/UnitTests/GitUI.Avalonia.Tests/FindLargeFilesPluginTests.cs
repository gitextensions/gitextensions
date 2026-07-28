using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Plugins.FindLargeFiles;
using GitExtUtils;
using GitUI;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FindLargeFilesPluginTests
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
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _workingDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.FindLargeFilesTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void Find_large_files_form_should_construct_with_original_layout_and_translation_keys()
    {
        using FindLargeFilesForm form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(760);
        form.Height.Should().Be(421);
        form.FindControl<ListBox>("BranchesGrid").Should().NotBeNull();
        form.FindControl<ProgressBar>("pbRevisions")!.Height.Should().Be(27);
        form.FindControl<Button>("Delete")!.Width.Should().Be(75);
        form.FindControl<Button>("Cancel")!.Width.Should().Be(75);

        translation.Received(1).AddTranslationItem(
            nameof(FindLargeFilesForm), "sHADataGridViewTextBoxColumn", "HeaderText", "SHA");
        translation.Received(1).AddTranslationItem(
            nameof(FindLargeFilesForm), "pathDataGridViewTextBoxColumn", "HeaderText", "Path");
        translation.Received(1).AddTranslationItem(
            nameof(FindLargeFilesForm), "CompressedSize", "HeaderText", "Compressed size");
        translation.Received(1).AddTranslationItem(
            nameof(FindLargeFilesForm), "dataGridViewCheckBoxColumn1", "HeaderText", "Delete");
    }

    [AvaloniaTest]
    public void Find_large_files_plugin_should_expose_its_embedded_icon()
    {
        FindLargeFilesPlugin plugin = new();

        plugin.Id.Should().Be(new Guid("5AE20AB1-D677-46C5-ABDB-7874FF5A9296"));
        PluginIconProvider.GetIcon(plugin).Should().NotBeNull();
    }

    [AvaloniaTest]
    public async Task Find_large_files_form_should_scan_repository_blobs()
    {
        GitModule module = CreateRepository();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        using FindLargeFilesForm form = new(threshold: 0.001f, commands);
        FindLargeFilesForm.TestAccessor accessor = form.GetTestAccessor();

        IReadOnlyList<GitObject> objects = await accessor.FindLargeFilesAsync();

        GitObject gitObject = objects.Should().ContainSingle().Which;
        gitObject.Path.Should().Be("large.bin");
        gitObject.SizeInBytes.Should().Be(2048);
        gitObject.CommitCount.Should().Be(1);
        gitObject.LastCommitDate.Should().BeAfter(DateTime.MinValue);
    }

    [AvaloniaTest]
    public void Find_large_files_form_should_generate_native_history_rewrite_scripts()
    {
        string originalGitCommand = AppSettings.GitCommandValue;
        try
        {
            AppSettings.GitCommandValue = "/opt/Git Tools/git";
            using FindLargeFilesForm form = new();
            FindLargeFilesForm.TestAccessor accessor = form.GetTestAccessor();
            GitObject gitObject = new("sha", "folder/it's large.bin", 2048, "commit")
            {
                Delete = true,
            };

            string windowsCommand = accessor.GenerateCommandForTesting([gitObject], useWindowsBatch: true);
            string posixCommand = accessor.GenerateCommandForTesting([gitObject], useWindowsBatch: false);

            windowsCommand.Should().StartWith("SET gitexe=\"/opt/Git Tools/git\"");
            windowsCommand.Should().Contain("%gitexe% filter-branch");
            windowsCommand.Should().Contain("'folder/it'\\''s large.bin'");
            windowsCommand.Should().Contain("for /f \"usebackq\"");
            posixCommand.Should().StartWith("#!/bin/sh\nset -e\n");
            posixCommand.Should().Contain("gitexe='/opt/Git Tools/git'");
            posixCommand.Should().Contain("\"$gitexe\" filter-branch --index-filter");
            posixCommand.Should().Contain("while IFS= read -r ref; do");
            posixCommand.Should().NotContain("%gitexe%", "POSIX shells do not expand batch variables");
            accessor.QuoteForPosixShellForTesting("folder/it's large.bin")
                .Should().Be("'folder/it'\"'\"'s large.bin'");

            if (!OperatingSystem.IsWindows())
            {
                string scriptPath = Path.Combine(_workingDirectory, "find-large-files.sh");
                File.WriteAllText(scriptPath, posixCommand);
                ProcessStartInfo startInfo = new("/bin/sh")
                {
                    RedirectStandardError = true,
                    UseShellExecute = false,
                };
                startInfo.ArgumentList.Add("-n");
                startInfo.ArgumentList.Add(scriptPath);
                using Process process = Process.Start(startInfo)!;
                process.WaitForExit();
                process.ExitCode.Should().Be(0, process.StandardError.ReadToEnd());
            }
        }
        finally
        {
            AppSettings.GitCommandValue = originalGitCommand;
        }
    }

    private GitModule CreateRepository()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "trunk" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllBytes(Path.Combine(_workingDirectory, "large.bin"), new byte[2048]);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "large.bin" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        return module;
    }
}
