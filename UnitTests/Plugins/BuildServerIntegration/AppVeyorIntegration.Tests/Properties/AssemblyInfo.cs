using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.ContinuousIntegration;
using ApprovalTests.Reporters.TestFrameworks;
using CommonTestUtils;

[assembly: NUnit.Framework.Category("UnitTests")]
[assembly: TestAppSettings]
[assembly: UseReporter(typeof(NUnitReporter), typeof(AppVeyorReporter), typeof(DiffReporter))]
[assembly: UseApprovalSubdirectory("ApprovedFiles")]
