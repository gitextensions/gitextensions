// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WPFUI.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.UI.WPF
{
	using NBug.Core.Reporting.Info;
	using NBug.Core.UI.WinForms;
	using NBug.Core.Util.Exceptions;
	using NBug.Core.Util.Serialization;
	using NBug.Enums;

	/// <summary>
	/// This class is used to prevent statically referencing any WPF dlls from the UISelector.cs thus prevents
	/// any unnecessary assembly from getting loaded into the memory.
	/// </summary>
	internal static class WPFUI
	{
		internal static UIDialogResult ShowDialog(UIMode uiMode, SerializableException exception, Report report)
		{
			if (uiMode == UIMode.Minimal)
			{
				// ToDo:  Create WPF dialogs
				return new Minimal().ShowDialog(report);
			}
			else if (uiMode == UIMode.Normal)
			{
				// ToDo:  Create WPF dialogs
				using (var ui = new Normal())
				{
					return ui.ShowDialog(report);
				}
			}
			else if (uiMode == UIMode.Full)
			{
				// ToDo:  Create WPF dialogs
				using (var ui = new Full())
				{
					return ui.ShowDialog(exception, report);
				}
			}
			else
			{
				throw NBugConfigurationException.Create(() => Settings.UIMode, "Parameter supplied for settings property is invalid.");
			}
		}
	}
}