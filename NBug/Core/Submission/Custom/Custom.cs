using System.Xml.Linq;
using System.Xml.Serialization;
using NBug.Core.Reporting.Info;
using NBug.Core.Util.Serialization;

namespace NBug.Core.Submission.Custom
{
	using System.IO;
	using System.Net;
	using System.Net.Mail;

	using NBug.Core.Util.Logging;

	public class CustomFactory : IProtocolFactory
	{
		public IProtocol FromConnectionString(string connectionString)
		{
			return new Custom(connectionString);
		}

		public string SupportedType
		{
			get { return "Custom"; }
		}
	}

	public class Custom : ProtocolBase
	{
		public Custom(string connectionString)
			: base(connectionString)
		{
		}

		public Custom()
		{			
		}	

		public override bool Send(string fileName, Stream file, Report report, SerializableException exception)
		{
			if (Settings.CustomSubmissionHandle != null)
				return false;

			var e = new CustomSubmissionEventArgs(fileName, file, report, exception);
			Settings.CustomSubmissionHandle.DynamicInvoke(this, e);
			return e.Result;			
		}
	}
}

