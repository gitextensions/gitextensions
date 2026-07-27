using System.ComponentModel.Design;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Plugins.CreateLocalBranches;
using GitExtUtils;
using GitUI;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class CreateLocalBranchesPluginTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _bareRemoteDirectory = null!;
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

        string testId = Guid.NewGuid().ToString("N");
        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.CreateLocalBranchesTests-{testId}");
        _bareRemoteDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.CreateLocalBranchesTests-{testId}.git");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
        TestDirectory.Delete(_bareRemoteDirectory);
    }

    [AvaloniaTest]
    public void Create_local_branches_form_should_construct_with_original_layout_and_translation_keys()
    {
        using CreateLocalBranchesForm form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(518);
        form.Height.Should().Be(79);
        form.CanResize.Should().BeFalse();
        TextBox remote = form.FindControl<TextBox>("_NO_TRANSLATE_Remote")!;
        TextBlock label = form.FindControl<TextBlock>("label1")!;
        Button create = form.FindControl<Button>("button1")!;
        label.Margin.Should().Be(new Thickness(15, 11, 0, 0));
        label.Height.Should().Be(20);
        remote.Width.Should().Be(176);
        remote.Height.Should().Be(27);
        create.Width.Should().Be(377);
        create.Height.Should().Be(30);
        create.Margin.Should().Be(new Thickness(66, 40, 0, 0));

        translation.Received(1).AddTranslationItem(
            nameof(CreateLocalBranchesForm), "$this", "Text", "Create local tracking branches");
        translation.Received(1).AddTranslationItem(
            nameof(CreateLocalBranchesForm), "button1", "Text", "Create local tracking branches");
        translation.Received(1).AddTranslationItem(
            nameof(CreateLocalBranchesForm), "label1", "Text", "Remote to create tracking branches for");
        translation.DidNotReceive().AddTranslationItem(
            nameof(CreateLocalBranchesForm), "_NO_TRANSLATE_Remote", Arg.Any<string>(), Arg.Any<string>());
    }

    [AvaloniaTest]
    public void Create_local_branches_plugin_should_expose_its_embedded_icon()
    {
        CreateLocalBranchesPlugin plugin = new();

        plugin.Id.Should().Be(new Guid("BE7BEE10-21B5-489F-9664-957945C203DC"));
        PluginIconProvider.GetIcon(plugin).Should().NotBeNull();
    }

    [AvaloniaTest]
    public void Create_local_branches_form_should_create_tracking_branches_for_the_selected_remote()
    {
        GitModule module = CreateRepositoryAndRemote();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        GitUIEventArgs eventArgs = new(ownerForm: null, commands);
        StubMessageBoxHost messageBoxHost = new();
        WinFormsShims.IMessageBoxHost? originalMessageBoxHost = TryGetMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = messageBoxHost;

        try
        {
            using CreateLocalBranchesForm form = new(eventArgs);
            form.FindControl<TextBox>("_NO_TRANSLATE_Remote")!.Text = "origin";
            form.Show();
            Dispatcher.UIThread.RunJobs();

            form.FindControl<Button>("button1")!
                .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            string branches = module.GitExecutable.GetOutput(new GitArgumentBuilder("for-each-ref")
            {
                "--format=%(refname:short):%(upstream:short)",
                "refs/heads/feature",
                "refs/heads/main"
            });
            branches.Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries)
                .Select(branch => branch.Trim())
                .Should().BeEquivalentTo("feature:origin/feature", "main:origin/main");
            messageBoxHost.Messages.Should().ContainSingle()
                .Which.Should().EndWith("local tracking branches have been created/updated.");
        }
        finally
        {
            WinFormsShims.ShimHost.MessageBoxHost = originalMessageBoxHost ?? new StubMessageBoxHost();
        }
    }

    private GitModule CreateRepositoryAndRemote()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "trunk" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "readme.txt"), "hello\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "readme.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--bare", _bareRemoteDirectory }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("remote") { "add", "origin", _bareRemoteDirectory }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("push") { "--quiet", "origin", "HEAD:refs/heads/main" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("push") { "--quiet", "origin", "HEAD:refs/heads/feature" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("fetch") { "--quiet", "origin" }).Should().BeTrue();
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
            return WinFormsShims.DialogResult.OK;
        }
    }
}
