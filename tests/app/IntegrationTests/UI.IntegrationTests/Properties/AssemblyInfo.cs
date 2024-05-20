using CommonTestUtils;

[assembly: ConfigureJoinableTaskFactory]
[assembly: TestAppSettings]

// Don't allow tests to run in parrallel
[assembly: NonParallelizable]
[assembly: Category("IntegrationTests")]
