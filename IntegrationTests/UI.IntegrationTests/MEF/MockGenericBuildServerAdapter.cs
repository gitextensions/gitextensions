﻿using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace UITests.CommandsDialogs.SettingsDialog.Pages
{
    [PartNotDiscoverable]
    [MockGenericBuildServerIntegrationMetadata("GenericBuildServerMock")]
    [Export(typeof(IBuildServerAdapter))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class MockGenericBuildServerAdapter : IBuildServerAdapter
    {
        public string UniqueKey { get; }

        public void Dispose()
        {
        }

        public IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
        {
            throw new NotImplementedException();
        }

        public IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler)
        {
            throw new NotImplementedException();
        }

        public void Initialize(IBuildServerWatcher buildServerWatcher, SettingsSource config, Action openSettings, Func<ObjectId, bool>? isCommitInRevisionGrid = null)
        {
        }
    }
}
