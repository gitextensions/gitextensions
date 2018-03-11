﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BugNet.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Submission.Tracker
{
	using System.IO;
	using System.Net;

	using NBug.Core.Reporting.Info;
	using NBug.Core.Util.Serialization;

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
			//HttpWebRequest request; //suppress unused Warning

			return true;
		}
	}
}