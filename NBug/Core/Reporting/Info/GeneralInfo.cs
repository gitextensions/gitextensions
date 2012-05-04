// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneralInfo.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Reporting.Info
{
	using System;
	using System.Diagnostics;
	using System.Reflection;

	using NBug.Core.Util.Serialization;

	[Serializable]
	public class GeneralInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GeneralInfo"/> class. This is the default constructor provided for XML
		/// serialization and de-serialization.
		/// </summary>
		public GeneralInfo()
		{
		}

		internal GeneralInfo(SerializableException serializableException)
		{
			// this.HostApplication = Settings.EntryAssembly.GetName().Name; // Does not get the extensions of the file!
			this.HostApplication = Settings.EntryAssembly.GetLoadedModules()[0].Name;

			// this.HostApplicationVersion = Settings.EntryAssembly.GetName().Version.ToString(); // Gets AssemblyVersion not AssemblyFileVersion
			this.HostApplicationVersion = this.NBugVersion = FileVersionInfo.GetVersionInfo(Settings.EntryAssembly.Location).ProductVersion;

			// this.NBugVersion = System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString(); // Gets AssemblyVersion not AssemblyFileVersion
			this.NBugVersion = FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location).ProductVersion;

			this.CLRVersion = Environment.Version.ToString();

			this.DateTime = System.DateTime.UtcNow.ToString();

			if (serializableException != null)
			{
				this.ExceptionType = serializableException.Type;

				if (!string.IsNullOrEmpty(serializableException.TargetSite))
				{
					this.TargetSite = serializableException.TargetSite;
				}
				else if (serializableException.InnerException != null && !string.IsNullOrEmpty(serializableException.InnerException.TargetSite))
				{
					this.TargetSite = serializableException.InnerException.TargetSite;
				}

				this.ExceptionMessage = serializableException.Message;
			}
		}

		public string CLRVersion { get; set; }

		public string DateTime { get; set; }

		public string ExceptionMessage { get; set; }

		public string ExceptionType { get; set; }

		public string HostApplication { get; set; }

		/// <summary>
		/// Gets or sets AssemblyFileVersion of host assembly.
		/// </summary>
		public string HostApplicationVersion { get; set; }

		/// <summary>
		/// Gets or sets AssemblyFileVersion of NBug.dll assembly.
		/// </summary>
		public string NBugVersion { get; set; }

		public string TargetSite { get; set; }

		public string UserDescription { get; set; }
	}
}
