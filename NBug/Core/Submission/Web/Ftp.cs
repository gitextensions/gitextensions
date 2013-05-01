// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ftp.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using NBug.Core.Reporting.Info;
using NBug.Core.Util.Serialization;

namespace NBug.Core.Submission.Web
{
	using System;
	using System.IO;
	using System.Net;
	using System.Text;

	using NBug.Core.Util.Logging;

	internal class FtpFactory : IProtocolFactory
	{
		public IProtocol FromConnectionString(string connectionString)
		{
			return new Ftp(connectionString);
		}

		public string SupportedType
		{
			get { return "Ftp"; }
		}
	}

	public class Ftp : ProtocolBase
	{
		public Ftp(string connectionString)
			: base(connectionString)
		{
		}

		public Ftp()
		{
		}

		// Connection string format (single line)
		// Warning: There should be no semicolon (;) or equals sign (=) used in any field except for password.
		// Warning: No fild value value should contain the phrase 'password='
		// Note: Url should be a full url with a trailing slash (/), like: ftp://....../

		/* Type=FTP;
		 * Url=ftp://tracker.mydomain.com/myproject/;
		 * UseSSL=false;
		 * Username=;
		 * Password=;
		 */

		public string Url { get; set; }

		public string Usessl { get; set; }

		public string Username { get; set; }

		public string Password { get; set; }

		public override bool Send(string fileName, Stream file, Report report, SerializableException exception)
		{
			var request = (FtpWebRequest) WebRequest.Create(new Uri(this.Url + fileName));

			if (!string.IsNullOrEmpty(this.Usessl))
			{
				request.EnableSsl = Convert.ToBoolean(this.Usessl.ToLower());
			}

			if (!string.IsNullOrEmpty(this.Username))
			{
				request.Credentials = new NetworkCredential(this.Username, this.Password);
			}

			request.Method = WebRequestMethods.Ftp.UploadFile;
			request.Proxy = null; // Otherwise we'll get an exception: The requested FTP command is not supported when a HTTP proxy is used

			using (var requestStream = request.GetRequestStream())
			{
				file.Position = 0;
				file.CopyTo(requestStream);
				file.Position = 0;
			}

			using (var response = (FtpWebResponse)request.GetResponse())
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				var responseString = reader.ReadToEnd(); // Null on successful transfer
				if (!string.IsNullOrEmpty(responseString))
				{
					Logger.Info("Response from FTP server: " + responseString);
				}

				Logger.Info("Response from FTP server: " + response.StatusDescription);
			}

			return true;
		}
	}
}
