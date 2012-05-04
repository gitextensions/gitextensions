// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Report.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Reporting.Info
{
	using System;

	using NBug.Core.Util.Serialization;

	[Serializable]
	public class Report
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Report"/> class to be filled with information later on.
		/// </summary>
		public Report()
		{
		}

		internal Report(SerializableException serializableException)
		{
			this.GeneralInfo = new GeneralInfo(serializableException);
		}

		/// <summary>
		/// Gets or sets a custom object property to store user supplied information in the bug report. You need to handle
		/// <see cref="NBug.Settings.ProcessingException"/> event to fill this property with required information. 
		/// </summary>
		public object CustomInfo { get; set; }

		/// <summary>
		/// Gets or sets the general information about the exception and the system to be presented in the bug report.
		/// </summary>
		public GeneralInfo GeneralInfo { get; set; }

		/*/// <summary>
		/// Gets or sets a custom object property to store user supplied information in the bug report. You need to handle
		/// <see cref="NBug.Settings.ProcessingException"/> event to fill this property with required information. 
		/// </summary>
		public object StaticInfo { get; set; }*/
	}
}
