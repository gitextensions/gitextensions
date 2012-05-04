// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsTests.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Integration
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;

	using NBug.Core.Reporting;
	using NBug.Core.UI;
	using NBug.Core.Util;
	using NBug.Core.Util.Logging;
	using NBug.Core.Util.Storage;
	using NBug.Enums;
	using NBug.Tests.Tools.Fixtures;
	using NBug.Tests.Tools.Stubs;

	using Xunit;

	using StoragePath = NBug.Enums.StoragePath;

	public class SettingsTests : IUseFixture<SettingsFixture>, IUseFixture<ReportFixture>, IDisposable
	{
		private ReportFixture reportFixture;

		public void Dispose()
		{
			this.reportFixture.DeleteGarbageReportFile();
		}

		public void SetFixture(SettingsFixture settings)
		{
			settings.ReloadDefaults();
			settings.InitializeStandardSettings();
		}

		public void SetFixture(ReportFixture reportFix)
		{
			reportFix.DeleteGarbageReportFile();
			this.reportFixture = reportFix;
		}

		[Fact]
		public void ExitApplicationImmediatelyFalse()
		{
			// This is only valid with UIMode = none;
			Settings.ExitApplicationImmediately = false;
			Settings.UIMode = UIMode.None;
			Assert.Equal(new BugReport().Report(new Exception(), ExceptionThread.Main), ExecutionFlow.ContinueExecution);
		}

		[Fact]
		public void ExitApplicationImmediatelyTrue()
		{
			// This is only valid with UIMode = none;
			Settings.ExitApplicationImmediately = true;
			Settings.UIMode = UIMode.None;
			Assert.Equal(new BugReport().Report(new Exception(), ExceptionThread.Main), ExecutionFlow.BreakExecution);
		}

		[Fact]
		public void HandleProcessCorruptedStateExceptionsFalse()
		{
			// ToDo: Find a way to harmlessly simulate access violation exceptions
			/*Settings.ExitApplicationImmediately = false;
			Settings.HandleProcessCorruptedStateExceptions = false;
			Handler.UnhandledException(this, new UnhandledExceptionEventArgs(new AccessViolationException("Testing a corrupt ConsoleApp main thread AccessViolationException."), true));
			Assert.Equal(Storer.GetReportCount(), 0);*/
		}

		[Fact]
		public void HandleProcessCorruptedStateExceptionsTrue()
		{
			Settings.ExitApplicationImmediately = false;
			Settings.HandleProcessCorruptedStateExceptions = true;
			Handler.UnhandledException(this, new UnhandledExceptionEventArgs(new AccessViolationException("Testing a corrupt ConsoleApp main thread AccessViolationException."), true));
			this.reportFixture.VerifyAndDeleteCompressedReportFile();
		}

		[Fact]
		public void InternalLogWritten()
		{
			var message = string.Empty;
			var category = LoggerCategory.NBugTrace;
			Settings.InternalLogWritten += (m, c) => { message = m; category = c; };
			Logger.Info("Testing logger info message");
			Assert.Equal(message, "Testing logger info message");
			Assert.Equal(category, LoggerCategory.NBugInfo);
		}

		[Fact]
		public void ProcessingException()
		{
			Settings.ProcessingException += (e, r) => { r.CustomInfo = "Some custom info string"; };
			new BugReport().Report(new DummyArgumentException(), ExceptionThread.Main);
			this.reportFixture.VerifyAndDeleteCompressedReportFile(true);

			var list = new List<string> { "Some custom info string", "Some more info" };
			Settings.ProcessingException += (e, r) => { r.CustomInfo = list; };
			new BugReport().Report(new DummyArgumentException(), ExceptionThread.Main);
			this.reportFixture.VerifyAndDeleteCompressedReportFile(true);

			Settings.ProcessingException += (e, r) => { r.CustomInfo = new DummySerializableException(); };
			new BugReport().Report(new DummyArgumentException(), ExceptionThread.Main);
			this.reportFixture.VerifyAndDeleteCompressedReportFile(true);
		}

		[Fact]
		public void ProcessingExceptionNonSerializableCustomInfo()
		{
			Settings.ThrowExceptions = false;
			Settings.DisplayDeveloperUI = false;

			// Should not throw even if the custom info cannot be serialized
			Settings.ProcessingException += (e, r) => { r.CustomInfo = new Exception("Just testing"); };
			new BugReport().Report(new DummyArgumentException(), ExceptionThread.Main);
			this.reportFixture.VerifyAndDeleteCompressedReportFile();
		}

		[Fact]
		public void MaxQueuedReports()
		{
			Settings.MaxQueuedReports = 2;
			new BugReport().Report(new Exception(), ExceptionThread.Main);
			new BugReport().Report(new Exception(), ExceptionThread.Main);
			new BugReport().Report(new Exception(), ExceptionThread.Main);

			Assert.Equal(2, Storer.GetReportCount());
			Storer.TruncateReportFiles(1);
			Assert.Equal(1, Storer.GetReportCount());
			Storer.TruncateReportFiles(0);
			Assert.Equal(0, Storer.GetReportCount());
		}

		[Fact]
		public void MiniDumpTypeFull()
		{
			Settings.MiniDumpType = MiniDumpType.Full;
			new BugReport().Report(new Exception(), ExceptionThread.Main);
			Assert.Equal(Storer.GetReportCount(), 1);

			using (var storer = new Storer())
			using (var stream = storer.GetFirstReportFile())
			{
				Assert.NotNull(stream);
				Assert.InRange(stream.Length, 5 * 1024 * 1024, 150 * 1024 * 1024);
				storer.DeleteCurrentReportFile();
			}

			Assert.Equal(Storer.GetReportCount(), 0);
		}

		[Fact]
		public void MiniDumpTypeNone()
		{
			Settings.MiniDumpType = MiniDumpType.None;
			new BugReport().Report(new Exception(), ExceptionThread.Main);
			Assert.Equal(Storer.GetReportCount(), 1);

			using (var storer = new Storer())
			using (var stream = storer.GetFirstReportFile())
			{
				Assert.NotNull(stream);
				Assert.InRange(stream.Length, 0.5 * 1024, 30 * 1024);
				storer.DeleteCurrentReportFile();
			}

			Assert.Equal(Storer.GetReportCount(), 0);
		}

		[Fact]
		public void MiniDumpTypeNormal()
		{
			Settings.MiniDumpType = MiniDumpType.Normal;
			new BugReport().Report(new Exception(), ExceptionThread.Main);
			Assert.Equal(Storer.GetReportCount(), 1);

			using (var storer = new Storer())
			using (var stream = storer.GetFirstReportFile())
			{
				Assert.NotNull(stream);
				Assert.InRange(stream.Length, 100 * 1024, 25 * 1024 * 1024);
				storer.DeleteCurrentReportFile();
			}

			Assert.Equal(Storer.GetReportCount(), 0);
		}

		[Fact]
		public void MiniDumpTypeTiny()
		{
			Settings.MiniDumpType = MiniDumpType.Tiny;
			new BugReport().Report(new Exception(), ExceptionThread.Main);
			Assert.Equal(Storer.GetReportCount(), 1);

			using (var storer = new Storer())
			using (var stream = storer.GetFirstReportFile())
			{
				Assert.NotNull(stream);
				Assert.InRange(stream.Length, 10 * 1024, 1 * 1024 * 1024);
				storer.DeleteCurrentReportFile();
			}

			Assert.Equal(Storer.GetReportCount(), 0);
		}

		[Fact]
		public void NBugDirectory()
		{
			Assert.Equal(Settings.NBugDirectory, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
		}

		[Fact]
		public void StopReportingAfterDaysExpired()
		{
			Settings.StopReportingAfter = 0;
			new BugReport().Report(new Exception(), ExceptionThread.Main);
			Assert.Equal(0, Storer.GetReportCount());
		}

		[Fact]
		public void StopReportingAfterDaysValid()
		{
			Settings.StopReportingAfter = 99;
			new BugReport().Report(new Exception(), ExceptionThread.Main);
			Assert.Equal(1, Storer.GetReportCount());
			Storer.TruncateReportFiles(0);
		}

		[Fact]
		public void StoragePathCurrentDirectory()
		{
			Settings.StoragePath = StoragePath.CurrentDirectory;
			new BugReport().Report(new Exception(), ExceptionThread.Main);
			Assert.Equal(Storer.GetReportCount(), 1);

			using (var storer = new Storer())
			using (var stream = storer.GetFirstReportFile())
			{
				Assert.NotNull(stream);
				Assert.True(storer.FilePath.Contains(Path.GetDirectoryName(Settings.EntryAssembly.Location)));
				storer.DeleteCurrentReportFile();
			}

			Assert.Equal(Storer.GetReportCount(), 0);
		}

		[Fact]
		public void StoragePathCustom()
		{
			Settings.StoragePath = "C:\\";
			new BugReport().Report(new Exception(), ExceptionThread.Main);
			Assert.Equal(Storer.GetReportCount(), 1);

			using (var storer = new Storer())
			using (var stream = storer.GetFirstReportFile())
			{
				Assert.NotNull(stream);
				Assert.True(storer.FilePath.Contains("C:\\"));
				storer.DeleteCurrentReportFile();
			}

			Assert.Equal(Storer.GetReportCount(), 0);
		}

		[Fact]
		public void StoragePathIsolatedStorage()
		{
			Settings.StoragePath = StoragePath.IsolatedStorage;
			new BugReport().Report(new Exception(), ExceptionThread.Main);
			Assert.Equal(Storer.GetReportCount(), 1);

			using (var storer = new Storer())
			using (var stream = storer.GetFirstReportFile())
			{
				Assert.NotNull(stream);
				var filePath = stream.GetType().GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(stream).ToString();
				Assert.True(filePath.Contains("IsolatedStorage"));
				storer.DeleteCurrentReportFile();
			}

			Assert.Equal(Storer.GetReportCount(), 0);
		}

		[Fact]
		public void StoragePathWindowsTemp()
		{
			Settings.StoragePath = StoragePath.WindowsTemp;
			new BugReport().Report(new Exception(), ExceptionThread.Main);
			Assert.Equal(Storer.GetReportCount(), 1);

			using (var storer = new Storer())
			using (var stream = storer.GetFirstReportFile())
			{
				Assert.NotNull(stream);
				Assert.True(storer.FilePath.Contains(Path.Combine(new[] { Path.GetTempPath(), Settings.EntryAssembly.GetName().Name })));
				storer.DeleteCurrentReportFile();
			}

			Assert.Equal(Storer.GetReportCount(), 0);
		}

		[Fact]
		public void WriteLogToDiskFalse()
		{
			Settings.WriteLogToDisk = false;
			File.Delete(Path.Combine(Settings.NBugDirectory, "NBug.log"));
			Logger.Info("Testing log file entry.");
			Assert.False(File.Exists(Path.Combine(Settings.NBugDirectory, "NBug.log")));
			File.Delete(Path.Combine(Settings.NBugDirectory, "NBug.log"));
		}

		[Fact]
		public void WriteLogToDiskTrue()
		{
			if (!Settings.WriteLogToDisk)
			{
				// Bug: This is currently just a workaround
				Settings.WriteLogToDisk = true;
				File.Delete(Path.Combine(Settings.NBugDirectory, "NBug.log"));
				Logger.Info("Testing log file entry.");
				Assert.True(File.Exists(Path.Combine(Settings.NBugDirectory, "NBug.log")));
				File.Delete(Path.Combine(Settings.NBugDirectory, "NBug.log"));
			}
		}
	}
}
