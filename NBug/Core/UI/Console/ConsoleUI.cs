// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleUI.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.UI.Console
{
	using System;

	using NBug.Core.Reporting.Info;
	using NBug.Core.Util.Exceptions;
	using NBug.Core.Util.Serialization;
	using NBug.Enums;

	internal static class ConsoleUI
	{
		internal static UIDialogResult ShowDialog(UIMode uiMode, SerializableException exception, Report report)
		{
			if (uiMode == UIMode.Minimal)
			{
				// Do not interact with the user
				Console.WriteLine(Environment.NewLine + Settings.Resources.UI_Console_Minimal_Message);
				return new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send);
			}
			else if (uiMode == UIMode.Normal)
			{
				// ToDo: Create normal console UI
				Console.WriteLine(Environment.NewLine + Settings.Resources.UI_Console_Normal_Message);
				return new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send);
			}
			else if (uiMode == UIMode.Full)
			{
				// ToDo: Create full console UI
				Console.WriteLine(Environment.NewLine + Settings.Resources.UI_Console_Full_Message);
				return new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send);
			}
			else
			{
				throw NBugConfigurationException.Create(() => Settings.UIMode, "Parameter supplied for settings property is invalid.");
			}
		}
	}
}