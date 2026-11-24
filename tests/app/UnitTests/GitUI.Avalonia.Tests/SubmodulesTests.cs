using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SubmodulesDialog;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class SubmodulesTests
{
    private static readonly ObjectId CommitId = ObjectId.Parse("1111111111111111111111111111111111111111");

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormSubmodules_should_construct_and_display_the_selected_submodule()
    {
        FormSubmodules form = new();
        GitSubmoduleInfo first = new(
            "first",
            "externals/first",
            "https://example.com/first.git",
            CommitId,
            "main",
            isInitialized: true,
            isUpToDate: true);
        GitSubmoduleInfo second = new(
            "second",
            "externals/second",
            "https://example.com/second.git",
            CommitId,
            "stable",
            isInitialized: true,
            isUpToDate: false);

        FormSubmodules.TestAccessor accessor = form.GetTestAccessor();
        accessor.SetModules([first, second], second.LocalPath);

        accessor.Submodules.ItemCount.Should().Be(2);
        accessor.Submodules.SelectedItem.Should().BeSameAs(second);
        accessor.Name.Text.Should().Be(second.Name);
        accessor.RemotePath.Text.Should().Be(second.RemotePath);
        accessor.LocalPath.Text.Should().Be(second.LocalPath);
        accessor.Commit.Text.Should().Be(CommitId.ToString());
        accessor.Branch.Text.Should().Be(second.Branch);
        accessor.Status.Text.Should().Be("Modified");
        accessor.Remove.IsEnabled.Should().BeTrue();
        accessor.Pull.IsEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormAddSubmodule_should_derive_the_local_path_from_the_remote()
    {
        FormAddSubmodule form = new();
        FormAddSubmodule.TestAccessor accessor = form.GetTestAccessor();

        accessor.Directory.Text = "https://github.com/AvaloniaUI/Avalonia.git";

        accessor.LocalPath.Text.Should().Be("Avalonia");
    }

    [AvaloniaTest]
    public async Task FormSubmodules_should_load_submodules_without_blocking_the_UI_thread()
    {
        GitSubmoduleInfo submodule = new(
            "first",
            "externals/first",
            "https://example.com/first.git",
            CommitId,
            "main",
            isInitialized: true,
            isUpToDate: true);
        IGitModule module = Substitute.For<IGitModule>();
        module.GetSubmodulesInfo().Returns([submodule]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormSubmodules form = new(commands);
        form.Show();
        try
        {
            FormSubmodules.TestAccessor accessor = form.GetTestAccessor();
            await WaitUntilAsync(() => accessor.Submodules.ItemCount == 1);

            accessor.Submodules.SelectedItem.Should().BeSameAs(submodule);
            accessor.Name.Text.Should().Be(submodule.Name);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Submodule_forms_should_use_the_existing_translation_keys_once()
    {
        FormSubmodules manager = new();
        FormAddSubmodule add = new();
        ITranslation translation = Substitute.For<ITranslation>();

        manager.AddTranslationItems(translation);
        manager.TranslateItems(translation);
        add.AddTranslationItems(translation);
        add.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormSubmodules), "$this", "Text", "Submodules");
        translation.Received(1).AddTranslationItem(nameof(FormSubmodules), "AddSubmodule", "Text", "Add submodule");
        translation.Received(1).AddTranslationItem(nameof(FormSubmodules), "nameDataGridViewTextBoxColumn", "HeaderText", "Name");
        translation.Received(1).AddTranslationItem(nameof(FormSubmodules), "Status", "HeaderText", "Status");
        translation.Received(1).AddTranslationItem(nameof(FormSubmodules), "groupBox1", "Text", "Details");
        translation.Received(1).AddTranslationItem(nameof(FormSubmodules), "_removeSelectedSubmodule", "Text", "Are you sure you want remove the selected submodule?");
        translation.Received(1).AddTranslationItem(nameof(FormAddSubmodule), "$this", "Text", "Add submodule");
        translation.Received(1).AddTranslationItem(nameof(FormAddSubmodule), "Add", "Text", "Add");
        translation.Received(1).AddTranslationItem(nameof(FormAddSubmodule), "Browse", "Text", "Browse");
        translation.Received(1).AddTranslationItem(nameof(FormAddSubmodule), "chkForce", "Text", "Force");
        translation.Received(1).AddTranslationItem(nameof(FormAddSubmodule), "_remoteAndLocalPathRequired", "Text", "A remote path and local path are required");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(
            emittedKeys.Length,
            "each field must be routed through exactly one translation path");
    }

    private static async Task WaitUntilAsync(Func<bool> predicate)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (!predicate() && DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        predicate().Should().BeTrue("the asynchronous submodule load should complete");
    }
}
