using System.Diagnostics.CodeAnalysis;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.ScriptsEngine;
using ResourceManager;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

/// <summary>Base window that provides access to the module and <see cref="IGitUICommands"/>.</summary>
public class GitModuleForm : GitExtensionsForm, IGitUICommandsSource, ResourceManager.IGitModuleForm, IScriptOptionsForm
{
    private bool _isReactivation;
    private IHotkeySettingsLoader? _hotkeySettingsLoader;
    private IScriptsRunner? _scriptsRunner;
    private IReadOnlyList<HotkeyCommand> _scriptHotkeys = [];
    private bool _scriptHotkeysLoaded;
    private IGitUICommands? _uiCommands;

    /// <inheritdoc />
    public event EventHandler<GitUICommandsChangedEventArgs>? UICommandsChanged;

    /// <summary>For the visual designer and construction tests only, like WinForms.</summary>
    protected GitModuleForm()
    {
    }

    protected GitModuleForm(IGitUICommands? commands, bool enablePositionRestore)
        : base(enablePositionRestore)
    {
        if (commands is not null)
        {
            UICommands = commands;
        }
    }

    public IHotkeySettingsLoader HotkeySettingsReader
        => _hotkeySettingsLoader ??= UICommands.GetRequiredService<IHotkeySettingsLoader>();

    public IScriptsRunner ScriptsRunner
        => _scriptsRunner ??= UICommands.GetRequiredService<IScriptsRunner>();

    /// <inheritdoc />
    public IGitUICommands UICommands
    {
        get => _uiCommands
            ?? throw new InvalidOperationException($"{nameof(UICommands)} is null. {GetType().FullName} was constructed incorrectly.");
        protected set
        {
            ArgumentNullException.ThrowIfNull(value);
            IGitUICommands? oldCommands = _uiCommands;
            _uiCommands = value;
            _hotkeySettingsLoader = null;
            _scriptsRunner = null;
            _scriptHotkeysLoaded = false;
            OnUICommandsChanged(new GitUICommandsChangedEventArgs(oldCommands));
        }
    }

    /// <summary>Gets the module of the currently set <see cref="UICommands"/>.</summary>
    public IGitModule Module => UICommands.Module;

    protected override bool ExecuteCommand(int command)
    {
        IScriptsManager scriptsManager = UICommands.GetRequiredService<IScriptsManager>();
        ScriptInfo? script = scriptsManager.GetScript(command);
        return script is not null
            ? ScriptsRunner.RunScript(script, this, UICommands, GetScriptOptionsProvider())
            : base.ExecuteCommand(command);
    }

    public override bool ProcessHotkey(WinFormsShims.Keys keyData)
    {
        if (!HotkeysEnabled)
        {
            return false;
        }

        if (base.ProcessHotkey(keyData))
        {
            return true;
        }

        if (!_scriptHotkeysLoaded)
        {
            ReloadScriptHotkeys();
        }

        // Avalonia reports modifier-only and unsupported key events as None; they are not assignable script hotkeys.
        if (keyData == WinFormsShims.Keys.None)
        {
            return false;
        }

        HotkeyCommand? hotkey = _scriptHotkeys.FirstOrDefault(command => command.KeyData == keyData);
        return hotkey is not null && ExecuteCommand(hotkey.CommandCode);
    }

    protected void ReloadScriptHotkeys()
    {
        _scriptHotkeys = UICommands.GetService(typeof(IHotkeySettingsLoader)) is IHotkeySettingsLoader loader
            ? loader.LoadHotkeys(FormSettings.HotkeySettingsName)
            : [];
        _scriptHotkeysLoaded = true;
    }

    public virtual IScriptOptionsProvider GetScriptOptionsProvider()
        => ScriptOptionsProviderBase.Default;

    protected override void OnApplicationActivated()
    {
        base.OnApplicationActivated();

        if (_isReactivation && _uiCommands is IGitUICommands uiCommands)
        {
            uiCommands.Module.InvalidateGitSettings();
        }
        else
        {
            _isReactivation = true;
        }
    }

    protected virtual void OnUICommandsChanged(GitUICommandsChangedEventArgs e)
    {
        UICommandsChanged?.Invoke(this, e);
    }

    public override bool TryGetUICommands([NotNullWhen(true)] out IGitUICommands? commands)
    {
        commands = _uiCommands;
        return commands is not null;
    }
}
