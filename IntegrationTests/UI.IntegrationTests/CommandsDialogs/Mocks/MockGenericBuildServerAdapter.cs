using System;
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitExtensions.UITests.CommandsDialogs
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

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Action openSettings, Func<ObjectId, bool>? isCommitInRevisionGrid = null)
        {
        }
    }
}
