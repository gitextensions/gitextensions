namespace GitUITests;

[TestFixture]
public sealed class LinqExtensionsTests
{
    [Test]
    public void AsReadOnlyList_returns_singleton_empty()
    {
        ClassicAssert.AreSame(Array.Empty<int>(), new HashSet<int>().AsReadOnlyList());
        ClassicAssert.AreSame(Array.Empty<int>(), new Dictionary<int, int>().Values.AsReadOnlyList());
    }

    [Test]
    public void AsReadOnlyList_copies_to_new_list_if_required()
    {
        HashSet<int> set = [1, 2, 3];

        ClassicAssert.AreEqual(new[] { 1, 2, 3 }, set.AsReadOnlyList());
    }

    [Test]
    public void AsReadOnlyList_returns_object_unchanged_when_has_required_interface()
    {
        Test(Array.Empty<int>());
        Test(new int[1]);
        Test(new List<int>());
        Test(new List<int> { 1, 2, 3 });

        void Test(IEnumerable<int> e)
        {
            if (e is IReadOnlyList<int>)
            {
                ClassicAssert.AreSame(e, e.AsReadOnlyList());
            }
            else
            {
                ClassicAssert.AreNotSame(e, e.AsReadOnlyList());
            }
        }
    }

    [Test]
    public void IndexOf()
    {
        int[] ints = new[] { 0, 1, 2, 3 };

        ClassicAssert.AreEqual(0, ints.IndexOf(i => i == 0));
        ClassicAssert.AreEqual(1, ints.IndexOf(i => i == 1));
        ClassicAssert.AreEqual(2, ints.IndexOf(i => i == 2));
        ClassicAssert.AreEqual(3, ints.IndexOf(i => i == 3));
        ClassicAssert.AreEqual(-1, ints.IndexOf(i => i == 4));
    }
}
