using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace GitUI.HelperDialogs;

public sealed partial class FormBuildServerCredentials : GitExtensionsFormBase
{
    public FormBuildServerCredentials()
        : this(string.Empty)
    {
    }

    public FormBuildServerCredentials(string buildServerUniqueKey)
    {
        InitializeComponent();

        labelHeader.Text = string.Format(labelHeader.Text ?? string.Empty, buildServerUniqueKey);
        radioButtonGuestAccess.IsCheckedChanged += authenticationMethodChanged;
        radioButtonAuthenticatedUser.IsCheckedChanged += authenticationMethodChanged;
        radioButtonBearerToken.IsCheckedChanged += authenticationMethodChanged;
        buttonOK.Click += buttonOK_Click;
        buttonCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        Opened += FormBuildServerCredentials_Load;
        InitializeComplete();
    }

    public IBuildServerCredentials? BuildServerCredentials { get; set; }

    private void buttonOK_Click(object? sender, EventArgs e)
    {
        BuildServerCredentials ??= new BuildServerCredentials();
        BuildServerCredentials.BuildServerCredentialsType = radioButtonAuthenticatedUser.IsChecked == true
            ? BuildServerCredentialsType.UsernameAndPassword
            : radioButtonBearerToken.IsChecked == true
                ? BuildServerCredentialsType.BearerToken
                : BuildServerCredentialsType.Guest;
        BuildServerCredentials.Username = textBoxUserName.Text;
        BuildServerCredentials.Password = textBoxPassword.Text;
        BuildServerCredentials.BearerToken = textBoxBearerToken.Text;
        DialogResult = DialogResult.OK;
    }

    private void FormBuildServerCredentials_Load(object? sender, EventArgs e)
    {
        if (BuildServerCredentials is not null)
        {
            radioButtonGuestAccess.IsChecked = BuildServerCredentials.BuildServerCredentialsType == BuildServerCredentialsType.Guest;
            radioButtonAuthenticatedUser.IsChecked = BuildServerCredentials.BuildServerCredentialsType == BuildServerCredentialsType.UsernameAndPassword;
            radioButtonBearerToken.IsChecked = BuildServerCredentials.BuildServerCredentialsType == BuildServerCredentialsType.BearerToken;
            textBoxUserName.Text = BuildServerCredentials.Username;
            textBoxPassword.Text = BuildServerCredentials.Password;
            textBoxBearerToken.Text = BuildServerCredentials.BearerToken;
        }

        UpdateUI();
    }

    private void authenticationMethodChanged(object? sender, EventArgs e)
        => UpdateUI();

    private void UpdateUI()
    {
        textBoxUserName.IsEnabled = radioButtonAuthenticatedUser.IsChecked == true;
        textBoxPassword.IsEnabled = radioButtonAuthenticatedUser.IsChecked == true;
        textBoxBearerToken.IsEnabled = radioButtonBearerToken.IsChecked == true;
    }
}
