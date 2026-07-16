using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class DeleteTagTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormDeleteTag_should_construct_with_local_and_remote_controls()
    {
        FormDeleteTag form = new();

        form.FindControl<ComboBox>("Tags").Should().NotBeNull();
        form.FindControl<RemotesComboboxControl>("remotesComboboxControl1").Should().NotBeNull();
        form.FindControl<GotoUserManualControl>("gotoUserManualControl1").Should().NotBeNull();
        Button delete = form.FindControl<Button>("Ok")
            ?? throw new InvalidOperationException("Delete button was not created.");
        delete.IsDefault.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormDeleteTag_should_use_existing_translation_keys_once()
    {
        FormDeleteTag form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormDeleteTag), "$this", "Text", "Delete tag");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteTag), "Ok", "Text", "Delete");
        translation.Received(1).AddTranslationItem(
            nameof(FormDeleteTag),
            "deleteTag",
            "Text",
            "Delete tag also from the following remote(s):");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteTag), "label1", "Text", "Select tag");
        translation.Received(1).AddTranslationItem(
            nameof(FormDeleteTag),
            "label2",
            "Text",
            "This will delete the selected tag from the (local) repository.");
        translation.Received(1).AddTranslationItem(
            nameof(FormDeleteTag),
            "label3",
            "Text",
            "(includes information about deleting tags which are already pushed)");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public void GotoUserManualControl_should_use_existing_translation_keys_once()
    {
        GotoUserManualControl control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);
        control.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(GotoUserManualControl),
            "_gotoUserManualControlTooltip",
            "Text",
            "Read more about this feature at {0}");
        translation.Received(1).AddTranslationItem(
            nameof(GotoUserManualControl),
            "linkLabelHelp",
            "Text",
            "Help");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public void FormDeleteTag_should_load_defaults_and_render_at_its_minimum_size()
    {
        (IGitUICommands commands, _) = CreateCommands("v1.0");
        FormDeleteTag form = new(commands, "v1.0")
        {
            Width = 470,
            Height = 210,
        };
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            ComboBox tags = form.FindControl<ComboBox>("Tags")
                ?? throw new InvalidOperationException("Tag selector was not created.");
            CheckBox deleteRemote = form.FindControl<CheckBox>("deleteTag")
                ?? throw new InvalidOperationException("Remote-delete option was not created.");
            RemotesComboboxControl remotes = form.FindControl<RemotesComboboxControl>("remotesComboboxControl1")
                ?? throw new InvalidOperationException("Remote selector was not created.");

            tags.Text.Should().Be("v1.0");
            tags.Bounds.Width.Should().BeGreaterThan(0);
            remotes.SelectedRemote.Should().Be("origin");
            remotes.IsEnabled.Should().BeFalse();
            deleteRemote.IsChecked = true;
            remotes.IsEnabled.Should().BeTrue();
            Dispatcher.UIThread.RunJobs();
            form.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Delete_click_should_delete_local_and_selected_remote_tag()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands("v1.0");
        ArgumentString? remoteDeleteArguments = null;
        commands.StartGitCommandProcessDialog(
                Arg.Any<WinFormsShims.IWin32Window>(),
                Arg.Do<ArgumentString>(arguments => remoteDeleteArguments = arguments))
            .Returns(true);

        FormDeleteTag form = new(commands, "v1.0");
        form.Show();
        Dispatcher.UIThread.RunJobs();

        CheckBox deleteRemote = form.FindControl<CheckBox>("deleteTag")
            ?? throw new InvalidOperationException("Remote-delete option was not created.");
        Button delete = form.FindControl<Button>("Ok")
            ?? throw new InvalidOperationException("Delete button was not created.");
        deleteRemote.IsChecked = true;
        delete.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        module.Received(1).DeleteTag("v1.0");
        remoteDeleteArguments.Should().NotBeNull();
        remoteDeleteArguments!.Value.ToString().Should().Be("push \"origin\" :refs/tags/v1.0");
        form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
    }

    [AvaloniaTest]
    public void Delete_click_should_delete_a_tag_in_a_real_repository()
    {
        string workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.DeleteTag-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workingDirectory);
        try
        {
            using ServiceContainer services = CreateServiceContainer();
            GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), workingDirectory);
            module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
            module.SetSetting("user.name", "Avalonia Test");
            module.SetSetting("user.email", "avalonia@example.com");
            File.WriteAllText(Path.Combine(workingDirectory, "tracked.txt"), "content");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("tag") { "avalonia-delete" }).Should().BeTrue();

            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);

            FormDeleteTag form = new(commands, "avalonia-delete");
            form.Show();
            Dispatcher.UIThread.RunJobs();

            Button delete = form.FindControl<Button>("Ok")
                ?? throw new InvalidOperationException("Delete button was not created.");
            delete.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            module.GetRefs(RefsFilter.Tags).Select(gitRef => gitRef.LocalName).Should().NotContain("avalonia-delete");
        }
        finally
        {
            Directory.Delete(workingDirectory, recursive: true);
        }
    }

    private static (IGitUICommands Commands, IGitModule Module) CreateCommands(string tagName)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(true);
        module.GetCurrentRemote().Returns("origin");
        module.GetRemoteNames().Returns(["origin", "upstream"]);
        module.GetRefs(RefsFilter.Tags).Returns(
        [
            new GitRef(module, ObjectId.Random(), $"refs/tags/{tagName}"),
        ]);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return (commands, module);
    }

    private static ServiceContainer CreateServiceContainer()
    {
        ServiceContainer services = new();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(services);
        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        services.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        services.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        GitCommands.ServiceContainerRegistry.RegisterServices(services);
        return services;
    }
}
