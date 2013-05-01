// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BugNet.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using NBug.Core.Reporting.Info;
using NBug.Core.Util.Serialization;

namespace NBug.Core.Submission.Tracker
{
	using System.IO;
	using System.Net;

	internal class BugNet : ProtocolBase
	{

		internal BugNet(string connectionString)
			: base(connectionString)
		{
		}

		internal BugNet()
		{
		}

		public override bool Send(string fileName, Stream file, Report report, SerializableException exception)
		{
			HttpWebRequest request;

			return true;
		}
	}
}