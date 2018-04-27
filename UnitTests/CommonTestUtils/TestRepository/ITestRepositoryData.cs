using System;

namespace CommonTestUtils.TestRepository
{
    public interface ITestRepositoryData : IDisposable
    {
        string ContentPath { get; }
    }
}