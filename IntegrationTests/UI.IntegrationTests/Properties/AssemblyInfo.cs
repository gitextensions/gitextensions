using ApprovalTests.Reporters;
using ApprovalTests.Reporters.ContinuousIntegration;
using ApprovalTests.Reporters.TestFrameworks;
using CommonTestUtils;
using NUnit.Framework;

[assembly: ConfigureJoinableTaskFactory]
[assembly: TestAppSettings]
[assembly: UseReporter(typeof(NUnitReporter), typeof(AppVeyorReporter), typeof(DiffReporter))]

// Don't allow tests to run in parrallel
[assembly: NonParallelizable]
