using System.Xml;
using System.Xml.Serialization;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.Hotkey;
using GitUIPluginInterfaces;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class HotkeyTests
{
    [TestCase(Key.F5, KeyModifiers.None, WinFormsShims.Keys.F5)]
    [TestCase(Key.B, KeyModifiers.Control | KeyModifiers.Shift, WinFormsShims.Keys.B | WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift)]
    [TestCase(Key.OemComma, KeyModifiers.Meta, WinFormsShims.Keys.Oemcomma | WinFormsShims.Keys.Control)]
    [TestCase(Key.OemBackslash, KeyModifiers.None, WinFormsShims.Keys.OemBackslash)]
    [TestCase(Key.Left, KeyModifiers.Alt, WinFormsShims.Keys.Left | WinFormsShims.Keys.Alt)]
    [TestCase(Key.None, KeyModifiers.Control, WinFormsShims.Keys.None)]
    public void KeysMapper_should_map_key_and_modifiers(Key key, KeyModifiers modifiers, WinFormsShims.Keys expected)
    {
        KeysMapper.ToKeys(key, modifiers).Should().Be(expected);
    }

    [Test]
    public void HotkeySettingsManager_should_load_default_browse_hotkeys()
    {
        string? serializedHotkeys = AppSettings.SerializedHotkeys;
        AppSettings.SerializedHotkeys = string.Empty;
        try
        {
            IHotkeySettingsLoader loader = new HotkeySettingsManager();

            IReadOnlyList<HotkeyCommand> hotkeys = loader.LoadHotkeys(FormBrowse.HotkeySettingsName);

            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.Refresh
                && command.KeyData == WinFormsShims.Keys.F5);
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.Commit
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.Space));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.GitBash
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.G));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.MergeBranches
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.M));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.Rebase
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.E));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.FocusRevisionGrid
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.D1));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.FocusCommitInfo
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.D2));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.FocusDiff
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.D3));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.FocusFileTree
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.D4));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.FocusGpgInfo
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.D5));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.FocusGitConsole
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.D6));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.FocusOutputHistoryAndToggleIfPanel
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.D9));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.FocusNextTab
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.Tab));
        }
        finally
        {
            AppSettings.SerializedHotkeys = serializedHotkeys!;
        }
    }

    [Test]
    public void HotkeySettingsManager_should_load_default_resolve_conflicts_hotkeys()
    {
        string? serializedHotkeys = AppSettings.SerializedHotkeys;
        AppSettings.SerializedHotkeys = string.Empty;
        try
        {
            IHotkeySettingsLoader loader = new HotkeySettingsManager();

            IReadOnlyList<HotkeyCommand> hotkeys = loader.LoadHotkeys(FormResolveConflicts.HotkeySettingsName);

            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormResolveConflicts.Commands.ChooseRemote
                && command.KeyData == WinFormsShims.Keys.R);
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormResolveConflicts.Commands.Rescan
                && command.KeyData == WinFormsShims.Keys.F5);
        }
        finally
        {
            AppSettings.SerializedHotkeys = serializedHotkeys!;
        }
    }

    [Test]
    public void HotkeySettingsManager_should_load_default_stash_hotkeys()
    {
        string? serializedHotkeys = AppSettings.SerializedHotkeys;
        AppSettings.SerializedHotkeys = string.Empty;
        try
        {
            IHotkeySettingsLoader loader = new HotkeySettingsManager();

            IReadOnlyList<HotkeyCommand> hotkeys = loader.LoadHotkeys(FormStash.HotkeySettingsName);

            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormStash.Command.NextStash
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.N));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormStash.Command.PreviousStash
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.P));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormStash.Command.Refresh
                && command.KeyData == WinFormsShims.Keys.F5);
        }
        finally
        {
            AppSettings.SerializedHotkeys = serializedHotkeys!;
        }
    }

    [Test]
    public void HotkeySettingsManager_should_load_upstream_revision_grid_hotkeys()
    {
        string? serializedHotkeys = AppSettings.SerializedHotkeys;
        AppSettings.SerializedHotkeys = string.Empty;
        try
        {
            IHotkeySettingsLoader loader = new HotkeySettingsManager();

            IReadOnlyList<HotkeyCommand> hotkeys = loader.LoadHotkeys(RevisionGridControl.HotkeySettingsName);

            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)RevisionGridControl.Command.SelectCurrentRevision
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.C));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)RevisionGridControl.Command.ShowRemoteBranches
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.R));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)RevisionGridControl.Command.RenameRef
                && command.KeyData == WinFormsShims.Keys.F2);
        }
        finally
        {
            AppSettings.SerializedHotkeys = serializedHotkeys!;
        }
    }

    [Test]
    public void HotkeySettingsManager_should_apply_a_persisted_override()
    {
        string? serializedHotkeys = AppSettings.SerializedHotkeys;
        try
        {
            HotkeySettings[] settings =
            [
                new HotkeySettings(
                    FormBrowse.HotkeySettingsName,
                    new HotkeyCommand((int)FormBrowse.Command.Refresh, nameof(FormBrowse.Command.Refresh))
                    {
                        KeyData = WinFormsShims.Keys.F6,
                    }),
            ];
            XmlSerializer serializer = new(typeof(HotkeySettings[]), [typeof(HotkeyCommand)]);
            XmlWriterSettings writerSettings = new() { OmitXmlDeclaration = true };
            using StringWriter writer = new();
            using (XmlWriter xmlWriter = XmlWriter.Create(writer, writerSettings))
            {
                serializer.Serialize(xmlWriter, settings);
            }

            AppSettings.SerializedHotkeys = writer.ToString();
            IHotkeySettingsLoader loader = new HotkeySettingsManager();

            IReadOnlyList<HotkeyCommand> hotkeys = loader.LoadHotkeys(FormBrowse.HotkeySettingsName);

            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.Refresh
                && command.KeyData == WinFormsShims.Keys.F6);
        }
        finally
        {
            AppSettings.SerializedHotkeys = serializedHotkeys!;
        }
    }

    [AvaloniaTest]
    public void FormBrowse_F5_should_dispatch_refresh_through_the_hotkey_command()
    {
        (FormBrowse form, IGitUICommands commands, ILockableNotifier notifier) = CreateBrowseForm(
            new HotkeyCommand((int)FormBrowse.Command.Refresh, nameof(FormBrowse.Command.Refresh))
            {
                KeyData = WinFormsShims.Keys.F5,
            });
        form.Show();
        try
        {
            form.KeyPress(Key.F5, RawInputModifiers.None, PhysicalKey.F5, keySymbol: null);

            notifier.Received(1).Notify();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormBrowse_configured_hotkey_should_dispatch_the_matching_command()
    {
        (FormBrowse form, IGitUICommands commands, _) = CreateBrowseForm(
            new HotkeyCommand((int)FormBrowse.Command.Commit, nameof(FormBrowse.Command.Commit))
            {
                KeyData = WinFormsShims.Keys.Control | WinFormsShims.Keys.Space,
            });
        form.Show();
        try
        {
            form.KeyPress(Key.Space, RawInputModifiers.Control, PhysicalKey.Space, keySymbol: " ");

            commands.Received(1).StartCommitDialog(form);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormBrowse_create_tag_hotkey_should_open_for_the_current_revision()
    {
        (FormBrowse form, IGitUICommands commands, _) = CreateBrowseForm(
            new HotkeyCommand((int)FormBrowse.Command.CreateTag, nameof(FormBrowse.Command.CreateTag))
            {
                KeyData = WinFormsShims.Keys.Control | WinFormsShims.Keys.T,
            });
        form.Show();
        try
        {
            form.KeyPress(Key.T, RawInputModifiers.Control, PhysicalKey.T, keySymbol: "t");

            commands.Received(1).StartCreateTagDialog(form, revision: null);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormBrowse_workspace_hotkeys_should_select_and_cycle_functional_tabs()
    {
        CommitInfoPosition originalPosition = AppSettings.CommitInfoPosition;
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
        AppSettings.ShowSplitViewLayout = true;
        (FormBrowse form, _, _) = CreateBrowseForm(
            new HotkeyCommand((int)FormBrowse.Command.FocusDiff, nameof(FormBrowse.Command.FocusDiff))
            {
                KeyData = WinFormsShims.Keys.F6,
            },
            new HotkeyCommand((int)FormBrowse.Command.FocusFileTree, nameof(FormBrowse.Command.FocusFileTree))
            {
                KeyData = WinFormsShims.Keys.F9,
            },
            new HotkeyCommand((int)FormBrowse.Command.FocusGpgInfo, nameof(FormBrowse.Command.FocusGpgInfo))
            {
                KeyData = WinFormsShims.Keys.F10,
            },
            new HotkeyCommand((int)FormBrowse.Command.FocusNextTab, nameof(FormBrowse.Command.FocusNextTab))
            {
                KeyData = WinFormsShims.Keys.F7,
            },
            new HotkeyCommand((int)FormBrowse.Command.FocusPrevTab, nameof(FormBrowse.Command.FocusPrevTab))
            {
                KeyData = WinFormsShims.Keys.F8,
            });
        form.Show();
        try
        {
            form.KeyPress(Key.F6, RawInputModifiers.None, PhysicalKey.F6, keySymbol: null);
            form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(form.DiffTabPage);

            form.KeyPress(Key.F7, RawInputModifiers.None, PhysicalKey.F7, keySymbol: null);
            form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(form.TreeTabPage);

            form.KeyPress(Key.F8, RawInputModifiers.None, PhysicalKey.F8, keySymbol: null);
            form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(form.DiffTabPage);

            form.KeyPress(Key.F9, RawInputModifiers.None, PhysicalKey.F9, keySymbol: null);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(form.TreeTabPage);
            form.fileTree.FileStatusList.IsKeyboardFocusWithin.Should().BeTrue();

            form.KeyPress(Key.F9, RawInputModifiers.None, PhysicalKey.F9, keySymbol: null);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            form.fileTree.FileViewer.IsKeyboardFocusWithin.Should().BeTrue();

            form.RefreshGpgInfo(new GitRevision(ObjectId.Parse("0123456789012345678901234567890123456789")));
            form.KeyPress(Key.F10, RawInputModifiers.None, PhysicalKey.F10, keySymbol: null);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(form.GpgInfoTabPage);
            form.revisionGpgInfo1.IsKeyboardFocusWithin.Should().BeTrue();
        }
        finally
        {
            form.Close();
            AppSettings.CommitInfoPosition = originalPosition;
            AppSettings.ShowSplitViewLayout = originalShowSplitView;
        }
    }

    [AvaloniaTest]
    public void FormBrowse_should_route_revision_grid_hotkeys_to_the_control()
    {
        bool originalShowRemoteBranches = AppSettings.ShowRemoteBranches;
        (FormBrowse form, _, _) = CreateBrowseFormWithRevisionHotkeys(
            new HotkeyCommand(
                (int)RevisionGridControl.Command.ShowRemoteBranches,
                nameof(RevisionGridControl.Command.ShowRemoteBranches))
            {
                KeyData = WinFormsShims.Keys.F6,
            });
        form.Show();
        try
        {
            form.KeyPress(Key.F6, RawInputModifiers.None, PhysicalKey.F6, keySymbol: null);

            AppSettings.ShowRemoteBranches.Should().Be(!originalShowRemoteBranches);
        }
        finally
        {
            AppSettings.ShowRemoteBranches = originalShowRemoteBranches;
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormBrowse_Escape_should_not_close_the_repository_browser()
    {
        (FormBrowse form, _, _) = CreateBrowseForm();
        form.Show();
        try
        {
            form.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);

            form.IsVisible.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }

    private static (FormBrowse Form, IGitUICommands Commands, ILockableNotifier Notifier) CreateBrowseForm(params HotkeyCommand[] hotkeys)
        => CreateBrowseForm(hotkeys, revisionHotkeys: []);

    private static (FormBrowse Form, IGitUICommands Commands, ILockableNotifier Notifier) CreateBrowseFormWithRevisionHotkeys(
        params HotkeyCommand[] hotkeys)
        => CreateBrowseForm(browseHotkeys: [], revisionHotkeys: hotkeys);

    private static (FormBrowse Form, IGitUICommands Commands, ILockableNotifier Notifier) CreateBrowseForm(
        IReadOnlyList<HotkeyCommand> browseHotkeys,
        IReadOnlyList<HotkeyCommand> revisionHotkeys)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");
        IHotkeySettingsLoader loader = Substitute.For<IHotkeySettingsLoader>();
        loader.LoadHotkeys(FormBrowse.HotkeySettingsName).Returns(browseHotkeys);
        loader.LoadHotkeys(RevisionGridControl.HotkeySettingsName).Returns(revisionHotkeys);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(loader);

        return (new FormBrowse(commands), commands, notifier);
    }
}
