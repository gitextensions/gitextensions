// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Protocol.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Submission
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;
	using System.Xml.Serialization;

	using NBug.Core.Reporting.Info;
	using NBug.Core.Util.Exceptions;
	using NBug.Core.Util.Serialization;
	using NBug.Core.Util.Storage;

	public class Protocol
	{
		private readonly Dictionary<StoredItemType, XElement> reportItems = new Dictionary<StoredItemType, XElement>();

				/// <summary>
		/// Initializes a new instance of the Protocol class to be extended by derived types.
		/// </summary>
		/// <param name="connectionString">Connection string to be parsed.</param>
		/// <param name="protocol">Derived protocol type.</param>
		protected Protocol(string connectionString, Protocols protocol)
		{
			this.Type = protocol;
			var fields = Parse(connectionString);
			var properties = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

			foreach (var property in properties.Where(property => property.Name != "Type" && fields.ContainsKey(property.Name)))
			{
				property.SetValue(this, fields[property.Name], null);
			}
		}

		/// <summary>
		/// Initializes a new instance of the Protocol class to be extended by derived types.
		/// </summary>
		/// <param name="connectionString">Connection string to be parsed.</param>
		/// <param name="reportFile">Report file in which the report details are contained.</param>
		/// <param name="protocol">Derived protocol type.</param>
		protected Protocol(string connectionString, Stream reportFile, Protocols protocol) : this(connectionString, protocol)
		{
			this.ReportFile = reportFile;
			
			// ToDo: Optimize this so large dump files will not be loaded into the memory before sending but streamed from the disk
			// Not inside using(...) block to prevent file stream from getting closed
			var zipStorer = ZipStorer.Open(reportFile, FileAccess.Read);
			using (Stream zipItemStream = new MemoryStream())
			{
				var zipDirectory = zipStorer.ReadCentralDir();
				
				foreach (var entry in zipDirectory)
				{
					if (Path.GetFileName(entry.FilenameInZip) == StoredItemFile.Exception)
					{
						zipItemStream.SetLength(0);
						zipStorer.ExtractFile(entry, zipItemStream);
						zipItemStream.Position = 0;
						var deserializer = new XmlSerializer(typeof(SerializableException));
						this.SerializableException = (SerializableException)deserializer.Deserialize(zipItemStream);
						zipItemStream.Position = 0;
						this.reportItems.Add(StoredItemType.Exception, XElement.Load(zipItemStream));
					}
					else if (Path.GetFileName(entry.FilenameInZip) == StoredItemFile.Report)
					{
						zipItemStream.SetLength(0);
						zipStorer.ExtractFile(entry, zipItemStream);
						zipItemStream.Position = 0;
						var deserializer = new XmlSerializer(typeof(Report));
						this.Report = (Report)deserializer.Deserialize(zipItemStream);
						zipItemStream.Position = 0;
						this.reportItems.Add(StoredItemType.Report, XElement.Load(zipItemStream));
					}
				}
			}

			this.ReportFile.Position = 0;
		}

		protected Protocol(Protocols protocol)
		{
			this.Type = protocol;
		}

		/// <summary>
		/// Gets serialized representation of the connection string.
		/// </summary>
		internal string ConnectionString
		{
			get
			{
				var connectionString = "Type=" + this.Type + ";";
				var properties = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

				foreach (var property in properties)
				{
					var prop = property.GetValue(this, null);
					if (prop != null)
					{
						var val = prop.ToString();

						if (!string.IsNullOrEmpty(val))
						{
							// Escape = and ; characters
							connectionString += property.Name.Replace(";", @"\;").Replace("=", @"\=") + "=" + val.Replace(";", @"\;").Replace("=", @"\=") + ";";
						}
					}
				}

				return connectionString;
			}
		}

		// ToDo: Actually not the report file but an instance of Storer.cs should be used here! It has all the methods for handling the files anyway.
		protected Stream ReportFile { get; set; }

		protected SerializableException SerializableException { get; set; }

		protected Report Report { get; set; }

		private Protocols Type { get; set; }

		// Currently ; and = characters are illegal and needs preparsing and escaping
		internal static Dictionary<string, string> Parse(string connectionString)
		{
			try
			{
				// Pre-processing the connection string, detect escape sequence, trim trailing semicolon
				var data = new Dictionary<string, string>();
				var fields = Regex.Split(connectionString.TrimEnd(new[] { ';' }), @"(?<!\\);|(?<!\\)=");

				// Replace the escape sequence and build the dictionary
				for (var i = 0; i < fields.Length; i++)
				{
					data.Add(fields[i].Replace(@"\;", ";").Replace(@"\=", "="), fields[++i].Replace(@"\;", ";").Replace(@"\=", "="));
				}

				return data;
			}
			catch (Exception exception)
			{
				throw new NBugConfigurationException("Cannot parse the connection string supplied. The connection string may be malformed: " + connectionString, exception);
			}
		}

		internal XElement GetReport(StoredItemType reportItemType)
		{
			return this.reportItems[reportItemType];
		}

		// Password field may contain the illegal ';' character so it is always the last field and isolated
		internal string GetSettingsPasswordField(string connectionString)
		{
			return connectionString.Substring(connectionString.ToLower().IndexOf("password=") + 9).Substring(0, connectionString.Substring(connectionString.ToLower().IndexOf("password=") + 9).Length - 1);
		}
	}
}
