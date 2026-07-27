using System.ComponentModel.Design;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Plugins.DeleteUnusedBranches;
using GitExtUtils;
using GitUI;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class DeleteUnusedBranchesPluginTests
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
            $"GitExtensions.Avalonia.DeleteUnusedBranchesTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void Delete_unused_branches_form_should_construct_with_original_layout_and_translation_keys()
    {
        using DeleteUnusedBranchesForm form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(760);
        form.Height.Should().Be(500);
        form.MinWidth.Should().Be(600);
        form.MinHeight.Should().Be(400);
        form.FindControl<ListBox>("BranchesGrid").Should().NotBeNull();
        form.FindControl<CheckBox>("_NO_TRANSLATE_deleteDataGridViewCheckBoxColumn").Should().NotBeNull();
        form.FindControl<Button>("RefreshBtn")!.Height.Should().Be(25);
        form.FindControl<Button>("Delete")!.Width.Should().Be(75);
        form.FindControl<Button>("Cancel")!.Width.Should().Be(75);

        translation.Received(1).AddTranslationItem(
            nameof(DeleteUnusedBranchesForm), "nameDataGridViewTextBoxColumn", "HeaderText", "Name");
        translation.Received(1).AddTranslationItem(
            nameof(DeleteUnusedBranchesForm), "dateDataGridViewTextBoxColumn", "HeaderText", "Last activity");
        translation.Received(1).AddTranslationItem(
            nameof(DeleteUnusedBranchesForm), "Author", "HeaderText", "Last author");
        translation.Received(1).AddTranslationItem(
            nameof(DeleteUnusedBranchesForm), "Message", "HeaderText", "Last message");
        translation.DidNotReceive().AddTranslationItem(
            nameof(DeleteUnusedBranchesForm),
            "_NO_TRANSLATE_deleteDataGridViewCheckBoxColumn",
            Arg.Any<string>(),
            Arg.Any<string>());
        translation.DidNotReceive().AddTranslationItem(
            nameof(DeleteUnusedBranchesForm),
            "_NO_TRANSLATE_Remote",
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [AvaloniaTest]
    public void Delete_unused_branches_plugin_should_expose_its_embedded_icon()
    {
        DeleteUnusedBranchesPlugin plugin = new();

        plugin.Id.Should().Be(new Guid("DC3CA904-B9A5-4FE8-BF63-5B8EE9C2DDAC"));
        PluginIconProvider.GetIcon(plugin).Should().NotBeNull();
    }

    [AvaloniaTest]
    public async Task Delete_unused_branches_form_should_find_sort_and_delete_selected_local_branches()
    {
        GitModule module = CreateRepository();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        IGitPlugin plugin = Substitute.For<IGitPlugin>();
        DeleteUnusedBranchesFormSettings settings = new(
            daysOlderThan: 0,
            mergedInBranch: "HEAD",
            removeDeleteRemoteBranchesFromFlag: false,
            remoteName: "origin",
            userRegexToFilterBranchesFlag: true,
            regexFilter: "^(zeta|alpha)$",
            regexCaseInsensitiveFlag: false,
            regexInvertedFlag: false,
            includeUnmergedBranchesFlag: false);
        StubMessageBoxHost messageBoxHost = new();
        WinFormsShims.IMessageBoxHost? originalMessageBoxHost = TryGetMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = messageBoxHost;

        try
        {
            using DeleteUnusedBranchesForm form = new(settings, module, commands, plugin);
            DeleteUnusedBranchesForm.TestAccessor accessor = form.GetTestAccessor();
            accessor.LoadSettings();

            await accessor.RefreshObsoleteBranchesAsync();

            accessor.Branches.Select(branch => branch.Name).Should().BeEquivalentTo("alpha", "zeta");
            accessor.Branches.Should().OnlyContain(branch => branch.Delete);
            accessor.Status.Text.Should().Be("2/2 branches selected.");

            accessor.SortByName();
            accessor.BranchesGrid.ItemsSource!.Cast<Branch>().Select(branch => branch.Name)
                .Should().ContainInOrder("alpha", "zeta");
            accessor.SortByName();
            accessor.BranchesGrid.ItemsSource!.Cast<Branch>().Select(branch => branch.Name)
                .Should().ContainInOrder("zeta", "alpha");

            await accessor.DeleteSelectedBranchesAsync();

            module.GitExecutable.GetOutput(new GitArgumentBuilder("branch") { "--list", "alpha", "zeta" })
                .Should().BeNullOrWhiteSpace();
            form.HasDeletedBranch.Should().BeTrue();
            commands.RepoChangedNotifier.Received(1).Notify();
            messageBoxHost.Messages.Should().ContainSingle(message => message.Contains("2 selected branches"));
        }
        finally
        {
            WinFormsShims.ShimHost.MessageBoxHost = originalMessageBoxHost ?? new StubMessageBoxHost();
        }
    }

    private GitModule CreateRepository()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "trunk" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "readme.txt"), "hello\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "readme.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "zeta" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "alpha" }).Should().BeTrue();
        return module;
    }

    private static WinFormsShims.IMessageBoxHost? TryGetMessageBoxHost()
    {
        try
        {
            return WinFormsShims.ShimHost.MessageBoxHost;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<string> Messages { get; } = [];

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            return buttons == WinFormsShims.MessageBoxButtons.YesNo
                ? WinFormsShims.DialogResult.Yes
                : WinFormsShims.DialogResult.OK;
        }
    }
}
