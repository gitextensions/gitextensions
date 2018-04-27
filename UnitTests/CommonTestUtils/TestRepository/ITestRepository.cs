using System;
using GitCommands;

namespace CommonTestUtils.TestRepository
{
    public interface ITestRepository : IDisposable
    {
        GitModule Module { get; }
    }
}