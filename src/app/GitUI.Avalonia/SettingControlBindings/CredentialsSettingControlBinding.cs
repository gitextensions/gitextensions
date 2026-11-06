using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Settings.UserControls;
using GitUI.Compat;

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
            Height = 23,
            Name = "passwordTextBox",
            PasswordChar = '\u25CF',
            VerticalAlignment = VerticalAlignment.Top,
        };
        TextBox userNameTextBox = new()
        {
            Height = 23,
            Name = "userNameTextBox",
            VerticalAlignment = VerticalAlignment.Top,
        };
        Grid mainTableLayoutPanel = new()
        {
            Name = "mainTableLayoutPanel",
            ColumnDefinitions = new ColumnDefinitions(model.ShowUserName ? "Auto,*,Auto,*" : "0,0,Auto,*"),
            Height = 24,
        };
        Grid control = new()
        {
            Height = 24,
            Children = { mainTableLayoutPanel },
        };
        control.Tag = (model, userNameTextBox, passwordTextBox);
        TextBlock userNameLabel = CreateLabel(model.UserNameLabelText, "userNameLabel", column: 0, model.ShowUserName);
        TextBlock passwordLabel = CreateLabel(model.PasswordLabelText, "passwordLabel", column: 2, isVisible: true);
        PlaceTextBox(userNameTextBox, column: 1, model.ShowUserName);
        PlaceTextBox(passwordTextBox, column: 3, isVisible: true);

        // Match the source TableLayoutPanel.Controls order, which is also the semantic
        // accessibility and capture-tree order.
        mainTableLayoutPanel.Children.Add(userNameLabel);
        mainTableLayoutPanel.Children.Add(userNameTextBox);
        mainTableLayoutPanel.Children.Add(passwordTextBox);
        mainTableLayoutPanel.Children.Add(passwordLabel);
        return control;

        static TextBlock CreateLabel(string labelText, string labelName, int column, bool isVisible)
        {
            TextBlock label = new()
            {
                Height = 20,
                IsVisible = isVisible,
                Margin = column == 0 ? new Thickness(0, 3, 4, 0) : new Thickness(4, 3, 4, 0),
                Name = labelName,
                Text = labelText,
                VerticalAlignment = VerticalAlignment.Top,
            };
            Grid.SetColumn(label, column);
            WinFormsAutoSizeTextBlock.Attach(label);
            return label;
        }

        static void PlaceTextBox(TextBox textBox, int column, bool isVisible)
        {
            textBox.IsVisible = isVisible;
            textBox.Margin = column == 1 ? new Thickness(4, 0) : new Thickness(4, 0, 0, 0);
            Grid.SetColumn(textBox, column);
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
