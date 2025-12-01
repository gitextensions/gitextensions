using CommonTestUtils;

[assembly: Epilogue]
[assembly: ConfigureJoinableTaskFactory]
[assembly: TestAppSettings]

// Don't allow tests to run in parallel
[assembly: NonParallelizable]
[assembly: Category("IntegrationTests")]
