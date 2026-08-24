using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Settings.UserControls;

namespace GitUI.SettingControlBindings;

internal sealed class CredentialsSettingControlBinding : SettingControlBinding<CredentialsSetting, Grid>
{
    public CredentialsSettingControlBinding(CredentialsSetting setting, CredentialsControl? control)
        : base(setting, customControl: null)
    {
        Setting.CustomControl = control ?? new CredentialsControl();
    }

    public override Grid CreateControl()
    {
        CredentialsControl model = Setting.CustomControl ??= new CredentialsControl();
        TextBox passwordTextBox = new()
        {
            Height = 20,
            Name = "passwordTextBox",
            PasswordChar = '\u25CF',
        };
        TextBox userNameTextBox = new()
        {
            Height = 20,
            Name = "userNameTextBox",
        };
        Grid control = new()
        {
            ColumnDefinitions = new ColumnDefinitions(model.ShowUserName ? "Auto,*,Auto,*" : "0,0,Auto,*"),
            Height = 21,
        };
        control.Tag = (model, userNameTextBox, passwordTextBox);
        AddField(model.UserNameLabelText, "userNameLabel", userNameTextBox, column: 0, model.ShowUserName);
        AddField(model.PasswordLabelText, "passwordLabel", passwordTextBox, column: 2, isVisible: true);
        return control;

        void AddField(string labelText, string labelName, TextBox textBox, int column, bool isVisible)
        {
            TextBlock label = new()
            {
                IsVisible = isVisible,
                Margin = column == 0 ? new Thickness(0, 3, 3, 0) : new Thickness(3, 3, 3, 0),
                Name = labelName,
                Text = labelText,
                VerticalAlignment = VerticalAlignment.Center,
            };
            textBox.IsVisible = isVisible;
            textBox.Margin = column == 0 ? new Thickness(3, 0) : new Thickness(3, 0, 0, 0);
            Grid.SetColumn(label, column);
            control.Children.Add(label);

            Grid.SetColumn(textBox, column + 1);
            control.Children.Add(textBox);
        }
    }

    public override void LoadSetting(SettingsSource settings, Grid control)
    {
        (CredentialsControl model, TextBox userNameTextBox, TextBox passwordTextBox) =
            ((CredentialsControl, TextBox, TextBox))control.Tag!;
        if (SettingLevelSupported(settings.SettingLevel))
        {
            NetworkCredential credentials = Setting.GetValueOrDefault(settings);
            model.UserName = credentials.UserName;
            model.Password = credentials.Password;
            userNameTextBox.Text = credentials.UserName;
            passwordTextBox.Text = credentials.Password;
            control.IsEnabled = true;
        }
        else
        {
            model.UserName = string.Empty;
            model.Password = string.Empty;
            userNameTextBox.Text = string.Empty;
            passwordTextBox.Text = string.Empty;
            control.IsEnabled = false;
        }
    }

    public override void SaveSetting(SettingsSource settings, Grid control)
    {
        (CredentialsControl model, TextBox userNameTextBox, TextBox passwordTextBox) =
            ((CredentialsControl, TextBox, TextBox))control.Tag!;
        if (SettingLevelSupported(settings.SettingLevel))
        {
            model.UserName = userNameTextBox.Text ?? string.Empty;
            model.Password = passwordTextBox.Text ?? string.Empty;
            Setting.SaveValue(settings, model.UserName, model.Password);

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
