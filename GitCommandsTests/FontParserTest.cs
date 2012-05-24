#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Category = Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute;
#else
using NUnit.Framework;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestContext = System.Object;
using TestProperty = NUnit.Framework.PropertyAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
#endif
using System.Drawing;
using GitCommands;

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