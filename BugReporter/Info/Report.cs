// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Report.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using BugReporter.Serialization;

namespace BugReporter.Info
{
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
            GeneralInfo = new GeneralInfo(serializableException);
        }

        /// <summary>
        /// Gets or sets the general information about the exception and the system to be presented in the bug report.
        /// </summary>
        public GeneralInfo? GeneralInfo { get; set; }

        public override string ToString()
        {
            XmlSerializer serializer = new(typeof(Report));
            using MemoryStream stream = new();
            stream.SetLength(0);
            serializer.Serialize(stream, this);
            stream.Position = 0;
            var doc = XDocument.Load(stream);
            return doc.Root.ToString();
        }
    }
}
