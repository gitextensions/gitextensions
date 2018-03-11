// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeveloperUITests.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Functional
{
	using System;

	using NBug.Core.Reporting;
	using NBug.Core.Submission;
	using NBug.Core.UI;
	using NBug.Core.Util;
	using NBug.Core.Util.Exceptions;
	using NBug.Enums;
	using NBug.Tests.Tools.Extensions;
	using NBug.Tests.Tools.Fixtures;
	using NBug.Tests.Tools.Stubs;

	using Xunit;

	public class DeveloperUITests : IUseFixture<UIFixture>, IUseFixture<SettingsFixture>
	{
		[Fact]
		public void ConfigErrorWithInternalConfigurationExceptionViewer()
		{
			Settings.AddDestinationFromConnectionString(
				"Type=Mail;From=postmaster@localhost;FromName=NBug Internal Error Reporter;To=postmaster@localhost;SmtpServer=localhost;");
			new BugReport().Report(new DummyArgumentException(), ExceptionThread.Main);
			new Dispatcher(false);

			Settings.UIMode = UIMode.Minimal;
			Settings.UIProvider = UIProvider.Auto; // This is invalid!
			Assert.Equal(new BugReport().Report(new Exception(), ExceptionThread.Main), ExecutionFlow.ContinueExecution);
		}

		[Fact]
		[UI]
		public void InternalConfigurationExceptionViewer()
		{
			NBugConfigurationException.Create(() => Settings.UIMode, "Testing invalid UIMode configuration exception.");
		}

		[Fact]
		[UI]
		public void InternalLogViewerWithTrace()
		{
			// ToDo: Run it in sync (blocking) mode for testing or otherwise the test runner process will be corrupted
			// InternalLogViewer.LogEntry("Test log entry", LoggerCategory.NBugTrace);
		}

		[Fact]
		[UI]
		public void InternalRuntimeExceptionViewer()
		{
			new NBugRuntimeException("Testing runtime exception.", new DummyArgumentException());
		}

		[Fact]
		[UI]
		public void InternalRuntimeExceptionViewerWithoutInnerException()
		{
			new NBugRuntimeException("Testing runtime exception without inner exception.");
		}

		public void SetFixture(UIFixture data)
		{
		}

		public void SetFixture(SettingsFixture data)
		{
		}
	}
}