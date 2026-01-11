using FluentAssertions;
using GitExtensions.Extensibility.Extensions;

namespace GitExtUtilsTests.GitUI;

[TestFixture]
public sealed class UiExtensionsTests
{
    [TestCase(null, null, null)]
    [TestCase("", "", "")]
    [TestCase("b", "n", "b\r\n\r\nNotes:\r\n    n")]
    [TestCase("b\r\nb2", "n\r\nn2", "b\r\nb2\r\n\r\nNotes:\r\n    n\r\n    n2")]
    [TestCase("b\r\nb2\r\n", "n\r\nn2", "b\r\nb2\r\n\r\n\r\nNotes:\r\n    n\r\n    n2")]
    [TestCase("b", "", "b")]
    [TestCase("b", "\t", "b\r\n\r\nNotes:\r\n    \t")]
    [TestCase("b", "  ", "b\r\n\r\nNotes:\r\n      ")]
    [TestCase("b", null, "b")]
    [TestCase("", "n", "\r\nNotes:\r\n    n")]
    public void FormatBodyAndNotes(string body, string notes, string expected)
        => UIExtensions.FormatBodyAndNotes(body, notes)?.ReplaceLineEndings("\r\n").Should().Be(expected); // under linux we`d get only \n newline sequences, leading to failing tests
}
