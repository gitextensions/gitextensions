// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Mail.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Configurator.SubmitPanels.Web
{
	using System;
	using System.Net.Mail;
	using System.Windows.Forms;

	public partial class Mail : UserControl, ISubmitPanel
	{
		public Mail()
		{
			this.InitializeComponent();
			this.portNumericUpDown.Maximum = decimal.MaxValue;
			this.priorityComboBox.SelectedIndex = 1;
		}

		public string ConnectionString
		{
			get
			{
				// Check the mendatory fields
				if (this.toListBox.Items.Count == 0)
				{
					MessageBox.Show(
						string.Format("Mandatory field \"{0}\" cannot be left blank.", this.toLabel.Name), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return null;
				}

				var mail = new Core.Submission.Web.Mail
					{
						From = this.fromTextBox.Text, 
						FromName = this.fromNameTextBox.Text, 
						ReplyTo = this.replyToTextBox.Text, 
						CustomSubject = this.customSubjectTextBox.Text, 
						CustomBody = this.customBodyTextBox.Text, 
						SmtpServer = this.smtpServerTextBox.Text, 
						Priority = (MailPriority)Enum.Parse(typeof(MailPriority), this.priorityComboBox.SelectedItem.ToString())
					};

				foreach (var item in this.toListBox.Items)
				{
					mail.To += item + ",";
				}

				mail.To = mail.To.TrimEnd(new[] { ',' });

				if (this.ccListBox.Items.Count != 0)
				{
					foreach (var item in this.ccListBox.Items)
					{
						mail.Cc += item + ",";
					}

					mail.Cc = mail.Cc.TrimEnd(new[] { ',' });
				}

				if (this.bccListBox.Items.Count != 0)
				{
					foreach (var item in this.bccListBox.Items)
					{
						mail.Bcc += item + ",";
					}

					mail.Bcc = mail.Bcc.TrimEnd(new[] { ',' });
				}

				if (!this.defaultPortCheckBox.Checked)
				{
					mail.Port = (int)this.portNumericUpDown.Value;
				}

				if (this.useAuthenticationCheckBox.Checked)
				{
					// Make sure that we can use authentication even with emtpy username and password
					if (string.IsNullOrEmpty(this.usernameTextBox.Text))
					{
						mail.UseAuthentication = true;
					}

					mail.Username = this.usernameTextBox.Text;
					mail.Password = this.passwordTextBox.Text;
				}

				mail.UseSsl = this.useSslCheckBox.Checked;
				mail.UseAttachment = this.useAttachmentCheckBox.Checked;

				return mail.ConnectionString;
			}

			set
			{
				var mail = new Core.Submission.Web.Mail(value);

				this.fromTextBox.Text = mail.From;
				this.fromNameTextBox.Text = mail.FromName;
				this.smtpServerTextBox.Text = mail.SmtpServer;
				this.useSslCheckBox.Checked = mail.UseSsl;
				this.priorityComboBox.SelectedItem = mail.Priority;
				this.useAuthenticationCheckBox.Checked = mail.UseAuthentication;
				this.usernameTextBox.Text = mail.Username;
				this.passwordTextBox.Text = mail.Password;
				this.customSubjectTextBox.Text = mail.CustomSubject;
				this.customBodyTextBox.Text = mail.CustomBody;
				this.replyToTextBox.Text = mail.ReplyTo;
				this.useAttachmentCheckBox.Checked = mail.UseAttachment;
				this.portNumericUpDown.Value = mail.Port;

				if (this.portNumericUpDown.Value == 25 || this.portNumericUpDown.Value == 465 || mail.Port > 0)
				{
					this.defaultPortCheckBox.Checked = true;
				}
				else
				{
					this.defaultPortCheckBox.Checked = false;
				}

				this.toListBox.Items.Clear();
				if (!string.IsNullOrEmpty(mail.To))
				{
					foreach (var to in mail.To.Split(','))
					{
						this.toListBox.Items.Add(to);
					}
				}

				this.ccListBox.Items.Clear();
				if (!string.IsNullOrEmpty(mail.Cc))
				{
					foreach (var cc in mail.Cc.Split(','))
					{
						this.ccListBox.Items.Add(cc);
					}
				}

				this.bccListBox.Items.Clear();
				if (!string.IsNullOrEmpty(mail.Bcc))
				{
					foreach (var bcc in mail.Bcc.Split(','))
					{
						this.bccListBox.Items.Add(bcc);
					}
				}
			}
		}

		private void BccAddButton_Click(object sender, EventArgs e)
		{
			this.bccListBox.Items.Add(this.bccTextBox.Text);
		}

		private void BccRemoveButton_Click(object sender, EventArgs e)
		{
			this.bccListBox.Items.RemoveAt(this.bccListBox.SelectedIndex);
		}

		private void CcAddButton_Click(object sender, EventArgs e)
		{
			this.ccListBox.Items.Add(this.ccTextBox.Text);
		}

		private void CcRemoveButton_Click(object sender, EventArgs e)
		{
			this.ccListBox.Items.RemoveAt(this.ccListBox.SelectedIndex);
		}

		private void DefaultPortCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (this.defaultPortCheckBox.Checked)
			{
				this.portNumericUpDown.Enabled = false;
				this.portNumericUpDown.Value = this.useSslCheckBox.Checked ? 465 : 25;
			}
			else
			{
				this.portNumericUpDown.Enabled = true;
			}
		}

		private void ToAddButton_Click(object sender, EventArgs e)
		{
			this.toListBox.Items.Add(this.toTextBox.Text);
		}

		private void ToRemoveButton_Click(object sender, EventArgs e)
		{
			this.toListBox.Items.RemoveAt(this.toListBox.SelectedIndex);
		}

		private void UseAuthenticationCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (this.useAuthenticationCheckBox.Checked)
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

		private void UseSslCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this.portNumericUpDown.Value = this.useSslCheckBox.Checked ? 465 : 25;
		}
	}
}