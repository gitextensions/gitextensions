using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.UserControls.RevisionGrid;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using MediaColor = Avalonia.Media.Color;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class RemotesTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private StubMessageBoxHost _messageBoxes = null!;

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

        _messageBoxes = new StubMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxes;

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.RemotesTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormRemotes_should_construct()
    {
        FormRemotes form = new();

        form.GetTestAccessor().RemoteName.Should().NotBeNull();
        form.GetTestAccessor().Save.Should().NotBeNull();
        form.FindControl<ComboBox>("Url").Should().NotBeNull();
        form.FindControl<TextBlock>("labelPushUrl")!.IsVisible.Should().BeFalse("the push url row shows only with a separate push url");
        form.FindControl<ListBox>("RemoteBranches").Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormRemotes_should_use_existing_translation_keys_once()
    {
        FormRemotes form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "$this", "Text", "Remote repositories");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "tabPage1", "Text", "Remote repositories");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "tabPage2", "Text", "Default pull behavior (fetch & merge)");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "label1", "Text", "&Name");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "label2", "Text", "&Url");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "labelPushUrl", "Text", "&Push Url");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "lblRemoteColor", "Text", "Color");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "checkBoxSepPushUrl", "Text", "Sep&arate Push Url");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "gbMgtPanel", "Text", "Create New Remote");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "Save", "Text", "&Save changes");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "btnRemoteColor", "Text", "Set &color");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "btnRemoteColorReset", "Text", "Default color");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "SaveDefaultPushPull", "Text", "&Save changes");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "label4", "Text", "&Local branch name");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "label5", "Text", "&Remote repository");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "label6", "Text", "&Default merge with");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "lblRemotePrefix", "Text", "Prefix");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "BranchName", "HeaderText", "Local branch name");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "RemoteCombo", "HeaderText", "Remote repository");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "MergeWith", "HeaderText", "Default merge with");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "_lvgEnabledHeader", "Text", "Active");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "_lvgDisabledHeader", "Text", "Inactive");
        translation.Received(1).AddTranslationItem(nameof(FormRemotes), "_questionDeleteRemote", "Text", "Are you sure you want to delete this remote?");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public async Task FormRemotes_should_add_toggle_and_delete_a_remote()
    {
        GitModule module = CreateRepositoryWithCommit();
        IGitUICommands commands = CreateCommands(module);
        string url = Path.Combine(_workingDirectory, "other");
        Directory.CreateDirectory(url);

        FormRemotes form = new(commands);
        form.Show();
        try
        {
            FormRemotes.TestAccessor accessor = form.GetTestAccessor();

            // No remotes yet: the management panel creates a new one.
            accessor.Delete.IsEnabled.Should().BeFalse();
            accessor.ToggleState.IsEnabled.Should().BeFalse();

            accessor.RemoteName.Text = "origin";
            form.FindControl<ComboBox>("Url")!.Text = url;

            // Answer "No" to the auto-configure question.
            _messageBoxes.NextYesNoResult = WinFormsShims.DialogResult.No;
            accessor.Save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            module.GetRemoteNames().Should().Contain("origin");
            accessor.RemoteCount.Should().Be(1);
            accessor.RemoteGroupHeaders.Should().Equal("Active");
            accessor.Delete.IsEnabled.Should().BeTrue("the saved remote is selected again");

            // Deactivate: the remote becomes invisible to git.
            accessor.ToggleState.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            module.GetRemoteNames().Should().NotContain("origin");
            accessor.RemoteCount.Should().Be(1, "the inactive remote stays listed");
            accessor.RemoteGroupHeaders.Should().Equal("Inactive");

            // Activate again.
            accessor.ToggleState.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            module.GetRemoteNames().Should().Contain("origin");

            // Delete after confirming.
            _messageBoxes.NextYesNoResult = WinFormsShims.DialogResult.Yes;
            accessor.Delete.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            module.GetRemoteNames().Should().BeEmpty();
            form.FindControl<ListBox>("Remotes")!.ItemCount.Should().Be(0);
        }
        finally
        {
            form.Close();
            await RepositoryHistoryManager.Remotes.RemoveRecentAsync(url);
        }
    }

    [AvaloniaTest]
    public async Task FormRemotes_default_pull_tab_should_save_the_tracking_remote()
    {
        GitModule module = CreateRepositoryWithCommit();
        string url = Path.Combine(_workingDirectory, "other");
        Directory.CreateDirectory(url);
        module.AddRemote("origin", url).Should().BeEmpty();

        IGitUICommands commands = CreateCommands(module);

        FormRemotes form = new(commands) { PreselectLocalOnLoad = "main" };
        form.Show();
        try
        {
            form.GetTestAccessor().TabControl.SelectedItem.Should().Be(form.FindControl<TabItem>("tabPage2"),
                "preselecting a local branch opens the default pull behavior tab");

            ListBox remoteBranches = form.FindControl<ListBox>("RemoteBranches")!;
            remoteBranches.ItemCount.Should().Be(1);
            (remoteBranches.SelectedItem as IGitRef)!.LocalName.Should().Be("main");
            form.FindControl<TextBox>("LocalBranchNameEdit")!.Text.Should().Be("main");

            ComboBox remoteRepositoryCombo = form.FindControl<ComboBox>("RemoteRepositoryCombo")!;
            remoteRepositoryCombo.SelectedItem = "origin";
            form.FindControl<ComboBox>("DefaultMergeWithCombo")!.Text = "integration";

            // Committing happens when the combo loses focus, like the WinForms Validated event.
            remoteRepositoryCombo.Focus();
            form.FindControl<TextBox>("LocalBranchNameEdit")!.Focus();
            await WaitUntilAsync(() => module.GetSetting("branch.main.remote") == "origin");

            module.GetSetting("branch.main.merge").Should().Be("refs/heads/integration");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task FormRemotes_should_save_and_render_a_remote_color()
    {
        GitModule module = CreateRepositoryWithCommit();
        string url = Path.Combine(_workingDirectory, "other");
        Directory.CreateDirectory(url);
        module.AddRemote("origin", url).Should().BeEmpty();

        FormRemotes form = new(CreateCommands(module));
        form.Show();
        try
        {
            FormRemotes.TestAccessor accessor = form.GetTestAccessor();
            accessor.RemoteColor.Color = MediaColor.Parse("#123456");
            accessor.Save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            module.GetSetting("remote.origin.color").Should().Be("#123456");
            accessor.RemoteColor.Content.Should().BeOfType<Border>()
                .Which.Background.Should().BeAssignableTo<Avalonia.Media.ISolidColorBrush>()
                .Which.Color.Should().Be(MediaColor.Parse("#123456"));

            module.ResetRemoteColors();
            GitRef remoteRef = new(module, ObjectId.Random(), "refs/remotes/origin/main", remote: "origin");
            RevisionGridRefRenderer.RefLabelControl label = RevisionGridRefRenderer.CreateLabels([remoteRef])
                .Should().ContainSingle()
                .Which.Should().BeOfType<RevisionGridRefRenderer.RefLabelControl>().Subject;
            label.RefBrush.Should().BeAssignableTo<Avalonia.Media.ISolidColorBrush>()
                .Which.Color.Should().Be(MediaColor.Parse("#123456"));

            accessor.RemoteColorReset.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            accessor.Save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            module.GetSetting("remote.origin.color").Should().BeEmpty();
        }
        finally
        {
            form.Close();
            await RepositoryHistoryManager.Remotes.RemoveRecentAsync(url);
        }
    }

    private GitModule CreateRepositoryWithCommit()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "readme.txt"), "hello\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "readme.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        return module;
    }

    private IGitUICommands CreateCommands(IGitModule module)
    {
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(Arg.Any<Type>()).Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        return commands;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the condition should be met within the timeout");
    }

    /// <summary>Answers yes/no questions with a configurable result, everything else with OK.</summary>
    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<string> Messages { get; } = [];

        public WinFormsShims.DialogResult NextYesNoResult { get; set; } = WinFormsShims.DialogResult.Yes;

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            return buttons switch
            {
                WinFormsShims.MessageBoxButtons.YesNo or WinFormsShims.MessageBoxButtons.YesNoCancel => NextYesNoResult,
                _ => WinFormsShims.DialogResult.OK,
            };
        }
    }
}
