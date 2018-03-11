// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Mail.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Submission.Web
{
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Mail;

	using NBug.Core.Reporting.Info;
	using NBug.Core.Util.Logging;
	using NBug.Core.Util.Serialization;

	public class MailFactory : IProtocolFactory
	{
		public string SupportedType
		{
			get
			{
				return "Mail";
			}
		}

		public IProtocol FromConnectionString(string connectionString)
		{
			return new Mail(connectionString);
		}
	}

	public class Mail : ProtocolBase
	{
		public Mail(string connectionString)
			: base(connectionString)
		{
		}

		public Mail()
		{
			this.Port = 25;
		}

		// Connection string format (single line)
		// Warning: There should be no semicolon (;) or equals sign (=) used in any field except for password.
		// Warning: Password filed should be the last item.
		// Warning: No fild value value should contain the phrase 'password='
		// Note: Normal ports may be 25/26 depending on the host!
		// Note: Priority can be High, Normal, Low (Normal by default).

		/* Type=Mail;
		 * From=my_tracker@gmail.com;
		 * FromName=NBug Error Reporter;
		 * To=bugtracker@mycompany.com,someone@dummy.com,my_tracker@gmail.com;
		 * Cc=;
		 * Bcc=;
		 * ReplyTo=;
		 * UseAttachment=false;
		 * CustomSubject=;
		 * CustomBody=;
		 * SmtpServer=smtp.gmail.com;
		 * UseSSL=yes;
		 * Port=465;
		 * Priority=;
		 * UseAuthentication=yes;
		 * Username=my_tracker@gmail.com;
		 * Password=mypassword;
		 */
		public string Bcc { get; set; }

		public string Cc { get; set; }

		public string CustomBody { get; set; }

		public string CustomSubject { get; set; }

		public string From { get; set; }

		public string FromName { get; set; }

		public string Password { get; set; }

		public int Port { get; set; }

		public MailPriority Priority { get; set; }

		public string ReplyTo { get; set; }

		public string SmtpServer { get; set; }

		public string To { get; set; }

		public bool UseAttachment { get; set; }

		public bool UseAuthentication { get; set; }

		public bool UseSsl { get; set; }

		public string Username { get; set; }

		public override bool Send(string fileName, Stream file, Report report, SerializableException exception)
		{
			if (string.IsNullOrEmpty(this.From) || string.IsNullOrEmpty(this.To))
			{
				return false;
			}

			if (string.IsNullOrEmpty(this.ReplyTo))
			{
				this.ReplyTo = this.From;
			}

			if (this.Port <= 0)
			{
				this.Port = this.UseSsl ? 465 : 25;
			}

			if (!this.UseAttachment)
			{
				this.UseAttachment = false;
			}

			// Make sure that we can use authentication even with emtpy username and password
			if (!string.IsNullOrEmpty(this.Username))
			{
				this.UseAuthentication = true;
			}

			using (var smtpClient = new SmtpClient())
			using (var message = new MailMessage())
			{
				if (!string.IsNullOrEmpty(this.SmtpServer))
				{
					smtpClient.Host = this.SmtpServer;
				}

				smtpClient.Port = this.Port;

				if (this.UseAuthentication)
				{
					smtpClient.Credentials = new NetworkCredential(this.Username, this.Password);
				}

				smtpClient.EnableSsl = this.UseSsl;

				if (!string.IsNullOrEmpty(this.Cc))
				{
					message.CC.Add(this.Cc);
				}

				if (!string.IsNullOrEmpty(this.Bcc))
				{
					message.Bcc.Add(this.Bcc);
				}

				message.Priority = this.Priority;

				message.To.Add(this.To);
				message.ReplyToList.Add(this.ReplyTo);
				message.From = !string.IsNullOrEmpty(this.FromName) ? new MailAddress(this.From, this.FromName) : new MailAddress(this.From);

				if (this.UseAttachment)
				{
					// ToDo: Report file name should be attached to the report file object itself, file shouldn't be accessed directly!
					file.Position = 0;
					message.Attachments.Add(new Attachment(file, fileName));
				}

				if (!string.IsNullOrEmpty(this.CustomSubject))
				{
					message.Subject = this.CustomSubject;
				}
				else
				{
					message.Subject = "NBug: " + report.GeneralInfo.HostApplication + " (" + report.GeneralInfo.HostApplicationVersion + "): "
					                  + report.GeneralInfo.ExceptionType + " @ " + report.GeneralInfo.TargetSite;
				}

				if (!string.IsNullOrEmpty(this.CustomBody))
				{
					message.Body = this.CustomBody + Environment.NewLine + Environment.NewLine + report + Environment.NewLine + Environment.NewLine + exception;
				}
				else
				{
					message.Body = report + Environment.NewLine + Environment.NewLine + exception;
				}

				smtpClient.Send(message);
				Logger.Trace("Submitted bug report email to: " + this.To);

				return true;
			}
		}
	}
}