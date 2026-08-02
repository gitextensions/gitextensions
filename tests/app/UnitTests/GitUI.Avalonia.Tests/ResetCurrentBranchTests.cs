using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class ResetCurrentBranchTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private StubMessageBoxHost _messageBoxes = null!;

    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        System.IO.Abstractions.FileSystem fileSystem = new();
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(new GitDirectoryResolver(fileSystem));
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _messageBoxes = new StubMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxes;
        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.ResetCurrentBranchTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormResetCurrentBranch_should_construct_with_the_original_controls_and_palette()
    {
        FormResetCurrentBranch form = new();
        FormResetCurrentBranch.TestAccessor accessor = form.GetTestAccessor();

        accessor.SelectedResetType.Should().Be(FormResetCurrentBranch.ResetType.Soft);
        accessor.BuildResetCommand().Should().BeNull();
        accessor.Ok.Should().NotBeNull();
        accessor.Cancel.Should().NotBeNull();
        accessor.Help.Should().NotBeNull();
        GetColor(accessor.Soft.Background).Should().NotBe(GetColor(accessor.Mixed.Background));
        GetColor(accessor.Mixed.Background).Should().NotBe(GetColor(accessor.Hard.Background));
    }

    [AvaloniaTest]
    public void FormResetCurrentBranch_should_build_every_reset_mode_and_help_target()
    {
        GitRevision revision = new(ObjectId.Parse("0123456789abcdef0123456789abcdef01234567"));
        IGitModule module = Substitute.For<IGitModule>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormResetCurrentBranch form = FormResetCurrentBranch.Create(commands, revision);
        FormResetCurrentBranch.TestAccessor accessor = form.GetTestAccessor();
        (FormResetCurrentBranch.ResetType Type, ResetMode Mode, string HelpSection)[] cases =
        [
            (FormResetCurrentBranch.ResetType.Soft, ResetMode.Soft, "--soft"),
            (FormResetCurrentBranch.ResetType.Mixed, ResetMode.Mixed, "--mixed"),
            (FormResetCurrentBranch.ResetType.Keep, ResetMode.Keep, "--keep"),
            (FormResetCurrentBranch.ResetType.Merge, ResetMode.Merge, "--merge"),
            (FormResetCurrentBranch.ResetType.Hard, ResetMode.Hard, "--hard"),
        ];

        foreach ((FormResetCurrentBranch.ResetType type, ResetMode mode, string helpSection) in cases)
        {
            accessor.SelectResetType(type);

            accessor.SelectedResetType.Should().Be(type);
            accessor.BuildResetCommand().Should().Be(Commands.Reset(mode, revision.Guid, quiet: false));
            accessor.GetHelpUrl().Should().EndWith(helpSection);
        }
    }

    [AvaloniaTest]
    public void FormResetCurrentBranch_should_load_the_selected_branch_and_revision_summary()
    {
        GitRevision revision = new(ObjectId.Random());
        IGitModule module = Substitute.For<IGitModule>();
        module.GetSelectedBranch().Returns("feature/reset-dialog");
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormResetCurrentBranch form = FormResetCurrentBranch.Create(commands, revision);
        FormResetCurrentBranch.TestAccessor accessor = form.GetTestAccessor();

        accessor.Load();

        accessor.BranchInfo.Should().Be("Reset branch 'feature/reset-dialog' to revision:");
        accessor.SummaryRevision.Should().BeSameAs(revision);
    }

    [AvaloniaTest]
    public void FormResetCurrentBranch_cancel_should_return_cancel()
    {
        FormResetCurrentBranch form = new();

        form.GetTestAccessor().Cancel.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        form.DialogResult.Should().Be(WinFormsShims.DialogResult.Cancel);
    }

    [AvaloniaTest]
    public void FormResetCurrentBranch_should_hard_reset_a_real_repository()
    {
        (GitModule module, GitRevision target) = CreateRepositoryWithResetTarget();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormResetCurrentBranch form = FormResetCurrentBranch.Create(commands, target, FormResetCurrentBranch.ResetType.Hard);
        ArgumentString command = form.GetTestAccessor().BuildResetCommand()!.Value;

        module.GitExecutable.RunCommand(command).Should().BeTrue(command.ToString());

        module.GetCurrentCheckout().Should().Be(target.ObjectId);
        File.ReadAllText(Path.Combine(_workingDirectory, "content.txt")).Should().Be("before");
    }

    [AvaloniaTest]
    public void GitUICommands_should_report_an_unresolved_reset_target()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.RevParse("missing").Returns(default(ObjectId));
        GitUICommands commands = new(_serviceContainer, module);

        bool result = commands.StartResetCurrentBranchDialog(owner: null, "missing");

        result.Should().BeFalse();
        _messageBoxes.Messages.Should().ContainSingle().Which.Should().Be("Branch \"missing\" could not be resolved.");
    }

    [AvaloniaTest]
    public void Revision_grid_should_reuse_the_existing_reset_current_branch_translation_key()
    {
        RevisionGridControl control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(RevisionGridControl),
            "resetCurrentBranchToHereToolStripMenuItem",
            "Text",
            "Reset c&urrent branch to here...");
    }

    [AvaloniaTest]
    public void FormResetCurrentBranch_should_use_the_existing_translation_keys_once()
    {
        FormResetCurrentBranch form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormResetCurrentBranch), "$this", "Text", "Reset current branch");
        translation.Received(1).AddTranslationItem(nameof(FormResetCurrentBranch), "Cancel", "Text", "Cancel");
        translation.Received(1).AddTranslationItem(
            nameof(FormResetCurrentBranch),
            "Hard",
            "Text",
            "&Hard: reset working directory and index\n(discard ALL local changes, even uncommitted changes)");
        translation.Received(1).AddTranslationItem(
            nameof(FormResetCurrentBranch),
            "Keep",
            "Text",
            "&Keep: update working directory to the commit \n(abort if there are local changes), reset index");
        translation.Received(1).AddTranslationItem(
            nameof(FormResetCurrentBranch),
            "Merge",
            "Text",
            "&Merge: update working directory to the commit and keep local changes \n(abort if there are conflicts), reset index");
        translation.Received(1).AddTranslationItem(nameof(FormResetCurrentBranch), "Mixed", "Text", "Mi&xed: leave working directory untouched, reset index");
        translation.Received(1).AddTranslationItem(nameof(FormResetCurrentBranch), "Ok", "Text", "OK");
        translation.Received(1).AddTranslationItem(nameof(FormResetCurrentBranch), "Soft", "Text", "&Soft: leave working directory and index untouched");
        translation.Received(1).AddTranslationItem(nameof(FormResetCurrentBranch), "_branchInfo", "Text", "Reset branch '{0}' to revision:");
        translation.Received(1).AddTranslationItem(nameof(FormResetCurrentBranch), "_resetCaption", "Text", "Reset branch");
        translation.Received(1).AddTranslationItem(nameof(FormResetCurrentBranch), "_resetHardWarning", "Text", "You are about to discard ALL local changes, are you sure?");

        AccessorText(form.GetTestAccessor().Keep).Should().StartWith("Keep:").And.Contain("\n(abort if there are local changes)");
        AccessorText(form.GetTestAccessor().Merge).Should().StartWith("Merge:").And.Contain("\n(abort if there are conflicts)");
        AccessorText(form.GetTestAccessor().Hard).Should().StartWith("Hard:").And.Contain("\n(discard ALL local changes");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);

        return;

        static string AccessorText(RadioButton radioButton)
            => radioButton.Content.Should().BeOfType<TextBlock>().Which.Text ?? string.Empty;
    }

    private (GitModule Module, GitRevision Target) CreateRepositoryWithResetTarget()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        string filePath = Path.Combine(_workingDirectory, "content.txt");
        File.WriteAllText(filePath, "before");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "content.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        ObjectId targetId = module.GetCurrentCheckout();
        File.WriteAllText(filePath, "after");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "change" }).Should().BeTrue();
        File.WriteAllText(filePath, "dirty");
        return (module, module.GetRevision(targetId, shortFormat: true, loadRefs: false));
    }

    private static Color GetColor(IBrush? brush)
        => brush.Should().BeAssignableTo<ISolidColorBrush>().Which.Color;

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
