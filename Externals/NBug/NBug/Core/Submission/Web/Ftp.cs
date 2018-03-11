﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ftp.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Submission.Web
{
	using System;
	using System.IO;
	using System.Net;

	using NBug.Core.Reporting.Info;
	using NBug.Core.Util.Logging;
	using NBug.Core.Util.Serialization;

	internal class FtpFactory : IProtocolFactory
	{
		public string SupportedType
		{
			get
			{
				return "Ftp";
			}
		}

		public IProtocol FromConnectionString(string connectionString)
		{
			return new Ftp(connectionString);
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

		public string Password { get; set; }

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

		public string Username { get; set; }

		public string Usessl { get; set; }

		public override bool Send(string fileName, Stream file, Report report, SerializableException exception)
		{
			var request = (FtpWebRequest)WebRequest.Create(new Uri(this.Url + fileName));

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