// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InternalExceptionViewer.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.UI.Developer
{
	using System;
	using System.Diagnostics;
	using System.Drawing;
	using System.Windows.Forms;

	using NBug.Core.Util.Exceptions;
	using NBug.Core.Util.Serialization;
	using NBug.Properties;

	internal partial class InternalExceptionViewer : Form
	{
		internal InternalExceptionViewer()
		{
			this.InitializeComponent();
			this.Icon = Resources.NBug_icon_16;
			this.warningPictureBox.Image = SystemIcons.Warning.ToBitmap();
		}

		internal void ShowDialog(Exception exception)
		{
			if (exception is NBugConfigurationException)
			{
				this.ShowDialog(exception as NBugConfigurationException);
			}
			else if (exception is NBugRuntimeException)
			{
				this.ShowDialog(exception as NBugRuntimeException);
			}
			else
			{
				this.messageLabel.Text =
					"An internal runtime exception has occurred. This maybe due to a configuration failure or an internal bug. You may choose to debug the exception or send a bug report to NBug developers. You may also use discussion forum to get help.";
				this.bugReportButton.Enabled = true;
				this.DisplayExceptionDetails(exception);
			}
		}

		internal void ShowDialog(NBugConfigurationException configurationException)
		{
			this.messageLabel.Text =
				"An internal configuration exception has occurred. Please correct the invalid configuration regarding the information below. You may also use discussion forum to get help or read the online documentation's configuration section.";
			this.invalidSettingLabel.Enabled = true;
			this.invalidSettingTextBox.Enabled = true;
			this.invalidSettingTextBox.Text = configurationException.MisconfiguredProperty;
			this.DisplayExceptionDetails(configurationException);
		}

		internal void ShowDialog(NBugRuntimeException runtimeException)
		{
			this.messageLabel.Text =
				"An internal runtime exception has occurred. This maybe due to a configuration failure or an internal bug. You may choose to debug the exception or send a bug report to NBug developers. You may also use discussion forum to get help.";
			this.bugReportButton.Enabled = true;
			this.DisplayExceptionDetails(runtimeException);
		}

		private void BugReportButton_Click(object sender, EventArgs e)
		{
			// ToDo: Activate internal bug reporting feature (and add some integrations tests for it)
			/*new BugReport();
			new Dispatcher(false);
			MessageBox.Show("Successfully sent bug report to NBug developer community.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);*/
			MessageBox.Show(
				"Internal bug reporting feature is not implemented yet but you can still manually submit a bug report using the bug tracker.", 
				"Information", 
				MessageBoxButtons.OK, 
				MessageBoxIcon.Information);
			this.bugReportButton.Enabled = false;
		}

		private void DebugButton_Click(object sender, EventArgs e)
		{
			// Let the exception propagate down to SEH
			this.Close();
		}

		private void DisplayExceptionDetails(Exception exception)
		{
			this.exceptionTextBox.Text = exception.GetType().ToString();
			this.exceptionMessageTextBox.Text = exception.Message;

			if (exception.TargetSite != null)
			{
				this.targetSiteTextBox.Text = exception.TargetSite.ToString();
			}
			else if (exception.InnerException != null && exception.InnerException.TargetSite != null)
			{
				this.targetSiteTextBox.Text = exception.InnerException.TargetSite.ToString();
			}

			this.exceptionDetails.Initialize(new SerializableException(exception));
			this.ShowDialog();
		}

		private void DocumentationToolStripButton_Click(object sender, EventArgs e)
		{
			Process.Start(this.documentationToolStripButton.Tag.ToString());
		}

		private void ForumToolStripLabel_Click(object sender, EventArgs e)
		{
			Process.Start(this.forumToolStripLabel.Tag.ToString());
		}

		private void QuitButton_Click(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		private void TrackerToolStripLabel_Click(object sender, EventArgs e)
		{
			Process.Start(this.trackerToolStripLabel.Tag.ToString());
		}
	}
}