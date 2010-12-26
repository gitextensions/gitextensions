using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Github
{
    public partial class ConfigureGithub : Form
    {
        public ConfigureGithub()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var auth = GithubPlugin.Instance.Auth;
            if (auth!= null)
            {
                _usernameTB.Text = auth.Username;
                _passwordTB.Text = auth.Password;
                _apitokenTB.Text = auth.ApiToken;
            }
        }

        private void _saveBtn_Click(object sender, EventArgs e)
        {
            GithubPlugin.Instance.SetAuth(_usernameTB.Text, _passwordTB.Text, _apitokenTB.Text);
            Close();
        }

        private void _getApiTokenBtn_Click(object sender, EventArgs e)
        {
            try
            {
                _apitokenTB.Text = GithubPlugin.GetApiTokenFromGithub(_usernameTB.Text, _passwordTB.Text);
                _saveBtn.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to get API token using username/password. Check that your U/P is correct and that the network is OK.\r\nError:" + ex.Message, "Error");
                _saveBtn.Enabled = false;
            }
        }

        private void _usernameTB_TextChanged(object sender, EventArgs e)
        {
            //_saveBtn.Enabled = false;
        }

        private void _passwordTB_TextChanged(object sender, EventArgs e)
        {
            //_saveBtn.Enabled = false;
        }
    }
}
