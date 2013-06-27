// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WPFUI.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.UI.WPF
{
    using System.ComponentModel;
	using NBug.Core.Reporting.Info;
	using NBug.Core.Util.Exceptions;
	using NBug.Core.Util.Serialization;
	using NBug.Enums;

	/// <summary>
	/// This class is used to prevent statically referencing any WPF dlls from the UISelector.cs thus prevents
	/// any unnecessary assembly from getting loaded into the memory.
	/// </summary>
	internal static class CustomUI
	{
		internal static UIDialogResult ShowDialog(UIMode uiMode, SerializableException exception, Report report)
		{
            if (Settings.CustomUIHandle != null)
			{
                var e = new CustomUIEventArgs(uiMode, exception, report);
                Settings.CustomUIHandle.DynamicInvoke(null, e);
                return e.Result;
            }
			else
			{
				throw NBugConfigurationException.Create(() => Settings.UIMode, "Parameter supplied for settings property is invalid.");
			}
		}
	}
}
