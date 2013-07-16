namespace NBug
{
	using NBug.Core.Reporting.Info;
	using NBug.Core.Util.Serialization;
	using System;
	using System.IO;

	public class CustomSubmissionEventArgs : EventArgs
	{
		internal CustomSubmissionEventArgs(string fileName, Stream file, Report report, SerializableException exception)
		{
			FileName = fileName;
			File = file;
			Report = report;
			Exception = exception;
			Result = false;
		}

		public string FileName { get; private set; }

		public Stream File { get; private set; }

		public Report Report { get; private set; }

		public SerializableException Exception { get; private set; }

		public bool Result { get; set; }
	}
}