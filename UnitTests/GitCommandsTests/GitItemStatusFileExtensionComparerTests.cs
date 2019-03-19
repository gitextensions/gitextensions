using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitCommands;
using GitCommands.Git;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class GitItemStatusFileExtensionComparerTests
    {
        private GitItemStatusFileExtensionComparer _comparerUnderTest;

        [SetUp]
        public void Setup()
        {
            _comparerUnderTest = new GitItemStatusFileExtensionComparer();
        }

        [Test]
        public void Compares_file_extension_then_normal_item_comparison()
        {
            var unsortedItems = new[]
            {
                new GitItemStatus() { Name = null, OldName = null }, // I don't think this is possible but a good test case nonetheless.
                new GitItemStatus() { Name = null, OldName = "src/deletedFile.txt" },
                new GitItemStatus() { Name = "src/newFile.txt", OldName = null },
                new GitItemStatus() { Name = "src/newFile.cs", OldName = null },
                new GitItemStatus() { Name = "src/Alice.cs", OldName = null },
                new GitItemStatus() { Name = "src/newName.cs", OldName = "src/oldName.cs" },
                new GitItemStatus() { Name = "newFile.txt", OldName = null },
                new GitItemStatus() { Name = "newName.cs", OldName = "oldName.cs" },
                new GitItemStatus() { Name = "changeExtension.cs", OldName = "changeExtension.txt" },
            };

            var copy = unsortedItems.ToList();
            copy.Sort(_comparerUnderTest);
            var sorted = copy;

            CollectionAssert.AreEqual(
                new GitItemStatus[]
                {
                    // I would prefer this first null entry be last but it is the result of typical one side is null comparisons.
                    new GitItemStatus() { Name = null, OldName = null },
                    new GitItemStatus() { Name = "changeExtension.cs", OldName = "changeExtension.txt" },
                    new GitItemStatus() { Name = "newName.cs", OldName = "oldName.cs" },
                    new GitItemStatus() { Name = "src/Alice.cs", OldName = null },
                    new GitItemStatus() { Name = "src/newFile.cs", OldName = null },
                    new GitItemStatus() { Name = "src/newName.cs", OldName = "src/oldName.cs" },
                    new GitItemStatus() { Name = "newFile.txt", OldName = null },
                    new GitItemStatus() { Name = null, OldName = "src/deletedFile.txt" },
                    new GitItemStatus() { Name = "src/newFile.txt", OldName = null },
                },
                sorted,
                new GitItemStatusCollectionEqualityComparer());
        }

        [Test]
        public void Nulls_are_handled()
        {
            Assert.AreEqual(1, _comparerUnderTest.Compare(new GitItemStatus(), null));
            Assert.AreEqual(0, _comparerUnderTest.Compare(null, null));
            Assert.AreEqual(-1, _comparerUnderTest.Compare(null, new GitCommands.GitItemStatus()));
        }

        /// <summary>
        /// This is a pure service for the NUnit CollectionAssert function call above.
        /// Not sure why it takes <see cref="IComparer"/> over <see cref="IEqualityComparer"/>.
        /// This is not a typical comparer that can be used for sorting. We return 0 for equal
        /// and 1 for not equal.
        /// </summary>
        private class GitItemStatusCollectionEqualityComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x == null || y == null)
                {
                    return 1;
                }

                var lhs = x as GitItemStatus;
                var rhs = y as GitItemStatus;

                return lhs.Name == rhs.Name && lhs.OldName == rhs.OldName ? 0 : 1;
            }
        }
    }
}
