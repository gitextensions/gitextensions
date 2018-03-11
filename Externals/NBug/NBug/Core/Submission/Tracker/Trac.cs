﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Trac.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Submission.Tracker
{
	using System.IO;

	using NBug.Core.Reporting.Info;
	using NBug.Core.Util.Serialization;

	internal class Trac : ProtocolBase
	{
		protected Trac(string connectionString)
			: base(connectionString)
		{
		}

		// Connection string format (single line)
		// Warning: There should be no semicolon (;) or equals sign (=) used in any field except for password
		// Warning: No fild value value should contain the phrase 'password='
		// Warning: XML-RPC.NET assembly should be referenced
		// Note: Url should be a full url without a trailing slash (/), like: http://......
		// Note: Anononymous URL is: http://trac-hacks.org/xmlrpc and authenticated URL is: http://trac-hacks.org/login/xmlrpc

		/* Type=Trac;
		 * Url=http://tracker.mydomain.com/xmlrpc;
		 */
		public override bool Send(string fileName, Stream file, Report report, SerializableException exception)
		{
			// ToDo: Check to see if XML-RPC.NET is referenced and if not, show a developer UI as a waning -or- even better, dynamically load the assembly and use that and not the referenced one!
			return true;
		}
	}
}