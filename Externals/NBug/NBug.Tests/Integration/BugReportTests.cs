// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BugReportTests.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Integration
{
	using NBug.Core.Reporting;
	using NBug.Core.UI;
	using NBug.Core.Util;
	using NBug.Tests.Tools.Fixtures;
	using NBug.Tests.Tools.Stubs;

	using Xunit;

	public class BugReportTests : IUseFixture<SettingsFixture>, IUseFixture<ReportFixture>
	{
		private ReportFixture report;

		[Fact]
		public void GenerateBugReport()
		{
			Assert.Equal(new BugReport().Report(new DummyArgumentException(), ExceptionThread.UI_WinForms), ExecutionFlow.BreakExecution);

			this.report.VerifyAndDeleteCompressedReportFile();
		}

		public void SetFixture(SettingsFixture settings)
		{
			settings.InitializeStandardSettings();
		}

		public void SetFixture(ReportFixture reportFixture)
		{
			this.report = reportFixture;
		}
	}
}