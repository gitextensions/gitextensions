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
        [TestCase("Add).git", ExpectedResult = "git")]
        [TestCase("[Add].git", ExpectedResult = "git")]
        [TestCase("Introduce .babeljs]", ExpectedResult = "")]
        [TestCase("func().otherFun", ExpectedResult = "otherFun")]
        public string ExtractsMeaningfulWord(string text)
        {
            return _wordAtCursorExtractor.Extract(text, text.Length - 1);
        }
    }
}