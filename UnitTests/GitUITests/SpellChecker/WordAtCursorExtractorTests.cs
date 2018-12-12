using GitUI.SpellChecker;
using NUnit.Framework;

namespace GitUITests.SpellChecker
{
    [TestFixture]
    public class WordAtCursorExtractorTests
    {
        private WordAtCursorExtractor _wordAtCursorExtractor;

        [SetUp]
        public void SetUp()
        {
            _wordAtCursorExtractor = new WordAtCursorExtractor();
        }

        [TestCase(".git", ExpectedResult = ".git")]
        [TestCase("Add .g", ExpectedResult = ".g")]
        [TestCase("Add.g", ExpectedResult = "g")]
        [TestCase("Add,git", ExpectedResult = "git")]
        [TestCase("Add (.git", ExpectedResult = ".git")]
        [TestCase("Introduce Tes", ExpectedResult = "Tes")]
        [TestCase("Add).git", ExpectedResult = "git")] // test strings with a closing round bracket confuse the testcase parser a little
        [TestCase("[Add].git", ExpectedResult = "git")]
        [TestCase("Introduce .babeljs]", ExpectedResult = "")]
        [TestCase("func().otherFun", ExpectedResult = "otherFun")]
        [TestCase("func().other_fun", ExpectedResult = "other_fun")]
        [TestCase("obj._field", ExpectedResult = "_field")]
        [TestCase("func(type init_", ExpectedResult = "init_")]
        public string ExtractsMeaningfulWord(string text)
        {
            return _wordAtCursorExtractor.Extract(text, text.Length - 1);
        }

        [TestCase("", -2, 0, 0, ExpectedResult = true)]
        [TestCase("", -1, 0, 0, ExpectedResult = true)]
        [TestCase("", 0, 0, 0, ExpectedResult = true)]
        [TestCase("", 1, 0, 0, ExpectedResult = true)]
        [TestCase("_obj.f", -2, 0, 0, ExpectedResult = true)]
        [TestCase("_obj.f", -1, 0, 0, ExpectedResult = true)]
        [TestCase("_obj.f", 0, 0, 4, ExpectedResult = true)]
        [TestCase("_obj.f", 1, 0, 4, ExpectedResult = true)]
        [TestCase("_obj.f", 2, 0, 4, ExpectedResult = true)]
        [TestCase("_obj.f", 3, 0, 4, ExpectedResult = true)]
        [TestCase("_obj.f", 4, 4, 1, ExpectedResult = true)]
        [TestCase("_obj.f", 5, 5, 1, ExpectedResult = true)]
        [TestCase("_obj.f", 6, 5, 1, ExpectedResult = true)]
        [TestCase("_obj.f", 7, 5, 1, ExpectedResult = true)]
        [TestCase("0  3", 0, 0, 1, ExpectedResult = true)]
        [TestCase("0  2", 1, 1, 1, ExpectedResult = true)]
        [TestCase("0  2", 2, 2, 1, ExpectedResult = true)]
        [TestCase("0  2", 3, 3, 1, ExpectedResult = true)]
        public bool Test_GetWordBounds(string text, int index, int expectedStart, int expectedLength)
        {
            (int start, int length) = _wordAtCursorExtractor.GetWordBounds(text, index);
            return start == expectedStart && length == expectedLength;
        }

        [TestCase("", -2, ExpectedResult = -1)]
        [TestCase("", -1, ExpectedResult = -1)]
        [TestCase("", 0, ExpectedResult = -1)]
        [TestCase("", 1, ExpectedResult = -1)]
        [TestCase("012", -2, ExpectedResult = -1)]
        [TestCase("012", -1, ExpectedResult = -1)]
        [TestCase("012", 0, ExpectedResult = 0)]
        [TestCase("012", 1, ExpectedResult = 0)]
        [TestCase("012", 2, ExpectedResult = 0)]
        [TestCase("012", 3, ExpectedResult = 0)]
        [TestCase("012", 4, ExpectedResult = 0)]
        [TestCase("01A34", 5, ExpectedResult = 0)]
        [TestCase("01a34", 5, ExpectedResult = 0)]
        [TestCase("01_34", 5, ExpectedResult = 0)]
        [TestCase("01 34", 5, ExpectedResult = 3)]
        [TestCase("01.34", 5, ExpectedResult = 3)]
        [TestCase("01 .4", 5, ExpectedResult = 3)]
        [TestCase("01..4", 5, ExpectedResult = 3)]
        [TestCase("01).4", 5, ExpectedResult = 4)]
        [TestCase("01].4", 5, ExpectedResult = 4)]
        [TestCase("012.4", 5, ExpectedResult = 4)]
        [TestCase("012.4", 4, ExpectedResult = 4)]
        [TestCase("012.4", 3, ExpectedResult = 4)] // The result is right from the initial index!
        [TestCase("012/4", 3, ExpectedResult = 4)] // ditto
        [TestCase("012.4", 2, ExpectedResult = 0)]
        [TestCase("012.4", 1, ExpectedResult = 0)]
        [TestCase("012.4", 0, ExpectedResult = 0)]
        public int Test_FindStartOfWord(string text, int index)
        {
            return _wordAtCursorExtractor.FindStartOfWord(text, index);
        }

        [TestCase("", -2, ExpectedResult = -1)]
        [TestCase("", -1, ExpectedResult = -1)]
        [TestCase("", 0, ExpectedResult = 0)]
        [TestCase("", 1, ExpectedResult = 0)]
        [TestCase("012", -2, ExpectedResult = -1)]
        [TestCase("012", -1, ExpectedResult = -1)]
        [TestCase("012", 0, ExpectedResult = 3)]
        [TestCase("012", 1, ExpectedResult = 3)]
        [TestCase("012", 2, ExpectedResult = 3)]
        [TestCase("012", 3, ExpectedResult = 3)]
        [TestCase("012", 4, ExpectedResult = 3)]
        [TestCase("01A34", 0, ExpectedResult = 5)]
        [TestCase("01a34", 0, ExpectedResult = 5)]
        [TestCase("01_34", 0, ExpectedResult = 5)]
        [TestCase("01 34", 0, ExpectedResult = 2)]
        [TestCase("01/34", 0, ExpectedResult = 2)]
        [TestCase("01/34", 1, ExpectedResult = 2)]
        [TestCase("01/34", 2, ExpectedResult = 2)]
        [TestCase("01/34", 3, ExpectedResult = 5)]
        [TestCase("01/34", 4, ExpectedResult = 5)]
        [TestCase("/12", 0, ExpectedResult = 0)]
        [TestCase("/12", 1, ExpectedResult = 3)]
        [TestCase("01/", 0, ExpectedResult = 2)]
        [TestCase("01/", 1, ExpectedResult = 2)]
        [TestCase("01/", 2, ExpectedResult = 2)]
        [TestCase("01/", 3, ExpectedResult = 3)]
        public int Test_FindEndOfWord(string text, int index)
        {
            return _wordAtCursorExtractor.FindEndOfWord(text, index);
        }
    }
}