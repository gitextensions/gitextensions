using System.Net;
using Avalonia.Controls;
using Avalonia.Layout;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Settings.UserControls;

namespace GitUI.SettingControlBindings;

internal sealed class CredentialsSettingControlBinding : SettingControlBinding<CredentialsSetting, Grid>
{
    private readonly CredentialsControl _model;
    private readonly TextBox _passwordTextBox = new() { PasswordChar = '\u25CF' };
    private readonly TextBox _userNameTextBox = new();

    public CredentialsSettingControlBinding(CredentialsSetting setting, CredentialsControl? control)
        : base(setting, customControl: null)
    {
        _model = control ?? new CredentialsControl();
    }

    public override Grid CreateControl()
    {
        Setting.CustomControl = _model;
        Grid control = new()
        {
            ColumnDefinitions = new ColumnDefinitions(_model.ShowUserName ? "Auto,*,Auto,*" : "0,0,Auto,*"),
            ColumnSpacing = 6,
        };
        AddField(_model.UserNameLabelText, _userNameTextBox, column: 0, _model.ShowUserName);
        AddField(_model.PasswordLabelText, _passwordTextBox, column: 2, isVisible: true);
        return control;

        void AddField(string labelText, TextBox textBox, int column, bool isVisible)
        {
            TextBlock label = new()
            {
                IsVisible = isVisible,
                Text = labelText,
                VerticalAlignment = VerticalAlignment.Center,
            };
            textBox.IsVisible = isVisible;
            Grid.SetColumn(label, column);
            control.Children.Add(label);

            Grid.SetColumn(textBox, column + 1);
            control.Children.Add(textBox);
        }
    }

    public override void LoadSetting(SettingsSource settings, Grid control)
    {
        if (SettingLevelSupported(settings.SettingLevel))
        {
            NetworkCredential credentials = Setting.GetValueOrDefault(settings);
            _model.UserName = credentials.UserName;
            _model.Password = credentials.Password;
            _userNameTextBox.Text = credentials.UserName;
            _passwordTextBox.Text = credentials.Password;
            control.IsEnabled = true;
        }
        else
        {
            _model.UserName = string.Empty;
            _model.Password = string.Empty;
            _userNameTextBox.Text = string.Empty;
            _passwordTextBox.Text = string.Empty;
            control.IsEnabled = false;
        }
    }

    public override void SaveSetting(SettingsSource settings, Grid control)
    {
        if (SettingLevelSupported(settings.SettingLevel))
        {
            _model.UserName = _userNameTextBox.Text ?? string.Empty;
            _model.Password = _passwordTextBox.Text ?? string.Empty;
            Setting.SaveValue(settings, _model.UserName, _model.Password);

            // Reload actual settings.
            LoadSetting(settings, control);
        }
    }

    private static bool SettingLevelSupported(SettingLevel settingLevel)
    {
        return settingLevel switch
        {
            SettingLevel.Global or SettingLevel.Local or SettingLevel.Effective => true,
            _ => false,
        };
    }
}
