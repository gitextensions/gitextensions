using Avalonia.Controls;
using Avalonia.Input;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtUtils;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using ResourceManager;
using ResourceManager.Hotkey;

namespace GitUI.CommandsDialogs.Menus;

internal partial class ToolsToolStripMenuItem : ToolStripMenuItemEx
{
    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    public ToolsToolStripMenuItem()
    {
        InitializeComponent();

        SubmenuOpened += (_, _) =>
        {
            if (HasUICommands)
            {
                RefreshState(Module.IsBareRepository());
            }
        };
        gitBashToolStripMenuItem.Click += gitBashToolStripMenuItem_Click;
        gitGUIToolStripMenuItem.Click += GitGuiToolStripMenuItemClick;
        kGitToolStripMenuItem.Click += KGitToolStripMenuItemClick;

        // The original menu item carries a static Keys.F12 accelerator; FormBrowse routes it
        // while this submenu is closed because Avalonia otherwise only displays the gesture.
        gitcommandLogToolStripMenuItem.InputGesture = new KeyGesture(Key.F12);
        gitcommandLogToolStripMenuItem.Click += GitcommandLogToolStripMenuItemClick;
        settingsToolStripMenuItem.Click += OnShowSettingsClick;
        InputAccessibility.Apply(this);
    }

    public override void RefreshShortcutKeys(IEnumerable<HotkeyCommand>? hotkeys)
    {
        gitBashToolStripMenuItem.InputGesture = GetGesture(FormBrowse.Command.GitBash);
        gitGUIToolStripMenuItem.InputGesture = GetGesture(FormBrowse.Command.GitGui);
        kGitToolStripMenuItem.InputGesture = GetGesture(FormBrowse.Command.GitGitK);
        settingsToolStripMenuItem.InputGesture = GetGesture(FormBrowse.Command.OpenSettings);

        base.RefreshShortcutKeys(hotkeys);

        return;

        Avalonia.Input.KeyGesture? GetGesture(FormBrowse.Command command)
            => KeysMapper.ToKeyGesture(
                hotkeys?.FirstOrDefault(hotkey => hotkey.CommandCode == (int)command)?.KeyData);
    }

    public override void RefreshState(bool bareRepository)
    {
        gitGUIToolStripMenuItem.IsEnabled = !bareRepository;

        base.RefreshState(bareRepository);
    }

    private void GitcommandLogToolStripMenuItemClick(object? sender, EventArgs e)
    {
        FormGitCommandLog.ShowOrActivate(OwnerForm!);
    }

    private void GitGuiToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.Module.RunGui();
    }

    private void KGitToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.Module.RunGitK();
    }

    private void OnShowSettingsClick(object? sender, EventArgs e)
    {
        string translation = AppSettings.Translation;
        CommitInfoPosition commitInfoPosition = AppSettings.CommitInfoPosition;

        UICommands.StartSettingsDialog(OwnerForm);

        SettingsChanged?.Invoke(sender, new(translation, commitInfoPosition));
    }

    private void gitBashToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        try
        {
            UICommands.GetRequiredService<ITerminalLauncher>().Launch(UICommands.Module.WorkingDir);
        }
        catch (PlatformNotSupportedException exception) when (FlatpakEnvironment.IsFlatpak())
        {
            // Cross-platform constraint: a confined app cannot execute a host terminal.
            MessageBoxes.FailedToRunShell(OwnerForm, "Git bash", exception);
        }
        catch (Exception exception)
        {
            MessageBoxes.FailedToRunShell(OwnerForm, "Git bash", exception);
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(ToolsToolStripMenuItem menu)
    {
        public MenuItem GitBashMenuItem => menu.gitBashToolStripMenuItem;
        public MenuItem GitGuiMenuItem => menu.gitGUIToolStripMenuItem;
        public MenuItem GitKMenuItem => menu.kGitToolStripMenuItem;
        public MenuItem GitCommandLogMenuItem => menu.gitcommandLogToolStripMenuItem;
        public MenuItem SettingsMenuItem => menu.settingsToolStripMenuItem;
    }
}
