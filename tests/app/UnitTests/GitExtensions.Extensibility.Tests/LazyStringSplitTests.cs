using CommonTestUtils;
using GitExtensions.Extensibility;

namespace GitExtUtilsTests;
public sealed class LazyStringSplitTests
{
    [TestCase("a;b;c", ';', new[] { "a", "b", "c" })]
    [TestCase("a_b_c", '_', new[] { "a", "b", "c" })]
    [TestCase("aa;bb;cc", ';', new[] { "aa", "bb", "cc" })]
    [TestCase("aaa;bbb;ccc", ';', new[] { "aaa", "bbb", "ccc" })]
    [TestCase(";a", ';', new[] { "", "a" })]
    [TestCase("a;", ';', new[] { "a", "" })]
    [TestCase(";a;b;c", ';', new[] { "", "a", "b", "c" })]
    [TestCase("a;b;c;", ';', new[] { "a", "b", "c", "" })]
    [TestCase(";a;b;c;", ';', new[] { "", "a", "b", "c", "" })]
    [TestCase(";;a;;b;;c;;", ';', new[] { "", "", "a", "", "b", "", "c", "", "" })]
    [TestCase("", ';', new[] { "" })]
    [TestCase(";", ';', new[] { "", "" })]
    [TestCase(";;", ';', new[] { "", "", "" })]
    [TestCase(";;;", ';', new[] { "", "", "", "" })]
    [TestCase(";;;a", ';', new[] { "", "", "", "a" })]
    [TestCase("a;;;", ';', new[] { "a", "", "", "" })]
    [TestCase(";a;;", ';', new[] { "", "a", "", "" })]
    [TestCase(";;a;", ';', new[] { "", "", "a", "" })]
    [TestCase("a", ';', new[] { "a" })]
    [TestCase("aa", ';', new[] { "aa" })]
    public void None(string input, char delimiter, string[] expected)
    {
        // This boxes
        IEnumerable<string> actual = new LazyStringSplit(input, delimiter, StringSplitOptions.None);

        actual.Should().Equal(expected);

        // Non boxing foreach
        List<string> list = [.. new LazyStringSplit(input, delimiter, StringSplitOptions.None)];

        list.Should().Equal(expected);

        // Equivalence with string.Split
        input.Split([delimiter], StringSplitOptions.None).Should().Equal(expected);
    }

    [TestCase("a;b;c", ';', new[] { "a", "b", "c" })]
    [TestCase("a_b_c", '_', new[] { "a", "b", "c" })]
    [TestCase("aa;bb;cc", ';', new[] { "aa", "bb", "cc" })]
    [TestCase("aaa;bbb;ccc", ';', new[] { "aaa", "bbb", "ccc" })]
    [TestCase(";a", ';', new[] { "a" })]
    [TestCase("a;", ';', new[] { "a" })]
    [TestCase(";a;b;c", ';', new[] { "a", "b", "c" })]
    [TestCase("a;b;c;", ';', new[] { "a", "b", "c" })]
    [TestCase(";a;b;c;", ';', new[] { "a", "b", "c" })]
    [TestCase(";;a;;b;;c;;", ';', new[] { "a", "b", "c" })]
    [TestCase("", ';', new string[0])]
    [TestCase(";", ';', new string[0])]
    [TestCase(";;", ';', new string[0])]
    [TestCase(";;;", ';', new string[0])]
    [TestCase(";;;a", ';', new[] { "a" })]
    [TestCase("a;;;", ';', new[] { "a" })]
    [TestCase(";a;;", ';', new[] { "a" })]
    [TestCase(";;a;", ';', new[] { "a" })]
    [TestCase("a", ';', new[] { "a" })]
    [TestCase("aa", ';', new[] { "aa" })]
    public void RemoveEmptyEntries(string input, char delimiter, string[] expected)
    {
        // This boxes
        IEnumerable<string> actual = new LazyStringSplit(input, delimiter, StringSplitOptions.RemoveEmptyEntries);

        actual.Should().Equal(expected);

        // Non boxing foreach
        List<string> list = [.. new LazyStringSplit(input, delimiter, StringSplitOptions.RemoveEmptyEntries)];

        list.Should().Equal(expected);

        // Equivalence with string.Split
        input.Split([delimiter], StringSplitOptions.RemoveEmptyEntries).Should().Equal(expected);
    }

    [Test]
    public void Constructor_WithNullInput_Throws()
    {
        ((Action)(() => _ = new LazyStringSplit(null!, ';'))).Should().Throw<ArgumentNullException>();
    }
}
