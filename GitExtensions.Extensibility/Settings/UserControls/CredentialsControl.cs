using System;
using System.Windows.Forms;

namespace GitExtensions.Extensibility.Settings.UserControls
{
    public partial class CredentialsControl : UserControl
    {
        public CredentialsControl(string userNameLabelText = null, string passwordLabelText = null)
        {
            InitializeComponent();

            if (!string.IsNullOrWhiteSpace(userNameLabelText))
            {
                userNameLabel.Text = userNameLabelText;
            }

            if (!string.IsNullOrWhiteSpace(passwordLabelText))
            {
                passwordLabel.Text = passwordLabelText;
            }
        }

        public string UserName
        {
            get => userNameTextBox.Text;

            set => userNameTextBox.Text = value;
        }

        public string Password
        {
            get => passwordTextBox.Text;

            set => passwordTextBox.Text = value;
        }

        private void CredentialsControl_Load(object sender, EventArgs e)
        {
            passwordTextBox.UseSystemPasswordChar = true;
        }
    }
}
