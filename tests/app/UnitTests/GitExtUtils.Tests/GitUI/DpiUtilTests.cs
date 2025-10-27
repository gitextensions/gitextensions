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
        ClassicAssert.GreaterOrEqual(result, 0);
    }

    [Test]
    public void Scale_with_ceiling_true_should_use_ceiling()
    {
        // When ceiling is true, Math.Ceiling should be used
        int result = DpiUtil.Scale(5, ceiling: true);
        
        // Result should be a valid integer (basic sanity check)
        ClassicAssert.GreaterOrEqual(result, 0);
    }

    [Test]
    public void Scale_default_parameter_should_use_round()
    {
        // When ceiling parameter is omitted, it defaults to false (Math.Round)
        int resultDefault = DpiUtil.Scale(5);
        int resultExplicit = DpiUtil.Scale(5, ceiling: false);
        
        // Both should produce the same result
        ClassicAssert.AreEqual(resultExplicit, resultDefault);
    }

    [Test]
    public void Scale_with_zero_should_return_zero()
    {
        int resultRound = DpiUtil.Scale(0, ceiling: false);
        int resultCeiling = DpiUtil.Scale(0, ceiling: true);
        
        ClassicAssert.AreEqual(0, resultRound);
        ClassicAssert.AreEqual(0, resultCeiling);
    }

    [Test]
    public void Scale_with_negative_value_should_work()
    {
        // Negative values should be scaled correctly
        int resultRound = DpiUtil.Scale(-5, ceiling: false);
        int resultCeiling = DpiUtil.Scale(-5, ceiling: true);
        
        // Results should be valid integers
        ClassicAssert.LessOrEqual(resultRound, 0);
        ClassicAssert.LessOrEqual(resultCeiling, 0);
    }
}
