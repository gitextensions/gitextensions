using System.Xml;
using System.Xml.Serialization;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.Hotkey;
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
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");
        IHotkeySettingsLoader loader = Substitute.For<IHotkeySettingsLoader>();
        loader.LoadHotkeys(FormBrowse.HotkeySettingsName).Returns(hotkeys);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(loader);

        return (new FormBrowse(commands), commands, notifier);
    }
}
