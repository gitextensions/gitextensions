using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using GitCommands;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace ResourceManager;

// Twin of ResourceManager/GitExtensionsFormBase.cs: provides xlf translation for windows
// plus the WinForms form idioms the ported code-behind relies on (Text, DialogResult,
// synchronous ShowDialog, AcceptButton, OnRuntimeLoad). Hotkeys and theming arrive later.
public class GitExtensionsFormBase : Window, ITranslate, WinFormsShims.IWin32Window
{
    private WinFormsShims.DialogResult _dialogResult = WinFormsShims.DialogResult.None;
    private bool _isShownModally;
    private bool _runtimeLoadRaised;
    private Button? _acceptButton;

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

        if (!_runtimeLoadRaised)
        {
            _runtimeLoadRaised = true;
            OnRuntimeLoad(e);
        }
    }

    /// <summary>
    ///  Called once when the window is first shown, like the WinForms
    ///  <c>GitExtensionsFormBase.OnRuntimeLoad</c> (which forms override to start work).
    /// </summary>
    protected virtual void OnRuntimeLoad(EventArgs e)
    {
    }

    /// <summary>Performs post-initialisation tasks such as translation.</summary>
    /// <remarks>Subclasses must ensure this method is called in their constructor, ideally as the final statement.</remarks>
    protected void InitializeComplete()
    {
        Translator.Translate(this, AppSettings.CurrentTranslation);
    }

    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public virtual void AddTranslationItems(ITranslation translation)
    {
        TranslationUtils.AddTranslationItemsFromFields(GetType().Name, this, translation);
        AvaloniaTranslationUtils.AddTranslationItemsFromFields(GetType().Name, this, translation);
    }

    public virtual void TranslateItems(ITranslation translation)
    {
        TranslationUtils.TranslateItemsFromFields(GetType().Name, this, translation);
        AvaloniaTranslationUtils.TranslateItemsFromFields(GetType().Name, this, translation);
    }
}
