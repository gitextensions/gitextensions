// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProtocolBase.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using NBug.Core.Util;
using System;

namespace NBug.Core.Submission
{
	using NBug.Core.Reporting.Info;
	using NBug.Core.Util.Serialization;
	using System.IO;
	using System.Linq;
	using System.Reflection;

	public abstract class ProtocolBase : IProtocol
	{
		/// <summary>
		/// Initializes a new instance of the ProtocolBase class to be extended by derived types.
		/// </summary>
		/// <param name="connectionString">Connection string to be parsed.</param>
		protected ProtocolBase(string connectionString)
		{
			var fields = ConnectionStringParser.Parse(connectionString);
			var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

			foreach (var property in properties.Where(property => property.Name != "Type" && fields.ContainsKey(property.Name)))
			{
				if (property.PropertyType == typeof(bool))
					property.SetValue(this, Convert.ToBoolean(fields[property.Name].Trim()), null);
				else if (property.PropertyType == typeof(int))
					property.SetValue(this, Convert.ToInt32(fields[property.Name].Trim()), null);
				else if (property.PropertyType.BaseType == typeof(Enum))
					property.SetValue(this, Enum.Parse(property.PropertyType, fields[property.Name]), null);
				else
					property.SetValue(this, fields[property.Name], null);
			}
		}

		protected ProtocolBase()
		{
		}

		/// <summary>
		/// Gets serialized representation of the connection string.
		/// </summary>
		public string ConnectionString
		{
			get
			{
				var connectionString = String.Format("Type={0};", GetType().Name);
				var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.GetProperty)
					.Where(p => p.Name != "ConnectionString");

				foreach (var property in properties)
				{
					var prop = property.GetValue(this, null);
					if (prop != null)
					{
						var val = prop.ToString();

						if (!string.IsNullOrEmpty(val))
						{
							// Escape = and ; characters
							connectionString += String.Format("{0}={1};", property.Name.Replace(";", @"\;").Replace("=", @"\="), val.Replace(";", @"\;").Replace("=", @"\="));
						}
					}
				}

				return connectionString;
			}
		}

		// Password field may contain the illegal ';' character so it is always the last field and isolated
		internal string GetSettingsPasswordField(string connectionString)
		{
			return connectionString.Substring(connectionString.ToLower().IndexOf("password=") + 9).Substring(0, connectionString.Substring(connectionString.ToLower().IndexOf("password=") + 9).Length - 1);
		}

		public abstract bool Send(string fileName, Stream file, Report report, SerializableException exception);
	}
}