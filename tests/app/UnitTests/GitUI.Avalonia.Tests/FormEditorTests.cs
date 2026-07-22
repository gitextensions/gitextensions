using System.ComponentModel.Design;
using System.Text;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
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
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FormEditorTests
{
    private GitUICommands _commands = null!;
    private ServiceContainer _serviceContainer = null!;
    private StubMessageBoxHost _stubMessageBoxHost = null!;
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
        _serviceContainer.AddService<IAppTitleGenerator>(new AppTitleGenerator(repositoryDescriptionProvider));
        _serviceContainer.AddService<ILinkFactory>(new LinkFactory());
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.FormEditor-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        _commands = new GitUICommands(_serviceContainer, module);

        _stubMessageBoxHost = new StubMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = _stubMessageBoxHost;
    }

    [TearDown]
    public void TearDown()
    {
        WinFormsShims.ShimHost.MessageBoxHost = new StubMessageBoxHost { Result = WinFormsShims.DialogResult.OK };
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public async Task FormEditor_should_load_the_requested_line_track_changes_and_preserve_the_file_preamble()
    {
        UTF8Encoding encoding = new(encoderShouldEmitUTF8Identifier: true);
        string filePath = Path.Combine(_workingDirectory, "message.txt");
        const string initialText = "first line\nsecond line\nthird line\n";
        File.WriteAllBytes(filePath, [.. encoding.GetPreamble(), .. encoding.GetBytes(initialText)]);

        using FormEditor form = new(_commands, filePath, showWarning: false, lineNumber: 2);
        FormEditor.TestAccessor accessor = form.GetTestAccessor();
        await WaitUntilAsync(() => accessor.FileViewer.GetText() == initialText);

        form.Text.Should().Be(filePath);
        accessor.FileViewer.CurrentFileLine.Should().Be(2);
        accessor.HasChanges.Should().BeFalse();
        accessor.IsSaveEnabled.Should().BeFalse();

        accessor.FileViewer.TextEditor.Text += "edited";

        accessor.HasChanges.Should().BeTrue();
        accessor.IsSaveEnabled.Should().BeTrue();
        form.ProcessHotkey(WinFormsShims.Keys.Control | WinFormsShims.Keys.S).Should().BeTrue();

        accessor.HasChanges.Should().BeFalse();
        accessor.IsSaveEnabled.Should().BeFalse();
        File.ReadAllBytes(filePath).Should().Equal([.. encoding.GetPreamble(), .. encoding.GetBytes(initialText + "edited")]);
    }

    [AvaloniaTest]
    public async Task FormEditor_should_retain_warning_read_only_and_close_confirmation_behavior()
    {
        string filePath = Path.Combine(_workingDirectory, "protected.txt");
        File.WriteAllText(filePath, "protected content", _commands.Module.FilesEncoding);
        using FormEditor form = new(_commands, filePath, showWarning: true, readOnly: true);
        FormEditor.TestAccessor accessor = form.GetTestAccessor();
        await WaitUntilAsync(() => accessor.FileViewer.GetText() == "protected content");

        accessor.IsWarningVisible.Should().BeTrue();
        accessor.FileViewer.IsReadOnly.Should().BeTrue();

        accessor.FileViewer.IsReadOnly = false;
        accessor.FileViewer.TextEditor.Text += " edited";
        _stubMessageBoxHost.Result = WinFormsShims.DialogResult.Cancel;

        accessor.ConfirmClose().Should().BeFalse();
        form.DialogResult.Should().Be(WinFormsShims.DialogResult.None);

        _stubMessageBoxHost.Result = WinFormsShims.DialogResult.No;

        accessor.ConfirmClose().Should().BeTrue();
        form.DialogResult.Should().Be(WinFormsShims.DialogResult.Cancel);
        File.ReadAllText(filePath, _commands.Module.FilesEncoding).Should().Be("protected content");
        _stubMessageBoxHost.Messages.Should().OnlyContain(message => message == "Do you want to save changes?");
    }

    [AvaloniaTest]
    public void FormEditor_should_emit_only_the_existing_translation_keys()
    {
        using FormEditor form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormEditor), "$this", "Text", "Editor");
        translation.Received(1).AddTranslationItem(nameof(FormEditor), "_cannotOpenFile", "Text", "Cannot open file:");
        translation.Received(1).AddTranslationItem(nameof(FormEditor), "_cannotSaveFile", "Text", "Cannot save file:");
        translation.Received(1).AddTranslationItem(nameof(FormEditor), "_saveChanges", "Text", "Do you want to save changes?");
        translation.Received(1).AddTranslationItem(nameof(FormEditor), "_saveChangesCaption", "Text", "Save changes");
        translation.Received(1).AddTranslationItem(
            nameof(FormEditor),
            "labelWarning",
            "Text",
            "Here be dragons!\r\nChanging this file by hand can be harmful and might break something.\r\nIf you are not sure just close this window.");
        translation.Received(1).AddTranslationItem(nameof(FormEditor), "toolStripSaveButton", "ToolTipText", "Save");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Should().HaveCount(7);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the editor file should load before the timeout");
    }

    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<string> Messages { get; } = [];

        public WinFormsShims.DialogResult Result { get; set; }

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            return Result;
        }
    }
}
