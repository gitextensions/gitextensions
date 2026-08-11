using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommitInfo;
using GitUI.Compat;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class CommitInfoTests
{
    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    public void XhtmlTextBlock_should_preserve_text_links_underlines_and_line_breaks()
    {
        XhtmlTextBlock block = new();
        string? activatedUri = null;
        block.LinkClicked += (_, e) => activatedUri = e.LinkUri;

        block.SetXHTMLText("Author: A &amp; B\n<u>tag</u>: <a href='gitext://gototag/v1'>v1</a><br/>done");

        block.GetPlainText().Should().Be($"Author: A & B{Environment.NewLine}tag: v1{Environment.NewLine}done");
        HyperlinkButton link = block.GetVisualDescendants().OfType<HyperlinkButton>().Single();
        link.Content.Should().Be("v1");
        ToolTip.GetTip(link).Should().Be("gitext://gototag/v1");

        link.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        activatedUri.Should().Be("gitext://gototag/v1");
    }

    [AvaloniaTest]
    public void CommitInfo_should_preserve_the_original_named_surfaces()
    {
        CommitInfo control = new();
        CommitInfo.TestAccessor accessor = control.GetTestAccessor();

        accessor.Header.Should().NotBeNull();
        accessor.Avatar.Should().NotBeNull();
        accessor.CommitMessage.Name.Should().Be("rtbxCommitMessage");
        accessor.RevisionInfo.Name.Should().Be("RevisionInfo");
    }

    [AvaloniaTest]
    public void CommitInfoHeader_should_preserve_the_original_named_surfaces()
    {
        CommitInfoHeader header = new();
        CommitInfoHeader.TestAccessor accessor = header.GetTestAccessor();
        ContextMenu contextMenu = new();

        header.SetContextMenuStrip(contextMenu);

        accessor.Avatar.Name.Should().Be("avatarControl");
        accessor.RevisionHeader.Name.Should().Be("rtbRevisionHeader");
        accessor.RevisionHeader.ContextMenu.Should().BeSameAs(contextMenu);
    }

    [AvaloniaTest]
    public void CommitInfo_should_render_the_hostless_revision_body_and_hide_a_null_revision()
    {
        CommitInfo control = new();
        CommitInfo.TestAccessor accessor = control.GetTestAccessor();
        GitRevision revision = new(ObjectId.Parse("1234567890abcdef1234567890abcdef12345678"))
        {
            Subject = "Commit subject",
            Body = "Commit body",
        };

        control.Revision = revision;

        accessor.TableLayout.IsVisible.Should().BeTrue();
        accessor.CommitMessage.GetPlainText().Should().Be("Commit body");

        control.Revision = null;

        accessor.TableLayout.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void CommitInfo_should_project_the_configured_add_notes_shortcut_to_its_menu()
    {
        IHotkeySettingsLoader loader = Substitute.For<IHotkeySettingsLoader>();
        loader.LoadHotkeys(FormBrowse.HotkeySettingsName).Returns(
        [
            new HotkeyCommand((int)FormBrowse.Command.AddNotes, nameof(FormBrowse.Command.AddNotes))
            {
                KeyData = WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.N,
            },
        ]);
        IGitModule module = Substitute.For<IGitModule>();
        module.IsValidGitWorkingDir().Returns(false);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(loader);
        commands.GetService(typeof(ILinkFactory)).Returns(new LinkFactory());
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(commands);

        CommitInfo control = new() { UICommandsSource = source };
        KeyGesture? gesture = control.GetTestAccessor().AddNoteMenuItem.InputGesture;

        gesture.Should().NotBeNull();
        gesture!.Key.Should().Be(Key.N);
        gesture.KeyModifiers.Should().Be(KeyModifiers.Control | KeyModifiers.Shift);
        loader.Received().LoadHotkeys(FormBrowse.HotkeySettingsName);
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void CommitInfo_context_menu_should_toggle_the_original_settings()
    {
        bool originalLocal = AppSettings.CommitInfoShowContainedInBranchesLocal;
        bool originalRemote = AppSettings.CommitInfoShowContainedInBranchesRemote;
        bool originalRemoteIfNoLocal = AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
        bool originalTags = AppSettings.CommitInfoShowContainedInTags;
        bool originalAnnotated = AppSettings.ShowAnnotatedTagsMessages;
        bool originalDerived = AppSettings.CommitInfoShowTagThisCommitDerivesFrom;
        try
        {
            CommitInfo.TestAccessor accessor = new CommitInfo().GetTestAccessor();

            accessor.ShowLocalBranchesMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            accessor.ShowRemoteBranchesMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            accessor.ShowRemoteBranchesIfNoLocalMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            accessor.ShowTagsMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            accessor.ShowAnnotatedTagMessagesMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            accessor.ShowDerivedTagMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            AppSettings.CommitInfoShowContainedInBranchesLocal.Should().Be(!originalLocal);
            AppSettings.CommitInfoShowContainedInBranchesRemote.Should().Be(!originalRemote);
            AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal.Should().Be(!originalRemoteIfNoLocal);
            AppSettings.CommitInfoShowContainedInTags.Should().Be(!originalTags);
            AppSettings.ShowAnnotatedTagsMessages.Should().Be(!originalAnnotated);
            AppSettings.CommitInfoShowTagThisCommitDerivesFrom.Should().Be(!originalDerived);
        }
        finally
        {
            AppSettings.CommitInfoShowContainedInBranchesLocal = originalLocal;
            AppSettings.CommitInfoShowContainedInBranchesRemote = originalRemote;
            AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal = originalRemoteIfNoLocal;
            AppSettings.CommitInfoShowContainedInTags = originalTags;
            AppSettings.ShowAnnotatedTagsMessages = originalAnnotated;
            AppSettings.CommitInfoShowTagThisCommitDerivesFrom = originalDerived;
        }
    }

    [AvaloniaTest]
    public void CommitInfo_should_preserve_menu_translation_keys_without_empty_text_keys()
    {
        CommitInfo control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(CommitInfo), "addNoteToolStripMenuItem", "Text", "Add &notes");
        translation.DidNotReceive().AddTranslationItem(nameof(CommitInfo), "RevisionInfo", "Text", Arg.Any<string>());
        translation.DidNotReceive().AddTranslationItem(nameof(CommitInfo), "rtbxCommitMessage", "Text", Arg.Any<string>());
    }
}
