// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Normal.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.UI.WinForms
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;

	using NBug.Core.Reporting.Info;

	internal partial class Normal : Form
	{
		private UIDialogResult uiDialogResult;

		internal Normal()
		{
			InitializeComponent();
			this.Icon = Properties.Resources.NBug_icon_16;
			this.warningPictureBox.Image = SystemIcons.Warning.ToBitmap();
			this.warningLabel.Text = Settings.Resources.UI_Dialog_Normal_Message;
			this.continueButton.Text = Settings.Resources.UI_Dialog_Normal_Continue_Button;
			this.quitButton.Text = Settings.Resources.UI_Dialog_Normal_Quit_Button;
		}

		internal UIDialogResult ShowDialog(Report report)
		{
			this.Text = string.Format("{0} {1}", report.GeneralInfo.HostApplication, Settings.Resources.UI_Dialog_Normal_Title);
			this.exceptionMessageLabel.Text = report.GeneralInfo.ExceptionMessage;

			this.ShowDialog();

			return this.uiDialogResult;
		}

		private void ContinueButton_Click(object sender, EventArgs e)
		{
			this.uiDialogResult = new UIDialogResult(ExecutionFlow.ContinueExecution, SendReport.Send);
			this.Close();
		}

		private void QuitButton_Click(object sender, EventArgs e)
		{
			this.uiDialogResult = new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send);
			this.Close();
		}
	}
}
