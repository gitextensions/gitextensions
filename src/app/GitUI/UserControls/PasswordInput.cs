using GitExtUtils.GitUI.Theming;
using ResourceManager;

namespace GitUI.UserControls;

public partial class PasswordInput : TranslatedControl
{
    private IButtonControl? _originalAcceptButton;

    public PasswordInput()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public event EventHandler<TextEventArgs>? PasswordEntered;

    private void SendInput_Click(object sender, EventArgs e)
    {
        PasswordEntered?.Invoke(this, new TextEventArgs(Password.Text));
        Password.Text = "";
    }

    private void ShowPassword_Click(object sender, EventArgs e)
    {
        Password.UseSystemPasswordChar = !Password.UseSystemPasswordChar;
        ShowPassword.Image = (Password.UseSystemPasswordChar ? Properties.Images.EyeClosed : Properties.Images.EyeOpened).AdaptLightness();
        Password.Focus();
    }

    private void Text_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.Text))
        {
            Password.Paste(e.Data.GetData(DataFormats.Text).ToString());
        }
    }

    private void Text_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.Text))
        {
            e.Effect = DragDropEffects.Copy;
            Password.Focus();

            // Workaround for a bug in GetCharIndexFromPosition: It cannot return the position after the last character
            string original = Password.Text;
            Password.Text = $"{original} ";
            int pos = Password.GetCharIndexFromPosition(Password.PointToClient(new Point(e.X, e.Y)));
            Password.Text = original;
            Password.Select(pos, length: 0);
        }
    }

    private void Text_Enter(object sender, EventArgs e)
    {
        if (FindForm() is Form parentForm)
        {
            _originalAcceptButton = parentForm.AcceptButton;
            parentForm.AcceptButton = SendInput;
        }
    }

    private void Text_Leave(object sender, EventArgs e)
    {
        if (FindForm() is Form parentForm && parentForm.AcceptButton == SendInput)
        {
            parentForm.AcceptButton = _originalAcceptButton;
        }
    }
}
