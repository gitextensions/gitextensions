// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Minimal.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.UI.WinForms
{
	using System.Windows.Forms;

	using NBug.Core.Reporting.Info;
	
	internal class Minimal
	{
		internal UIDialogResult ShowDialog(Report report)
		{
			MessageBox.Show(
				new Form { TopMost = true },
				Settings.Resources.UI_Dialog_Minimal_Message,
				report.GeneralInfo.HostApplication + " Error",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);

			return new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send);
		}
	}
}
