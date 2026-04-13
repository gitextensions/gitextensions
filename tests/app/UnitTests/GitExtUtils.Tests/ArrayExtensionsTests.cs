using GitExtUtils;

namespace GitExtUtilsTests;
public sealed class ArrayExtensionsTests
{
    [Test]
    public void Subsequence()
    {
        int[] nums = [.. Enumerable.Range(0, 10)];

        nums.Subsequence(0, 4).Should().Equal(new[] { 0, 1, 2, 3 });
        nums.Subsequence(1, 4).Should().Equal(new[] { 1, 2, 3, 4 });

        nums.Subsequence(0, 10).Should().Equal(nums);

        nums.Subsequence(0, 0).Should().Equal(Array.Empty<int>());

        nums.Subsequence(9, 0).Should().Equal(Array.Empty<int>());
    }

    [Test]
    public void Append()
    {
        new[] { 0 }.Append(1).Should().Equal(new[] { 0, 1 });
        Array.Empty<int>().Append(0).Should().Equal(new[] { 0 });
        Array.Empty<int>().Append(0).Append(1).Append(2).Should().Equal(new[] { 0, 1, 2 });
    }
}
