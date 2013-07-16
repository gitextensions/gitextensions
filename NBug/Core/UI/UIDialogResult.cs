// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UIDialogResult.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.UI
{
	public enum SendReport
	{
		Send,
		DoNotSend
	}

	public enum ExecutionFlow
	{
		/// <summary>
		/// This will handle all unhandled exceptions to be able to continue execution.
		/// </summary>
		ContinueExecution,

		/// <summary>
		/// This will handle all unhandled exceptions and exit the application.
		/// </summary>
		BreakExecution,
	}

	public struct UIDialogResult
	{
		internal ExecutionFlow Execution;
		internal SendReport Report;

		public UIDialogResult(ExecutionFlow execution, SendReport report)
		{
			this.Execution = execution;
			this.Report = report;
		}
	}
}