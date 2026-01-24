using GitExtUtils;

namespace GitExtUtilsTests;

[TestFixture]
public sealed class ArrayExtensionsTests
{
    [Test]
    public void Subsequence()
    {
        int[] nums = [.. Enumerable.Range(0, 10)];

        ClassicAssert.AreEqual(
            new[] { 0, 1, 2, 3 },
            nums.Subsequence(0, 4));
        ClassicAssert.AreEqual(
            new[] { 1, 2, 3, 4 },
            nums.Subsequence(1, 4));

        ClassicAssert.AreEqual(
            nums,
            nums.Subsequence(0, 10));

        ClassicAssert.AreEqual(
            Array.Empty<int>(),
            nums.Subsequence(0, 0));

        ClassicAssert.AreEqual(
            Array.Empty<int>(),
            nums.Subsequence(9, 0));
    }

    [Test]
    public void Append()
    {
        ClassicAssert.AreEqual(
            new[] { 0, 1 },
            new[] { 0 }.Append(1));
        ClassicAssert.AreEqual(
            new[] { 0 },
            Array.Empty<int>().Append(0));
        ClassicAssert.AreEqual(
            new[] { 0, 1, 2 },
            Array.Empty<int>().Append(0).Append(1).Append(2));
    }
}
