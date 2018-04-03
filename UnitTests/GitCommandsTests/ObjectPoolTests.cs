using System;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class ObjectPoolTests
    {
        [Test]
        public void Intern()
        {
            var pool = new ObjectPool<string>(StringComparer.Ordinal);

            var s1 = "hello";
            var s2 = "HELLO";
            var s3 = "HELLO".ToLower();

            Assert.AreNotSame(s1, s3);

            Assert.AreSame(s1, pool.Intern(s1));
            Assert.AreSame(s2, pool.Intern(s2));
            Assert.AreSame(s1, pool.Intern(s3));
        }
    }
}