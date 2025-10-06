using GitExtUtils;

namespace GitExtUtilsTests;

[TestFixture]
public sealed class MruCacheTests
{
    [Test]
    public void Add_expires_last_used_entry_when_at_capacity()
    {
        MruCache<int, string> cache = new(capacity: 3);

        cache.Add(1, "one");
        cache.Add(2, "two");
        cache.Add(3, "three");

        ClassicAssert.True(cache.TryGetValue(1, out _));
        ClassicAssert.True(cache.TryGetValue(2, out _));
        ClassicAssert.True(cache.TryGetValue(3, out _));

        cache.Add(4, "four");

        ClassicAssert.False(cache.TryGetValue(1, out _));
        ClassicAssert.True(cache.TryGetValue(2, out _));
        ClassicAssert.True(cache.TryGetValue(3, out _));
        ClassicAssert.True(cache.TryGetValue(4, out _));
    }

    [Test]
    public void TryGetValue_renews_lifespan_of_entry()
    {
        MruCache<int, string> cache = new(capacity: 3);

        cache.Add(1, "one");
        cache.Add(2, "two");
        cache.Add(3, "three");

        ClassicAssert.True(cache.TryGetValue(3, out _));
        ClassicAssert.True(cache.TryGetValue(2, out _));
        ClassicAssert.True(cache.TryGetValue(1, out _));

        cache.Add(4, "four");

        ClassicAssert.True(cache.TryGetValue(1, out _));
        ClassicAssert.True(cache.TryGetValue(2, out _));
        ClassicAssert.False(cache.TryGetValue(3, out _));
        ClassicAssert.True(cache.TryGetValue(4, out _));
    }

    [Test]
    public void Clear_removes_all_entries()
    {
        MruCache<int, string> cache = new(capacity: 3);

        cache.Add(1, "one");
        cache.Add(2, "two");
        cache.Add(3, "three");

        cache.Clear();

        ClassicAssert.False(cache.TryGetValue(1, out _));
        ClassicAssert.False(cache.TryGetValue(2, out _));
        ClassicAssert.False(cache.TryGetValue(3, out _));
    }

    [Test]
    public void TryRemove_removes_existing_entries()
    {
        MruCache<int, string> cache = new(capacity: 3);

        cache.Add(1, "one");
        cache.Add(2, "two");
        cache.Add(3, "three");

        ClassicAssert.True(cache.TryRemove(1, out string? removed));
        ClassicAssert.AreEqual("one", removed);
        ClassicAssert.False(cache.TryGetValue(1, out _));

        ClassicAssert.True(cache.TryGetValue(2, out _));
        ClassicAssert.True(cache.TryGetValue(3, out _));
    }

    [Test]
    public void TryRemove_returns_false_for_unknown_entry()
    {
        MruCache<int, string> cache = new(capacity: 3);

        cache.Add(1, "one");

        ClassicAssert.False(cache.TryRemove(2, out string? removed));
        ClassicAssert.Null(removed);
    }
}
