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
using GitUI.ScriptsEngine;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class CreateTagTests
{
    private static readonly ObjectId RevisionId = ObjectId.Parse("0123456789012345678901234567890123456789");

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormCreateTag_should_construct_with_all_tag_operations()
    {
        FormCreateTag form = new();
        ComboBox annotate = form.FindControl<ComboBox>("annotate")
            ?? throw new InvalidOperationException("Tag-operation selector was not created.");
        TextBox message = form.FindControl<TextBox>("tagMessage")
            ?? throw new InvalidOperationException("Tag-message field was not created.");
        TextBox key = form.FindControl<TextBox>("textBoxGpgKey")
            ?? throw new InvalidOperationException("GPG-key field was not created.");

        form.FindControl<CommitPickerSmallControl>("commitPickerSmallControl1").Should().NotBeNull();
        annotate.ItemCount.Should().Be(4);
        annotate.SelectedIndex.Should().Be(0);
        message.IsEnabled.Should().BeFalse();
        key.IsEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormCreateTag_should_render_all_fields_at_its_minimum_size()
    {
        (IGitUICommands commands, _) = CreateCommands();
        FormCreateTag form = new(commands, RevisionId)
        {
            Width = 455,
            Height = 260,
        };
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            TextBox tagName = form.FindControl<TextBox>("textBoxTagName")
                ?? throw new InvalidOperationException("Tag-name field was not created.");
            CommitPickerSmallControl picker = form.FindControl<CommitPickerSmallControl>("commitPickerSmallControl1")
                ?? throw new InvalidOperationException("Commit picker was not created.");
            ComboBox annotate = form.FindControl<ComboBox>("annotate")
                ?? throw new InvalidOperationException("Tag-operation selector was not created.");
            TextBox message = form.FindControl<TextBox>("tagMessage")
                ?? throw new InvalidOperationException("Tag-message field was not created.");
            Button create = form.FindControl<Button>("Ok")
                ?? throw new InvalidOperationException("Create-tag button was not created.");

            tagName.Bounds.Width.Should().BeGreaterThan(0);
            picker.Bounds.Width.Should().BeGreaterThan(0);
            annotate.Bounds.Width.Should().BeGreaterThan(0);
            message.Bounds.Height.Should().BeGreaterThan(0);
            create.Bounds.Width.Should().BeGreaterThan(0);
            form.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormCreateTag_should_use_existing_translation_keys_once()
    {
        FormCreateTag form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "$this", "Text", "Create tag");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "ForceTag", "Text", "Force");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "Ok", "Text", "Create tag");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "_messageCaption", "Text", "Tag");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "_noRevisionSelected", "Text", "Select 1 revision to create the tag on.");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "_pushToCaption", "Text", "Push tag to '{0}'");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "_trsAnnotated", "Text", "Annotated tag");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "_trsLightweight", "Text", "Lightweight tag");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "_trsSignDefault", "Text", "Sign with default GPG");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "_trsSignSpecificKey", "Text", "Sign with specific GPG");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "keyIdLbl", "Text", "Specific Key Id");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "label1", "Text", "Tag name");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "label2", "Text", "Message");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "label3", "Text", "Create tag at this revision");
        translation.Received(1).AddTranslationItem(nameof(FormCreateTag), "pushTag", "Text", "Push tag to '{0}'");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public void Annotate_selection_should_enable_the_matching_message_and_key_fields()
    {
        FormCreateTag form = new();
        ComboBox annotate = form.FindControl<ComboBox>("annotate")
            ?? throw new InvalidOperationException("Tag-operation selector was not created.");
        TextBox message = form.FindControl<TextBox>("tagMessage")
            ?? throw new InvalidOperationException("Tag-message field was not created.");
        TextBox key = form.FindControl<TextBox>("textBoxGpgKey")
            ?? throw new InvalidOperationException("GPG-key field was not created.");

        annotate.SelectedIndex = 1;

        message.IsEnabled.Should().BeTrue();
        key.IsEnabled.Should().BeFalse();

        annotate.SelectedIndex = 3;

        message.IsEnabled.Should().BeTrue();
        key.IsEnabled.Should().BeTrue();

        annotate.SelectedIndex = 0;

        message.IsEnabled.Should().BeFalse();
        key.IsEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public void Create_click_should_build_the_selected_tag_command()
    {
        (IGitUICommands commands, _) = CreateCommands();
        IGitCommand? executedCommand = null;
        commands.StartCommandLineProcessDialog(
                Arg.Any<WinFormsShims.IWin32Window>(),
                Arg.Do<IGitCommand>(command => executedCommand = command))
            .Returns(true);

        FormCreateTag form = new(commands, RevisionId);
        TextBox tagName = form.FindControl<TextBox>("textBoxTagName")
            ?? throw new InvalidOperationException("Tag-name field was not created.");
        CheckBox force = form.FindControl<CheckBox>("ForceTag")
            ?? throw new InvalidOperationException("Force option was not created.");
        Button create = form.FindControl<Button>("Ok")
            ?? throw new InvalidOperationException("Create-tag button was not created.");

        tagName.Text = "v1.0";
        force.IsChecked = true;
        create.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        IGitCommand command = executedCommand
            ?? throw new InvalidOperationException("The create-tag command was not executed.");
        command.Arguments.ToString().Should().Be($"tag -f \"v1.0\" -- {RevisionId}");
        form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
    }

    [AvaloniaTest]
    public void PushTag_should_stop_when_the_before_push_script_cancels()
    {
        (IGitUICommands commands, _) = CreateCommands();
        TestScriptEventRecorder scriptEvents = (TestScriptEventRecorder)commands.GetRequiredService<IScriptsRunner>();
        scriptEvents.CancelledEvents.Add(ScriptEvent.BeforePush);
        commands.StartCommandLineProcessDialog(
                Arg.Any<WinFormsShims.IWin32Window>(),
                Arg.Any<IGitCommand>())
            .Returns(true);
        FormCreateTag form = new(commands, RevisionId);
        form.Show();
        try
        {
            form.FindControl<TextBox>("textBoxTagName")!.Text = "v1.0";
            form.FindControl<CheckBox>("pushTag")!.IsChecked = true;
            form.FindControl<Button>("Ok")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            scriptEvents.Events.Should().Equal(ScriptEvent.BeforePush);
            form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
        }
        finally
        {
            if (form.IsVisible)
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public void Create_click_should_create_a_tag_in_a_real_repository()
    {
        string workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.CreateTag-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workingDirectory);
        try
        {
            using ServiceContainer services = new();
            GitExtUtils.ServiceContainerRegistry.RegisterServices(services);
            System.IO.Abstractions.FileSystem fileSystem = new();
            GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
            services.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
            services.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
            GitCommands.ServiceContainerRegistry.RegisterServices(services);

            GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), workingDirectory);
            module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
            module.SetSetting("user.name", "Avalonia Test");
            module.SetSetting("user.email", "avalonia@example.com");
            File.WriteAllText(Path.Combine(workingDirectory, "tracked.txt"), "content");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
            ObjectId revisionId = module.GetCurrentCheckout();

            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.StartCommandLineProcessDialog(
                    Arg.Any<WinFormsShims.IWin32Window>(),
                    Arg.Any<IGitCommand>())
                .Returns(call => module.GitExecutable.RunCommand(call.ArgAt<IGitCommand>(1).Arguments));

            FormCreateTag form = new(commands, revisionId);
            TextBox tagName = form.FindControl<TextBox>("textBoxTagName")
                ?? throw new InvalidOperationException("Tag-name field was not created.");
            Button create = form.FindControl<Button>("Ok")
                ?? throw new InvalidOperationException("Create-tag button was not created.");

            tagName.Text = "avalonia-smoke";
            create.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
            module.GetRefs(RefsFilter.Tags)
                .Select(gitRef => gitRef.Name)
                .Should()
                .Contain("avalonia-smoke");
        }
        finally
        {
            TestDirectory.Delete(workingDirectory);
        }
    }

    [Test]
    public void StartCreateTagDialog_should_reject_an_artificial_revision()
    {
        IGitModule module = Substitute.For<IGitModule>();
        GitUICommands commands = new(Substitute.For<IServiceProvider>(), module);

        commands.StartCreateTagDialog(revision: new GitRevision(ObjectId.WorkTreeId)).Should().BeFalse();
    }

    private static (IGitUICommands Commands, IGitModule Module) CreateCommands()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.RevParse(RevisionId.ToString()).Returns(RevisionId);
        module.WorkingDir.Returns(Path.GetTempPath());

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(typeof(IScriptsRunner)).Returns(new TestScriptEventRecorder());
        return (commands, module);
    }
}
