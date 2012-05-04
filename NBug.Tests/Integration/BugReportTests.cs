// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BugReportTests.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
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
			Assert.Equal(
				new BugReport().Report(new DummyArgumentException(), ExceptionThread.UI_WinForms), 
				ExecutionFlow.BreakExecution);

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
