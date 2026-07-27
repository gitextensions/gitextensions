using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using GitExtUtils;
using GitUI.Compat;
using GitUI.Hotkey;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public class TextboxHotkey : TextBox
{
    private WinFormsShims.Keys _keyData;

    private IHotkeySettingsManager HotkeySettingsManager
    {
        get
        {
            if (this.GetLogicalAncestors().OfType<SettingsPageBase>().FirstOrDefault() is not SettingsPageBase settingsPage)
            {
                throw new InvalidOperationException($"{GetType().Name} must be sited on a {typeof(SettingsPageBase)} control");
            }

            return settingsPage.ServiceProvider.GetRequiredService<IHotkeySettingsManager>();
        }
    }

    protected override Type StyleKeyOverride => typeof(TextBox);

    /// <summary>Gets or sets the KeyData.</summary>
    public WinFormsShims.Keys KeyData
    {
        get => _keyData;
        set
        {
            if (_keyData == value)
            {
                return;
            }

            _keyData = value;

            if (_keyData != WinFormsShims.Keys.None)
            {
                // TODO: do not change text color on already assigned keys, which occur only once
                if (HotkeySettingsManager.IsUniqueKey(_keyData))
                {
                    Foreground = Brushes.Red;
                }
                else
                {
                    ClearValue(ForegroundProperty);
                }
            }

            Text = _keyData.ToText();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        WinFormsShims.Keys keyData = KeysMapper.ToKeys(e);

        // We don't want only a modifier key pressed
        // TODO Further restrict the allowed keys
        if (keyData != WinFormsShims.Keys.None && !keyData.GetKeyCode().IsModifierKey())
        {
            KeyData = keyData;
        }

        // Swallow all keys
        e.Handled = true;
    }
}
