// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HandlerTests.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Integration
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	using NBug.Tests.Tools.Fixtures;

	using Xunit;

	public class HandlerTests : IUseFixture<SettingsFixture>, IUseFixture<ReportFixture>
	{
		private ReportFixture report;

		[Fact]
		public void CorruptThreadExceptionHandler()
		{
			Settings.HandleProcessCorruptedStateExceptions = true;
			Handler.ThreadException(this, new ThreadExceptionEventArgs(new Exception("Testing a corrupt WinForms UI thread Exception.")));
			this.report.VerifyAndDeleteCompressedReportFile();
		}

		[Fact]
		public void CorruptUnhandledExceptionHandler()
		{
			Settings.HandleProcessCorruptedStateExceptions = true;

			// Since there are no UI related assemblies loaded, this should behave as a console app exception
			Handler.UnhandledException(this, new UnhandledExceptionEventArgs(new AccessViolationException("Testing a corrupt ConsoleApp main thread AccessViolationException."), true));
			this.report.VerifyAndDeleteCompressedReportFile();
		}

		[Fact]
		public void CorruptUnobservedTaskExceptionHandler()
		{
			Settings.HandleProcessCorruptedStateExceptions = true;
			Handler.UnobservedTaskException(this, new UnobservedTaskExceptionEventArgs(new AggregateException("Testing a corrupt Task exception.", new Exception("Task exception inner exception as aggregated exception."))));
			this.report.VerifyAndDeleteCompressedReportFile();
		}

		public void SetFixture(SettingsFixture settings)
		{
			settings.InitializeStandardSettings();

			// This is requred otherwise the test runner will be forced to quit before test finishes
			Settings.ExitApplicationImmediately = false;
		}

		public void SetFixture(ReportFixture reportFixture)
		{
			this.report = reportFixture;
		}

		[Fact]
		public void ThreadExceptionHandler()
		{
			Handler.ThreadException(this, new ThreadExceptionEventArgs(new Exception("Testing a WinForms UI thread Exception.")));
			this.report.VerifyAndDeleteCompressedReportFile();
		}

		[Fact] // Simulate System.AppDomain.UnhandledException event on the main thread in the default appdomain
		public void UnhandledExceptionHandler()
		{
			// Since there are no UI related assemblies loaded, this should behave as a console app exception
			Handler.UnhandledException(this, new UnhandledExceptionEventArgs(new Exception("Testing a ConsoleApp main thread Exception."), true));
			this.report.VerifyAndDeleteCompressedReportFile();
		}

		[Fact]
		public void UnobservedTaskExceptionHandler()
		{
			Handler.UnobservedTaskException(this, new UnobservedTaskExceptionEventArgs(new AggregateException("Testing a Task exception.", new Exception("Task exception inner exception as aggregated exception."))));
			this.report.VerifyAndDeleteCompressedReportFile();
		}
	}
}
