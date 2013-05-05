// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Dispatcher.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using NBug.Core.Reporting.Info;
using NBug.Core.Util.Serialization;

namespace NBug.Core.Submission
{
	using System;
	using System.IO;
	using System.Threading.Tasks;

	using NBug.Core.Util.Logging;
	using NBug.Core.Util.Storage;

	internal class Dispatcher
	{
		/// <summary>
		/// Initializes a new instance of the Dispatcher class to send queued reports.
		/// </summary>
		/// <param name="isAsynchronous">
		/// Decides whether to start the dispatching process asynchronously on a background thread.
		/// </param>
		internal Dispatcher(bool isAsynchronous)
		{
			// Test if it has NOT been more than x many days since entry assembly was last modified)
			// This is the exact verifier code in the BugReport.cs of CreateReportZip() function
			if (Settings.StopReportingAfter < 0 || File.GetLastWriteTime(Settings.EntryAssembly.Location).AddDays(Settings.StopReportingAfter).CompareTo(DateTime.Now) > 0)
			{
				if (isAsynchronous)
				{
					// Log and swallow NBug's internal exceptions by default
					Task.Factory.StartNew(this.Dispatch).ContinueWith(
						t => Logger.Error("An exception occurred while dispatching bug report. Check the inner exception for details", t.Exception),
						TaskContinuationOptions.OnlyOnFaulted);
				}
				else
				{
					try
					{
						this.Dispatch();
					}
					catch (Exception exception)
					{
						Logger.Error("An exception occurred while dispatching bug report. Check the inner exception for details.", exception);
					}
				}
			}
			else
			{
				// ToDo: Cleanout all the remaining report files and disable NBug completely
			}
		}

		private void Dispatch()
		{
			// Make sure that we are not interfering with the crucial startup work);
			if (!Settings.RemoveThreadSleep)
			{
				System.Threading.Thread.Sleep(Settings.SleepBeforeSend * 1000);
			}

			// Turncate extra report files and try to send the first one in the queue
			Storer.TruncateReportFiles();

			// Now go through configured destinations and submit to all automatically
			for (bool hasReport = true; hasReport;)
			{
				using (Storer storer = new Storer())
				using (Stream stream = storer.GetFirstReportFile())
				{
					if (stream != null)
					{
						var exceptionData = GetDataFromZip(stream);
						if (this.EnumerateDestinations(stream, exceptionData) == false)
						{
							break;
						}

						// Delete the file after it was sent
						storer.DeleteCurrentReportFile();
					}
					else
					{
						hasReport = false;
					}
				}
			}
		}

		private class ExceptionData
		{
			public Report Report { get; set; }
			public SerializableException Exception { get; set; }
		}

		private ExceptionData GetDataFromZip(Stream stream)
		{
			var results = new ExceptionData();
			var zipStorer = ZipStorer.Open(stream, FileAccess.Read);
			using (Stream zipItemStream = new MemoryStream())
			{
				var zipDirectory = zipStorer.ReadCentralDir();
				foreach (var entry in zipDirectory)
				{
					if (Path.GetFileName(entry.FilenameInZip) == StoredItemFile.Exception)
					{
						zipItemStream.SetLength(0);
						zipStorer.ExtractFile(entry, zipItemStream);
						zipItemStream.Position = 0;
						var deserializer = new XmlSerializer(typeof(SerializableException));
						results.Exception = (SerializableException)deserializer.Deserialize(zipItemStream);
						zipItemStream.Position = 0;
					}
					else if (Path.GetFileName(entry.FilenameInZip) == StoredItemFile.Report)
					{
						zipItemStream.SetLength(0);
						zipStorer.ExtractFile(entry, zipItemStream);
						zipItemStream.Position = 0;
						var deserializer = new XmlSerializer(typeof(Report));
						results.Report = (Report)deserializer.Deserialize(zipItemStream);
						zipItemStream.Position = 0;
					}
				}
			}

			return results;
		}

		/// <summary>
		/// Enumerate all protocols to see if they are properly configured and send using the ones that are configured 
		/// as many times as necessary.
		/// </summary>
		/// <param name="reportFile">The file to read the report from.</param>
		/// <returns>Returns <see langword="true"/> if the sending was successful. 
		/// Returns <see langword="true"/> if the report was submitted to at least one destination.</returns>
		private bool EnumerateDestinations(Stream reportFile, ExceptionData exceptionData)
		{
			bool sentSuccessfullyAtLeastOnce = false;
			string fileName = Path.GetFileName(((FileStream) reportFile).Name);
			foreach (var destination in Settings.Destinations)
			{
				try
				{
					Logger.Trace(string.Format("Submitting bug report via {0}.", destination.GetType().Name));
					if (destination.Send(fileName, reportFile, exceptionData.Report, exceptionData.Exception))
					{
						sentSuccessfullyAtLeastOnce = true;
					}
				}
				catch (Exception exception)
				{
					Logger.Error(string.Format("An exception occurred while submitting bug report with {0}. Check the inner exception for details.", destination.GetType().Name), exception);
				}
			}
			return sentSuccessfullyAtLeastOnce;
		}
	}
}
