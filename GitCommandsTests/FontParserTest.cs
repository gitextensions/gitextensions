using System.Drawing;
using GitCommands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitCommandsTests
{
    [TestClass]
    public class FontParserTest
    {
        private readonly Font _defaultFont = new Font("Arial", 9);

        [TestMethod]
        public void Can_convert_font_to_string()
        {
            Assert.AreEqual("Courier New;10", new Font("Courier New", 10).AsString());
        }
        
        [TestMethod]
        public void Can_convert_string_to_font()
        {
            Assert.AreEqual(new Font("Courier New", 10), "Courier New;10".Parse(_defaultFont));
        }
        
        [TestMethod]
        public void Returns_default_font_if_parses_null_or_empty_string()
        {
            Assert.AreEqual(_defaultFont, ((string) null).Parse(_defaultFont), "Null string");
            Assert.AreEqual(_defaultFont, string.Empty.Parse(_defaultFont), "Empty string");
        }
        
        [TestMethod]
        public void Returns_default_font_if_parses_wrong_string()
        {
            Assert.AreEqual(_defaultFont, "Courier New;fd".Parse(_defaultFont));
        }
    }
}