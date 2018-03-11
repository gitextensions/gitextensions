// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatcherTests.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Integration
{
	using System;
	using System.Diagnostics;

	using NBug.Core.Reporting;
	using NBug.Core.Submission;
	using NBug.Core.Submission.Tracker;
	using NBug.Core.Submission.Web;
	using NBug.Core.Util;
	using NBug.Core.Util.Storage;
	using NBug.Enums;
	using NBug.Tests.Tools.Fixtures;
	using NBug.Tests.Tools.Stubs;

	using Xunit;

	public class DispatcherTests : IUseFixture<SettingsFixture>, IUseFixture<ReportFixture>
	{
		private SettingsFixture settings;

		[Fact]
		public void EmailDispatcher()
		{
			var destinations = this.settings.ReadCustomDispatcherDestinationSettings(typeof(Mail).Name);
			foreach (var destination in destinations)
			{
				Settings.Destinations.Clear();
				Settings.AddDestinationFromConnectionString(destination);
				try
				{
					throw new DummyArgumentException();
				}
				catch (Exception exception)
				{
					Trace.WriteLine("Sending e-mail with connection string: " + destination);
					new BugReport().Report(exception, ExceptionThread.Main);
					Assert.Equal(1, Storer.GetReportCount());
					new Dispatcher(false); // Dispatch async = false;
					Assert.Equal(0, Storer.GetReportCount());
				}
			}

			Trace.WriteLine("Emails sent: " + destinations.Count);
		}

		[Fact]
		public void FtpDispatcher()
		{
			var destinations = this.settings.ReadCustomDispatcherDestinationSettings(typeof(Ftp).Name);
			foreach (var destination in destinations)
			{
				Settings.Destinations.Clear();
				Settings.AddDestinationFromConnectionString(destination);
				try
				{
					throw new DummyArgumentException();
				}
				catch (Exception exception)
				{
					Trace.WriteLine("Uploading report using FTP with connection string: " + destination);
					new BugReport().Report(exception, ExceptionThread.Main);
					Assert.Equal(1, Storer.GetReportCount());
					new Dispatcher(false); // Dispatch async = false;
					Assert.Equal(0, Storer.GetReportCount());
				}
			}

			Trace.WriteLine("FTP uploads: " + destinations.Count);
		}

		[Fact]
		public void HttpDispatcher()
		{
			var destinations = this.settings.ReadCustomDispatcherDestinationSettings(typeof(Http).Name);
			foreach (var destination in destinations)
			{
				Settings.Destinations.Clear();
				Settings.AddDestinationFromConnectionString(destination);
				try
				{
					throw new DummyArgumentException();
				}
				catch (Exception exception)
				{
					Trace.WriteLine("Uploading report using HTTP with connection string: " + destination);
					new BugReport().Report(exception, ExceptionThread.Main);
					Assert.Equal(1, Storer.GetReportCount());
					new Dispatcher(false); // Dispatch async = false;
					Assert.Equal(0, Storer.GetReportCount());
				}
			}

			Trace.WriteLine("HTTP uploads: " + destinations.Count);
		}

		[Fact]
		public void MixedDestinations()
		{
			Settings.AddDestinationFromConnectionString(this.settings.ReadCustomDispatcherDestinationSettings(typeof(Mail).Name)[0]);
			Settings.AddDestinationFromConnectionString(this.settings.ReadCustomDispatcherDestinationSettings(typeof(Http).Name)[0]);
			Settings.AddDestinationFromConnectionString(this.settings.ReadCustomDispatcherDestinationSettings(typeof(Ftp).Name)[0]);
			Settings.AddDestinationFromConnectionString(this.settings.ReadCustomDispatcherDestinationSettings(typeof(Redmine).Name)[0]);

			// ToDo: Settings.Destination5 = this.settings.ReadCustomDispatcherDestinationSettings(Protocols.Trac)[0];
			try
			{
				throw new DummyArgumentException();
			}
			catch (Exception exception)
			{
				Trace.WriteLine("Submitting bug report to 5 different destinations at once: Mail, HTTP, FTP, Redmine, Trac");
				new BugReport().Report(exception, ExceptionThread.Main);
				Assert.Equal(1, Storer.GetReportCount());
				new Dispatcher(false); // Dispatch async = false;
				Assert.Equal(0, Storer.GetReportCount());
			}
		}

		[Fact]
		public void RedmineDispatcher()
		{
			var destinations = this.settings.ReadCustomDispatcherDestinationSettings(typeof(Redmine).Name);
			foreach (var destination in destinations)
			{
				Settings.Destinations.Clear();
				Settings.AddDestinationFromConnectionString(destination);
				try
				{
					throw new DummyArgumentException();
				}
				catch (Exception exception)
				{
					Trace.WriteLine("Submitting bug report to Redmine issue tracker with connection string: " + destination);
					new BugReport().Report(exception, ExceptionThread.Main);
					Assert.Equal(1, Storer.GetReportCount());
					new Dispatcher(false); // Dispatch async = false;
					Assert.Equal(0, Storer.GetReportCount());
				}
			}

			Trace.WriteLine("Redmine submissions: " + destinations.Count);
		}

		public void SetFixture(SettingsFixture settingsFixture)
		{
			// ToDo: This doesn't work all the time and log still gets written to disk!
			Settings.WriteLogToDisk = false;
			Settings.UIMode = UIMode.None;
			Settings.MiniDumpType = MiniDumpType.None;
			this.settings = settingsFixture;
		}

		public void SetFixture(ReportFixture report)
		{
			report.DeleteGarbageReportFile();
		}
	}
}