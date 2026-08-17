using System.Xml;
using System.Xml.Serialization;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.Compat;
using GitUI.Editor;
using GitUI.Hotkey;
using GitUI.LeftPanel;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class HotkeyTests
{
    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

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

    [TestCase(WinFormsShims.Keys.F5, Key.F5, KeyModifiers.None)]
    [TestCase(WinFormsShims.Keys.B | WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift, Key.B, KeyModifiers.Control | KeyModifiers.Shift)]
    [TestCase(WinFormsShims.Keys.Oemcomma | WinFormsShims.Keys.Alt, Key.OemComma, KeyModifiers.Alt)]
    public void KeysMapper_should_map_persisted_hotkeys_to_Avalonia_gestures(
        WinFormsShims.Keys keyData,
        Key expectedKey,
        KeyModifiers expectedModifiers)
    {
        KeyGesture gesture = KeysMapper.ToKeyGesture(keyData)!;

        gesture.Key.Should().Be(expectedKey);
        gesture.KeyModifiers.Should().Be(expectedModifiers);
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
                command.CommandCode == (int)FormBrowse.Command.ManageWorkTrees
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.Alt | WinFormsShims.Keys.W));
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
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.OpenSettings
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.Oemcomma));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormBrowse.Command.QuickFetch
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.Down));
        }
        finally
        {
            AppSettings.SerializedHotkeys = serializedHotkeys!;
        }
    }

    [Test]
    public void HotkeySettingsManager_should_load_the_original_commit_dialog_hotkeys()
    {
        string? serializedHotkeys = AppSettings.SerializedHotkeys;
        AppSettings.SerializedHotkeys = string.Empty;
        try
        {
            IHotkeySettingsLoader loader = new HotkeySettingsManager();
            IReadOnlyList<HotkeyCommand> hotkeys = loader.LoadHotkeys(FormCommit.HotkeySettingsName);

            hotkeys.Should().HaveCount(18);
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormCommit.Command.FocusUnstagedFiles
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.D1));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormCommit.Command.FocusCommitMessage
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.D4));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormCommit.Command.ToggleSelectionFilter
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.F));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormCommit.Command.StageAll
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.S));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormCommit.Command.OpenWithDifftool
                && command.KeyData == WinFormsShims.Keys.F3);
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormCommit.Command.ConventionalCommit_PrefixMessageWithScope
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.T));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormCommit.Command.SelectNext_AlternativeHotkey1
                && command.KeyData == (WinFormsShims.Keys.Alt | WinFormsShims.Keys.Down));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FormCommit.Command.SelectPrevious_AlternativeHotkey2
                && command.KeyData == (WinFormsShims.Keys.Alt | WinFormsShims.Keys.Left));
        }
        finally
        {
            AppSettings.SerializedHotkeys = serializedHotkeys ?? string.Empty;
        }
    }

    [Test]
    public void HotkeySettingsManager_should_expose_user_scripts_in_the_original_settings_category()
    {
        string? serializedHotkeys = AppSettings.SerializedHotkeys;
        AppSettings.SerializedHotkeys = string.Empty;
        try
        {
            ScriptInfo script = new() { Name = "&Review", HotkeyCommandIdentifier = 9012 };
            IScriptsManager scriptsManager = Substitute.For<IScriptsManager>();
            scriptsManager.GetScripts().Returns(new System.ComponentModel.BindingList<ScriptInfo>([script]));
            IHotkeySettingsLoader loader = new HotkeySettingsManager(scriptsManager);

            IReadOnlyList<HotkeyCommand> hotkeys = loader.LoadHotkeys(FormSettings.HotkeySettingsName);

            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == script.HotkeyCommandIdentifier
                && command.Name == "Review"
                && command.KeyData == WinFormsShims.Keys.None);
        }
        finally
        {
            AppSettings.SerializedHotkeys = serializedHotkeys!;
        }
    }

    [Test]
    public void HotkeySettingsManager_should_load_default_FileViewer_hotkeys()
    {
        string? serializedHotkeys = AppSettings.SerializedHotkeys;
        AppSettings.SerializedHotkeys = string.Empty;
        try
        {
            IHotkeySettingsLoader loader = new HotkeySettingsManager();

            IReadOnlyList<HotkeyCommand> hotkeys = loader.LoadHotkeys(FileViewer.HotkeySettingsName);

            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FileViewer.Command.Find
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.F));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FileViewer.Command.FindNextOrOpenWithDifftool
                && command.KeyData == WinFormsShims.Keys.F3);
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FileViewer.Command.FindPrevious
                && command.KeyData == (WinFormsShims.Keys.Shift | WinFormsShims.Keys.F3));
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)FileViewer.Command.GoToLine
                && command.KeyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.G));
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

    [Test]
    public void HotkeySettingsManager_should_save_the_edited_reduced_settings()
    {
        string? serializedHotkeys = AppSettings.SerializedHotkeys;
        AppSettings.SerializedHotkeys = string.Empty;
        try
        {
            IHotkeySettingsManager manager = new HotkeySettingsManager();
            IReadOnlyList<HotkeySettings> settings = manager.LoadSettings();
            HotkeyCommand refresh = settings
                .Single(setting => setting.Name == FormBrowse.HotkeySettingsName)
                .Commands!
                .Single(command => command.CommandCode == (int)FormBrowse.Command.Refresh);
            refresh.KeyData = WinFormsShims.Keys.F6;

            manager.SaveSettings(settings);

            AppSettings.SerializedHotkeys.Should().Contain(nameof(FormBrowse.Command.Refresh));
            manager.IsUniqueKey(WinFormsShims.Keys.F6).Should().BeTrue();
            manager.LoadHotkeys(FormBrowse.HotkeySettingsName)
                .Should()
                .ContainSingle(command =>
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
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public void FormBrowse_should_format_toolbar_shortcuts_like_the_original()
    {
        (FormBrowse form, _, _) = CreateBrowseForm(
            new HotkeyCommand((int)FormBrowse.Command.OpenSettings, nameof(FormBrowse.Command.OpenSettings))
            {
                KeyData = WinFormsShims.Keys.Control | WinFormsShims.Keys.Oemcomma,
            },
            new HotkeyCommand((int)FormBrowse.Command.QuickFetch, nameof(FormBrowse.Command.QuickFetch))
            {
                KeyData = WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.Down,
            });

        ToolTip.GetTip(form.EditSettings).Should().Be("Settings\u00A0(Ctrl+,)");
        IconButton quickFetch = form.ToolStripMain.Children
            .OfType<IconButton>()
            .Single(button => button.Name == FormBrowse.FetchPullToolbarShortcutsPrefix + form.fetchToolStripMenuItem.Name);
        ToolTip.GetTip(quickFetch).Should().Be("Fetch\u00A0(Ctrl+Shift+Down)");
    }

    [Test]
    [Category("P4.3")]
    public void HotkeySettingsManager_should_load_the_original_left_panel_hotkeys()
    {
        string? serializedHotkeys = AppSettings.SerializedHotkeys;
        AppSettings.SerializedHotkeys = string.Empty;
        try
        {
            IHotkeySettingsLoader loader = new HotkeySettingsManager();

            IReadOnlyList<HotkeyCommand> hotkeys = loader.LoadHotkeys(RepoObjectsTree.HotkeySettingsName);

            hotkeys.Should().BeEquivalentTo(
            [
                new HotkeyCommand((int)RepoObjectsTree.Command.Delete, nameof(RepoObjectsTree.Command.Delete)) { KeyData = WinFormsShims.Keys.Delete },
                new HotkeyCommand((int)RepoObjectsTree.Command.MultiSelect, nameof(RepoObjectsTree.Command.MultiSelect)) { KeyData = WinFormsShims.Keys.Control | WinFormsShims.Keys.Space },
                new HotkeyCommand((int)RepoObjectsTree.Command.MultiSelectWithChildren, nameof(RepoObjectsTree.Command.MultiSelectWithChildren)) { KeyData = WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.Space },
                new HotkeyCommand((int)RepoObjectsTree.Command.Rename, nameof(RepoObjectsTree.Command.Rename)) { KeyData = WinFormsShims.Keys.F2 },
                new HotkeyCommand((int)RepoObjectsTree.Command.Search, nameof(RepoObjectsTree.Command.Search)) { KeyData = WinFormsShims.Keys.F3 },
            ], options => options.WithStrictOrdering());
        }
        finally
        {
            AppSettings.SerializedHotkeys = serializedHotkeys!;
        }
    }

    [AvaloniaTest]
    [Category("P4.3")]
    public void FormBrowse_should_route_left_panel_hotkeys_when_the_repository_tree_has_focus()
    {
        (FormBrowse form, IGitUICommands commands, _) = CreateBrowseForm(
            browseHotkeys: [],
            revisionHotkeys: [],
            leftPanelHotkeys:
            [
                new HotkeyCommand((int)RepoObjectsTree.Command.Delete, nameof(RepoObjectsTree.Command.Delete))
                {
                    KeyData = WinFormsShims.Keys.Delete,
                },
            ]);
        form.Show();
        try
        {
            form.FindControl<Grid>("mainContentGrid")!.IsVisible = true;
            form.FindControl<Border>("leftPanel")!.IsVisible = true;
            RepoObjectsTree tree = form.FindControl<RepoObjectsTree>("repoObjectsTree")!;
            tree.SetRefs([new GitRef(commands.Module, ObjectId.Random(), "refs/heads/feature")], [], "main");
            RepoObjectsTree.TestAccessor accessor = tree.GetTestAccessor();
            TreeViewItem branch = accessor.Tree.Items.Cast<TreeViewItem>().First().Items.Cast<TreeViewItem>().Single();
            accessor.Tree.SelectedItem = branch;
            Dispatcher.UIThread.RunJobs();
            accessor.Tree.Focusable = true;
            accessor.Tree.Focus(NavigationMethod.Tab, KeyModifiers.None);
            Dispatcher.UIThread.RunJobs();
            tree.IsKeyboardFocusWithin.Should().BeTrue();

            form.KeyPress(Key.Delete, RawInputModifiers.None, PhysicalKey.Delete, keySymbol: null);

            commands.Received(1).StartDeleteBranchDialog(tree, "feature");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [Category("P4.2")]
    public void FormBrowse_menu_access_key_should_take_precedence_over_a_user_script_hotkey()
    {
        ScriptInfo script = new()
        {
            Name = "Conflicting script",
            HotkeyCommandIdentifier = 9014,
        };
        IScriptsManager scriptsManager = Substitute.For<IScriptsManager>();
        scriptsManager.GetScripts().Returns(new System.ComponentModel.BindingList<ScriptInfo>([script]));
        scriptsManager.GetScript(script.HotkeyCommandIdentifier).Returns(script);
        IScriptsRunner scriptsRunner = Substitute.For<IScriptsRunner>();
        (FormBrowse form, _, _) = CreateBrowseForm(
            browseHotkeys: [],
            revisionHotkeys: [],
            scriptsManager,
            scriptsRunner,
            scriptHotkeys:
            [
                new HotkeyCommand(script.HotkeyCommandIdentifier, script.Name)
                {
                    KeyData = WinFormsShims.Keys.Alt | WinFormsShims.Keys.S,
                },
            ]);
        form.Show();
        try
        {
            form.KeyPress(Key.S, RawInputModifiers.Alt, PhysicalKey.S, keySymbol: "s");

            form.fileToolStripMenuItem.IsSubMenuOpen.Should().BeTrue();
            scriptsRunner.DidNotReceive().RunScript(
                Arg.Any<ScriptInfo>(),
                Arg.Any<IWin32Window>(),
                Arg.Any<IGitUICommands>(),
                Arg.Any<IScriptOptionsProvider>());
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [Category("P4.2")]
    public void FormBrowse_unmapped_key_should_not_execute_an_unassigned_user_script_hotkey()
    {
        ScriptInfo script = new()
        {
            Name = "Unassigned script",
            HotkeyCommandIdentifier = 9015,
        };
        IScriptsManager scriptsManager = Substitute.For<IScriptsManager>();
        scriptsManager.GetScripts().Returns(new System.ComponentModel.BindingList<ScriptInfo>([script]));
        scriptsManager.GetScript(script.HotkeyCommandIdentifier).Returns(script);
        IScriptsRunner scriptsRunner = Substitute.For<IScriptsRunner>();
        (FormBrowse form, _, _) = CreateBrowseForm(
            browseHotkeys: [],
            revisionHotkeys: [],
            scriptsManager,
            scriptsRunner,
            scriptHotkeys:
            [
                new HotkeyCommand(script.HotkeyCommandIdentifier, script.Name)
                {
                    KeyData = WinFormsShims.Keys.None,
                },
            ]);
        form.Show();
        try
        {
            form.KeyPress(Key.LeftAlt, RawInputModifiers.Alt, PhysicalKey.AltLeft, keySymbol: null);

            scriptsRunner.DidNotReceive().RunScript(
                Arg.Any<ScriptInfo>(),
                Arg.Any<IWin32Window>(),
                Arg.Any<IGitUICommands>(),
                Arg.Any<IScriptOptionsProvider>());
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public void GitExtensionsFormBase_should_not_dispatch_an_unassigned_hotkey()
    {
        HotkeyCommand unassigned = new(17, "Unassigned") { KeyData = WinFormsShims.Keys.None };
        IGitUICommands commands = CreateHotkeyCommands([unassigned]);
        TestHotkeyForm form = new(commands);

        form.ProcessHotkey(WinFormsShims.Keys.None).Should().BeFalse();
        form.ExecutedCommand.Should().BeNull();

        form.ProcessHotkey(WinFormsShims.Keys.F5).Should().BeFalse();
        form.ExecutedCommand.Should().BeNull();
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public void GitExtensionsFormBase_should_dispatch_an_assigned_hotkey()
    {
        HotkeyCommand assigned = new(17, "Refresh") { KeyData = WinFormsShims.Keys.F5 };
        IGitUICommands commands = CreateHotkeyCommands([assigned]);
        TestHotkeyForm form = new(commands);

        form.ProcessHotkey(WinFormsShims.Keys.F5).Should().BeTrue();
        form.ExecutedCommand.Should().Be(assigned.CommandCode);
    }

    [Test]
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public void GitExtensionsControl_should_not_dispatch_an_unassigned_hotkey()
    {
        HotkeyCommand unassigned = new(19, "Unassigned") { KeyData = WinFormsShims.Keys.None };
        IGitUICommands commands = CreateHotkeyCommands([unassigned]);
        TestHotkeyControl control = new(commands);

        control.ProcessHotkey(WinFormsShims.Keys.None).Should().BeFalse();
        control.ExecutedCommand.Should().BeNull();

        control.ProcessHotkey(WinFormsShims.Keys.F5).Should().BeFalse();
        control.ExecutedCommand.Should().BeNull();
    }

    [Test]
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public void GitExtensionsControl_should_dispatch_an_assigned_hotkey()
    {
        HotkeyCommand assigned = new(19, "Refresh") { KeyData = WinFormsShims.Keys.F5 };
        IGitUICommands commands = CreateHotkeyCommands([assigned]);
        TestHotkeyControl control = new(commands);

        control.ProcessHotkey(WinFormsShims.Keys.F5).Should().BeTrue();
        control.ExecutedCommand.Should().Be(assigned.CommandCode);
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public void FileStatusList_should_not_dispatch_an_unassigned_hotkey()
    {
        (FormBrowse form, _, _) = CreateBrowseForm(
            browseHotkeys: [],
            revisionHotkeys: [],
            fileStatusHotkeys:
            [
                new HotkeyCommand(
                    (int)RevisionDiffControl.Command.StageSelectedFile,
                    nameof(RevisionDiffControl.Command.StageSelectedFile))
                {
                    KeyData = WinFormsShims.Keys.None,
                },
            ]);
        FileStatusList fileStatusList = form.fileTree.FileStatusList;
        int stageInvocations = 0;
        fileStatusList.GetTestAccessor().StageMenuItem.Click += (_, _) => stageInvocations++;

        fileStatusList.ProcessHotkey(WinFormsShims.Keys.None).Should().BeFalse();

        stageInvocations.Should().Be(0);
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public void FormBrowse_modifier_only_key_should_not_dispatch_an_unassigned_revision_grid_hotkey()
    {
        bool originalShowRemoteBranches = AppSettings.ShowRemoteBranches;
        (FormBrowse form, _, _) = CreateBrowseFormWithRevisionHotkeys(
            new HotkeyCommand(
                (int)RevisionGridControl.Command.ShowRemoteBranches,
                nameof(RevisionGridControl.Command.ShowRemoteBranches))
            {
                KeyData = WinFormsShims.Keys.None,
            });
        form.Show();
        try
        {
            form.FindControl<Grid>("mainContentGrid")!.IsVisible = true;
            form.RevisionGrid.GetTestAccessor().SetRevisions(
                [new GitRevision(ObjectId.Random()) { Subject = "Selected revision" }]);
            form.RevisionGrid.FocusRevisionGrid();
            Dispatcher.UIThread.RunJobs();
            form.RevisionGrid.GetTestAccessor().Revisions.IsKeyboardFocusWithin.Should().BeTrue();

            form.KeyPress(Key.LeftAlt, RawInputModifiers.Alt, PhysicalKey.AltLeft, keySymbol: null);

            AppSettings.ShowRemoteBranches.Should().Be(originalShowRemoteBranches);
        }
        finally
        {
            AppSettings.ShowRemoteBranches = originalShowRemoteBranches;
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormBrowse_should_build_the_user_script_toolbar_from_the_shared_manager()
    {
        ScriptInfo script = new()
        {
            Name = "Review",
            HotkeyCommandIdentifier = 9014,
            OnEvent = ScriptEvent.ShowInUserMenuBar,
            Icon = "EditFile",
        };
        IScriptsManager scriptsManager = Substitute.For<IScriptsManager>();
        scriptsManager.GetScripts().Returns(new System.ComponentModel.BindingList<ScriptInfo>([script]));
        IScriptsRunner scriptsRunner = Substitute.For<IScriptsRunner>();
        scriptsRunner.RunScript(script, Arg.Any<IWin32Window>(), Arg.Any<IGitUICommands>(), Arg.Any<IScriptOptionsProvider>()).Returns(true);
        (FormBrowse form, IGitUICommands commands, _) = CreateBrowseForm(
            browseHotkeys: [],
            revisionHotkeys: [],
            scriptsManager,
            scriptsRunner,
            scriptHotkeys: []);
        scriptsManager.GetScript(script.HotkeyCommandIdentifier).Returns(script);
        StackPanel toolbar = form.FindControl<StackPanel>("ToolStripScripts")!;

        toolbar.IsVisible.Should().BeTrue();
        IconButton button = toolbar.Children.Should().ContainSingle().Which.Should().BeOfType<IconButton>().Subject;
        button.Content.Should().Be("Review");
        button.Icon.Should().NotBeNull();
        button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

        scriptsRunner.Received(1).RunScript(script, form, commands, Arg.Any<IScriptOptionsProvider>());
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
        bool originalShowGpgInformation = AppSettings.ShowGpgInformation.Value;
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
        AppSettings.ShowGpgInformation.Value = true;
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
            AppSettings.ShowGpgInformation.Value = originalShowGpgInformation;
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
        IReadOnlyList<HotkeyCommand> revisionHotkeys,
        IScriptsManager? scriptsManager = null,
        IScriptsRunner? scriptsRunner = null,
        IReadOnlyList<HotkeyCommand>? scriptHotkeys = null,
        IReadOnlyList<HotkeyCommand>? leftPanelHotkeys = null,
        IReadOnlyList<HotkeyCommand>? fileStatusHotkeys = null)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(true);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");
        IHotkeySettingsLoader loader = Substitute.For<IHotkeySettingsLoader>();
        IRepositoryHistoryUIService repositoryHistory = Substitute.For<IRepositoryHistoryUIService>();
        IUserRepositoriesListController repositoriesController = Substitute.For<IUserRepositoriesListController>();
        loader.LoadHotkeys(FormBrowse.HotkeySettingsName).Returns(browseHotkeys);
        loader.LoadHotkeys(RevisionGridControl.HotkeySettingsName).Returns(revisionHotkeys);
        loader.LoadHotkeys(FormSettings.HotkeySettingsName).Returns(scriptHotkeys ?? []);
        loader.LoadHotkeys(RepoObjectsTree.HotkeySettingsName).Returns(leftPanelHotkeys ?? []);
        loader.LoadHotkeys(RevisionDiffControl.HotkeySettingsName).Returns(fileStatusHotkeys ?? []);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(loader);
        commands.GetService(typeof(IRepositoryHistoryUIService)).Returns(repositoryHistory);
        commands.GetService(typeof(IUserRepositoriesListController)).Returns(repositoriesController);
        commands.GetService(typeof(IScriptsManager)).Returns(scriptsManager);
        commands.GetService(typeof(IScriptsRunner)).Returns(scriptsRunner);

        return (new FormBrowse(commands), commands, notifier);
    }

    private static IGitUICommands CreateHotkeyCommands(IReadOnlyList<HotkeyCommand> hotkeys)
    {
        IHotkeySettingsLoader loader = Substitute.For<IHotkeySettingsLoader>();
        loader.LoadHotkeys("Test").Returns(hotkeys);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(loader);
        return commands;
    }

    private sealed class TestHotkeyForm : GitExtensionsFormBase
    {
        private readonly IGitUICommands _commands;

        public TestHotkeyForm(IGitUICommands commands)
        {
            _commands = commands;
            HotkeysEnabled = true;
            LoadHotkeys("Test");
        }

        public int? ExecutedCommand { get; private set; }

        public override bool TryGetUICommands([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IGitUICommands? commands)
        {
            commands = _commands;
            return true;
        }

        protected override bool ExecuteCommand(int command)
        {
            ExecutedCommand = command;
            return true;
        }
    }

    private sealed class TestHotkeyControl : GitExtensionsControl
    {
        private readonly IServiceProvider _serviceProvider;

        public TestHotkeyControl(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            HotkeysEnabled = true;
            LoadHotkeys("Test");
        }

        public int? ExecutedCommand { get; private set; }

        protected override IServiceProvider ServiceProvider => _serviceProvider;

        protected override bool ExecuteCommand(int command)
        {
            ExecutedCommand = command;
            return true;
        }
    }
}
