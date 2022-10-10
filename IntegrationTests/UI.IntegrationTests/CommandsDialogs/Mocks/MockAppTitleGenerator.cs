﻿using System.Composition;
using GitCommands;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Shared, PartNotDiscoverable]
    [Export(typeof(IAppTitleGenerator))]
    internal class MockAppTitleGenerator : IAppTitleGenerator
    {
        public string Generate(string? workingDir = null, bool isValidWorkingDir = false, string? branchName = null, string defaultBranchName = "", string? pathName = null) => "Mock title";
    }
}
