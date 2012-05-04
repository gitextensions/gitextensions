// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BugNet.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Submission.Tracker
{
	using System.IO;
	using System.Net;

	public class BugNet : Protocol
	{
		internal BugNet(string connectionString, Stream reportFile)
			: base(connectionString, reportFile, Protocols.BugNET)
		{
		}

		internal BugNet(string connectionString)
			: base(connectionString, Protocols.BugNET)
		{
		}

		internal BugNet()
			: base(Protocols.BugNET)
		{
		}

		internal bool Send()
		{
			HttpWebRequest request;

			return true;
		}
	}
}