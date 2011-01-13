using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

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

            _useHttpsRB.Checked = GithubPlugin.Instance.PreferredAccessMethod == "https";
            _useSshRB.Checked = GithubPlugin.Instance.PreferredAccessMethod == "ssh";
            _passwordTB_TextChanged(null, null);
        }

        private void _saveBtn_Click(object sender, EventArgs e)
        {
            if (_useHttpsRB.Checked)
            {
                if (_passwordTB.Text.Trim().Length == 0)
                {
                    MessageBox.Show(this, "You must have a password set if you use the https access method", "Error");
                    return;
                }

                if (_passwordTB.Text.Trim().Length > 0)
                {
                    if (MessageBox.Show(this, "Your password will be stored in both registry and\r\nin the .git/config file in any of the repositores you clone.\r\nAre you sure?", "Warning", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                        return;
                }
            }
            else if (_passwordTB.Text.Trim().Length > 0)
            {
                if (MessageBox.Show(this, "Your password will be stored in registry. As you have chosen SSH as the method to get repositories, you don't really need to save the password.\r\nAre you sure you want to anyway?", "Warning", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                    return;
            }

            try
            {
                GithubPlugin.Instance.SetAuth(_usernameTB.Text, _passwordTB.Text, _apitokenTB.Text);
                GithubPlugin.Instance.PreferredAccessMethod = _useHttpsRB.Checked ? "https" : "ssh";

                if (GithubPlugin.Instance.ConfigurationOk || MessageBox.Show(this, "Username/apitoken invalid or network down. Save anyway?", "Warning", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Save failed");
            }
        }

        private void _getApiTokenBtn_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/account#admin_bucket");
            /*
            try
            {
                _apitokenTB.Text = GithubPlugin.GetApiTokenFromGithub(_usernameTB.Text, _passwordTB.Text);
                _saveBtn.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to get API token using username/password. Check that your U/P is correct and that the network is OK.\r\nError:" + ex.Message, "Error");
                _saveBtn.Enabled = false;
            }*/
        }

        private void _usernameTB_TextChanged(object sender, EventArgs e)
        {
            //_saveBtn.Enabled = false;
        }

        private void _passwordTB_TextChanged(object sender, EventArgs e)
        {
            _useHttpsRB.Enabled = _passwordTB.Text.Trim().Length > 0;
            //_saveBtn.Enabled = false;
        }
    }
}
