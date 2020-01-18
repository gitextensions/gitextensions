using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace GitUITests
{
    [TestFixture]
    public sealed class LinqExtensionsTests
    {
        [Test]
        public void AsReadOnlyList_returns_singleton_empty()
        {
            Assert.AreSame(Array.Empty<int>(), new HashSet<int>().AsReadOnlyList());
            Assert.AreSame(Array.Empty<int>(), new Dictionary<int, int>().Values.AsReadOnlyList());
        }

        [Test]
        public void AsReadOnlyList_copies_to_new_list_if_required()
        {
            var set = new HashSet<int> { 1, 2, 3 };

            Assert.AreEqual(new[] { 1, 2, 3 }, set.AsReadOnlyList());
        }

        [Test]
        public void AsReadOnlyList_returns_object_unchanged_when_has_required_interface()
        {
            Test(new int[0]);
            Test(new int[1]);
            Test(new List<int>());
            Test(new List<int> { 1, 2, 3 });

            void Test(IEnumerable<int> e)
            {
                if (e is IReadOnlyList<int>)
                {
                    Assert.AreSame(e, e.AsReadOnlyList());
                }
                else
                {
                    Assert.AreNotSame(e, e.AsReadOnlyList());
                }
            }
        }

        [Test]
        public void IndexOf()
        {
            var ints = new[] { 0, 1, 2, 3 };

            Assert.AreEqual(0, ints.IndexOf(i => i == 0));
            Assert.AreEqual(1, ints.IndexOf(i => i == 1));
            Assert.AreEqual(2, ints.IndexOf(i => i == 2));
            Assert.AreEqual(3, ints.IndexOf(i => i == 3));
            Assert.AreEqual(-1, ints.IndexOf(i => i == 4));
        }
    }
}