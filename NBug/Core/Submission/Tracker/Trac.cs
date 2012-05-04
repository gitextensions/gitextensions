// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Trac.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Submission.Tracker
{
	using System.IO;

	internal class Trac : Protocol
	{
		protected Trac(string connectionString, Stream reportFile)
			: base(connectionString, reportFile, Protocols.Mail)
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

		internal bool Send()
		{
			// ToDo: Check to see if XML-RPC.NET is referenced and if not, show a developer UI as a waning -or- even better, dynamically load the assembly and use that and not the referenced one!
			return true;
		}
	}
}
