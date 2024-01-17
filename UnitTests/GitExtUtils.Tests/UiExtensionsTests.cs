using GitUI;

namespace GitExtUtilsTests
{
    [TestFixture]
    public sealed class UiExtensionsTests
    {
        [TestCase(null, null, null)]
        [TestCase("", "", "")]
        [TestCase("b", "n", "b\r\nNotes:\r\n    n")]
        [TestCase("b\r\nb2", "n\r\nn2", "b\r\nb2\r\nNotes:\r\n    n\r\n    n2")]
        [TestCase("b\r\nb2\r\n", "n\r\nn2", "b\r\nb2\r\n\r\nNotes:\r\n    n\r\n    n2")]
        [TestCase("b", "", "b")]
        [TestCase("b", "\t", "b")]
        [TestCase("b", "  ", "b")]
        [TestCase("b", null, "b")]
        [TestCase(null, "n", "\r\nNotes:\r\n    n")]
        [TestCase("", "n", "\r\nNotes:\r\n    n")]
        public void FormatBodyAndNotes(string body, string notes, string expected)
            => Assert.AreEqual(expected, UIExtensions.FormatBodyAndNotes(body, notes)?.ReplaceLineEndings("\r\n")); // under linux we`d get only \n newline sequences, leading to failing tests
    }
}
