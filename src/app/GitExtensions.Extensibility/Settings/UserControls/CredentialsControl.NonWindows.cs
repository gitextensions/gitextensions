namespace GitExtensions.Extensibility.Settings.UserControls;

public class CredentialsControl : Control
{
    public CredentialsControl(string? userNameLabelText = null, string? passwordLabelText = null)
    {
        ChangeLabelText(userNameLabelText, passwordLabelText);
    }

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool ShowUserName { get; private set; } = true;

    public string UserNameLabelText { get; private set; } = "User name";

    public string PasswordLabelText { get; private set; } = "API token/Password";

    public void ChangeUIMode(bool showUserName, string? passwordLabelText = null, string? userNameLabelText = null)
    {
        ChangeLabelText(userNameLabelText, passwordLabelText);
        ShowUserName = showUserName;
    }

    private void ChangeLabelText(string? userNameLabelText, string? passwordLabelText)
    {
        if (!string.IsNullOrWhiteSpace(userNameLabelText))
        {
            UserNameLabelText = userNameLabelText;
        }

        if (!string.IsNullOrWhiteSpace(passwordLabelText))
        {
            PasswordLabelText = passwordLabelText;
        }
    }
}
