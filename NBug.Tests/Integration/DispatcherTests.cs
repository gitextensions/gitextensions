// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatcherTests.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Integration
{
	using System;
	using System.Diagnostics;

	using NBug.Core.Reporting;
	using NBug.Core.Submission;
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
			var destinations = this.settings.ReadCustomDispatcherDestinationSettings(Protocols.Mail);
			foreach (var destination in destinations)
			{
				Settings.Destination1 = destination;
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
			var destinations = this.settings.ReadCustomDispatcherDestinationSettings(Protocols.FTP);
			foreach (var destination in destinations)
			{
				Settings.Destination1 = destination;
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
			var destinations = this.settings.ReadCustomDispatcherDestinationSettings(Protocols.HTTP);
			foreach (var destination in destinations)
			{
				Settings.Destination1 = destination;
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
			Settings.Destination1 = this.settings.ReadCustomDispatcherDestinationSettings(Protocols.Mail)[0];
			Settings.Destination2 = this.settings.ReadCustomDispatcherDestinationSettings(Protocols.HTTP)[0];
			Settings.Destination3 = this.settings.ReadCustomDispatcherDestinationSettings(Protocols.FTP)[0];
			Settings.Destination4 = this.settings.ReadCustomDispatcherDestinationSettings(Protocols.Redmine)[0];

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
			var destinations = this.settings.ReadCustomDispatcherDestinationSettings(Protocols.Redmine);
			foreach (var destination in destinations)
			{
				Settings.Destination1 = destination;
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
