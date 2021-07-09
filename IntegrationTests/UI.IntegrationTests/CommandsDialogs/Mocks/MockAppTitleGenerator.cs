using System.Composition;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using Microsoft.VisualStudio.Composition;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Shared, PartNotDiscoverable]
    [Export(typeof(IAppTitleGenerator))]
    internal class MockAppTitleGenerator : IAppTitleGenerator
    {
        public string Generate(string? workingDir = null, bool isValidWorkingDir = false, string? branchName = null) => "Mock title";
    }
}
