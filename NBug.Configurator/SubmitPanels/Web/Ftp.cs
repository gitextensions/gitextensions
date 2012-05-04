// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ftp.cs" company="NBusy Project">
//   Copyright © 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Configurator.SubmitPanels.Web
{
	using System;
	using System.Windows.Forms;

	public partial class Ftp : UserControl, ISubmitPanel
	{
		public Ftp()
		{
			InitializeComponent();
		}

		public string ConnectionString
		{
			get
			{
				// Check the mendatory fields
				if (string.IsNullOrEmpty(this.urlTextBox.Text))
				{
					MessageBox.Show("Mandatory field \"" + urlTextBox.Name + "\" cannot be left blank.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return null;
				}

				// Do fixes
				if (!this.urlTextBox.Text.EndsWith("/"))
				{
					this.urlTextBox.Text += "/";
				}

				var ftp = new Core.Submission.Web.Ftp
					{
						Url = this.urlTextBox.Text
					};

				if (this.useSslCheckBox.Checked)
				{
					ftp.Usessl = "true";
				}

				if (this.usernamePasswordRadioButton.Checked)
				{
					ftp.Username = this.usernameTextBox.Text;
					ftp.Password = this.passwordTextBox.Text;
				}

				return ftp.ConnectionString;
			}

			set
			{
				var ftp = new Core.Submission.Web.Ftp(value);

				this.urlTextBox.Text = ftp.Url;

				if (!string.IsNullOrEmpty(ftp.Usessl))
				{
					this.useSslCheckBox.Checked = Convert.ToBoolean(ftp.Usessl);
				}

				if (!string.IsNullOrEmpty(ftp.Username))
				{
					this.usernameTextBox.Text = ftp.Username;
					this.passwordTextBox.Text = ftp.Password;
					this.usernamePasswordRadioButton.Checked = true;
				}
			}
		}

		private void UsernamePasswordRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (this.usernamePasswordRadioButton.Checked)
			{
				this.usernameTextBox.Enabled = true;
				this.passwordTextBox.Enabled = true;
			}
			else
			{
				this.usernameTextBox.Enabled = false;
				this.passwordTextBox.Enabled = false;
			}
		}
	}
}
