// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Minimal.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Configuration;

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
				report.GeneralInfo.HostApplication + " " + Properties.Localization.UI_Dialog_Minimal_Title, 
				MessageBoxButtons.OK, 
				MessageBoxIcon.Warning);

			return new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send);
		}
	}
}