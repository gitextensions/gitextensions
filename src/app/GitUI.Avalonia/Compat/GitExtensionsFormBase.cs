using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.Compat;
using GitUI.Properties;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace ResourceManager;

// NOTE do not make this class abstract as it breaks the Avalonia designer

/// <summary>
/// Base class for all Git Extensions forms.
/// </summary>
/// <remarks>
/// Deriving from this class requires a call to <see cref="InitializeComplete"/> at
/// the end of the constructor. Omitting this call with result in a runtime exception.
/// </remarks>
public class GitExtensionsFormBase : Window, ITranslate, WinFormsShims.IWin32Window
{
    private WinFormsShims.DialogResult _dialogResult = WinFormsShims.DialogResult.None;
    private bool _isShownModally;
    private bool _runtimeLoadRaised;
    private Button? _acceptButton;
    private Button? _cancelButton;
    private IReadOnlyList<HotkeyCommand> _hotkeys = [];

    /// <summary>Creates a new <see cref="GitExtensionsFormBase"/> indicating position restore.</summary>
    public GitExtensionsFormBase()
    {
        Icon = Images.ApplicationIcon;
        Activated += GitExtensionsFormBase_Activated;
    }

    /// <summary>The window title, under its WinForms name so ported code compiles unchanged.</summary>
    public string? Text
    {
        get => Title;
        set => Title = value;
    }

    /// <summary>
    ///  The WinForms dialog result: assigning a value other than <c>None</c> while the window
    ///  is shown modally closes it, and <see cref="ShowDialog"/> returns the assigned value.
    /// </summary>
    public WinFormsShims.DialogResult DialogResult
    {
        get => _dialogResult;
        set
        {
            _dialogResult = value;
            if (value is not WinFormsShims.DialogResult.None && _isShownModally)
            {
                Close();
            }
        }
    }

    /// <summary>Records the result while a window close is already in progress.</summary>
    /// <remarks>
    ///  Use this from an <c>OnClosing</c> override. Assigning
    ///  <see cref="DialogResult"/> there would request another close and re-enter Avalonia's
    ///  closing pipeline.
    /// </remarks>
    protected void SetDialogResultOnClose(WinFormsShims.DialogResult value)
        => _dialogResult = value;

    /// <summary>The button activated by Enter; mapped to Avalonia's <see cref="Button.IsDefault"/>.</summary>
    public Button? AcceptButton
    {
        get => _acceptButton;
        set
        {
            if (_acceptButton is not null)
            {
                _acceptButton.IsDefault = false;
            }

            _acceptButton = value;
            if (value is not null)
            {
                value.IsDefault = true;
            }
        }
    }

    /// <summary>The button activated by Escape, under the WinForms property name.</summary>
    public Button? CancelButton
    {
        get => _cancelButton;
        set => _cancelButton = value;
    }

    /// <summary>Setting focuses the control; <see langword="null"/> clears nothing (WinForms parity).</summary>
    public Avalonia.Input.InputElement? ActiveControl
    {
        get;
        set
        {
            field = value;
            value?.Focus();
        }
    }

    nint WinFormsShims.IWin32Window.Handle => TryGetPlatformHandle()?.Handle ?? 0;

    /// <summary>
    ///  Shows the window modally with WinForms blocking semantics: the call returns when the
    ///  window closes, pumping the UI with a nested dispatcher frame meanwhile.
    /// </summary>
    public WinFormsShims.DialogResult ShowDialog(WinFormsShims.IWin32Window? owner = null)
    {
        Window? ownerWindow = owner as Window
            ?? (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        _isShownModally = true;
        try
        {
            return DispatcherPump.Wait(async () =>
            {
                if (ownerWindow is not null && ownerWindow != this && ownerWindow.IsVisible)
                {
                    await ShowDialog(ownerWindow);
                }
                else
                {
                    // No usable owner window (e.g. an error dialog before the main window
                    // opens): emulate the modal loop by waiting for the window to close.
                    TaskCompletionSource closed = new();
                    Closed += (_, _) => closed.TrySetResult();
                    Show();
                    await closed.Task;
                }

                return DialogResult;
            });
        }
        finally
        {
            _isShownModally = false;
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        // Avalonia's preview host opens the window; design mode must never start runtime work.
        if (!Design.IsDesignMode && !_runtimeLoadRaised)
        {
            _runtimeLoadRaised = true;
            OnRuntimeLoad(e);
        }
    }

    private void GitExtensionsFormBase_Activated(object? sender, EventArgs e)
    {
        if (!Design.IsDesignMode)
        {
            OnApplicationActivated();
            if (WindowState == WindowState.Minimized
                && Owner is null
                && AppSettings.WorkaroundActivateFromMinimize)
            {
                // Application occasionally requires explicit "restore" in Taskbar.
                // See https://github.com/gitextensions/gitextensions/pull/10119.
                System.Diagnostics.Trace.WriteLine("WindowState is unexpectedly Minimized in OnApplicationActivated(), restoring.");
                WindowState = WindowState.Normal;
            }
        }
    }

    /// <summary>
    ///  Called once when the window is first shown, like the WinForms
    ///  <c>GitExtensionsFormBase.OnRuntimeLoad</c> (which forms override to start work).
    /// </summary>
    protected virtual void OnRuntimeLoad(EventArgs e)
    {
    }

    /// <summary>
    /// Notifies whenever the application becomes active.
    /// </summary>
    protected virtual void OnApplicationActivated()
    {
    }

    protected bool IsDesignMode => Design.IsDesignMode;

    /// <summary>
    ///  Gets or sets a value that specifies if the hotkeys are used.
    /// </summary>
    protected bool HotkeysEnabled { get; set; }

    /// <summary>
    ///  Gets the currently loaded hotkeys.
    /// </summary>
    protected IReadOnlyList<HotkeyCommand>? Hotkeys => _hotkeys;

    /// <summary>
    ///  Loads hotkeys for the specified configuration setting.
    /// </summary>
    /// <param name="hotkeySettingsName">The setting name.</param>
    protected void LoadHotkeys(string hotkeySettingsName)
    {
        _hotkeys = GetHotkeys(hotkeySettingsName);
    }

    /// <summary>
    ///  Loads hotkeys for the specified configuration setting.
    /// </summary>
    /// <param name="hotkeySettingsName">The setting name.</param>
    protected IReadOnlyList<HotkeyCommand> GetHotkeys(string hotkeySettingsName)
    {
        if (!HotkeysEnabled || !TryGetUICommands(out IGitUICommands? commands))
        {
            return [];
        }

        return commands.GetService(typeof(IHotkeySettingsLoader)) is IHotkeySettingsLoader loader
            ? loader.LoadHotkeys(hotkeySettingsName)
            : [];
    }

    protected WinFormsShims.Keys GetShortcutKeys<T>(T commandCode)
        where T : struct, Enum
        => _hotkeys.GetShortcutKey(commandCode);

    protected string GetShortcutKeyDisplayString<T>(T commandCode)
        where T : struct, Enum
        => _hotkeys.GetShortcutDisplay(commandCode);

    protected string GetShortcutKeyTooltipString<T>(T commandCode)
        where T : struct, Enum
        => _hotkeys.GetShortcutToolTip(commandCode);

    /// <summary>
    /// Checks if the form wants to handle the key and executes that hotkey
    /// (without propagating an unhandled key to the base class function as in <cref>ProcessCmdKey</cref>).
    /// </summary>
    public virtual bool ProcessHotkey(WinFormsShims.Keys keyData)
    {
        // Avalonia maps modifier-only and unsupported key events to None; None is not an assignable hotkey.
        if (!HotkeysEnabled || keyData == WinFormsShims.Keys.None)
        {
            return false;
        }

        HotkeyCommand? hotkey = _hotkeys.FirstOrDefault(hotkey => hotkey.KeyData == keyData);
        return hotkey is not null && ExecuteCommand(hotkey.CommandCode);
    }

    /// <summary>
    ///  Attempts to find an instance of <see cref="IGitUICommands"/>.
    /// </summary>
    /// <param name="commands">
    ///  The instance of <see cref="IGitUICommands"/> directly assigned form
    ///  (if the form implements <see cref="IGitModuleForm"/>); <see langword="null"/>, otherwise.
    /// </param>
    /// <returns>
    ///  <see langword="true"/>, if an instance of <see cref="IGitUICommands"/> is found; <see langword="false"/>, otherwise.
    /// </returns>
    public virtual bool TryGetUICommands([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IGitUICommands? commands)
    {
        commands = null;
        return false;
    }

    /// <summary>Override this method to handle form-specific Hotkey commands.</summary>
    protected virtual bool ExecuteCommand(int command)
    {
        return false;
    }

    /// <summary>Controls the shared Escape-to-close behavior; the repository browser opts out.</summary>
    protected virtual bool CloseOnEscape => true;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        WinFormsShims.Keys keyData = KeysMapper.ToKeys(e);
        if (ProcessHotkey(keyData))
        {
            e.Handled = true;
            return;
        }

        if (keyData == WinFormsShims.Keys.Enter && AcceptButton is { IsEnabled: true } acceptButton)
        {
            acceptButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, acceptButton));
            e.Handled = true;
            return;
        }

        if (CloseOnEscape && keyData == WinFormsShims.Keys.Escape)
        {
            if (CancelButton is { IsEnabled: true } cancelButton)
            {
                cancelButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, cancelButton));
            }
            else
            {
                Close();
            }

            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    /// <summary>Performs post-initialisation tasks such as translation and DPI scaling.</summary>
    /// <remarks>
    /// <para>Subclasses must ensure this method is called in their constructor, ideally as the final statement.</para>
    /// <para>Requiring this extra life-cycle event allows preparing the UI after any call to <c>InitializeComponent</c>,
    /// but before it is show. Both the WinForms <c>Load</c> and <c>Shown</c> events occur too late for
    /// operations that effect layout.</para>
    /// </remarks>
    protected void InitializeComplete()
    {
        Translator.Translate(this, AppSettings.CurrentTranslation);
        AvaloniaTranslationUtils.RemoveTextBlockMnemonicMarkers(this);
        InputAccessibility.Apply(this);
    }

    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public virtual void AddTranslationItems(ITranslation translation)
    {
        AvaloniaTranslationUtils.AddTranslationItemsFromFields(GetType().Name, this, translation);
    }

    public virtual void TranslateItems(ITranslation translation)
    {
        AvaloniaTranslationUtils.TranslateItemsFromFields(GetType().Name, this, translation);
    }
}
