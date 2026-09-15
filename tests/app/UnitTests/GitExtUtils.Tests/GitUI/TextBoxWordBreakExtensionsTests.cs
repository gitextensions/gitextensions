using GitUI;

namespace GitExtUtilsTests.GitUI;
public sealed class TextBoxWordBreakExtensionsTests
{
    // Path-like text: '/' and '.' are boundaries, '-' keeps a word together.
    [TestCase("src/app/Foo-bar.cs", 0, 3)]
    [TestCase("src/app/Foo-bar.cs", 3, 4)]
    [TestCase("src/app/Foo-bar.cs", 4, 7)]
    [TestCase("src/app/Foo-bar.cs", 8, 15)] // jumps over "Foo-bar" as a single word
    [TestCase("src/app/Foo-bar.cs", 16, 18)] // last word -> end of text
    // Consecutive punctuation: each character is its own stop.
    [TestCase("a/.b", 0, 1)]
    [TestCase("a/.b", 1, 2)]
    [TestCase("a/.b", 2, 3)]
    // Whitespace runs are skipped.
    [TestCase("foo  bar", 0, 5)]
    [TestCase("foo  bar", 5, 8)]
    // Parentheses are boundaries.
    [TestCase("(foo)", 0, 1)]
    [TestCase("(foo)", 1, 4)]
    // '_', '+' and '-' are word characters.
    [TestCase("a_b+c-d", 0, 7)]
    // At or past the end of the text.
    [TestCase("abc", 3, 3)]
    [TestCase("", 0, 0)]
    public void FindNextBoundary_returns_start_of_next_word(string text, int position, int expected)
    {
        TextBoxWordBreakExtensions.TestAccessor.FindNextBoundary(text, position).Should().Be(expected);
    }

    [TestCase("src/app/Foo-bar.cs", 18, 16)]
    [TestCase("src/app/Foo-bar.cs", 16, 15)]
    [TestCase("src/app/Foo-bar.cs", 7, 4)]
    [TestCase("src/app/Foo-bar.cs", 3, 0)]
    [TestCase("a/.b", 4, 3)]
    [TestCase("a/.b", 3, 2)]
    [TestCase("a/.b", 1, 0)]
    [TestCase("foo  bar", 8, 5)]
    [TestCase("foo  bar", 5, 0)]
    [TestCase("(foo)", 5, 4)]
    [TestCase("(foo)", 4, 1)]
    [TestCase("(foo)", 1, 0)]
    [TestCase("a_b+c-d", 7, 0)]
    [TestCase("abc", 0, 0)]
    [TestCase("", 0, 0)]
    public void FindPreviousBoundary_returns_start_of_current_or_previous_word(string text, int position, int expected)
    {
        TextBoxWordBreakExtensions.TestAccessor.FindPreviousBoundary(text, position).Should().Be(expected);
    }

    [Test]
    public void FindNextBoundary_clamps_position_past_end_of_text()
    {
        TextBoxWordBreakExtensions.TestAccessor.FindNextBoundary("abc", 999).Should().Be(3);
    }

    [Test]
    public void FindPreviousBoundary_clamps_position_past_end_of_text()
    {
        TextBoxWordBreakExtensions.TestAccessor.FindPreviousBoundary("abc", 999).Should().Be(0);
    }

    // Path-like text. Clicking anywhere in a segment selects just that segment,
    // and "Foo-bar" stays whole because '-' is a word character.
    [TestCase("src/app/Foo-bar.cs", 0, 0, 3)] // "src"
    [TestCase("src/app/Foo-bar.cs", 2, 0, 3)]
    [TestCase("src/app/Foo-bar.cs", 3, 3, 4)] // "/"
    [TestCase("src/app/Foo-bar.cs", 5, 4, 7)] // "app"
    [TestCase("src/app/Foo-bar.cs", 8, 8, 15)] // "Foo-bar"
    [TestCase("src/app/Foo-bar.cs", 11, 8, 15)] // clicking the '-' still selects "Foo-bar"
    [TestCase("src/app/Foo-bar.cs", 15, 15, 16)] // "."
    [TestCase("src/app/Foo-bar.cs", 17, 16, 18)] // "cs"
    // Consecutive punctuation: each character is its own run.
    [TestCase("a/.b", 1, 1, 2)] // "/"
    [TestCase("a/.b", 2, 2, 3)] // "."
    // Whitespace runs select as a unit.
    [TestCase("foo  bar", 1, 0, 3)] // "foo"
    [TestCase("foo  bar", 3, 3, 5)] // "  "
    [TestCase("foo  bar", 6, 5, 8)] // "bar"
    // Out of range returns an empty range at the index.
    [TestCase("abc", 3, 3, 3)]
    [TestCase("abc", -1, -1, -1)]
    [TestCase("", 0, 0, 0)]
    public void GetWordAt_selects_the_run_containing_the_index(string text, int index, int expectedStart, int expectedEnd)
    {
        TextBoxWordBreakExtensions.TestAccessor.GetWordAt(text, index).Should().Be((expectedStart, expectedEnd));
    }
}
