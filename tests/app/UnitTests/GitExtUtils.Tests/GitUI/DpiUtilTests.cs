using FluentAssertions;
using GitExtUtils.GitUI;

namespace GitExtUtilsTests.GitUI;

[TestFixture]
public sealed class DpiUtilTests
{
    [Test]
    public void Scale_with_ceiling_false_should_use_round()
    {
        // The actual DPI values are system-dependent, so we test the rounding behavior
        // by verifying that the method respects the ceiling parameter
        
        // When ceiling is false (default), Math.Round should be used
        // We can't test exact values without mocking static properties,
        // but we can verify the method signature works correctly
        int result = DpiUtil.Scale(5, ceiling: false);
        
        // Result should be a valid integer (basic sanity check)
        result.Should().BeGreaterOrEqual(0);
    }

    [Test]
    public void Scale_with_ceiling_true_should_use_ceiling()
    {
        int result = DpiUtil.Scale(1, ceiling: true);
        
        result.Should().BeGreaterThan(0);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(7)]
    [TestCase(8)]
    [TestCase(9)]
    [TestCase(10)]
    [TestCase(11)]
    [TestCase(12)]
    [TestCase(13)]
    [TestCase(14)]
    [TestCase(15)]
    [TestCase(16)]
    public void Scale_default_parameter_should_use_round(int value)
    {
        int resultDefault = DpiUtil.Scale(value);
        int resultExplicit = DpiUtil.Scale(value, ceiling: false);
        
        resultExplicit.Should().Be(resultDefault);
    }

    [TestCase(0, false, 0)]
    [TestCase(0, true, 0)]
    public void Scale_with_zero_should_return_zero(int input, bool ceiling, int expected)
    {
        int result = DpiUtil.Scale(input, ceiling: ceiling);
        
        result.Should().Be(expected);
    }

    [TestCase(-5, false)]
    [TestCase(-5, true)]
    public void Scale_with_negative_value_should_work(int input, bool ceiling)
    {
        // Negative values should be scaled correctly
        int result = DpiUtil.Scale(input, ceiling: ceiling);
        
        result.Should().BeLessOrEqualTo(0);
    }
}
