// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializableException.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BugReporter.Serialization
{
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
            OriginalException = exception ?? throw new ArgumentNullException();

            var oldCulture = Thread.CurrentThread.CurrentCulture;
            var oldUICulture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                // Prefer messages in English, instead of in language of the user.
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

                Type = exception.GetType().ToString();

                if (exception.Data.Count != 0)
                {
                    foreach (DictionaryEntry entry in exception.Data)
                    {
                        if (entry.Value is not null)
                        {
                            // Assign 'Data' property only if there is at least one entry with non-null value
                            Data ??= new SerializableDictionary<object, object>();

                            Data.Add(entry.Key, entry.Value);
                        }
                    }
                }

                if (exception.HelpLink is not null)
                {
                    HelpLink = exception.HelpLink;
                }

                if (exception.InnerException is not null)
                {
                    InnerException = new SerializableException(exception.InnerException);
                }

                if (exception is AggregateException)
                {
                    InnerExceptions = new List<SerializableException>();

                    foreach (var innerException in ((AggregateException)exception).InnerExceptions)
                    {
                        InnerExceptions.Add(new SerializableException(innerException));
                    }

                    InnerExceptions.RemoveAt(0);
                }

                Message = exception.Message != string.Empty ? exception.Message : string.Empty;

                if (exception.Source is not null)
                {
                    Source = exception.Source;
                }

                if (exception.StackTrace is not null)
                {
                    StackTrace = exception.StackTrace;
                }

                if (exception.TargetSite is not null)
                {
                    TargetSite = string.Format("{0} @ {1}", exception.TargetSite, exception.TargetSite.DeclaringType);
                }

                ExtendedInformation = GetExtendedInformation(exception);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
                Thread.CurrentThread.CurrentUICulture = oldUICulture;
            }
        }

        public SerializableDictionary<object, object>? Data { get; set; }

        public SerializableDictionary<string, object>? ExtendedInformation { get; set; }

        public string? HelpLink { get; set; }

        public SerializableException? InnerException { get; set; }

        public List<SerializableException>? InnerExceptions { get; set; }

        public string? Message { get; set; }

        public Exception? OriginalException { get; }

        public string? Source { get; set; }

        public string? StackTrace { get; set; }

        // This will make TargetSite property XML serializable but RuntimeMethodInfo class does not have a parameterless
        // constructor thus the serializer throws an exception if full info is used
        public string? TargetSite { get; set; }

        public string? Type { get; set; }

        public override string ToString()
        {
            if (OriginalException is not null)
            {
                return OriginalException.ToString();
            }

            StringBuilder output = new();
            output.Append($"{Type}: {Message}");

            Stack<SerializableException> inners = new();
            SerializableException? inner = InnerException;
            while (inner is not null)
            {
                output.Append($" ---> {inner.Type}: {inner.Message}");
                inners.Push(inner);
                inner = inner.InnerException;
            }

            output.AppendLine();

            while (inners.Count > 0)
            {
                inner = inners.Pop();
                output.AppendLine(inner.StackTrace);
                output.AppendLine("   --- End of inner exception stack trace ---");
            }

            output.AppendLine(StackTrace);

            return output.ToString();
        }

        public string ToXmlString()
        {
            XmlSerializer serializer = new(typeof(SerializableException));
            using MemoryStream stream = new();
            stream.SetLength(0);
            serializer.Serialize(stream, this);
            stream.Position = 0;
            var doc = XDocument.Load(stream);
            return doc.Root.ToString();
        }

        public static SerializableException FromXmlString(string xml)
        {
            XmlSerializer serializer = new(typeof(SerializableException));
            using StringReader reader = new(xml);
            SerializableException exception = (SerializableException)serializer.Deserialize(reader);

            if (exception.StackTrace?.IndexOf(Environment.NewLine) < 0)
            {
                // Presume that the payload was serialized with \n only
                exception.StackTrace = exception.StackTrace.Replace("\n", Environment.NewLine);
            }

            return exception;
        }

        private SerializableDictionary<string, object>? GetExtendedInformation(Exception exception)
        {
            var extendedProperties = (from property in exception.GetType().GetProperties()
                                      where
                                          property.Name != "Data" && property.Name != "InnerExceptions" && property.Name != "InnerException"
                                          && property.Name != "Message" && property.Name != "Source" && property.Name != "StackTrace"
                                          && property.Name != "TargetSite" && property.Name != "HelpLink" && property.CanRead
                                      select property).ToArray();

            if (extendedProperties.Any())
            {
                SerializableDictionary<string, object> extendedInformation = new();

                foreach (var property in extendedProperties.Where(property => property.GetValue(exception, null) is not null))
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
