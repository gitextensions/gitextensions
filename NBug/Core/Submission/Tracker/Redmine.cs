// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Redmine.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Submission.Tracker
{
	using System;
	using System.IO;
	using System.Net;
	using System.Text;
	using System.Xml.Linq;

	using NBug.Core.Util.Logging;
	using NBug.Core.Util.Storage;

	internal class Redmine : Protocol
	{
		internal Redmine(string connectionString, Stream reportFile)
			: base(connectionString, reportFile, Protocols.Redmine)
		{
		}

		internal Redmine(string connectionString)
			: base(connectionString, Protocols.Redmine)
		{
		}

		internal Redmine()
			: base(Protocols.Redmine)
		{
		}

		// Connection string format (single line)
		// Warning: There should be no semicolon (;) or equals sign (=) used in any field except for password
		// Note: Url should be a full url with a trailing slash (/), like: http://....../

		/* Type=Redmine;
		 * Url=http://tracker.mydomain.com/;
		 * ProjectId=myproject;
		 * TrackerId=;
		 * PriorityId=;
		 * CategoryId=;
		 * CustomSubject=;
		 * FixedVersionId=;
		 * AssignedToId=;
		 * ParentId=;
		 * StatusId=;
		 * AuthorId=;
		 * ApiKey=myapikey;
		 */

		public string Url { get; set; }

		public string ProjectId { get; set; }

		public string TrackerId { get; set; }

		public string PriorityId { get; set; }

		public string CategoryId { get; set; }

		public string CustomSubject { get; set; }

		public string FixedVersionId { get; set; }

		public string AssignedToId { get; set; }

		public string ParentId { get; set; }

		public string StatusId { get; set; }

		public string AuthorId { get; set; }

		public string ApiKey { get; set; }

		internal bool Send()
		{
			HttpWebRequest request;

			if (string.IsNullOrEmpty(this.ApiKey))
			{
				request = (HttpWebRequest)WebRequest.Create(new Uri(this.Url + "issues.xml"));
			}
			else
			{
				request = (HttpWebRequest)WebRequest.Create(new Uri(this.Url + "issues.xml?key=" + this.ApiKey));
			}

			// Used POST as per http://www.redmine.org/projects/redmine/wiki/Rest_Issues#Creating-an-issue
			request.Method = "POST";
			request.ContentType = "application/xml";
			request.Accept = "application/xml";
			request.ServicePoint.Expect100Continue = false; // Patch #11593. Some servers seem to have problem accepting the Expect: 100-continue header

			// Redmine v1.1.1 REST XML templates (* indicate required fields)
			// Note: <*_id> fields always ask for numeric values

			/* <?xml version="1.0"?>
			 * <issue>
			 *   *<subject>Example</subject>
			 *   *<project_id>myproject</project_id>
			 *   <tracker_id></tracker_id> <!-- Bug=1/Issue=2/etc. -->
			 *   <priority_id></priority_id> <!-- Normal=4 -->
			 *   <category_id></category_id>
			 *   <description></description>
			 *   <fixed_version_id></fixed_version_id>
			 *   <assigned_to_id></assigned_to_id>
			 *   <parent_id></parent_id>
			 *   <status_id></status_id>
			 *   <author_id></author_id>
			 *   <start_date></start_date>
			 *   <due_date></due_date>
			 *   <done_ratio></done_ratio>
			 *   <estimated_hours></estimated_hours>
			 *   <created_on></created_on>
			 *   <updated_on></updated_on>
			 * </issue>
			 */

			var subject = "NBug: " + this.Report.GeneralInfo.HostApplication + " (" +
										this.Report.GeneralInfo.HostApplicationVersion + "): " +
										this.Report.GeneralInfo.ExceptionType + " @ " +
										this.Report.GeneralInfo.TargetSite;

			var description = "<pre>" + this.GetReport(StoredItemType.Report) + Environment.NewLine + Environment.NewLine +
			                  this.GetReport(StoredItemType.Exception) + "</pre>";

			var redmineRequestXml = new XElement("issue", new XElement("project_id", this.ProjectId));

			if (!string.IsNullOrEmpty(this.TrackerId))
			{
				redmineRequestXml.Add(new XElement("tracker_id", this.TrackerId));
			}

			if (!string.IsNullOrEmpty(this.PriorityId))
			{
				redmineRequestXml.Add(new XElement("priority_id", this.PriorityId));
			}

			if (!string.IsNullOrEmpty(this.CategoryId))
			{
				redmineRequestXml.Add(new XElement("category_id", this.CategoryId));
			}

			// Add the subject and make sure that it is less than 255 characters or Redmine trows an HTTP error code 422 : Unprocessable Entity
			if (!string.IsNullOrEmpty(this.CustomSubject))
			{
				var sbj = this.CustomSubject + " : " + subject;
				redmineRequestXml.Add(sbj.Length > 254 ? new XElement("subject", sbj.Substring(0, 254)) : new XElement("subject", sbj));
			}
			else
			{
				redmineRequestXml.Add(subject.Length > 254 ? new XElement("subject", subject.Substring(0, 254)) : new XElement("subject", subject));
			}

			if (!string.IsNullOrEmpty(description))
			{
				redmineRequestXml.Add(new XElement("description", description));
			}

			if (!string.IsNullOrEmpty(this.FixedVersionId))
			{
				redmineRequestXml.Add(new XElement("fixed_version_id", this.FixedVersionId));
			}

			if (!string.IsNullOrEmpty(this.AssignedToId))
			{
				redmineRequestXml.Add(new XElement("assigned_to_id", this.AssignedToId));
			}

			if (!string.IsNullOrEmpty(this.ParentId))
			{
				redmineRequestXml.Add(new XElement("parent_id", this.ParentId));
			}

			if (!string.IsNullOrEmpty(this.StatusId))
			{
				redmineRequestXml.Add(new XElement("status_id", this.StatusId));
			}

			if (!string.IsNullOrEmpty(this.AuthorId))
			{
				redmineRequestXml.Add(new XElement("author_id", this.AuthorId));
			}
		
			var bytes = Encoding.UTF8.GetBytes(redmineRequestXml.ToString());

			request.ContentLength = bytes.Length;

			using (var putStream = request.GetRequestStream())
			{
				putStream.Write(bytes, 0, bytes.Length);
			}

			// Log the response from Redmine RESTful service
			using (var response = (HttpWebResponse)request.GetResponse())
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				Logger.Info("Response from Redmine Issue Tracker: " + reader.ReadToEnd());
			}

			return true;
		}
	}
}
