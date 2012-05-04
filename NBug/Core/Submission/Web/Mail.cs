// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Mail.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Submission.Web
{
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Mail;

	using NBug.Core.Util.Logging;
	using NBug.Core.Util.Storage;

	internal class Mail : Protocol
	{
		internal Mail(string connectionString, Stream reportFile)
			: base(connectionString, reportFile, Protocols.Mail)
		{
		}

		internal Mail(string connectionString)
			: base(connectionString, Protocols.Mail)
		{
		}

		internal Mail()
			: base(Protocols.Mail)
		{
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

		public string From { get; set; }

		public string FromName { get; set; }

		public string To { get; set; }

		public string Cc { get; set; }

		public string Bcc { get; set; }

		public string ReplyTo { get; set; }

		public string UseAttachment { get; set; }

		public string CustomSubject { get; set; }

		public string CustomBody { get; set; }

		public string SmtpServer { get; set; }

		public string UseSsl { get; set; }

		public string Port { get; set; }

		public string Priority { get; set; }

		public string UseAuthentication { get; set; }

		public string Username { get; set; }

		public string Password { get; set; }
		
		internal bool Send()
		{
			if (ReportFile == null)
			{
				return false;
			}

			if (string.IsNullOrEmpty(this.From) || string.IsNullOrEmpty(this.To))
			{
				return false;
			}

			if (string.IsNullOrEmpty(this.ReplyTo))
			{
				this.ReplyTo = this.From;
			}

			if (string.IsNullOrEmpty(this.UseSsl))
			{
				this.UseSsl = "false";
			}

			if (string.IsNullOrEmpty(this.Port))
			{
				this.Port = this.UseSsl == "true" ? "465" : "25";
			}

			if (string.IsNullOrEmpty(this.UseAttachment))
			{
				this.UseAttachment = "false";
			}

			// Make sure that we can use authentication even with emtpy username and password
			if (!string.IsNullOrEmpty(this.Username))
			{
				this.UseAuthentication = "true";
			}
			else if (string.IsNullOrEmpty(this.UseAuthentication))
			{
				this.UseAuthentication = "false";
			}

			using (var smtpClient = new SmtpClient())
			using (var message = new MailMessage())
			{
				if (!string.IsNullOrEmpty(this.SmtpServer))
				{
					smtpClient.Host = this.SmtpServer;
				}

				if (!string.IsNullOrEmpty(this.Port))
				{
					smtpClient.Port = Convert.ToInt32(this.Port);
				}

				if (this.UseAuthentication.ToLower() == "true")
				{
					smtpClient.Credentials = new NetworkCredential(this.Username, this.Password);
				}

				if (this.UseSsl == "true")
				{
					smtpClient.EnableSsl = true;
				}

				if (!string.IsNullOrEmpty(this.Cc))
				{
					message.CC.Add(this.Cc);
				}

				if (!string.IsNullOrEmpty(this.Bcc))
				{
					message.Bcc.Add(this.Bcc);
				}

				if (!string.IsNullOrEmpty(this.Priority))
				{
					switch (this.Priority.ToLower())
					{
						case "high":
							message.Priority = MailPriority.High;
							break;
						case "normal":
							message.Priority = MailPriority.Normal;
							break;
						case "low":
							message.Priority = MailPriority.Low;
							break;
					}
				}

				message.To.Add(this.To);
				message.ReplyToList.Add(this.ReplyTo);
				message.From = !string.IsNullOrEmpty(this.FromName) ? new MailAddress(this.From, this.FromName) : new MailAddress(this.From);

				if (this.UseAttachment.ToLower() == "true")
				{
					// ToDo: Report file name should be attached to the report file object itself, file shouldn't be accessed directly!
					this.ReportFile.Position = 0;
					message.Attachments.Add(new Attachment(this.ReportFile, Path.GetFileName(((FileStream)this.ReportFile).Name)));
				}

				if (!string.IsNullOrEmpty(this.CustomSubject))
				{
					message.Subject = this.CustomSubject;
				}
				else
				{
					message.Subject = "NBug: " + this.Report.GeneralInfo.HostApplication + " (" +
														this.Report.GeneralInfo.HostApplicationVersion + "): " +
														this.Report.GeneralInfo.ExceptionType + " @ " +
														this.Report.GeneralInfo.TargetSite;
				}

				if (!string.IsNullOrEmpty(this.CustomBody))
				{
					message.Body = this.CustomBody + Environment.NewLine + Environment.NewLine + this.GetReport(StoredItemType.Report) +
					               Environment.NewLine + Environment.NewLine + this.GetReport(StoredItemType.Exception);
				}
				else
				{
					message.Body = this.GetReport(StoredItemType.Report) + Environment.NewLine + Environment.NewLine +
												 this.GetReport(StoredItemType.Exception);
				}

				smtpClient.Send(message);
				Logger.Trace("Submitted bug report email to: " + this.To);

				return true;
			}
		}
	}
}
