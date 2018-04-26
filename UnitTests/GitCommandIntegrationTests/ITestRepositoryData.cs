using System;

namespace GitCommandIntegrationTests
{
    internal interface ITestRepositoryData : IDisposable
    {
        string ContentPath { get; }
    }
}