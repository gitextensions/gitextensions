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

    [TestCase("a;b;c", ';', StringSplitOptions.None, new[] { "a", "b", "c" })]
    [TestCase("a_b_\tc", '_', StringSplitOptions.None, new[] { "a", "b", "\tc" })]
    [TestCase("aa;\rbb;cc", ';', StringSplitOptions.None, new[] { "aa", "\rbb", "cc" })]
    [TestCase("\naaa;bbb;ccc", ';', StringSplitOptions.None, new[] { "\naaa", "bbb", "ccc" })]
    [TestCase(";a", ';', StringSplitOptions.None, new[] { "", "a" })]
    [TestCase("a;", ';', StringSplitOptions.None, new[] { "a", "" })]
    [TestCase(";a;b;c", ';', StringSplitOptions.None, new[] { "", "a", "b", "c" })]
    [TestCase("a;b;c;", ';', StringSplitOptions.None, new[] { "a", "b", "c", "" })]
    [TestCase(";a;b;c;", ';', StringSplitOptions.None, new[] { "", "a", "b", "c", "" })]
    [TestCase(";;a;;b;;c;;", ';', StringSplitOptions.None, new[] { "", "", "a", "", "b", "", "c", "", "" })]
    [TestCase("", ';', StringSplitOptions.None, new[] { "" })]
    [TestCase(";", ';', StringSplitOptions.None, new[] { "", "" })]
    [TestCase(";;", ';', StringSplitOptions.None, new[] { "", "", "" })]
    [TestCase(";;;", ';', StringSplitOptions.None, new[] { "", "", "", "" })]
    [TestCase(";;;a", ';', StringSplitOptions.None, new[] { "", "", "", "a" })]
    [TestCase("a;;;", ';', StringSplitOptions.None, new[] { "a", "", "", "" })]
    [TestCase(";a;;", ';', StringSplitOptions.None, new[] { "", "a", "", "" })]
    [TestCase(";;a;", ';', StringSplitOptions.None, new[] { "", "", "a", "" })]
    [TestCase("a", ';', StringSplitOptions.None, new[] { "a" })]
    [TestCase("aa", ';', StringSplitOptions.None, new[] { "aa" })]
    [TestCase("a;b;c", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "a", "b", "c" })]
    [TestCase("a_b_c", '_', StringSplitOptions.RemoveEmptyEntries, new[] { "a", "b", "c" })]
    [TestCase("aa;bb;cc", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "aa", "bb", "cc" })]
    [TestCase("aaa;bbb;ccc", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "aaa", "bbb", "ccc" })]
    [TestCase(";a", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "a" })]
    [TestCase("a;", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "a" })]
    [TestCase(";a;b;c", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "a", "b", "c" })]
    [TestCase("a;b;c;", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "a", "b", "c" })]
    [TestCase(";a;b;c;", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "a", "b", "c" })]
    [TestCase(";;a;;b;;c;;", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "a", "b", "c" })]
    [TestCase("", ';', StringSplitOptions.RemoveEmptyEntries, new string[0])]
    [TestCase(";", ';', StringSplitOptions.RemoveEmptyEntries, new string[0])]
    [TestCase(";;", ';', StringSplitOptions.RemoveEmptyEntries, new string[0])]
    [TestCase(";;;", ';', StringSplitOptions.RemoveEmptyEntries, new string[0])]
    [TestCase(";;;a", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "a" })]
    [TestCase("a;;;", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "a" })]
    [TestCase(";a;;", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "a" })]
    [TestCase(";;a;", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "a" })]
    [TestCase("a", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "a" })]
    [TestCase("aa", ';', StringSplitOptions.RemoveEmptyEntries, new[] { "aa" })]
    [TestCase("a;b;c", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "a", "b", "c" })]
    [TestCase("a_b_c\r", '_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "a", "b", "c" })]
    [TestCase("\naa;bb;cc", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "aa", "bb", "cc" })]
    [TestCase("aaa;\rbbb ;ccc", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "aaa", "bbb", "ccc" })]
    [TestCase(";\ta", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "a" })]
    [TestCase("a;\n", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "a" })]
    [TestCase(";a; b;c", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "a", "b", "c" })]
    [TestCase("a;b;\nc;", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "a", "b", "c" })]
    [TestCase(";a;b;c;", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "a", "b", "c" })]
    [TestCase(";;a;;b;;c;;", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "a", "b", "c" })]
    [TestCase("\n", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new string[0])]
    [TestCase("\n;\r", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new string[0])]
    [TestCase("\r;\t;\n", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new string[0])]
    [TestCase("\t;\n; ;", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new string[0])]
    [TestCase(";;;a", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "a" })]
    [TestCase("a;;;", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "a" })]
    [TestCase("\n;\na\r\n; ;\t", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "a" })]
    [TestCase(";\r;a\n;", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "a" })]
    [TestCase(" a\t", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "a" })]
    [TestCase("\taa ", ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, new[] { "aa" })]
    public void Ranges(string input, char delimiter, StringSplitOptions options, string[] expected)
    {
        List<string> actual = [];
        foreach ((int beginning, int length) in input.LazySplitRanges(delimiter, options))
        {
            actual.Add(input.Substring(beginning, length));
        }

        actual.Should().Equal(expected);

        // Equivalence with string.Split
        input.Split([delimiter], options).Should().Equal(expected);
    }
}
