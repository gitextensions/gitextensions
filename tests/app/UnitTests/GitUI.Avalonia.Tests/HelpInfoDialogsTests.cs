using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class HelpInfoDialogsTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void EnvironmentInfo_should_construct_and_show_the_environment_report()
    {
        UserEnvironmentInformation.Initialise("9999999999999999999999999999999999abcdef", isDirty: true);

        EnvironmentInfo control = new();
        EnvironmentInfo.TestAccessor accessor = control.GetTestAccessor();

        accessor.CopyButton.Should().NotBeNull();
        accessor.EnvironmentIssueInfo.Text.Should().Contain("Git Extensions");
    }

    [AvaloniaTest]
    public void FormCommandlineHelp_should_construct_and_list_the_commands()
    {
        FormCommandlineHelp form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormCommandlineHelp), "$this", "Text", "Commandline usage");
        translation.Received(1).AddTranslationItem(nameof(FormCommandlineHelp), "label1", "Text", "Supported commandline arguments for\ngitex.cmd / gitex (located in the same folder as GitExtensions.exe):");
    }

    [AvaloniaTest]
    public void FormDonate_should_construct_and_emit_its_translation_keys()
    {
        FormDonate form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormDonate), "$this", "Text", "Donate");
        translation.Received(1).AddTranslationItem(nameof(FormDonate), "_donateText", "Text", Arg.Any<string>());
        FormDonate.DonationUrl.Should().Be("https://opencollective.com/gitextensions");
    }

    [AvaloniaTest]
    public void FormOpenDirectory_should_construct_with_the_original_controls_and_keys()
    {
        FormOpenDirectory form = new();
        FormOpenDirectory.TestAccessor accessor = form.GetTestAccessor();

        accessor.Directory.Should().NotBeNull();
        accessor.OpenButton.Should().NotBeNull();

        ITranslation translation = Substitute.For<ITranslation>();
        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormOpenDirectory), "$this", "Text", "Open local repository");
        translation.Received(1).AddTranslationItem(nameof(FormOpenDirectory), "label1", "Text", "&Directory:");
        translation.Received(1).AddTranslationItem(nameof(FormOpenDirectory), "Load", "Text", "Open");
        translation.Received(1).AddTranslationItem(nameof(FormOpenDirectory), "folderBrowserButton", "Text", "&Browse...");
        translation.Received(1).AddTranslationItem(nameof(FormOpenDirectory), "folderGoUpButton", "toolTip1", "Go to parent directory...");
    }

    [AvaloniaTest]
    public void FormOpenDirectory_should_reject_a_non_repository_path()
    {
        IGitExecutorProvider executorProvider = Substitute.For<IGitExecutorProvider>();
        ILocalRepositoryManager localRepositoryManager = Substitute.For<ILocalRepositoryManager>();

        string missing = Path.Combine(Path.GetTempPath(), $"ge-open-{Guid.NewGuid():N}");

        FormOpenDirectory.TestAccessor.OpenGitRepository(executorProvider, missing, localRepositoryManager)
            .Should().BeNull();
    }
}
