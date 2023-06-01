using CommonTestUtils;

[assembly: ConfigureJoinableTaskFactory]
[assembly: TestAppSettings]

// Don't allow tests to run in parrallel
[assembly: NUnit.Framework.NonParallelizable]
[assembly: NUnit.Framework.Category("IntegrationTests")]
