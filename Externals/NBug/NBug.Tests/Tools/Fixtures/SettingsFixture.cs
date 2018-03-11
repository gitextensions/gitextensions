// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsFixture.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Tools.Fixtures
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml.Serialization;

	using NBug.Core.Util;
	using NBug.Enums;
	using NBug.Properties;

	using Settings = NBug.Settings;

	public class SettingsFixture : IDisposable
	{
		// This is static for performance reasons (also not disposed at the destructor due to the same reason)
		private static Stream settings;

		private List<string> dispatcherDestinations;

		public SettingsFixture()
		{
			if (settings == null)
			{
				// Assembly.GetEntryAssembly() is null for tests wihtout GUI runner so filling it in to prevent null object exceptions
				Settings.EntryAssembly = Assembly.GetExecutingAssembly();

				settings = new MemoryStream();
				Settings.SaveCustomSettings(settings, false);
			}
		}

		public void Dispose()
		{
			this.ReloadDefaults();
		}

		internal void InitializeStandardSettings()
		{
			Settings.SkipDispatching = true;
			Settings.UIMode = UIMode.None;
			Settings.WriteLogToDisk = false;
		}

		/// <summary>
		/// Returns null if the custom settings file is not found so check for null object reference.
		/// </summary>
		/// <returns>
		/// The <see cref="List"/>.
		/// </returns>
		internal List<string> ReadCustomDispatcherDestinationSettings(string protocol)
		{
			// This generates a sample settings file for future reference
			/*this.dispatcherDestinations.Add("Just testing this stuff.");
			var serializer = new XmlSerializer(typeof(List<string>));
			using (FileStream stream = new FileStream("DispatcherDestinations.xml", FileMode.Create))
			{
				serializer.Serialize(stream, this.dispatcherDestinations);
			}*/

			// Make sure that settings are red and cached only once during the object lifetime
			if (this.dispatcherDestinations == null)
			{
				this.dispatcherDestinations = new List<string>();
				var path = Path.Combine(Settings.NBugDirectory, "DispatcherDestinations.xml");

				if (!File.Exists(path))
				{
					path = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Docs\\ASFT\\Dev\\_Tools\\Settings\\DispatcherDestinations.xml");

					if (!File.Exists(path))
					{
						return null;
					}
				}

				var serializer = new XmlSerializer(typeof(List<string>));
				using (var stream = new FileStream(path, FileMode.Open))
				{
					this.dispatcherDestinations = (List<string>)serializer.Deserialize(stream);
				}
			}

			return
				(from destination in this.dispatcherDestinations where ConnectionStringParser.Parse(destination)["Type"].Equals(protocol) select destination)
					.ToList();
		}

		internal void ReloadDefaults()
		{
			SettingsOverride.LoadCustomSettings(settings);
		}
	}
}