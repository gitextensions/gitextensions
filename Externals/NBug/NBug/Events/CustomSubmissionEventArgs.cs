// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomSubmissionEventArgs.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Events
{
	using System;
	using System.IO;

	using NBug.Core.Reporting.Info;
	using NBug.Core.Util.Serialization;

	public class CustomSubmissionEventArgs : EventArgs
	{
		internal CustomSubmissionEventArgs(string fileName, Stream file, Report report, SerializableException exception)
		{
			this.FileName = fileName;
			this.File = file;
			this.Report = report;
			this.Exception = exception;
			this.Result = false;
		}

		public SerializableException Exception { get; private set; }

		public Stream File { get; private set; }

		public string FileName { get; private set; }

		public Report Report { get; private set; }

		public bool Result { get; set; }
	}
}