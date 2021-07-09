using System.Composition;
using GitCommands.UserRepositoryHistory;
using Microsoft.VisualStudio.Composition;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Shared, PartNotDiscoverable]
    [Export(typeof(IRepositoryDescriptionProvider))]
    internal class MockRepositoryDescriptionProvider : IRepositoryDescriptionProvider
    {
        internal const string ShortName = "gitextension";

        public string Get(string repositoryDir) => ShortName;
    }
}
