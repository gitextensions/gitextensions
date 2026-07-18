using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
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

namespace GitExtensionsTests;

[TestFixture]
public sealed class FormPushTests
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
        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PushTests-{testId}");
        _bareRemoteDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PushTests-{testId}.git");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
        if (Directory.Exists(_bareRemoteDirectory))
        {
            TestDirectory.Delete(_bareRemoteDirectory);
        }
    }

    [AvaloniaTest]
    public void FormPush_should_construct_and_use_existing_translation_keys()
    {
        FormPush form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormPush), "$this", "Text", "Push");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "groupBox2", "Text", "Push to");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "labelFrom", "Text", "&Branch to push");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "labelTo", "Text", "&to");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "ckForceWithLease", "Text", "&Force with lease");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "_forceWithLeaseTooltips", "Text", Arg.Any<string>());
        translation.Received(1).AddTranslationItem(nameof(FormPush), "_noCurrentBranch", "Text", "No branch is selected, cannot push.");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "_pushCaption", "Text", "Push");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "_pushToCaption", "Text", "Push to {0}");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(
            emittedKeys.Length,
            "each field must be routed through exactly one translation path");
    }

    [AvaloniaTest]
    public void FormPush_should_show_the_current_branch_selected_remote_and_force_with_lease()
    {
        GitModule module = CreateRepositoryAndRemote();
        FormPush form = new(new GitUICommands(_serviceContainer, module));

        form.Show();
        try
        {
            ComboBox remotes = form.FindControl<ComboBox>("_NO_TRANSLATE_Remotes")
                ?? throw new InvalidOperationException("Remote selector was not created.");
            TextBox branch = form.FindControl<TextBox>("_NO_TRANSLATE_Branch")
                ?? throw new InvalidOperationException("Branch field was not created.");
            TextBox remoteBranch = form.FindControl<TextBox>("RemoteBranch")
                ?? throw new InvalidOperationException("Remote branch field was not created.");
            CheckBox forceWithLease = form.FindControl<CheckBox>("ckForceWithLease")
                ?? throw new InvalidOperationException("Force-with-lease checkbox was not created.");
            Button push = form.FindControl<Button>("Push")
                ?? throw new InvalidOperationException("Push button was not created.");
            TextBlock labelFrom = form.FindControl<TextBlock>("labelFrom")
                ?? throw new InvalidOperationException("Branch label was not created.");
            TextBlock labelTo = form.FindControl<TextBlock>("labelTo")
                ?? throw new InvalidOperationException("Remote-branch label was not created.");

            remotes.SelectedItem.Should().Be("origin");
            labelFrom.Text.Should().Be("Branch to push");
            labelTo.Text.Should().Be("to");
            branch.Text.Should().Be(module.GetSelectedBranch());
            remoteBranch.Text.Should().Be(branch.Text);
            push.IsEnabled.Should().BeTrue();
            forceWithLease.IsChecked.Should().BeFalse();

            form.CheckForceWithLease();

            forceWithLease.IsChecked.Should().BeTrue();
            form.CaptureRenderedFrame().Should().NotBeNull("the reduced push dialog should render headlessly");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void StartPushDialog_should_push_the_current_branch_to_a_local_bare_remote()
    {
        bool closeProcessDialog = AppSettings.CloseProcessDialog;
        AppSettings.CloseProcessDialog = true;
        try
        {
            GitModule module = CreateRepositoryAndRemote();
            ObjectId localCommit = module.GetCurrentCheckout();
            string branch = module.GetSelectedBranch();
            GitUICommands commands = new(_serviceContainer, module);
            FormPush owner = new(commands);
            owner.Show();

            bool result;
            bool pushCompleted;
            try
            {
                result = commands.StartPushDialog(owner, pushOnShow: true, forceWithLease: false, out pushCompleted);
            }
            finally
            {
                owner.Close();
            }

            GitModule remoteModule = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _bareRemoteDirectory);
            result.Should().BeTrue();
            pushCompleted.Should().BeTrue();
            remoteModule.RevParse($"refs/heads/{branch}").Should().Be(localCommit);
        }
        finally
        {
            AppSettings.CloseProcessDialog = closeProcessDialog;
        }
    }

    private GitModule CreateRepositoryAndRemote()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "initial line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--bare", _bareRemoteDirectory });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("remote") { "add", "origin", _bareRemoteDirectory });
        return module;
    }
}
