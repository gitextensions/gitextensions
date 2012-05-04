// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializableException.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Util.Serialization
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	[Serializable]
	public class SerializableException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SerializableException"/> class. 
		/// Default constructor provided for XML serialization and de-serialization.
		/// </summary>
		public SerializableException()
		{
		}

		public SerializableException(Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException();
			}

			this.Type = exception.GetType().ToString();

			if (exception.Data.Count != 0)
			{
				foreach (DictionaryEntry entry in exception.Data)
				{
					if (entry.Value != null)
					{
						// Assign 'Data' property only if there is at least one entry with non-null value
						if (this.Data == null)
						{
							this.Data = new SerializableDictionary<object, object>();
						}

						this.Data.Add(entry.Key, entry.Value);
					}
				}
			}

			if (exception.HelpLink != null)
			{
				this.HelpLink = exception.HelpLink;
			}

			if (exception.InnerException != null)
			{
				this.InnerException = new SerializableException(exception.InnerException);
			}

			if (exception is AggregateException)
			{
				this.InnerExceptions = new List<SerializableException>();

				foreach (var innerException in ((AggregateException)exception).InnerExceptions)
				{
					this.InnerExceptions.Add(new SerializableException(innerException));
				}

				this.InnerExceptions.RemoveAt(0);
			}

			this.Message = exception.Message != string.Empty ? exception.Message : string.Empty;

			if (exception.Source != null)
			{
				this.Source = exception.Source;
			}

			if (exception.StackTrace != null)
			{
				this.StackTrace = exception.StackTrace;
			}

			if (exception.TargetSite != null)
			{
				this.TargetSite = exception.TargetSite + " @ " + exception.TargetSite.DeclaringType;
			}

			this.ExtendedInformation = this.GetExtendedInformation(exception);
		}

		public SerializableDictionary<object, object> Data { get; set; }

		public SerializableDictionary<string, object> ExtendedInformation { get; set; }

		public string HelpLink { get; set; }

		public SerializableException InnerException { get; set; }

		public List<SerializableException> InnerExceptions { get; set; }
		
		public string Message { get; set; }

		public string Source { get; set; }

		public string StackTrace { get; set; }

		// This will make TargetSite property XML serializable but RuntimeMethodInfo class does not have a parameterless
		// constructor thus the serializer throws an exception if full info is used
		public string TargetSite { get; set;  }

		public string Type { get; set; }

		private SerializableDictionary<string, object> GetExtendedInformation(Exception exception)
		{
			var extendedProperties = from property in exception.GetType().GetProperties()
			                         where
																property.Name != "Data" && property.Name != "InnerExceptions" && property.Name != "InnerException" &&
			                         	property.Name != "Message" && property.Name != "Source" && property.Name != "StackTrace" &&
																property.Name != "TargetSite" && property.Name != "HelpLink" && property.CanRead
			                         select property;

			if (extendedProperties.Count() != 0)
			{
				var extendedInformation = new SerializableDictionary<string, object>();

				foreach (var property in extendedProperties.Where(property => property.GetValue(exception, null) != null))
				{
					extendedInformation.Add(property.Name, property.GetValue(exception, null));
				}

				return extendedInformation;
			}
			else
			{
				return null;
			}
		}
	}
}