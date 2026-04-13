using GitExtUtils;

namespace GitExtUtilsTests;
public sealed class MruCacheTests
{
    [Test]
    public void Add_expires_last_used_entry_when_at_capacity()
    {
        MruCache<int, string> cache = new(capacity: 3);

        cache.Add(1, "one");
        cache.Add(2, "two");
        cache.Add(3, "three");

        cache.TryGetValue(1, out _).Should().BeTrue();
        cache.TryGetValue(2, out _).Should().BeTrue();
        cache.TryGetValue(3, out _).Should().BeTrue();

        cache.Add(4, "four");

        cache.TryGetValue(1, out _).Should().BeFalse();
        cache.TryGetValue(2, out _).Should().BeTrue();
        cache.TryGetValue(3, out _).Should().BeTrue();
        cache.TryGetValue(4, out _).Should().BeTrue();
    }

    [Test]
    public void TryGetValue_renews_lifespan_of_entry()
    {
        MruCache<int, string> cache = new(capacity: 3);

        cache.Add(1, "one");
        cache.Add(2, "two");
        cache.Add(3, "three");

        cache.TryGetValue(3, out _).Should().BeTrue();
        cache.TryGetValue(2, out _).Should().BeTrue();
        cache.TryGetValue(1, out _).Should().BeTrue();

        cache.Add(4, "four");

        cache.TryGetValue(1, out _).Should().BeTrue();
        cache.TryGetValue(2, out _).Should().BeTrue();
        cache.TryGetValue(3, out _).Should().BeFalse();
        cache.TryGetValue(4, out _).Should().BeTrue();
    }

    [Test]
    public void Clear_removes_all_entries()
    {
        MruCache<int, string> cache = new(capacity: 3);

        cache.Add(1, "one");
        cache.Add(2, "two");
        cache.Add(3, "three");

        cache.Clear();

        cache.TryGetValue(1, out _).Should().BeFalse();
        cache.TryGetValue(2, out _).Should().BeFalse();
        cache.TryGetValue(3, out _).Should().BeFalse();
    }

    [Test]
    public void TryRemove_removes_existing_entries()
    {
        MruCache<int, string> cache = new(capacity: 3);

        cache.Add(1, "one");
        cache.Add(2, "two");
        cache.Add(3, "three");

        cache.TryRemove(1, out string? removed).Should().BeTrue();
        removed.Should().Be("one");
        cache.TryGetValue(1, out _).Should().BeFalse();

        cache.TryGetValue(2, out _).Should().BeTrue();
        cache.TryGetValue(3, out _).Should().BeTrue();
    }

    [Test]
    public void TryRemove_returns_false_for_unknown_entry()
    {
        MruCache<int, string> cache = new(capacity: 3);

        cache.Add(1, "one");

        cache.TryRemove(2, out string? removed).Should().BeFalse();
        removed.Should().BeNull();
    }
}
