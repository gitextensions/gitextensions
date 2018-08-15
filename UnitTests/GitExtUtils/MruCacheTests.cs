using GitExtUtils;
using NUnit.Framework;

namespace GitExtUtilsTests
{
    [TestFixture]
    public sealed class MruCacheTests
    {
        [Test]
        public void Add_expires_last_used_entry_when_at_capacity()
        {
            var cache = new MruCache<int, string>(capacity: 3);

            cache.Add(1, "one");
            cache.Add(2, "two");
            cache.Add(3, "three");

            Assert.True(cache.TryGetValue(1, out _));
            Assert.True(cache.TryGetValue(2, out _));
            Assert.True(cache.TryGetValue(3, out _));

            cache.Add(4, "four");

            Assert.False(cache.TryGetValue(1, out _));
            Assert.True(cache.TryGetValue(2, out _));
            Assert.True(cache.TryGetValue(3, out _));
            Assert.True(cache.TryGetValue(4, out _));
        }

        [Test]
        public void TryGetValue_renews_lifespan_of_entry()
        {
            var cache = new MruCache<int, string>(capacity: 3);

            cache.Add(1, "one");
            cache.Add(2, "two");
            cache.Add(3, "three");

            Assert.True(cache.TryGetValue(3, out _));
            Assert.True(cache.TryGetValue(2, out _));
            Assert.True(cache.TryGetValue(1, out _));

            cache.Add(4, "four");

            Assert.True(cache.TryGetValue(1, out _));
            Assert.True(cache.TryGetValue(2, out _));
            Assert.False(cache.TryGetValue(3, out _));
            Assert.True(cache.TryGetValue(4, out _));
        }

        [Test]
        public void Clear_removes_all_entries()
        {
            var cache = new MruCache<int, string>(capacity: 3);

            cache.Add(1, "one");
            cache.Add(2, "two");
            cache.Add(3, "three");

            cache.Clear();

            Assert.False(cache.TryGetValue(1, out _));
            Assert.False(cache.TryGetValue(2, out _));
            Assert.False(cache.TryGetValue(3, out _));
        }

        [Test]
        public void TryRemove_removes_existing_entries()
        {
            var cache = new MruCache<int, string>(capacity: 3);

            cache.Add(1, "one");
            cache.Add(2, "two");
            cache.Add(3, "three");

            Assert.True(cache.TryRemove(1, out var removed));
            Assert.AreEqual("one", removed);
            Assert.False(cache.TryGetValue(1, out _));

            Assert.True(cache.TryGetValue(2, out _));
            Assert.True(cache.TryGetValue(3, out _));
        }

        [Test]
        public void TryRemove_returns_false_for_unknown_entry()
        {
            var cache = new MruCache<int, string>(capacity: 3);

            cache.Add(1, "one");

            Assert.False(cache.TryRemove(2, out var removed));
            Assert.Null(removed);
        }
    }
}