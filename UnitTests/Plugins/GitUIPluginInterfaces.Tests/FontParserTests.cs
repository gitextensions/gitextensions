using System.Drawing;
using FluentAssertions;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitUIPluginInterfacesTests
{
    [TestFixture]
    public class FontParserTests
    {
        private Font _defaultFont;

        [SetUp]
        public void Setup()
        {
            _defaultFont = new Font("Arial", 9);
        }

        [TearDown]
        public void TearDown()
        {
            _defaultFont.Dispose();
        }

        [TestCase(FontStyle.Regular, "Arial;9;_IC_;0;0")]
        [TestCase(FontStyle.Bold, "Arial;9;_IC_;1;0")]
        [TestCase(FontStyle.Italic, "Arial;9;_IC_;0;1")]
        [TestCase(FontStyle.Bold | FontStyle.Italic, "Arial;9;_IC_;1;1")]
        public void AsString_should_persist_font_with_styles(FontStyle fontStyle, string serialised)
        {
            using (var font = new Font("Arial", 9, fontStyle))
            {
                font.AsString().Should().Be(serialised);
            }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void Parse_should_return_default_if_null_or_empty(string serialised)
        {
            var font = serialised.Parse(_defaultFont);

            font.Should().Be(_defaultFont);
        }

        [TestCase("Arial")]
        [TestCase("Arial;")]
        public void Parse_should_return_default_if_less_then_two_parts(string serialised)
        {
            var font = serialised.Parse(_defaultFont);

            font.Should().Be(_defaultFont);
        }

        [TestCase("Courier;8.25;", "Courier", 8.25f, FontStyle.Regular)]
        [TestCase("Courier;12;_IC_", "Courier", 12f, FontStyle.Regular)]
        [TestCase("Courier;11,3;", "Courier", 11.3f, FontStyle.Regular)]
        [TestCase("Courier;11,3;ru", "Courier", 11.3f, FontStyle.Regular)]
        [TestCase("Courier;12;_IC_;0;0", "Courier", 12f, FontStyle.Regular)]
        [TestCase("Courier;12;_IC_;1;0", "Courier", 12f, FontStyle.Bold)]
        [TestCase("Courier;12;_IC_;0;1", "Courier", 12f, FontStyle.Italic)]
        [TestCase("Courier;12;_IC_;1;1", "Courier", 12f, FontStyle.Italic | FontStyle.Bold)]
        public void Parse_should_parse(string serialised, string name, float size, FontStyle style)
        {
            var font = serialised.Parse(_defaultFont);

            font.Should().NotBe(_defaultFont);
            font.OriginalFontName.Should().Be(name);
            font.Size.Should().Be(size);
            font.Style.Should().Be(style);
        }
    }
}