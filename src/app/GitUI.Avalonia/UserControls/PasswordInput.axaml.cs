using Avalonia.Controls;
using Avalonia.VisualTree;
using ResourceManager;

namespace GitUI.UserControls;

// Twin of GitUI/UserControls/PasswordInput.cs. Drag and drop of text onto the password box
// is not ported yet.
public partial class PasswordInput : TranslatedControl
{
    private Button? _originalAcceptButton;

    public PasswordInput()
    {
        InitializeComponent();

        SendInput.Click += SendInput_Click;
        ShowPassword.Click += ShowPassword_Click;
        Password.GotFocus += Text_Enter;
        Password.LostFocus += Text_Leave;

        InitializeComplete();
    }

    public event EventHandler<TextEventArgs>? PasswordEntered;

    private void SendInput_Click(object sender, EventArgs e)
    {
        PasswordEntered?.Invoke(this, new TextEventArgs(Password.Text ?? ""));
        Password.Text = "";
    }

    private void ShowPassword_Click(object sender, EventArgs e)
    {
        Password.RevealPassword = !Password.RevealPassword;
        ShowPassword.Icon = Password.RevealPassword
            ? Properties.Images.EyeOpened
            : Properties.Images.EyeClosed;
        Password.Focus();
    }

    private void Text_Enter(object sender, EventArgs e)
    {
        if (this.FindAncestorOfType<GitExtensionsFormBase>() is GitExtensionsFormBase parentForm)
        {
            _originalAcceptButton = parentForm.AcceptButton;
            parentForm.AcceptButton = SendInput;
        }
    }

    private void Text_Leave(object sender, EventArgs e)
    {
        if (this.FindAncestorOfType<GitExtensionsFormBase>() is GitExtensionsFormBase parentForm && parentForm.AcceptButton == SendInput)
        {
            parentForm.AcceptButton = _originalAcceptButton;
        }
    }
}
